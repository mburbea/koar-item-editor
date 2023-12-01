using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using KoAR.SaveEditor.Properties;
using Microsoft.Windows.Themes;

namespace KoAR.SaveEditor;

partial class App
{
    static App() => Type.GetTypeCode(typeof(PlatformCulture)); // Needed to enforce loading of PresentationFramework.Aero.dll before initializing App.

    public static Version Version { get; } = App.ParseVersion(Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion);

    public static void CreateGithubIssue(string titlePrefix) => App.OpenInBrowser(
        $"https://github.com/mburbea/koar-item-editor/issues/new?labels=bug&template=bug_report.md&title={WebUtility.UrlEncode($"{titlePrefix} (in v{App.Version})")}"
    );

    public static void OpenInBrowser(string url) => Process.Start(startInfo: new(url) { UseShellExecute = true })?.Dispose();

    public static void ShowExceptionDialog(string mainInstruction, Exception exception)
    {
        string content = $"{exception.GetType().FullName}: {exception.Message}";
        TaskDialogPage page = new()
        {
            Caption = "KoAR Save Editor",
            Heading = mainInstruction,
            Text = content,
            Expander = new()
            {
                Text = exception.StackTrace,
                Position = TaskDialogExpanderPosition.AfterFootnote,
            },
            Verification = new("Open GitHub bug report? (Requires a free account)"),
            Icon = TaskDialogIcon.Error,
        };
        TaskDialog.ShowDialog(page);
        if (page.Verification is { Checked: true })
        {
            App.CreateGithubIssue(content);
        }
    }

    private static Version ParseVersion(string text) => new(text.IndexOf('+') is int index and not -1 ? text[..index] : text);

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

    private void App_DispatcherUnhandledException(object? sender, DispatcherUnhandledExceptionEventArgs e)
    {
        App.ShowExceptionDialog("Unhandled Error Encountered", e.Exception);
        Environment.Exit(0);
    }
}