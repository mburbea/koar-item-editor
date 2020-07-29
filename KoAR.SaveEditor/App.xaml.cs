using System;
using KoAR.Core;
using Microsoft.Windows.Themes;

namespace KoAR.SaveEditor
{
    partial class App
    {
        static App()
        {
            Type.GetTypeCode(typeof(PlatformCulture)); // Needed to enforce loading of PresentationFramework.Aero.dll before initializing App.
            Amalur.Initialize();
        }
    }
}
