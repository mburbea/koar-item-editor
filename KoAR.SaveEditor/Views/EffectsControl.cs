using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using KoAR.Core;

namespace KoAR.SaveEditor.Views
{
    public sealed class EffectsControl : Control
    {
        public static readonly DependencyProperty AddEffectCommandProperty = DependencyProperty.Register(nameof(EffectsControl.AddEffectCommand), typeof(ICommand), typeof(EffectsControl));

        public static readonly DependencyProperty CapacityProperty = DependencyProperty.Register(nameof(EffectsControl.Capacity), typeof(int), typeof(EffectsControl),
            new PropertyMetadata(int.MaxValue));

        public static readonly DependencyProperty DeleteEffectCommandProperty = DependencyProperty.Register(nameof(EffectsControl.DeleteEffectCommand), typeof(ICommand), typeof(EffectsControl));

        public static readonly DependencyProperty EffectDefinitionsProperty = DependencyProperty.Register(nameof(EffectsControl.EffectDefinitions), typeof(IEnumerable<IEffectInfo>), typeof(EffectsControl),
            new PropertyMetadata(EffectsControl.EffectDefinitionsProperty_ValueChanged));

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

        private static void EffectDefinitionsProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((EffectsControl)d).PendingEffect = ((IEnumerable<IEffectInfo>?)e.NewValue)?.FirstOrDefault();
        }

        private static void PendingEffectProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((EffectsControl)d).PendingEffectCode = ((IEffectInfo?)e.NewValue)?.Code;
        }
    }
}
