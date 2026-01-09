using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
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

        public bool IsConnected => _serialPort != null && _serialPort.IsOpen;

        private readonly Dictionary<byte, Key> _keyMap = new Dictionary<byte, Key>
        {
            { 0x1E, Key.A }, { 0x30, Key.B }, { 0x2E, Key.C }, { 0x20, Key.D },
            { 0x12, Key.E }, { 0x21, Key.F }, { 0x22, Key.G }, { 0x23, Key.H },
            { 0x17, Key.I }, { 0x24, Key.J }, { 0x25, Key.K }, { 0x26, Key.L }
        };

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
                _serialPort.BaudRate = baudRate;
                _serialPort.DataBits = 8;
                _serialPort.StopBits = StopBits.One;
                _serialPort.Parity = Parity.None;
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
            try { if (_serialPort.IsOpen) { _serialPort.DataReceived -= SerialPort_DataReceived; _serialPort.Close(); } } catch { }
        }

        // --- [추가됨] 부저 제어 ---
        // isOn: true(켜기), false(끄기)
        public void SetBuzzer(bool isOn)
        {
            // STX(02) MOD(11) SEL(3B) DATA1(31=On, 30=Off) DATA2(20) DATA3(20) CRC_H CRC_L ETX(03)
            byte state = isOn ? (byte)0x31 : (byte)0x30;
            SendPacket(0x11, 0x3B, state, 0x20, 0x20);
        }

        // --- [추가됨] LED 제어 ---
        // ledId: 0x41(L1), 0x42(L2), 0x43(L3), 0x61(R1), 0x62(R2), 0x63(R3)
        // color: 0x30(Off), 0x31(Blue), 0x32(Red), 0x33(Purple/All)
        public void SetLed(byte ledId, byte color)
        {
            SendPacket(0x11, 0x3A, ledId, color, 0x20);
        }

        // --- [추가됨] 패킷 전송 및 CRC 계산 ---
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

            // CRC16 (Modbus) 계산 (앞 6바이트 대상)
            ushort crc = CalculateCrc(packet, 6);
            packet[6] = (byte)(crc >> 8); // CRC_H (or Lo depending on protocol, trying Std Big Endian here)
            packet[7] = (byte)(crc & 0xFF); // CRC_L

            // DTP7H manual implies Little Endian for some fields but usually Big Endian for CRC in packets. 
            // If it doesn't work, swap packet[6] and packet[7].

            packet[8] = 0x03; // ETX

            try { _serialPort.Write(packet, 0, packet.Length); } catch { }
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
            ushort crc = 0xFFFF;
            for (int i = 0; i < length; i++)
            {
                byte index = (byte)(crc ^ data[i]);
                crc = (ushort)((crc >> 8) ^ CrcTable[index]);
            }
            return crc;
        }

        // (기존 수신 로직은 동일)
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e) { /* ... 기존 코드 유지 ... */ }
        private void ProcessBuffer() { /* ... 기존 코드 유지 ... */ }
        private void ParsePacket(byte[] packet) { /* ... 기존 코드 유지 ... */ }
    }
}