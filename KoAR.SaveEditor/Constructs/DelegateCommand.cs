using System;
using System.Windows.Input;

namespace KoAR.SaveEditor.Constructs;

public sealed class DelegateCommand : ICommand
{
    private readonly Func<bool>? _canExecute;
    private readonly Action _execute;

    public DelegateCommand(Action execute, Func<bool>? canExecute = null)
    {
        this._execute = execute ?? throw new ArgumentNullException(nameof(execute));
        this._canExecute = canExecute;
    }

    event EventHandler? ICommand.CanExecuteChanged
    {
        add
        {
            if (this._canExecute != null)
            {
                CommandManager.RequerySuggested += value;
            }
        }
        remove
        {
            if (this._canExecute != null)
            {
                CommandManager.RequerySuggested -= value;
            }
        }
    }

    bool ICommand.CanExecute(object? parameter) => this.CanExecute();

    public bool CanExecute() => this._canExecute == null || this._canExecute();

    void ICommand.Execute(object? parameter) => this.Execute();

    public void Execute()
    {
        if (this.CanExecute())
        {
            this._execute();
        }
    }
}

public sealed class DelegateCommand<T> : ICommand
{
    private readonly Func<T, bool>? _canExecute;
    private readonly Action<T> _execute;

    public DelegateCommand(Action<T> execute, Func<T, bool>? canExecute = null)
    {
        this._execute = execute ?? throw new ArgumentNullException(nameof(execute));
        this._canExecute = canExecute;
    }

    event EventHandler? ICommand.CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    bool ICommand.CanExecute(object? parameter) => parameter is T converted && this.CanExecute(converted);

    public bool CanExecute(T parameter) => this._canExecute == null || this._canExecute(parameter);

    void ICommand.Execute(object? parameter)
    {
        if (parameter is T converted)
        {
            this.Execute(converted);
        }
    }

    public void Execute(T parameter)
    {
        if (this.CanExecute(parameter))
        {
            this._execute(parameter);
        }
    }
}
