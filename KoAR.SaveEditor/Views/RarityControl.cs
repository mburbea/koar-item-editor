using System.Windows;
using System.Windows.Controls;
using KoAR.Core;

namespace KoAR.SaveEditor.Views
{
    public sealed class RarityControl : Control
    {
        public static readonly DependencyProperty RarityProperty = DependencyProperty.Register(nameof(RarityControl.Rarity), typeof(Rarity), typeof(RarityControl),
            new PropertyMetadata(Rarity.None));

        static RarityControl() => FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(RarityControl), new FrameworkPropertyMetadata(typeof(RarityControl)));

        public Rarity Rarity
        {
            get => (Rarity)this.GetValue(RarityControl.RarityProperty);
            set => this.SetValue(RarityControl.RarityProperty, value);
        }
    }
}
