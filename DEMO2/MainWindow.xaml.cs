using System;
using System.Windows;
using DEMO2.Manual;  // ManualView 사용을 위해 필요
using DEMO2.Drivers; // [중요] KeypadEventArgs, DTP7HDriver 사용을 위해 필수!

namespace DEMO2
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // ▼ [핵심] 드라이버의 키 이벤트에 내 함수를 연결(+=)
            // (주의: XAML에 <drivers:DTP7HConnection x:Name="MyDriverControl" ... />이 있어야 함)
            if (MyDriverControl != null)
            {
                MyDriverControl.Driver.KeypadEvent += OnDriverKeypadEvent;
            }

            // 이벤트 핸들러 연결
            btnManual.Click += BtnManual_Click;
            btnAuto.Click += BtnAuto_Click;
            btnDriverTest.Click += BtnDriverTest_Click;
            btnLock.Click += BtnLock_Click;
            btnCloseApp.Click += BtnCloseApp_Click;

            // 기본 화면은 Manual로 설정
            MainContentArea.Content = new ManualView();
        }

        // ▼ 이벤트가 발생하면 실행될 함수 (실제 동작)
        private void OnDriverKeypadEvent(object sender, KeypadEventArgs e)
        {
            // 통신은 별도 스레드에서 돌기 때문에, UI를 고치려면 Dispatcher가 필요
            this.Dispatcher.Invoke(() =>
            {
                if (e.IsDown)
                {
                    // 키가 눌렸을 때 할 일 (예: 조그 시작)
                    Console.WriteLine($"[Main] Key Down: {e.Key}");
                }
                else
                {
                    // 키를 뗐을 때 할 일 (예: 조그 정지)
                    Console.WriteLine($"[Main] Key Up: {e.Key}");
                }
            });
        }

        private void BtnManual_Click(object sender, RoutedEventArgs e)
        {
            // ManualView를 표시
            MainContentArea.Content = new ManualView();
        }

        private void BtnDriverTest_Click(object sender, RoutedEventArgs e)
        {
            // 중앙 화면을 DriverTestView로 변경
            // (DriverTestView.xaml이 DEMO2.Manual 폴더 등에 있어야 함)
            MainContentArea.Content = new DriverTestView();
        }

        private void BtnAuto_Click(object sender, RoutedEventArgs e)
        {
            // 아직 구현 안 됨 (빈 화면)
            MainContentArea.Content = null;
        }

        private void BtnLock_Click(object sender, RoutedEventArgs e)
        {
            // 아직 구현 안 됨 (빈 화면)
            MainContentArea.Content = null;
        }

        private void BtnCloseApp_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}