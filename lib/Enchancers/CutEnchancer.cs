using System.Collections.Generic;
using System.Linq;

namespace lib.Enchancers;

public class CutEnchancer : ISolutionEnchancer
{
    private const int PCUT_DELTA = 3;
    private const int LCUT_DELTA = 3;

    public List<Move> Enchance(Screen problem, List<Move> moves)
    {
        var cutIndexes = moves.Select((move, i) => new { move, i }).Where(x => x.move is CutMove).Select(x => x.i).ToArray();
        if (!cutIndexes.Any())
            return moves;

        var bestScore = GetScore(problem, moves);

        while (true)
        {
            var bestCutIndex = -1;
            Move? bestCut = null;

            var canvas = new Canvas(problem);
            foreach (var cutIndex in cutIndexes)
            {
                var copy = canvas.Copy();
                ApplyRange(copy, moves, 0, cutIndex - 1);
                foreach (var cut in IterateCuts(copy, moves[cutIndex]))
                {
                    var copy2 = copy.Copy();
                    copy2.Apply(cut);
                    ApplyRange(copy2, moves, cutIndex + 1, moves.Count - 1);
                    var score = copy2.GetScore(problem);
                    if (score < bestScore)
                    {
                        bestScore = score;
                        bestCutIndex = cutIndex;
                        bestCut = cut;
                    }
                }
            }

            if (bestCut == null)
                return moves;

            moves = moves.ToList();
            moves[bestCutIndex] = bestCut;
            return moves;
        }
    }

    private IEnumerable<Move> IterateCuts(Canvas canvas, Move move)
    {
        var cut = (CutMove)move;
        var block = canvas.Blocks[cut.BlockId];
        if (cut is HCutMove hCut)
        {
            for (int dx = -LCUT_DELTA; dx <= LCUT_DELTA; dx++)
            {
                var x = hCut.LineNumber + dx;
                if (x > block.Left && x < block.Right)
                    yield return hCut with { LineNumber = x };
            }
        }
        else if (cut is VCutMove vCut)
        {
            for (int dy = -LCUT_DELTA; dy <= LCUT_DELTA; dy++)
            {
                var y = vCut.LineNumber + dy;
                if (y > block.Bottom && y < block.Top)
                    yield return vCut with { LineNumber = y };
            }
        }
        else if (cut is PCutMove pCut)
        {
            for (int dx = -PCUT_DELTA; dx <= PCUT_DELTA; dx++)
            for (int dy = -PCUT_DELTA; dy <= PCUT_DELTA; dy++)
            {
                var v = pCut.Point + new V(dx, dy);
                if (v.IsStrictlyInside(block))
                    yield return pCut with { Point = v };
            }
        }
    }

    private int GetScore(Screen problem, List<Move> moves)
    {
        var canvas = new Canvas(problem);
        ApplyRange(canvas, moves, 0, moves.Count - 1);
        return canvas.GetScore(problem);
    }

    private void ApplyRange(Canvas canvas, List<Move> moves, int start, int end)
    {
        for (var i = start; i <= end; i++)
        {
            var move = moves[i];
            canvas.Apply(move);
        }
    }
}
