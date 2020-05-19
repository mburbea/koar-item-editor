using System.Windows;
using System.Windows.Controls;
using KoAR.Core;

namespace KoAR.SaveEditor.Views
{
    public sealed class EquipmentTypeIndicator : Control
    {
        public static readonly DependencyProperty EquipmentTypeProperty = DependencyProperty.Register(nameof(EquipmentTypeIndicator.EquipmentType), typeof(EquipmentType?), typeof(EquipmentTypeIndicator));

        static EquipmentTypeIndicator()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(EquipmentTypeIndicator), new FrameworkPropertyMetadata(typeof(EquipmentTypeIndicator)));
        }

        public EquipmentType? EquipmentType
        {
            get => (EquipmentType?)this.GetValue(EquipmentTypeIndicator.EquipmentTypeProperty);
            set => this.SetValue(EquipmentTypeIndicator.EquipmentTypeProperty, value);
        }
    }
}
