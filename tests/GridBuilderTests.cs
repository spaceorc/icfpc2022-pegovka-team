using System;
using System.IO;
using lib;
using lib.Algorithms;
using NUnit.Framework;

namespace tests;

[TestFixture]
public class GridBuilderTests
{
    [Test]
    public void TestBuild()
    {
        var problem = Screen.LoadProblem(1);
        var grid = GridBuilder.BuildOptimalGrid(problem, (block, similarity) => similarity + Math.Sqrt(block.ScalarSize));

    }

    [Test]
    public void TestOptimizeRowHeights()
    {
        Func<SimpleBlock,double,double> estimateBlock = (block, similarity) => 1.0 * similarity + 400.0 * 400 / block.ScalarSize;

        var problem = Screen.LoadProblem(8);
        var grid = GridBuilder.BuildRegularGrid(problem, 11, 10);
        double estimation;

        problem.ToImage(Path.Combine(FileHelper.FindDirectoryUpwards("tests"), "regular.png"), grid);

        (grid, estimation) = GridBuilder.OptimizeRowHeights(problem, grid, estimateBlock);
        problem.ToImage(Path.Combine(FileHelper.FindDirectoryUpwards("tests"), "optimizedR.png"), grid);
        Console.Out.WriteLine($"rows={estimation}");

        (grid, estimation) = GridBuilder.OptimizeCellWidths(problem, grid, estimateBlock);
        problem.ToImage(Path.Combine(FileHelper.FindDirectoryUpwards("tests"), "optimizedC.png"), grid);
        Console.Out.WriteLine($"cells={estimation}");

        // (grid, estimation) = GridBuilder.OptimizeCellsViaMerge(problem, grid, estimateBlock);
        // problem.ToImage(Path.Combine(FileHelper.FindDirectoryUpwards("tests"), "optimizedM.png"), grid);
        // Console.Out.WriteLine($"merge={estimation}");

        var (moves, score) = new GridGuidedPainter(grid, problem).GetBestResult();
        problem.MovesToImage(moves, Path.Combine(FileHelper.FindDirectoryUpwards("tests"), "solved0.png"));
        Console.Out.WriteLine(score);

        for (int i = 0; i < 11; i++)
        {
            (grid, estimation) = GridBuilder.OptimizeCellsViaMerge(problem, grid, i, estimateBlock);
            problem.ToImage(Path.Combine(FileHelper.FindDirectoryUpwards("tests"), $"optimizedM{i}.png"), grid);
            Console.Out.WriteLine($"merge{i}={estimation}");
        }

        (moves, score) = new GridGuidedPainter(grid, problem).GetBestResult();
        problem.MovesToImage(moves, Path.Combine(FileHelper.FindDirectoryUpwards("tests"), "solved_after_merge.png"));
        Console.Out.WriteLine(score);

        (grid, estimation) = GridBuilder.OptimizeRowHeights(problem, grid, estimateBlock);
        problem.ToImage(Path.Combine(FileHelper.FindDirectoryUpwards("tests"), "optimizedR2.png"), grid);
        Console.Out.WriteLine($"rows={estimation}");

        (grid, estimation) = GridBuilder.OptimizeCellWidths(problem, grid, estimateBlock);
        problem.ToImage(Path.Combine(FileHelper.FindDirectoryUpwards("tests"), "optimizedC2.png"), grid);
        Console.Out.WriteLine($"cells={estimation}");

        (moves, score) = new GridGuidedPainter(grid, problem).GetBestResult();
        problem.MovesToImage(moves, Path.Combine(FileHelper.FindDirectoryUpwards("tests"), "solved_after_merge_and_move2.png"));
        Console.Out.WriteLine(score);

        (grid, estimation) = GridBuilder.OptimizeRowHeights(problem, grid, estimateBlock);
        problem.ToImage(Path.Combine(FileHelper.FindDirectoryUpwards("tests"), "optimizedR3.png"), grid);
        Console.Out.WriteLine($"rows={estimation}");

        (grid, estimation) = GridBuilder.OptimizeCellWidths(problem, grid, estimateBlock);
        problem.ToImage(Path.Combine(FileHelper.FindDirectoryUpwards("tests"), "optimizedC3.png"), grid);
        Console.Out.WriteLine($"cells={estimation}");

        (moves, score) = new GridGuidedPainter(grid, problem).GetBestResult();
        problem.MovesToImage(moves, Path.Combine(FileHelper.FindDirectoryUpwards("tests"), "solved_after_merge_and_move3.png"));
        Console.Out.WriteLine(score);
    }
}
