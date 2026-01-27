using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Windows.Input;

namespace DEMO2.Drivers
{
    // 버튼 이벤트 데이터를 전달하기 위한 클래스
    public class KeypadEventArgs : EventArgs
    {
        public Key Key { get; set; }
        public bool IsDown { get; set; }
    }

    // 인터페이스 정의: 드라이버와 매니저 사이의 결합도를 낮추는 핵심 약속
    public interface ITeachPendant
    {
        event EventHandler<KeypadEventArgs> KeypadEvent;
        bool IsConnected { get; }
        bool Connect(string portName, int baudRate);
        void Disconnect();
        void SetLed(byte ledId, byte color);
        void SetBuzzer(bool isOn);
    }

    // 인터페이스를 상속받아 구현한 실제 드라이버 클래스
    public class DTP7HDriver : ITeachPendant
    {
        private SerialPort _serialPort;
        public event EventHandler<KeypadEventArgs> KeypadEvent;
        private List<byte> _receiveBuffer = new List<byte>();

        // [LED 주소 상수] 
        public const byte LED_LEFT_1 = 0x41;
        public const byte LED_LEFT_2 = 0x42;
        public const byte LED_LEFT_3 = 0x43;
        public const byte LED_RIGHT_1 = 0x63;
        public const byte LED_RIGHT_2 = 0x62;
        public const byte LED_RIGHT_3 = 0x61;

        // [LED 색상 상수]
        public const byte LED_COLOR_OFF = 0x30;
        public const byte LED_COLOR_BLUE = 0x31;
        public const byte LED_COLOR_RED = 0x32;
        public const byte LED_COLOR_ALL = 0x33;

        // [키 매핑] 매뉴얼 스캔코드 기준
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
                _serialPort.BaudRate = 115200; // 장치 규격 고정
                _serialPort.DataBits = 8;
                _serialPort.StopBits = StopBits.One;
                _serialPort.Parity = Parity.None;

                // 통신 신호 활성화
                _serialPort.DtrEnable = true;
                _serialPort.RtsEnable = true;

                _serialPort.DataReceived += SerialPort_DataReceived;
                _serialPort.Open();

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Connect Error: {ex.Message}");
                return false;
            }
        }

        public void Disconnect()
        {
            try
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    _serialPort.DataReceived -= SerialPort_DataReceived;
                    _serialPort.Close();
                }
            }
            catch { }
        }

        public void SetBuzzer(bool isOn)
        {
            byte state = isOn ? (byte)0x31 : (byte)0x30;
            // 부저 제어 패킷 구조 적용 (MOD: 0x11, SEL: 0x3B)
            SendPacket(0x11, 0x3B, state, 0x20, 0x20);
        }

        public void SetLed(byte ledId, byte color)
        {
            // LED 제어 패킷 구조 적용 (SEL: 0x3A)
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

            // CRC 바이트 순서: Big Endian (High Byte First)
            packet[6] = (byte)(crc >> 8);
            packet[7] = (byte)(crc & 0xFF);

            packet[8] = 0x03; // ETX

            try
            {
                _serialPort.Write(packet, 0, packet.Length);
                // 장치 처리 응답 시간 보류
                Thread.Sleep(10);
            }
            catch { }
        }

        private ushort CalculateCrc(byte[] data, int length)
        {
            ushort crc = 0xFFFF;
            for (int i = 0; i < length; i++)
            {
                crc ^= data[i];
                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 0x0001) != 0)
                        crc = (ushort)((crc >> 1) ^ 0xA001);
                    else
                        crc >>= 1;
                }
            }
            return crc;
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (_serialPort == null || !_serialPort.IsOpen) return;

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

                if (_receiveBuffer[8] != 0x03)
                {
                    _receiveBuffer.RemoveAt(0);
                    continue;
                }

                byte[] packet = _receiveBuffer.GetRange(0, 9).ToArray();
                _receiveBuffer.RemoveRange(0, 9);
                ParsePacket(packet);
            }
        }

        private void ParsePacket(byte[] packet)
        {
            // 수신 데이터 상태 체크 (0x31: Down, 0x30: Up)
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