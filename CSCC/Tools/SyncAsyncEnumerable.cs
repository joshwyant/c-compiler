namespace CSCC.Tools;

class SyncAsyncEnumerable<T>(IEnumerable<T> enumerable) : IAsyncEnumerable<T>
{
    readonly IEnumerable<T> SyncEnumerable = enumerable;
    IAsyncEnumerator<T>? AsyncEnumerator;
    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return AsyncEnumerator ??= new SyncAsyncEnumerator(SyncEnumerable.GetEnumerator(), cancellationToken);
    }
    class SyncAsyncEnumerator(IEnumerator<T> syncEnumerator, CancellationToken cancellationToken = default) : IAsyncEnumerator<T>
    {
        readonly CancellationToken CancellationToken = cancellationToken;
        readonly IEnumerator<T> SyncEnumerator = syncEnumerator;
        public T Current => SyncEnumerator.Current;

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> MoveNextAsync()
        {
            if (CancellationToken.IsCancellationRequested)
            {
                return ValueTask.FromCanceled<bool>(CancellationToken);
            }
            return ValueTask.FromResult(SyncEnumerator.MoveNext());
        }
    }
}