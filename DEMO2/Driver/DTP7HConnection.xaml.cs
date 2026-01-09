using System;
using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace DEMO2.Drivers
{
    public partial class DTP7HConnection : UserControl
    {
        // 실제 드라이버 인스턴스 (외부에서 주입받거나 여기서 생성)
        public DTP7HDriver Driver { get; private set; }

        public DTP7HConnection()
        {
            InitializeComponent();

            // 드라이버 생성
            Driver = new DTP7HDriver();

            // 포트 목록 로드
            LoadComPorts();

            // (옵션) 포트 목록을 주기적으로 갱신하고 싶다면 타이머 사용 가능
        }

        private void LoadComPorts()
        {
            CmbPort.Items.Clear();
            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                CmbPort.Items.Add(port);
            }

            if (CmbPort.Items.Count > 0)
                CmbPort.SelectedIndex = 0;
        }

        private void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            if (Driver.IsConnected)
            {
                // 연결 해제
                Driver.Disconnect();
                UpdateUI(false);
            }
            else
            {
                // 연결 시도
                if (CmbPort.SelectedItem == null)
                {
                    MessageBox.Show("Please select a COM port.");
                    return;
                }

                string portName = CmbPort.SelectedItem.ToString();
                bool success = Driver.Connect(portName);

                if (success)
                {
                    UpdateUI(true);
                }
                else
                {
                    MessageBox.Show("Connection Failed. Check Port or Cable.");
                    UpdateUI(false);
                }
            }
        }

        private void UpdateUI(bool isConnected)
        {
            if (isConnected)
            {
                StatusLed.Fill = Brushes.LimeGreen; // 연결됨 (녹색)
                BtnConnect.Content = "Close";
                CmbPort.IsEnabled = false;
            }
            else
            {
                StatusLed.Fill = Brushes.Red;       // 끊김 (빨강)
                BtnConnect.Content = "Connect";
                CmbPort.IsEnabled = true;
            }
        }
    }
}