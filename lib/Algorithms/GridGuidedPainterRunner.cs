using System;
using System.Collections.Generic;
using lib.db;

namespace lib.Algorithms;

public static class GridGuidedPainterRunner
{
    public static IList<Move> Solve(int problemId, int rows, int cols)
    {
        Func<SimpleBlock,double,double> estimateBlock = (block, similarity) => 1.0 * similarity + 400.0 * 400 / block.ScalarSize;

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

        var (moves, _) = new GridGuidedPainter(grid, problem).GetBestResult();
        return moves;
    }
}
