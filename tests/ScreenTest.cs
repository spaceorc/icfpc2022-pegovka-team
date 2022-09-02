using FluentAssertions;
using lib;
using NUnit.Framework;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace tests;

[TestFixture]
public class ScreenTests
{
    [Test]
    public void TestLoad()
    {
        var fn = FileHelper.FindFilenameUpwards("problems/1.png");
        using var image = (Image<Rgba32>)Image.Load(fn, new PngDecoder());
        var screen = Screen.LoadFrom(image);
    } 

}
