using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace lib;

public class Canvas
{
    public int Width;
    public int Height;
    public Dictionary<string, Block> Blocks;
    public int TopLevelIdCounter;
    public int TotalCost;

    public V Size => new(Width, Height);
    public int ScalarSize => Width * Height;

    public int GetScore(Screen screen) => GetSimilarity(screen) + TotalCost;

    public Canvas(int width, int height, Rgba backgroundColor)
    {
        Width = width;
        Height = height;
        Blocks = new Dictionary<string, Block>
        {
            { "0", new SimpleBlock("0", V.Zero, new V(width, height), backgroundColor) }
        };
    }

    public Canvas(Screen problem)
        : this(problem.Width, problem.Height, new Rgba(255, 255, 255, 255))
    {
    }

    public Canvas(int width, int height, Dictionary<string, Block> blocks, int topLevelIdCounter, int totalCost)
    {
        Width = width;
        Height = height;
        Blocks = blocks;
        TopLevelIdCounter = topLevelIdCounter;
        TotalCost = totalCost;
    }

    public void ApplyColor(ColorMove move)
    {
        var block = Blocks[move.BlockId];
        var cost = Move.GetCost(ScalarSize, block.ScalarSize, move.BaseCost);

        switch (block)
        {
            case SimpleBlock simpleBlock:
                Blocks[move.BlockId] = simpleBlock with { Color = move.Color };
                break;

            case ComplexBlock:
                Blocks[move.BlockId] = new SimpleBlock(block.Id, block.BottomLeft, block.TopRight, move.Color);
                break;

            default:
                throw new Exception($"Unexpected block {block}");
        }

        TotalCost += cost;
    }

    public static (Block leftBlock, Block rightBlock) PreApplyVCut(Block block, int offset)
    {
        if (block is SimpleBlock simpleBlock)
        {
            var leftBlock = new SimpleBlock(
                block.Id + ".0",
                block.BottomLeft,
                new V(offset, block.TopRight.Y),
                simpleBlock.Color
            );
            var rightBlock = new SimpleBlock(
                block.Id + ".1",
                new V(offset, block.BottomLeft.Y),
                block.TopRight,
                simpleBlock.Color
            );
            return (leftBlock, rightBlock);
        }

        if (block is ComplexBlock complexBlock)
        {
            var leftBlocks = new List<SimpleBlock>();
            var rightBlocks = new List<SimpleBlock>();
            foreach (var subBlock in complexBlock.Children)
            {
                if (subBlock.BottomLeft.X >= offset)
                {
                    rightBlocks.Add(subBlock);
                    continue;
                }

                if (subBlock.TopRight.X <= offset)
                {
                    leftBlocks.Add(subBlock);
                    continue;
                }

                leftBlocks.Add(new SimpleBlock(
                    "child",
                    subBlock.BottomLeft,
                    new V(offset, subBlock.TopRight.Y),
                    subBlock.Color
                ));
                rightBlocks.Add(new SimpleBlock(
                    "child",
                    new V(offset, subBlock.BottomLeft.Y),
                    subBlock.TopRight,
                    subBlock.Color
                ));
            }

            var leftBlock2 = new ComplexBlock(
                block.Id + ".0",
                block.BottomLeft,
                new V(offset, block.TopRight.Y),
                leftBlocks.ToArray()
            );
            var rightBlock2 = new ComplexBlock(
                block.Id + ".1",
                new V(offset, block.BottomLeft.Y),
                block.TopRight,
                rightBlocks.ToArray()
            );
            return (leftBlock2, rightBlock2);
        }

        throw new Exception($"Unexpected block {block}");
    }

    public (Block leftBlock, Block rightBlock) ApplyVCut(VCutMove move)
    {
        var block = Blocks[move.BlockId];
        var cost = Move.GetCost(ScalarSize, block.ScalarSize, move.BaseCost);
        if (!(block.BottomLeft.X <= move.LineNumber && move.LineNumber <= block.TopRight.X))
        {
            throw new Exception($"Vertical Line X={move.LineNumber} is out of block {block}");
        }
        var (a, b) = PreApplyVCut(block, move.LineNumber);
        Blocks.Remove(block.Id);
        Blocks[block.Id + ".0"] = a;
        Blocks[block.Id + ".1"] = b;
        TotalCost += cost;
        return (a, b);
    }

    public static (Block bottomBlock, Block topBlock) PreApplyHCut(Block block, int offset)
    {
        if (block is SimpleBlock simpleBlock)
        {
            var bottomBlock = new SimpleBlock(
                block.Id + ".0",
                block.BottomLeft,
                new V(block.TopRight.X, offset),
                simpleBlock.Color
            );
            var topBlock = new SimpleBlock(
                block.Id + ".1",
                new V(block.BottomLeft.X, offset),
                block.TopRight,
                simpleBlock.Color
            );
            return (bottomBlock, topBlock);
        }

        if (block is ComplexBlock complexBlock)
        {
            var bottomBlocks = new List<SimpleBlock>();
            var topBlocks = new List<SimpleBlock>();
            foreach (var subBlock in complexBlock.Children)
            {
                if (subBlock.BottomLeft.Y >= offset)
                {
                    topBlocks.Add(subBlock);
                    continue;
                }

                if (subBlock.TopRight.Y <= offset)
                {
                    bottomBlocks.Add(subBlock);
                    continue;
                }

                bottomBlocks.Add(new SimpleBlock(
                    "child",
                    subBlock.BottomLeft,
                    new V(subBlock.TopRight.X, offset),
                    subBlock.Color
                ));
                topBlocks.Add(new SimpleBlock(
                    "child",
                    new V(subBlock.BottomLeft.X, offset),
                    subBlock.TopRight,
                    subBlock.Color
                ));
            }

            var bottomBlock2 = new ComplexBlock(
                block.Id + ".0",
                block.BottomLeft,
                new V(block.TopRight.X, offset),
                bottomBlocks.ToArray()
            );
            var topBlock2 = new ComplexBlock(
                block.Id + ".1",
                new V(block.BottomLeft.X, offset),
                block.TopRight,
                topBlocks.ToArray()
            );
            return (bottomBlock2, topBlock2);
        }
        throw new Exception($"Unexpected block {block}");
    }

    public (Block bottomBlock, Block topBlock) ApplyHCut(HCutMove move)
    {
        var block = Blocks[move.BlockId];
        var cost = Move.GetCost(ScalarSize, block.ScalarSize, move.BaseCost);
        if (!(block.BottomLeft.Y <= move.LineNumber && move.LineNumber <= block.TopRight.Y))
        {
            throw new Exception($"Horizontal Line Y={move.LineNumber} is out of block {block}");
        }
        var (a, b) = PreApplyHCut(block, move.LineNumber);
        Blocks.Remove(block.Id);
        Blocks[block.Id + ".0"] = a;
        Blocks[block.Id + ".1"] = b;
        TotalCost += cost;
        return (a, b);
    }

    public void ApplyMerge(MergeMove move)
    {
        var block1 = Blocks[move.Block1Id];
        var block2 = Blocks[move.Block2Id];
        var cost = Move.GetCost(ScalarSize, Math.Max(block1.ScalarSize, block2.ScalarSize), move.BaseCost);
        var bottomToTop = (block1.BottomLeft.Y == block2.TopRight.Y ||
                           block1.TopRight.Y == block2.BottomLeft.Y) &&
                          block1.BottomLeft.X == block2.BottomLeft.X &&
                          block1.TopRight.X == block2.TopRight.X;
        if (bottomToTop)
        {
            TopLevelIdCounter++;
            V newBottomLeft, newTopRight;
            if (block1.BottomLeft.Y < block2.BottomLeft.Y)
            {
                newBottomLeft = block1.BottomLeft;
                newTopRight = block2.TopRight;
            }
            else
            {
                newBottomLeft = block2.BottomLeft;
                newTopRight = block1.TopRight;
            }

            var newBlock = new ComplexBlock(
                TopLevelIdCounter.ToString(),
                newBottomLeft,
                newTopRight,
                block1.GetChildren().Concat(block2.GetChildren()).ToArray()
            );
            Blocks[newBlock.Id] = newBlock;
            Blocks.Remove(block1.Id);
            Blocks.Remove(block2.Id);
            TotalCost += cost;
            return;
        }

        var leftToRight = (block1.BottomLeft.X == block2.TopRight.X ||
                           block1.TopRight.X == block2.BottomLeft.X) &&
                          block1.BottomLeft.Y == block2.BottomLeft.Y &&
                          block1.TopRight.Y == block2.TopRight.Y;
        if (leftToRight)
        {
            TopLevelIdCounter++;
            V newBottomLeft, newTopRight;
            if (block1.BottomLeft.X < block2.BottomLeft.X)
            {
                newBottomLeft = block1.BottomLeft;
                newTopRight = block2.TopRight;
            }
            else
            {
                newBottomLeft = block2.BottomLeft;
                newTopRight = block1.TopRight;
            }

            var newBlock = new ComplexBlock(
                TopLevelIdCounter.ToString(),
                newBottomLeft,
                newTopRight,
                block1.GetChildren().Concat(block2.GetChildren()).ToArray()
            );
            Blocks[newBlock.Id] = newBlock;
            Blocks.Remove(block1.Id);
            Blocks.Remove(block2.Id);
            TotalCost += cost;
            return;
        }

        throw new Exception($"Invalid merge {block1} {block2}");
    }


    public void ApplySwap(SwapMove move)
    {
        var block1 = Blocks[move.Block1Id];
        var block2 = Blocks[move.Block2Id];
        if (block1.Size != block2.Size)
            throw new Exception($"Blocks are not the same size, {block1} and {block2}");

        var cost = Move.GetCost(ScalarSize, block1.ScalarSize, move.BaseCost);
        Blocks[block1.Id] = block2 with { Id = block1.Id };
        Blocks[block2.Id] = block1 with { Id = block2.Id };
        TotalCost += cost;
    }

    public void ApplyPCut(PCutMove move)
    {
        var block = Blocks[move.BlockId];
        var cost = Move.GetCost(ScalarSize, block.ScalarSize, move.BaseCost);

        if (!move.Point.IsStrictlyInside(block.BottomLeft, block.TopRight))
            throw new Exception($"Point {move.Point} is out of block{block}");

        switch (block)
        {
            case SimpleBlock simpleBlock:
                var bottomLeftBlock = new SimpleBlock(
                    block.Id + ".0",
                    block.BottomLeft,
                    move.Point,
                    simpleBlock.Color
                );
                var bottomRightBlock = new SimpleBlock(
                    block.Id + ".1",
                    new V(move.Point.X, block.BottomLeft.Y),
                    new V(block.TopRight.X, move.Point.Y),
                    simpleBlock.Color
                );
                var topRightBlock = new SimpleBlock(
                    block.Id + ".2",
                    move.Point,
                    block.TopRight,
                    simpleBlock.Color
                );
                var topLeftBlock = new SimpleBlock(
                    block.Id + ".3",
                    new V(block.BottomLeft.X, move.Point.Y),
                    new V(move.Point.X, block.TopRight.Y),
                    simpleBlock.Color
                );
                Blocks.Remove(block.Id);
                Blocks[bottomLeftBlock.Id] = bottomLeftBlock;
                Blocks[bottomRightBlock.Id] = bottomRightBlock;
                Blocks[topRightBlock.Id] = topRightBlock;
                Blocks[topLeftBlock.Id] = topLeftBlock;
                break;

            case ComplexBlock complexBlock:
                var bottomLeftBlocks = new List<SimpleBlock>();
                var bottomRightBlocks = new List<SimpleBlock>();
                var topRightBlocks = new List<SimpleBlock>();
                var topLeftBlocks = new List<SimpleBlock>();
                foreach (var subBlock in complexBlock.Children)
                {
                    // ReSharper disable once InvalidXmlDocComment
                    /**
                     * __________________________
                     * |        |       |       |
                     * |   1    |   2   |   3   |
                     * |________|_______|_______|
                     * |        |       |       |
                     * |   4    |   5   |  6    |
                     * |________|_______|_______|
                     * |        |       |       |
                     * |   7    |   8   |   9   |
                     * |________|_______|_______|
                     */
                    // Case 3
                    if (subBlock.BottomLeft.X >= move.Point.X && subBlock.BottomLeft.Y >= move.Point.Y)
                    {
                        topRightBlocks.Add(subBlock);
                        continue;
                    }

                    // Case 7
                    if (subBlock.TopRight.X <= move.Point.X && subBlock.TopRight.Y <= move.Point.Y)
                    {
                        bottomLeftBlocks.Add(subBlock);
                        continue;
                    }

                    // Case 1
                    if (subBlock.TopRight.X <= move.Point.X && subBlock.BottomLeft.Y >= move.Point.Y)
                    {
                        topLeftBlocks.Add(subBlock);
                        continue;
                    }

                    // Case 9
                    if (subBlock.BottomLeft.X >= move.Point.X && subBlock.TopRight.Y <= move.Point.Y)
                    {
                        bottomRightBlocks.Add(subBlock);
                        continue;
                    }

                    // Case 5
                    if (move.Point.IsInside(subBlock.BottomLeft, subBlock.TopRight))
                    {
                        if (subBlock.BottomLeft.X != move.Point.X && subBlock.BottomLeft.Y != move.Point.Y)
                            bottomLeftBlocks.Add(new SimpleBlock(
                                "bl_child",
                                subBlock.BottomLeft,
                                move.Point,
                                subBlock.Color
                            ));
                        if (subBlock.TopRight.X != move.Point.X && subBlock.BottomLeft.Y != move.Point.Y)
                            bottomRightBlocks.Add(new SimpleBlock(
                                "br_child",
                                new V(move.Point.X, subBlock.BottomLeft.Y),
                                new V(subBlock.TopRight.X, move.Point.Y),
                                subBlock.Color
                            ));
                        if (subBlock.TopRight.X != move.Point.X && subBlock.TopRight.Y != move.Point.Y)
                            topRightBlocks.Add(new SimpleBlock(
                                "tr_child",
                                move.Point,
                                subBlock.TopRight,
                                subBlock.Color
                            ));
                        if (subBlock.BottomLeft.X != move.Point.X && subBlock.TopRight.Y != move.Point.Y)
                            topLeftBlocks.Add(new SimpleBlock(
                                "tl_child",
                                new V(subBlock.BottomLeft.X, move.Point.Y),
                                new V(move.Point.X, subBlock.TopRight.Y),
                                subBlock.Color
                            ));
                        continue;
                    }

                    // Case 2
                    if (subBlock.BottomLeft.X <= move.Point.X
                        && move.Point.X <= subBlock.TopRight.X
                        && move.Point.Y < subBlock.BottomLeft.Y)
                    {
                        if (subBlock.BottomLeft.X != move.Point.X)
                            topLeftBlocks.Add(new SimpleBlock(
                                "case2_tl_child",
                                subBlock.BottomLeft,
                                new V(move.Point.X, subBlock.TopRight.Y),
                                subBlock.Color
                            ));
                        if (subBlock.TopRight.X != move.Point.X)
                            topRightBlocks.Add(new SimpleBlock(
                                "case2_tr_child",
                                new V(move.Point.X, subBlock.BottomLeft.Y),
                                subBlock.TopRight,
                                subBlock.Color
                            ));
                        continue;
                    }

                    // Case 8
                    if (subBlock.BottomLeft.X <= move.Point.X
                        && move.Point.X <= subBlock.TopRight.X
                        && move.Point.Y > subBlock.TopRight.Y)
                    {
                        if (subBlock.BottomLeft.X != move.Point.X)
                            bottomLeftBlocks.Add(new SimpleBlock(
                                "case8_bl_child",
                                subBlock.BottomLeft,
                                new V(move.Point.X, subBlock.TopRight.Y),
                                subBlock.Color
                            ));
                        if (subBlock.TopRight.X != move.Point.X)
                            bottomRightBlocks.Add(new SimpleBlock(
                                "case8_br_child",
                                new V(move.Point.X, subBlock.BottomLeft.Y),
                                subBlock.TopRight,
                                subBlock.Color
                            ));
                        continue;
                    }

                    // Case 4
                    if (subBlock.BottomLeft.Y <= move.Point.Y
                        && move.Point.Y <= subBlock.TopRight.Y
                        && move.Point.X < subBlock.BottomLeft.X)
                    {
                        if (subBlock.BottomLeft.Y != move.Point.Y)
                            bottomRightBlocks.Add(new SimpleBlock(
                                "case4_br_child",
                                subBlock.BottomLeft,
                                new V(subBlock.TopRight.X, move.Point.Y),
                                subBlock.Color
                            ));
                        if (subBlock.TopRight.Y != move.Point.Y)
                            topRightBlocks.Add(new SimpleBlock(
                                "case4_tr_child",
                                new V(subBlock.BottomLeft.X, move.Point.Y),
                                subBlock.TopRight,
                                subBlock.Color
                            ));
                        continue;
                    }

                    // Case 6
                    if (subBlock.BottomLeft.Y <= move.Point.Y
                        && move.Point.Y <= subBlock.TopRight.Y
                        && move.Point.X > subBlock.TopRight.X)
                    {
                        if (subBlock.BottomLeft.Y != move.Point.Y)
                            bottomLeftBlocks.Add(new SimpleBlock(
                                "case6_bl_child",
                                subBlock.BottomLeft,
                                new V(subBlock.TopRight.X, move.Point.Y),
                                subBlock.Color
                            ));
                        if (subBlock.TopRight.Y != move.Point.Y)
                            topLeftBlocks.Add(new SimpleBlock(
                                "case6_br_child",
                                new V(subBlock.BottomLeft.X, move.Point.Y),
                                subBlock.TopRight,
                                subBlock.Color
                            ));
                        continue;
                    }
                }

                var bottomLeftBlockC = new ComplexBlock(
                    block.Id + ".0",
                    block.BottomLeft,
                    move.Point,
                    bottomLeftBlocks.ToArray()
                );
                var bottomRightBlockC = new ComplexBlock(
                    block.Id + ".1",
                    new V(move.Point.X, block.BottomLeft.Y),
                    new V(block.TopRight.X, move.Point.Y),
                    bottomRightBlocks.ToArray()
                );
                var topRightBlockC = new ComplexBlock(
                    block.Id + ".2",
                    move.Point,
                    block.TopRight,
                    topRightBlocks.ToArray()
                );
                var topLeftBlockC = new ComplexBlock(
                    block.Id + ".3",
                    new V(block.BottomLeft.X, move.Point.Y),
                    new V(move.Point.X, block.TopRight.Y),
                    topLeftBlocks.ToArray()
                );
                Blocks.Remove(block.Id);
                Blocks[bottomLeftBlockC.Id] = bottomLeftBlockC;
                Blocks[bottomRightBlockC.Id] = bottomRightBlockC;
                Blocks[topRightBlockC.Id] = topRightBlockC;
                Blocks[topLeftBlockC.Id] = topLeftBlockC;
                break;

            default:
                throw new Exception($"Unexpected block {block}");
        }

        TotalCost += cost;
    }

    public Screen ToScreen()
    {
        var screen = new Screen(Width, Height);
        foreach (var block in Blocks.Values)
        {
            foreach (var simpleBlock in block.GetChildren())
            {
                for (int x = simpleBlock.BottomLeft.X; x < simpleBlock.TopRight.X; x++)
                for (int y = simpleBlock.BottomLeft.Y; y < simpleBlock.TopRight.Y; y++)
                {
                    screen.Pixels[x, y] = simpleBlock.Color;
                }
            }
        }

        return screen;
    }

    public int GetSimilarity(Screen screen)
    {
        var diff = 0.0;
        foreach (var block in Blocks.Values)
        {
            foreach (var simpleBlock in block.GetChildren())
                diff += screen.DiffTo(simpleBlock);
        }

        return (int)Math.Round(diff);
    }

    public void Apply(Move move)
    {
        switch (move)
        {
            case ColorMove cm:
                ApplyColor(cm);
                break;
            case VCutMove vcm:
                ApplyVCut(vcm);
                break;
            case HCutMove hcm:
                ApplyHCut(hcm);
                break;
            case PCutMove pcm:
                ApplyPCut(pcm);
                break;
            case MergeMove mm:
                ApplyMerge(mm);
                break;
            case SwapMove sm:
                ApplySwap(sm);
                break;
            case NopMove:
                break;
            default:
                throw new Exception(move.ToString());
        }
    }

    public Canvas Copy()
    {
        return new(Width, Height, Blocks.ToDictionary(t => t.Key, t => t.Value), TopLevelIdCounter, TotalCost);
    }
}
