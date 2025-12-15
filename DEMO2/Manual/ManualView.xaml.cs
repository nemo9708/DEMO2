using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DEMO2.Manual.StationTeaching; // 경로 필수

namespace DEMO2.Manual
{
    public partial class ManualView : UserControl
    {
        // 색상 정의
        private readonly Brush ActiveColor = new SolidColorBrush(Color.FromRgb(173, 255, 47)); // 활성 (연두)
        private readonly Brush InactiveColor = Brushes.LightGray; // 비활성 (회색)

        private ManualMenuView _menuView;
        private StationTeachingView _stationView;

        public ManualView()
        {
            InitializeComponent();

            _menuView = new ManualMenuView();
            _menuView.StationTeachingClicked += OnStationTeachingClicked;

            _stationView = new StationTeachingView();

            SwitchTab("Manual");
        }

        // 메뉴에서 Station Teaching 버튼 클릭 시
        private void OnStationTeachingClicked(object sender, System.EventArgs e)
        {
            tabStation.Visibility = Visibility.Visible;
            SwitchTab("Station");
        }

        // 탭 헤더 클릭
        private void TabManual_Click(object sender, RoutedEventArgs e)
        {
            SwitchTab("Manual");
        }

        private void TabStation_Click(object sender, RoutedEventArgs e)
        {
            SwitchTab("Station");
        }

        // [추가됨] X 버튼 클릭 (탭 닫기)
        private void BtnCloseTab_Click(object sender, RoutedEventArgs e)
        {
            // 부모 버튼(tabStation)의 클릭 이벤트가 발생하지 않도록 방지
            e.Handled = true;

            // 1. 탭 숨기기
            tabStation.Visibility = Visibility.Collapsed;

            // 2. Manual 화면으로 복귀
            SwitchTab("Manual");
        }

        private void SwitchTab(string tabName)
        {
            if (tabName == "Manual")
            {
                ManualContentArea.Content = _menuView;

                // 스타일 업데이트
                tabManual.Background = ActiveColor;
                tabStation.Background = InactiveColor;
                tabManual.FontWeight = FontWeights.Bold;
                tabStation.FontWeight = FontWeights.Normal;

                // 테두리 강조 (선택된 탭 느낌)
                tabManual.BorderBrush = Brushes.Black;
                tabStation.BorderBrush = Brushes.Gray;
            }
            else if (tabName == "Station")
            {
                ManualContentArea.Content = _stationView;

                // 스타일 업데이트
                tabManual.Background = InactiveColor;
                tabStation.Background = ActiveColor;
                tabManual.FontWeight = FontWeights.Normal;
                tabStation.FontWeight = FontWeights.Bold;

                // 테두리 강조
                tabManual.BorderBrush = Brushes.Gray;
                tabStation.BorderBrush = Brushes.Black;
            }
        }
    }
}