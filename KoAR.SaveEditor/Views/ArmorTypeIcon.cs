using System.Windows;
using System.Windows.Controls;
using KoAR.Core;

namespace KoAR.SaveEditor.Views
{
    public class ArmorTypeIcon : Control
    {
        public static readonly DependencyProperty ArmorTypeProperty = DependencyProperty.Register(nameof(ArmorTypeIcon.ArmorType), typeof(ArmorType), typeof(ArmorTypeIcon),
            new PropertyMetadata(ArmorType.None));

        static ArmorTypeIcon() => FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ArmorTypeIcon), new FrameworkPropertyMetadata(typeof(ArmorTypeIcon)));

        public ArmorType ArmorType
        {
            get => (ArmorType)this.GetValue(ArmorTypeIcon.ArmorTypeProperty);
            set => this.SetValue(ArmorTypeIcon.ArmorTypeProperty, value);
        }
    }
}
