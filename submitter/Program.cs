using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using lib;
using lib.api;

namespace submitter
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var api = new Api();
            var r = api.FetchProblem(1);
            Console.WriteLine(r.Result);
        }
    }
}
