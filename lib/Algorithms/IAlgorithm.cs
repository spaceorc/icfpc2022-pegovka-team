using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;

namespace lib.Algorithms;

public interface IAlgorithm
{
    public IEnumerable<string> GetResult(Screen screen);

    public int Score(Image<Rgba32> image);
}
