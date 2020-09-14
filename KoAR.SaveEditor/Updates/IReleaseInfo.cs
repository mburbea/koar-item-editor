using System;

namespace KoAR.SaveEditor.Updates
{
    public interface IReleaseInfo
    {
        string Body { get; }

        string Name { get; }

        DateTime PublishedAt { get; }
        
        string Version { get; }

        int ZipFileSize { get; }

        string ZipFileUri { get; }
    }
}
