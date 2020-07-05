using System;
using System.Threading;
using System.Threading.Tasks;

namespace KoAR.SaveEditor.Constructs
{
    public static class Debounce
    {
        public static Func<Task> Method(Action method, int ms = 250) => new Debouncer(method, ms).CreateTask;

        public static Func<Task<T>> Method<T>(Func<T> method, int ms = 250) => new Debouncer<T>(method, ms).CreateTask;

        private sealed class Debouncer : DebouncerBase
        {
            private readonly Action _method;

            public Debouncer(Action method, int milliseconds)
                : base(milliseconds) => this._method = method;

            public Task CreateTask() => this.DelayTask().ContinueWith(_ => this._method(), TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        private sealed class Debouncer<T> : DebouncerBase
        {
            private readonly Func<T> _method;

            public Debouncer(Func<T> method, int milliseconds)
                : base(milliseconds) => this._method = method;

            public Task<T> CreateTask() => this.DelayTask().ContinueWith(_ => this._method(), TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        private abstract class DebouncerBase
        {
            private readonly int _milliseconds;
            private CancellationTokenSource? _debounceSource;

            protected DebouncerBase(int milliseconds) => this._milliseconds = milliseconds;

            protected Task DelayTask()
            {
                Interlocked.Exchange(ref this._debounceSource, new CancellationTokenSource())?.Cancel();
                return Task.Delay(this._milliseconds, this._debounceSource!.Token);
            }
        }
    }
}
