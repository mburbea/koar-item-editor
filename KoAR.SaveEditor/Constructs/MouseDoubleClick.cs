using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KoAR.SaveEditor.Constructs;

public static class MouseDoubleClick
{
    public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.RegisterAttached("CommandParameter", typeof(object), typeof(MouseDoubleClick),
        new());

    public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(MouseDoubleClick),
        new(MouseDoubleClick.OnCommandPropertyChanged));

    public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.RegisterAttached("CommandTarget", typeof(IInputElement), typeof(MouseDoubleClick),
        new());

    public static ICommand? GetCommand(Control control) => (ICommand?)control.GetValue(MouseDoubleClick.CommandProperty);

    public static object? GetCommandParameter(Control control) => control.GetValue(MouseDoubleClick.CommandParameterProperty);

    public static IInputElement? GetCommandTarget(Control control) => (IInputElement?)control.GetValue(MouseDoubleClick.CommandTargetProperty);

    public static void SetCommand(Control control, ICommand? value) => control.SetValue(MouseDoubleClick.CommandProperty, value);

    public static void SetCommandParameter(Control control, object? value) => control.SetValue(MouseDoubleClick.CommandParameterProperty, value);

    public static void SetCommandTarget(Control control, IInputElement? value) => control.SetValue(MouseDoubleClick.CommandTargetProperty, value);

    private static void Control_MouseDoubleClick(object? sender, MouseButtonEventArgs e)
    {
        Control control = (Control)sender!;
        if (MouseDoubleClick.GetCommand(control) is not { } command)
        {
            return;
        }
        object? parameter = MouseDoubleClick.GetCommandParameter(control);
        if (command is RoutedCommand routedCommand)
        {
            IInputElement target = MouseDoubleClick.GetCommandTarget(control) ?? control;
            if (routedCommand.CanExecute(parameter, target))
            {
                routedCommand.Execute(parameter, target);
            }
        }
        else if (command.CanExecute(parameter))
        {
            command.Execute(parameter);
        }
    }

    private static void OnCommandPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Control control || e.OldValue is null == e.NewValue is null)
        {
            return;
        }
        if (e.OldValue != null)
        {
            WeakEventManager<Control, MouseButtonEventArgs>.RemoveHandler(control, nameof(Control.MouseDoubleClick), MouseDoubleClick.Control_MouseDoubleClick);
        }
        if (e.NewValue != null)
        {
            WeakEventManager<Control, MouseButtonEventArgs>.AddHandler(control, nameof(Control.MouseDoubleClick), MouseDoubleClick.Control_MouseDoubleClick);
        }
    }
}
