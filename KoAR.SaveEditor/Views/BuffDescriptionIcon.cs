using System.Windows;
using System.Windows.Controls;
using KoAR.Core;

namespace KoAR.SaveEditor.Views
{
    public sealed class BuffDescriptionIcon : Control
    {
        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(nameof(BuffDescriptionIcon.Description), typeof(BuffDescription), typeof(BuffDescriptionIcon));

        static BuffDescriptionIcon() => FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(BuffDescriptionIcon), new FrameworkPropertyMetadata(typeof(BuffDescriptionIcon)));

        public BuffDescription? Description
        {
            get => (BuffDescription?)this.GetValue(BuffDescriptionIcon.DescriptionProperty);
            set => this.SetValue(BuffDescriptionIcon.DescriptionProperty, value);
        }
    }
}
