using System;
using System.Reflection;

namespace KoAR.SaveEditor.Views
{
    public static class ApplicationVersion
    {
        public static Version Current { get; } = new Version(Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion);
    }
}
