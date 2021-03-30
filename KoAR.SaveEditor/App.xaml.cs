using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using KoAR.SaveEditor.Properties;
using Microsoft.Windows.Themes;
using KPreisser.UI;

namespace KoAR.SaveEditor
{
    partial class App
    {
        static App() => Type.GetTypeCode(typeof(PlatformCulture)); // Needed to enforce loading of PresentationFramework.Aero.dll before initializing App.

        public static Version Version { get; } = new(Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion);

        public static void ShowExceptionDialog(string mainInstruction, Exception exception)
        {
            string content = $"{exception.GetType().FullName}: {exception.Message}";
            TaskDialog dialog = new(new()
            {
                Title = "KoAR Save Editor",
                Instruction = mainInstruction,
                Text = content,
                Expander = {
                    Text = exception.StackTrace,
                    ExpandFooterArea = true,
                },
                CheckBox = new("Open GitHub bug report? (requires free account)"),
                Icon = TaskDialogStandardIcon.Error,
            });
            dialog.Show();
            if (dialog.Page.CheckBox.Checked)
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
