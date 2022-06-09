using System;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using KoAR.SaveEditor.Properties;
using KoAR.SaveEditor.Updates;
using KoAR.SaveEditor.Views.Updates;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace KoAR.SaveEditor.Views.Main;

partial class MainWindow
{
    public MainWindow() => this.InitializeComponent();

    public IntPtr Handle { get; private set; }

    private MainWindowViewModel ViewModel => (MainWindowViewModel)this.DataContext;

    protected override void OnClosed(EventArgs e)
    {
        Settings.Default.Save();
        base.OnClosed(e);
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape && this.ZoomPopup.IsOpen)
        {
            this.ZoomPopup.IsOpen = false;
            e.Handled = true;
        }
        base.OnPreviewKeyDown(e);
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        this.Handle = ((HwndSource)PresentationSource.FromVisual(this)).Handle;
        base.OnSourceInitialized(e);
    }

    private async void Help_Executed(object? sender, ExecutedRoutedEventArgs e)
    {
        TaskDialogButton button = TaskDialog.ShowDialog(this.Handle, new()
        {
            Caption = "KoAR Save Editor",
            Heading = "Help",
            Icon = TaskDialogIcon.Information,
            Buttons =
            {
                new TaskDialogCommandLinkButton("OK", "Close this window") { Tag = 0 },
                new TaskDialogCommandLinkButton("Found a bug? Open a new GitHub bug report.", "(Requires a free account)") { Tag = 1 },
                new TaskDialogCommandLinkButton("Downgrade to v2...", "I am running Reckoning") { Tag = 2 },
            },
            SizeToContent = true,
            AllowCancel = true,
            Text = @"This version of the editor has only been tested against the remaster. If you're on the original and are running into errors, consider downgrading.

1. Your saves are usually not in the same folder as the game. The editor attemps to make educated guesses as to the save file directory.
2. When modifying item names, do NOT use special characters.
3. Editing equipped items is restricted, and even still may cause game crashes.",
            Footnote = new($"v{App.Version}")
        });
        if (button is not { Tag: int tag and > 0 })
        {
            return;
        }
        if (tag == 1)
        {
            App.CreateGithubIssue("Enter title here");
            return;
        }
        using CancellationTokenSource source = new(2500);
        IReleaseInfo? release = default;
        try
        {
            if ((release = await UpdateMethods.FetchLatest2xReleaseAsync(source.Token)) != null)
            {
                using OriginalUpdateViewModel viewModel = new(release);
                UpdateWindow window = new() { DataContext = viewModel, Owner = this };
                window.ShowDialog();
            }
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            if (release is null)
            {
                // this might fail if the github is down or your internet sucks. For now let's try to open a browser window to nexusmods.
                App.OpenInBrowser("https://www.nexusmods.com/kingdomsofamalurreckoning/mods/10?tab=files");
            }
        }
    }

    private void Open_Executed(object? sender, ExecutedRoutedEventArgs e) => this.ViewModel.OpenFile();

    private void Save_CanExecute(object? sender, CanExecuteRoutedEventArgs e) => e.CanExecute = this.ViewModel.HasUnsavedChanges;

    private void Save_Executed(object? sender, ExecutedRoutedEventArgs e) => this.ViewModel.SaveFile();
}