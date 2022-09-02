using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace lib.Algorithms;

public class SimpleAlgorithm : IAlgorithm
{
    public IEnumerable<string> GetResult(Screen screen)
    {
        var canvas = new Canvas(screen.Width, screen.Height, new Rgba(255, 255, 255, 255));

        var averageColor = GetAverageColor(screen);

        var move = new ColorMove("0", averageColor);
        var totalCost = move.GetCost(canvas);

        yield return move.ToString();
    }

    public Rgba GetAverageColor(Screen screen)
    {
        var pixelsCount = screen.Height * screen.Width;

        var (r, g, b, a) = (0, 0, 0, 0);

        for (int y = 0; y < screen.Height; y++)
        for (int x = 0; x < screen.Width; x++)
            {
                var pixel = screen.Pixels[x, y];
                r += pixel.R;
                g += pixel.G;
                b += pixel.B;
                a += pixel.A;
            }

        return new Rgba(
            r / pixelsCount,
            g / pixelsCount,
            b / pixelsCount,
            a / pixelsCount);
    }

    public int Score(Image<Rgba32> image)
    {
        throw new NotImplementedException();
    }
}
