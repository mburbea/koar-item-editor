using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using KoAR.SaveEditor.Properties;

namespace KoAR.SaveEditor.Views;

public sealed class ZoomScaler : Control
{
    public static readonly RoutedCommand ResetZoomCommand = new("Reset Zoom", typeof(ZoomScaler));

    public static readonly DependencyProperty TargetProperty = DependencyProperty.Register(nameof(ZoomScaler.Target), typeof(FrameworkElement), typeof(ZoomScaler),
        new(null, ZoomScaler.TargetProperty_ValueChanged));

    static ZoomScaler() => FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ZoomScaler), new FrameworkPropertyMetadata(typeof(ZoomScaler)));

    public FrameworkElement? Target
    {
        get => (FrameworkElement?)this.GetValue(ZoomScaler.TargetProperty);
        set => this.SetValue(ZoomScaler.TargetProperty, value);
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
        this.CommandBindings.Add(new(ZoomScaler.ResetZoomCommand, ZoomScaler.ResetZoomCommand_Executed));
    }

    private static void AttachToTarget(FrameworkElement target)
    {
        if ((target as Window ?? Window.GetWindow(target)) is not { } window)
        {
            target.Loaded += ZoomScaler.Target_Loaded;
            return;
        }
        ScaleTransform transform = new();
        Binding binding = new(nameof(Settings.ZoomScale)) { Source = Settings.Default };
        BindingOperations.SetBinding(transform, ScaleTransform.ScaleXProperty, binding);
        BindingOperations.SetBinding(transform, ScaleTransform.ScaleYProperty, binding);
        target.LayoutTransform = transform;
        window.AddHandler(UIElement.PreviewKeyDownEvent, new KeyEventHandler(ZoomScaler.Window_PreviewKeyDown));
        window.AddHandler(UIElement.PreviewMouseWheelEvent, new MouseWheelEventHandler(ZoomScaler.Window_PreviewMouseWheel));
    }

    private static void DetachFromTarget(FrameworkElement target)
    {
        if ((target as Window ?? Window.GetWindow(target)) is not { } window)
        {
            target.Loaded -= ZoomScaler.Target_Loaded;
            return;
        }
        target.LayoutTransform = null;
        window.RemoveHandler(UIElement.PreviewKeyDownEvent, new KeyEventHandler(ZoomScaler.Window_PreviewKeyDown));
        window.RemoveHandler(UIElement.PreviewMouseWheelEvent, new MouseWheelEventHandler(ZoomScaler.Window_PreviewMouseWheel));
    }

    private static void ResetZoomCommand_Executed(object? sender, ExecutedRoutedEventArgs e)
    {
        Settings.Default.ZoomScale = 1d;
    }

    private static void Target_Loaded(object? sender, RoutedEventArgs e)
    {
        FrameworkElement target = (FrameworkElement)sender!;
        target.Loaded -= ZoomScaler.Target_Loaded;
        ZoomScaler.AttachToTarget(target);
    }

    private static void TargetProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is FrameworkElement previous)
        {
            ZoomScaler.DetachFromTarget(previous);
        }
        if (e.NewValue is FrameworkElement next)
        {
            ZoomScaler.AttachToTarget(next);
        }
    }

    private static void Window_PreviewKeyDown(object? sender, KeyEventArgs e)
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

    private static void Window_PreviewMouseWheel(object? sender, MouseWheelEventArgs e)
    {
        if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control)
        {
            return;
        }
        Settings.Default.ZoomScale = e.Delta switch
        {
            > 0 => Math.Min(Settings.Default.MaxZoomScale, Settings.Default.ZoomScale + 0.05),
            < 0 => Math.Max(Settings.Default.MinZoomScale, Settings.Default.ZoomScale - 0.05),
            _ => Settings.Default.ZoomScale,
        };
    }
}
