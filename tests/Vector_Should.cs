using System.Linq;
using FluentAssertions;
using lib.Origami;
using NUnit.Framework;

namespace tests;

[TestFixture]
public class Vector_Should
{
    [TestCase("0,0 1,1", "0,1", "1,0")]
    [TestCase("0,0 1,0", "0,1", "0,-1")]
    [TestCase("10,10 11,10", "10,11", "10,9")]
    [TestCase("1/4,1/4 1,0", "1/16,1/16", "17/80,41/80")]
    public void BeMirrored(string segment, string point, string expectedPoint)
    {
        RSeg s = segment;
        VR p = point;
        var p2 = VR.Parse(expectedPoint);
        p.Reflect(s).Should().Be(p2);
        p2.Reflect(s).Should().Be(p);
    }

    [TestCase("-1,-1", "0,0", "1", "1,1")]
    [TestCase("9,9", "10,10", "1", "11,11")]
    [TestCase("0,1", "0,0", "1/2", "1,0")]
    [TestCase("0,1", "0,0", "1", "0,-1")]
    [TestCase("0,1", "0,0", "0", "0,1")]
    public void BeRotated(string point, string pivot, string x, string expectedPoint)
    {
        VR p = point;
        VR pv = pivot;
        var expected = VR.Parse(expectedPoint);
        p.Rotate(pv, Rational.Parse(x)).Should().Be(expected);
        expected.Rotate(pv, -Rational.Parse(x)).Should().Be(p);
    }


    [TestCase("0,0", "-1,-1 1,-1 1,1 -1,1", ExpectedResult = PointToPolygonPositionType.Inside)]
    [TestCase("0,0", "-1,1 1,1 1,-1 -1,-1", ExpectedResult = PointToPolygonPositionType.Inside)]
    [TestCase("0,1/100000", "-1,-1 1,-1 1,1 -1,1", ExpectedResult = PointToPolygonPositionType.Inside)]
    [TestCase("2,0", "-1,-1 1,-1 1,1 -1,1", ExpectedResult = PointToPolygonPositionType.Outside)]
    [TestCase("-1,-1", "-1,-1 1,-1 1,1 -1,1", ExpectedResult = PointToPolygonPositionType.Boundary)]
    [TestCase("-1,0", "-1,-1 1,-1 1,1 -1,1", ExpectedResult = PointToPolygonPositionType.Boundary)]
    [TestCase("-1,0", "-1,-1 1,-1 1,1 -1,1", ExpectedResult = PointToPolygonPositionType.Boundary)]
    [TestCase("-1,1", "-1,-1 1,-1 1,1 -1,1", ExpectedResult = PointToPolygonPositionType.Boundary)]
    [TestCase("-1,-1", "-1,-1 1,-1 1,1 -1,1", ExpectedResult = PointToPolygonPositionType.Boundary)]
    [TestCase("0,-1", "-1,-1 1,-1 1,1 -1,1", ExpectedResult = PointToPolygonPositionType.Boundary)]
    public PointToPolygonPositionType BeInValidPositionToPolygon(string point, string polygonDef)
    {
        VR p = point;
        var polygon = new RPolygon(polygonDef.Split(' ').Select(VR.Parse).ToArray());
        return p.GetPositionToPolygon(polygon);
    }
}
