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
            new(TaskbarItemInfoOverlay.OnPropertyChanged));

        public static readonly DependencyProperty ContentTemplateProperty = DependencyProperty.RegisterAttached("ContentTemplate", typeof(DataTemplate), typeof(TaskbarItemInfoOverlay),
            new(TaskbarItemInfoOverlay.OnPropertyChanged));

        public static readonly DependencyProperty ContentTemplateSelectorProperty = DependencyProperty.RegisterAttached("ContentTemplateSelector", typeof(DataTemplateSelector), typeof(TaskbarItemInfoOverlay),
            new(TaskbarItemInfoOverlay.OnPropertyChanged));

        public static object? GetContent(TaskbarItemInfo info)
        {
            return info?.GetValue(TaskbarItemInfoOverlay.ContentProperty);
        }

        public static DataTemplate? GetContentTemplate(TaskbarItemInfo info)
        {
            return (DataTemplate?)info?.GetValue(TaskbarItemInfoOverlay.ContentTemplateProperty);
        }

        public static DataTemplateSelector? GetContentTemplateSelector(TaskbarItemInfo info)
        {
            return (DataTemplateSelector?)info?.GetValue(TaskbarItemInfoOverlay.ContentTemplateSelectorProperty);
        }

        public static void SetContent(TaskbarItemInfo info, object? value)
        {
            info?.SetValue(TaskbarItemInfoOverlay.ContentProperty, value);
        }

        public static void SetContentTemplate(TaskbarItemInfo info, DataTemplate? value)
        {
            info?.SetValue(TaskbarItemInfoOverlay.ContentTemplateProperty, value);
        }

        public static void SetContentTemplateSelector(TaskbarItemInfo info, DataTemplateSelector? value)
        {
            info?.SetValue(TaskbarItemInfoOverlay.ContentTemplateSelectorProperty, value);
        }

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TaskbarItemInfo info)
            {
                return;
            }
            object? content = TaskbarItemInfoOverlay.GetContent(info);
            if (content == null)
            {
                info.Overlay = null;
                return;
            }
            UIElement element = new ContentPresenter
            {
                Content = content,
                ContentTemplate = TaskbarItemInfoOverlay.GetContentTemplate(info),
                ContentTemplateSelector = TaskbarItemInfoOverlay.GetContentTemplateSelector(info)
            };
            element.Arrange(new(0, 0, 16, 16));
            RenderTargetBitmap bitmap = new(16, 16, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(element);
            bitmap.Freeze();
            info.Overlay = bitmap;
        }
    }
}
