using System;
using System.Threading;
using System.Threading.Tasks;

namespace WaterLibs.Threading
{
    public class SizedSemaphore
    {
        private readonly ulong size;
        private ulong current;
        private readonly object lockObject;

        public SizedSemaphore(ulong size)
        {
            this.size = size;
            this.current = size;
            this.lockObject = new();
        }

        public IDisposable? TryLock(ulong quantity)
        {
            if (quantity > this.size)
            {
                throw new ArgumentException(
                    $"Borrowed quantity must be less or equal than the semaphore size ({this.size}). Got {quantity}.",
                    nameof(quantity)
                );
            }

            bool borrowed = false;
            lock (this.lockObject)
            {
                if (quantity <= this.current)
                {
                    this.current -= quantity;
                    borrowed = true;
                }
            }
            return borrowed ? new Locked(this, quantity) : null;
        }

        public IDisposable Lock(ulong quantity)
        {
            IDisposable? borrowed = null;
            while (borrowed is null)
            {
                borrowed = this.TryLock(quantity);
            }
            return borrowed;
        }

        public async Task<IDisposable> LockAsync(
            ulong quantity,
            int millisecondsBusyWaitInterval,
            CancellationToken cancellationToken
        )
        {
            IDisposable? borrowed = this.TryLock(quantity);
            while (borrowed is null)
            {
                await Task.Delay(millisecondsBusyWaitInterval, cancellationToken).ConfigureAwait(false);
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException(
                        "Operation was canceled before lock was acquired",
                        cancellationToken
                    );
                }
                borrowed = this.TryLock(quantity);
            }
            return borrowed;
        }

        public async Task<IDisposable> LockAsync(ulong quantity, CancellationToken cancellationToken)
        {
            IDisposable? borrowed = this.TryLock(quantity);
            while (borrowed is null)
            {
                await Task.Yield();
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException(
                        "Operation was canceled before lock was acquired",
                        cancellationToken
                    );
                }
                borrowed = this.TryLock(quantity);
            }
            return borrowed;
        }

        private void Unlock(ulong quantity)
        {
            lock (this.lockObject)
            {
                this.current += quantity;
            }
        }

        private sealed class Locked : IDisposable
        {
            private readonly SizedSemaphore semaphore;
            private readonly ulong borrowed;
            private bool disposed;

            public Locked(SizedSemaphore semaphore, ulong borrowed)
            {
                this.semaphore = semaphore;
                this.borrowed = borrowed;
                this.disposed = false;
            }

            public void Dispose()
            {
                if (!this.disposed)
                {
                    this.semaphore.Unlock(this.borrowed);
                    this.disposed = true;
                }
                GC.SuppressFinalize(this);
            }

            ~Locked()
            {
                this.Dispose();
            }
        }
    }
}
