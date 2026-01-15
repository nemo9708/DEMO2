using System.Windows;
using System.Windows.Controls;
using DEMO2.Drivers; // DTP7HDriver가 있는 네임스페이스

namespace DEMO2.Manual
{
    public partial class DriverTestView : UserControl
    {
        private DTP7HDriver _driver;

        public DriverTestView()
        {
            InitializeComponent();
            this.Loaded += DriverTestView_Loaded;
        }

        private void DriverTestView_Loaded(object sender, RoutedEventArgs e)
        {
            // 메인 윈도우에서 드라이버 인스턴스 가져오기
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null && mainWindow.MyDriverControl != null)
            {
                _driver = mainWindow.MyDriverControl.Driver;
            }
        }

        // 로그 출력 헬퍼 메서드 (디버깅용)
        private void Log(string msg)
        {
            Dispatcher.Invoke(() =>
            {
                // XAML에 LogBox(TextBox)가 있다고 가정
                if (LogBox != null)
                {
                    LogBox.AppendText(msg + "\r\n");
                    LogBox.ScrollToEnd();
                }
            });
        }

        // [공통 전송 메서드] 
        // 버튼 클릭 시 이 메서드를 호출하여 로그와 전송을 동시에 처리합니다.
        private void SendLed(byte ledId, byte color, string ledName)
        {
            if (_driver == null)
            {
                Log("Error: Driver is null");
                return;
            }

            if (!_driver.IsConnected)
            {
                Log("Warning: Not connected");
            }

            _driver.SetLed(ledId, color);
            // 실제 전송되는 주소값 확인 (예: 0xC1이 찍혀야 정상)
            Log($"Sent: {ledName} (ID: 0x{ledId:X2}, Color: 0x{color:X2})");
        }


        // --- Buzzer ---
        private void BtnBuzzerOn_Click(object sender, RoutedEventArgs e)
        {
            _driver?.SetBuzzer(true);
            Log("Sent: Buzzer ON");
        }

        private void BtnBuzzerOff_Click(object sender, RoutedEventArgs e)
        {
            _driver?.SetBuzzer(false);
            Log("Sent: Buzzer OFF");
        }


        // --- Left LEDs (상수 사용으로 전면 교체) ---

        // Left LED 1
        private void BtnLeftLed1Blue_Click(object sender, RoutedEventArgs e)
            => SendLed(DTP7HDriver.LED_LEFT_1, DTP7HDriver.LED_COLOR_BLUE, "Left1 Blue");

        private void BtnLeftLed1Red_Click(object sender, RoutedEventArgs e)
            => SendLed(DTP7HDriver.LED_LEFT_1, DTP7HDriver.LED_COLOR_RED, "Left1 Red");

        private void BtnLeftLed1Off_Click(object sender, RoutedEventArgs e)
            => SendLed(DTP7HDriver.LED_LEFT_1, DTP7HDriver.LED_COLOR_OFF, "Left1 Off");

        // Left LED 2
        private void BtnLeftLed2Blue_Click(object sender, RoutedEventArgs e)
            => SendLed(DTP7HDriver.LED_LEFT_2, DTP7HDriver.LED_COLOR_BLUE, "Left2 Blue");

        private void BtnLeftLed2Red_Click(object sender, RoutedEventArgs e)
            => SendLed(DTP7HDriver.LED_LEFT_2, DTP7HDriver.LED_COLOR_RED, "Left2 Red");

        private void BtnLeftLed2Off_Click(object sender, RoutedEventArgs e)
            => SendLed(DTP7HDriver.LED_LEFT_2, DTP7HDriver.LED_COLOR_OFF, "Left2 Off");

        // Left LED 3
        private void BtnLeftLed3Blue_Click(object sender, RoutedEventArgs e)
            => SendLed(DTP7HDriver.LED_LEFT_3, DTP7HDriver.LED_COLOR_BLUE, "Left3 Blue");

        private void BtnLeftLed3Red_Click(object sender, RoutedEventArgs e)
            => SendLed(DTP7HDriver.LED_LEFT_3, DTP7HDriver.LED_COLOR_RED, "Left3 Red");

        private void BtnLeftLed3Off_Click(object sender, RoutedEventArgs e)
            => SendLed(DTP7HDriver.LED_LEFT_3, DTP7HDriver.LED_COLOR_OFF, "Left3 Off");


        // --- Right LEDs (상수 사용으로 전면 교체) ---

        // Right LED 1
        private void BtnRightLed1Blue_Click(object sender, RoutedEventArgs e)
            => SendLed(DTP7HDriver.LED_RIGHT_1, DTP7HDriver.LED_COLOR_BLUE, "Right1 Blue");

        private void BtnRightLed1Red_Click(object sender, RoutedEventArgs e)
            => SendLed(DTP7HDriver.LED_RIGHT_1, DTP7HDriver.LED_COLOR_RED, "Right1 Red");

        private void BtnRightLed1Off_Click(object sender, RoutedEventArgs e)
            => SendLed(DTP7HDriver.LED_RIGHT_1, DTP7HDriver.LED_COLOR_OFF, "Right1 Off");

        // Right LED 2
        private void BtnRightLed2Blue_Click(object sender, RoutedEventArgs e)
            => SendLed(DTP7HDriver.LED_RIGHT_2, DTP7HDriver.LED_COLOR_BLUE, "Right2 Blue");

        private void BtnRightLed2Red_Click(object sender, RoutedEventArgs e)
            => SendLed(DTP7HDriver.LED_RIGHT_2, DTP7HDriver.LED_COLOR_RED, "Right2 Red");

        private void BtnRightLed2Off_Click(object sender, RoutedEventArgs e)
            => SendLed(DTP7HDriver.LED_RIGHT_2, DTP7HDriver.LED_COLOR_OFF, "Right2 Off");

        // Right LED 3
        private void BtnRightLed3Blue_Click(object sender, RoutedEventArgs e)
            => SendLed(DTP7HDriver.LED_RIGHT_3, DTP7HDriver.LED_COLOR_BLUE, "Right3 Blue");

        private void BtnRightLed3Red_Click(object sender, RoutedEventArgs e)
            => SendLed(DTP7HDriver.LED_RIGHT_3, DTP7HDriver.LED_COLOR_RED, "Right3 Red");

        private void BtnRightLed3Off_Click(object sender, RoutedEventArgs e)
            => SendLed(DTP7HDriver.LED_RIGHT_3, DTP7HDriver.LED_COLOR_OFF, "Right3 Off");
    }
}