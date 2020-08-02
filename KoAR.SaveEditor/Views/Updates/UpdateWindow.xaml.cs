using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace KoAR.SaveEditor.Views.Updates
{
    partial class UpdateWindow
    {
        public static readonly DependencyProperty DocumentProperty = DependencyProperty.RegisterAttached(nameof(RichTextBox.Document), typeof(FlowDocument), typeof(UpdateWindow),
            new PropertyMetadata(DocumentProperty_ValueChanged));

        public UpdateWindow() => this.InitializeComponent();

        public static FlowDocument? GetDocument(RichTextBox textBox) => (FlowDocument?)textBox?.GetValue(UpdateWindow.DocumentProperty);

        public static void SetDocument(RichTextBox textBox, FlowDocument? document) => textBox?.SetValue(UpdateWindow.DocumentProperty, document);

        protected override void OnClosing(CancelEventArgs e)
        {
            using UpdateViewModel viewModel = (UpdateViewModel)this.DataContext;
            PropertyChangedEventManager.RemoveHandler(viewModel, this.ViewModel_DialogResultChanged, nameof(viewModel.DialogResult));
            base.OnClosing(e);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == FrameworkElement.DataContextProperty && e.NewValue is UpdateViewModel viewModel)
            {
                PropertyChangedEventManager.AddHandler(viewModel, this.ViewModel_DialogResultChanged, nameof(viewModel.DialogResult));
            }
            base.OnPropertyChanged(e);
        }

        private static void DocumentProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((RichTextBox)d).Document = (FlowDocument?)e.NewValue;

        private void ViewModel_DialogResultChanged(object sender, PropertyChangedEventArgs e)
        {
            this.DialogResult = ((UpdateViewModel)sender).DialogResult;
            this.Close();
        }
    }
}
