using System;
using System.Collections.Generic;
using System.Linq;
using lib.Algorithms;
using lib.Origami;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace lib;

public class Screen
{
    private const double Alpha = 0.005;
    private readonly GeometricMedian geometricMedian = new();
    public Rgba[,] Pixels;

    public int Width => Pixels.GetLength(0);
    public int Height => Pixels.GetLength(1);

    public static Screen LoadProblem(int problem)
    {
        var file = FileHelper.FindFilenameUpwards($"problems/problem{problem}.png");
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

    public double DiffTo(V bottomLeft, V topRight, Rgba color)
    {
        var diff = 0.0;
        for (int x = bottomLeft.X; x < topRight.X; x++)
        for (int y = bottomLeft.Y; y < topRight.Y; y++)
        {
            var p1 = Pixels[x, y];
            diff += p1.DiffTo(color);
        }
        return diff * Alpha;
    }

    public double DiffTo(Screen other)
    {
        var diff = 0.0;
        for (int x = 0; x < Width; x++)
        for (int y = 0; y < Height; y++)
        {
            var p1 = Pixels[x, y];
            var p2 = other.Pixels[x, y];
            diff += p1.DiffTo(p2);
        }
        return diff * Alpha;
    }

    public double DiffTo(SimpleBlock block)
    {
        return DiffTo(block.BottomLeft, block.TopRight, block.Color);
    }

    public double DiffTo(Block block)
    {
        return block switch
        {
            SimpleBlock sb => DiffTo(sb),
            ComplexBlock cb => DiffTo(cb),
            _ => throw new Exception(block.ToString())
        };
    }

    public double DiffTo(ComplexBlock block)
    {
        return block.Children.Sum(DiffTo);
    }

    public Rgba GetAverageColor(Block block)
    {
        var pixelsCount = block.ScalarSize;

        var (r, g, b, a) = (0, 0, 0, 0);

        for (int x = block.BottomLeft.X; x < block.TopRight.X; x++)
        for (int y = block.BottomLeft.Y; y < block.TopRight.Y; y++)
        {
            var pixel = Pixels[x, y];
            r += pixel.R;
            g += pixel.G;
            b += pixel.B;
            a += pixel.A;
        }

        return new Rgba(
            (r / pixelsCount),
            (g / pixelsCount),
            (b / pixelsCount),
            (a / pixelsCount));
    }

    public Rgba GetAverageColor2(Block block)
    {
        var pixelsCount = block.ScalarSize;

        var (r, g, b, a) = (0.0, 0.0, 0.0, 0.0);

        for (int x = block.BottomLeft.X; x < block.TopRight.X; x++)
        for (int y = block.BottomLeft.Y; y < block.TopRight.Y; y++)
        {
            var pixel = Pixels[x, y];
            r += pixel.R*pixel.R;
            g += pixel.G*pixel.G;
            b += pixel.B*pixel.B;
            a += pixel.A*pixel.A;
        }

        return new Rgba(
            (int)Math.Sqrt(r / pixelsCount),
            (int)Math.Sqrt(g / pixelsCount),
            (int)Math.Sqrt(b / pixelsCount),
            (int)Math.Sqrt(a / pixelsCount));
    }

    public Rgba GetAverageColorByGeometricMedian(Block block)
    {
        return geometricMedian.GetGeometricMedian(this, block);
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

    public int CalculateScore(IEnumerable<Move> moves)
    {
        var canvas = new Canvas(this);
        foreach (var move in moves)
            canvas.Apply(move);
        return canvas.GetScore(this);
    }
}
