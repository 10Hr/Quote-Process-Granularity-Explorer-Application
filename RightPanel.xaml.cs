using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ZSExplorer.View
{
    public partial class RightPanel : UserControl
    {
        public RightPanel()
        {
            InitializeComponent();

            Example: hook slider event to update label immediately
            TimeWindowSlider.ValueChanged += (s, e) =>
            {
                TimeWindowValueText.Text = $"Selected: {TimeWindowSlider.Value} days";
            };

            // Example: pulsing animation on StatusIndicator ellipse
            var pulseAnimation = new ColorAnimationUsingKeyFrames();
            pulseAnimation.KeyFrames.Add(new DiscreteColorKeyFrame(Colors.Green, KeyTime.FromTimeSpan(TimeSpan.Zero)));
            pulseAnimation.KeyFrames.Add(new DiscreteColorKeyFrame(Colors.Red, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.5))));
            pulseAnimation.RepeatBehavior = RepeatBehavior.Forever;

            var brush = new SolidColorBrush(Colors.Green);
            StatusIndicator.Fill = brush;
            brush.BeginAnimation(SolidColorBrush.ColorProperty, pulseAnimation);

            RemoveButton.Click += RemoveButton_Click;
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            // Raise an event or call parent method to remove this control
            OnRemoveRequested();
        }

        public event RoutedEventHandler RemoveRequested;

        protected virtual void OnRemoveRequested()
        {
            RemoveRequested?.Invoke(this, new RoutedEventArgs());
        }

        // You can add properties here to get/set fields, e.g.:
        // public string ContractSymbol
        // {
        //     get => ContractSymbolText.Text;
        //     set => ContractSymbolText.Text = value;
        // }

        // similarly for other text fields...
    }
}
