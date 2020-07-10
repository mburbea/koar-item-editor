using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Views
{
    public sealed class EffectsControl : Control
    {
        public static readonly DependencyProperty AddEffectCommandProperty = DependencyProperty.Register(nameof(EffectsControl.AddEffectCommand), typeof(ICommand), typeof(EffectsControl));

        public static readonly DependencyProperty BuffsProperty = DependencyProperty.Register(nameof(EffectsControl.Buffs), typeof(IReadOnlyList<Buff>), typeof(EffectsControl),
            new PropertyMetadata(EffectsControl.BuffsProperty_ValueChanged));

        public static readonly DependencyProperty BuffsFilterProperty = DependencyProperty.Register(nameof(EffectsControl.BuffsFilter), typeof(BuffsFilter), typeof(EffectsControl));

        public static readonly DependencyProperty CapacityProperty = DependencyProperty.Register(nameof(EffectsControl.Capacity), typeof(int), typeof(EffectsControl),
            new PropertyMetadata(int.MaxValue));

        public static readonly DependencyProperty DeleteEffectCommandProperty = DependencyProperty.Register(nameof(EffectsControl.DeleteEffectCommand), typeof(ICommand), typeof(EffectsControl));

        public static readonly DependencyProperty EffectsProperty = DependencyProperty.Register(nameof(EffectsControl.Effects), typeof(IEnumerable<Buff>), typeof(EffectsControl));

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(nameof(EffectsControl.Header), typeof(object), typeof(EffectsControl));

        public static readonly DependencyProperty HeaderTemplateProperty = DependencyProperty.Register(nameof(EffectsControl.HeaderTemplate), typeof(DataTemplate), typeof(EffectsControl));

        public static readonly DependencyProperty PendingEffectIdProperty = DependencyProperty.Register(nameof(EffectsControl.PendingEffectId), typeof(uint?), typeof(EffectsControl));

        public static readonly DependencyProperty PendingEffectProperty = DependencyProperty.Register(nameof(EffectsControl.PendingEffect), typeof(Buff), typeof(EffectsControl),
            new PropertyMetadata(EffectsControl.PendingEffectProperty_ValueChanged));

        public static readonly DependencyProperty UnsupportedFormatProperty = DependencyProperty.Register(nameof(EffectsControl.UnsupportedFormat), typeof(bool), typeof(EffectsControl),
            new PropertyMetadata(BooleanBoxes.False));

        static EffectsControl() => FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(EffectsControl), new FrameworkPropertyMetadata(typeof(EffectsControl)));

        public ICommand? AddEffectCommand
        {
            get => (ICommand?)this.GetValue(EffectsControl.AddEffectCommandProperty);
            set => this.SetValue(EffectsControl.AddEffectCommandProperty, value);
        }

        public IReadOnlyList<Buff>? Buffs
        {
            get => (IReadOnlyList<Buff>?)this.GetValue(EffectsControl.BuffsProperty);
            set => this.SetValue(EffectsControl.BuffsProperty, value);
        }

        public BuffsFilter BuffsFilter
        {
            get => (BuffsFilter)this.GetValue(EffectsControl.BuffsFilterProperty);
            set => this.SetValue(EffectsControl.BuffsFilterProperty, value);
        }

        public int Capacity
        {
            get => (int)this.GetValue(EffectsControl.CapacityProperty);
            set => this.SetValue(EffectsControl.CapacityProperty, value);
        }

        public ICommand? DeleteEffectCommand
        {
            get => (ICommand?)this.GetValue(EffectsControl.DeleteEffectCommandProperty);
            set => this.SetValue(EffectsControl.DeleteEffectCommandProperty, value);
        }

        public IEnumerable<Buff>? Effects
        {
            get => (IEnumerable<Buff>?)this.GetValue(EffectsControl.EffectsProperty);
            set => this.SetValue(EffectsControl.EffectsProperty, value);
        }

        public object? Header
        {
            get => this.GetValue(EffectsControl.HeaderProperty);
            set => this.SetValue(EffectsControl.HeaderProperty, value);
        }

        public DataTemplate? HeaderTemplate
        {
            get => (DataTemplate?)this.GetValue(EffectsControl.HeaderTemplateProperty);
            set => this.SetValue(EffectsControl.HeaderTemplateProperty, value);
        }

        public Buff? PendingEffect
        {
            get => (Buff?)this.GetValue(EffectsControl.PendingEffectProperty);
            set => this.SetValue(EffectsControl.PendingEffectProperty, value);
        }

        public uint? PendingEffectId
        {
            get => (uint?)this.GetValue(EffectsControl.PendingEffectIdProperty);
            set => this.SetValue(EffectsControl.PendingEffectIdProperty, value);
        }

        public bool UnsupportedFormat
        {
            get => (bool)this.GetValue(EffectsControl.UnsupportedFormatProperty);
            set => this.SetValue(EffectsControl.UnsupportedFormatProperty, BooleanBoxes.GetBox(value));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (this.Template.FindName("PART_ListBox", this) is ListBox listBox)
            {
                listBox.CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, EffectsControl.CopyCommand_Executed));
            }
        }

        private static void CopyCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Clipboard.SetText(((Buff)((ListBox)sender).ItemContainerGenerator.ItemFromContainer((ListBoxItem)e.OriginalSource)).Id.ToString("X6"));
            e.Handled = true;
        }

        private static void BuffsProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //EffectsControl control = (EffectsControl)d;
            //control.PendingEffect = ((IReadOnlyList<Buff>?)e.NewValue)?.FirstOrDefault(buff => control.BuffsFilter.Matches(buff));
        }

        private static void PendingEffectProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((EffectsControl)d).PendingEffectId = ((Buff?)e.NewValue)?.Id;
        }
    }
}
