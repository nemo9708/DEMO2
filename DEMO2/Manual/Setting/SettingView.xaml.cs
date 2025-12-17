using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DEMO2.Manual.Setting;

namespace DEMO2.Manual.Setting
{
    public partial class SettingView : UserControl
    {
        private readonly Brush _activeBrush;
        private readonly Brush _inactiveBrush;

        private readonly UserControl _originOffsetView;
        private readonly UserControl _systemPointView;
        private readonly UserControl _retryView;
        private readonly UserControl _vacuumView;
        private readonly UserControl _softLimitView;
        private readonly UserControl _othersView;

        private readonly Dictionary<Button, UserControl> _navigationMap;

        public SettingView()
        {
            InitializeComponent();

            _activeBrush = (Brush)FindResource("NavActive");
            _inactiveBrush = (Brush)FindResource("NavInactive");

            _originOffsetView = new OriginOffsetView();
            _systemPointView = new SystemPointView();
            _retryView = CreatePlaceholderView("Retry Set.");
            _vacuumView = CreatePlaceholderView("Vacuum Set.");
            _softLimitView = CreatePlaceholderView("Soft Limits");
            _othersView = CreatePlaceholderView("Others");

            _navigationMap = new Dictionary<Button, UserControl>
            {
                { btnOriginOffset, _originOffsetView },
                { btnSystemPoint, _systemPointView },
                { btnRetrySet, _retryView },
                { btnVacuumSet, _vacuumView },
                { btnSoftLimits, _softLimitView },
                { btnOthers, _othersView }
            };

            ActivateButton(btnOriginOffset);
        }

        private void OnOriginOffsetClicked(object sender, RoutedEventArgs e)
        {
            ActivateButton(btnOriginOffset);
        }

        private void OnSystemPointClicked(object sender, RoutedEventArgs e)
        {
            ActivateButton(btnSystemPoint);
        }

        private void OnRetrySetClicked(object sender, RoutedEventArgs e)
        {
            ActivateButton(btnRetrySet);
        }

        private void OnVacuumSetClicked(object sender, RoutedEventArgs e)
        {
            ActivateButton(btnVacuumSet);
        }

        private void OnSoftLimitsClicked(object sender, RoutedEventArgs e)
        {
            ActivateButton(btnSoftLimits);
        }

        private void OnOthersClicked(object sender, RoutedEventArgs e)
        {
            ActivateButton(btnOthers);
        }

        private void ActivateButton(Button target)
        {
            foreach (var entry in _navigationMap)
            {
                var button = entry.Key;
                var view = entry.Value;

                if (button == target)
                {
                    button.Background = _activeBrush;
                    ContentArea.Content = view;
                }
                else
                {
                    button.Background = _inactiveBrush;
                }
            }
        }

        private UserControl CreatePlaceholderView(string title)
        {
            var grid = new Grid
            {
                Background = Brushes.White
            };

            grid.Children.Add(new TextBlock
            {
                Text = title + " 화면 준비 중",
                FontSize = 24,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.Gray,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            });

            return new UserControl
            {
                Content = grid
            };
        }
    }
}