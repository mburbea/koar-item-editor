using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using KoAR.SaveEditor.Properties;

namespace KoAR.SaveEditor.Views
{
    public sealed class ZoomScaler : Control
    {
        public static readonly DependencyProperty TargetProperty = DependencyProperty.Register(nameof(ZoomScaler.Target), typeof(FrameworkElement), typeof(ZoomScaler),
            new PropertyMetadata(null, ZoomScaler.TargetProperty_ValueChanged));

        static ZoomScaler() => FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ZoomScaler), new FrameworkPropertyMetadata(typeof(ZoomScaler)));

        public FrameworkElement? Target
        {
            get => (FrameworkElement?)this.GetValue(ZoomScaler.TargetProperty);
            set => this.SetValue(ZoomScaler.TargetProperty, value);
        }

        private static void AttachToTarget(FrameworkElement target)
        {
            if (!target.IsInitialized)
            {
                target.Initialized += ZoomScaler.Target_Initialized;
                return;
            }
            ScaleTransform transform = new ScaleTransform();
            Binding binding = new Binding(nameof(Settings.ZoomScale)) { Source = Settings.Default };
            BindingOperations.SetBinding(transform, ScaleTransform.ScaleXProperty, binding);
            BindingOperations.SetBinding(transform, ScaleTransform.ScaleYProperty, binding);
            target.LayoutTransform = transform;
            if (PresentationSource.FromVisual(target)?.RootVisual is UIElement rootElement)
            {
                rootElement.AddHandler(UIElement.PreviewKeyDownEvent, new KeyEventHandler(ZoomScaler.RootElement_PreviewKeyDown));
                rootElement.AddHandler(UIElement.PreviewMouseWheelEvent, new MouseWheelEventHandler(ZoomScaler.RootElement_PreviewMouseWheel));
            }
        }

        private static void DetachFromTarget(FrameworkElement target)
        {
            if (!target.IsInitialized)
            {
                target.Initialized -= ZoomScaler.Target_Initialized;
                return;
            }
            target.LayoutTransform = null;
            if (PresentationSource.FromVisual(target)?.RootVisual is UIElement rootElement)
            {
                rootElement.RemoveHandler(UIElement.PreviewKeyDownEvent, new KeyEventHandler(ZoomScaler.RootElement_PreviewKeyDown));
                rootElement.RemoveHandler(UIElement.PreviewMouseWheelEvent, new MouseWheelEventHandler(ZoomScaler.RootElement_PreviewMouseWheel));
            }
        }

        private static void RootElement_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control)
            {
                return;
            }
            Settings.Default.ZoomScale = e.Key switch
            {
                Key.D0 => 1d,
                Key.NumPad0 => 1d,
                Key.OemPlus => Math.Min(Settings.Default.MaxZoomScale, Settings.Default.ZoomScale + 0.05),
                Key.OemMinus => Math.Max(Settings.Default.MinZoomScale, Settings.Default.ZoomScale - 0.05),
                _ => Settings.Default.ZoomScale,
            };
        }

        private static void RootElement_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control)
            {
                return;
            }
            Settings.Default.ZoomScale = Math.Sign(e.Delta) switch
            {
                1 => Math.Min(Settings.Default.MaxZoomScale, Settings.Default.ZoomScale + 0.05),
                -1 => Math.Max(Settings.Default.MinZoomScale, Settings.Default.ZoomScale - 0.05),
                _ => Settings.Default.ZoomScale,
            };
        }

        private static void Target_Initialized(object sender, EventArgs e)
        {
            FrameworkElement target = (FrameworkElement)sender;
            target.Initialized -= ZoomScaler.Target_Initialized;
            ZoomScaler.AttachToTarget(target);
        }

        private static void TargetProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
            {
                ZoomScaler.DetachFromTarget((FrameworkElement)e.OldValue);
            }
            if (e.NewValue != null)
            {
                ZoomScaler.AttachToTarget((FrameworkElement)e.NewValue);
            }
        }
    }
}
