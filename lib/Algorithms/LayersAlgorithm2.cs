using System;
using System.Collections.Generic;
using System.Linq;

namespace lib.Algorithms;

public class LayersAlgorithm2 : IAlgorithm
{
    private readonly int cellSize;

    public LayersAlgorithm2(int cellSize)
    {
        this.cellSize = cellSize;
    }

    public record Cell(int Left, int Width, Rgba Color)
    {
        public int Height { get; set; }
    }
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
        public List<Move> AppliedMoves = new();


        public void StartNextLine()
        {
            foreach (var cell in CurrentRow)
                cell.Height = FindBestHeight(this, cell, deltaH);
            Bottom += CurrentRow.Min(c => c.Height);
            //Bottom += deltaH;
            Left = 0;
            CurrentRow.Clear();
        }
        int FindBestHeight(State state, Cell cell, int minHeight)
        {
            var canvas = state.Canvas;
            var screen = state.Screen;
            var bl = new V(cell.Left, state.Bottom);
            var maxHeight = canvas.Height - state.Bottom + 1;
            //var prevPenalty = double.PositiveInfinity;
            for (var h = minHeight; h < maxHeight; h++)
            {
                var tr = bl + new V(cell.Width, h);
                var similarityPenalty = screen.DiffTo(bl, tr, cell.Color);
                if (similarityPenalty > 40 && similarityPenalty / (h * cell.Width) > 0.01) return h - 1;
                //prevPenalty = similarityPenalty;
            }

            return maxHeight;


        }

        public void ApplyColor(Block block, Rgba color)
        {
            if (block is SimpleBlock sb && sb.Color == color) return;
            if (Move.GetCost(400*400, block.ScalarSize, 5) > 500) return;
            var colorMove = new ColorMove(block.Id, color);
            AppliedMoves.Add(colorMove);
            Canvas.Apply(colorMove);
        }
    }

    private int FindNextWidth(State state)
    {
        if (state.Left < state.Canvas.Width-cellSize)
          return FindBestWidth(state, cellSize, state.Left, state.Bottom);
        else
            return cellSize;
    }

    int FindBestWidth2(State state, int height, int left, int bottom)
    {
        var canvas = state.Canvas;
        var screen = state.Screen;
        var bl = new V(left, bottom);
        var maxWidth = canvas.Width - left + 1;
        for (var w = 5; w < maxWidth; w++)
        {
            var tr = bl + new V(w, height);
            var color = screen.GetAverageColor(bl, tr);
            var similarityPenalty = screen.DiffTo(bl, tr, color);
            if (similarityPenalty > 40 && similarityPenalty / (w * height) > 0.1) return w - 1;
        }

        return maxWidth;


    }

    int FindBestWidth(State state, int height, int left, int bottom)
    {
        var canvas = state.Canvas;
        var screen = state.Screen;
        var penaltyByWidth = new Dictionary<int, double>();

        for (var w = cellSize/2; w < canvas.Width - left + 5; w+=1)
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
        if (penaltiesByWidth.Count < 40000000)
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
        {
            var w = FindNextWidth(state);
            var color = screen.GetAverageColorByGeometricMedian(state.Left, state.Bottom, w, cellSize);
            state.CurrentRow.Add(new Cell(state.Left, w, color));
            state.Left += w;
            state.ApplyColor(state.Block, color);
        }
        while (state.Left < state.Canvas.Width)
        {
            var w = FindNextWidth(state);
            var color = screen.GetAverageColorByGeometricMedian(state.Left, state.Bottom, w, cellSize);
            state.CurrentRow.Add(new Cell(state.Left, w, color));
            var vCut = new VCutMove(state.Block.Id, state.Left);
            state.Left += w;
            var (leftBlock, rightBlock) = state.Canvas.ApplyVCut(vCut);
            state.AppliedMoves.Add(vCut);
            state.ApplyColor(rightBlock, color);
            var mergeMove = new MergeMove(leftBlock.Id, rightBlock.Id);
            state.Canvas.Apply(mergeMove);
            state.AppliedMoves.Add(mergeMove);
        }

        state.StartNextLine();
        state.AppliedMoves.Add(new NopMove("Finish First Line"));
        var moves = state.AppliedMoves;
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

                state.ApplyColor(topBlock, color);
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

                state.ApplyColor(topRightBlock, color);

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
