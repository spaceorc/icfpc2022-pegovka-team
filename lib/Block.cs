using System;
using System.Collections.Generic;

namespace lib;

public abstract record Block
{
    public string Id { get; set; }
    public V BottomLeft { get; set; }
    public V TopRight { get; set; }

    protected Block(string id, V bottomLeft, V topRight)
    {
        Id = id;
        BottomLeft = bottomLeft;
        TopRight = topRight;
        if (bottomLeft.X >= topRight.X || bottomLeft.Y >= topRight.Y)
            throw new Exception($"Bad block {bottomLeft} {topRight}");
    }

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
