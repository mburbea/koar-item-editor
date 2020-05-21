using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using KoAR.Core;

namespace KoAR.SaveEditor.Views
{
    public sealed class EffectListControl : Control
    {
        public static readonly DependencyProperty AddEffectCommandProperty = DependencyProperty.Register(nameof(EffectListControl.AddEffectCommand), typeof(ICommand), typeof(EffectListControl));

        public static readonly DependencyProperty AvailableEffectsProperty = DependencyProperty.Register(nameof(EffectListControl.AvailableEffects), typeof(IEnumerable<IEffectInfo>), typeof(EffectListControl));

        public static readonly DependencyProperty CapacityProperty = DependencyProperty.Register(nameof(EffectListControl.Capacity), typeof(int), typeof(EffectListControl));

        public static readonly DependencyProperty DeleteEffectCommandProperty = DependencyProperty.Register(nameof(EffectListControl.DeleteEffectCommand), typeof(ICommand), typeof(EffectListControl));

        public static readonly DependencyProperty EffectsProperty = DependencyProperty.Register(nameof(EffectListControl.Effects), typeof(IEnumerable<IEffectInfo>), typeof(EffectListControl),
            new FrameworkPropertyMetadata(EffectListControl.EffectsProperty_ValueChanged));

        public static readonly DependencyProperty SelectedEffectProperty = DependencyProperty.Register(nameof(EffectListControl.SelectedEffect), typeof(IEffectInfo), typeof(EffectListControl),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        static EffectListControl() => FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(EffectListControl), new FrameworkPropertyMetadata(typeof(EffectListControl)));

        public ICommand? AddEffectCommand
        {
            get => (ICommand?)this.GetValue(EffectListControl.AddEffectCommandProperty);
            set => this.SetValue(EffectListControl.AddEffectCommandProperty, value);
        }

        public IEnumerable<IEffectInfo>? AvailableEffects
        {
            get => (IEnumerable<IEffectInfo>?)this.GetValue(EffectListControl.AvailableEffectsProperty);
            set => this.SetValue(EffectListControl.AvailableEffectsProperty, value);
        }

        public int Capacity
        {
            get => (int)this.GetValue(EffectListControl.CapacityProperty);
            set => this.SetValue(EffectListControl.CapacityProperty, value);
        }

        public ICommand? DeleteEffectCommand
        {
            get => (ICommand?)this.GetValue(EffectListControl.DeleteEffectCommandProperty);
            set => this.SetValue(EffectListControl.DeleteEffectCommandProperty, value);
        }

        public IEnumerable<IEffectInfo>? Effects
        {
            get => (IEnumerable<IEffectInfo>?)this.GetValue(EffectListControl.EffectsProperty);
            set => this.SetValue(EffectListControl.EffectsProperty, value);
        }

        public IEffectInfo? SelectedEffect
        {
            get => (IEffectInfo?)this.GetValue(EffectListControl.SelectedEffectProperty);
            set => this.SetValue(EffectListControl.SelectedEffectProperty, value);
        }

        private static void EffectsProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((EffectListControl)d).SelectedEffect = ((IEnumerable<IEffectInfo>?)e.NewValue)?.FirstOrDefault();
        }
    }
}
