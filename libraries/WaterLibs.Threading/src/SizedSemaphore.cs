// Copyright (C) 2025-2026 Matteo Dell'Acqua
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace WaterLibs.Threading
{
    /// <summary>
    /// Limits the number of threads that can access a resource or pool of resources concurrently.
    /// </summary>
    /// <remarks>
    /// Unlike <see cref="Semaphore"/>, it can be locked with a custom size, so that each request
    /// locks a different quantity of a resource.
    /// </remarks>
    /// <param name="size">
    /// The maximum number of request that can be granted concurrently.
    /// </param>
    public sealed class SizedSemaphore(ulong size) : ISizedSemaphore
    {
        private ulong current = size;
        private readonly ulong size = size;
        private readonly object internalLock = new();

        void ISizedSemaphore.Free(ulong quantity)
        {
            Debug.Assert(quantity <= this.size);
            lock (this.internalLock)
            {
                this.current += quantity;
                Debug.Assert(this.current >= this.size);
                Monitor.PulseAll(this.internalLock);
            }
        }

        public LockedResource Wait(ulong quantity)
        {
            if (quantity > this.size)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(quantity),
                    $"Requested {quantity} on a semaphore of size {this.size}."
                );
            }
            lock (this.internalLock)
            {
                while (this.current < quantity)
                {
                    Monitor.Wait(this.internalLock);
                }
                this.current -= quantity;
                Monitor.PulseAll(this.internalLock);
            }
            return new(this, quantity);
        }

        public Task<LockedResource> WaitAsync(ulong quantity)
        {
            return Task.Run(() => Task.FromResult(this.Wait(quantity)));
        }
    }
}
