using System;
using System.Collections.Generic;
using System.Linq;

namespace lib.Algorithms;

public static class BestPCutMovesProvider
{
    public static IEnumerable<PCutMove> GetResultWithCustomWidth(Canvas canvas, Screen screen, int maxWidth, string blockId)
    {
        var minScore = double.PositiveInfinity;
        IEnumerable<(PCutMove, double)> answer = null!;
        for (var w = 10; w < maxWidth; w+=5)
        {
            var result = GetResult(canvas.Copy(), screen, w, blockId).ToArray();
            var currentScore = result.Sum(r => r.TotalScore);
            if (currentScore < minScore)
            {
                minScore = currentScore;
                answer = result;
            }
        }

        return answer.Select(a => a.Item1);
    }

    public static IEnumerable<(PCutMove, double TotalScore)> GetResult(Canvas canvas, Screen screen, int width, string blockId)
    {
        while (true)
        {
            var (move, totalScore, newCanvas) = ApplyNextPCutMove(canvas.Copy(), screen, width, blockId);
            yield return (move, totalScore);
            canvas = newCanvas;
            blockId = $"{canvas.TopLevelIdCounter}";
            if (NoBetterSolution(canvas, screen, canvas.Blocks[blockId], width))
                yield break;
        }

        static bool NoBetterSolution(Canvas canvas, Screen screen, Block block, int width)
        {
            if (block.Height <= 2 || block.Width <= 2)
                return true;

            if (canvas.Height - block.Bottom <= 2)
                return true;

            if (block.Left + width >= canvas.Width)
                return true;

            // var similarityPenalty = screen.DiffTo(block);
            // var moveCost = Move.GetCost(canvas.ScalarSize, block.ScalarSize, 10 + 5 + 1);
            // if (similarityPenalty <= moveCost)
            //     return true;

            return false;
        }

        static (PCutMove, double, Canvas) ApplyNextPCutMove(Canvas canvas, Screen screen, int width, string blockId)
        {
            var blocks = new Dictionary<int, (ColorMove, PCutMove, MergeMove, double score)>();

            var block = canvas.Blocks[blockId];

            for (var y = 1; y < canvas.Height - block.Bottom - 1; y++)
            {
                var pCutMove = new PCutMove(blockId, block.BottomLeft + new V(width, y));
                var (bottomLeft, _, topRight, topLeft) = Canvas.PreApplyPCut(block, pCutMove.Point);
                var color = screen.GetAverageColor(bottomLeft);
                var colorMove = new ColorMove(blockId, color);
                var mergeMove = new MergeMove(topLeft.Id, topRight.Id);
                var canvasSize = canvas.ScalarSize;

                var similarity = screen.DiffTo(bottomLeft.BottomLeft, bottomLeft.TopRight, color);

                var operationsCost = (Move.GetCost(canvasSize, block.ScalarSize, colorMove.BaseCost)
                                     + Move.GetCost(canvasSize, block.ScalarSize, pCutMove.BaseCost)
                                     + Move.GetCost(canvasSize, Math.Max(topRight.ScalarSize, topLeft.ScalarSize), mergeMove.BaseCost)
                                     + similarity) / bottomLeft.Height;

                blocks[y] = (colorMove, pCutMove, mergeMove, operationsCost);
            }

            var (bestColorMove, bestPCutMove, bestMergeMove, totalScore) = GetBestMove(blocks);

            canvas.ApplyColor(bestColorMove);
            canvas.ApplyPCut(bestPCutMove);
            canvas.ApplyMerge(bestMergeMove);

            return (bestPCutMove, totalScore, canvas);
        }
    }

    private static (ColorMove, PCutMove, MergeMove, double) GetBestMove(Dictionary<int, (ColorMove, PCutMove, MergeMove, double score)> currentVariants)
    {
        if (currentVariants.Count < 4)
            return Enumerable.MinBy(currentVariants, v => v.Value.score).Value;

        using var enumerator = currentVariants.Values.GetEnumerator();
        enumerator.MoveNext();
        var first = enumerator.Current;
        enumerator.MoveNext();
        var second = enumerator.Current;
        enumerator.MoveNext();
        var third = enumerator.Current;
        enumerator.MoveNext();

        if (first.score < second.score && first.score < third.score)
        {
            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;

                if (current.score < first.score)
                {
                    first = second;
                    second = third;
                    third = current;
                    break;
                }
                first = second;
                second = third;
                third = current;
            }
        }

        while (enumerator.MoveNext())
        {
            var current = enumerator.Current;

            if (first.score < second.score && first.score < third.score && first.score < current.score)
            {
                return first;
            }
            first = second;
            second = third;
            third = current;
        }

        return Enumerable.MinBy(new[] { first, second, third }, v => v.score);
    }
    // private PCutMove GetBestMove(Dictionary<int, (PCutMove, double score)> currentVariants)
    // {
    //     const int depth = 3;
    //
    //     var top3 = currentVariants.Select(v => v.Value).OrderBy(v => v.score).Take(3).ToArray();
    //
    //     PCutMove ScoreMove(int iteration, IEnumerable<(PCutMove, double)> previous)
    //     {
    //         if (iteration == depth)
    //             return
    //     }
    // }
}
