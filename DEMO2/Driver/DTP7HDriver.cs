using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;
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

        // 프로토콜 기본 상수 (매뉴얼 16P, 18P, 19P 공통 규격)
        private const byte STX = 0x02;
        private const byte ETX = 0x03;
        private const byte MOD_SET = 0x11;
        private const byte MOD_GET = 0x10;

        // 장치 식별자 (SEL)
        private const byte SEL_LED = 0x3A;
        private const byte SEL_BUZZER = 0x3B;
        private const byte SEL_KEYPAD = 0x3D;

        // DTP7H 모델 전용 LED 위치 ID (DATA1) - 매뉴얼 11P, 16P 기준
        public const byte LED_LEFT_1 = 0x41;
        public const byte LED_LEFT_2 = 0x42;
        public const byte LED_LEFT_3 = 0x43;
        public const byte LED_RIGHT_1 = 0x63; // 매뉴얼 16P 표 기준
        public const byte LED_RIGHT_2 = 0x62;
        public const byte LED_RIGHT_3 = 0x61;

        // LED 색상 상태 (DATA2) - 매뉴얼 11P 기준
        public const byte LED_COLOR_OFF = 0x30;
        public const byte LED_COLOR_BLUE = 0x31;
        public const byte LED_COLOR_RED = 0x32;
        public const byte LED_COLOR_ALL = 0x33;

        // 키패드 매핑 (매뉴얼 19P~20P 스캔코드 및 예제 기준)
        private readonly Dictionary<byte, Key> _keyMap = new Dictionary<byte, Key>
        {
            { 0x1E, Key.A }, { 0x30, Key.B }, { 0x2E, Key.C }, { 0x20, Key.D },
            { 0x12, Key.E }, { 0x21, Key.F }, { 0x22, Key.G }, { 0x23, Key.H },
            { 0x17, Key.I }, { 0x24, Key.J }, { 0x25, Key.K }, { 0x26, Key.L },
            // 물리 축 버튼 및 기능키 추가
            { 0x48, Key.Up }, { 0x50, Key.Down }, { 0x4B, Key.Left }, { 0x4D, Key.Right },
            { 0x3B, Key.F1 }, { 0x3C, Key.F2 }, { 0x44, Key.F10 }, { 0x57, Key.F11 }
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
                // 매뉴얼 10P: Baudrate는 115200만 지원함
                _serialPort.BaudRate = 115200;
                _serialPort.DataBits = 8;
                _serialPort.StopBits = StopBits.One;
                _serialPort.Parity = Parity.None;
                _serialPort.Handshake = Handshake.None;
                _serialPort.ReadTimeout = 500;
                _serialPort.WriteTimeout = 500;

                // 통신 활성화를 위한 신호 설정
                _serialPort.DtrEnable = true;
                _serialPort.RtsEnable = true;

                _serialPort.DataReceived += SerialPort_DataReceived;
                _serialPort.Open();

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Connect Error: " + ex.Message);
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
            // 매뉴얼 18P: MOD(0x11), SEL(0x3B), DATA1(상태), DATA2(0x20), DATA3(0x20)
            SendPacket(MOD_SET, SEL_BUZZER, state, 0x20, 0x20);
        }

        public void SetLed(byte ledId, byte color)
        {
            // 매뉴얼 16P: MOD(0x11), SEL(0x3A), DATA1(위치), DATA2(색상), DATA3(0x20)
            SendPacket(MOD_SET, SEL_LED, ledId, color, 0x20);
        }

        private void SendPacket(byte mod, byte sel, byte d1, byte d2, byte d3)
        {
            if (!IsConnected) return;

            byte[] packet = new byte[9];
            packet[0] = STX;
            packet[1] = mod;
            packet[2] = sel;
            packet[3] = d1;
            packet[4] = d2;
            packet[5] = d3;

            ushort crc = CalculateCrc(packet, 6);
            // 매뉴얼 14P: CRC 바이트 순서는 High Byte가 앞임 (Big Endian)
            packet[6] = (byte)(crc >> 8);
            packet[7] = (byte)(crc & 0xFF);
            packet[8] = ETX;

            try
            {
                _serialPort.Write(packet, 0, packet.Length);
                // 매뉴얼 8P 지침: 전송 후 최소 1ms 딜레이 보장
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
                int stxIndex = _receiveBuffer.IndexOf(STX);
                if (stxIndex == -1) { _receiveBuffer.Clear(); return; }
                if (stxIndex > 0) _receiveBuffer.RemoveRange(0, stxIndex);
                if (_receiveBuffer.Count < 9) return;

                if (_receiveBuffer[8] != ETX)
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
            // 매뉴얼 19P: 수신 패킷은 MOD_GET(0x10) 및 SEL_KEYPAD(0x3D)여야 함
            if (packet[1] != MOD_GET || packet[2] != SEL_KEYPAD) return;

            // DATA1(packet[3]): Key Status (0x31: Down, 0x30: Up)
            bool isDown = (packet[3] == 0x31);
            // DATA2(packet[4]): Key Scan Code
            byte keyCode = packet[4];

            if (_keyMap.ContainsKey(keyCode))
            {
                // C# 6.0의 이벤트 호출 방식
                KeypadEvent?.Invoke(this, new KeypadEventArgs
                {
                    Key = _keyMap[keyCode],
                    IsDown = isDown
                });
            }
        }
    }
}