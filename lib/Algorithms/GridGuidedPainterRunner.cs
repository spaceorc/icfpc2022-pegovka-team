using System.Collections.Generic;
using System.Linq;

namespace lib.Algorithms;


public class GridGuidedPainterResult
{
    public GridGuidedPainterResult(IList<Move> moves, int rows, int cols, int colorTolerance, int score, int orientation, Canvas canvas)
    {
        Canvas = canvas;
        Moves = moves;
        Rows = rows;
        Cols = cols;
        ColorTolerance = colorTolerance;
        Orientation = orientation;
        Score = score;
    }

    public Canvas Canvas;
    public IList<Move> Moves;
    public int Rows, Cols;
    public int ColorTolerance;
    public int Orientation;
    public int Score;
}

public static class GridGuidedPainterRunner
{
    public static GridGuidedPainterResult Solve(int problemId, int rows, int cols, int orientation = 0, bool useSwapperPreprocessor = false)
    {
        var originalProblem = Screen.LoadProblem(problemId);
        return Solve(problemId, originalProblem, rows, cols, orientation);
    }

    public static GridGuidedPainterResult Solve(int problemId, Screen originalProblem, int rows, int cols, int orientation = 0, bool useSwapperPreprocessor = false)
    {
        var problemBeforePreprocessor = Rotator.Rotate(originalProblem, orientation);
        var problem = problemBeforePreprocessor;
        int[] preprocessorRows = null!;

        if (useSwapperPreprocessor)
            (problem, preprocessorRows) = SwapperPreprocessor.Preprocess(problemBeforePreprocessor, 4);


        var grid = GridBuilder.BuildRegularGrid(problem, rows, cols);

        (grid, _) = GridBuilder.OptimizeRowHeights(problem, grid);
        (grid, _) = GridBuilder.OptimizeCellWidths(problem, grid);
        (grid, _) = GridBuilder.OptimizeCellsViaMerge(problem, grid);
        (grid, _) = GridBuilder.OptimizeRowsViaMerge(problem, grid);
        (grid, _) = GridBuilder.OptimizeRowHeights(problem, grid);
        (grid, _) = GridBuilder.OptimizeCellWidths(problem, grid);
        (grid, _) = GridBuilder.OptimizeCellsViaMerge(problem, grid);
        (grid, _) = GridBuilder.OptimizeRowHeights(problem, grid);
        (grid, _) = GridBuilder.OptimizeCellWidths(problem, grid);

        // problem.ToImage($"{problemId}-grid-{rows}-{cols}-{orientation}-{useSwapperPreprocessor}.png", grid);

        GridGuidedPainterResult? bestResult = null;
        foreach (var colorTolerance in new[]{0, 1, 2, 4, 8, 16, 32, 48})
        {
            var (moves, score, canvas) = new GridGuidedPainter(grid, problem, colorTolerance).GetBestResultWithCanvas();
            if (bestResult == null || score < bestResult.Score)
            {
                bestResult = new GridGuidedPainterResult(moves, rows, cols, colorTolerance, score, 0, canvas);
            }
        }

        if (useSwapperPreprocessor)
        {
            var moves = bestResult!.Moves.ToList();
            moves.AddRange(SwapperPreprocessor.Postprocess(preprocessorRows, bestResult!.Canvas));
            bestResult = new GridGuidedPainterResult(moves, rows, cols, bestResult.ColorTolerance, problemBeforePreprocessor.GetScore(moves), orientation, bestResult!.Canvas);
        }

        var rMoves = Rotator.RotateBack(problemBeforePreprocessor, bestResult!.Moves.ToList(), orientation);
        var rCanvas = new Canvas(originalProblem);
        foreach (var rMove in rMoves)
            rCanvas.Apply(rMove);

        return new GridGuidedPainterResult(rMoves, rows, cols, bestResult!.ColorTolerance, bestResult!.Score, orientation, rCanvas);
    }
}
