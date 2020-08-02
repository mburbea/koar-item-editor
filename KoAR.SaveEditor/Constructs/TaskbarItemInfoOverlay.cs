using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shell;

namespace KoAR.SaveEditor.Constructs
{
    public static class TaskbarItemInfoOverlay
    {
        public static readonly DependencyProperty ContentProperty = DependencyProperty.RegisterAttached("Content", typeof(object), typeof(TaskbarItemInfoOverlay),
            new PropertyMetadata(TaskbarItemInfoOverlay.OnPropertyChanged));

        public static readonly DependencyProperty ContentTemplateProperty = DependencyProperty.RegisterAttached("ContentTemplate", typeof(DataTemplate), typeof(TaskbarItemInfoOverlay),
            new PropertyMetadata(TaskbarItemInfoOverlay.OnPropertyChanged));

        public static readonly DependencyProperty ContentTemplateSelectorProperty = DependencyProperty.RegisterAttached("ContentTemplateSelector", typeof(DataTemplateSelector), typeof(TaskbarItemInfoOverlay),
            new PropertyMetadata(TaskbarItemInfoOverlay.OnPropertyChanged));

        public static object? GetContent(Window window)
        {
            return window?.GetValue(TaskbarItemInfoOverlay.ContentProperty);
        }

        public static DataTemplate? GetContentTemplate(Window window)
        {
            return (DataTemplate?)window?.GetValue(TaskbarItemInfoOverlay.ContentTemplateProperty);
        }

        public static DataTemplateSelector? GetContentTemplateSelector(Window window)
        {
            return (DataTemplateSelector?)window?.GetValue(TaskbarItemInfoOverlay.ContentTemplateSelectorProperty);
        }

        public static void SetContent(Window window, object? value)
        {
            window?.SetValue(TaskbarItemInfoOverlay.ContentProperty, value);
        }

        public static void SetContentTemplate(Window window, DataTemplate? value)
        {
            window?.SetValue(TaskbarItemInfoOverlay.ContentTemplateProperty, value);
        }

        public static void SetContentTemplateSelector(Window window, DataTemplateSelector? value)
        {
            window?.SetValue(TaskbarItemInfoOverlay.ContentTemplateSelectorProperty, value);
        }

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is Window window))
            {
                return;
            }
            TaskbarItemInfo info = window.TaskbarItemInfo ??= new TaskbarItemInfo();
            object? content = TaskbarItemInfoOverlay.GetContent(window);
            if (content == null)
            {
                info.Overlay = null;
                return;
            }
            UIElement element = new ContentPresenter
            {
                Content = content,
                ContentTemplate = TaskbarItemInfoOverlay.GetContentTemplate(window),
                ContentTemplateSelector = TaskbarItemInfoOverlay.GetContentTemplateSelector(window)
            };
            element.Arrange(new Rect(0, 0, 16, 16));
            RenderTargetBitmap bitmap = new RenderTargetBitmap(16, 16, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(element);
            bitmap.Freeze();
            info.Overlay = bitmap;
        }
    }
}
