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
    public static GridGuidedPainterResult Solve(int problemId, int rows, int cols, int orientation = 0)
    {
        var originalProblem = Screen.LoadProblem(problemId);
        var problem = Rotator.Rotate(originalProblem, orientation);

        var grid = GridBuilder.BuildRegularGrid(problem, rows, cols);

        (grid, _) = GridBuilder.OptimizeRowHeights(problem, grid);
        (grid, _) = GridBuilder.OptimizeCellWidths(problem, grid);
        (grid, _) = GridBuilder.OptimizeCellsViaMerge(problem, grid);
        (grid, _) = GridBuilder.OptimizeRowsViaMerge(problem, grid);
        (grid, _) = GridBuilder.OptimizeRowHeights(problem, grid);
        (grid, _) = GridBuilder.OptimizeCellWidths(problem, grid);
        (grid, _) = GridBuilder.OptimizeRowHeights(problem, grid);
        (grid, _) = GridBuilder.OptimizeCellWidths(problem, grid);

        //problem.ToImage($"{problemId}-grid-{rows}-{cols}-{orientation}.png", grid);

        GridGuidedPainterResult? bestResult = null;
        foreach (var colorTolerance in new[]{0, 1, 2, 4, 8, 16, 32, 48})
        {
            var (moves, score, canvas) = new GridGuidedPainter(grid, problem, colorTolerance).GetBestResultWithCanvas();
            if (bestResult == null || score < bestResult.Score)
            {
                bestResult = new GridGuidedPainterResult(moves, rows, cols, colorTolerance, score, 0, canvas);
            }
        }

        var rMoves = Rotator.RotateBack(problem, bestResult!.Moves.ToList(), orientation);
        var rCanvas = new Canvas(originalProblem);
        foreach (var rMove in rMoves)
            rCanvas.Apply(rMove);

        return new GridGuidedPainterResult(rMoves, rows, cols, bestResult!.ColorTolerance, bestResult!.Score, orientation, rCanvas);
    }
}
