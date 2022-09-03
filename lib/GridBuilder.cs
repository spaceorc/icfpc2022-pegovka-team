using System;
using System.Collections.Generic;
using lib.Algorithms;

namespace lib;

public static class GridBuilder
{
    public static Grid BuildOptimalGrid(Screen problem, Func<SimpleBlock, double, double> estimateBlock)
    {
        var grid = BuildRegularRows(problem, 10);
        (grid, _) = OptimizeRowsCount(problem, grid, estimateBlock);

        var (optimizedGrid, _) = OptimizeGrid(problem, grid, estimateBlock);

        return optimizedGrid;
    }

    public static (Grid grid, double estimation) OptimizeGrid(Screen problem, Grid grid, Func<SimpleBlock, double, double> estimateBlock)
    {
        var bestEstimation = EstimateGrid(problem, grid, estimateBlock);

        while (true)
        {
            var (gr, _) = OptimizeRowHeights(problem, grid, estimateBlock);
            var (optimizedGrid, nextEstimation) = OptimizeAllGridRows(problem, gr, estimateBlock);
            if (nextEstimation >= bestEstimation)
                return (grid, bestEstimation);

            bestEstimation = nextEstimation;
            grid = optimizedGrid;

        }
    }

    public static (Grid grid, double estimation) OptimizeAllGridRows(Screen problem, Grid grid, Func<SimpleBlock, double, double> estimateBlock)
    {
        var bestEstimation = EstimateGrid(problem, grid, estimateBlock);

        while (true)
        {
            var optimized = false;
            for (int i = 0; i < grid.Rows.Count; i++)
            {
                var (copy, estimation) = OptimizeCells(problem, grid, i, estimateBlock);
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

    public static (Grid grid, double estimation) OptimizeRowsCount(Screen problem, Grid grid, Func<SimpleBlock, double, double> estimateBlock)
    {
        var bestEstimation = EstimateGrid(problem, grid, estimateBlock);

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
                var (optimizedGrid, nextEstimation) = OptimizeRowHeights(problem, copy, estimateBlock);
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

    public static (Grid grid, double estimation) OptimizeCells(Screen problem, Grid grid, int rowIndex, Func<SimpleBlock, double, double> estimateBlock)
    {
        var bestEstimation = EstimateGrid(problem, grid, estimateBlock);

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
                var (optimizedGrid, nextEstimation) = OptimizeCellWidths(problem, copy, rowIndex, estimateBlock);
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

    public static (Grid grid, double estimation) OptimizeCellWidths(Screen problem, Grid grid, int rowIndex, Func<SimpleBlock, double, double> estimateBlock)
    {
        (grid, _) = OptimizeCellWidths(problem, grid, rowIndex, 128, estimateBlock);
        (grid, _) = OptimizeCellWidths(problem, grid, rowIndex, 64, estimateBlock);
        (grid, _) = OptimizeCellWidths(problem, grid, rowIndex, 32, estimateBlock);
        (grid, _) = OptimizeCellWidths(problem, grid, rowIndex, 16, estimateBlock);
        (grid, _) = OptimizeCellWidths(problem, grid, rowIndex, 8, estimateBlock);
        (grid, _) = OptimizeCellWidths(problem, grid, rowIndex, 4, estimateBlock);
        (grid, _) = OptimizeCellWidths(problem, grid, rowIndex, 2, estimateBlock);
        return OptimizeCellWidths(problem, grid, rowIndex, 1, estimateBlock);
    }

    public static (Grid grid, double estimation) OptimizeRowHeights(Screen problem, Grid grid, Func<SimpleBlock, double, double> estimateBlock)
    {
        (grid, _) = OptimizeRowHeights(problem, grid, 3, estimateBlock);
        return OptimizeRowHeights(problem, grid, 1, estimateBlock);
    }

    public static (Grid grid, double estimation) OptimizeCellWidths(Screen problem, Grid grid, int rowIndex, int delta, Func<SimpleBlock, double, double> estimateBlock)
    {
        var bestEstimation = EstimateGrid(problem, grid, estimateBlock);

        while (true)
        {
            var optimized = false;
            for (int i = 0; i < grid.Rows[rowIndex].Cells.Count - 1; i++)
            {
                for (int d = -1; d <= 1; d += 2)
                {
                    var copy = grid.Copy();
                    copy.Rows[rowIndex].Cells[i].Width += d * delta;
                    copy.Rows[rowIndex].Cells[i + 1].Width -= d * delta;
                    if (copy.Rows[rowIndex].Cells[i].Width <= 0)
                        continue;
                    if (copy.Rows[rowIndex].Cells[i + 1].Width <= 0)
                        continue;
                    var nextEstimation = EstimateGrid(problem, copy, estimateBlock);
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

    public static (Grid grid, double estimation) OptimizeRowHeights(Screen problem, Grid grid, int delta, Func<SimpleBlock, double, double> estimateBlock)
    {
        var bestEstimation = EstimateGrid(problem, grid, estimateBlock);

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
                    var nextEstimation = EstimateGrid(problem, copy, estimateBlock);
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

    public static double EstimateGrid(Screen problem, Grid grid, Func<SimpleBlock, double, double> estimateBlock)
    {
        var bottom = 0;
        var totalEstimation = 0.0;
        foreach (var row in grid.Rows)
        {
            var left = 0;
            foreach (var cell in row.Cells)
            {
                var block = new SimpleBlock("", new V(left, bottom), new V(left + cell.Width, bottom + row.Height), new Rgba(0, 0, 0, 0));
                var color = problem.GetAverageColor(block);
                block = block with { Color = color };
                var similarity = problem.DiffTo(block);
                var estimation = estimateBlock(block, similarity);
                totalEstimation += estimation;
                left += cell.Width;
            }

            bottom += row.Height;
        }

        return totalEstimation;
    }
}
