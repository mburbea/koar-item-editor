using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;
using KoAR.SaveEditor.Properties;
using KoAR.SaveEditor.Updates;
using KoAR.SaveEditor.Views.Inventory;
using KoAR.SaveEditor.Views.Stash;
using KoAR.SaveEditor.Views.Updates;
using Microsoft.Win32;
using System.Windows.Interop;
using KPreisser.UI;
using TaskDialog2 = System.Windows.Forms.TaskDialog;
using TaskDialog2Icon = System.Windows.Forms.TaskDialogIcon;

namespace KoAR.SaveEditor.Views.Main
{
    public sealed class MainWindowViewModel : NotifierBase
    {
        private readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;
        private bool _hasUnsavedChanges;
        private InventoryManagerViewModel? _inventoryManager;
        private bool _isCheckingForUpdate;
        private Mode _mode;
        private StashManagerViewModel? _stashManager;

        public MainWindowViewModel()
        {
            this.CheckForUpdateCommand = new(this.CheckForUpdate, () => !this._isCheckingForUpdate);
            this.OpenUpdateWindowCommand = new(() => this.OpenUpdateWindow());
            if (!(bool)DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(Window)).DefaultValue)
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

        public void OpenFile()
        {
            OpenFileDialog dialog = new()
            {
                Title = "Open Save File...",
                DefaultExt = ".sav",
                Filter = "Save Files (*.sav)|*.sav|Switch Save Files|*.*",
                FilterIndex = Settings.Default.LastFilterUsed,
                CheckFileExists = true,
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
                TaskDialog2.ShowDialog(new()
                {
                    Caption = "KoAR Save Editor",
                    Heading = "File Not Supported",
                    Text = e.Message,
                    Icon = TaskDialog2Icon.Error
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

        public void OpenOriginalUpdateWindow(IReleaseInfo release)
        {
            Settings.Default.Save();
            using OriginalUpdateViewModel viewModel = new(release);
            UpdateWindow window = new() { DataContext = viewModel, Owner = Application.Current.MainWindow };
            window.ShowDialog();
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
            MessageBox.Show(Application.Current.MainWindow, $"Save successful! Original save backed up as {backupPath}.", "KoAR Save Editor", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public async void ShowHelp()
        {
            TaskDialog dialog = new(new()
            {
                Title = $"KoAR Save Editor",
                Instruction = "Help",
                Icon = TaskDialogStandardIcon.Information,
                CustomButtonStyle = TaskDialogCustomButtonStyle.CommandLinks,
                CustomButtons = {
                    { "Ok.","Close this window" },
                    { "Found a bug? File a new github bug report.", "Requires a free account" },
                    { "Downgrade to v2.", "I am running Reckoning" },
                },
                SizeToContent = true,
                AllowCancel = true,
                Footer = new(" "), // Dialog looks a bit weird without a footer.
                Text = @"This version of the editor is only tested against the remaster.
If you're on the original and are running into errors consider downgrading.
1. Your saves are usually not in the same folder as the game.
The editor attemps to make educated guesses as to the save file directory.
2. When modifying item names, do NOT use special characters.
3. Editing equipped items is restricted, and even still may cause game crashes."
            });
            TaskDialogButton result = dialog.Show(new WindowInteropHelper(Application.Current.MainWindow).Handle);
            if (result == dialog.Page.CustomButtons[1])
            {
                Process.Start(new ProcessStartInfo("https://github.com/mburbea/koar-item-editor/issues/new?labels=bug&template=bug_report.md")
                {
                    UseShellExecute = true
                })?.Dispose();
            }
            else if (result == dialog.Page.CustomButtons[2])
            {
                bool dispatched = false;
                using CancellationTokenSource source = new(2500);
                try
                {
                    IReleaseInfo? release = await UpdateMethods.FetchLatest2xReleaseAsync(source.Token).ConfigureAwait(false);
                    if (release != null)
                    {
                        dispatched = true;
                        Application.Current.Dispatcher.Invoke(new Action<IReleaseInfo>(this.OpenOriginalUpdateWindow), release);
                    }
                }
                catch (OperationCanceledException)
                {
                }
                finally
                {
                    if (!dispatched)
                    {
                        // this might fail if the github is down or your internet sucks. For now let's try to open a browser window to nexusmods."
                        Process.Start(new ProcessStartInfo("https://www.nexusmods.com/kingdomsofamalurreckoning/mods/10?tab=files")
                        {
                            UseShellExecute = true
                        })?.Dispose();
                    }
                }
            }
        }

        private async void Application_Activated(object? sender, EventArgs e)
        {
            Application application = (Application)sender!;
            application.Activated -= this.Application_Activated;
            application.MainWindow.Closing += this.MainWindow_Closing;
            try
            {
                using CancellationTokenSource source = new(2500);
                await this.UpdateNotifier.CheckForUpdatesAsync(source.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
            if (Debugger.IsAttached || this.UpdateNotifier.Update == null || !application.Dispatcher.Invoke(this.OpenUpdateWindow))
            {
                await application.Dispatcher.InvokeAsync(this.OpenFile);
            }
        }

        private bool CancelDueToUnsavedChanges(TaskDialogCustomButton proceedText, TaskDialogCustomButton saveProceedText, TaskDialogCustomButton cancelDescription)
        {
            if (!this.HasUnsavedChanges)
            {
                return false;
            }
            TaskDialog dialog = new(new()
            {
                Instruction = "Unsaved Changes Detected!",
                Text = "Changed were made to the equipment that have not been saved.",
                CustomButtons =
                {
                    proceedText,
                    saveProceedText,
                    cancelDescription
                },
                Title = "KoAR Save Editor",
                Icon = TaskDialogStandardIcon.Warning,
                CustomButtonStyle = TaskDialogCustomButtonStyle.CommandLinks,
                AllowCancel = true,
                Footer = new(" ") // Dialog looks a bit weird without a footer.
            });
            TaskDialogButton result = dialog.Show(new WindowInteropHelper(Application.Current.MainWindow).Handle);
            if (result == dialog.Page.CustomButtons[0])
            {
                return false;
            }
            else if (result == dialog.Page.CustomButtons[1])
            {
                this.SaveFile();
                return false;
            }
            return true;
        }

        private async void CheckForUpdate()
        {
            try
            {
                this.IsCheckingForUpdate = true;
                using CancellationTokenSource source = new();
                source.CancelAfter(15000); // 15s
                await this.UpdateNotifier.CheckForUpdatesAsync(source.Token).ConfigureAwait(false);
            }
            catch
            {
                // Do Nothing.
            }
            finally
            {
                this.IsCheckingForUpdate = false;
                if (this.UpdateNotifier.Update != null)
                {
                    this._dispatcher.Invoke(this.OpenUpdateWindow);
                }
            }
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = this.CancelDueToUnsavedChanges(
                new("Quit without saving."),
                new("Save before closing.", "File will be saved and then the application will close."),
                new("Cancel.", "Application will not close.")
            );
        }

        private bool OpenUpdateWindow()
        {
            if (this.UpdateNotifier.Update == null)
            {
                return false;
            }
            using UpdateViewModel viewModel = new(this.UpdateNotifier.Update);
            UpdateWindow window = new() { DataContext = viewModel, Owner = Application.Current.MainWindow };
            return window.ShowDialog().GetValueOrDefault();
        }
    }
}