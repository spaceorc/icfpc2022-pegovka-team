using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;

namespace lib.Algorithms.RectBiter;


public class BiterState
{
    public HashSet<string> FixedBlockIds;
    public readonly Screen Screen;
    public Canvas Canvas;

    public BiterState(Canvas canvas, HashSet<string> fixedBlockIds, Screen screen)
    {
        Canvas = canvas;
        FixedBlockIds = fixedBlockIds;
        this.Screen = screen;
    }

    public BiterState Clone()
    {
        return new BiterState(Canvas.Copy(), FixedBlockIds.ToHashSet(), Screen);
    }

    public void Apply(SolutionPart part)
    {
        foreach (var move in part.Moves)
            Canvas.Apply(move);

        foreach (var blockId in part.BlocksToFix)
            FixedBlockIds.Add(blockId);
    }
}


public class SolutionPart
{
    public SolutionPart(List<Move> moves, List<string> blocksToFix)
    {
        Moves = moves;
        BlocksToFix = blocksToFix;
    }

    public List<Move> Moves;
    public List<string> BlocksToFix;
    public override string ToString()
    {
        return $"{Moves.StrJoin(";")} {BlocksToFix.StrJoin(";")}";
    }
}

public class GreedyRectBiter
{
    private readonly Random random;
    private Dictionary<string, double> blockPenalties = new Dictionary<string, double>();

    public GreedyRectBiter(Random random)
    {
        this.random = random;
    }

    public (Move[] moves, long score) Solve(BiterState state, int countdownPerSolutionPart)
    {
        var moves = new List<Move>();
        var minPenalty = long.MaxValue;
        var bestMovesCount = 0;
        while (!state.Canvas.Blocks.Keys.All(state.FixedBlockIds.Contains))
        {
            var part = SearchBestSolutionPart(state, countdownPerSolutionPart);
            state.Apply(part);
            moves.AddRange(part.Moves);
            var penalty = state.Canvas.GetScore(state.Screen);
            if (penalty < minPenalty)
            {
                minPenalty = penalty;
                bestMovesCount = moves.Count;
            }
        }

        return (moves.Take(bestMovesCount).ToArray(), minPenalty);
    }

    public SolutionPart SearchBestSolutionPart(BiterState state, Countdown countdown)
    {
        var blocksToFix = FindBlocksToFix(state).ToList();
        if (blocksToFix.Any()) return new SolutionPart(new List<Move>(), blocksToFix);
        var minPenalty = double.PositiveInfinity;
        SolutionPart? bestPart = null;
        var count = 0;
        while (!countdown.IsFinished())
        {
            var part = GenerateRandomSolutionPart(state);
            var stateCopy = state.Clone();
            stateCopy.Apply(part);
            var penalty = GetStatePenalty(stateCopy);
            if (penalty < minPenalty)
            {
                //Console.WriteLine(penalty + " " + part);
                minPenalty = penalty;
                bestPart = part;
            }
            count++;
        }
        //Console.WriteLine(count + " sims");


        return bestPart ?? throw new Exception("Need more time");
    }

    private static IEnumerable<string> FindBlocksToFix(BiterState state)
    {
        var colorWithHCutBaseCost = 5 + 7;

        foreach (var aliveBlock in state.Canvas.Blocks.Where(b => !state.FixedBlockIds.Contains(b.Key)))
        {
            if (aliveBlock.Value.Height <= 2 || aliveBlock.Value.Width <= 2)
                yield return aliveBlock.Key;
            else
            {
                var similarityPenalty = state.Screen.DiffTo(aliveBlock.Value);
                var moveCost = Move.GetCost(state.Canvas.ScalarSize, aliveBlock.Value.ScalarSize, colorWithHCutBaseCost);
                if (similarityPenalty <= moveCost)
                    yield return aliveBlock.Key;
            }
        }
    }

    private double GetStatePenalty(BiterState state)
    {
        var fixedBlocks = state.FixedBlockIds.Select(id => state.Canvas.Blocks[id]).ToList();
        var fixedSimilarityPenalty = fixedBlocks.Sum(b => blockPenalties.GetOrCreate(b.Id, id => state.Screen.DiffTo(b)));
        var movesCost = state.Canvas.TotalCost;
        var fixedPixelsCount = fixedBlocks.Sum(b => b.ScalarSize);
        return movesCost + fixedSimilarityPenalty;
    }

    private SolutionPart GenerateRandomSolutionPart(BiterState state)
    {
        SolutionPart CreateSolutionPart(Block a, Block b, string blockId, Move cut)
        {
            var colorA = state.Screen.GetAverageColor(a);
            var colorB = state.Screen.GetAverageColor(b);
            var penaltyA = state.Screen.DiffTo(a.BottomLeft, a.TopRight, colorA) / a.ScalarSize;
            var penaltyB = state.Screen.DiffTo(b.BottomLeft, b.TopRight, colorB) / b.ScalarSize;
            return penaltyA <= penaltyB
                ? new SolutionPart(new List<Move> { new ColorMove(blockId, colorA), cut }, new List<string> { a.Id })
                : new SolutionPart(new List<Move> { new ColorMove(blockId, colorB), cut }, new List<string> { b.Id });
        }

        var activeBlocks = state.Canvas.Blocks.Keys.Where(id => !state.FixedBlockIds.Contains(id)).ToList();
        var blockId = random.SelectOne(activeBlocks);
        var block = state.Canvas.Blocks[blockId];
        var caseIndex = random.Next(3);
        if (caseIndex == 0)
        {
            var y = random.Next(block.BottomLeft.Y+1, block.TopRight.Y-1);
            var hCut = new HCutMove(blockId, y);
            var (a, b) = Canvas.PreApplyHCut(block, y);
            return CreateSolutionPart(a, b, blockId, hCut);
        }
        if (caseIndex == 1)
        {
            var x = random.Next(block.BottomLeft.X+1, block.TopRight.X-1);
            var vCut = new VCutMove(blockId, x);
            var (a, b) = Canvas.PreApplyVCut(block, x);
            return CreateSolutionPart(a, b, blockId, vCut);
        }
        else
        {
            var x = random.Next(block.BottomLeft.X + 1, block.TopRight.X - 1);
            var y = random.Next(block.BottomLeft.Y + 1, block.TopRight.Y - 1);
            var pCut = new PCutMove(blockId, new V(x, y));
            var (a, b, c, d) = Canvas.PreApplyPCut(block, new V(x, y));
            var blocks = new[] { a, b, c, d };
            var minBlock = blocks.MinBy(subBlock => subBlock.ScalarSize);
            var color = state.Screen.GetAverageColor(minBlock);
            return new SolutionPart(new List<Move> { new ColorMove(block.Id, color), pCut }, new List<string> { minBlock.Id });
        }
    }
}
