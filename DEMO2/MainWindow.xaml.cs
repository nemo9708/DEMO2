using System;
using System.Windows;
using System.Runtime.InteropServices; // 윈도우 API 사용을 위해 필수
using System.Threading.Tasks;         // 버저 딜레이(Delay)를 위해 필요
using DEMO2.Manual;   // ManualView 사용
using DEMO2.Driver;  // DTP7HDriver 사용

namespace DEMO2
{
    public partial class MainWindow : Window
    {
        // =========================================================
        // ▼ 윈도우 시스템 함수(키보드 이벤트) 가져오기
        // =========================================================
        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        // 매뉴얼 8페이지: LED 및 버저 제어 코드 (가상 키 코드)
        const byte VK_LED1_BLUE = 0xCA; // 파란불 (Right LED1)
        const byte VK_BUZZER = 0xD3; // 버저
        const int KEYEVENTF_KEYUP = 0x02; // 키 떼는 동작

        public MainWindow()
        {
            InitializeComponent();

            // ▼ 드라이버 이벤트 연결
            // (주의: XAML에 MyDriverControl이 있어야 함)
            if (MyDriverControl != null)
            {
                MyDriverControl.Driver.KeypadEvent += OnDriverKeypadEvent;
            }

            // 버튼 이벤트 연결
            btnManual.Click += BtnManual_Click;
            btnAuto.Click += BtnAuto_Click;
            btnDriverTest.Click += BtnDriverTest_Click;
            btnLock.Click += BtnLock_Click;
            btnCloseApp.Click += BtnCloseApp_Click;

            // [수정됨] 초기 화면 설정 시 드라이버 전달
            // ManualView 생성자에 MyDriverControl.Driver를 인수로 전달합니다.
            if (MyDriverControl != null)
            {
                MainContentArea.Content = new ManualView(MyDriverControl.Driver);
            }
        }

        // ▼ 드라이버 키패드 이벤트 핸들러
        private void OnDriverKeypadEvent(object sender, KeypadEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (e.IsDown)
                {
                    Console.WriteLine($"[Main] Key Down: {e.Key}");

                    // (활용 예시) 키패드의 특정 키를 누르면 LED를 켜고 싶다면?
                    // if (e.Key.ToString() == "F1") TurnOnLed1(); 
                }
                else
                {
                    Console.WriteLine($"[Main] Key Up: {e.Key}");
                    // if (e.Key.ToString() == "F1") TurnOffLed1();
                }
            });
        }

        // =========================================================
        // ▼ LED/버저 제어 함수 (필요할 때 호출해서 쓰기)
        // =========================================================

        // LED 켜기 함수
        public void TurnOnLed1()
        {
            keybd_event(VK_LED1_BLUE, 0, 0, 0); // 누름 신호 -> 켜짐
        }

        // LED 끄기 함수
        public void TurnOffLed1()
        {
            keybd_event(VK_LED1_BLUE, 0, KEYEVENTF_KEYUP, 0); // 뗌 신호 -> 꺼짐
        }

        // 버저 울리기 함수 (비동기)
        public async void BeepBuzzer()
        {
            keybd_event(VK_BUZZER, 0, 0, 0);       // 소리 시작
            await Task.Delay(500);                 // 0.5초 대기 (UI 안 멈춤)
            keybd_event(VK_BUZZER, 0, KEYEVENTF_KEYUP, 0); // 소리 끝
        }
        // =========================================================

        // ▼ 화면 전환 및 버튼 핸들러들
        private void BtnManual_Click(object sender, RoutedEventArgs e)
        {
            // [수정됨] 버튼 클릭 시에도 드라이버 전달
            if (MyDriverControl != null)
            {
                MainContentArea.Content = new ManualView(MyDriverControl.Driver);
            }
        }

        private void BtnDriverTest_Click(object sender, RoutedEventArgs e)
        {
            // 기존 화면 전환 기능 유지
            MainContentArea.Content = new DriverTestView();

            // [테스트] 버튼을 누를 때마다 "삑" 소리가 나게 하려면 아래 주석을 푸세요
            // BeepBuzzer(); 

            // [테스트] 버튼 누를 때 LED도 켜보고 싶다면?
            // TurnOnLed1();
            // (끄는 건 TurnOffLed1()을 어딘가에서 호출해야 합니다)
        }

        private void BtnAuto_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = null;
        }

        private void BtnLock_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = null;
        }

        private void BtnCloseApp_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}