using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using DEMO2.Manual.StationTeaching.Points;
using DEMO2.Manual.Setting;
using DEMO2.Drivers;
using DEMO2.Core;

namespace DEMO2.Manual.StationTeaching
{
    public partial class StationTeachingView : UserControl
    {
        private OnePointView _onePointView;
        private ThreePointView _threePointView;
        private ManualPointView _manualPointView;
        private SlotSetView _slotSetView;
        private UserControl _currentActiveView;

        private bool _isSlotSetMode = false;
        private readonly Brush _defaultButtonColor = new SolidColorBrush(Color.FromRgb(173, 216, 230));
        private readonly Brush _activeButtonColor = new SolidColorBrush(Color.FromRgb(50, 205, 50));

        private DTP7HDriver _driverRef;
        private string _lastActiveJogAxis = "";

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

            DeviceManager.Instance.Motion.PositionChanged += OnPositionChanged;

            this.Loaded += StationTeachingView_Loaded;
            this.Unloaded += StationTeachingView_Unloaded;
        }

        private void StationTeachingView_Loaded(object sender, RoutedEventArgs e)
        {
            // 중앙 매니저에서 드라이버 실체를 가져옵니다.
            _driverRef = DeviceManager.Instance.TP;

            if (_driverRef != null)
            {
                _driverRef.KeypadEvent += OnDriverKeypadEvent;
            }
        }

        private void StationTeachingView_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_driverRef != null)
            {
                _driverRef.KeypadEvent -= OnDriverKeypadEvent;
                _driverRef = null;
            }
            DeviceManager.Instance.Motion.PositionChanged -= OnPositionChanged;
        }

        private void OnPositionChanged(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                var motion = DeviceManager.Instance.Motion;
                TxtAxisA.Text = motion.AxisA.ToString("F3");
                TxtAxisTheta.Text = motion.AxisTheta.ToString("F3");
                TxtAxisZ.Text = motion.AxisZ.ToString("F3");
                TxtAxisY.Text = motion.AxisY.ToString("F3");
                TxtAxisPhi.Text = motion.AxisPhi.ToString("F3");
            });
        }

        private void OnDriverKeypadEvent(object sender, KeypadEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (_isSlotSetMode) return;

                var motion = DeviceManager.Instance.Motion;

                if (e.IsDown)
                {
                    switch (e.Key)
                    {
                        case Key.A: motion.StartJog("A", -1); HighlightAxis("A", true); break;
                        case Key.B: motion.StartJog("A", 1); HighlightAxis("A", true); break;
                        case Key.C: motion.StartJog("Theta", -1); HighlightAxis("Theta", true); break;
                        case Key.D: motion.StartJog("Theta", 1); HighlightAxis("Theta", true); break;
                        case Key.E: motion.StartJog("Z", -1); HighlightAxis("Z", true); break;
                        case Key.F: motion.StartJog("Z", 1); HighlightAxis("Z", true); break;
                        case Key.G: motion.StartJog("Y", -1); HighlightAxis("Y", true); break;
                        case Key.H: motion.StartJog("Y", 1); HighlightAxis("Y", true); break;
                        case Key.I: motion.StartJog("Phi", -1); HighlightAxis("Phi", true); break;
                        case Key.J: motion.StartJog("Phi", 1); HighlightAxis("Phi", true); break;
                    }
                }
                else
                {
                    motion.StopJog();
                    ResetAllHighlights();
                }
            });
        }

        private void HighlightAxis(string axis, bool isActive)
        {
            _lastActiveJogAxis = axis;
            Brush bg = isActive ? Brushes.SkyBlue : _defaultButtonColor;
            UpdateAxisBackground(axis, bg);
        }

        private void ResetAllHighlights()
        {
            UpdateAxisBackground("A", _defaultButtonColor);
            UpdateAxisBackground("Theta", _defaultButtonColor);
            UpdateAxisBackground("Z", _defaultButtonColor);
            UpdateAxisBackground("Y", _defaultButtonColor);
            UpdateAxisBackground("Phi", _defaultButtonColor);
        }

        private void UpdateAxisBackground(string axis, Brush bg)
        {
            switch (axis)
            {
                case "A": TxtAxisA.Background = bg; break;
                case "Theta": TxtAxisTheta.Background = bg; break;
                case "Z": TxtAxisZ.Background = bg; break;
                case "Y": TxtAxisY.Background = bg; break;
                case "Phi": TxtAxisPhi.Background = bg; break;
            }
        }

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