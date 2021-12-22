using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace KoAR.SaveEditor.Views.Updates;

partial class UpdateControl
{
    private Window? _window;

    public UpdateControl()
    {
        this.InitializeComponent();
        this.Loaded += this.UpdateControl_Loaded;
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        if (e.Property == FrameworkElement.DataContextProperty && e.NewValue is UpdateViewModelBase viewModel)
        {
            PropertyChangedEventManager.AddHandler(viewModel, this.ViewModel_DialogResultChanged, nameof(viewModel.DialogResult));
        }
        base.OnPropertyChanged(e);
    }

    private void RichTextBox_Loaded(object sender, RoutedEventArgs e) => ((RichTextBox)sender).Document = ((UpdateViewModelBase)this.DataContext).Document;

    private void UpdateControl_Loaded(object? sender, RoutedEventArgs e)
    {
        this.Loaded -= this.UpdateControl_Loaded;
        (this._window = Window.GetWindow(this)).Closing += this.Window_Closing;
    }

    private void ViewModel_DialogResultChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (this._window == null)
        {
            return;
        }
        this._window.DialogResult = ((UpdateViewModelBase)sender!).DialogResult;
        this._window.Close();
    }

    private void Window_Closing(object? sender, CancelEventArgs e)
    {
        ((Window)sender!).Closing -= this.Window_Closing;
        UpdateViewModelBase viewModel = (UpdateViewModelBase)this.DataContext;
        PropertyChangedEventManager.RemoveHandler(viewModel, this.ViewModel_DialogResultChanged, nameof(viewModel.DialogResult));
    }
}
