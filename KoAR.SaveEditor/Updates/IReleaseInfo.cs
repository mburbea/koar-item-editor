using System;

namespace KoAR.SaveEditor.Updates;

public interface IReleaseInfo
{
    string Body { get; }

    string Name { get; }

    DateTime PublishedAt { get; }

    Version Version { get; }

    int ZipFileSize { get; }

    string ZipFileUri { get; }
}
