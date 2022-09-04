﻿using System.Collections.Generic;
using System.Linq;

namespace lib.Algorithms;

public class GridGuidedPainter
{
    private readonly Grid grid;
    private readonly Screen screen;
    private readonly Canvas canvas;
    private readonly List<Move> moves;
    private double colorTolerance;

    public GridGuidedPainter(Grid grid, Screen screen, double colorTolerance = 0)
    {
        this.grid = grid;
        this.screen = screen;
        this.colorTolerance = colorTolerance;
        canvas = new Canvas(screen);
        moves = new List<Move>();
    }

    public (List<Move> moves, int) GetBestResult()
    {
        var (ms, score, _) = GetBestResultWithCanvas();
        return (ms, score);
    }

    public (List<Move> moves, int, Canvas canvas) GetBestResultWithCanvas()
    {
        var start = V.Zero;
        var rowBottomLeft = start;
        foreach (var gridRow in grid.Rows)
        {
            var cellBottomLeft = rowBottomLeft;
            PaintRow(gridRow, cellBottomLeft);
            rowBottomLeft += V.Down * gridRow.Height;
        }
        return (moves, canvas.GetScore(screen), canvas);
    }

    private void PaintRow(GridRow gridRow, V cellBottomLeft)
    {
        foreach (var cell in gridRow.Cells)
        {
            PaintGridCell(cell, cellBottomLeft, gridRow.Height);
            cellBottomLeft += V.Right * cell.Width;
        }
    }

    private void PaintGridCell(GridCell cell, V bottomLeft, int height)
    {
        var color = screen.GetAverageColorByGeometricMedian(bottomLeft.X, bottomLeft.Y, cell.Width, height);
        if (Block.IsFilledWithColor(color, bottomLeft, cell.Width, height, colorTolerance)) return;
        if (bottomLeft.Y == 0)
        {
            if (bottomLeft.X == 0)
                PaintInitialCell(color);
            else
                PaintBottomCell(bottomLeft, color);
        }
        else
        {
            if (bottomLeft.X == 0)
                PaintFirstCellInRow(bottomLeft, color);
            else
                PaintInnerCell(bottomLeft, color);
        }
    }

    private void PaintInnerCell(V bottomLeft, Rgba color)
    {
        var (bottomLeftBlock, bottomRightBlock, topRightBlock, topLeftBlock) = DoPCut(Block, bottomLeft);
        DoColor(topRightBlock, color);
        var rightBlock = DoMerge(topRightBlock, bottomRightBlock);
        var leftBlock = DoMerge(topLeftBlock, bottomLeftBlock);
        DoMerge(rightBlock, leftBlock);
    }

    private (Block bottomLeftBlock, Block bottomRightBlock, Block topRightBlock, Block topLeftBlock) DoPCut(Block block, V bottomLeft)
    {
        var pCut = new PCutMove(block.Id, bottomLeft);
        var (bottomLeftBlock, bottomRightBlock, topRightBlock, topLeftBlock) = canvas.ApplyPCut(pCut);
        moves.Add(pCut);
        return (bottomLeftBlock, bottomRightBlock, topRightBlock, topLeftBlock);
    }

    private void PaintFirstCellInRow(V bottomLeft, Rgba color)
    {
        var (bottom, top) = DoHCut(Block, bottomLeft);
        DoColor(top, color);
        DoMerge(bottom, top);
    }

    private void PaintBottomCell(V bottomLeft, Rgba color)
    {
        var (left, right) = DoVCut(Block, bottomLeft);
        DoColor(right, color);
        DoMerge(left, right);
    }

    private void PaintInitialCell(Rgba color)
    {
        DoColor(Block, color);
    }

    private (Block left, Block right) DoVCut(Block block, V bottomLeft)
    {
        var vCut = new VCutMove(block.Id, bottomLeft.X);
        var (left, right) = canvas.ApplyVCut(vCut);
        moves.Add(vCut);
        return (left, right);
    }

    private (Block bottom, Block top) DoHCut(Block block, V bottomLeft)
    {
        var hCut = new HCutMove(block.Id, bottomLeft.Y);
        moves.Add(hCut);
        return canvas.ApplyHCut(hCut);
    }

    private ComplexBlock DoMerge(Block left, Block right)
    {
        var merge = new MergeMove(left.Id, right.Id);
        moves.Add(merge);
        return canvas.ApplyMerge(merge);
    }

    private void DoColor(Block block, Rgba color)
    {
        var colorMove = new ColorMove(block.Id, color);
        moves.Add(colorMove);
        canvas.ApplyColor(colorMove);
    }

    private Block Block => canvas.Blocks.Values.Single();
}
