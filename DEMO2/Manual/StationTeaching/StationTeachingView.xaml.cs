using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DEMO2.Manual.StationTeaching.Points; // Points 폴더 참조

namespace DEMO2.Manual.StationTeaching
{
    public partial class StationTeachingView : UserControl
    {
        private OnePointView _onePointView;
        private ThreePointView _threePointView;
        private ManualPointView _manualPointView;

        public StationTeachingView()
        {
            InitializeComponent();
            InitializeDropdowns();

            // 뷰 인스턴스 생성
            _onePointView = new OnePointView();
            _threePointView = new ThreePointView();
            _manualPointView = new ManualPointView();

            // 초기 화면
            PointsContentArea.Content = _onePointView;
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
            if (PointsContentArea != null)
                PointsContentArea.Content = _onePointView;
        }

        // 3 Point 선택
        private void On3PointChecked(object sender, RoutedEventArgs e)
        {
            if (PointsContentArea != null)
                PointsContentArea.Content = _threePointView;
        }

        // Manual 선택
        private void OnManualChecked(object sender, RoutedEventArgs e)
        {
            if (PointsContentArea != null)
                PointsContentArea.Content = _manualPointView;
        }
    }
}