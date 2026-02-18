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

namespace Docs.Test
{
    public class FaviconTest
    {
        [Fact]
        public async Task Favicon_ComparedWithMainWebsite_IsTheSame()
        {
            byte[] expected = await GetExpectedFavicon();
            byte[] actual = await GetActualFavicon();

            Assert.Equal(expected, actual);
        }

        private static async Task<byte[]> GetActualFavicon()
        {
            return [];
        }

        private static async Task<byte[]> GetExpectedFavicon()
        {
            using HttpClient httpClient = new();
            using HttpRequestMessage request = new(HttpMethod.Get, "https://matteoh2o1999.github.io/favicon.svg");
            using HttpResponseMessage responseMessage = await httpClient.SendAsync(request);
            responseMessage.EnsureSuccessStatusCode();
            return await responseMessage.Content.ReadAsByteArrayAsync();
        }
    }
}
