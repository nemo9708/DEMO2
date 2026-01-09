using System;
using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DEMO2.Drivers
{
    public partial class DTP7HConnection : UserControl
    {
        public DTP7HDriver Driver { get; private set; }

        public DTP7HConnection()
        {
            InitializeComponent();

            Driver = new DTP7HDriver();

            LoadComPorts();
            LoadBaudRates(); // [추가] 보레이트 목록 로드
        }

        private void LoadComPorts()
        {
            CmbPort.Items.Clear();
            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                CmbPort.Items.Add(port);
            }
            if (CmbPort.Items.Count > 0) CmbPort.SelectedIndex = 0;
        }

        // [추가] 보레이트 목록 채우기
        private void LoadBaudRates()
        {
            CmbBaud.Items.Clear();
            // DTP7H 기본값 115200을 포함하여 자주 쓰는 속도 추가
            int[] bauds = { 9600, 19200, 38400, 57600, 115200 };

            foreach (int baud in bauds)
            {
                CmbBaud.Items.Add(baud);
            }

            // 기본값 115200 선택
            CmbBaud.SelectedItem = 115200;
        }

        private void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            if (Driver.IsConnected)
            {
                Driver.Disconnect();
                UpdateUI(false);
            }
            else
            {
                if (CmbPort.SelectedItem == null || CmbBaud.SelectedItem == null)
                {
                    MessageBox.Show("Please select Port and Baud Rate.");
                    return;
                }

                string portName = CmbPort.SelectedItem.ToString();
                int baudRate = (int)CmbBaud.SelectedItem; // 선택된 보레이트 가져오기

                // [수정] 보레이트도 같이 전달
                bool success = Driver.Connect(portName, baudRate);

                if (success)
                {
                    UpdateUI(true);
                }
                else
                {
                    MessageBox.Show("Connection Failed.");
                    UpdateUI(false);
                }
            }
        }

        private void UpdateUI(bool isConnected)
        {
            if (isConnected)
            {
                StatusLed.Fill = Brushes.LimeGreen;
                BtnConnect.Content = "Close";
                CmbPort.IsEnabled = false;
                CmbBaud.IsEnabled = false; // 연결 중엔 수정 불가
            }
            else
            {
                StatusLed.Fill = Brushes.Red;
                BtnConnect.Content = "Connect";
                CmbPort.IsEnabled = true;
                CmbBaud.IsEnabled = true;
            }
        }
    }
}