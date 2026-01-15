using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Windows.Input;
using System.Threading; // Sleep 사용을 위해 추가

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

        // [수정 1] 시리얼 통신 규격에 맞는 주소로 변경 (피드백 반영)
        // Left LED: 순차적 (0x41 ~ 0x43)
        public const byte LED_LEFT_1 = 0x41;
        public const byte LED_LEFT_2 = 0x42;
        public const byte LED_LEFT_3 = 0x43;

        // Right LED: 역순 배치 (0x63 ~ 0x61) - 피드백 내용 반영
        public const byte LED_RIGHT_1 = 0x63;
        public const byte LED_RIGHT_2 = 0x62;
        public const byte LED_RIGHT_3 = 0x61;

        // LED 색상 정의
        public const byte LED_COLOR_OFF = 0x30;
        public const byte LED_COLOR_BLUE = 0x31;
        public const byte LED_COLOR_RED = 0x32;
        public const byte LED_COLOR_ALL = 0x33;

        // 키 매핑 (매뉴얼 19페이지 참조)
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

        // 시리얼 연결
        public bool Connect(string portName, int baudRate)
        {
            try
            {
                if (_serialPort.IsOpen) _serialPort.Close();

                _serialPort.PortName = portName;
                _serialPort.BaudRate = 115200; // 피드백: 115200 고정 필수
                _serialPort.DataBits = 8;
                _serialPort.StopBits = StopBits.One;
                _serialPort.Parity = Parity.None;

                // 장비가 PC의 존재를 인식하도록 신호 활성화 (기존 유지)
                _serialPort.DtrEnable = true;
                _serialPort.RtsEnable = true;

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
            // 부저 On/Off 값 (0x31/0x30)
            byte state = isOn ? (byte)0x31 : (byte)0x30;
            // SEL: 0x3B (Buzzer)
            SendPacket(0x11, 0x3B, state, 0x20, 0x20);
        }

        // LED 제어
        public void SetLed(byte ledId, byte color)
        {
            // SEL: 0x3A (LED)
            SendPacket(0x11, 0x3A, ledId, color, 0x20);
        }

        // 패킷 전송
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

            // CRC 계산: STX(idx 0)부터 DATA3(idx 5)까지 총 6바이트
            ushort crc = CalculateCrc(packet, 6);

            packet[6] = (byte)(crc >> 8);   // CRC_H (Big Endian)
            packet[7] = (byte)(crc & 0xFF); // CRC_L
            packet[8] = 0x03; // ETX

            try
            {
                _serialPort.Write(packet, 0, packet.Length);

                // [수정 2] 피드백 반영: 전송 후 필수 지연 시간 추가 (5ms 이상 권장)
                Thread.Sleep(10);
            }
            catch { }
        }

        // CRC16 Modbus Lookup Table
        private static readonly ushort[] CrcTable = {
            0X0000, 0XC0C1, 0XC181, 0X0140, 0XC301, 0X03C0, 0X0280, 0XC241,
            0XC601, 0X06C0, 0X0780, 0XC741, 0X0500, 0XC5C1, 0XC481, 0X0440,
            0XCC01, 0X0CC0, 0X0D80, 0XCD41, 0X0F00, 0XCFC1, 0XCE81, 0X0E40,
            0XA001, 0X60C0, 0X6181, 0XA140, 0X6301, 0XA3C0, 0XA280, 0X6241,
            0X6601, 0XA6C0, 0XA780, 0X6741, 0XA500, 0X65C1, 0X6481, 0XA440,
            0X6C01, 0XACC0, 0XAD80, 0X6D41, 0XAF00, 0X6FC1, 0X6E81, 0XAE40,
            0X0A01, 0XCAC0, 0XCB81, 0X0B40, 0XC901, 0X09C0, 0X0880, 0XC841,
            0XD801, 0X18C0, 0X1981, 0XD940, 0X1B01, 0XDBC0, 0XDA80, 0X1A41,
            0X1E01, 0XDEC0, 0XDF81, 0X1F40, 0XDD01, 0X1DC0, 0X1C80, 0XDC41,
            0X1401, 0XD4C0, 0XD581, 0X1540, 0XD701, 0X17C0, 0X1680, 0XD641,
            0XD201, 0X12C0, 0X1381, 0XD340, 0X1101, 0XD1C0, 0XD080, 0X1041,
            0XF001, 0X30C0, 0X3181, 0XF140, 0X3301, 0XF3C0, 0XF280, 0X3241,
            0X3601, 0XF6C0, 0XF780, 0X3741, 0XF500, 0X35C1, 0X3481, 0XF440,
            0X3C01, 0XFCC0, 0XFD80, 0X3D41, 0XFF00, 0X3FC1, 0X3E81, 0XFE40,
            0XFA01, 0X3AC0, 0X3B81, 0XFA40, 0X3901, 0XF9C0, 0XF880, 0X3841,
            0X2801, 0XE8C0, 0XE981, 0X2940, 0XEB01, 0X2BC0, 0X2A80, 0XEA41,
            0XEE01, 0X2EC0, 0X2F81, 0XEF40, 0X2D01, 0XEDC0, 0XEC80, 0X2C41,
            0XE401, 0X24C0, 0X2581, 0XE540, 0X2701, 0XE7C0, 0XE680, 0X2641,
            0X2201, 0XE2C0, 0XE381, 0X2340, 0XE101, 0X21C0, 0X2080, 0XE041,
            0XA001, 0X60C0, 0X6181, 0XA140, 0X6301, 0XA3C0, 0XA280, 0X6241,
            0X6601, 0XA6C0, 0XA780, 0X6741, 0XA500, 0X65C1, 0X6481, 0XA440,
            0X6C01, 0XACC0, 0XAD80, 0X6D41, 0XAF00, 0X6FC1, 0X6E81, 0XAE40,
            0XAA01, 0X6AC0, 0X6B81, 0XAA40, 0X6901, 0XA9C0, 0XA880, 0X6841,
            0X7801, 0XB8C0, 0XB981, 0X7940, 0XBB01, 0X7BC0, 0X7A80, 0XBA41,
            0XBE01, 0X7EC0, 0X7F81, 0XBF40, 0X7D01, 0XBDC0, 0XBC80, 0X7C41,
            0XB401, 0X74C0, 0X7581, 0XB540, 0X7701, 0XB7C0, 0XB680, 0X7641,
            0X7201, 0XB2C0, 0XB381, 0X7340, 0XB101, 0X71C0, 0X7080, 0XB041,
            0X5001, 0X90C0, 0X9181, 0X5140, 0X9301, 0X53C0, 0X5280, 0X9241,
            0X9601, 0X56C0, 0X5781, 0X9740, 0X5501, 0X95C0, 0X9481, 0X5440,
            0X9C01, 0X5CC0, 0X5D81, 0X9D40, 0X5F01, 0X9FC0, 0X9E81, 0X5E40,
            0X5A01, 0X9AC0, 0X9B81, 0X5B40, 0X9901, 0X59C0, 0X5880, 0X9841,
            0X8801, 0X48C0, 0X4981, 0X8940, 0X4B01, 0X8BC0, 0X8A80, 0X4A41,
            0X4E01, 0X8EC0, 0X8F81, 0X4F40, 0X8D01, 0X4DC0, 0X4C80, 0X8C41,
            0X4401, 0X84C0, 0X8581, 0X4540, 0X8701, 0X47C0, 0X4680, 0X8641,
            0X8201, 0X42C0, 0X4381, 0X8340, 0X4101, 0X81C0, 0X8080, 0X4041
        };

        private ushort CalculateCrc(byte[] data, int length)
        {
            ushort crc = 0xFFFF; // Modbus 초기값

            for (int i = 0; i < length; i++)
            {
                byte index = (byte)(crc ^ data[i]);
                crc = (ushort)((crc >> 8) ^ CrcTable[index]);
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