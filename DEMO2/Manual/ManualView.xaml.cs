using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DEMO2.Manual.StationTeaching;
using DEMO2.Manual.StationTeaching.TEST;
using DEMO2.Manual.Setting;
using DEMO2.Driver; // 인터페이스 네임스페이스 추가

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

        // 부모로부터 받은 드라이버 참조 보관
        private ITeachPendant _driver;

        // 1. 생성자에서 ITeachPendant를 주입받도록 수정
        public ManualView(ITeachPendant driver)
        {
            InitializeComponent();
            _driver = driver;

            // 뷰 초기화
            _menuView = new ManualMenuView();
            _stationView = new StationTeachingView();
            _settingView = new SettingView();
            _testView = new TestView();

            // 2. StationTeachingView에 드라이버 주입 및 이벤트 연결
            _stationView.SetDriver(_driver);
            _stationView.ViewChangeRequested += OnViewChangeRequested;

            // 메뉴 이벤트 연결
            _menuView.StationTeachingClicked += OnStationTeachingClicked;
            _menuView.SettingClicked += OnSettingClicked;
            _menuView.Test2Clicked += OnTest2Clicked;

            SwitchTab("Manual");
        }

        // 3. 자식 뷰(StationTeachingView)의 화면 전환 요청 처리
        private void OnViewChangeRequested(string targetView)
        {
            if (targetView == "Test")
            {
                OpenTestView();
            }
            else if (targetView == "Setting")
            {
                OpenSettingView();
            }
        }

        // 메뉴에서 Station Teaching 버튼 클릭 시
        private void OnStationTeachingClicked(object sender, EventArgs e)
        {
            tabStation.Visibility = Visibility.Visible;
            SwitchTab("Station");
        }

        private void OnSettingClicked(object sender, EventArgs e)
        {
            OpenSettingView();
        }

        private void OnTest2Clicked(object sender, EventArgs e)
        {
            OpenTest2View();
        }

        // 탭 헤더 클릭 이벤트들
        private void TabManual_Click(object sender, RoutedEventArgs e) => SwitchTab("Manual");
        private void TabStation_Click(object sender, RoutedEventArgs e) => SwitchTab("Station");
        private void TabSetting_Click(object sender, RoutedEventArgs e) => SwitchTab("Setting");
        private void TabTest_Click(object sender, RoutedEventArgs e) => SwitchTab("Test");

        // X 버튼 클릭 (탭 닫기)
        private void BtnCloseTab_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            tabStation.Visibility = Visibility.Collapsed;
            SwitchTab("Manual");
        }

        private void BtnCloseSettingTab_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            tabSetting.Visibility = Visibility.Collapsed;
            SwitchTab("Manual");
        }

        private void BtnCloseTestTab_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            tabTest.Visibility = Visibility.Collapsed;
            SwitchTab("Manual");
        }

        // 외부/자식 요청으로 호출되는 화면 오픈 메서드들
        public void OpenTestView()
        {
            tabTest.Visibility = Visibility.Visible;
            SwitchTab("Test");
        }

        public void OpenSettingView()
        {
            tabSetting.Visibility = Visibility.Visible;
            SwitchTab("Setting");
        }

        public void OpenTest2View()
        {
            tabTest2.Visibility = Visibility.Visible;
            SwitchTab("Test2");
        }

        // 탭 전환 및 스타일 적용 로직
        private void SwitchTab(string tabName)
        {
            // 스타일 초기화
            tabManual.Background = tabStation.Background = tabSetting.Background = tabTest.Background = InactiveColor;
            tabManual.FontWeight = tabStation.FontWeight = tabSetting.FontWeight = tabTest.FontWeight = FontWeights.Normal;

            if (tabName == "Manual")
            {
                ManualContentArea.Content = _menuView;
                tabManual.Background = ActiveColor;
                tabManual.FontWeight = FontWeights.Bold;
            }
            else if (tabName == "Station")
            {
                ManualContentArea.Content = _stationView;
                tabStation.Background = ActiveColor;
                tabStation.FontWeight = FontWeights.Bold;
            }
            else if (tabName == "Setting")
            {
                ManualContentArea.Content = _settingView;
                tabSetting.Background = ActiveColor;
                tabSetting.FontWeight = FontWeights.Bold;
            }
            else if (tabName == "Test")
            {
                ManualContentArea.Content = _testView;
                tabTest.Background = ActiveColor;
                tabTest.FontWeight = FontWeights.Bold;
            }
        }
    }
}