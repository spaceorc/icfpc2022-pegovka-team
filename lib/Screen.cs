using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace lib;

public class Screen
{
    public Rgba[,] Pixels;

    public int Width => Pixels.GetLength(0);
    public int Height => Pixels.GetLength(1);

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
            ps[x, bitmap.Height - y - 1] = new Rgba(p.R, p.G, p.B, p.A);
        }
        return new Screen(ps);
    }

    public Screen(int width, int height)
        : this(new Rgba[width, height])
    {
    }

    public Screen(Rgba[,] pixels)
    {
        Pixels = pixels;
    }

    public double DiffTo(Screen other)
    {
        var diff = 0.0;
        var alpha = 0.005;
        for (int x = 0; x < Width; x++)
        for (int y = 0; y < Height; y++)
        {
            var p1 = Pixels[x, y];
            var p2 = other.Pixels[x, y];
            diff += p1.DiffTo(p2);
        }
        return diff * alpha;
    }

    public double DiffTo(SimpleBlock block)
    {
        var diff = 0.0;
        var alpha = 0.005;
        for (int x = block.BottomLeft.X; x < block.TopRight.X; x++)
        for (int y = block.BottomLeft.Y; y < block.TopRight.Y; y++)
        {
            var p1 = Pixels[x, y];
            diff += p1.DiffTo(block.Color);
        }
        return diff * alpha;
    }

    public void ToImage(string pngPath)
    {
        using var image = new Image<Rgba32>(Width, Height);
        for (int y = 0; y < Height; y++)
        for (int x = 0; x < Width; x++)
        {
            var pixel = Pixels[x, y];
            image[x, Height - y - 1] = new Rgba32((byte)pixel.R, (byte)pixel.G, (byte)pixel.B, (byte)pixel.A);
        }
        image.Save(pngPath, new PngEncoder());
    }
}
