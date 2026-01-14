using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Threading;
using DEMO2.Manual.StationTeaching.Points;
using DEMO2.Manual.Setting;
using DEMO2.Drivers; // [중요] 드라이버 네임스페이스 추가

namespace DEMO2.Manual.StationTeaching
{
    public partial class StationTeachingView : UserControl
    {
        // 뷰 인스턴스
        private OnePointView _onePointView;
        private ThreePointView _threePointView;
        private ManualPointView _manualPointView;
        private SlotSetView _slotSetView;
        private UserControl _currentActiveView;

        private bool _isSlotSetMode = false;
        private readonly Brush _defaultButtonColor = new SolidColorBrush(Color.FromRgb(173, 216, 230));
        private readonly Brush _activeButtonColor = new SolidColorBrush(Color.FromRgb(50, 205, 50));

        // 조그 기능 변수
        private DispatcherTimer _jogTimer;
        private string _currentAxis = "";
        private int _currentDirection = 0;

        // 좌표값
        private double _valA = 0;
        private double _valTheta = 0;
        private double _valZ = 0;
        private double _valY = 0;
        private double _valPhi = 0;

        // 드라이버 참조 저장용
        private DTP7HDriver _driverRef;

        public StationTeachingView()
        {
            InitializeComponent();
            InitializeDropdowns();

            // 뷰 생성
            _onePointView = new OnePointView();
            _threePointView = new ThreePointView();
            _manualPointView = new ManualPointView();
            _slotSetView = new SlotSetView();

            // 초기 화면
            _currentActiveView = _onePointView;
            PointsContentArea.Content = _currentActiveView;

            // 조그 타이머 설정
            _jogTimer = new DispatcherTimer();
            _jogTimer.Interval = TimeSpan.FromMilliseconds(50);
            _jogTimer.Tick += JogTimer_Tick;

            this.Loaded += StationTeachingView_Loaded;
            this.Unloaded += StationTeachingView_Unloaded;
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

        private void StationTeachingView_Loaded(object sender, RoutedEventArgs e)
        {
            // [핵심 수정] MainWindow를 찾아서 드라이버 이벤트를 연결합니다.
            var mainWindow = Window.GetWindow(this) as MainWindow;

            if (mainWindow != null && mainWindow.MyDriverControl != null)
            {
                // 드라이버 참조 저장 (해제할 때 쓰기 위해)
                _driverRef = mainWindow.MyDriverControl.Driver;

                // 이벤트 구독 (이제 드라이버가 소리치면 여기서 듣습니다!)
                _driverRef.KeypadEvent += OnDriverKeypadEvent;
            }
        }

        private void StationTeachingView_Unloaded(object sender, RoutedEventArgs e)
        {
            // 메모리 누수 방지를 위해 이벤트 연결 해제
            if (_driverRef != null)
            {
                _driverRef.KeypadEvent -= OnDriverKeypadEvent;
                _driverRef = null;
            }
            _jogTimer.Stop();
        }

        // [핵심 수정] 드라이버 전용 이벤트 핸들러 (KeyDown/KeyUp 통합됨)
        private void OnDriverKeypadEvent(object sender, KeypadEventArgs e)
        {
            // UI 스레드에서 실행되도록 보장
            this.Dispatcher.Invoke(() =>
            {
                if (_isSlotSetMode) return;

                if (e.IsDown)
                {
                    // --- [키 눌림] ---
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
                    // --- [키 뗌] ---
                    // 어떤 키를 떼든 해당 축 정지 명령을 보냄
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
            HighlightAxis(_currentAxis, false);
            _currentAxis = "";
            _currentDirection = 0;
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

        // --- 기존 기능 코드들 (변경 없음) ---
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

        private void BtnTest_Click(object sender, RoutedEventArgs e)
        {
            var parent = FindParent<ManualView>(this);
            if (parent != null) parent.OpenTestView();
        }
        private void BtnSetting_Click(object sender, RoutedEventArgs e)
        {
            var parent = FindParent<ManualView>(this);
            if (parent != null) parent.OpenSettingView();
        }
        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            T parent = parentObject as T;
            return parent ?? FindParent<T>(parentObject);
        }
    }
}