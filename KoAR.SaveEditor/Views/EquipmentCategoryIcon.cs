using System.Windows;
using System.Windows.Controls;
using KoAR.Core;

namespace KoAR.SaveEditor.Views
{
    public sealed class EquipmentCategoryIcon : Control
    {
        public static readonly DependencyProperty CategoryProperty = DependencyProperty.Register(nameof(EquipmentCategoryIcon.Category), typeof(EquipmentCategory?), typeof(EquipmentCategoryIcon));

        static EquipmentCategoryIcon() => FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(EquipmentCategoryIcon), new FrameworkPropertyMetadata(typeof(EquipmentCategoryIcon)));

        public EquipmentCategory? Category
        {
            get => (EquipmentCategory?)this.GetValue(EquipmentCategoryIcon.CategoryProperty);
            set => this.SetValue(EquipmentCategoryIcon.CategoryProperty, value);
        }
    }
}
