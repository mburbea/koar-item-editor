using System.Windows;
using System.Windows.Controls;
using KoAR.Core;

namespace KoAR.SaveEditor.Views
{
    public sealed class DamageTypeIndicator : Control
    {
        public static readonly DependencyProperty CoreEffectProperty = DependencyProperty.Register(nameof(DamageTypeIndicator.CoreEffect), typeof(CoreEffectInfo), typeof(DamageTypeIndicator),
            new PropertyMetadata());

        static DamageTypeIndicator()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(DamageTypeIndicator), new FrameworkPropertyMetadata(typeof(DamageTypeIndicator)));
        }

        public CoreEffectInfo? CoreEffect
        {
            get => (CoreEffectInfo?)this.GetValue(DamageTypeIndicator.CoreEffectProperty);
            set => this.SetValue(DamageTypeIndicator.CoreEffectProperty, value);
        }
    }
}
