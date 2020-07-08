using System;
using System.Threading;
using System.Threading.Tasks;

namespace KoAR.SaveEditor.Constructs
{
    public static class Debounce
    {
        public static Func<Task<T>> Method<T>(Func<T> method, int ms = 250)
        {
            CancellationTokenSource? tokenSource = null;
            return delegate
            {
                Interlocked.Exchange(ref tokenSource, new CancellationTokenSource())?.Cancel();
                return Task.Delay(ms, tokenSource!.Token).ContinueWith(_ => method(), TaskContinuationOptions.OnlyOnRanToCompletion);
            };
        }

        public static Func<Task> Method(Action method, int ms = 250)
        {
            CancellationTokenSource? tokenSource = null;
            return delegate
            {
                Interlocked.Exchange(ref tokenSource, new CancellationTokenSource())?.Cancel();
                return Task.Delay(ms, tokenSource!.Token).ContinueWith(_ => method(), TaskContinuationOptions.OnlyOnRanToCompletion);
            };
        }
    }
}
