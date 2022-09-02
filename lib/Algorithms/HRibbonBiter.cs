using System.Collections.Generic;
using System.Linq;

namespace lib.Algorithms;

public class AvgStd2
{
    public double Avg;
    public double Std2;
    public int N;

    public void AddValue(double value)
    {
        var newAvg = (Avg * N + value) / (N + 1);
        var newStd2 = N * (Std2 +(newAvg - Avg).Squared() + (newAvg - value).Squared()) / (N + 1);
        Avg = newAvg;
        Std2 = newStd2;
        N++;
    }
}

public class HRibbonBiter
{
    public IEnumerable<Move> Solve(Screen problem)
    {
        var canvas = new Canvas(problem);
        SimpleBlock mainBlock = (SimpleBlock)canvas.Blocks.Values.Single();
        while (true)
        {
            (int ribbonHeight, Rgba color) = FindTopRibbon(mainBlock, problem, canvas);
            if (mainBlock.Color != color)
            {
                var colorMove = new ColorMove(mainBlock.Id, color);
                canvas.ApplyColor(colorMove);
                yield return colorMove;
            }

            if (ribbonHeight == mainBlock.Height) break;
            var hCutMove = new HCutMove(mainBlock.Id, mainBlock.BottomLeft.Y + ribbonHeight);
            var (_, tBlock) = canvas.ApplyHCut(hCutMove);
            yield return hCutMove;
            mainBlock = (SimpleBlock)tBlock;
        }
    }

    private (int ribbonHeight, Rgba color) FindTopRibbon(SimpleBlock block, Screen problem, Canvas canvas)
    {
        int sr = 0;
        var sg = 0;
        var sb = 0;
        var sa = 0;
        var n = 0;
        Rgba prevColor = new Rgba(0,0,0,0);
        for (int h = 1; h <= block.Height; h++)
        {
            var moveCost = new HCutMove(block.Id, block.BottomLeft.Y + h).GetCost(canvas);
            for (int x = block.BottomLeft.X; x < block.TopRight.X ; x++)
            {
                var pixel = problem.Pixels[x, block.BottomLeft.Y+h-1];
                sr+=pixel.R;
                sg+=pixel.G;
                sb+=pixel.B;
                sa+=pixel.A;
                n++;
            }

            var color = new Rgba(sr / n, sg / n, sb / n, sa / n);
            var ribbon = new SimpleBlock("", block.BottomLeft, new V(block.TopRight.X, block.BottomLeft.Y + h), color);
            var similarityCost = problem.DiffTo(ribbon);
            if (h > 1 && similarityCost > moveCost && color != prevColor)
                return (h - 1, prevColor);
            prevColor = color;
        }
        return (block.Height, prevColor);
    }
}
