using System.Windows;
using System.Windows.Controls;
using KoAR.Core;

namespace KoAR.SaveEditor.Views
{
    public sealed class CoreEffectIndicator : Control
    {
        public static readonly DependencyProperty CoreEffectProperty = DependencyProperty.Register(nameof(CoreEffectIndicator.CoreEffect), typeof(CoreEffectInfo), typeof(CoreEffectIndicator),
            new PropertyMetadata());

        static CoreEffectIndicator()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(CoreEffectIndicator), new FrameworkPropertyMetadata(typeof(CoreEffectIndicator)));
        }

        public CoreEffectInfo? CoreEffect
        {
            get => (CoreEffectInfo?)this.GetValue(CoreEffectIndicator.CoreEffectProperty);
            set => this.SetValue(CoreEffectIndicator.CoreEffectProperty, value);
        }
    }
}
