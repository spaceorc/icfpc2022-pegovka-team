using System;
using System.Collections.Generic;
using System.Linq;

namespace lib.Algorithms;

public class LayersAlgorithm : IAlgorithm
{
    private readonly int cellSize;

    public LayersAlgorithm(int cellSize)
    {
        this.cellSize = cellSize;
    }

    public record Cell(int Left, int Width, Rgba Color);
    public class State
    {
        public State(Screen screen, int deltaH)
        {
            Screen = screen;
            this.deltaH = deltaH;
            Canvas = new Canvas(screen);
            Bottom = 0;
            Left = 0;
            CurrentRow = new List<Cell>();
        }

        public Canvas Canvas;
        public Block Block => Canvas.Blocks.Values.Single();
        public Screen Screen;
        public readonly int deltaH;
        public int Bottom;
        public int Left;
        public List<Cell> CurrentRow;

        public void StartNextLine()
        {
            Bottom += deltaH; // TODO: calculate better deltaH using CurrentRow
            Left = 0;
            CurrentRow.Clear();
        }
    }

    private int FindNextWidth(State state)
    {
        if (state.Left < state.Canvas.Width-cellSize)
          return FindBestWidth(state, cellSize, state.Left, state.Bottom);
        else
            return cellSize;
    }

    int FindBestWidth(State state, int height, int left, int bottom)
    {
        var canvas = state.Canvas;
        var screen = state.Screen;
        var penaltyByWidth = new Dictionary<int, double>();

        for (var w = cellSize; w < canvas.Width - left + 5; w+=1)
        {
            var bl = new V(left, bottom);
            var tr = bl + new V(w, height);
            var color = screen.GetAverageColor(bl, tr);
            //var color = screen.GetAverageColorByGeometricMedian(bl.X, bl.Y, w, height);
            var similarityPenalty = screen.DiffTo(bl, tr, color);
            var widthToRightBorder = canvas.Width - tr.X;
            var penalty = (similarityPenalty) / w;
            if (widthToRightBorder > 0 && widthToRightBorder < cellSize)
                penalty += 100500;
            penaltyByWidth[w] = penalty;
        }

        return GetBestMove(penaltyByWidth);
    }

    private static int GetBestMove(Dictionary<int, double> penaltiesByWidth)
    {
        if (penaltiesByWidth.Count < 4)
            return Enumerable.MinBy(penaltiesByWidth, v => v.Value).Key;

        using var enumerator = penaltiesByWidth.GetEnumerator();
        enumerator.MoveNext();
        var first = enumerator.Current;
        enumerator.MoveNext();
        var second = enumerator.Current;
        enumerator.MoveNext();
        var third = enumerator.Current;
        enumerator.MoveNext();

        if (first.Value < second.Value && first.Value < third.Value)
        {
            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;

                if (current.Value < first.Value)
                {
                    first = second;
                    second = third;
                    third = current;
                    break;
                }
                first = second;
                second = third;
                third = current;
            }
        }

        while (enumerator.MoveNext())
        {
            var current = enumerator.Current;

            if (first.Value < second.Value && first.Value < third.Value && first.Value < current.Value)
            {
                return first.Key;
            }
            first = second;
            second = third;
            third = current;
        }

        return Enumerable.MinBy(new[] { first, second, third }, v => v.Value).Key;
    }

    public (IList<Move>, double Score) GetBestResult(Screen screen)
    {
        var state = new State(screen, cellSize);
        var moves = new List<Move>();
        {
            var w = FindNextWidth(state);
            var color = screen.GetAverageColorByGeometricMedian(state.Left, state.Bottom, w, cellSize);
            state.CurrentRow.Add(new Cell(state.Left, w, color));
            state.Left += w;
            var colorMove = new ColorMove(state.Block.Id, color);
            moves.Add(colorMove);
            state.Canvas.Apply(colorMove);
        }
        while (state.Left < state.Canvas.Width)
        {
            var w = FindNextWidth(state);
            var color = screen.GetAverageColorByGeometricMedian(state.Left, state.Bottom, w, cellSize);
            state.CurrentRow.Add(new Cell(state.Left, w, color));
            var vCut = new VCutMove(state.Block.Id, state.Left);
            state.Left += w;
            var (leftBlock, rightBlock) = state.Canvas.ApplyVCut(vCut);
            var colorMove = new ColorMove(rightBlock.Id, color);
            var mergeMove = new MergeMove(leftBlock.Id, rightBlock.Id);
            state.Canvas.Apply(colorMove);
            state.Canvas.Apply(mergeMove);
            moves.Add(vCut);
            moves.Add(colorMove);
            moves.Add(mergeMove);
        }
        state.StartNextLine();
        moves.Add(new NopMove("Finish First Line"));

        while (state.Bottom < state.Canvas.Height)
        {
            {
                var hCut = new HCutMove(state.Block.Id, state.Bottom);
                moves.Add(hCut);
                var (bottomBlock, topBlock) = state.Canvas.ApplyHCut(hCut);
                var w = FindNextWidth(state);
                var color = screen.GetAverageColorByGeometricMedian(state.Left, state.Bottom, w, cellSize);
                state.CurrentRow.Add(new Cell(state.Left, w, color));
                state.Left += w;
                var colorMove = new ColorMove(topBlock.Id, color);
                moves.Add(colorMove);
                state.Canvas.Apply(colorMove);
                var mergeMove = new MergeMove(bottomBlock.Id, topBlock.Id);
                moves.Add(mergeMove);
                state.Canvas.ApplyMerge(mergeMove);
            }
            moves.Add(new NopMove("Finish First Cell"));


            while (state.Left < state.Canvas.Width)
            {
                var w = FindNextWidth(state);
                var color = screen.GetAverageColorByGeometricMedian(state.Left, state.Bottom, w, cellSize);
                state.CurrentRow.Add(new Cell(state.Left, w, color));
                var pCut = new PCutMove(state.Block.Id, new V(state.Left, state.Bottom));
                state.Left += w;
                var (bottomLeftBlock, bottomRightBlock, topRightBlock, topLeftBlock) = state.Canvas.ApplyPCut(pCut);
                moves.Add(pCut);
                var colorMove = new ColorMove(topRightBlock.Id, color);
                state.Canvas.Apply(colorMove);
                moves.Add(colorMove);
                var mergeMoves = GetNotSoOptimalMergeMoves(bottomLeftBlock, bottomRightBlock, topRightBlock, topLeftBlock, state);
                foreach (var mergeMove in mergeMoves)
                {
                    state.Canvas.Apply(mergeMove);
                    moves.Add(mergeMove);
                }
            }
            state.StartNextLine();
            moves.Add(new NopMove("Finish Next Line"));

        }

        return (moves, state.Canvas.GetScore(screen));
    }

    //TODO Make it optimal
    private IEnumerable<Move> GetNotSoOptimalMergeMoves(Block bl, Block br, Block tr, Block tl, State state)
    {
        var v1 = new MergeMove(bl.Id, tl.Id);
        var v2 = new MergeMove(br.Id, tr.Id);
        var leftId = state.Canvas.TopLevelIdCounter + 1;
        var rightId = state.Canvas.TopLevelIdCounter + 2;
        var h = new MergeMove(leftId.ToString(), rightId.ToString());
        return new[] { v1, v2, h };
    }
}
