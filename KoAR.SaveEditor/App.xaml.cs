using System;
using System.Reflection;
using Microsoft.Windows.Themes;

namespace KoAR.SaveEditor
{
    partial class App
    {
        static App() => Type.GetTypeCode(typeof(PlatformCulture)); // Needed to enforce loading of PresentationFramework.Aero.dll before initializing App.

        public static Version Version { get; } = new Version(Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion);
    }
}
