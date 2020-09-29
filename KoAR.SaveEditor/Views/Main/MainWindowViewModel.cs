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
using TaskDialogInterop;

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
            this.CheckForUpdateCommand = new DelegateCommand(this.CheckForUpdate, () => !this._isCheckingForUpdate);
            this.OpenUpdateWindowCommand = new DelegateCommand(() => this.OpenUpdateWindow());
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

        public UpdateNotifier UpdateNotifier { get; } = new UpdateNotifier();

        public void OpenFile()
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "Open Save File...",
                DefaultExt = ".sav",
                Filter = "Save Files (*.sav)|*.sav",
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
                $"Ignore.\nLoad \"{dialog.FileName}\" without saving the current file.",
                $"Save before loading.\nCurrent file will be saved and then \"{dialog.FileName}\" will be loaded.",
                "Proceed with the current file."
            ))
            {
                return;
            }
            GameSave gameSave;
            try
            {
                gameSave = new GameSave(dialog.FileName);
            }
            catch (NotSupportedException e)
            {
                TaskDialog.Show(new TaskDialogOptions
                {
                    Title = "KoAR Save Editor",
                    MainInstruction = "File Not Supported",
                    Content = e.Message,
                    MainIcon = VistaTaskDialogIcon.Error,
                });
                return;
            }
            catch (Exception e)
            {
                App.ShowExceptionDialog("Error Loading File", e);
                return;
            }
            this.GameSave = gameSave;
            Settings.Default.LastDirectory = Path.GetFullPath(Path.GetDirectoryName(dialog.FileName));
            this.InventoryManager = new InventoryManagerViewModel(this);
            this.StashManager = this.GameSave.Stash != null ? new StashManagerViewModel(this) : default;
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
            MessageBox.Show(Application.Current.MainWindow, $"Save successful! Original save backed up as {backupPath}.", "KoAR Save Editor", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void Application_Activated(object sender, EventArgs e)
        {
            Application application = (Application)sender;
            application.Activated -= this.Application_Activated;
            application.MainWindow.Closing += this.MainWindow_Closing;
            try
            {
                using CancellationTokenSource source = new CancellationTokenSource();
                source.CancelAfter(2500);
                if (false && Settings.Default.Acknowledged3x)
                {
                    await this.UpdateNotifier.CheckForUpdatesAsync(source.Token).ConfigureAwait(false);
                }
                else
                {
                    IReleaseInfo? release = await UpdateMethods.FetchLatest2xReleaseAsync(source.Token).ConfigureAwait(false);
                    if (release != null)
                    {
                        TaskDialogResult downgradePromptResult = TaskDialog.Show(new TaskDialogOptions
                        {
                            Title = "KoAR Save Editor",
                            MainInstruction = $"Downgrade to {release.Version}?",
                            Content = $"You are currently running v{App.Version}. 3.0 releases are only tested against Re-Reckoning. If you are playing Reckoning you should consider downgrading.",
                            MainIcon = VistaTaskDialogIcon.Information,
                            VerificationText = $"Check here to downgrade to {release.Version}",
                        }) ;

                        if (downgradePromptResult.VerificationChecked == true)
                        {
                            application.Dispatcher.Invoke(new Action<IReleaseInfo>(this.OpenOriginalUpdateWindow), release);
                            return;
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            if (Debugger.IsAttached || this.UpdateNotifier.Update == null || !application.Dispatcher.Invoke(this.OpenUpdateWindow))
            {
                await application.Dispatcher.InvokeAsync(this.OpenFile);
            }
        }

        private bool CancelDueToUnsavedChanges(string proceedText, string saveProceedText, string cancelDescription)
        {
            if (!this.HasUnsavedChanges)
            {
                return false;
            }
            TaskDialogResult result = TaskDialog.Show(new TaskDialogOptions
            {
                MainInstruction = "Unsaved Changes Detected!",
                Content = "Changed were made to the equipment that have not been saved.",
                Owner = Application.Current.MainWindow,
                CommandButtons = new[]
                {
                    proceedText,
                    saveProceedText,
                    $"Cancel.\n{cancelDescription}"
                },
                DefaultButtonIndex = 0,
                Title = "KoAR Save Editor",
                MainIcon = VistaTaskDialogIcon.Warning,
                AllowDialogCancellation = true,
                FooterText = " " // Dialog looks a bit weird without a footer.
            });
            switch (result.CommandButtonResult)
            {
                case 0: // Proceed.
                    return false;
                case 1: // Save & Proceed.
                    this.SaveFile();
                    return false;
            }
            return true; // Cancel.
        }

        private async void CheckForUpdate()
        {
            try
            {
                this.IsCheckingForUpdate = true;
                using CancellationTokenSource source = new CancellationTokenSource();
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
                "Quit without saving.",
                "Save before closing.\nFile will be saved and then the application will close.",
                "Application will not close."
            );
        }

        private void OpenOriginalUpdateWindow(IReleaseInfo release)
        {
            Settings.Default.Acknowledged3x = true;
            Settings.Default.Save();
            using OriginalUpdateViewModel viewModel = new OriginalUpdateViewModel(release);
            UpdateWindow window = new UpdateWindow { DataContext = viewModel, Owner = Application.Current.MainWindow };
            window.ShowDialog();
        }

        private bool OpenUpdateWindow()
        {
            if (this.UpdateNotifier.Update == null)
            {
                return false;
            }
            using UpdateViewModel viewModel = new UpdateViewModel(this.UpdateNotifier.Update);
            UpdateWindow window = new UpdateWindow { DataContext = viewModel, Owner = Application.Current.MainWindow };
            return window.ShowDialog().GetValueOrDefault();
        }
    }
}
