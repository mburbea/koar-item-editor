﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;
using KoAR.SaveEditor.Properties;
using KoAR.SaveEditor.Views.Inventory;
using KoAR.SaveEditor.Views.Stash;
using KoAR.SaveEditor.Views.Updates;
using Application = System.Windows.Application;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace KoAR.SaveEditor.Views.Main;

public sealed class MainWindowViewModel : NotifierBase
{
    private bool _hasUnsavedChanges;
    private InventoryManagerViewModel? _inventoryManager;
    private bool _isCheckingForUpdate;
    private Mode _mode;
    private StashManagerViewModel? _stashManager;

    public MainWindowViewModel()
    {
        this.CheckForUpdateCommand = new(this.CheckForUpdate, () => !this._isCheckingForUpdate);
        this.OpenUpdateWindowCommand = new(() => this.OpenUpdateWindow());
        if (!(bool)DesignerProperties.IsInDesignModeProperty.GetMetadata(dependencyObject: new()).DefaultValue)
        {
            Application.Current.Activated += this.Application_Activated;
        }
    }

    public DelegateCommand CheckForUpdateCommand { get; }

    public GameSave? GameSave
    {
        get;
        private set;
    }

    public bool HasUnsavedChanges
    {
        get => this._hasUnsavedChanges;
        private set => this.SetValue(ref this._hasUnsavedChanges, value);
    }

    public InventoryManagerViewModel? InventoryManager
    {
        get => this._inventoryManager;
        private set
        {
            using (Interlocked.Exchange(ref this._inventoryManager, value))
            {
                this.OnPropertyChanged();
            }
        }
    }

    public bool IsCheckingForUpdate
    {
        get => this._isCheckingForUpdate;
        private set => this.SetValue(ref this._isCheckingForUpdate, value);
    }

    public Mode Mode
    {
        get => this._mode;
        set => this.SetValue(ref this._mode, value);
    }

    public DelegateCommand OpenUpdateWindowCommand { get; }

    public StashManagerViewModel? StashManager
    {
        get => this._stashManager;
        private set
        {
            using (Interlocked.Exchange(ref this._stashManager, value))
            {
                this.OnPropertyChanged();
            }
        }
    }

    public UpdateNotifier UpdateNotifier { get; } = new();

    private static IntPtr MainWindowHandle => ((MainWindow)Application.Current.MainWindow).Handle;

    public void OpenFile(string? fileName = default)
    {
        OpenFileDialog dialog = new()
        {
            Title = "Open Save File...",
            DefaultExt = ".sav",
            Filter = "Save Files (*.sav)|*.sav|PS4 Save Files (*.bin)|*.bin|Switch Save Files|*.*",
            FilterIndex = Settings.Default.LastFilterUsed,
            CheckFileExists = true,
            FileName = fileName ?? string.Empty,
            InitialDirectory = Path.GetFullPath(string.IsNullOrEmpty(Settings.Default.LastDirectory)
                ? Amalur.FindSaveGameDirectory()
                : Settings.Default.LastDirectory)
        };
        bool? result;
        try
        {
            result = dialog.ShowDialog(Application.Current.MainWindow);
        }
        catch
        {
            dialog.InitialDirectory = Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.Personal));
            result = dialog.ShowDialog(Application.Current.MainWindow);
        }
        if (result != true || this.CancelDueToUnsavedChanges(
            new("Ignore.", $"Load \"{dialog.FileName}\" without saving the current file."),
            new("Save before loading.", $"Current file will be saved and then \"{dialog.FileName}\" will be loaded."),
            new("Cancel.", "Proceed with the current file.")
        ))
        {
            return;
        }
        GameSave gameSave;
        try
        {
            gameSave = new(dialog.FileName);
        }
        catch (NotSupportedException e)
        {
            TaskDialog.ShowDialog(new()
            {
                Caption = "KoAR Save Editor",
                Heading = "File Not Supported",
                Text = e.Message,
                Icon = TaskDialogIcon.Error
            });
            return;
        }
        catch (Exception e)
        {
            App.ShowExceptionDialog("Error Loading File", e);
            return;
        }
        this.GameSave = gameSave;
        Settings.Default.LastDirectory = Path.GetFullPath(Path.GetDirectoryName(dialog.FileName)!);
        Settings.Default.LastFilterUsed = dialog.FilterIndex;
        this.InventoryManager = new(this);
        this.StashManager = this.GameSave.Stash != null ? new(this) : default;
        this.OnPropertyChanged(nameof(this.GameSave)); // Notifying the change is explicitly done after the view models are set.
        this.HasUnsavedChanges = false;
        this.Mode = Mode.Inventory;
    }

    public void RegisterUnsavedChange() => this.HasUnsavedChanges = true;

    public void SaveFile()
    {
        if (this.GameSave == null)
        {
            return;
        }
        string backupPath = this.GameSave.SaveFile();
        this.HasUnsavedChanges = false;
        TaskDialog.ShowDialog(MainWindowViewModel.MainWindowHandle, new()
        {
            Caption = "KoAR Save Editor",
            Heading = "Save Successful!",
            Text = $"Original save backed up as {backupPath}.",
            Buttons = { TaskDialogButton.OK },
            DefaultButton = TaskDialogButton.OK,
            AllowCancel = true,
            Icon = TaskDialogIcon.Information,
        });
    }

    private async void Application_Activated(object? sender, EventArgs e)
    {
        Application application = (Application)sender!;
        application.Activated -= this.Application_Activated;
        application.MainWindow.Closing += this.MainWindow_Closing;
        using (CancellationTokenSource source = new(2500))
        {
            await this.UpdateNotifier.CheckForUpdatesAsync(source.Token);
        }
        if (Debugger.IsAttached || !this.UpdateNotifier.HasUpdate || !this.OpenUpdateWindow())
        {
            this.OpenFile();
        }
    }

    private bool CancelDueToUnsavedChanges(TaskDialogCommandLinkButton proceedButton, TaskDialogCommandLinkButton saveProceedButton, TaskDialogCommandLinkButton cancelButton)
    {
        if (!this.HasUnsavedChanges)
        {
            return false;
        }
        TaskDialogButton button = TaskDialog.ShowDialog(MainWindowViewModel.MainWindowHandle, new()
        {
            Heading = "Unsaved Changes Detected!",
            Text = "Changed were made to the equipment that have not been saved.",
            Buttons =
            {
                proceedButton,
                saveProceedButton,
                cancelButton
            },
            Caption = "KoAR Save Editor",
            Icon = TaskDialogIcon.Warning,
            AllowCancel = true,
            Footnote = new(" ") // Dialog looks a bit weird without a footer.
        });
        if (button == proceedButton)
        {
            return false;
        }
        if (button == saveProceedButton)
        {
            this.SaveFile();
            return false;
        }
        return true;
    }

    private async void CheckForUpdate()
    {
        this.IsCheckingForUpdate = true;
        using (CancellationTokenSource source = new(15000))
        {
            await this.UpdateNotifier.CheckForUpdatesAsync(source.Token);
        }
        this.IsCheckingForUpdate = false;
        switch (this.UpdateNotifier.UpdateReleases)
        {
            case { Count: > 0 }:
                this.OpenUpdateWindow();
                break;
            case { Count: 0 }:
                TaskDialog.ShowDialog(new()
                {
                    Caption = "KoAR Save Editor",
                    Heading = "KoAR Save Editor is up to date.",
                    Text = $"You are already running the latest version (v{App.Version}).",
                    Icon = TaskDialogIcon.Information,
                });
                break;
        }
    }

    private void MainWindow_Closing(object? sender, CancelEventArgs e)
    {
        e.Cancel = this.CancelDueToUnsavedChanges(
            new("Quit without saving."),
            new("Save before closing.", "File will be saved and then the application will close."),
            new("Cancel.", "Application will not close.")
        );
    }

    private bool OpenUpdateWindow()
    {
        if (this.UpdateNotifier.UpdateReleases is not { } releases)
        {
            return false;
        }
        using UpdateViewModel viewModel = new(releases);
        UpdateWindow window = new() { DataContext = viewModel, Owner = Application.Current.MainWindow };
        return window.ShowDialog().GetValueOrDefault();
    }
}
