using System;

namespace KoAR.SaveEditor.Views.Updates
{
    public interface IReleaseInfo
    {
        string Body { get; }

        string Name { get; }

        DateTime PublishedAt { get; }
        
        string Version { get; }
    }
}
