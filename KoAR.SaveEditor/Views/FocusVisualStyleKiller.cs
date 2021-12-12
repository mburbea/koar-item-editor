using System.Windows;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Views;

public static class FocusVisualStyleKiller
{
    public static readonly DependencyProperty KillProperty = DependencyProperty.RegisterAttached("Kill", typeof(bool), typeof(FocusVisualStyleKiller),
        new FrameworkPropertyMetadata(BooleanBoxes.False, FrameworkPropertyMetadataOptions.Inherits, FocusVisualStyleKiller.KillProperty_ValueChanged));

    public static bool GetKill(FrameworkElement element) => element != null && (bool)element.GetValue(FocusVisualStyleKiller.KillProperty);

    public static void SetKill(FrameworkElement element, bool value) => element?.SetValue(FocusVisualStyleKiller.KillProperty, BooleanBoxes.GetBox(value));

    private static void KillProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FrameworkElement element && (bool)e.NewValue)
        {
            element.FocusVisualStyle = null;
        }
    }
}
