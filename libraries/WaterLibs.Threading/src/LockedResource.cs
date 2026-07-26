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

namespace WaterLibs.Threading
{
    /// <summary>
    /// A lock on a certain amount of a certain resource.
    /// </summary>
    /// <remarks>
    /// <see cref="Dispose"/> must be called to free the locked resource.
    /// </remarks>
    /// <example>
    /// The destructor of this class calls the <see cref="Dispose"/> method,
    /// but is dependant on garbage collection.
    /// <br/>
    /// To ensure prompt resource release, it is advisable to take advantage
    /// of the <see langword="using"/> syntax to handle the call to <see cref="Dispose"/>.
    /// <code>
    /// using (LockedResource lockedResource = semaphore.Wait(5))
    /// {
    ///     // Use the resource
    /// }
    /// // The resource is released
    /// </code>
    /// </example>
    public sealed class LockedResource : IDisposable
    {
        private readonly ISizedSemaphore semaphore;
        private bool disposed;

        /// <summary>
        /// The resource quantity that this lock holds.
        /// </summary>
        public ulong Quantity { get; }

        internal LockedResource(ISizedSemaphore semaphore, ulong quantity)
        {
            this.semaphore = semaphore;
            Quantity = quantity;
            this.disposed = false;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!this.disposed)
            {
                this.semaphore.Free(Quantity);
                this.disposed = true;
            }
        }

        /// <inheritdoc/>
        ~LockedResource()
        {
            this.Dispose();
        }
    }
}
