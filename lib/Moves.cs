namespace lib;

public record ColorMove(string BlockId, Rgba Color)
{
    public override string ToString() => $"color[{BlockId}]{Color}";
}

public record PCutMove(string BlockId, V Point)
{
    public override string ToString() => $"cut[{BlockId}]{Point}";
}

public enum Orientation
{
    /// <summary>Vertical</summary>
    X,
    /// <summary>Horizontal</summary>
    Y
}
public record LCutMove(string BlockId, Orientation Orientation, int LineNumber)
{
    public override string ToString() => $"cut[{BlockId}][{Orientation}][{LineNumber}]";
}

public record SwapMove(string Block1Id, string Block2Id)
{
    public override string ToString() => $"swap[{Block1Id}][{Block2Id}]";
}

public record MergeMove(string Block1Id, string Block2Id)
{
    public override string ToString() => $"merge[{Block1Id}][{Block2Id}]";
}
