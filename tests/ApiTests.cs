using System;
using lib;
using lib.api;
using NUnit.Framework;

namespace tests;

public class ApiTests
{
    [Test]
    [Explicit]
    public void TestGetAll()
    {
        var api = new Api();
        var response = api.GetAllProblems();
        Console.WriteLine(response);
    }

    [Test]
    [Explicit]
    public void TestPostSubmission()
    {
        var api = new Api();
        var response = api.PostSolution(8, " ");
        Console.WriteLine(response);
    }

    // [Test]
    // [Explicit]
    // public void TestDownloadProblem()
    // {
    //     var api = new Api();
    //     var response = api.DownloadProblem(8);
    //     Console.WriteLine(response);
    // }

    [Test]
    [Explicit]
    public void FetchAllProblems()
    {
        var api = new Api();

        for (var i = 1; i <= 15; i++)
        {
            var problem = api.FetchProblem(i).Result;
            ScreenRepo.SaveProblem(i, problem);
        }
    }
}
