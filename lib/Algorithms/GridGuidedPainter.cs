using System.Collections.Generic;
using System.Linq;

namespace lib.Algorithms;

public class GridGuidedPainter
{
    private readonly Grid grid;
    private readonly Screen screen;
    private readonly Canvas canvas;
    private readonly List<Move> moves;

    public GridGuidedPainter(Grid grid, Screen screen)
    {
        this.grid = grid;
        this.screen = screen;
        canvas = new Canvas(screen);
        moves = new List<Move>();
    }

    public (IList<Move>, double Score) GetBestResult()
    {
        var start = V.Zero;
        var rowBottomLeft = start;
        foreach (var gridRow in grid.Rows)
        {
            var cellBottomLeft = rowBottomLeft;
            PaintRow(gridRow, cellBottomLeft);
            rowBottomLeft += V.Down * gridRow.Height;
        }
        return (moves, canvas.GetScore(screen));
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
        if (bottomLeft.Y == 0)
        {
            if (bottomLeft.X == 0)
                PaintInitialCell(cell, height);
            else
                PaintBottomCell(cell, bottomLeft, height);
        }
        else
        {
            if (bottomLeft.X == 0)
                PaintFirstCellInRow(cell, bottomLeft, height);
            else
                PaintInnerCell(cell, bottomLeft, height);
        }
    }

    private void PaintInnerCell(GridCell cell, V bottomLeft, int height)
    {
        var pCut = new PCutMove(Block.Id, bottomLeft);
        var (bottomLeftBlock, bottomRightBlock, topRightBlock, topLeftBlock) = canvas.ApplyPCut(pCut);
        moves.Add(pCut);
        DoColor(cell.Width, height, topRightBlock);
        var rightBlock = DoMerge(topRightBlock, bottomRightBlock);
        var leftBlock = DoMerge(topLeftBlock, bottomLeftBlock);
        DoMerge(rightBlock, leftBlock);
    }

    private void PaintFirstCellInRow(GridCell cell, V bottomLeft, int height)
    {
        var hCut = new HCutMove(Block.Id, bottomLeft.Y);
        var (bottom, top) = canvas.ApplyHCut(hCut);
        moves.Add(hCut);
        DoColor(cell.Width, height, top);
        DoMerge(bottom, top);
    }

    private void PaintBottomCell(GridCell cell, V bottomLeft, int height)
    {
        var vCut = new VCutMove(Block.Id, bottomLeft.X);
        var (left, right) = canvas.ApplyVCut(vCut);
        moves.Add(vCut);
        DoColor(cell.Width, height, right);
        DoMerge(left, right);
    }

    private ComplexBlock DoMerge(Block left, Block right)
    {
        var merge = new MergeMove(left.Id, right.Id);
        moves.Add(merge);
        return canvas.ApplyMerge(merge);
    }

    private void PaintInitialCell(GridCell cell, int height)
    {
        DoColor(cell.Width, height, Block);
    }

    private void DoColor(int width, int height, Block block)
    {
        var color = screen.GetAverageColorByGeometricMedian(block.Left, block.Bottom, width, height);
        var colorMove = new ColorMove(block.Id, color);
        moves.Add(colorMove);
        canvas.ApplyColor(colorMove);
    }

    public Block Block => canvas.Blocks.Values.Single();
}
