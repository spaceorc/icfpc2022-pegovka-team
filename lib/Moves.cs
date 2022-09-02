using System;

namespace lib;

public abstract record Move(int BaseCost)
{
    public int GetCost(int canvasSize, int blockSize)
    {
        return (int)Math.Round((double)BaseCost * canvasSize / blockSize);
    }

    public int GetCost(Canvas canvas)
    {
        return GetCost(canvas.ScalarSize, GetBlockScalarSize(canvas));
    }

    protected abstract int GetBlockScalarSize(Canvas canvas);
}

public record NopMove() : Move(0)
{
    protected override int GetBlockScalarSize(Canvas canvas) => 1;

    public override string ToString()
    {
        return "";
    }
}

public record ColorMove(string BlockId, Rgba Color) : Move(5)
{
    protected override int GetBlockScalarSize(Canvas canvas)
    {
        return canvas.Blocks[BlockId].ScalarSize;
    }

    public override string ToString() => $"color [{BlockId}] {Color}";
}

public record PCutMove(string BlockId, V Point) : Move(10)
{
    protected override int GetBlockScalarSize(Canvas canvas)
    {
        return canvas.Blocks[BlockId].ScalarSize;
    }

    public override string ToString() => $"cut [{BlockId}] {Point}";
}

public record HCutMove(string BlockId, int LineNumber) : Move(7)
{
    protected override int GetBlockScalarSize(Canvas canvas)
    {
        return canvas.Blocks[BlockId].ScalarSize;
    }

    public override string ToString() => $"cut [{BlockId}] [y] [{LineNumber}]";
}

public record VCutMove(string BlockId, int LineNumber) : Move(7)
{
    protected override int GetBlockScalarSize(Canvas canvas)
    {
        return canvas.Blocks[BlockId].ScalarSize;
    }

    public override string ToString() => $"cut [{BlockId}] [x] [{LineNumber}]";
}

public record SwapMove(string Block1Id, string Block2Id) : Move(3)
{
    protected override int GetBlockScalarSize(Canvas canvas)
    {
        return canvas.Blocks[Block1Id].ScalarSize;
    }

    public override string ToString() => $"swap [{Block1Id}] [{Block2Id}]";
}

public record MergeMove(string Block1Id, string Block2Id) : Move(1)
{
    protected override int GetBlockScalarSize(Canvas canvas)
    {
        return Math.Max(canvas.Blocks[Block1Id].ScalarSize, canvas.Blocks[Block2Id].ScalarSize);
    }

    public override string ToString() => $"merge [{Block1Id}] [{Block2Id}]";
}
