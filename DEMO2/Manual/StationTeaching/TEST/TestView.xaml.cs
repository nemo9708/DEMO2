using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DEMO2.Manual.StationTeaching.TEST
{
    /// <summary>
    /// TestView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TestView : UserControl
    {
        private readonly Brush _activeNavBrush;
        private readonly Brush _inactiveNavBrush;

        public TestView()
        {
            InitializeComponent();
            _activeNavBrush = (Brush)FindResource("GreenBrush");
            _inactiveNavBrush = (Brush)FindResource("SkyBlue");

            InitializeDropdowns();
            SetNavigationState(true);
        }

        private void InitializeDropdowns()
        {
            var numbers = Enumerable.Range(1, 10).ToList();
            cmbGroup.ItemsSource = numbers;
            cmbGroup.SelectedIndex = 0;

            cmbCassette.ItemsSource = numbers;
            cmbCassette.SelectedIndex = 0;
        }

        private void OnCassetteClick(object sender, RoutedEventArgs e)
        {
            SetNavigationState(true);
        }

        private void OnStageClick(object sender, RoutedEventArgs e)
        {
            SetNavigationState(false);
        }

        private void SetNavigationState(bool isCassetteActive)
        {
            btnCassette.Background = isCassetteActive ? _activeNavBrush : _inactiveNavBrush;
            btnStage.Background = isCassetteActive ? _inactiveNavBrush : _activeNavBrush;
        }
    }
}