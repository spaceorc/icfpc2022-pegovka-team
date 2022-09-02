using SixLabors.ImageSharp.ColorSpaces;
using static Yandex.Cloud.Mdb.Clickhouse.V1.Config.ClickhouseConfig.Types.ExternalDictionary.Types.Structure.Types;

namespace lib;

public record Block(string Id, V BottomLeft, V TopRight);
public record SimpleBlock(string Id, V BottomLeft, V TopRight, Rgba Color) : Block(Id, BottomLeft, TopRight);
public record ComplexBlock(string Id, V BottomLeft, V TopRight, SimpleBlock[] Children) : Block(Id, BottomLeft, TopRight);
