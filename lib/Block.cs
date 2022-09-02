using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SixLabors.ImageSharp.ColorSpaces;
using static Yandex.Cloud.Mdb.Clickhouse.V1.Config.ClickhouseConfig.Types.ExternalDictionary.Types.Structure.Types;

namespace lib;

public abstract record Block(string Id, V BottomLeft, V TopRight)
{
    public V Size => TopRight - BottomLeft;
    public int Height => TopRight.Y - BottomLeft.Y;
    public int Width => TopRight.X - BottomLeft.X;
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
