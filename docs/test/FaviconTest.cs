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

using Microsoft.Playwright;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Docs.Test
{
    [TestClass]
    public class FaviconTest
    {
        [AssemblyInitialize]
        public static void Setup(TestContext _)
        {
            Microsoft.Playwright.Program.Main(["install", "chromium"]);
        }

        [TestMethod]
        [DeploymentItem("Deployment/favicon.svg")]
        public async Task FaviconSVG_ComparedWithMainWebsite_IsTheSame()
        {
            byte[] expected = await GetExpectedFaviconSvg();
            byte[] actual = File.ReadAllBytes("Deployment/favicon.svg");

            actual.Should().Equal(expected);
        }

        private static async Task<byte[]> GetExpectedFaviconSvg()
        {
            using HttpClient httpClient = new();
            using HttpRequestMessage request = new(HttpMethod.Get, "https://matteoh2o1999.github.io/favicon.svg");
            using HttpResponseMessage responseMessage = await httpClient.SendAsync(request);
            responseMessage.EnsureSuccessStatusCode();
            return await responseMessage.Content.ReadAsByteArrayAsync();
        }

        [TestMethod]
        [DeploymentItem("Deployment/icon.png")]
        public void FaviconPNG_InDocsFolder_Exists()
        {
            FileInfo png = new("Deployment/icon.png");

            png.Exists.Should().BeTrue();
        }

        [TestMethod]
        [DeploymentItem("Deployment/icon.png")]
        [DeploymentItem("Deployment/favicon.svg")]
        public async Task FaviconPNG_ComparedToSVG_IsTheSame()
        {
            byte[] expectedBytes = await GetExpectedFaviconPng();
            byte[] actualBytes = File.ReadAllBytes("Deployment/icon.png");

            using Image<Rgba64> expected = Image.Load<Rgba64>(expectedBytes);
            using Image<Rgba64> actual = Image.Load<Rgba64>(actualBytes);

            actual.Width.Should().Be(expected.Width);
            actual.Height.Should().Be(expected.Height);

            for (int i = 0; i < actual.Width; i++)
            {
                for (int j = 0; j < actual.Height; j++)
                {
                    actual[i, j].Should().Be(expected[i, j]);
                }
            }
        }

        private static async Task<byte[]> GetExpectedFaviconPng()
        {
            FileInfo svg = new("./Deployment/favicon.svg");

            IPlaywright playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
            IPage page = await browser.NewPageAsync();
            await page.SetViewportSizeAsync(512, 512);
            await page.GotoAsync($"file://{svg.FullName}");
            await page.EvaluateAsync(
                """
                const svg = document.querySelector('svg');
                if (svg) {
                    svg.style.width = '100%';
                    svg.style.height = '100%';
                    svg.style.objectFit = 'contain';
                }
                """
            );
            return await page.ScreenshotAsync(new() { OmitBackground = true });
        }
    }
}
