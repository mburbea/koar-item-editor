using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Views.Updates
{
    using MarkdownEngine = Markdown.Xaml.Markdown;

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
            this.FlowDocument = this.UpdateService?.Update != null 
                ? UpdateViewModel.CreateDocument(this.UpdateService.Update) 
                : new FlowDocument();
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

        public FlowDocument FlowDocument { get; }

        public double Speed
        {
            get => this._speed;
            set => this.SetValue(ref this._speed, value);
        }

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

        private static FlowDocument CreateDocument(UpdateInfo update)
        {
            MarkdownEngine engine = new MarkdownEngine();
            SolidColorBrush alternateBrush = new SolidColorBrush(Color.FromArgb(8, 0, 0, 0));
            alternateBrush.Freeze();
            FlowDocument document = new FlowDocument();
            foreach (IReleaseInfo release in update.Releases)
            {
                Section section = new Section { Background = document.Blocks.Count % 2 == 0 ? Brushes.White : alternateBrush };
                section.Blocks.Add(new Paragraph
                {
                    Inlines =
                    {
                        new Run($"{release.Name}{Environment.NewLine}Released:") { FontWeight = FontWeights.SemiBold },
                        new Run($" {GetDescriptiveText(DateTime.UtcNow - release.PublishedAt)} ago."),
                    }
                });
                section.Blocks.AddRange(engine.Transform(release.Body).Blocks.ToList());
                document.Blocks.Add(section);
            }
            return document;

            static string GetDescriptiveText(TimeSpan timeSpan)
            {
                if (timeSpan.TotalDays >= 365d)
                {
                    int years = (int)Math.Floor(timeSpan.TotalDays / 365d);
                    return years > 1 ? $"{years} years" : "A year";
                }
                if (timeSpan.TotalDays >= 30d)
                {
                    int months = (int)Math.Floor(timeSpan.TotalDays / 30d);
                    return months > 1 ? $"{months} months" : "A month";
                }
                if (timeSpan.TotalDays >= 1d)
                {
                    int days = (int)Math.Floor(timeSpan.TotalDays);
                    return days > 1 ? $"{days} days" : "A day";
                }
                if (timeSpan.TotalHours >= 1d)
                {
                    int hours = (int)Math.Floor(timeSpan.TotalHours);
                    return hours > 1 ? $"{hours} hours" : "An hour";
                }
                if (timeSpan.TotalMinutes >= 1d)
                {
                    int minutes = (int)Math.Floor(timeSpan.TotalMinutes);
                    return minutes > 1 ? $"{minutes} minutes" : "A minute";
                }
                return "Seconds";
            }
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
            string scriptFileName = await this.UpdateService.ExtractPowershellScript().ConfigureAwait(false);
            this.UpdateService.ExecuteUpdate(scriptFileName, this._tempFileName);
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
