using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading; // 타이머 사용을 위해 필요

namespace DEMO2.Manual
{
    public partial class Test2View : UserControl
    {
        private Window _parentWindow;

        // 조그 동작을 위한 타이머
        private DispatcherTimer _jogTimer;

        // 현재 움직이고 있는 축과 방향 저장
        private string _currentAxis = "";
        private int _currentDirection = 0; // +1: 증가, -1: 감소, 0: 정지

        // 각 축의 좌표값 저장 변수
        private double _valX = 0;
        private double _valY = 0;
        private double _valZ = 0;
        private double _valA = 0;
        private double _valB = 0;
        private double _valC = 0;

        public Test2View()
        {
            InitializeComponent();

            // 타이머 설정 (0.05초마다 실행)
            _jogTimer = new DispatcherTimer();
            _jogTimer.Interval = TimeSpan.FromMilliseconds(50);
            _jogTimer.Tick += JogTimer_Tick;

            this.Loaded += Test2View_Loaded;
            this.Unloaded += Test2View_Unloaded;
        }

        // 타이머가 작동할 때마다 실행되는 함수 (숫자 업데이트)
        private void JogTimer_Tick(object sender, EventArgs e)
        {
            // 이동 속도 (한 번 틱마다 변하는 양)
            double step = 1.5;

            // 현재 선택된 축의 값을 변경
            switch (_currentAxis)
            {
                case "X":
                    _valX += step * _currentDirection;
                    TxtX1.Text = _valX.ToString("F2"); // 소수점 2자리 표시
                    break;
                case "Y":
                    _valY += step * _currentDirection;
                    TxtY2.Text = _valY.ToString("F2");
                    break;
                case "Z":
                    _valZ += step * _currentDirection;
                    TxtZ3.Text = _valZ.ToString("F2");
                    break;
                case "A":
                    _valA += step * _currentDirection;
                    TxtA4.Text = _valA.ToString("F2");
                    break;
                case "B":
                    _valB += step * _currentDirection;
                    TxtB5.Text = _valB.ToString("F2");
                    break;
                case "C":
                    _valC += step * _currentDirection;
                    TxtC6.Text = _valC.ToString("F2");
                    break;
            }
        }

        private void Test2View_Loaded(object sender, RoutedEventArgs e)
        {
            _parentWindow = Window.GetWindow(this);
            if (_parentWindow != null)
            {
                _parentWindow.PreviewKeyDown += OnParentWindowPreviewKeyDown;
                _parentWindow.PreviewKeyUp += OnParentWindowPreviewKeyUp;
                this.Focus();
            }
        }

        private void Test2View_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_parentWindow != null)
            {
                _parentWindow.PreviewKeyDown -= OnParentWindowPreviewKeyDown;
                _parentWindow.PreviewKeyUp -= OnParentWindowPreviewKeyUp;
                _parentWindow = null;
            }
            // 뷰가 사라질 때 타이머도 정지
            _jogTimer.Stop();
        }

        // --- [키 눌림] 타이머 시작 (숫자 변화 시작) ---
        private void OnParentWindowPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.IsRepeat) return;

            switch (e.Key)
            {
                // 1축 (X) : A(-), B(+)
                case Key.A: StartJogUI(BtnX1, "X", -1); break;
                case Key.B: StartJogUI(BtnX1, "X", 1); break;

                // 2축 (Y) : C(-), D(+)
                case Key.C: StartJogUI(BtnY2, "Y", -1); break;
                case Key.D: StartJogUI(BtnY2, "Y", 1); break;

                // 3축 (Z) : E(-), F(+)
                case Key.E: StartJogUI(BtnZ3, "Z", -1); break;
                case Key.F: StartJogUI(BtnZ3, "Z", 1); break;

                // 4축 (A) : G(-), H(+)
                case Key.G: StartJogUI(BtnA4, "A", -1); break;
                case Key.H: StartJogUI(BtnA4, "A", 1); break;

                // 5축 (B) : I(-), J(+)
                case Key.I: StartJogUI(BtnB5, "B", -1); break;
                case Key.J: StartJogUI(BtnB5, "B", 1); break;

                // 6축 (C) : K(-), L(+)
                case Key.K: StartJogUI(BtnC6, "C", -1); break;
                case Key.L: StartJogUI(BtnC6, "C", 1); break;
            }
        }

        // --- [키 뗌] 타이머 정지 (숫자 멈춤) ---
        private void OnParentWindowPreviewKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.A: case Key.B: StopJogUI(BtnX1); break;
                case Key.C: case Key.D: StopJogUI(BtnY2); break;
                case Key.E: case Key.F: StopJogUI(BtnZ3); break;
                case Key.G: case Key.H: StopJogUI(BtnA4); break;
                case Key.I: case Key.J: StopJogUI(BtnB5); break;
                case Key.K: case Key.L: StopJogUI(BtnC6); break;
            }
        }

        // 조그 시작 (타이머 ON)
        private void StartJogUI(Button btn, string axis, int direction)
        {
            if (btn != null) btn.Background = Brushes.SkyBlue;

            _currentAxis = axis;
            _currentDirection = direction;

            // 타이머가 꺼져있다면 시작
            if (!_jogTimer.IsEnabled)
            {
                _jogTimer.Start();
            }
        }

        // 조그 정지 (타이머 OFF)
        private void StopJogUI(Button btn)
        {
            if (btn != null) btn.ClearValue(Button.BackgroundProperty);

            // 타이머 정지
            _jogTimer.Stop();
            _currentDirection = 0;
            _currentAxis = "";
        }
    }
}