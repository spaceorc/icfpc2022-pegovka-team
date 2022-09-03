using System;
using System.IO;
using lib;
using lib.Enhancers;
using NUnit.Framework;

namespace tests;

public class EnhancerTests
{
    [Test]
    public void Run()
    {
        var problem = Screen.LoadProblem(24);
        var filename = FileHelper.FindFilenameUpwards("hand-solutions/problem-24-43138.txt");
        var moves = Moves.Parse(File.ReadAllText(filename));
        var canvas = new Canvas(problem);
        foreach (var move in moves)
        {
            // Console.WriteLine(move);
            canvas.Apply(move);
        }

        Console.WriteLine();
        Console.WriteLine(canvas.GetScore(problem));
        canvas.ToScreen().ToImage($"{filename}_original.png");

        // Console.WriteLine("Enchanced cuts:");
        // var enchanced = new CutEnhancer().Enhance(problem, moves);
        // canvas = new Canvas(problem);
        // foreach (var move in enchanced)
        // {
        //     // Console.WriteLine(move);
        //     canvas.Apply(move);
        // }
        //
        // Console.WriteLine();
        // Console.WriteLine(canvas.GetScore(problem));
        // canvas.ToScreen().ToImage($"{filename}_enchanced_cuts.png");
        //
        // Console.WriteLine("Enchanced colors:");
        // enchanced = new ColorEnhancer().Enhance(problem, moves);
        // canvas = new Canvas(problem);
        // foreach (var move in enchanced)
        // {
        //     // Console.WriteLine(move);
        //     canvas.Apply(move);
        // }
        //
        // Console.WriteLine();
        // Console.WriteLine(canvas.GetScore(problem));
        // canvas.ToScreen().ToImage($"{filename}_enchanced_colors.png");

        Console.WriteLine("Enhanced all:");
        var enhanced = new CombinedEnhancer(new CutEnhancer(), new ColorEnhancer()).Enhance(problem, moves);
        canvas = new Canvas(problem);
        foreach (var move in enhanced)
        {
            Console.WriteLine(move);
            canvas.Apply(move);
        }

        Console.WriteLine();
        Console.WriteLine(canvas.GetScore(problem));
        canvas.ToScreen().ToImage($"{filename}_enhanced_all.png");
    }

    [Test]
    public void Run2()
    {
        var problem = Screen.LoadProblem(2);
        var moves = Moves.Parse(File.ReadAllText(FileHelper.FindFilenameUpwards("hand-solutions/problem2.txt")));
        var canvas = new Canvas(problem);
        foreach (var move in moves)
        {
            Console.WriteLine(move);
            canvas.Apply(move);
        }

        Console.WriteLine();
        Console.WriteLine(canvas.GetScore(problem));
    }

    [Test]
    public void Run3()
    {
        var problem = Screen.LoadProblem(2);
        var moves = Moves.Parse(@"cut [0] [x] [100]
cut [0.1] [x] [200]
swap [0.0] [0.1.0]");
        var canvas = new Canvas(problem);
        foreach (var move in moves)
        {
            Console.WriteLine(move);
            canvas.Apply(move);
        }

        Console.WriteLine();
        Console.WriteLine(canvas.GetScore(problem));
    }
}
