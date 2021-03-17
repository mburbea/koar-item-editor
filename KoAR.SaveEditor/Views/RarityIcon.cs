using System.Windows;
using System.Windows.Controls;
using KoAR.Core;

namespace KoAR.SaveEditor.Views
{
    public sealed class RarityIcon : Control
    {
        public static readonly DependencyProperty RarityProperty = DependencyProperty.Register(nameof(RarityIcon.Rarity), typeof(Rarity), typeof(RarityIcon),
            new(Rarity.None));

        static RarityIcon() => FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(RarityIcon), new FrameworkPropertyMetadata(typeof(RarityIcon)));

        public Rarity Rarity
        {
            get => (Rarity)this.GetValue(RarityIcon.RarityProperty);
            set => this.SetValue(RarityIcon.RarityProperty, value);
        }
    }
}
