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

#:property PublishTrimmed=false
#:package Microsoft.Playwright@*

using System.Diagnostics;
using Microsoft.Playwright;

internal sealed class Program
{
    private static readonly DirectoryInfo root = new(Directory.GetCurrentDirectory());

    private static void Main(string[] args)
    {
        AssertRoot();
        string docsFolder = Path.Combine(root.FullName, "docs");
        string svg = Path.Combine(docsFolder, "favicon.svg");
        string png = Path.Combine(docsFolder, "icon.png");
        ConvertToPNG(svg, png).GetAwaiter().GetResult();
    }

    private static async Task ConvertToPNG(string svgPath, string pngPath)
    {
        Microsoft.Playwright.Program.Main(["install", "chromium"]);
        IPlaywright playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        IPage page = await browser.NewPageAsync();
        await page.SetViewportSizeAsync(512, 512);
        await page.GotoAsync($"file://{svgPath}");
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
        await page.ScreenshotAsync(new() { Path = pngPath, OmitBackground = true });
    }

    private static void AssertRoot()
    {
        string[] files =
        [
            "DotnetLibs.slnx",
            "Directory.Build.props",
            ".csharpierrc",
            ".editorconfig",
            ".gitattributes",
            ".gitignore",
        ];
        string[] dirs = ["docs", "libraries", ".github"];
        Trace.Assert(root.EnumerateFiles().Select(f => f.Name).Intersect(files).Count() == files.Length);
        Trace.Assert(root.EnumerateDirectories().Select(d => d.Name).Intersect(dirs).Count() == dirs.Length);
    }
}
