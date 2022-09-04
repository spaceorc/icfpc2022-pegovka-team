using System;
using System.Collections.Generic;
using lib.db;

namespace lib.Algorithms;


public class GridGuidedPainterResult
{
    public GridGuidedPainterResult(IList<Move> moves, int rows, int cols, int colorTolerance, int score, Canvas canvas)
    {
        Canvas = canvas;
        Moves = moves;
        Rows = rows;
        Cols = cols;
        ColorTolerance = colorTolerance;
        Score = score;
    }

    public Canvas Canvas;
    public IList<Move> Moves;
    public int Rows, Cols;
    public int ColorTolerance;
    public int Score;
}

public static class GridGuidedPainterRunner
{

    public static GridGuidedPainterResult Solve(int problemId, int rows, int cols)
    {
        Func<SimpleBlock,double,double> estimateBlock = (block, similarity) => 1.0 * similarity + 5*400.0 * 400 / ((400 - block.Left)*(400 - block.Bottom));

        var problem = Screen.LoadProblem(problemId);
        var grid = GridBuilder.BuildRegularGrid(problem, rows, cols);
        double estimation;


        (grid, estimation) = GridBuilder.OptimizeRowHeights(problem, grid, estimateBlock);
        (grid, estimation) = GridBuilder.OptimizeCellWidths(problem, grid, estimateBlock);
        (grid, estimation) = GridBuilder.OptimizeCellsViaMerge(problem, grid, estimateBlock);
        (grid, estimation) = GridBuilder.OptimizeRowHeights(problem, grid, estimateBlock);
        (grid, estimation) = GridBuilder.OptimizeCellWidths(problem, grid, estimateBlock);
        (grid, estimation) = GridBuilder.OptimizeRowHeights(problem, grid, estimateBlock);
        (grid, estimation) = GridBuilder.OptimizeCellWidths(problem, grid, estimateBlock);

        problem.ToImage($"{problemId}-grid-{rows}-{cols}.png", grid);

        GridGuidedPainterResult? bestResult = null;
        foreach (var colorTolerance in new[]{8, 16, 32, 48})
        {
            var (moves, score, canvas) = new GridGuidedPainter(grid, problem, colorTolerance).GetBestResultWithCanvas();
            if (bestResult == null || score < bestResult.Score)
            {
                bestResult = new GridGuidedPainterResult(moves, rows, cols, colorTolerance, score, canvas);
            }
        }

        return bestResult!;
    }
}
