using System;
using System.Windows.Input;

namespace DEMO2.Driver
{
    // 버튼 이벤트 데이터를 담는 클래스 (기존에 있다면 그대로 사용)
    public class KeypadEventArgs : EventArgs
    {
        public Key Key { get; set; }
        public bool IsDown { get; set; }
    }

    public interface ITeachPendant
    {
        // 이벤트 정의: 이 인터페이스만 보고 버튼 신호를 기다릴 수 있음.
        event EventHandler<KeypadEventArgs> KeypadEvent;

        bool Connect(string portName, int baudRate);
        void Disconnect(); // 연결 해제도 인터페이스에 있는 것이 좋음니다.
        void SetLed(byte ledId, byte color);
        void SetBuzzer(bool isOn);
        bool IsConnected { get; }
    }
}