using KoAR.SaveEditor.Properties;
using Microsoft.Windows.Themes;
using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using TaskDialogInterop;

namespace KoAR.SaveEditor
{
    partial class App
    {
        static App() => Type.GetTypeCode(typeof(PlatformCulture)); // Needed to enforce loading of PresentationFramework.Aero.dll before initializing App.

        public static Version Version { get; } = new Version(Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion);

        public static void ShowExceptionDialog(string mainInstruction, Exception exception)
        {
            string content = $"{exception.GetType().FullName}: {exception.Message}";
            TaskDialogResult dialogResult = TaskDialog.Show(new TaskDialogOptions
            {
                Title = "KoAR Save Editor",
                MainInstruction = mainInstruction,
                Content = content,
                ExpandedInfo = exception.StackTrace,
                VerificationText = "Open GitHub bug report? (requires free account)",
                MainIcon = VistaTaskDialogIcon.Error,
            });
            if (dialogResult.VerificationChecked == true)
            {
                string title = $"{content} (in v{App.Version})";
                Process.Start($"https://github.com/mburbea/koar-item-editor/issues/new?labels=bug&template=bug_report.md&title={WebUtility.UrlEncode(title)}");
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            this.DispatcherUnhandledException -= this.App_DispatcherUnhandledException;
            base.OnExit(e);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            this.DispatcherUnhandledException += this.App_DispatcherUnhandledException;
            if (!Settings.Default.Upgraded)
            {
                Settings.Default.Upgrade();
                Settings.Default.Upgraded = true;
                Settings.Default.Save();
            }
            base.OnStartup(e);
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            App.ShowExceptionDialog("Unhandled Error Encountered", e.Exception);
            Environment.Exit(0);
        }
    }
}
