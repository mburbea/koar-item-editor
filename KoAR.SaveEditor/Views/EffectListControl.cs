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

        public static readonly DependencyProperty CapacityProperty = DependencyProperty.Register(nameof(EffectListControl.Capacity), typeof(int), typeof(EffectListControl),
            new PropertyMetadata(int.MaxValue));

        public static readonly DependencyProperty DeleteEffectCommandProperty = DependencyProperty.Register(nameof(EffectListControl.DeleteEffectCommand), typeof(ICommand), typeof(EffectListControl));

        public static readonly DependencyProperty EffectDefinitionsProperty = DependencyProperty.Register(nameof(EffectListControl.EffectDefinitions), typeof(IEnumerable<IEffectInfo>), typeof(EffectListControl));

        public static readonly DependencyProperty EffectsProperty = DependencyProperty.Register(nameof(EffectListControl.Effects), typeof(IEnumerable<uint>), typeof(EffectListControl),
            new FrameworkPropertyMetadata(EffectListControl.EffectsProperty_ValueChanged));

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(nameof(EffectListControl.Header), typeof(object), typeof(EffectListControl));

        public static readonly DependencyProperty HeaderTemplateProperty = DependencyProperty.Register(nameof(EffectListControl.HeaderTemplate), typeof(DataTemplate), typeof(EffectListControl));

        public static readonly DependencyProperty PendingEffectCodeProperty = DependencyProperty.Register(nameof(EffectListControl.PendingEffectCode), typeof(int?), typeof(EffectListControl));

        public static readonly DependencyProperty PendingEffectProperty = DependencyProperty.Register(nameof(EffectListControl.PendingEffect), typeof(IEffectInfo), typeof(EffectListControl),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        static EffectListControl() => FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(EffectListControl), new FrameworkPropertyMetadata(typeof(EffectListControl)));

        public ICommand? AddEffectCommand
        {
            get => (ICommand?)this.GetValue(EffectListControl.AddEffectCommandProperty);
            set => this.SetValue(EffectListControl.AddEffectCommandProperty, value);
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

        public IEnumerable<IEffectInfo>? EffectDefinitions
        {
            get => (IEnumerable<IEffectInfo>?)this.GetValue(EffectListControl.EffectDefinitionsProperty);
            set => this.SetValue(EffectListControl.EffectDefinitionsProperty, value);
        }

        public IEnumerable<uint>? Effects
        {
            get => (IEnumerable<uint>?)this.GetValue(EffectListControl.EffectsProperty);
            set => this.SetValue(EffectListControl.EffectsProperty, value);
        }

        public object? Header
        {
            get => this.GetValue(EffectListControl.HeaderProperty);
            set => this.SetValue(EffectListControl.HeaderProperty, value);
        }

        public DataTemplate? HeaderTemplate
        {
            get => (DataTemplate?)this.GetValue(EffectListControl.HeaderTemplateProperty);
            set => this.SetValue(EffectListControl.HeaderTemplateProperty, value);
        }

        public IEffectInfo? PendingEffect
        {
            get => (IEffectInfo?)this.GetValue(EffectListControl.PendingEffectProperty);
            set => this.SetValue(EffectListControl.PendingEffectProperty, value);
        }

        public int? PendingEffectCode
        {
            get => (int?)this.GetValue(EffectListControl.PendingEffectCodeProperty);
            set => this.SetValue(EffectListControl.PendingEffectCodeProperty, value);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        private static void EffectsProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        //    ((EffectListControl)d).PendingEffect = ((IEnumerable<IEffectInfo>?)e.NewValue)?.FirstOrDefault();
        }
    }
}
