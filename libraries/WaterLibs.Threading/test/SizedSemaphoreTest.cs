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

namespace WaterLibs.Threading.Test
{
    [TestClass]
    public class SizedSemaphoreTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void Constructor_NonZeroSize_Succeeds()
        {
            _ = new SizedSemaphore(1);
        }

        [TestMethod]
        public void Constructor_ZeroSize_ThrowsArgumentOutOfRangeException()
        {
            Invoking(() => new SizedSemaphore(0)).Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void Wait_QuantityLessOrEqualThenSize_Succeeds()
        {
            SizedSemaphore sizedSemaphore = new(2);

            using LockedResource lockedResource = sizedSemaphore.Wait(1);
        }

        [TestMethod]
        public void Wait_QuantityGreaterOrEqualThenSize_ThrowsArgumentOutOfRangeException()
        {
            SizedSemaphore sizedSemaphore = new(2);

            sizedSemaphore.Invoking(s => s.Wait(3)).Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void Wait_TotalLessOrEqualThenSize_Succeeds()
        {
            SizedSemaphore sizedSemaphore = new(4);

            using LockedResource lockedResource1 = sizedSemaphore.Wait(1);
            using LockedResource lockedResource2 = sizedSemaphore.Wait(2);
        }

        [TestMethod]
        public void Wait_RequestAlreadyLockedResource_SucceedsWhenFreed()
        {
            SizedSemaphore sizedSemaphore = new(1);

            LockedResource lockedResource = sizedSemaphore.Wait(1);
            Task otherRequest = Task.Run(() => sizedSemaphore.Wait(1), TestContext.CancellationToken);

            otherRequest.Wait(1000, TestContext.CancellationToken).Should().BeFalse();

            lockedResource.Dispose();

            otherRequest.Wait(1000, TestContext.CancellationToken).Should().BeTrue();
        }

        [TestMethod]
        public void Wait_RequestSpecificQuantity_LocksSpecificQuantity()
        {
            SizedSemaphore sizedSemaphore = new(4);

            using LockedResource lockedResource = sizedSemaphore.Wait(2);

            lockedResource.Quantity.Should().Be(2);
        }

        [TestMethod]
        public void Wait_NoQuantitySpecified_DefaultsTo1()
        {
            SizedSemaphore sizedSemaphore = new(4);

            using LockedResource lockedResource = sizedSemaphore.Wait();

            lockedResource.Quantity.Should().Be(1);
        }
    }
}
