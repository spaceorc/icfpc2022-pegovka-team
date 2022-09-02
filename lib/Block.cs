using System.Collections.Generic;

namespace lib;

public abstract record Block(string Id, V BottomLeft, V TopRight)
{
    public V Size => TopRight - BottomLeft;
    public int Height => TopRight.Y - BottomLeft.Y;
    public int Width => TopRight.X - BottomLeft.X;
    public int Left => BottomLeft.X;
    public int Right => TopRight.X;
    public int Bottom => BottomLeft.Y;
    public int Top => TopRight.Y;
    public int ScalarSize => Size.GetScalarSize();
    public abstract IEnumerable<SimpleBlock> GetChildren();
}

public record SimpleBlock(string Id, V BottomLeft, V TopRight, Rgba Color) : Block(Id, BottomLeft, TopRight)
{
    public override IEnumerable<SimpleBlock> GetChildren() => new[] { this };
}

public record ComplexBlock(string Id, V BottomLeft, V TopRight, SimpleBlock[] Children) : Block(Id, BottomLeft, TopRight)
{
    public override IEnumerable<SimpleBlock> GetChildren() => Children;
}
