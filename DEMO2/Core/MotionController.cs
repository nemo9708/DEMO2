using System;
using System.Windows.Threading;

namespace DEMO2.Core
{
    public class MotionController
    {
        private DispatcherTimer _jogTimer;
        private string _currentAxis = "";
        private int _currentDirection = 0;

        // [데이터] 모든 축의 좌표를 중앙 관리합니다.
        public double AxisA { get; private set; }
        public double AxisTheta { get; private set; }
        public double AxisZ { get; private set; }
        public double AxisY { get; private set; }
        public double AxisPhi { get; private set; }

        // [이벤트] 좌표가 변하면 알림을 보냅니다.
        public event EventHandler PositionChanged;

        public MotionController()
        {
            _jogTimer = new DispatcherTimer();
            _jogTimer.Interval = TimeSpan.FromMilliseconds(50);
            _jogTimer.Tick += JogTimer_Tick;
        }

        private void JogTimer_Tick(object sender, EventArgs e)
        {
            double step = 1.0;
            switch (_currentAxis)
            {
                case "A": AxisA += step * _currentDirection; break;
                case "Theta": AxisTheta += step * _currentDirection; break;
                case "Z": AxisZ += step * _currentDirection; break;
                case "Y": AxisY += step * _currentDirection; break;
                case "Phi": AxisPhi += step * _currentDirection; break;
            }
            // 좌표가 바뀌었음을 구독자(UI 등)에게 알림
            PositionChanged?.Invoke(this, EventArgs.Empty);
        }

        public void StartJog(string axis, int direction)
        {
            _currentAxis = axis;
            _currentDirection = direction;
            if (!_jogTimer.IsEnabled) _jogTimer.Start();
        }

        public void StopJog()
        {
            _jogTimer.Stop();
            _currentAxis = "";
            _currentDirection = 0;
        }
    }
}