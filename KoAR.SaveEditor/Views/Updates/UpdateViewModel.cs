using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Views.Updates
{
    public sealed class UpdateViewModel : NotifierBase, IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;
        private readonly string _tempFileName = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.zip");
        private int? _bytesTransferred;
        private bool? _dialogResult;
        private Task? _downloadTask;
        private Exception? _error;
        private double _speed;

        public UpdateViewModel()
        {
            this.CancelCommand = new DelegateCommand(this.Cancel);
            this.DownloadCommand = new DelegateCommand(this.Download, () => !this.BytesTransferred.HasValue);
        }

        public int? BytesTransferred
        {
            get => this._bytesTransferred;
            set => this.SetValue(ref this._bytesTransferred, value);
        }

        public DelegateCommand CancelCommand { get; }

        public bool? DialogResult
        {
            get => this._dialogResult;
            set => this.SetValue(ref this._dialogResult, value);
        }

        public DelegateCommand DownloadCommand { get; }

        public Exception? Error
        {
            get => this._error;
            set => this.SetValue(ref this._error, value);
        }

        public double Speed
        {
            get => this._speed;
            set => this.SetValue(ref this._speed, value);
        }

        public Release? Update => this.UpdateService.Update;

        public UpdateService UpdateService { get; } = (UpdateService)Application.Current.TryFindResource(typeof(UpdateService));

        public void Dispose()
        {
            this.UpdateService.DownloadProgress -= this.UpdateService_DownloadProgress;
            if (this.DialogResult == true)
            {
                return;
            }
            if (this._downloadTask == null)
            {
                this.DeleteFile();
                return;
            }
            this.Cancel();
            this._downloadTask.ContinueWith(delegate { this.DeleteFile(); }, TaskContinuationOptions.None);
        }

        private static async Task<string> ExtractPowershellScript()
        {
            string fileName = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.ps1");
            using FileStream fileStream = File.Create(fileName);
            await Application.GetResourceStream(new Uri("/Views/Updates/update.ps1", UriKind.Relative)).Stream.CopyToAsync(fileStream).ConfigureAwait(false);
            return fileName;
        }

        private void Cancel()
        {
            this._cancellationTokenSource.Cancel();
            this.DialogResult = false;
        }

        private void DeleteFile()
        {
            try
            {
                File.Delete(this._tempFileName);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void Download()
        {
            this.BytesTransferred = 0;
            this.UpdateService.DownloadProgress += this.UpdateService_DownloadProgress;
            this._downloadTask = this.DownloadAndUpdateAsync().ContinueWith(
                delegate { this.DialogResult = true; },
                default,
                TaskContinuationOptions.OnlyOnRanToCompletion,
                TaskScheduler.FromCurrentSynchronizationContext()
            );
        }

        private async Task DownloadAndUpdateAsync()
        {
            await this.UpdateService.DownloadUpdateAsync(this._tempFileName, this._cancellationTokenSource.Token).ConfigureAwait(false);
            string scriptFileName = await UpdateViewModel.ExtractPowershellScript().ConfigureAwait(false);
            Process.Start(new ProcessStartInfo
            {
                WorkingDirectory = Path.GetTempPath(),
                UseShellExecute = false,
                FileName = "powershell.exe",
                Arguments = $"-ExecutionPolicy Bypass -File \"{Path.GetFileName(scriptFileName)}\" {Process.GetCurrentProcess().Id} \"{Path.GetFileName(this._tempFileName)}\"",
            }).WaitForExit();
        }

        private void UpdateService_DownloadProgress(object sender, EventArgs<DownloadProgress> e)
        {
            if (this._dispatcher.CheckAccess())
            {
                ReportProgress();
            }
            else
            {
                this._dispatcher.InvokeAsync(ReportProgress);
            }

            void ReportProgress()
            {
                (this.BytesTransferred, this.Speed, this.Error) = (e.Data.BytesTransferred, e.Data.Speed, e.Data.Error);
            }
        }
    }
}
