using System.Windows;
using System.Windows.Input;

namespace KoAR.SaveEditor.Constructs
{
    public static class CommandMethods
    {
        public static bool TryExecute(this ICommand command, object? parameter = null, IInputElement? target = null)
        {
            if (command is RoutedCommand routedCommand)
            {
                return routedCommand.TryExecute(parameter, target);
            }
            if (command.CanExecute(parameter))
            {
                command.Execute(parameter);
                return true;
            }
            return false;
        }

        private static bool TryExecute(this RoutedCommand routedCommand, object? parameter, IInputElement? target)
        {
            if (routedCommand.CanExecute(parameter, target))
            {
                routedCommand.Execute(parameter, target);
                return true;
            }
            return false;
        }
    }
}
