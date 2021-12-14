local lib = ".\\mods\\dll\\KoreUtils.dll"
FileOpenWrite = package.loadlib(lib, "FileOpenWrite")
FileWrite = package.loadlib(lib, "FileWrite")
FileClose = package.loadlib(lib, "FileClose")
FileFlush = package.loadlib(lib, "FileFlush")

return{
    OpenWrite = FileOpenWrite,
    Write = FileWrite,
    Flush = FileFlush,
    Close = FileClose
}