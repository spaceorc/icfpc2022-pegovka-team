using System;
using System.Collections.Generic;
using System.Linq;

namespace lib;

public class BadMoveException : Exception
{
    public BadMoveException(string message) : base(message)
    {
    }
}

public class Canvas
{
    public int Width;
    public int Height;
    public Dictionary<string, Block> Blocks;
    public int TopLevelIdCounter;
    public int TotalCost;

    public Dictionary<string, StatValue> CostByMoveType = new()
    {
        { "color", new StatValue() },
        { "vcut", new StatValue() },
        { "hcut", new StatValue() },
        { "pcut", new StatValue() },
        { "merge", new StatValue() },
        { "swap", new StatValue() },
    };

    public V Size => new(Width, Height);
    public int ScalarSize => Width * Height;

    public int GetScore(Screen screen) => GetSimilarity(screen) + TotalCost;

    public Canvas(int width, int height, IEnumerable<SimpleBlock> blocks)
    {
        Width = width;
        Height = height;
        var simpleBlocks = blocks as SimpleBlock[] ?? blocks.ToArray();
        Blocks = simpleBlocks.ToDictionary(x => x.Id, x => (Block)x);
        TopLevelIdCounter = simpleBlocks.Length - 1;
    }

    public Canvas(Screen problem)
        : this(problem.Width, problem.Height, problem.InitialBlocks)
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
        CostByMoveType["color"].Add(cost);
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
            throw new BadMoveException($"Vertical Line X={move.LineNumber} is out of block {block}");
        }
        var (a, b) = PreApplyVCut(block, move.LineNumber);
        Blocks.Remove(block.Id);
        Blocks[block.Id + ".0"] = a;
        Blocks[block.Id + ".1"] = b;
        TotalCost += cost;
        CostByMoveType["vcut"].Add(cost);
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
            throw new BadMoveException($"Horizontal Line Y={move.LineNumber} is out of block {block}");
        }
        var (a, b) = PreApplyHCut(block, move.LineNumber);
        Blocks.Remove(block.Id);
        Blocks[block.Id + ".0"] = a;
        Blocks[block.Id + ".1"] = b;
        TotalCost += cost;
        CostByMoveType["color"].Add(cost);
        return (a, b);
    }

    public ComplexBlock ApplyMerge(MergeMove move)
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
            CostByMoveType["merge"].Add(cost);
            return newBlock;
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
            CostByMoveType["merge"].Add(cost);
            return newBlock;
        }

        throw new BadMoveException($"Invalid merge {block1} {block2}");
    }


    public void ApplySwap(SwapMove move)
    {
        var block1 = Blocks[move.Block1Id];
        var block2 = Blocks[move.Block2Id];
        if (block1.Size != block2.Size)
            throw new BadMoveException($"Blocks are not the same size, {block1} and {block2}");

        var diff = block1.BottomLeft - block2.BottomLeft;

        if (block1 is ComplexBlock complexBlock1)
        {
            var children1 = complexBlock1.Children.Select(subBlock => subBlock with
            {
                BottomLeft = subBlock.BottomLeft - diff,
                TopRight = subBlock.TopRight - diff
            }).ToArray();
            block1 = complexBlock1 with {Children = children1};
        }

        if (block2 is ComplexBlock complexBlock2)
        {
            var children2 = complexBlock2.Children.Select(subBlock => subBlock with
            {
                BottomLeft = subBlock.BottomLeft + diff,
                TopRight = subBlock.TopRight + diff
            }).ToArray();
            block2 = complexBlock2 with {Children = children2};
        }

        var cost = Move.GetCost(ScalarSize, block1.ScalarSize, move.BaseCost);
        Blocks[block1.Id] = block1 with
        {
            BottomLeft = block2.BottomLeft,
            TopRight = block2.TopRight,
        };
        Blocks[block2.Id] = block2 with
        {
            BottomLeft = block1.BottomLeft,
            TopRight = block1.TopRight,
        };
        TotalCost += cost;
        CostByMoveType["swap"].Add(cost);
    }

    public static (Block bl, Block br, Block tl, Block tr) PreApplyPCut(Block block, V point)
    {
        switch (block)
        {
            case SimpleBlock simpleBlock:
                var bottomLeftBlock = new SimpleBlock(
                    block.Id + ".0",
                    block.BottomLeft,
                    point,
                    simpleBlock.Color
                );
                var bottomRightBlock = new SimpleBlock(
                    block.Id + ".1",
                    new V(point.X, block.BottomLeft.Y),
                    new V(block.TopRight.X, point.Y),
                    simpleBlock.Color
                );
                var topRightBlock = new SimpleBlock(
                    block.Id + ".2",
                    point,
                    block.TopRight,
                    simpleBlock.Color
                );
                var topLeftBlock = new SimpleBlock(
                    block.Id + ".3",
                    new V(block.BottomLeft.X, point.Y),
                    new V(point.X, block.TopRight.Y),
                    simpleBlock.Color
                );
                return (bottomLeftBlock, bottomRightBlock, topLeftBlock, topRightBlock);

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
                    if (subBlock.BottomLeft.X >= point.X && subBlock.BottomLeft.Y >= point.Y)
                    {
                        topRightBlocks.Add(subBlock);
                        continue;
                    }

                    // Case 7
                    if (subBlock.TopRight.X <= point.X && subBlock.TopRight.Y <= point.Y)
                    {
                        bottomLeftBlocks.Add(subBlock);
                        continue;
                    }

                    // Case 1
                    if (subBlock.TopRight.X <= point.X && subBlock.BottomLeft.Y >= point.Y)
                    {
                        topLeftBlocks.Add(subBlock);
                        continue;
                    }

                    // Case 9
                    if (subBlock.BottomLeft.X >= point.X && subBlock.TopRight.Y <= point.Y)
                    {
                        bottomRightBlocks.Add(subBlock);
                        continue;
                    }

                    // Case 5
                    if (point.IsInside(subBlock.BottomLeft, subBlock.TopRight))
                    {
                        if (subBlock.BottomLeft.X != point.X && subBlock.BottomLeft.Y != point.Y)
                            bottomLeftBlocks.Add(new SimpleBlock(
                                "bl_child",
                                subBlock.BottomLeft,
                                point,
                                subBlock.Color
                            ));
                        if (subBlock.TopRight.X != point.X && subBlock.BottomLeft.Y != point.Y)
                            bottomRightBlocks.Add(new SimpleBlock(
                                "br_child",
                                new V(point.X, subBlock.BottomLeft.Y),
                                new V(subBlock.TopRight.X, point.Y),
                                subBlock.Color
                            ));
                        if (subBlock.TopRight.X != point.X && subBlock.TopRight.Y != point.Y)
                            topRightBlocks.Add(new SimpleBlock(
                                "tr_child",
                                point,
                                subBlock.TopRight,
                                subBlock.Color
                            ));
                        if (subBlock.BottomLeft.X != point.X && subBlock.TopRight.Y != point.Y)
                            topLeftBlocks.Add(new SimpleBlock(
                                "tl_child",
                                new V(subBlock.BottomLeft.X, point.Y),
                                new V(point.X, subBlock.TopRight.Y),
                                subBlock.Color
                            ));
                        continue;
                    }

                    // Case 2
                    if (subBlock.BottomLeft.X <= point.X
                        && point.X <= subBlock.TopRight.X
                        && point.Y < subBlock.BottomLeft.Y)
                    {
                        if (subBlock.BottomLeft.X != point.X)
                            topLeftBlocks.Add(new SimpleBlock(
                                "case2_tl_child",
                                subBlock.BottomLeft,
                                new V(point.X, subBlock.TopRight.Y),
                                subBlock.Color
                            ));
                        if (subBlock.TopRight.X != point.X)
                            topRightBlocks.Add(new SimpleBlock(
                                "case2_tr_child",
                                new V(point.X, subBlock.BottomLeft.Y),
                                subBlock.TopRight,
                                subBlock.Color
                            ));
                        continue;
                    }

                    // Case 8
                    if (subBlock.BottomLeft.X <= point.X
                        && point.X <= subBlock.TopRight.X
                        && point.Y > subBlock.TopRight.Y)
                    {
                        if (subBlock.BottomLeft.X != point.X)
                            bottomLeftBlocks.Add(new SimpleBlock(
                                "case8_bl_child",
                                subBlock.BottomLeft,
                                new V(point.X, subBlock.TopRight.Y),
                                subBlock.Color
                            ));
                        if (subBlock.TopRight.X != point.X)
                            bottomRightBlocks.Add(new SimpleBlock(
                                "case8_br_child",
                                new V(point.X, subBlock.BottomLeft.Y),
                                subBlock.TopRight,
                                subBlock.Color
                            ));
                        continue;
                    }

                    // Case 4
                    if (subBlock.BottomLeft.Y <= point.Y
                        && point.Y <= subBlock.TopRight.Y
                        && point.X < subBlock.BottomLeft.X)
                    {
                        if (subBlock.BottomLeft.Y != point.Y)
                            bottomRightBlocks.Add(new SimpleBlock(
                                "case4_br_child",
                                subBlock.BottomLeft,
                                new V(subBlock.TopRight.X, point.Y),
                                subBlock.Color
                            ));
                        if (subBlock.TopRight.Y != point.Y)
                            topRightBlocks.Add(new SimpleBlock(
                                "case4_tr_child",
                                new V(subBlock.BottomLeft.X, point.Y),
                                subBlock.TopRight,
                                subBlock.Color
                            ));
                        continue;
                    }

                    // Case 6
                    if (subBlock.BottomLeft.Y <= point.Y
                        && point.Y <= subBlock.TopRight.Y
                        && point.X > subBlock.TopRight.X)
                    {
                        if (subBlock.BottomLeft.Y != point.Y)
                            bottomLeftBlocks.Add(new SimpleBlock(
                                "case6_bl_child",
                                subBlock.BottomLeft,
                                new V(subBlock.TopRight.X, point.Y),
                                subBlock.Color
                            ));
                        if (subBlock.TopRight.Y != point.Y)
                            topLeftBlocks.Add(new SimpleBlock(
                                "case6_br_child",
                                new V(subBlock.BottomLeft.X, point.Y),
                                subBlock.TopRight,
                                subBlock.Color
                            ));
                        continue;
                    }
                }

                var bottomLeftBlockC = new ComplexBlock(
                    block.Id + ".0",
                    block.BottomLeft,
                    point,
                    bottomLeftBlocks.ToArray()
                );
                var bottomRightBlockC = new ComplexBlock(
                    block.Id + ".1",
                    new V(point.X, block.BottomLeft.Y),
                    new V(block.TopRight.X, point.Y),
                    bottomRightBlocks.ToArray()
                );
                var topRightBlockC = new ComplexBlock(
                    block.Id + ".2",
                    point,
                    block.TopRight,
                    topRightBlocks.ToArray()
                );
                var topLeftBlockC = new ComplexBlock(
                    block.Id + ".3",
                    new V(block.BottomLeft.X, point.Y),
                    new V(point.X, block.TopRight.Y),
                    topLeftBlocks.ToArray()
                );
                return (bottomLeftBlockC, bottomRightBlockC, topLeftBlockC, topRightBlockC);
            default:
                throw new Exception($"Unexpected block {block}");
        }
    }

    public (Block bottomLeftBlock, Block bottomRightBlock, Block topRightBlock, Block topLeftBlock) ApplyPCut(PCutMove move)
    {
        var block = Blocks[move.BlockId];
        var cost = Move.GetCost(ScalarSize, block.ScalarSize, move.BaseCost);

        if (!move.Point.IsStrictlyInside(block.BottomLeft, block.TopRight))
            throw new BadMoveException($"Point {move.Point} is out of block{block}");
        var (bottomLeftBlock, bottomRightBlock, topLeftBlock, topRightBlock) = PreApplyPCut(block, move.Point);
        Blocks.Remove(block.Id);
        Blocks[bottomLeftBlock.Id] = bottomLeftBlock;
        Blocks[bottomRightBlock.Id] = bottomRightBlock;
        Blocks[topRightBlock.Id] = topRightBlock;
        Blocks[topLeftBlock.Id] = topLeftBlock;
        TotalCost += cost;
        CostByMoveType["pcut"].Add(cost);
        return (bottomLeftBlock, bottomRightBlock, topRightBlock, topLeftBlock);
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

    public SimpleBlock[] Flatten()
    {
        var result = new List<SimpleBlock>();
        foreach (var block in Blocks.Values)
            result.AddRange(block.GetChildren());
        return result.ToArray();
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
