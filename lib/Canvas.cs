using System;
using System.Collections.Generic;
using System.Linq;

namespace lib;

public class Canvas
{
    public int Width;
    public int Height;
    public Dictionary<string, Block> Blocks;
    public int TopLevelIdCounter = 0;

    public Canvas(int width, int height, Rgba backgroundColor)
    {
        Width = width;
        Height = height;
        Blocks = new Dictionary<string, Block>()
        {
            { "0", new SimpleBlock("0", V.Zero, new V(width, height), backgroundColor) }
        };
    }

    public int ApplyColor(ColorMove move)
    {
        var block = Blocks[move.BlockId];
        var cost = (int)Math.Round(5.0 * Width * Height / block.TopRight.Dist2To(block.BottomLeft));

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

        return cost;
    }

    public int ApplyVCut(LCutMove move)
    {
        if (move.Orientation != Orientation.X) throw new Exception(move.ToString());
        var block = Blocks[move.BlockId];
        var cost = (int)Math.Round(7.0 * Width * Height / block.TopRight.Dist2To(block.BottomLeft));
        if (!(block.BottomLeft.X <= move.LineNumber && move.LineNumber <= block.TopRight.X))
        {
            throw new Exception($"Vertical Line X={move.LineNumber} is out of block {block}");
        }

        if (block is SimpleBlock simpleBlock)
        {
            var leftBlock = new SimpleBlock(
                block.Id + ".0",
                block.BottomLeft,
                new V(move.LineNumber, block.TopRight.Y),
                simpleBlock.Color
            );
            var rightBlock = new SimpleBlock(
                block.Id + ".1",
                new V(move.LineNumber, block.BottomLeft.Y),
                block.TopRight,
                simpleBlock.Color
            );
            Blocks.Remove(block.Id);
            Blocks[block.Id + ".0"] = leftBlock;
            Blocks[block.Id + ".1"] = rightBlock;
            return cost;
        }

        if (block is ComplexBlock complexBlock)
        {
            var leftBlocks = new List<SimpleBlock>();
            var rightBlocks = new List<SimpleBlock>();
            foreach (var subBlock in complexBlock.Children)
            {
                if (subBlock.BottomLeft.X >= move.LineNumber)
                {
                    rightBlocks.Add(subBlock);
                    break;
                }

                if (subBlock.TopRight.X <= move.LineNumber)
                {
                    leftBlocks.Add(subBlock);
                    break;
                }

                leftBlocks.Add(new SimpleBlock(
                    "child",
                    subBlock.BottomLeft,
                    new V(move.LineNumber, subBlock.TopRight.Y),
                    subBlock.Color
                ));
                rightBlocks.Add(new SimpleBlock(
                    "child",
                    new V(move.LineNumber, subBlock.BottomLeft.Y),
                    subBlock.TopRight,
                    subBlock.Color
                ));
            }

            Blocks.Remove(block.Id);
            var leftBlock2 = new ComplexBlock(
                block.Id + ".0",
                block.BottomLeft,
                new V(move.LineNumber, block.TopRight.Y),
                leftBlocks.ToArray()
            );
            var rightBlock2 = new ComplexBlock(
                block.Id + ".1",
                new V(move.LineNumber, block.BottomLeft.Y),
                block.TopRight,
                rightBlocks.ToArray()
            );
            Blocks[block.Id + ".0"] = leftBlock2;
            Blocks[block.Id + ".1"] = rightBlock2;
            return cost;
        }
        throw new Exception($"Unexpected block {block}");
    }

    public int ApplyHCut(LCutMove move)
    {
        if (move.Orientation != Orientation.Y) throw new Exception(move.ToString());
        var block = Blocks[move.BlockId];
        var cost = (int)Math.Round(7.0 * Width * Height / block.TopRight.Dist2To(block.BottomLeft));
        if (!(block.BottomLeft.Y <= move.LineNumber && move.LineNumber <= block.TopRight.Y))
        {
            throw new Exception($"Horizontal Line Y={move.LineNumber} is out of block {block}");
        }

        if (block is SimpleBlock simpleBlock)
        {
            var bottomBlock = new SimpleBlock(
                block.Id + ".0",
                block.BottomLeft,
                new V(block.TopRight.X, move.LineNumber),
                simpleBlock.Color
            );
            var topBlock = new SimpleBlock(
                block.Id + ".1",
                new V(block.BottomLeft.X, move.LineNumber),
                block.TopRight,
                simpleBlock.Color
            );
            Blocks.Remove(block.Id);
            Blocks[block.Id + ".0"] = bottomBlock;
            Blocks[block.Id + ".1"] = topBlock;
            return cost;
        }

        if (block is ComplexBlock complexBlock)
        {
            var bottomBlocks = new List<SimpleBlock>();
            var topBlocks = new List<SimpleBlock>();
            foreach (var subBlock in complexBlock.Children)
            {
                if (subBlock.BottomLeft.Y >= move.LineNumber)
                {
                    topBlocks.Add(subBlock);
                    break;
                }

                if (subBlock.TopRight.Y <= move.LineNumber)
                {
                    bottomBlocks.Add(subBlock);
                    break;
                }

                bottomBlocks.Add(new SimpleBlock(
                    "child",
                    subBlock.BottomLeft,
                    new V(subBlock.TopRight.X, move.LineNumber),
                    subBlock.Color
                ));
                topBlocks.Add(new SimpleBlock(
                    "child",
                    new V(subBlock.BottomLeft.X, move.LineNumber),
                    subBlock.TopRight,
                    subBlock.Color
                ));
            }

            Blocks.Remove(block.Id);
            var bottomBlock2 = new ComplexBlock(
                block.Id + ".0",
                block.BottomLeft,
                new V(block.TopRight.X, move.LineNumber),
                bottomBlocks.ToArray()
            );
            var topBlock2 = new ComplexBlock(
                block.Id + ".1",
                new V(block.BottomLeft.X, move.LineNumber),
                block.TopRight,
                topBlocks.ToArray()
            );
            Blocks[block.Id + ".0"] = bottomBlock2;
            Blocks[block.Id + ".1"] = topBlock2;
            return cost;
        }
        throw new Exception($"Unexpected block {block}");
    }

    public int ApplyMerge(MergeMove move)
    {
        var block1 = Blocks[move.Block1Id];
        var block2 = Blocks[move.Block2Id];
        var cost = (int)Math.Round(1.0 * Width * Height / Math.Max(block1.Size.Len2, block2.Size.Len2));
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
            return cost;
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
            return cost;
        }

        throw new Exception($"Invalid merge {block1} {block2}");
    }


    public int ApplySwap(SwapMove move)
    {
        var block1 = Blocks[move.Block1Id];
        var block2 = Blocks[move.Block2Id];
        if (block1.Size != block2.Size) throw new Exception($"Blocks are not the same size, {block1} and {block2}");

        var cost = (int)Math.Round(7.0 * Width * Height / block1.TopRight.Dist2To(block1.BottomLeft));
        Blocks[block1.Id] = block2 with {Id = block1.Id};
        Blocks[block2.Id] = block1 with { Id = block2.Id };
        return cost;
    }

    public int ApplyPCut(PCutMove move)
    {
        var block = Blocks[move.BlockId];
        var cost = (int)Math.Round(10.0 * Width * Height / block.TopRight.Dist2To(block.BottomLeft));

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
                        break;
                    }

                    // Case 7
                    if (subBlock.TopRight.X <= move.Point.X && subBlock.TopRight.Y <= move.Point.Y)
                    {
                        bottomLeftBlocks.Add(subBlock);
                        break;
                    }

                    // Case 1
                    if (subBlock.TopRight.X <= move.Point.X && subBlock.BottomLeft.Y >= move.Point.Y)
                    {
                        topLeftBlocks.Add(subBlock);
                        break;
                    }

                    // Case 9
                    if (subBlock.BottomLeft.X >= move.Point.X && subBlock.TopRight.Y <= move.Point.Y)
                    {
                        bottomRightBlocks.Add(subBlock);
                        break;
                    }

                    // Case 5
                    if (move.Point.IsInside(subBlock.BottomLeft, subBlock.TopRight))
                    {
                        bottomLeftBlocks.Add(new SimpleBlock(
                            "bl_child",
                            subBlock.BottomLeft,
                            move.Point,
                            subBlock.Color
                        ));
                        bottomRightBlocks.Add(new SimpleBlock(
                            "br_child",
                            new V(move.Point.X, subBlock.BottomLeft.Y),
                            new V(subBlock.TopRight.X, move.Point.Y),
                            subBlock.Color
                        ));
                        topRightBlocks.Add(new SimpleBlock(
                            "tr_child",
                            move.Point,
                            subBlock.TopRight,
                            subBlock.Color
                        ));
                        topLeftBlocks.Add(new SimpleBlock(
                            "tl_child",
                            new V(subBlock.BottomLeft.X, move.Point.Y),
                            new V(move.Point.X, subBlock.TopRight.Y),
                            subBlock.Color
                        ));
                        break;
                    }

                    // Case 2
                    if (subBlock.BottomLeft.X <= move.Point.X
                        && move.Point.X <= subBlock.TopRight.X
                        && move.Point.Y < subBlock.BottomLeft.Y)
                    {
                        topLeftBlocks.Add(new SimpleBlock(
                            "case2_tl_child",
                            subBlock.BottomLeft,
                            new V(move.Point.X, subBlock.TopRight.Y),
                            subBlock.Color
                        ));
                        topRightBlocks.Add(new SimpleBlock(
                            "case2_tr_child",
                            new V(move.Point.X, subBlock.BottomLeft.Y),
                            subBlock.TopRight,
                            subBlock.Color
                        ));
                        break;
                    }

                    // Case 8
                    if (subBlock.BottomLeft.X <= move.Point.X
                        && move.Point.X <= subBlock.TopRight.X
                        && move.Point.Y > subBlock.TopRight.Y)
                    {
                        bottomLeftBlocks.Add(new SimpleBlock(
                            "case8_bl_child",
                            subBlock.BottomLeft,
                            new V(move.Point.X, subBlock.TopRight.Y),
                            subBlock.Color
                        ));
                        bottomRightBlocks.Add(new SimpleBlock(
                            "case8_br_child",
                            new V(move.Point.X, subBlock.BottomLeft.Y),
                            subBlock.TopRight,
                            subBlock.Color
                        ));
                        break;
                    }

                    // Case 4
                    if (subBlock.BottomLeft.Y <= move.Point.Y
                        && move.Point.Y <= subBlock.TopRight.Y
                        && move.Point.X < subBlock.BottomLeft.X)
                    {
                        bottomRightBlocks.Add(new SimpleBlock(
                            "case4_br_child",
                            subBlock.BottomLeft,
                            new V(subBlock.TopRight.X, move.Point.Y),
                            subBlock.Color
                        ));
                        topRightBlocks.Add(new SimpleBlock(
                            "case4_tr_child",
                            new V(subBlock.BottomLeft.X, move.Point.Y),
                            subBlock.TopRight,
                            subBlock.Color
                        ));
                        break;
                    }

                    // Case 6
                    if (subBlock.BottomLeft.Y <= move.Point.Y
                        && move.Point.Y <= subBlock.TopRight.Y
                        && move.Point.X > subBlock.TopRight.X)
                    {
                        bottomLeftBlocks.Add(new SimpleBlock(
                            "case6_bl_child",
                            subBlock.BottomLeft,
                            new V(subBlock.TopRight.X, move.Point.Y),
                            subBlock.Color
                        ));
                        topLeftBlocks.Add(new SimpleBlock(
                            "case6_br_child",
                            new V(subBlock.BottomLeft.X, move.Point.Y),
                            subBlock.TopRight,
                            subBlock.Color
                        ));
                        break;
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

        return cost;
    }
}
