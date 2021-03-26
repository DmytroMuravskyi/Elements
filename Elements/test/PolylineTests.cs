using System.Collections.Generic;
using System.Linq;
using Elements.Tests;
using Xunit;

namespace Elements.Geometry.Tests
{
    public class PolylineTests : ModelTest
    {
        public PolylineTests()
        {
            this.GenerateIfc = false;
        }

        [Fact, Trait("Category", "Examples")]
        public void Polyline()
        {
            this.Name = "Elements_Geometry_Polyline";

            // <example>
            var a = new Vector3();
            var b = new Vector3(10, 10);
            var c = new Vector3(20, 5);
            var d = new Vector3(25, 10);

            var pline = new Polyline(new[] { a, b, c, d });
            var offset = pline.Offset(1, EndType.Square);
            // </example>

            this.Model.AddElement(new ModelCurve(pline, BuiltInMaterials.XAxis));
            this.Model.AddElement(new ModelCurve(offset[0], BuiltInMaterials.YAxis));

            Assert.Equal(4, pline.Vertices.Count);
            Assert.Equal(3, pline.Segments().Length);
        }

        [Fact]
        public void Polyline_ClosedOffset()
        {
            var length = 10;
            var offsetAmt = 1;
            var a = new Vector3();
            var b = new Vector3(length, 0);
            var pline = new Polyline(new[] { a, b });
            var offsetResults = pline.Offset(offsetAmt, EndType.Square);
            Assert.Single<Polygon>(offsetResults);
            var offsetResult = offsetResults[0];
            Assert.Equal(4, offsetResult.Vertices.Count);
            // offsets to a rectangle that's offsetAmt longer than the segment in
            // each direction, and 2x offsetAmt in width, so the long sides are
            // each length + 2x offsetAmt, and the short sides are each 2x offsetAmt.
            var targetLength = 2 * length + 8 * offsetAmt;
            Assert.Equal(targetLength, offsetResult.Length(), 2);
        }

        [Fact]
        public void Polyline_OffsetOnSide_SingleSegment()
        {
            var line = new Polyline(new List<Vector3>(){
                new Vector3(0, 0, 0),
                new Vector3(10, 0, 0),
            });

            var polygons = line.OffsetOnSide(2, false);
            Assert.Single(polygons);

            // A rectangle extruded down from the original line.
            Assert.Equal(new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, -2, 0), new Vector3(10, -2, 0), new Vector3(10, 0, 0) }, polygons.First().Vertices);
        }

        [Fact]
        public void Polyline_OffsetOnSide_SingleSegmentFlipped()
        {
            var line = new Polyline(new List<Vector3>(){
                new Vector3(0, 0, 0),
                new Vector3(10, 0, 0),
            });
            var polygons = line.OffsetOnSide(2, true);
            Assert.Single(polygons);

            // A rectangle extruded up from the original line.
            Assert.Equal(new Vector3[] { new Vector3(10, 0, 0), new Vector3(10, 2, 0), new Vector3(0, 2, 0), new Vector3(0, 0, 0) }, polygons.First().Vertices);
        }

        [Fact]
        public void Polyline_OffsetOnSide_RightAngleJoin()
        {
            var line = new Polyline(new List<Vector3>(){
                new Vector3(0, 0, 0),
                new Vector3(10, 0, 0),
                new Vector3(10, 10, 0),
            });
            var polygons = line.OffsetOnSide(2, false);
            Assert.Equal(2, polygons.Count());

            // Extruding the two lines into rectangles, but joining at (12, -2) - forming a 45 degree join edge.
            Assert.Equal(new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, -2, 0), new Vector3(12, -2, 0), new Vector3(10, 0, 0) }, polygons[0].Vertices);
            Assert.Equal(new Vector3[] { new Vector3(10, 0, 0), new Vector3(12, -2, 0), new Vector3(12, 10, 0), new Vector3(10, 10, 0) }, polygons[1].Vertices);
        }

        [Fact]
        public void Polyline_OffsetOnSide_OuterJoin()
        {
            var line = new Polyline(new List<Vector3>(){
                new Vector3(0, 0, 0),
                new Vector3(10, 0, 0),
                new Vector3(0, 5, 0),
            });
            var polygons = line.OffsetOnSide(2, false);
            Assert.Equal(2, polygons.Count());

            // If the join is outside the polyline, and the intersection point would be far away, blunt the edge by adding a line.
            var bottomOfFlatEdge = new Vector3(12.683281572999748, 0.8944271909999159, 0);
            var topOfFlatEdge = new Vector3(0.8944271909999159, 6.7888543819998315, 0);
            Assert.Equal(new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, -2, 0), new Vector3(12, -2, 0), bottomOfFlatEdge, new Vector3(10, 0, 0) }, polygons[0].Vertices);
            Assert.Equal(new Vector3[] { new Vector3(10, 0, 0), bottomOfFlatEdge, topOfFlatEdge, new Vector3(0, 5, 0) }, polygons[1].Vertices);
        }

        [Fact]
        public void Polyline_OffsetOnSide_ClosedLoop()
        {
            // A square.
            var line = new Polyline(new List<Vector3>(){
                new Vector3(0, 0, 0),
                new Vector3(10, 0, 0),
                new Vector3(10, 10, 0),
                new Vector3(0, 10, 0),
                new Vector3(0, 0, 0),
            });
            var polygons = line.OffsetOnSide(2, false);
            Assert.Equal(4, polygons.Count());

            // This is generally identical to the right angle test, except every rectangle is a trapezoid with 45 degree angles
            Assert.Equal(new Vector3[] { new Vector3(0, 10, 0), new Vector3(-2, 12, 0), new Vector3(-2, -2, 0), new Vector3(0, 0, 0) }, polygons[0].Vertices);
            Assert.Equal(new Vector3[] { new Vector3(0, 0, 0), new Vector3(-2, -2, 0), new Vector3(12, -2, 0), new Vector3(10, 0, 0) }, polygons[1].Vertices);
            Assert.Equal(new Vector3[] { new Vector3(10, 0, 0), new Vector3(12, -2, 0), new Vector3(12, 12, 0), new Vector3(10, 10, 0) }, polygons[2].Vertices);
            Assert.Equal(new Vector3[] { new Vector3(10, 10, 0), new Vector3(12, 12, 0), new Vector3(-2, 12, 0), new Vector3(0, 10, 0) }, polygons[3].Vertices);
        }

        [Fact]
        public void SharedSegments_OpenMirroredSquares_NoResults()
        {
            var s = 1;

            var a = new Polygon(new List<Vector3>(){
                new Vector3(0, s, 0),
                new Vector3(-s, s, 0),
                new Vector3(-s, 0, 0),
                new Vector3(0, 0, 0),
            });
            var b = new Polygon(new List<Vector3>(){
                new Vector3(0, s, 0),
                new Vector3(s, s, 0),
                new Vector3(s, 0, 0),
                new Vector3(0, 0, 0),
            });

            var matches = Polygon.SharedSegments(a, b);

            Assert.Empty(matches);
        }
    }
}