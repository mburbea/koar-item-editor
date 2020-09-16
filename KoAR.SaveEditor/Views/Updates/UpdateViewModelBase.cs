﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using KoAR.SaveEditor.Constructs;
using KoAR.SaveEditor.Updates;

namespace KoAR.SaveEditor.Views.Updates
{
    using MarkdownEngine = Markdown.Xaml.Markdown;

    public abstract class UpdateViewModelBase : NotifierBase, IDisposable
    {
        private static readonly Brush _alternateBrush = UpdateViewModelBase.CreateAlternateBrush();

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;
        private readonly string _zipFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.zip");
        private int? _bytesTransferred;
        private bool? _dialogResult;
        private Task? _downloadTask;
        private Exception? _error;
        private double _speed;

        protected UpdateViewModelBase(IReadOnlyCollection<IReleaseInfo> releases)
        {
            this.Document = UpdateViewModelBase.CreateDocument(this.Releases = releases);
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

        public FlowDocument Document { get; }

        public DelegateCommand DownloadCommand { get; }

        public Exception? Error
        {
            get => this._error;
            set => this.SetValue(ref this._error, value);
        }

        public virtual string? Preamble { get; }

        public IReadOnlyCollection<IReleaseInfo> Releases { get; }

        public double Speed
        {
            get => this._speed;
            set => this.SetValue(ref this._speed, value);
        }

        public IReleaseInfo Target => this.Releases.First();

        public abstract string Title { get; }

        public virtual void Dispose()
        {
            this._cancellationTokenSource.Dispose();
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

        private static Brush CreateAlternateBrush()
        {
            SolidColorBrush brush = new SolidColorBrush(Color.FromArgb(8, 0, 0, 0));
            brush.Freeze();
            return brush;
        }

        private static FlowDocument CreateDocument(IEnumerable<IReleaseInfo> releases)
        {
            FlowDocument document = new FlowDocument();
            MarkdownEngine engine = new MarkdownEngine();
            foreach (IReleaseInfo release in releases)
            {
                Section section = new Section { Background = document.Blocks.Count % 2 == 0 ? Brushes.White : UpdateViewModelBase._alternateBrush };
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
                File.Delete(this._zipFilePath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void Download()
        {
            this._downloadTask = this.DownloadAndUpdateAsync().ContinueWith(
                delegate { this.DialogResult = true; },
                default,
                TaskContinuationOptions.OnlyOnRanToCompletion,
                TaskScheduler.FromCurrentSynchronizationContext()
            );
        }

        private async Task DownloadAndUpdateAsync()
        {
            await this.DownloadUpdateAsync().ConfigureAwait(false);
            UpdateMethods.ExecuteUpdate(this._zipFilePath);
        }

        private async Task DownloadUpdateAsync()
        {
            (this.BytesTransferred, this.Speed, this.Error) = (default, default, default);
            const int interval = 250;
            int bytesTransferred = 0, bytesPerInterval = 0;
            DispatcherTimer timer = new DispatcherTimer(TimeSpan.FromMilliseconds(interval), DispatcherPriority.Normal, Timer_Tick, this._dispatcher);
            try
            {
                CancellationToken cancellationToken = this._cancellationTokenSource.Token;
                timer.Start();
                IReleaseInfo release = this.Releases.First();
                HttpWebRequest request = WebRequest.CreateHttp(release.ZipFileUri);
                using WebResponse response = await request.GetResponseAsync().ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                using Stream stream = response.GetResponseStream();
                using FileStream fileStream = File.Create(this._zipFilePath);
                byte[] buffer = new byte[8192];
                while (bytesTransferred < release.ZipFileSize)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    int count = await stream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                    cancellationToken.ThrowIfCancellationRequested();
                    await fileStream.WriteAsync(buffer, 0, count).ConfigureAwait(false);
                    bytesTransferred += count;
                    bytesPerInterval += count;
                }
                await this._dispatcher.InvokeAsync(ReportProgress);
            }
            catch (Exception e)
            {
                await this._dispatcher.InvokeAsync(() => this.Error = e);
            }
            finally
            {
                timer.Stop();
            }

            void ReportProgress() => (this.BytesTransferred, this.Speed) = (bytesTransferred, bytesPerInterval * 1000d / interval);

            void Timer_Tick(object sender, EventArgs e) => ReportProgress();
        }
    }
}
