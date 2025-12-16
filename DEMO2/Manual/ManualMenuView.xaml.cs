using System;
using System.Windows;
using System.Windows.Controls;

namespace DEMO2.Manual
{
    public partial class ManualMenuView : UserControl
    {
        // 1. 이벤트 정의: EventHandler 뒤의 버튼이 눌렸음을 알림
        public event EventHandler StationTeachingClicked;
        public event EventHandler SettingClicked;

        public ManualMenuView()
        {
            InitializeComponent();
        }

        // 2. 버튼 클릭 핸들러
        private void BtnStationTeaching_Click(object sender, RoutedEventArgs e)
        {
            // 이벤트를 구독한 쪽(ManualView)에 신호를 보냄
            StationTeachingClicked?.Invoke(this, EventArgs.Empty);
        }

        private void BtnSetting_Click(object sender, RoutedEventArgs e)
        {
            SettingClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}