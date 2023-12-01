﻿using System;
using System.Windows.Input;

namespace KoAR.SaveEditor.Constructs;

public sealed class DelegateCommand(Action execute, Func<bool>? canExecute = null) : ICommand
{
    private readonly Action _execute = execute ?? throw new ArgumentNullException(nameof(execute));

    event EventHandler? ICommand.CanExecuteChanged
    {
        add
        {
            if (canExecute != null)
            {
                CommandManager.RequerySuggested += value;
            }
        }
        remove
        {
            if (canExecute != null)
            {
                CommandManager.RequerySuggested -= value;
            }
        }
    }

    bool ICommand.CanExecute(object? parameter) => this.CanExecute();

    public bool CanExecute() => canExecute == null || canExecute();

    void ICommand.Execute(object? parameter) => this.Execute();

    public void Execute()
    {
        if (this.CanExecute())
        {
            this._execute();
        }
    }
}

public sealed class DelegateCommand<T>(Action<T> execute, Func<T, bool>? canExecute = null) : ICommand
{
    private readonly Action<T> _execute = execute ?? throw new ArgumentNullException(nameof(execute));

    event EventHandler? ICommand.CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    bool ICommand.CanExecute(object? parameter) => parameter is T converted && this.CanExecute(converted);

    public bool CanExecute(T parameter) => canExecute == null || canExecute(parameter);

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
