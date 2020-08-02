using System.Windows;
using System.Windows.Controls;

namespace KoAR.SaveEditor.Constructs
{
    public enum ProgressBarState
    {
        Default = 0,
        Warning = 1,
        Error = 2
    }

    public sealed class StatefulProgressBar : ProgressBar
    {
        public static readonly DependencyProperty StateProperty = DependencyProperty.Register("State", typeof(ProgressBarState), typeof(StatefulProgressBar),
            new PropertyMetadata(ProgressBarState.Default));

        static StatefulProgressBar() => FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(StatefulProgressBar), new FrameworkPropertyMetadata(typeof(StatefulProgressBar)));

        public ProgressBarState State
        {
            get => (ProgressBarState)this.GetValue(StatefulProgressBar.StateProperty);
            set => this.SetValue(StatefulProgressBar.StateProperty, value);
        }
    }
}
