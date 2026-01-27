using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Threading;
using DEMO2.Manual.StationTeaching.Points;
using DEMO2.Driver; // 인터페이스 네임스페이스

namespace DEMO2.Manual.StationTeaching
{
    public partial class StationTeachingView : UserControl
    {
        // 1. 구체 클래스(DTP7HDriver) 대신 인터페이스에 의존
        private ITeachPendant _driver;

        // 2. 부모 뷰(ManualView)를 직접 찾지 않기 위한 이벤트 선언
        public event Action<string> ViewChangeRequested;

        // 뷰 인스턴스 및 상태 변수
        private OnePointView _onePointView;
        private ThreePointView _threePointView;
        private ManualPointView _manualPointView;
        private SlotSetView _slotSetView;
        private UserControl _currentActiveView;

        private bool _isSlotSetMode = false;
        private readonly Brush _defaultButtonColor = new SolidColorBrush(Color.FromRgb(173, 216, 230));
        private readonly Brush _activeButtonColor = new SolidColorBrush(Color.FromRgb(50, 205, 50));

        private DispatcherTimer _jogTimer;
        private string _currentAxis = "";
        private int _currentDirection = 0;

        private double _valA = 0, _valTheta = 0, _valZ = 0, _valY = 0, _valPhi = 0;

        public StationTeachingView()
        {
            InitializeComponent();
            InitializeDropdowns();

            _onePointView = new OnePointView();
            _threePointView = new ThreePointView();
            _manualPointView = new ManualPointView();
            _slotSetView = new SlotSetView();

            _currentActiveView = _onePointView;
            PointsContentArea.Content = _currentActiveView;

            _jogTimer = new DispatcherTimer();
            _jogTimer.Interval = TimeSpan.FromMilliseconds(50);
            _jogTimer.Tick += JogTimer_Tick;

            this.Unloaded += StationTeachingView_Unloaded;
        }

        // 3. 의존성 주입(DI) 메서드: 외부에서 드라이버를 넣어줌
        public void SetDriver(ITeachPendant driver)
        {
            // 기존 연결 해제
            if (_driver != null)
                _driver.KeypadEvent -= OnDriverKeypadEvent;

            _driver = driver;

            if (_driver != null)
                _driver.KeypadEvent += OnDriverKeypadEvent;
        }

        private void StationTeachingView_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_driver != null)
            {
                _driver.KeypadEvent -= OnDriverKeypadEvent;
            }
            _jogTimer.Stop();
        }

        private void OnDriverKeypadEvent(object sender, KeypadEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (_isSlotSetMode) return;

                if (e.IsDown)
                {
                    switch (e.Key)
                    {
                        case Key.A: StartJog("A", -1); break;
                        case Key.B: StartJog("A", 1); break;
                        case Key.C: StartJog("Theta", -1); break;
                        case Key.D: StartJog("Theta", 1); break;
                        case Key.E: StartJog("Z", -1); break;
                        case Key.F: StartJog("Z", 1); break;
                        case Key.G: StartJog("Y", -1); break;
                        case Key.H: StartJog("Y", 1); break;
                        case Key.I: StartJog("Phi", -1); break;
                        case Key.J: StartJog("Phi", 1); break;
                    }
                }
                else
                {
                    StopJog();
                }
            });
        }

        private void StartJog(string axis, int direction)
        {
            _currentAxis = axis;
            _currentDirection = direction;
            HighlightAxis(axis, true);
            if (!_jogTimer.IsEnabled) _jogTimer.Start();
        }

        private void StopJog()
        {
            _jogTimer.Stop();
            if (!string.IsNullOrEmpty(_currentAxis))
                HighlightAxis(_currentAxis, false);
            _currentAxis = "";
            _currentDirection = 0;
        }

        private void JogTimer_Tick(object sender, EventArgs e)
        {
            double step = 1.0;
            switch (_currentAxis)
            {
                case "A": _valA += step * _currentDirection; TxtAxisA.Text = _valA.ToString("F3"); break;
                case "Theta": _valTheta += step * _currentDirection; TxtAxisTheta.Text = _valTheta.ToString("F3"); break;
                case "Z": _valZ += step * _currentDirection; TxtAxisZ.Text = _valZ.ToString("F3"); break;
                case "Y": _valY += step * _currentDirection; TxtAxisY.Text = _valY.ToString("F3"); break;
                case "Phi": _valPhi += step * _currentDirection; TxtAxisPhi.Text = _valPhi.ToString("F3"); break;
            }
        }

        private void HighlightAxis(string axis, bool isActive)
        {
            Brush bg = isActive ? Brushes.SkyBlue : new SolidColorBrush(Color.FromRgb(173, 216, 230));
            switch (axis)
            {
                case "A": TxtAxisA.Background = bg; break;
                case "Theta": TxtAxisTheta.Background = bg; break;
                case "Z": TxtAxisZ.Background = bg; break;
                case "Y": TxtAxisY.Background = bg; break;
                case "Phi": TxtAxisPhi.Background = bg; break;
            }
        }

        // 4. FindParent 대신 이벤트를 통해 상위 뷰에 화면 전환 요청
        private void BtnTest_Click(object sender, RoutedEventArgs e)
        {
            ViewChangeRequested?.Invoke("Test");
        }

        private void BtnSetting_Click(object sender, RoutedEventArgs e)
        {
            ViewChangeRequested?.Invoke("Setting");
        }

        // --- 기타 UI 로직 (기존 유지) ---
        private void InitializeDropdowns()
        {
            var numbers = Enumerable.Range(1, 10).ToList();
            cmbGroup.ItemsSource = numbers; cmbGroup.SelectedIndex = 0;
            cmbCassette.ItemsSource = numbers; cmbCassette.SelectedIndex = 0;
        }

        private void On1PointChecked(object sender, RoutedEventArgs e) { ChangeView(_onePointView); }
        private void On3PointChecked(object sender, RoutedEventArgs e) { ChangeView(_threePointView); }
        private void OnManualChecked(object sender, RoutedEventArgs e) { ChangeView(_manualPointView); }

        private void ChangeView(UserControl view)
        {
            _currentActiveView = view;
            if (!_isSlotSetMode && PointsContentArea != null) PointsContentArea.Content = _currentActiveView;
        }

        private void BtnSlotSet_Click(object sender, RoutedEventArgs e)
        {
            _isSlotSetMode = !_isSlotSetMode;
            if (_isSlotSetMode)
            {
                btnSlotSet.Background = _activeButtonColor;
                PointsContentArea.Content = _slotSetView;
                stackArm.IsEnabled = false; borderBottomControl.IsEnabled = false; gridRightInfo.IsEnabled = false;
            }
            else
            {
                btnSlotSet.Background = _defaultButtonColor;
                PointsContentArea.Content = _currentActiveView;
                stackArm.IsEnabled = true; borderBottomControl.IsEnabled = true; gridRightInfo.IsEnabled = true;
            }
        }
    }
}