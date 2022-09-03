using System.Collections.Generic;

namespace lib.Algorithms;

public class Grid
{
    public Grid(List<GridRow> rows)
    {
        Rows = rows;
    }

    public List<GridRow> Rows;

}

public class GridRow
{
    public GridRow(int height, List<GridCell> cells)
    {
        Height = height;
        Cells = cells;
    }

    public int Height;
    public List<GridCell> Cells;
}

public class GridCell
{
    public GridCell(int width)
    {
        Width = width;
    }

    public int Width;
}
