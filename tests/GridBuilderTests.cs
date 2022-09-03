using System;
using lib;
using NUnit.Framework;

namespace tests;

[TestFixture]
public class GridBuilderTests
{
    [Test]
    public void TestBuild()
    {
        var problem = Screen.LoadProblem(1);
        var grid = GridBuilder.BuildOptimalGrid(problem, (block, similarity) => similarity / Math.Sqrt(block.ScalarSize));

    }
}
