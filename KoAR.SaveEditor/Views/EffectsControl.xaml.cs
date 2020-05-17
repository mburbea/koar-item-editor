using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using KoAR.Core;

namespace KoAR.SaveEditor.Views
{
    partial class EffectsControl
    {
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(nameof(EffectsControl.Command), typeof(ICommand), typeof(EffectsControl));

        public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.Register(nameof(EffectsControl.CommandTarget), typeof(IInputElement), typeof(EffectsControl));
        
        public static readonly DependencyProperty EffectsProperty = DependencyProperty.Register(nameof(EffectsControl.Effects), typeof(IReadOnlyList<EffectInfo>), typeof(EffectsControl));
        
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(nameof(EffectsControl.Header), typeof(object), typeof(EffectsControl));

        public static readonly DependencyProperty ProcessEffectLabelProperty = DependencyProperty.Register(nameof(EffectsControl.ProcessEffectLabel), typeof(object), typeof(EffectsControl));

        public static readonly DependencyProperty SelectedEffectProperty = DependencyProperty.Register(nameof(EffectsControl.SelectedEffect), typeof(EffectInfo), typeof(EffectsControl),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public EffectsControl() => this.InitializeComponent();

        public ICommand? Command
        {
            get => this.GetValue(EffectsControl.CommandProperty) as ICommand;
            set => this.SetValue(EffectsControl.CommandProperty, value);
        }

        public IInputElement? CommandTarget
        {
            get => this.GetValue(EffectsControl.CommandTargetProperty) as IInputElement;
            set => this.SetValue(EffectsControl.CommandTargetProperty, value);
        }

        public IReadOnlyList<EffectInfo>? Effects
        {
            get => this.GetValue(EffectsControl.EffectsProperty) as IReadOnlyList<EffectInfo>;
            set => this.SetValue(EffectsControl.EffectsProperty, value);
        }

        public object? Header
        {
            get => this.GetValue(EffectsControl.HeaderProperty);
            set => this.SetValue(EffectsControl.HeaderProperty, value);
        }

        public object? ProcessEffectLabel
        {
            get => this.GetValue(EffectsControl.ProcessEffectLabelProperty);
            set => this.SetValue(EffectsControl.ProcessEffectLabelProperty, value);
        }

        public EffectInfo? SelectedEffect
        {
            get => this.GetValue(EffectsControl.SelectedEffectProperty) as EffectInfo;
            set => this.SetValue(EffectsControl.SelectedEffectProperty, value);
        }
    }
}
