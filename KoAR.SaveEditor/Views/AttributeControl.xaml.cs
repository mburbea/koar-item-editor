using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using KoAR.Core;

namespace KoAR.SaveEditor.Views
{
    partial class AttributeControl
    {
        public static readonly DependencyProperty AttributesProperty = DependencyProperty.Register(nameof(AttributeControl.Attributes), typeof(IReadOnlyList<EffectInfo>), typeof(AttributeControl));

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(nameof(AttributeControl.Command), typeof(ICommand), typeof(AttributeControl));

        public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.Register(nameof(AttributeControl.CommandTarget), typeof(IInputElement), typeof(AttributeControl));
        
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(nameof(AttributeControl.Header), typeof(object), typeof(AttributeControl));
        
        public static readonly DependencyProperty ProcessItemLabelProperty = DependencyProperty.Register(nameof(AttributeControl.ProcessItemLabel), typeof(object), typeof(AttributeControl));

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(nameof(AttributeControl.SelectedItem), typeof(EffectInfo), typeof(AttributeControl),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public AttributeControl() => this.InitializeComponent();

        public IReadOnlyList<EffectInfo>? Attributes
        {
            get => this.GetValue(AttributeControl.AttributesProperty) as IReadOnlyList<EffectInfo>;
            set => this.SetValue(AttributeControl.AttributesProperty, value);
        }

        public ICommand? Command
        {
            get => this.GetValue(AttributeControl.CommandProperty) as ICommand;
            set => this.SetValue(AttributeControl.CommandProperty, value);
        }

        public IInputElement? CommandTarget
        {
            get => this.GetValue(AttributeControl.CommandTargetProperty) as IInputElement;
            set => this.SetValue(AttributeControl.CommandTargetProperty, value);
        }

        public object? Header
        {
            get => this.GetValue(AttributeControl.HeaderProperty);
            set => this.SetValue(AttributeControl.HeaderProperty, value);
        }

        public object? ProcessItemLabel
        {
            get => this.GetValue(AttributeControl.ProcessItemLabelProperty);
            set => this.SetValue(AttributeControl.ProcessItemLabelProperty, value);
        }

        public EffectInfo? SelectedItem
        {
            get => this.GetValue(AttributeControl.SelectedItemProperty) as EffectInfo;
            set => this.SetValue(AttributeControl.SelectedItemProperty, value);
        }
    }
}
