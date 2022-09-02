using System;

namespace lib;

public record Move(int BaseCost)
{
    public int GetCost(int canvasSize, int blockSize)
    {
        return (int)Math.Round((double)BaseCost * canvasSize / blockSize);
    }
}

public record NopMove() : Move(0);

public record ColorMove(string BlockId, Rgba Color) : Move(5)
{
    public override string ToString() => $"color [{BlockId}] {Color}";
}

public record PCutMove(string BlockId, V Point) : Move(10)
{
    public override string ToString() => $"cut [{BlockId}] {Point}";
}

public record HCutMove(string BlockId, int LineNumber) : Move(7)
{
    public override string ToString() => $"cut [{BlockId}] [y] [{LineNumber}]";
}

public record VCutMove(string BlockId, int LineNumber) : Move(7)
{
    public override string ToString() => $"cut [{BlockId}] [x] [{LineNumber}]";
}

public record SwapMove(string Block1Id, string Block2Id) : Move(3)
{
    public override string ToString() => $"swap [{Block1Id}] [{Block2Id}]";
}

public record MergeMove(string Block1Id, string Block2Id) : Move(1)
{
    public override string ToString() => $"merge [{Block1Id}] [{Block2Id}]";
}
