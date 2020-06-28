using KoAR.Core;
using System.ComponentModel;
using System.Windows;

namespace KoAR.SaveEditor
{
    partial class App
    {
        static App()
        {
            if (!(bool)DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(Window)).DefaultValue)
            {
                Amalur.Initialize();
            }
        }
    }
}
