using System.Windows;
using System.Windows.Controls;
using KoAR.Core;

namespace KoAR.SaveEditor.Views
{
    public sealed class DamageTypeControl : Control
    {
        public static readonly DependencyProperty CoreEffectProperty = DependencyProperty.Register(nameof(DamageTypeControl.CoreEffect), typeof(CoreEffectInfo), typeof(DamageTypeControl),
            new PropertyMetadata());

        static DamageTypeControl()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(DamageTypeControl), new FrameworkPropertyMetadata(typeof(DamageTypeControl)));
        }

        public CoreEffectInfo? CoreEffect
        {
            get => (CoreEffectInfo?)this.GetValue(DamageTypeControl.CoreEffectProperty);
            set => this.SetValue(DamageTypeControl.CoreEffectProperty, value);
        }
    }
}
