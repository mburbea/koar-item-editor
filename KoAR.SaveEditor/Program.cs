using System;
using KoAR.Core;
using KoAR.SaveEditor.Views;

namespace KoAR.SaveEditor
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Amalur.Initialize();
            App app = new App();
            app.InitializeComponent();
            app.Run();
        }
    }
}
