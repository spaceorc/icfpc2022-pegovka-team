using System.Collections.Generic;
using System.Linq;
using System.Threading;
using lib.Algorithms;

namespace lib;

public static class GridBuilder
{
    public static Grid BuildOptimalGrid(Screen problem)
    {
        var grid = BuildRegularRows(problem, 10);
        (grid, _) = OptimizeRowsCount(problem, grid);

        var (optimizedGrid, _) = OptimizeGrid(problem, grid);

        return optimizedGrid;
    }

    public static (Grid grid, double estimation) OptimizeGrid(Screen problem, Grid grid)
    {
        var bestEstimation = EstimateGrid(problem, grid);

        while (true)
        {
            var (gr, _) = OptimizeRowHeights(problem, grid);
            var (optimizedGrid, nextEstimation) = OptimizeAllGridRows(problem, gr);
            if (nextEstimation >= bestEstimation)
                return (grid, bestEstimation);

            bestEstimation = nextEstimation;
            grid = optimizedGrid;

        }
    }

    public static (Grid grid, double estimation) OptimizeAllGridRows(Screen problem, Grid grid)
    {
        var bestEstimation = EstimateGrid(problem, grid);

        while (true)
        {
            var optimized = false;
            for (int i = 0; i < grid.Rows.Count; i++)
            {
                var (copy, estimation) = OptimizeCells(problem, grid, i);
                if (estimation < bestEstimation)
                {
                    bestEstimation = estimation;
                    grid = copy;
                    optimized = true;
                    break;
                }
            }

            if (!optimized)
                return (grid, bestEstimation);
        }
    }

    public static (Grid grid, double estimation) OptimizeRowsCount(Screen problem, Grid grid)
    {
        var bestEstimation = EstimateGrid(problem, grid);

        while (true)
        {
            var optimized = false;
            for (int i = 0; i < grid.Rows.Count; i++)
            {
                var copy = grid.Copy();
                var height = copy.Rows[i].Height;
                if (height < 2)
                    continue;
                var dh = height / 2;
                copy.Rows[i].Height -= dh;
                var newRow = copy.Rows[i].Copy();
                newRow.Height = dh;
                copy.Rows.Insert(i + 1, newRow);
                var (optimizedGrid, nextEstimation) = OptimizeRowHeights(problem, copy);
                if (nextEstimation < bestEstimation)
                {
                    bestEstimation = nextEstimation;
                    grid = optimizedGrid;
                    optimized = true;
                    break;
                }
            }

            if (!optimized)
                return (grid, bestEstimation);
        }
    }

    public static (Grid grid, double estimation) OptimizeCells(Screen problem, Grid grid, int rowIndex)
    {
        var bestEstimation = EstimateGrid(problem, grid);

        while (true)
        {
            var optimized = false;
            for (int i = 0; i < grid.Rows[rowIndex].Cells.Count; i++)
            {
                var copy = grid.Copy();
                var width = copy.Rows[rowIndex].Cells[i].Width;
                if (width < 2)
                    continue;
                var dw = width / 2;
                copy.Rows[rowIndex].Cells[i].Width -= dw;
                copy.Rows[rowIndex].Cells.Insert(i + 1, new GridCell(dw));
                var (optimizedGrid, nextEstimation) = OptimizeCellWidths(problem, copy, rowIndex);
                if (nextEstimation < bestEstimation)
                {
                    bestEstimation = nextEstimation;
                    grid = optimizedGrid;
                    optimized = true;
                    break;
                }
            }

            if (!optimized)
                return (grid, bestEstimation);
        }
    }

    public static (Grid grid, double estimation) OptimizeCellWidths(Screen problem, Grid grid)
    {
        double estimation = double.PositiveInfinity;
        for (int i = 0; i < grid.Rows.Count; i++)
            (grid, estimation) = OptimizeCellWidths(problem, grid, i);
        return (grid, estimation);
    }

    public static (Grid grid, double estimation) OptimizeCellWidths(Screen problem, Grid grid, int rowIndex)
    {
        (grid, _) = OptimizeCellWidths(problem, grid, rowIndex, 3);
        return OptimizeCellWidths(problem, grid, rowIndex, 1);
    }

    public static (Grid grid, double estimation) OptimizeRowHeights(Screen problem, Grid grid)
    {
        (grid, _) = OptimizeRowHeights(problem, grid, 3);
        return OptimizeRowHeights(problem, grid, 1);
    }

    public static (Grid grid, double estimation) OptimizeCellWidths(Screen problem, Grid grid, int rowIndex, int delta)
    {
        var bestEstimation = EstimateGrid(problem, grid);

        var cellsToOptimize = new HashSet<int>(Enumerable.Range(0, grid.Rows[rowIndex].Cells.Count - 1));

        while (true)
        {
            var optimized = false;

            var newCellsToOptimize = new HashSet<int>();

            for (int i = 0; i < grid.Rows[rowIndex].Cells.Count - 1; i++)
            {
                if (!cellsToOptimize.Contains(i))
                    continue;

                for (int d = -1; d <= 1; d += 2)
                {
                    var copy = grid.Copy();
                    copy.Rows[rowIndex].Cells[i].Width += d * delta;
                    copy.Rows[rowIndex].Cells[i + 1].Width -= d * delta;
                    if (copy.Rows[rowIndex].Cells[i].Width <= 0)
                        continue;
                    if (copy.Rows[rowIndex].Cells[i + 1].Width <= 0)
                        continue;
                    var nextEstimation = EstimateGrid(problem, copy);
                    if (nextEstimation < bestEstimation)
                    {
                        bestEstimation = nextEstimation;
                        grid = copy;
                        optimized = true;

                        newCellsToOptimize.Add(i - 1);
                        newCellsToOptimize.Add(i);
                        newCellsToOptimize.Add(i + 1);

                        break;
                    }
                }
            }

            cellsToOptimize = newCellsToOptimize;

            if (!optimized)
                return (grid, bestEstimation);
        }
    }

    public static (Grid grid, double estimation) OptimizeCellsViaMerge(Screen problem, Grid grid)
    {
        double estimation = double.PositiveInfinity;
        for (int i = 0; i < grid.Rows.Count; i++)
            (grid, estimation) = OptimizeCellsViaMerge(problem, grid, i);
        return (grid, estimation);
    }

    public static (Grid grid, double estimation) OptimizeCellsViaMerge(Screen problem, Grid grid, int rowIndex)
    {
        var bestEstimation = EstimateGrid(problem, grid);

        while (true)
        {
            var optimized = false;
            for (int i = 0; i < grid.Rows[rowIndex].Cells.Count - 1; i++)
            {
                var copy = grid.Copy();
                copy.Rows[rowIndex].Cells[i].Width += copy.Rows[rowIndex].Cells[i + 1].Width;
                copy.Rows[rowIndex].Cells.RemoveAt(i + 1);

                var (optimizedGrid, nextEstimation) = OptimizeCellWidths(problem, copy, rowIndex);
                if (nextEstimation < bestEstimation)
                {
                    bestEstimation = nextEstimation;
                    grid = optimizedGrid;
                    optimized = true;
                    break;
                }
            }

            if (!optimized)
                return (grid, bestEstimation);
        }
    }

    public static (Grid grid, double estimation) OptimizeRowHeights(Screen problem, Grid grid, int delta)
    {
        var bestEstimation = EstimateGrid(problem, grid);

        while (true)
        {
            var optimized = false;
            for (int i = 0; i < grid.Rows.Count - 1; i++)
            {
                for (int d = -1; d <= 1; d += 2)
                {
                    var copy = grid.Copy();
                    copy.Rows[i].Height += d * delta;
                    copy.Rows[i + 1].Height -= d * delta;
                    if (copy.Rows[i].Height <= 0)
                        continue;
                    if (copy.Rows[i + 1].Height <= 0)
                        continue;
                    var nextEstimation = EstimateGrid(problem, copy);
                    if (nextEstimation < bestEstimation)
                    {
                        bestEstimation = nextEstimation;
                        grid = copy;
                        optimized = true;
                        break;
                    }
                }
            }

            if (!optimized)
                return (grid, bestEstimation);
        }
    }

    public static Grid BuildRegularRows(Screen problem, int rowCount)
    {
        var rows = new List<GridRow>();
        var heightLeft = problem.Height;
        for (int i = 0; i < rowCount; i++)
        {
            var height = heightLeft / (rowCount - i);
            rows.Add(new GridRow(height, new List<GridCell> { new(problem.Width) }));
            heightLeft -= height;
        }

        return new Grid(rows);
    }

    public static Grid BuildRegularGrid(Screen problem, int rowCount, int cellCount)
    {
        var rows = new List<GridRow>();
        var heightLeft = problem.Height;
        for (int i = 0; i < rowCount; i++)
        {
            var height = heightLeft / (rowCount - i);

            var cells = new List<GridCell>();
            var widthLeft = problem.Width;
            for (int k = 0; k < cellCount; k++)
            {
                var width = widthLeft / (cellCount - k);
                cells.Add(new GridCell(width));
                widthLeft -= width;
            }

            rows.Add(new GridRow(height, cells));
            heightLeft -= height;
        }

        return new Grid(rows);
    }

    public static int estimations;

    public static double EstimateGrid(Screen problem, Grid grid)
    {
        Interlocked.Increment(ref estimations);
        var bottom = 0;
        var totalEstimation = 0.0;
        foreach (var row in grid.Rows)
        {
            var leftToRight = row.Cells[0].Width <= row.Cells.Last().Width;
            var left = 0;
            foreach (var cell in row.Cells)
            {
                var block = new SimpleBlock("", new V(left, bottom), new V(left + cell.Width, bottom + row.Height), new Rgba(0, 0, 0, 0));
                var color = problem.GetAverageColor(block);
                block = block with { Color = color };
                var similarity = problem.DiffTo(block);
                var estimation = leftToRight
                    ? similarity + 7*400.0 * 400 / ((400 - block.Left)*(400 - block.Bottom))
                    : similarity + 7*400.0 * 400 / (block.Right*(400 - block.Bottom));
                totalEstimation += estimation;
                left += cell.Width;
            }

            bottom += row.Height;
        }

        return totalEstimation;
    }
}
