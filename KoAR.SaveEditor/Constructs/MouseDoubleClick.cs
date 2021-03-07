using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KoAR.SaveEditor.Constructs
{
    public static class MouseDoubleClick
    {
        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.RegisterAttached("CommandParameter", typeof(object), typeof(MouseDoubleClick),
            new());

        public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(MouseDoubleClick),
            new(MouseDoubleClick.OnCommandPropertyChanged));

        public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.RegisterAttached("CommandTarget", typeof(IInputElement), typeof(MouseDoubleClick),
            new());

        public static ICommand? GetCommand(Control control)
        {
            return (ICommand?)(control ?? throw new ArgumentNullException(nameof(control))).GetValue(MouseDoubleClick.CommandProperty);
        }

        public static object? GetCommandParameter(Control control)
        {
            return (control ?? throw new ArgumentNullException(nameof(control))).GetValue(MouseDoubleClick.CommandParameterProperty);
        }

        public static IInputElement? GetCommandTarget(Control control)
        {
            return (IInputElement?)(control ?? throw new ArgumentNullException(nameof(control))).GetValue(MouseDoubleClick.CommandTargetProperty);
        }

        public static void SetCommand(Control control, ICommand? value)
        {
            (control ?? throw new ArgumentNullException(nameof(control))).SetValue(MouseDoubleClick.CommandProperty, value);
        }

        public static void SetCommandParameter(Control control, object? value)
        {
            (control ?? throw new ArgumentNullException(nameof(control))).SetValue(MouseDoubleClick.CommandParameterProperty, value);
        }

        public static void SetCommandTarget(Control control, IInputElement? value)
        {
            (control ?? throw new ArgumentNullException(nameof(control))).SetValue(MouseDoubleClick.CommandTargetProperty, value);
        }

        private static void Control_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Control control = (Control)sender;
            ICommand? command = MouseDoubleClick.GetCommand(control);
            command?.TryExecute(MouseDoubleClick.GetCommandParameter(control), MouseDoubleClick.GetCommandTarget(control) ?? control);
        }

        private static void OnCommandPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Control control || (e.OldValue == null) == (e.NewValue == null))
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
}
