using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;      // 키 입력 처리를 위해 필수
using System.Windows.Threading;  // 타이머 처리를 위해 필수
using DEMO2.Manual.StationTeaching.Points;
using DEMO2.Manual.Setting;

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

        // --- [조그 기능] 변수 선언 ---
        private Window _parentWindow;
        private DispatcherTimer _jogTimer;

        private string _currentAxis = "";
        private int _currentDirection = 0; // +1 or -1

        // 좌표값 저장 변수
        private double _valA = 0;
        private double _valTheta = 0;
        private double _valZ = 0;
        private double _valY = 0;
        private double _valPhi = 0;

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

            // --- [조그 기능] 타이머 설정 ---
            _jogTimer = new DispatcherTimer();
            _jogTimer.Interval = TimeSpan.FromMilliseconds(50); // 0.05초 간격
            _jogTimer.Tick += JogTimer_Tick;

            // --- [조그 기능] 로드/언로드 이벤트 ---
            this.Loaded += StationTeachingView_Loaded;
            this.Unloaded += StationTeachingView_Unloaded;
        }

        // --- [조그 기능] 타이머 틱 (값 업데이트) ---
        private void JogTimer_Tick(object sender, EventArgs e)
        {
            double step = 1.0; // 속도 조절

            switch (_currentAxis)
            {
                case "A":
                    _valA += step * _currentDirection;
                    TxtAxisA.Text = _valA.ToString("F3");
                    break;
                case "Theta":
                    _valTheta += step * _currentDirection;
                    TxtAxisTheta.Text = _valTheta.ToString("F3");
                    break;
                case "Z":
                    _valZ += step * _currentDirection;
                    TxtAxisZ.Text = _valZ.ToString("F3");
                    break;
                case "Y":
                    _valY += step * _currentDirection;
                    TxtAxisY.Text = _valY.ToString("F3");
                    break;
                case "Phi":
                    _valPhi += step * _currentDirection;
                    TxtAxisPhi.Text = _valPhi.ToString("F3");
                    break;
            }
        }

        // --- [조그 기능] 뷰 로드시 키 훅 연결 ---
        private void StationTeachingView_Loaded(object sender, RoutedEventArgs e)
        {
            _parentWindow = Window.GetWindow(this);
            if (_parentWindow != null)
            {
                _parentWindow.PreviewKeyDown += OnParentWindowPreviewKeyDown;
                _parentWindow.PreviewKeyUp += OnParentWindowPreviewKeyUp;
                this.Focus();
            }
        }

        // --- [조그 기능] 뷰 언로드시 키 훅 해제 ---
        private void StationTeachingView_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_parentWindow != null)
            {
                _parentWindow.PreviewKeyDown -= OnParentWindowPreviewKeyDown;
                _parentWindow.PreviewKeyUp -= OnParentWindowPreviewKeyUp;
                _parentWindow = null;
            }
            _jogTimer.Stop(); // 타이머 강제 정지
        }

        // --- [조그 기능] 키 눌림 (매핑) ---
        private void OnParentWindowPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.IsRepeat) return;
            if (_isSlotSetMode) return; // 슬롯 설정 중에는 조그 금지

            switch (e.Key)
            {
                // 1축 물리버튼 -> A축
                case Key.A: StartJog("A", -1); break;
                case Key.B: StartJog("A", 1); break;

                // 2축 물리버튼 -> Theta(θ)축
                case Key.C: StartJog("Theta", -1); break;
                case Key.D: StartJog("Theta", 1); break;

                // 3축 물리버튼 -> Z축
                case Key.E: StartJog("Z", -1); break;
                case Key.F: StartJog("Z", 1); break;

                // 4축 물리버튼 -> Y축
                case Key.G: StartJog("Y", -1); break;
                case Key.H: StartJog("Y", 1); break;

                // 5축 물리버튼 -> Phi(Φ)축
                case Key.I: StartJog("Phi", -1); break;
                case Key.J: StartJog("Phi", 1); break;
            }
        }

        // --- [조그 기능] 키 뗌 (정지) ---
        private void OnParentWindowPreviewKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.A:
                case Key.B:
                    StopJog();
                    break;

                case Key.C:
                case Key.D:
                    StopJog();
                    break;

                case Key.E:
                case Key.F:
                    StopJog();
                    break;

                case Key.G:
                case Key.H:
                    StopJog();
                    break;

                case Key.I:
                case Key.J:
                    StopJog();
                    break;
            }
        }

        private void StartJog(string axis, int direction)
        {
            _currentAxis = axis;
            _currentDirection = direction;

            // 현재 움직이는 축 UI 강조 (선택사항)
            HighlightAxis(axis, true);

            if (!_jogTimer.IsEnabled)
                _jogTimer.Start();
        }

        private void StopJog()
        {
            _jogTimer.Stop();
            HighlightAxis(_currentAxis, false);
            _currentAxis = "";
            _currentDirection = 0;
        }

        // UI 강조 처리 함수
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

        // --- 기존 메서드들 ---

        private void InitializeDropdowns()
        {
            var numbers = Enumerable.Range(1, 10).ToList();
            cmbGroup.ItemsSource = numbers;
            cmbGroup.SelectedIndex = 0;
            cmbCassette.ItemsSource = numbers;
            cmbCassette.SelectedIndex = 0;
        }

        private void On1PointChecked(object sender, RoutedEventArgs e)
        {
            _currentActiveView = _onePointView;
            if (!_isSlotSetMode && PointsContentArea != null)
                PointsContentArea.Content = _currentActiveView;
        }

        private void On3PointChecked(object sender, RoutedEventArgs e)
        {
            _currentActiveView = _threePointView;
            if (!_isSlotSetMode && PointsContentArea != null)
                PointsContentArea.Content = _currentActiveView;
        }

        private void OnManualChecked(object sender, RoutedEventArgs e)
        {
            _currentActiveView = _manualPointView;
            if (!_isSlotSetMode && PointsContentArea != null)
                PointsContentArea.Content = _currentActiveView;
        }

        private void BtnSlotSet_Click(object sender, RoutedEventArgs e)
        {
            _isSlotSetMode = !_isSlotSetMode;

            if (_isSlotSetMode)
            {
                btnSlotSet.Background = _activeButtonColor;
                PointsContentArea.Content = _slotSetView;
                stackArm.IsEnabled = false;
                borderBottomControl.IsEnabled = false;
                gridRightInfo.IsEnabled = false;
            }
            else
            {
                btnSlotSet.Background = _defaultButtonColor;
                PointsContentArea.Content = _currentActiveView;
                stackArm.IsEnabled = true;
                borderBottomControl.IsEnabled = true;
                gridRightInfo.IsEnabled = true;
            }
        }

        private void BtnTest_Click(object sender, RoutedEventArgs e)
        {
            ManualView parentView = FindParent<ManualView>(this);
            if (parentView != null)
            {
                parentView.OpenTestView();
            }
            else
            {
                MessageBox.Show("ManualView를 찾을 수 없습니다.");
            }
        }

        private void BtnSetting_Click(object sender, RoutedEventArgs e)
        {
            ManualView parentView = FindParent<ManualView>(this);
            if (parentView != null)
            {
                parentView.OpenSettingView();
            }
            else
            {
                MessageBox.Show("ManualView를 찾을 수 없습니다.");
            }
        }

        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null) return null;

            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindParent<T>(parentObject);
        }
    }
}