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
    public GridRow(int height, List<GridCell> widths)
    {
        Height = height;
        Widths = widths;
    }

    public int Height;
    public List<GridCell> Widths;
}

public class GridCell
{
    public GridCell(int width, int similarityCost)
    {
        Width = width;
        SimilarityCost = similarityCost;
    }

    public int Width;
    public int SimilarityCost;
}
