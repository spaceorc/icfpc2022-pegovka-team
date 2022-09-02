using System;

namespace lib;

public record Rgba(int R, int G, int B, int A)
{
    public double DiffTo(Rgba other)
    {
        var rDist = (R - other.R) * (R - other.R);
        var gDist = (G - other.G) * (G - other.G);
        var bDist = (B - other.B) * (B - other.B);
        var aDist = (A - other.A) * (A - other.A);
        var distance = Math.Sqrt(rDist + gDist + bDist + aDist);
        return distance;
    }
}
