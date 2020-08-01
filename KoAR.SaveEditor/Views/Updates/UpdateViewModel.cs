using System;
using System.IO;
using System.Net;
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
        private readonly string _tempFileName = Path.GetTempFileName();
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
            set
            {
                if (this.SetValue(ref this._bytesTransferred, value))
                {
                    this.OnPropertyChanged(nameof(this.PercentTransferred));
                }
            }
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

        public double PercentTransferred => this.BytesTransferred.HasValue ? (double)this.BytesTransferred.Value / this.Update.FileSize : 0;

        public double Speed
        {
            get => this._speed;
            set => this.SetValue(ref this._speed, value);
        }

        public UpdateInfo Update => this.UpdateService.Update.GetValueOrDefault();

        public UpdateService UpdateService { get; } = (UpdateService)Application.Current.TryFindResource(typeof(UpdateService));

        public void Dispose()
        {
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

        private void Cancel()
        {
            this._cancellationTokenSource.Cancel();
            this.DialogResult = false;
        }

        private void Download()
        {
            this.BytesTransferred = 0;
            this._downloadTask = this.DownloadTask(Dispatcher.CurrentDispatcher);
        }

        private async Task DownloadTask(Dispatcher dispatcher)
        {
            DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Normal, dispatcher) { Interval = TimeSpan.FromSeconds(0.25d) };
            int bytesTransferred = 0, bytesPerInterval = 0;
            timer.Tick += Timer_Tick;
            timer.Start();
            try
            {
                HttpWebRequest request = WebRequest.CreateHttp(this.Update.ZipFileUrl);
                using WebResponse response = await request.GetResponseAsync().ConfigureAwait(false);
                this._cancellationTokenSource.Token.ThrowIfCancellationRequested();
                using Stream stream = response.GetResponseStream();
                using FileStream fileStream = File.Create(this._tempFileName);
                byte[] buffer = new byte[8192];
                while (bytesTransferred < this.Update.FileSize)
                {
                    this._cancellationTokenSource.Token.ThrowIfCancellationRequested();
                    int count = await stream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                    await Task.Delay(200).ConfigureAwait(false);
                    this._cancellationTokenSource.Token.ThrowIfCancellationRequested();
                    await fileStream.WriteAsync(buffer, 0, count).ConfigureAwait(false);
                    bytesTransferred += count;
                    bytesPerInterval += count;
                }
                dispatcher.Invoke(() => this.BytesTransferred = bytesTransferred);
            }
            catch (Exception e)
            {
                dispatcher.Invoke(() => this.Error = e);
            }
            finally
            {
                timer.Stop();
                timer.Tick -= Timer_Tick;
            }

            void Timer_Tick(object sender, EventArgs e)
            {
                this.BytesTransferred = bytesTransferred;
                this.Speed = bytesPerInterval * 1000d / timer.Interval.TotalMilliseconds;
                bytesPerInterval = 0;
            }
        }
    }
}
