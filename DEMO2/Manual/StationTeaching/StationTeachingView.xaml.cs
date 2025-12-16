using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DEMO2.Manual.StationTeaching.Points;

namespace DEMO2.Manual.StationTeaching
{
    public partial class StationTeachingView : UserControl
    {
        private OnePointView _onePointView;
        private ThreePointView _threePointView;
        private ManualPointView _manualPointView;
        private SlotSetView _slotSetView;

        private UserControl _currentActiveView; // 현재 보고 있던 화면 저장용

        private bool _isSlotSetMode = false;
        private readonly Brush _defaultButtonColor = new SolidColorBrush(Color.FromRgb(173, 216, 230));
        private readonly Brush _activeButtonColor = new SolidColorBrush(Color.FromRgb(50, 205, 50));

        public StationTeachingView()
        {
            InitializeComponent();
            InitializeDropdowns();

            // 뷰 인스턴스 생성
            _onePointView = new OnePointView();
            _threePointView = new ThreePointView();
            _manualPointView = new ManualPointView();
            _slotSetView = new SlotSetView();

            // 초기 화면: 1 Point
            _currentActiveView = _onePointView;
            PointsContentArea.Content = _currentActiveView;
        }

        private void InitializeDropdowns()
        {
            var numbers = Enumerable.Range(1, 10).ToList();
            cmbGroup.ItemsSource = numbers;
            cmbGroup.SelectedIndex = 0;
            cmbCassette.ItemsSource = numbers;
            cmbCassette.SelectedIndex = 0;
        }

        // 1 Point 선택
        private void On1PointChecked(object sender, RoutedEventArgs e)
        {
            _currentActiveView = _onePointView;
            if (!_isSlotSetMode && PointsContentArea != null)
                PointsContentArea.Content = _currentActiveView;
        }

        // 3 Point 선택
        private void On3PointChecked(object sender, RoutedEventArgs e)
        {
            _currentActiveView = _threePointView;
            if (!_isSlotSetMode && PointsContentArea != null)
                PointsContentArea.Content = _currentActiveView;
        }

        // Manual 선택
        private void OnManualChecked(object sender, RoutedEventArgs e)
        {
            _currentActiveView = _manualPointView;
            if (!_isSlotSetMode && PointsContentArea != null)
                PointsContentArea.Content = _currentActiveView;
        }

        // Slot Set 버튼 클릭 (토글)
        private void BtnSlotSet_Click(object sender, RoutedEventArgs e)
        {
            _isSlotSetMode = !_isSlotSetMode;

            if (_isSlotSetMode)
            {
                // [Slot Set 모드]
                btnSlotSet.Background = _activeButtonColor;

                // 중앙 화면 교체
                PointsContentArea.Content = _slotSetView;

                // 다른 영역 비활성화 (회색 처리됨)
                stackArm.IsEnabled = false;
                borderBottomControl.IsEnabled = false;
                gridRightInfo.IsEnabled = false;
            }
            else
            {
                // [복귀]
                btnSlotSet.Background = _defaultButtonColor;

                // 원래 화면으로 복구
                PointsContentArea.Content = _currentActiveView;

                // 다른 영역 활성화
                stackArm.IsEnabled = true;
                borderBottomControl.IsEnabled = true;
                gridRightInfo.IsEnabled = true;
            }
        }

        // TEST 버튼 클릭 이벤트 핸들러
        private void BtnTest_Click(object sender, RoutedEventArgs e)
        {
            // 부모 컨트롤인 ManualView를 찾습니다.
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

        // 부모 찾기 Helper 메서드 (이미 있다면 중복 추가하지 마세요)
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