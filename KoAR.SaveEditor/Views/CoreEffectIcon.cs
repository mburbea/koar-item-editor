using System.Windows;
using System.Windows.Controls;
using KoAR.Core;

namespace KoAR.SaveEditor.Views
{
    public sealed class CoreEffectIcon : Control
    {
        public static readonly DependencyProperty CoreEffectProperty = DependencyProperty.Register(nameof(CoreEffectIcon.CoreEffect), typeof(CoreEffectInfo), typeof(CoreEffectIcon),
            new PropertyMetadata());

        static CoreEffectIcon()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(CoreEffectIcon), new FrameworkPropertyMetadata(typeof(CoreEffectIcon)));
        }

        public CoreEffectInfo? CoreEffect
        {
            get => (CoreEffectInfo?)this.GetValue(CoreEffectIcon.CoreEffectProperty);
            set => this.SetValue(CoreEffectIcon.CoreEffectProperty, value);
        }
    }
}
