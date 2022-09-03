using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using Python.Included;
using Python.Runtime;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;

namespace lib.Algorithms
{
    public class GeometricMedian
    {
        private int[] Dr = new int[] { -1, 1, 0, 0, 0, 0, 0, 0 };
        private int[] Dg = new int[] { 0, 0, -1, 1, 0, 0, 0, 0 };
        private int[] Db = new int[] { 0, 0, 0, 0, -1, 1, 0, 0 };
        private int[] Da = new int[] { 0, 0, 0, 0, 0, 0, -1, 1 };

        public Rgba GetGeometricMedian(Screen screen, Block block)
        {
            var pixels = new List<Rgba>();
            for (int x = block.BottomLeft.X; x < block.TopRight.X; x++)
                for (int y = block.BottomLeft.Y; y < block.TopRight.Y; y++)
                {
                    var pixel = screen.Pixels[x, y];
                    pixels.Add(pixel);
                }

            return GetGeometricMedian(pixels.ToArray());
        }

        private Rgba GetGeometricMedian(Rgba[] points, double eps = 1e-4)
        {
            var (rm, gm, bm, am) = (0.0, 0.0, 0.0, 0.0);
            foreach (var p in points)
            {
                rm += p.R;
                gm += p.G;
                bm += p.B;
                am += p.A;
            }
            rm /= points.Length;
            gm /= points.Length;
            bm /= points.Length;
            am /= points.Length;

            var d = EuclidDistance(rm, gm, bm, am, points);

            var step = 128.0;
            while (step > eps)
            {
                var isDone = false;
                for (var i = 0; i < 8; i++)
                {
                    var nr = rm + step * Dr[i];
                    var ng = gm + step * Dg[i];
                    var nb = bm + step * Db[i];
                    var na = am + step * Da[i];

                    var t = EuclidDistance(nr, ng, nb, na, points);

                    if (t < d)
                    {
                        d = t;
                        rm = nr;
                        gm = ng;
                        bm = nb;
                        am = na;

                        isDone = true;
                        break;
                    }
                }

                if (!isDone)
                    step /= 2;
            }

            return new Rgba((int) Math.Round(rm), (int) Math.Round(gm), (int) Math.Round(bm), (int) Math.Round(am));
        }

        private double EuclidDistance(double r, double g, double b, double a, Rgba[] points)
        {
            var distance = 0.0;
            foreach (var other in points)
            {
                var rDist = (r - other.R) * (r - other.R);
                var gDist = (g - other.G) * (g - other.G);
                var bDist = (b - other.B) * (b - other.B);
                var aDist = (a - other.A) * (a - other.A);
                distance += Math.Sqrt(rDist + gDist + bDist + aDist);
            }
            return distance;
        }

        private IEnumerable<IList<T>> Product<T>(IEnumerable<T> source, int repeat = 1)
        {
            var result = new List<List<T>> { new List<T>() };
            foreach (var pool in Enumerable.Repeat(source, repeat))
            {
                var newResult = new List<List<T>>();
                foreach (var r in result)
                    foreach (var x in pool)
                    {
                        newResult.Add(r.Append(x).ToList());
                    }
                result = newResult;
            }
            foreach (var prod in result)
                yield return prod.ToList();
        }
    }
}
