using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using KoAR.Core;

namespace KoAR.SaveEditor.Views
{
    public sealed class EffectsControl : Control
    {
        public static readonly DependencyProperty AddEffectCommandProperty = DependencyProperty.Register(nameof(EffectsControl.AddEffectCommand), typeof(ICommand), typeof(EffectsControl));

        public static readonly DependencyProperty BuffsProperty = DependencyProperty.Register(nameof(EffectsControl.Buffs), typeof(IReadOnlyDictionary<uint, Buff>), typeof(EffectsControl));

        public static readonly DependencyProperty CapacityProperty = DependencyProperty.Register(nameof(EffectsControl.Capacity), typeof(int), typeof(EffectsControl),
            new PropertyMetadata(int.MaxValue));

        public static readonly DependencyProperty DeleteEffectCommandProperty = DependencyProperty.Register(nameof(EffectsControl.DeleteEffectCommand), typeof(ICommand), typeof(EffectsControl));

        public static readonly DependencyProperty EffectDefinitionsProperty = DependencyProperty.Register(nameof(EffectsControl.EffectDefinitions), typeof(IEnumerable<IEffectInfo>), typeof(EffectsControl),
            new PropertyMetadata(EffectsControl.EffectDefinitionsProperty_ValueChanged));

        public static readonly IMultiValueConverter EffectDisplayTextConverter = new DisplayTextConverter();

        public static readonly DependencyProperty EffectsProperty = DependencyProperty.Register(nameof(EffectsControl.Effects), typeof(IEnumerable<uint>), typeof(EffectsControl));

        public static readonly DependencyProperty EffectTranslationsProperty = DependencyProperty.Register(nameof(EffectsControl.EffectTranslations), typeof(IDictionary), typeof(EffectsControl));

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(nameof(EffectsControl.Header), typeof(object), typeof(EffectsControl));

        public static readonly DependencyProperty HeaderTemplateProperty = DependencyProperty.Register(nameof(EffectsControl.HeaderTemplate), typeof(DataTemplate), typeof(EffectsControl));

        public static readonly DependencyProperty PendingEffectCodeProperty = DependencyProperty.Register(nameof(EffectsControl.PendingEffectCode), typeof(uint?), typeof(EffectsControl));

        public static readonly DependencyProperty PendingEffectProperty = DependencyProperty.Register(nameof(EffectsControl.PendingEffect), typeof(IEffectInfo), typeof(EffectsControl),
            new PropertyMetadata(EffectsControl.PendingEffectProperty_ValueChanged));

        static EffectsControl() => FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(EffectsControl), new FrameworkPropertyMetadata(typeof(EffectsControl)));

        public ICommand? AddEffectCommand
        {
            get => (ICommand?)this.GetValue(EffectsControl.AddEffectCommandProperty);
            set => this.SetValue(EffectsControl.AddEffectCommandProperty, value);
        }

        public IReadOnlyDictionary<uint, Buff>? Buffs
        {
            get => (IReadOnlyDictionary<uint, Buff>?)this.GetValue(EffectsControl.BuffsProperty);
            set => this.SetValue(EffectsControl.BuffsProperty, value);
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

        public IEnumerable<IEffectInfo>? EffectDefinitions
        {
            get => (IEnumerable<IEffectInfo>?)this.GetValue(EffectsControl.EffectDefinitionsProperty);
            set => this.SetValue(EffectsControl.EffectDefinitionsProperty, value);
        }

        public IEnumerable<uint>? Effects
        {
            get => (IEnumerable<uint>?)this.GetValue(EffectsControl.EffectsProperty);
            set => this.SetValue(EffectsControl.EffectsProperty, value);
        }

        public IDictionary? EffectTranslations
        {
            get => (IDictionary?)this.GetValue(EffectsControl.EffectTranslationsProperty);
            set => this.SetValue(EffectsControl.EffectTranslationsProperty, value);
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

        public IEffectInfo? PendingEffect
        {
            get => (IEffectInfo?)this.GetValue(EffectsControl.PendingEffectProperty);
            set => this.SetValue(EffectsControl.PendingEffectProperty, value);
        }

        public uint? PendingEffectCode
        {
            get => (uint?)this.GetValue(EffectsControl.PendingEffectCodeProperty);
            set => this.SetValue(EffectsControl.PendingEffectCodeProperty, value);
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
            Clipboard.SetText(((uint)((ListBox)sender).ItemContainerGenerator.ItemFromContainer((ListBoxItem)e.OriginalSource)).ToString("X6"));
            e.Handled = true;
        }

        private static void EffectDefinitionsProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((EffectsControl)d).PendingEffect = ((IEnumerable<IEffectInfo>?)e.NewValue)?.FirstOrDefault();
        }

        private static void PendingEffectProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((EffectsControl)d).PendingEffectCode = ((IEffectInfo?)e.NewValue)?.Code;
        }

        private sealed class DisplayTextConverter : IMultiValueConverter
        {
            object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                if (values.Length < 3 || !(values[0] is uint code && values[1] is IDictionary translations && values[2] is IReadOnlyDictionary<uint, Buff> buffs))
                {
                    return DependencyProperty.UnsetValue;
                }
                if (translations.Contains(code))
                {
                    return ((IEffectInfo)translations[code]).DisplayText;
                }
                if (buffs.TryGetValue(code, out Buff buff))
                {
                    if (buff.Flavor != null)
                    {
                        return buff.Flavor;
                    }
                    if (buff.Ap != BuffType.Suffix && buff.Ap != BuffType.Prefix && buff.Ap != BuffType.Self && buff.Modifier != null)
                    {
                        return buff.Modifier;
                    }
                    if (buff.Desc.Length != 0)
                    {
                        return string.Join("; ", buff.Desc.Select(desc => desc.Text));
                    }
                    return buff.Name;
                }
                return "Unknown";
            }

            object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
        }
    }
}
