using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Windows.Input; // WPF Key Enum 사용

namespace DEMO2.Drivers
{
    // 키 이벤트 데이터를 담을 클래스
    public class KeypadEventArgs : EventArgs
    {
        public Key Key { get; set; }        // 매핑된 WPF 키 (예: Key.A)
        public bool IsDown { get; set; }    // true=눌림, false=뗌
    }

    public class DTP7HDriver
    {
        private SerialPort _serialPort;

        // 외부(UI)에서 구독할 이벤트: "키 눌림 발생!"
        public event EventHandler<KeypadEventArgs> KeypadEvent;

        // 수신 버퍼 (데이터가 끊겨서 들어올 때를 대비)
        private List<byte> _receiveBuffer = new List<byte>();

        // --- [매뉴얼 기반] DTP7H 키 코드 매핑 테이블 ---
        // 매뉴얼 Page 19 ~ 20 참조
        private readonly Dictionary<byte, Key> _keyMap = new Dictionary<byte, Key>
        {
            { 0x1E, Key.A }, // KEY_A -> 1축 (-)
            { 0x30, Key.B }, // KEY_B -> 1축 (+)
            { 0x2E, Key.C }, // KEY_C -> 2축 (-)
            { 0x20, Key.D }, // KEY_D -> 2축 (+)
            { 0x12, Key.E }, // KEY_E -> 3축 (-)
            { 0x21, Key.F }, // KEY_F -> 3축 (+)
            { 0x22, Key.G }, // KEY_G -> 4축 (-)
            { 0x23, Key.H }, // KEY_H -> 4축 (+)
            { 0x17, Key.I }, // KEY_I -> 5축 (-)
            { 0x24, Key.J }, // KEY_J -> 5축 (+)
            { 0x25, Key.K }, // KEY_K -> 6축 (-)
            { 0x26, Key.L }  // KEY_L -> 6축 (+)
        };

        public bool IsConnected => _serialPort != null && _serialPort.IsOpen;

        public DTP7HDriver()
        {
            _serialPort = new SerialPort();
        }

        // 연결 함수
        public bool Connect(string portName)
        {
            try
            {
                if (_serialPort.IsOpen) _serialPort.Close();

                _serialPort.PortName = portName;
                _serialPort.BaudRate = 115200; // DTP7H 고정값
                _serialPort.DataBits = 8;
                _serialPort.StopBits = StopBits.One;
                _serialPort.Parity = Parity.None;

                _serialPort.DataReceived += SerialPort_DataReceived;
                _serialPort.Open();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection Error: {ex.Message}");
                return false;
            }
        }

        // 해제 함수
        public void Disconnect()
        {
            try
            {
                if (_serialPort.IsOpen)
                {
                    _serialPort.DataReceived -= SerialPort_DataReceived;
                    _serialPort.Close();
                }
            }
            catch { }
        }

        // 데이터 수신 핸들러
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                int bytesToRead = _serialPort.BytesToRead;
                byte[] buffer = new byte[bytesToRead];
                _serialPort.Read(buffer, 0, bytesToRead);

                // 버퍼에 추가
                _receiveBuffer.AddRange(buffer);

                // 패킷 처리 (최소 9바이트 이상 모였을 때)
                ProcessBuffer();
            }
            catch { }
        }

        // 패킷 파싱 로직 (매뉴얼 프로토콜 준수)
        private void ProcessBuffer()
        {
            // 패킷 구조: STX(1) | MOD(1) | SEL(1) | DATA1(1) | DATA2(1) | ... | ETX(1) = 총 9바이트
            while (_receiveBuffer.Count >= 9)
            {
                // 1. STX (0x02) 찾기
                int stxIndex = _receiveBuffer.IndexOf(0x02);

                if (stxIndex == -1)
                {
                    _receiveBuffer.Clear(); // STX 없으면 다 버림
                    return;
                }

                if (stxIndex > 0)
                {
                    _receiveBuffer.RemoveRange(0, stxIndex); // STX 앞부분 쓰레기 데이터 삭제
                }

                // 다시 길이 확인
                if (_receiveBuffer.Count < 9) return;

                // 2. ETX (0x03) 확인 (인덱스 8)
                if (_receiveBuffer[8] != 0x03)
                {
                    // 깨진 패킷일 수 있으므로 STX 하나 지우고 다음 탐색
                    _receiveBuffer.RemoveAt(0);
                    continue;
                }

                // 3. 유효한 패킷 추출
                byte[] packet = _receiveBuffer.GetRange(0, 9).ToArray();
                _receiveBuffer.RemoveRange(0, 9); // 처리한 데이터 삭제

                // 4. 데이터 해석 및 이벤트 발생
                ParsePacket(packet);
            }
        }

        private void ParsePacket(byte[] packet)
        {
            // DATA1 (Index 3): 상태 (0x31=Down, 0x30=Up)
            bool isDown = (packet[3] == 0x31);

            // DATA2 (Index 4): 키 코드
            byte keyCode = packet[4];

            if (_keyMap.ContainsKey(keyCode))
            {
                Key mappedKey = _keyMap[keyCode];

                // 이벤트 발생 (구독자에게 알림)
                KeypadEvent?.Invoke(this, new KeypadEventArgs
                {
                    Key = mappedKey,
                    IsDown = isDown
                });
            }
        }
    }
}