using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace lib.Algorithms;

public class SimpleAlgorithm : IAlgorithm
{
    public const int CanvasSize = 400;

    public (IList<Move>, double Score) GetBestResult(Screen screen)
    {
        var (bestResult, bestScore) = (new List<Move>(), double.MaxValue);

        for (var size = 25; size <= CanvasSize; size *= 2)
        {
            var (result, score) = GetResult(screen, size);
            if (score < bestScore)
            {
                bestResult = result;
                bestScore = score;
            }
        }

        return (bestResult, bestScore);
    }

    private (List<Move> Moves, double Score) GetResult(Screen screen, int maxBlockSize)
    {
        var resultMoves = new List<Move>();

        var canvas = new Canvas(screen.Width, screen.Height, new Rgba(255, 255, 255, 255));

        while (true)
        {
            var block = GetBlock(canvas, maxBlockSize * 2);
            if (block is null) break;

            var cutMove = new PCutMove(block.Id, block.BottomLeft + block.Size / 2);
            canvas.ApplyPCut(cutMove);
            resultMoves.Add(cutMove);
        }

        var blocks = canvas.Blocks.Values.ToList();
        foreach (var block in blocks)
        {
            var averageBlockColor = GetAverageColor(screen, block);
            var colorMove = new ColorMove(block.Id, averageBlockColor);

            var currentScore = canvas.GetScore(screen);
            var currentBlocks = canvas.Blocks;
            canvas.ApplyColor(colorMove);

            var newScore = canvas.GetScore(screen);
            if (newScore < currentScore)
            {
                resultMoves.Add(colorMove);
            }
            else
            {
                canvas.Blocks = currentBlocks;
            }
        }

        var totalScore = canvas.GetScore(screen);
        return (resultMoves, totalScore);
    }

    public Rgba GetAverageColor(Screen screen, Block block)
    {
        var pixelsCount = screen.Height * screen.Width;

        var (r, g, b, a) = (0, 0, 0, 0);

        for (int x = block.BottomLeft.X; x < block.TopRight.X; x++)
            for (int y = block.BottomLeft.Y; y < block.TopRight.Y; y++)
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

    private Block? GetBlock(Canvas canvas, int minSize)
    {
        return canvas.Blocks.Values
            .Where(x => x.Size.X >= minSize && x.Size.Y >= minSize)
            .FirstOrDefault();
    }
}
