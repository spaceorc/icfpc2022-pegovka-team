using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using lib;
using lib.Algorithms;
using lib.db;
using NUnit.Framework;
using TextCopy;

namespace tests;

public class GridGuidedPainterTests
{
    [Test]
    [Parallelizable(ParallelScope.All)]
    public async Task Run([Range(1, 23)] int problemId)
    {
        var bestScore = double.PositiveInfinity;

        foreach (var size in new[]{5, 10, 20, 40, 80})
        {
            var screen = Screen.LoadProblem(problemId);
            var grid = CreateRegularGrid(size);
            var solver = new GridGuidedPainter(grid, screen);
            var (moves, score) = solver.GetBestResult();
            var solution = moves.StrJoin("\n");
            if (score < bestScore)
            {
                Console.WriteLine($"Score: {score} cellSize: {size} movesCount: {moves.Count}");
                bestScore = score;
                await ClipboardService.SetTextAsync(solution);
                await SolutionRepo.SubmitAsync(new ContestSolution(problemId, (int)score, solution, new SolverMeta(),"RegularGrid"));
            }
        }
    }

    private Grid CreateRegularGrid(int count)
    {
        var rows = new List<GridRow>();
        for (int y = 0; y < count; y++)
        {
            var cells = new List<GridCell>();
            for (int x = 0; x < count; x++)
            {
                cells.Add(new GridCell(400/count));
            }
            rows.Add(new GridRow(400/count, cells));
        }
        return new Grid(rows);

    }
}
