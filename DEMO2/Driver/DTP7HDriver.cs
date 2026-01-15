using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading; // [필수] Sleep 기능을 위해 추가
using System.Windows.Input;

namespace DEMO2.Drivers
{
    public class KeypadEventArgs : EventArgs
    {
        public Key Key { get; set; }
        public bool IsDown { get; set; }
    }

    public class DTP7HDriver
    {
        private SerialPort _serialPort;
        public event EventHandler<KeypadEventArgs> KeypadEvent;
        private List<byte> _receiveBuffer = new List<byte>();

        // ---------------------------------------------------------
        // [수정 1] 피드백 기반 주소 상수 수정 (시리얼 프로토콜 규격 준수)
        // ---------------------------------------------------------
        // Left LED: 순차적 (L1->0x41, L2->0x42, L3->0x43)
        public const byte LED_LEFT_1 = 0x41;
        public const byte LED_LEFT_2 = 0x42;
        public const byte LED_LEFT_3 = 0x43;

        // Right LED: 역순 배치 (R1->0x63, R2->0x62, R3->0x61)
        public const byte LED_RIGHT_1 = 0x63;
        public const byte LED_RIGHT_2 = 0x62;
        public const byte LED_RIGHT_3 = 0x61;

        // LED 색상 정의
        public const byte LED_COLOR_OFF = 0x30;
        public const byte LED_COLOR_BLUE = 0x31;
        public const byte LED_COLOR_RED = 0x32;
        public const byte LED_COLOR_ALL = 0x33;

        // 키 매핑
        private readonly Dictionary<byte, Key> _keyMap = new Dictionary<byte, Key>
        {
            { 0x1E, Key.A }, { 0x30, Key.B },
            { 0x2E, Key.C }, { 0x20, Key.D },
            { 0x12, Key.E }, { 0x21, Key.F },
            { 0x22, Key.G }, { 0x23, Key.H },
            { 0x17, Key.I }, { 0x24, Key.J },
            { 0x25, Key.K }, { 0x26, Key.L }
        };

        public bool IsConnected => _serialPort != null && _serialPort.IsOpen;

        public DTP7HDriver()
        {
            _serialPort = new SerialPort();
        }

        public bool Connect(string portName, int baudRate)
        {
            try
            {
                if (_serialPort.IsOpen) _serialPort.Close();

                _serialPort.PortName = portName;
                _serialPort.BaudRate = 115200;
                _serialPort.DataBits = 8;
                _serialPort.StopBits = StopBits.One;
                _serialPort.Parity = Parity.None;

                // [수정] 흐름 제어 신호 끄기 (표준 설정)
                _serialPort.DtrEnable = false;
                _serialPort.RtsEnable = false;

                _serialPort.DataReceived += SerialPort_DataReceived;
                _serialPort.Open();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }

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

        // 부저 제어
        public void SetBuzzer(bool isOn)
        {
            byte state = isOn ? (byte)0x31 : (byte)0x30;
            // SEL: 0x3B (Buzzer), POS: 0x64 (피드백 및 예전 코드 참조)
            // 만약 0x64가 안되면 0x20으로 변경해볼 수 있으나, Buzzer는 주소보다 명령어가 중요함
            SendPacket(0x11, 0x3B, 0x64, state, 0x20);
        }

        // LED 제어
        public void SetLed(byte ledId, byte color)
        {
            // SEL: 0x3A (LED)
            SendPacket(0x11, 0x3A, ledId, color, 0x20);
        }

        private void SendPacket(byte mod, byte sel, byte d1, byte d2, byte d3)
        {
            if (!IsConnected) return;

            byte[] packet = new byte[9];
            packet[0] = 0x02; // STX
            packet[1] = mod;
            packet[2] = sel;
            packet[3] = d1;
            packet[4] = d2;
            packet[5] = d3;

            ushort crc = CalculateCrc(packet, 6);

            // [핵심 수정] 표준 Modbus는 Low Byte가 먼저 나갑니다.
            // 기존: High -> Low (Big Endian)
            // 수정: Low -> High (Little Endian)
            packet[6] = (byte)(crc & 0xFF);      // CRC Low Byte
            packet[7] = (byte)((crc >> 8) & 0xFF); // CRC High Byte

            packet[8] = 0x03; // ETX

            try
            {
                _serialPort.Write(packet, 0, packet.Length);
                Thread.Sleep(10); // 딜레이 유지
            }
            catch { }
        }

        // [CRC 알고리즘 수정] 테이블 방식 대신 표준 Bitwise 방식 사용
        // 이 방식은 crc16_append와 논리적으로 동일합니다.
        private ushort CalculateCrc(byte[] data, int length)
        {
            ushort crc = 0xFFFF; // 초기값 0xFFFF

            for (int i = 0; i < length; i++)
            {
                crc ^= data[i]; // XOR
                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 0x0001) != 0) // LSB check
                    {
                        crc = (ushort)((crc >> 1) ^ 0xA001); // Polynomial 0xA001
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
            }
            return crc;
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                int bytesToRead = _serialPort.BytesToRead;
                byte[] buffer = new byte[bytesToRead];
                _serialPort.Read(buffer, 0, bytesToRead);
                _receiveBuffer.AddRange(buffer);
                ProcessBuffer();
            }
            catch { }
        }

        private void ProcessBuffer()
        {
            while (_receiveBuffer.Count >= 9)
            {
                int stxIndex = _receiveBuffer.IndexOf(0x02);
                if (stxIndex == -1) { _receiveBuffer.Clear(); return; }
                if (stxIndex > 0) _receiveBuffer.RemoveRange(0, stxIndex);
                if (_receiveBuffer.Count < 9) return;

                if (_receiveBuffer[8] != 0x03) { _receiveBuffer.RemoveAt(0); continue; }

                byte[] packet = _receiveBuffer.GetRange(0, 9).ToArray();
                _receiveBuffer.RemoveRange(0, 9);
                ParsePacket(packet);
            }
        }

        private void ParsePacket(byte[] packet)
        {
            bool isDown = (packet[3] == 0x31);
            byte keyCode = packet[4];

            if (_keyMap.ContainsKey(keyCode))
            {
                KeypadEvent?.Invoke(this, new KeypadEventArgs
                {
                    Key = _keyMap[keyCode],
                    IsDown = isDown
                });
            }
        }
    }
}