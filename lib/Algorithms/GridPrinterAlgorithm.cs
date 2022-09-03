using System;
using System.Collections.Generic;
using System.Linq;

namespace lib.Algorithms;

public class GridPrinterAlgorithm : IAlgorithm
{
    private readonly int cellSize;

    public GridPrinterAlgorithm(int cellSize)
    {
        this.cellSize = cellSize;
    }

    public class State
    {
        public State(Screen screen, int colSize)
        {
            Screen = screen;
            Canvas = new Canvas(screen);
            MainBlock = Canvas.Blocks.Values.Single();
            BlocksToMerge = new List<Block>();
            CurrentColSize = colSize;
        }

        public State(Canvas canvas, Screen screen, Block mainBlock, List<Block> blocksToMerge, int currentColSize)
        {
            Canvas = canvas;
            Screen = screen;
            MainBlock = mainBlock;
            BlocksToMerge = blocksToMerge;
            CurrentColSize = currentColSize;
        }

        public Canvas Canvas;
        public Screen Screen;
        public Block? MainBlock;
        public List<Block> BlocksToMerge;
        public int CurrentColSize;

        public IEnumerable<Move> CutCell(V cutPoint)
        {
            var mainBlock = MainBlock!;
            if (cutPoint.IsStrictlyInside(mainBlock))
            {
                var (bl, br, tl, tr) = Canvas.PreApplyPCut(mainBlock, cutPoint);
                var color = Screen.GetAverageColorByGeometricMedian(bl);
                var colorMove = new ColorMove(mainBlock.Id, color);
                var pCutMove = new PCutMove(mainBlock.Id, cutPoint);
                var mergeMove = new MergeMove(tl.Id, tr.Id);
                Canvas.Apply(colorMove);
                var (_, bottomRightBlock, _, _) = Canvas.ApplyPCut(pCutMove);
                MainBlock = Canvas.ApplyMerge(mergeMove);
                BlocksToMerge.Add(bottomRightBlock);
                //TODO do not color same color block
                yield return colorMove;
                yield return pCutMove;
                yield return mergeMove;
            }
            else
            {
                var (leftBlock, rightBlock) = Canvas.PreApplyVCut(mainBlock, cutPoint.X);
                var color = Screen.GetAverageColorByGeometricMedian(leftBlock);
                var colorMove = new ColorMove(mainBlock.Id, color);
                var vCutMove = new VCutMove(mainBlock.Id, cutPoint.X);
                Canvas.Apply(colorMove);
                var (_, right) = Canvas.ApplyVCut(vCutMove);
                MainBlock = null;
                BlocksToMerge.Add(right);
                yield return colorMove;
                yield return vCutMove;
            }
        }

        public bool ShouldStartNextColumn()
        {
            return MainBlock != null && MainBlock.Width > CurrentColSize;
        }

        public IEnumerable<Move> MergeRibbons()
        {
            MainBlock = BlocksToMerge[0];
            foreach (var nextRibbon in BlocksToMerge.Skip(1))
            {
                var mergeMove = new MergeMove(MainBlock.Id, nextRibbon.Id);
                MainBlock = Canvas.ApplyMerge(mergeMove);
                yield return mergeMove;
            }
            BlocksToMerge.Clear();
            CurrentColSize += 2;
        }
    }

    private V FindNextCutPoint(State state)
    {
        return state.MainBlock!.BottomLeft + new V(state.CurrentColSize, cellSize);
    }

    public (IList<Move>, double Score) GetBestResult(Screen screen)
    {
        var state = new State(screen, cellSize);
        var moves = new List<Move>();
        while (true)
        {
            while (true)
            {
                var cutPoint = FindNextCutPoint(state);
                moves.AddRange(state.CutCell(cutPoint));
                if (state.MainBlock == null)
                    break;
            }

            var mergeMoves = state.MergeRibbons().ToList();
            if (!state.ShouldStartNextColumn()) break;
            moves.AddRange(mergeMoves);
        }

        return (moves, state.Canvas.GetScore(screen));
    }
}
