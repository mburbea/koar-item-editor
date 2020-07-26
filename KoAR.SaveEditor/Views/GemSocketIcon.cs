using System.Windows;
using System.Windows.Controls;
using KoAR.Core;

namespace KoAR.SaveEditor.Views
{
    public sealed class GemSocketIcon : Control
    {
        public static readonly DependencyProperty GemSocketProperty = DependencyProperty.Register(nameof(GemSocketIcon.GemSocket), typeof(GemSocket), typeof(GemSocketIcon));

        static GemSocketIcon() => FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(GemSocketIcon), new FrameworkPropertyMetadata(typeof(GemSocketIcon)));

        public GemSocket GemSocket
        {
            get => (GemSocket)this.GetValue(GemSocketIcon.GemSocketProperty);
            set => this.SetValue(GemSocketIcon.GemSocketProperty, value);
        }
    }
}
