using System.Windows;
using System.Windows.Controls;
using KoAR.Core;

namespace KoAR.SaveEditor.Views
{
    public sealed class CoreEffectIcon : Control
    {
        public static readonly DependencyProperty CodeProperty = DependencyProperty.Register(nameof(CoreEffectIcon.Code), typeof(uint?), typeof(CoreEffectIcon),
            new PropertyMetadata(null, CoreEffectIcon.CodeProperty_ValueChanged));

        private static readonly DependencyPropertyKey _coreEffectPropertyKey = DependencyProperty.RegisterReadOnly(nameof(CoreEffectIcon.CoreEffect), typeof(CoreEffectInfo), typeof(CoreEffectIcon),
            new PropertyMetadata());

        static CoreEffectIcon() => FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(CoreEffectIcon), new FrameworkPropertyMetadata(typeof(CoreEffectIcon)));

        public static DependencyProperty CoreEffectProperty => CoreEffectIcon._coreEffectPropertyKey.DependencyProperty;

        public uint? Code
        {
            get => (uint?)this.GetValue(CoreEffectIcon.CodeProperty);
            set => this.SetValue(CoreEffectIcon.CodeProperty, value);
        }

        public CoreEffectInfo? CoreEffect
        {
            get => (CoreEffectInfo?)this.GetValue(CoreEffectIcon.CoreEffectProperty);
            private set => this.SetValue(CoreEffectIcon._coreEffectPropertyKey, value);
        }

        private static void CodeProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CoreEffectIcon icon = (CoreEffectIcon)d;
            if (e.NewValue == null)
            {
                icon.CoreEffect = null;
                return;
            }
            uint code = (uint)e.NewValue;
            icon.CoreEffect = Amalur.CoreEffects.TryGetValue(code, out CoreEffectInfo info)
                ? info
                : new CoreEffectInfo(code, DamageType.Unknown);
        }
    }
}
