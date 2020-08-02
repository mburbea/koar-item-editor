using System;

namespace KoAR.SaveEditor.Constructs
{
    public sealed class EventArgs<TData> : EventArgs
    {
        public EventArgs(TData data) => this.Data = data;

        public TData Data { get; }
    }
}
