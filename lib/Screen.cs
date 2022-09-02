
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace lib;

public class Screen
{
    public Rgba[,] Pixels;

    public static Screen LoadProblem(int problem)
    {
        var file = FileHelper.FindFilenameUpwards($"problems/{problem}.png");
        using var image = (Image<Rgba32>)Image.Load(file, new PngDecoder());
        return LoadFrom(image);
    }

    public static Screen LoadFrom(Image<Rgba32> bitmap)
    {
        var ps = new Rgba[bitmap.Width, bitmap.Height];
        for (int x = 0; x < bitmap.Width; x++)
        for (int y = 0; y < bitmap.Height; y++)
        {
            var p = bitmap[x, y];
            ps[x, y] = new Rgba(p.R, p.G, p.B, p.A);
        }
        return new Screen(ps);
    }

    public Screen(Rgba[,] pixels)
    {
        Pixels = pixels;
    }
}
