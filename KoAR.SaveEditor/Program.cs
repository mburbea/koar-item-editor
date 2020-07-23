using System;
using KoAR.Core;
using KoAR.SaveEditor.Views;
using Microsoft.Windows.Themes;

namespace KoAR.SaveEditor
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Type.GetTypeCode(typeof(PlatformCulture)); // Only needed due to support WPF loading of Aero dll.
            Amalur.Initialize();
            App app = new App();
            app.InitializeComponent();
            app.Run();
        }
    }
}
