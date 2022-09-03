using System;
using System.Collections.Generic;

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
        var bl = V.Zero;
        foreach (var gridRow in grid.Rows)
        {
        }
        throw new NotImplementedException();
    }

}
