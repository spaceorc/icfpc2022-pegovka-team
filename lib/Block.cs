using System;
using System.Collections.Generic;
using System.Linq;

namespace lib;

public class BadBlockException : Exception
{
    public BadBlockException(string message)
        : base(message)
    {
    }
}

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
            throw new BadBlockException($"Bad block {bottomLeft} {topRight}");
    }

    public V Size => TopRight - BottomLeft;
    public int Height => TopRight.Y - BottomLeft.Y;
    public int Width => TopRight.X - BottomLeft.X;
    public int Left => BottomLeft.X;
    public int Right => TopRight.X;
    public int Bottom => BottomLeft.Y;
    public int Top => TopRight.Y;
    public int ScalarSize => Size.GetScalarSize();
    public bool IntersectsWith(V bottomLeft, int width, int height)
    {
        var left = Math.Max(bottomLeft.X, Left);
        var right = Math.Min(bottomLeft.X+width, Right);
        var bottom = Math.Max(bottomLeft.Y, Bottom);
        var top = Math.Min(bottomLeft.Y+height, Top);
        return left < right && bottom < top;
    }
    public abstract IEnumerable<SimpleBlock> GetChildren();
    public Block MoveTo(Block other) => MoveTo(other.BottomLeft, other.TopRight);
    public abstract Block MoveTo(V bottomLeft, V topRight);

    public abstract bool IsFilledWithColor(Rgba color, V bottomLeft, int width, int height, double colorTolerance);
}

public record SimpleBlock(string Id, V BottomLeft, V TopRight, Rgba Color) : Block(Id, BottomLeft, TopRight)
{
    public override IEnumerable<SimpleBlock> GetChildren() => new[] { this };

    public override Block MoveTo(V bottomLeft, V topRight)
    {
        if (topRight - bottomLeft != Size)
            throw new Exception("topRight - bottomLeft != Size");

        return this with {BottomLeft = bottomLeft, TopRight = topRight};
    }
    public override bool IsFilledWithColor(Rgba color, V bottomLeft, int width, int height, double colorTolerance) => color.DiffTo(Color) <= colorTolerance;
}

public record ComplexBlock(string Id, V BottomLeft, V TopRight, SimpleBlock[] Children) : Block(Id, BottomLeft, TopRight)
{
    public override IEnumerable<SimpleBlock> GetChildren() => Children;

    public override Block MoveTo(V bottomLeft, V topRight)
    {
        if (topRight - bottomLeft != Size)
            throw new Exception("topRight - bottomLeft != Size");

        var diff = bottomLeft - BottomLeft;
        return this with
        {
            BottomLeft = bottomLeft,
            TopRight = topRight,
            Children = Children.Select(x => (SimpleBlock)x.MoveTo(x.BottomLeft + diff, x.TopRight + diff)).ToArray()
        };
    }
    public override bool IsFilledWithColor(Rgba color, V bottomLeft, int width, int height, double colorTolerance)
    {
        return !Children.Any(child => child.IntersectsWith(bottomLeft, width, height) && child.Color.DiffTo(color) > colorTolerance);
    }
}
