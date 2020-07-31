using System.IO;
using System.Net;
using System.Windows;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Views.Updates
{
    public sealed class UpdateViewModel : NotifierBase
    {
        private readonly string _tempFileName = Path.GetTempFileName();
        private int _downloaded;

        public UpdateViewModel()
        {
            this.DownloadCommand = new DelegateCommand<UpdateInfo>(this.Download);
        }

        public DelegateCommand<UpdateInfo> DownloadCommand { get; }

        public int Downloaded
        {
            get => this._downloaded;
            set => this.SetValue(ref this._downloaded, value);
        }

        public UpdateService UpdateService { get; } = (UpdateService)Application.Current.TryFindResource(typeof(UpdateService));

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
        }

        private async void Download(UpdateInfo info)
        {
            using WebClient client = new WebClient();
            try
            {
                client.DownloadProgressChanged += this.Client_DownloadProgressChanged;
                await client.DownloadFileTaskAsync(info.ZipFileUrl, this._tempFileName).ConfigureAwait(false);
            }
            finally
            {
                client.DownloadProgressChanged -= this.Client_DownloadProgressChanged;
            }
        }
    }
}
