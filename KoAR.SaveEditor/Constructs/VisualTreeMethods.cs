using System.Windows;
using System.Windows.Media;

namespace KoAR.SaveEditor.Constructs
{
    public static class VisualTreeMethods
    {
        public static T? FindVisualTreeAncestor<T>(this DependencyObject d)
            where T : class
        {
            while (d != null)
            {
                if ((d = VisualTreeHelper.GetParent(d)) is T element)
                {
                    return element;
                }
            }
            return default;
        }
    }
}
