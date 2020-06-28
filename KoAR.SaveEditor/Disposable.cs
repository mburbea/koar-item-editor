using System;
using System.Threading;

namespace KoAR.SaveEditor
{
    public sealed class Disposable : IDisposable
    {
        private Action? _action;

        public Disposable(Action? action) => this._action = action;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref this._action, null) is Action action)
            {
                action();
            }
        }
    }
}
