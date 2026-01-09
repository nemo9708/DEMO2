using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DEMO2.Manual.StationTeaching;
using DEMO2.Manual.StationTeaching.TEST;
using DEMO2.Manual.Setting;

namespace DEMO2.Manual
{
    public partial class ManualView : UserControl
    {
        // 색상 정의
        private readonly Brush ActiveColor = new SolidColorBrush(Color.FromRgb(173, 255, 47)); // 활성 (연두)
        private readonly Brush InactiveColor = Brushes.LightGray; // 비활성 (회색)

        private ManualMenuView _menuView;
        private StationTeachingView _stationView;
        private SettingView _settingView;
        private TestView _testView;

        public ManualView()
        {
            InitializeComponent();

            _menuView = new ManualMenuView();
            _menuView.StationTeachingClicked += OnStationTeachingClicked;
            _menuView.SettingClicked += OnSettingClicked;
            _menuView.Test2Clicked += OnTest2Clicked;

            // 초기화
            _stationView = new StationTeachingView();
            _settingView = new SettingView();
            _testView = new TestView();

            SwitchTab("Manual");
        }

        // 메뉴에서 Station Teaching 버튼 클릭 시
        private void OnStationTeachingClicked(object sender, System.EventArgs e)
        {
            tabStation.Visibility = Visibility.Visible;
            SwitchTab("Station");
        }

        private void OnSettingClicked(object sender, System.EventArgs e)
        {
            OpenSettingView();
        }

        private void OnTest2Clicked(object sender, System.EventArgs e)
        {
            OpenTest2View();
        }

        private void TabSettingClicked(object sender, System.EventArgs e)
        {
            tabSetting.Visibility = Visibility.Visible;
            SwitchTab("Setting");
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

        private void TabSetting_Click(object sender, RoutedEventArgs e)
        {
            SwitchTab("Setting");
        }

        // X 버튼 클릭 (탭 닫기)
        private void BtnCloseTab_Click(object sender, RoutedEventArgs e)
        {
            // 부모 버튼(tabStation)의 클릭 이벤트가 발생하지 않도록 방지
            e.Handled = true;

            // 1. 탭 숨기기
            tabStation.Visibility = Visibility.Collapsed;

            // 2. Manual 화면으로 복귀
            SwitchTab("Manual");
        }

        private void BtnCloseSettingTab_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            tabSetting.Visibility = Visibility.Collapsed;
            SwitchTab("Manual");
        }

        private void TabTest_Click(object sender, RoutedEventArgs e)
        {
            SwitchTab("Test");
        }

        // 물리 키 맵핑 테스트용
        private void TabTest2_Click(object sender, RoutedEventArgs e)
        {
            SwitchTab("Test2");
        }

        // TEST 탭 닫기 버튼 클릭
        private void BtnCloseTestTab_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true; // 부모 버튼 클릭 방지
            tabTest.Visibility = Visibility.Collapsed;
            SwitchTab("Manual");
        }

        // 물리 키 맵핑 테스트용
        private void BtnCloseTest2Tab_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            tabTest2.Visibility = Visibility.Collapsed;
            SwitchTab("Manual");
        }

        // 외부에서 호출할 메서드
        public void OpenTestView()
        {
            tabTest.Visibility = Visibility.Visible;
            SwitchTab("Test");
        }

        // 물리 키 맵핑 테스트용
        public void OpenTest2View()
        {
            tabTest2.Visibility = Visibility.Visible;
            SwitchTab("Test2");
        }

        public void OpenSettingView()
        {
            tabSetting.Visibility = Visibility.Visible;
            SwitchTab("Setting");
        }

        private void SwitchTab(string tabName)
        {
            // 스타일 초기화 (모두 비활성 상태로 설정 후 선택된 것만 활성화)
            tabManual.Background = InactiveColor;
            tabManual.FontWeight = FontWeights.Normal;
            tabManual.BorderBrush = Brushes.Gray;

            tabStation.Background = InactiveColor;
            tabStation.FontWeight = FontWeights.Normal;
            tabStation.BorderBrush = Brushes.Gray;

            tabSetting.Background = InactiveColor;
            tabSetting.FontWeight = FontWeights.Normal;
            tabSetting.BorderBrush = Brushes.Gray;

            // Test 탭 초기화
            tabTest.Background = InactiveColor;
            tabTest.FontWeight = FontWeights.Normal;
            tabTest.BorderBrush = Brushes.Gray;

            if (tabName == "Manual")
            {
                ManualContentArea.Content = _menuView;
                tabManual.Background = ActiveColor;
                tabManual.FontWeight = FontWeights.Bold;
                tabManual.BorderBrush = Brushes.Black;
            }
            else if (tabName == "Station")
            {
                ManualContentArea.Content = _stationView;
                tabStation.Background = ActiveColor;
                tabStation.FontWeight = FontWeights.Bold;
                tabStation.BorderBrush = Brushes.Black;
            }
            else if (tabName == "Setting")
            {
                ManualContentArea.Content = _settingView;
                tabSetting.Background = ActiveColor;
                tabSetting.FontWeight = FontWeights.Bold;
                tabSetting.BorderBrush = Brushes.Black;
            }

            // Test 탭 선택 시 로직
            else if (tabName == "Test")
            {
                ManualContentArea.Content = _testView;
                tabTest.Background = ActiveColor;
                tabTest.FontWeight = FontWeights.Bold;
                tabTest.BorderBrush = Brushes.Black;
            }
        }
    }
}