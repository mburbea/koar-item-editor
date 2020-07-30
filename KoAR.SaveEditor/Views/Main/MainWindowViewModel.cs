using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;
using KoAR.SaveEditor.Updates;
using KoAR.SaveEditor.Views.InventoryManager;
using KoAR.SaveEditor.Views.StashManager;
using Microsoft.Win32;
using TaskDialogInterop;

namespace KoAR.SaveEditor.Views.Main
{
    public sealed class MainWindowViewModel : NotifierBase
    {
        private bool _hasUnsavedChanges;
        private InventoryManagerViewModel? _inventoryManager;
        private ManagementMode _mode;
        private StashManagerViewModel? _stashManager;

        public MainWindowViewModel()
        {
            this.UpdateService = (UpdateService?)Application.Current.TryFindResource(typeof(UpdateService));
            if (!(bool)DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(Window)).DefaultValue)
            {
                Application.Current.Activated += this.Application_Activated;
            }
        }

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

        public ManagementMode Mode
        {
            get => this._mode;
            set => this.SetValue(ref this._mode, value);
        }

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

        public UpdateService? UpdateService { get; }

        public void OpenFile()
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "Open Save File...",
                DefaultExt = ".sav",
                Filter = "Save Files (*.sav)|*.sav",
                CheckFileExists = true,
            };
            if (dialog.ShowDialog(Application.Current.MainWindow) != true || this.CancelDueToUnsavedChanges(
                $"Ignore.\nLoad \"{dialog.FileName}\" without saving the current file.",
                $"Save before loading.\nCurrent file will be saved and then \"{dialog.FileName}\" will be loaded.",
                "Proceed with the current file."
            ))
            {
                return;
            }
            this.GameSave = new GameSave(dialog.FileName);
            this.InventoryManager = new InventoryManagerViewModel(this);
            this.StashManager = this.GameSave.Stash != null ? new StashManagerViewModel(this) : default;
            this.OnPropertyChanged(nameof(this.GameSave)); // Notifying the change is explicitly done after the view models are set.
            this.HasUnsavedChanges = false;
            this.Mode = ManagementMode.Inventory;
        }

        public void RegisterUnsavedChange()
        {
            this.HasUnsavedChanges = true;
        }

        public void SaveFile()
        {
            if (this.GameSave == null)
            {
                return;
            }
            this.GameSave.SaveFile();
            this.HasUnsavedChanges = false;
            MessageBox.Show(Application.Current.MainWindow, $"Save successful! Original save backed up as {this.GameSave.FileName}.bak.", "KoAR Save Editor", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Application_Activated(object sender, EventArgs e)
        {
            Application application = (Application)sender;
            application.Activated -= this.Application_Activated;
            application.MainWindow.Closing += this.MainWindow_Closing;
            application.Dispatcher.InvokeAsync(this.OpenFile);
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

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = this.CancelDueToUnsavedChanges(
                "Quit without saving.",
                "Save before closing.\nFile will be saved and then the application will close.",
                "Application will not close."
            );
        }
    }
}
