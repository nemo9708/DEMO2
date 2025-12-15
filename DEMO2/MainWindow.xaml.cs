using System.Windows;
using DEMO2.Manual; // Manual 폴더 안의 화면을 쓰기 위해 필수

namespace DEMO2
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // 이벤트 핸들러 연결
            btnManual.Click += BtnManual_Click;
            btnAuto.Click += BtnAuto_Click;
            btnLock.Click += BtnLock_Click;
        }

        private void BtnManual_Click(object sender, RoutedEventArgs e)
        {
            // ManualView를 표시
            MainContentArea.Content = new ManualView();
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
    }
}