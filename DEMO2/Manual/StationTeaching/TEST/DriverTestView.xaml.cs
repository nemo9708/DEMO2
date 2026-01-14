using System.Windows;
using System.Windows.Controls;
using DEMO2.Drivers; // 드라이버 사용

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
            // 메인 윈도우의 드라이버 가져오기
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null && mainWindow.MyDriverControl != null)
            {
                _driver = mainWindow.MyDriverControl.Driver;
            }
        }

        private void BtnBuzzerOn_Click(object sender, RoutedEventArgs e)
        {
            _driver?.SetBuzzer(true);
        }

        private void BtnBuzzerOff_Click(object sender, RoutedEventArgs e)
        {
            _driver?.SetBuzzer(false);
        }

        // LED 1 (Left)
        private void BtnLed1Blue_Click(object sender, RoutedEventArgs e) => _driver?.SetLed(0x41, 0x31);
        private void BtnLed1Red_Click(object sender, RoutedEventArgs e) => _driver?.SetLed(0x41, 0x32);
        private void BtnLed1Off_Click(object sender, RoutedEventArgs e) => _driver?.SetLed(0x41, 0x30);

        // LED 2 (Left 2 - 예시)
        private void BtnLed2Blue_Click(object sender, RoutedEventArgs e) => _driver?.SetLed(0x42, 0x31);
        private void BtnLed2Red_Click(object sender, RoutedEventArgs e) => _driver?.SetLed(0x42, 0x32);
        private void BtnLed2Off_Click(object sender, RoutedEventArgs e) => _driver?.SetLed(0x42, 0x30);
    }
}