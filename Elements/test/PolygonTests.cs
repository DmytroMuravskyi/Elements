using System;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;
using Elements.Tests;
using Elements.Serialization.glTF;
using System.IO;

namespace Elements.Geometry.Tests
{
    public class PolygonTests : ModelTest
    {
        private readonly ITestOutputHelper _output;

        public PolygonTests(ITestOutputHelper output)
        {
            this._output = output;
            this.GenerateIfc = false;
        }

        [Fact, Trait("Category", "Examples")]
        public void PolygonConstruct()
        {
            this.Name = "Elements_Geometry_Polygon";

            // <example>
            // Create a polygon.
            var star = Polygon.Star(5, 3, 5);
            // </example>

            this.Model.AddElement(new ModelCurve(star));
        }

        [Fact]
        public void Centroid()
        {
            // Square in Quadrant I
            var polygon = new Polygon
            (
                new[]
                {
                    Vector3.Origin,
                    new Vector3(6.0, 0.0),
                    new Vector3(6.0, 6.0),
                    new Vector3(0.0, 6.0),
                }
            );
            var centroid = polygon.Centroid();
            Assert.Equal(3.0, centroid.X);
            Assert.Equal(3.0, centroid.Y);

            // Square in Quadrant II
            polygon = new Polygon
            (
                new[]
                {
                    Vector3.Origin,
                    new Vector3(-6.0, 0.0),
                    new Vector3(-6.0, 6.0),
                    new Vector3(0.0, 6.0),
                }
            );
            centroid = polygon.Centroid();
            Assert.Equal(-3.0, centroid.X);
            Assert.Equal(3.0, centroid.Y);

            // Square in Quadrant III
            polygon = new Polygon
            (
                new[]
                {
                    Vector3.Origin,
                    new Vector3(-6.0, 0.0),
                    new Vector3(-6.0, -6.0),
                    new Vector3(0.0, -6.0),
                }
            );
            centroid = polygon.Centroid();
            Assert.Equal(-3.0, centroid.X);
            Assert.Equal(-3.0, centroid.Y);

            // Square in Quadrant IV
            polygon = new Polygon
            (
                new[]
                {
                    Vector3.Origin,
                    new Vector3(6.0, 0.0),
                    new Vector3(6.0, -6.0),
                    new Vector3(0.0, -6.0),
                }
            );
            centroid = polygon.Centroid();
            Assert.Equal(3.0, centroid.X);
            Assert.Equal(-3.0, centroid.Y);

            // Bow Tie in Quadrant I
            polygon = new Polygon
            (
                new[]
                {
                    new Vector3(1.0, 1.0),
                    new Vector3(4.0, 4.0),
                    new Vector3(7.0, 1.0),
                    new Vector3(7.0, 9.0),
                    new Vector3(4.0, 6.0),
                    new Vector3(1.0, 9.0)
                }
            );
            centroid = polygon.Centroid();
            Assert.Equal(4.0, centroid.X);
            Assert.Equal(5.0, centroid.Y);

            // Bow Tie in Quadrant III
            polygon = new Polygon
            (
                new[]
                {
                    new Vector3(-1.0, -1.0),
                    new Vector3(-4.0, -4.0),
                    new Vector3(-7.0, -1.0),
                    new Vector3(-7.0, -9.0),
                    new Vector3(-4.0, -6.0),
                    new Vector3(-1.0, -9.0)
                }
            );
            centroid = polygon.Centroid();
            Assert.Equal(-4.0, centroid.X);
            Assert.Equal(-5.0, centroid.Y);
        }

        [Fact]
        public void Contains()
        {
            var v1 = new Vector3();
            var v2 = new Vector3(7.5, 7.5);
            var p1 = new Polygon
            (
                new[]
                {
                    new Vector3(0.0, 0.0),
                    new Vector3(20.0, 0.0),
                    new Vector3(20.0, 20.0),
                    new Vector3(0.0, 20.0)
                }
            );
            var p2 = new Polygon
            (
                new[]
                {
                    new Vector3(0.0, 0.0),
                    new Vector3(10.0, 5.0),
                    new Vector3(10.0, 10.0),
                    new Vector3(5.0, 10.0)
                }
            );
            var p3 = new Polygon
            (
                new[]
                {
                    new Vector3(5.0, 5.0),
                    new Vector3(10.0, 5.0),
                    new Vector3(10.0, 10.0),
                    new Vector3(5.0, 10.0)
                }
            );

            Assert.False(p1.Contains(v1));
            Assert.True(p1.Contains(v2));
            Assert.False(p1.Contains(p2));
            Assert.True(p1.Contains(p3));
            Assert.False(p3.Contains(p1));
        }

        [Fact]
        public void Covers()
        {
            var v1 = new Vector3();
            var v2 = new Vector3(7.5, 7.5);
            var p1 = new Polygon
            (
                new[]
                {
                new Vector3(0.0, 0.0),
                new Vector3(20.0, 0.0),
                new Vector3(20.0, 20.0),
                new Vector3(0.0, 20.0)
                }
            );
            var p2 = new Polygon
            (
                new[]
                {
                new Vector3(0.0, 0.0),
                new Vector3(10.0, 5.0),
                new Vector3(10.0, 10.0),
                new Vector3(5.0, 10.0)
                }
            );
            var p3 = new Polygon
            (
                new[]
                {
                new Vector3(5.0, 5.0),
                new Vector3(10.0, 5.0),
                new Vector3(10.0, 10.0),
                new Vector3(5.0, 10.0)
                }
            );
            Assert.True(p1.Covers(v1));
            Assert.True(p1.Covers(p2.Reversed()));
            Assert.True(p3.Covers(v2));
            Assert.False(p3.Covers(v1));
            Assert.True(p1.Covers(p3));
            Assert.True(p1.Covers(p2));
            Assert.False(p3.Covers(p1));
        }

        [Fact]
        public void Disjoint()
        {
            var v1 = new Vector3();
            var v2 = new Vector3(27.5, 27.5);
            var p1 = new Polygon
            (
                new[]
                {
                new Vector3(0.0, 0.0),
                new Vector3(20.0, 0.0),
                new Vector3(20.0, 20.0),
                new Vector3(0.0, 20.0)
                }
            );
            var p2 = new Polygon
            (
                new[]
                {
                new Vector3(0.0, 0.0),
                new Vector3(10.0, 5.0),
                new Vector3(10.0, 10.0),
                new Vector3(5.0, 10.0)
                }
            );
            var p3 = new Polygon
            (
                new[]
                {
                new Vector3(25.0, 25.0),
                new Vector3(210.0, 25.0),
                new Vector3(210.0, 210.0),
                new Vector3(25.0, 210.0)
                }
            );
            var ps = new List<Polygon> { p2, p3 };

            Assert.True(p1.Disjoint(v2));
            Assert.False(p1.Disjoint(v1));
            Assert.True(p1.Disjoint(p3));
            Assert.False(p1.Disjoint(p2));
        }

        [Fact]
        public void Intersects()
        {
            var p1 = new Polygon
            (
                new[]
                {
                new Vector3(0.0, 0.0),
                new Vector3(20.0, 0.0),
                new Vector3(20.0, 20.0),
                new Vector3(0.0, 20.0)
                }
            );
            var p2 = new Polygon
            (
                new[]
                {
                new Vector3(0.0, 0.0),
                new Vector3(10.0, 5.0),
                new Vector3(10.0, 10.0),
                new Vector3(5.0, 10.0)
                }
            );
            var p3 = new Polygon
            (
                new[]
                {
                new Vector3(25.0, 25.0),
                new Vector3(210.0, 25.0),
                new Vector3(210.0, 210.0),
                new Vector3(25.0, 210.0)
                }
            );

            Assert.True(p1.Intersects(p2));
            Assert.False(p1.Intersects(p3));
        }

        [Fact]
        public void Touches()
        {
            var p1 = new Polygon
            (
                new[]
                {
                    new Vector3(),
                    new Vector3(4.0, 0.0),
                    new Vector3(4.0, 4.0),
                    new Vector3(0.0, 4.0)
                }
            );
            var p2 = new Polygon
            (
                new[]
                {
                    new Vector3(0.0, 1.0),
                    new Vector3(7.0, 1.0),
                    new Vector3(7.0, 2.0),
                    new Vector3(0.0, 2.0)
                }
            );
            var p3 = new Polygon
            (
                new[]
                {
                    new Vector3(),
                    new Vector3(4.0, 0.0),
                    new Vector3(4.0, 4.0),
                    new Vector3(0.0, 4.0)
                }
            );
            var p4 = new Polygon
            (
                new[]
                {
                    new Vector3(4.0, 0.0),
                    new Vector3(8.0, 0.0),
                    new Vector3(8.0, 4.0),
                    new Vector3(4.0, 8.0)
                }
            );
            Assert.False(p1.Touches(p2));
            Assert.True(p3.Touches(p4));
        }

        [Fact]
        public void Difference()
        {
            var p1 = new Polygon
            (
                new[]
                {
                    new Vector3(),
                    new Vector3(4, 0),
                    new Vector3(4, 4),
                    new Vector3(0, 4)
                }
            );
            var p2 = new Polygon
            (
                new[]
                {
                    new Vector3(3, 1),
                    new Vector3(7, 1),
                    new Vector3(7, 5),
                    new Vector3(3, 5)
                }
            );
            var vertices = p1.Difference(p2).First().Vertices;

            Assert.Contains(vertices, p => p.IsAlmostEqualTo(0.0, 0.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(4.0, 0.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(4.0, 1.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(3.0, 1.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(3.0, 4.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(0.0, 4.0));
        }

        [Fact]
        public void Intersection()
        {
            var p1 = new Polygon
            (
                new[]
                {
                    new Vector3(),
                    new Vector3(4.0, 0.0),
                    new Vector3(4.0, 4.0),
                    new Vector3(0.0, 4.0)
                }
            );
            var p2 = new Polygon
            (
                new[]
                {
                    new Vector3(3.0, 1.0),
                    new Vector3(7.0, 1.0),
                    new Vector3(7.0, 5.0),
                    new Vector3(3.0, 5.0)
                }
            );
            var p3 = new Polygon
            (
                new[]
                {
                    new Vector3(3.0, 2.0),
                    new Vector3(6.0, 2.0),
                    new Vector3(6.0, 3.0),
                    new Vector3(3.0, 3.0)
                }
            );
            var ps = new List<Polygon> { p2, p3 };
            var vertices = p1.Intersection(p2).First().Vertices;

            Assert.Contains(vertices, p => p.IsAlmostEqualTo(3.0, 1.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(4.0, 1.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(4.0, 4.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(3.0, 4.0));
        }

        [Fact]
        public void UnionAll()
        {
            var polygons = new List<Polygon> {
                Polygon.Rectangle(10, 10),
                Polygon.Rectangle(10, 10).TransformedPolygon(new Transform(5,5,0))
            };

            var result = Polygon.UnionAll(polygons).First();
            Assert.Equal(175, result.Area());
        }

        [Fact]
        public void Union()
        {
            var p1 = new Polygon
            (
                new[]
                {
                    new Vector3(),
                    new Vector3(4.0, 0.0),
                    new Vector3(4.0, 4.0),
                    new Vector3(0.0, 4.0)
                }
            );
            var p2 = new Polygon
            (
                new[]
                {
                    new Vector3(3.0, 1.0),
                    new Vector3(7.0, 1.0),
                    new Vector3(7.0, 5.0),
                    new Vector3(3.0, 5.0)
                }
            );
            var p3 = new Polygon
            (
                new[]
                {
                    new Vector3(3.0, 2.0),
                    new Vector3(8.0, 2.0),
                    new Vector3(8.0, 3.0),
                    new Vector3(3.0, 3.0)
                }
            );
            var ps = new List<Polygon> { p2, p3 };

            var vertices = p1.Union(p2).Vertices;

            Assert.Contains(vertices, p => p.IsAlmostEqualTo(0.0, 0.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(4.0, 0.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(4.0, 1.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(7.0, 1.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(7.0, 5.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(3.0, 5.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(3.0, 4.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(0.0, 4.0));

            vertices = p1.Union(ps).Vertices;

            Assert.Contains(vertices, p => p.IsAlmostEqualTo(0.0, 0.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(4.0, 0.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(4.0, 1.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(7.0, 1.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(7.0, 2.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(8.0, 2.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(8.0, 3.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(7.0, 3.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(7.0, 5.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(3.0, 5.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(3.0, 4.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(0.0, 4.0));

        }

        [Fact]
        public void XOR()
        {
            var p1 = new Polygon
            (
                new[]
                {
                    new Vector3(),
                    new Vector3(4, 0),
                    new Vector3(4, 4),
                    new Vector3(0, 4)
                }
            );
            var p2 = new Polygon
            (
                new[]
                {
                    new Vector3(3, 1),
                    new Vector3(7, 1),
                    new Vector3(7, 5),
                    new Vector3(3, 5)
                }
            );
            var vertices = p1.XOR(p2).First().Vertices;

            Assert.Contains(vertices, p => p.IsAlmostEqualTo(4.0, 1.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(7.0, 1.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(7.0, 5.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(3.0, 5.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(3.0, 4.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(0.0, 4.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(0.0, 0.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(4.0, 0.0));
        }

        [Fact]
        public void Offset()
        {
            var a = new Vector3();
            var b = new Vector3(2, 5);
            var c = new Vector3(-3, 5);

            var plinew = new Polygon(new[] { a, b, c });
            var offset = plinew.Offset(0.2);

            Assert.True(offset.Length == 1);
        }

        [Fact]
        public void TwoPeaks__Offset_2Polylines()
        {
            var a = new Vector3();
            var b = new Vector3(5, 0);
            var c = new Vector3(5, 5);
            var d = new Vector3(0, 1);
            var e = new Vector3(-5, 5);
            var f = new Vector3(-5, 0);

            var plinew = new Polygon(new[] { a, b, c, d, e, f });
            var offset = plinew.Offset(-0.5);
            Assert.Equal(2, offset.Count());
        }

        [Fact]
        public void Construct()
        {
            var a = new Vector3();
            var b = new Vector3(1, 0);
            var c = new Vector3(1, 1);
            var d = new Vector3(0, 1);
            var p = new Polygon(new[] { a, b, c, d });
            Assert.Equal(4, p.Segments().Count());
        }

        [Fact]
        public void Area()
        {
            var a = Polygon.Rectangle(1.0, 1.0);
            Assert.Equal(1.0, a.Area());

            var b = Polygon.Rectangle(2.0, 2.0);
            Assert.Equal(4.0, b.Area());

            var p1 = Vector3.Origin;
            var p2 = Vector3.XAxis;
            var p3 = new Vector3(1.0, 1.0);
            var p4 = new Vector3(0.0, 1.0);
            var pp = new Polygon(new[] { p1, p2, p3, p4 });
            Assert.Equal(1.0, pp.Area());
        }

        [Fact]
        public void Length()
        {
            var a = new Vector3();
            var b = new Vector3(1, 0);
            var c = new Vector3(1, 1);
            var d = new Vector3(0, 1);
            var p = new Polygon(new[] { a, b, c, d });
            Assert.Equal(4, p.Length());
        }

        [Fact]
        public void PointAt()
        {
            var a = new Vector3();
            var b = new Vector3(1, 0);
            var c = new Vector3(1, 1);
            var d = new Vector3(0, 1);
            var p = new Polygon(new[] { a, b, c, d });
            Assert.Equal(4, p.Segments().Count());
            Assert.Equal(new Vector3(1.0, 1.0), p.PointAt(0.5));

            var r = Polygon.Rectangle(2, 2);
            Assert.Equal(new Vector3(1, 1, 0), r.PointAt(0.5));
        }

        [Fact]
        public void TwoPeaks_Offset_2Polylines()
        {
            var a = new Vector3();
            var b = new Vector3(5, 0);
            var c = new Vector3(5, 5);
            var d = new Vector3(0, 1);
            var e = new Vector3(-5, 5);
            var f = new Vector3(-5, 0);

            var plinew = new Polygon(new[] { a, b, c, d, e, f });
            var offset = plinew.Offset(-0.5);
            Assert.Equal(2, offset.Count());
        }

        [Fact]
        public void SameVertices_RemovesDuplicates()
        {
            var a = new Vector3();
            var b = new Vector3(10, 0, 0);
            var c = new Vector3(10, 3, 0);
            var polygon = new Polygon(new[] { a, a, a, b, c });
            Assert.Equal(3, polygon.Segments().Count());
        }

        [Fact]
        public void UnionAllSequential()
        {
            Name = "UnionAllSequential";
            // sample data contributed by Marco Juliani
            var polygonsA = JsonConvert.DeserializeObject<List<Polygon>>(File.ReadAllText("../../../models/Geometry/testUnionAll.json"));
            var polygonsB = JsonConvert.DeserializeObject<List<Polygon>>(File.ReadAllText("../../../models/Geometry/testUnionAll_2.json"));
            var unionA = Polygon.UnionAll(polygonsA);
            var unionB = Polygon.UnionAll(polygonsB);
            Model.AddElements(unionA.Select(u => new ModelCurve(u)));
            Model.AddElements(unionB.Select(u => new ModelCurve(u)));
        }

        [Fact]
        public void ConvexHullAndBoundingRect()
        {
            Name = "Convex Hull";
            var rand = new Random();
            // fuzz test
            for (int test = 0; test < 50; test++)
            {
                var basePt = new Vector3((test % 5) * 12, (test - (test % 5)) / 5 * 12);
                var pts = new List<Vector3>();
                for (int i = 0; i < 20; i++)
                {
                    pts.Add(basePt + new Vector3(rand.NextDouble() * 10, rand.NextDouble() * 10));
                }
                var modelPts = pts.Select(p => new ModelCurve(new Circle(p, 0.2)));
                var hull = ConvexHull.FromPoints(pts);
                var boundingRect = Polygon.FromAlignedBoundingBox2d(pts);
                Model.AddElements(modelPts);
                Model.AddElements(boundingRect);
                Model.AddElement(hull);
            }

            // handle collinear pts test
            var coPts = new List<Vector3> {
                new Vector3(0,0),
                new Vector3(1,0),
                new Vector3(2,0),
                new Vector3(4,0),
                new Vector3(10,0),
                new Vector3(10,5),
                new Vector3(10,10)
            };
            var coHull = ConvexHull.FromPoints(coPts);
            Assert.Equal(50, coHull.Area());
        }

        [Fact]
        public void Reverse()
        {
            var a = Polygon.Ngon(3, 1.0);
            var b = a.Reversed();

            // Check that the vertices are properly reversed.
            Assert.Equal(a.Vertices.Reverse(), b.Vertices);
            var t = new Transform();
            var c = a.Transformed(t);
            var l = new Line(Vector3.Origin, new Vector3(0.0, 0.5, 0.5));
            var transforms = l.Frames(0.0, 0.0);

            var start = (Polygon)a.Transformed(transforms[0]);
            var end = (Polygon)b.Transformed(transforms[1]);

            var n1 = start.Plane();
            var n2 = end.Plane();

            // Check that the start and end have opposing normals.
            var dot = n1.Normal.Dot(n2.Normal);
            Assert.Equal(-1.0, dot, 5);
        }

        [Fact]
        public void Planar()
        {
            var a = Vector3.Origin;
            var b = new Vector3(5, 0, 0);
            var c = new Vector3(5, 0, 5);
            var p = new Polygon(new[] { a, b, c });
        }

        [Fact]
        public void PointInternal()
        {
            Name = "PointInternal";
            var extremelyConcavePolygon = new Polygon(new[] {
                new Vector3(
                        5.894565217391305,
                        0.0,
                        0.0
                        ),
                        new Vector3(
                        5.894565217391305,
                        0.69347826086956488,
                        0.0
                        ),
                        new Vector3(
                        0.19082958701549974,
                        0.13222235119778919,
                        0.0
                        ),
                        new Vector3(
                        0.0,
                        2.3964022904166224,
                        0.0
                        ),
                        new Vector3(
                        6.2310064053894925,
                        3.9731847432442322,
                        0.0
                        ),
                        new Vector3(
                        5.9682093299182242,
                        4.5513383092810225,
                        0.0
                        ),
                        new Vector3(
                        -0.085624933213594254,
                        2.4045397339325851,
                        0.0
                        ),
                        new Vector3(
                        0.0,
                        0.0,
                        0.0
                        )
            });
            var pointInternal = extremelyConcavePolygon.PointInternal();
            Assert.True(extremelyConcavePolygon.Contains(pointInternal));
            Model.AddElement(extremelyConcavePolygon);
            Curve.MinimumChordLength = 0.001;
            Model.AddElement(new Circle(pointInternal, 0.02));
            Curve.MinimumChordLength = 0.1;
        }

        [Fact]
        public void PolygonSplitWithPolyline()
        {
            Name = "PolygonSplitWithPolyline";
            var random = new Random(23);

            // Simple Split
            var polygon = Polygon.Rectangle(5, 5);
            var polyline = new Polyline(new[] { new Vector3(-3, 0), new Vector3(0, 1), new Vector3(3, 0) });
            var splitResults = polygon.Split(polyline);
            Assert.Equal(2, splitResults.Count);

            // Convex shape split
            var convexPolygon = new Polygon(new[] {
                new Vector3(-2.5,-2.5),
                new Vector3(2.5,-2.5),
                new Vector3(2.5,-1),
                new Vector3(1,-1),
                new Vector3(1,1),
                new Vector3(2.5,1),
                new Vector3(2.5,2.5),
                new Vector3(-2.5,2.5)
            });
            var convexSplitPolyline = new Polyline(new[] {
                new Vector3(1.5, -3),
                new Vector3(1.5,3)
            });

            var splitResults2 = convexPolygon.Split(convexSplitPolyline);
            Model.AddElements(splitResults2.Select(s => new Panel(s, random.NextMaterial())));
            Assert.True(splitResults2.Count == 3);

            // doesn't intersect, no change
            var shiftedPolygon = convexPolygon.TransformedPolygon(new Transform(6, 0, 0));
            var splitResults3 = shiftedPolygon.Split(convexSplitPolyline);
            Assert.True(splitResults3.Count == 1);
            Model.AddElements(splitResults3.Select(s => new Panel(s, random.NextMaterial())));

            // totally contained, no change
            var internalPl = new Polyline(new[] { new Vector3(6 - 2.5 + 0.5, -2), new Vector3(6 - 2.5 + 0.5, 2) });
            Model.AddElement(internalPl);
            var splitResults4 = shiftedPolygon.Split(internalPl);
            Assert.True(splitResults4.Count == 1);

            // split with pass through vertex
            var cornerPg = new Polygon(new[] { new Vector3(0, 10), new Vector3(3, 10), new Vector3(3, 13), new Vector3(0, 13) });
            var cornerPl = new Polyline(new[] {
                new Vector3(-1, 9),
                new Vector3(3, 13)
            });
            Model.AddElements(cornerPg, cornerPl);
            var splitResults5 = cornerPg.Split(cornerPl);
            Assert.True(splitResults5.Count == 2);
            Model.AddElements(splitResults5.Select(s => new Panel(s, random.NextMaterial())));

            // pass through incompletely, no change
            var cornerPl2 = new Polyline(new[] {
                new Vector3(-1, 9),
                new Vector3(2, 11)
            });

            var splitResults6 = cornerPg.Split(cornerPl2);
            Assert.True(splitResults6.Count == 1);

            // overlap at edge, no change.ioU
            var rect2 = Polygon.Ngon(5, 5).TransformedPolygon(new Transform(-6, -8, 0));
            var splitCrv = rect2.Segments()[3];
            var splitResults7 = rect2.Split(splitCrv.ToPolyline(1));
            Assert.True(splitResults7.Count == 1);
            Model.AddElements(splitResults7.Select(s => new Panel(s, random.NextMaterial())));


            // fuzz test
            var shifted = convexPolygon.TransformedPolygon(new Transform(12, 0, 0));
            var collection = new List<Polygon> { shifted };
            var bbox = new BBox3(shifted);
            var rect = Polygon.Rectangle(bbox.Min, bbox.Max);
            for (int i = 0; i < 20; i++)
            {
                var randomLine = new Polyline(new[] {
                    rect.PointAt(random.NextDouble()),
                    bbox.Min + new Vector3((bbox.Max.X - bbox.Min.X) * random.NextDouble(), (bbox.Max.Y - bbox.Min.Y) * random.NextDouble()),
                    rect.PointAt(random.NextDouble()) });
                collection = collection.SelectMany(c => c.Split(randomLine)).ToList();
            }
            Model.AddElements(collection.Select(c => new Panel(c, random.NextMaterial())));

        }

        [Fact]
        public void DeserializesWithoutDiscriminator()
        {
            // We've received a Polygon and we know that we're receiving
            // a Polygon. The Polygon should deserialize without a
            // discriminator.
            string json = @"
            {
                ""Vertices"": [
                    {""X"":1,""Y"":1,""Z"":2},
                    {""X"":2,""Y"":1,""Z"":2},
                    {""X"":2,""Y"":2,""Z"":2},
                    {""X"":1,""Y"":2,""Z"":2}
                ]
            }
            ";
            var polygon = JsonConvert.DeserializeObject<Polygon>(json);

            // We've created a new Polygon, which will have a discriminator
            // because it was created using the JsonInheritanceConverter.
            var newJson = JsonConvert.SerializeObject(polygon);
            var newPolygon = (Polygon)JsonConvert.DeserializeObject<Polygon>(newJson);

            Assert.Equal(polygon.Vertices.Count, newPolygon.Vertices.Count);
        }

        [Fact]
        public void Fillet()
        {
            var model = new Model();

            var shape1 = Polygon.L(10, 10, 5);
            var contour1 = shape1.Fillet(0.5);
            var poly1 = contour1.ToPolygon();
            var mass1 = new Mass(poly1);
            Assert.Equal(shape1.Segments().Count() * 2, contour1.Count());

            var t = new Transform(15, 0, 0);
            var shape2 = Polygon.Ngon(3, 5);
            var contour2 = shape2.Fillet(0.5);
            var poly2 = contour2.ToPolygon();
            var mass2 = new Mass(poly2, transform: t);
            Assert.Equal(shape2.Segments().Count() * 2, contour2.Count());

            var shape3 = Polygon.Star(5, 3, 5);
            var contour3 = shape3.Fillet(0.5);
            t = new Transform(30, 0, 0);
            var poly3 = contour3.ToPolygon();
            var mass3 = new Mass(poly3, transform: t);
            Assert.Equal(shape3.Segments().Count() * 2, contour3.Count());
        }

        [Fact]
        public void PolygonDifferenceWithManyCoincidentEdges()
        {
            // an angle of 47 remains known to fail. This may be a fundamental limitation of clipper w/r/t
            // polygon differences with coincident edges at an angle.
            // var rotations = new[] { 0, 47, 90 };
            var rotations = new[] { 0, 90 };
            var areas = new List<double>();
            foreach (var rotation in rotations)
            {

                var tR = new Transform(Vector3.Origin, rotation);
                var polygon = Polygon.Rectangle(Vector3.Origin, new Vector3(100.0, 50.0)).TransformedPolygon(tR);
                var subtracts = new List<Polygon>();

                var side1 = Polygon.Rectangle(Vector3.Origin, new Vector3(1.0, 20.0));
                var side2 = Polygon.Rectangle(new Vector3(0.0, 30.0), new Vector3(1.0, 50.0));
                for (var i = 1; i < 99; i++)
                {
                    var translate = new Transform(i, 0, 0);
                    subtracts.Add(side1.TransformedPolygon(translate).TransformedPolygon(tR));
                    subtracts.Add(side2.TransformedPolygon(translate).TransformedPolygon(tR));
                }
                var polygons = polygon.Difference(subtracts);

                areas.Add(polygons.First().Area());
            }
            var targetArea = areas[0];
            for (int i = 1; i < areas.Count; i++)
            {
                Assert.Equal(targetArea, areas[i], 4);
            }
        }

        [Fact]
        public void PolygonIsAlmostEqualAfterBoolean()
        {
            var innerPolygon = new Polygon(new[]
            {
                new Vector3(-0.81453490602472578, 0.20473478280229102),
                new Vector3(0.2454762730485458, 0.20473478280229102),
                new Vector3(0.2454762730485458, 5.4378426037008651),
                new Vector3(-0.81453490602472578, 5.4378426037008651)
            });

            var outerPolygon = new Polygon(new[]
            {
                new Vector3(-14.371519985751306, -4.8816304299427005),
                new Vector3(-17.661873645682569, 9.2555712951713573),
                new Vector3(12.965610421927806, 9.2555712951713573),
                new Vector3(12.965610421927806, 3.5538269529982784),
                new Vector3(6.4046991240848143, 3.5538269529982784),
                new Vector3(1.3278034769444158, -4.8816304299427005)
            });

            var intersection = innerPolygon.Intersection(outerPolygon);

            Assert.True(intersection[0].IsAlmostEqualTo(innerPolygon, Vector3.EPSILON));
        }

        [Fact]
        public void PolygonPointsAtToTheEnd()
        {
            this.Name = "PolygonPointsAtToTheEnd";

            var polyCircle = new Circle(Vector3.Origin, 5).ToPolygon(7);
            var polyline = new Polyline(polyCircle.Vertices.Take(polyCircle.Vertices.Count - 1).ToList());

            // Ensure that the PointAt function for u=1.0 is at the
            // end of the polygon AND at the end of the polyline.
            Assert.True(polyCircle.PointAt(1.0).IsAlmostEqualTo(polyCircle.Start));
            Assert.True(polyline.PointAt(1.0).IsAlmostEqualTo(polyline.Vertices[polyline.Vertices.Count - 1]));

            this.Model.AddElement(new ModelCurve(polyCircle));

            var circle = new Circle(Vector3.Origin, 0.1).ToPolygon();
            for (var u = 0.0; u <= 1.0; u += 0.05)
            {
                var pt = polyCircle.PointAt(u);
                this.Model.AddElement(new ModelCurve(circle.Transformed(new Transform(pt)), BuiltInMaterials.XAxis));
            }
        }

        [Fact]
        public void SharedSegments_ConcentricCircles_NoResults()
        {
            var a = new Circle(new Vector3(), 1).ToPolygon();
            var b = new Circle(new Vector3(), 2).ToPolygon();

            var results = Polygon.SharedSegments(a, b, true);

            Assert.Empty(results);
        }

        [Fact]
        public void SharedSegments_IntersectingCircles_NoResults()
        {
            var a = new Circle(new Vector3(1, 0, 0), 2).ToPolygon();
            var b = new Circle(new Vector3(-1, 0, 0), 2).ToPolygon();

            var results = Polygon.SharedSegments(a, b, true);

            Assert.Empty(results);
        }

        [Fact]
        public void SharedSegments_DuplicateCircles_TenResults()
        {
            var a = new Circle(new Vector3(), 1).ToPolygon();
            var b = new Circle(new Vector3(), 1).ToPolygon();

            var results = Polygon.SharedSegments(a, b, true);

            Assert.Equal(10, results.Count);
        }

        [Fact]
        public void SharedSegments_DuplicateCircles_AccurateResults()
        {
            var a = new Circle(new Vector3(), 1).ToPolygon();
            var b = new Circle(new Vector3(), 1).ToPolygon();

            var matches = Polygon.SharedSegments(a, b, true);

            var result = matches.Select(match =>
            {
                var (t, u) = match;

                var segmentA = a.Segments()[t];
                var segmentB = b.Segments()[u];

                // Reverse segment b if necessary
                if (!segmentA.Start.IsAlmostEqualTo(segmentB.Start))
                {
                    segmentB = segmentB.Reversed();
                }

                var sa = segmentA.Start;
                var sb = segmentB.Start;
                var ea = segmentA.End;
                var eb = segmentB.End;

                var startMatches = sa.IsAlmostEqualTo(sb);
                var endMatches = ea.IsAlmostEqualTo(eb);

                return startMatches && endMatches;
            });

            Assert.DoesNotContain(false, result);
        }

        [Fact]
        public void SharedSegments_DuplicateReversedCircles_AccurateResults()
        {
            var a = new Circle(new Vector3(), 1).ToPolygon();
            var b = new Circle(new Vector3(), 1).ToPolygon().Reversed();

            var matches = Polygon.SharedSegments(a, b, true);

            var result = matches.Select(match =>
            {
                var (t, u) = match;

                var segmentA = a.Segments()[t];
                var segmentB = b.Segments()[u];

                // Reverse segment b if necessary
                if (!segmentA.Start.IsAlmostEqualTo(segmentB.Start))
                {
                    segmentB = segmentB.Reversed();
                }

                var sa = segmentA.Start;
                var sb = segmentB.Start;
                var ea = segmentA.End;
                var eb = segmentB.End;

                var startMatches = sa.IsAlmostEqualTo(sb);
                var endMatches = ea.IsAlmostEqualTo(eb);

                return startMatches && endMatches;
            });

            Assert.DoesNotContain(false, result);
        }

        [Fact]
        public void SharedSegments_MirroredSquares_OneResult()
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

            var matches = Polygon.SharedSegments(a, b, true);

            Assert.Single(matches);
        }

        [Fact]
        public void SharedSegments_TouchingShiftedSquares_NoResults()
        {
            var s = 1;

            var a = new Polygon(new List<Vector3>(){
                new Vector3(0, s, 0),
                new Vector3(-s, s, 0),
                new Vector3(-s, 0, 0),
                new Vector3(0, 0, 0),
            });
            var b = new Polygon(new List<Vector3>(){
                new Vector3(0, s + 0.5, 0),
                new Vector3(s, s + 0.5, 0),
                new Vector3(s, 0.5, 0),
                new Vector3(0, 0.5, 0),
            });

            var matches = Polygon.SharedSegments(a, b, true);

            Assert.Empty(matches);
        }

        [Fact]
        public void TransformSegment_UnitSquare_Outwards()
        {
            var s = 0.5;

            var square = new Polygon(new List<Vector3>()
            {
                new Vector3(s, s, 0),
                new Vector3(-s, s, 0),
                new Vector3(-s, -s, 0),
                new Vector3(s, -s, 0)
            });

            var t = new Transform(0, 1, 0);

            square.TransformSegment(t, 0);

            var segment = square.Segments()[0];

            var start = square.Vertices[0];
            var end = square.Vertices[1];

            // Confirm vertices are correctly moved
            Assert.Equal(s, segment.Start.X);
            Assert.Equal(s + 1, segment.Start.Y);
            Assert.Equal(-s, segment.End.X);
            Assert.Equal(s + 1, segment.End.Y);

            // Confirm area has been correctly modified
            Assert.True(square.Area().ApproximatelyEquals(2));
        }

        [Fact]
        public void TransformSegment_UnitSquare_Inwards()
        {
            var s = 0.5;

            var square = new Polygon(new List<Vector3>()
            {
                new Vector3(s, s, 0),
                new Vector3(-s, s, 0),
                new Vector3(-s, -s, 0),
                new Vector3(s, -s, 0)
            });

            var t = new Transform(0, -0.5, 0);

            square.TransformSegment(t, 0);

            var segment = square.Segments()[0];

            // Confirm vertices are correctly moved
            Assert.Equal(s, segment.Start.X);
            Assert.Equal(s - 0.5, segment.Start.Y);
            Assert.Equal(-s, segment.End.X);
            Assert.Equal(s - 0.5, segment.End.Y);

            // Confirm area has been correctly modified
            Assert.True(square.Area().ApproximatelyEquals(0.5));
        }

        [Fact]
        public void TransformSegment_UnitSquare_LastSegment()
        {
            var s = 0.5;

            var square = new Polygon(new List<Vector3>()
            {
                new Vector3(s, s, 0),
                new Vector3(-s, s, 0),
                new Vector3(-s, -s, 0),
                new Vector3(s, -s, 0)
            });

            var t = new Transform(1, 0, 0);

            square.TransformSegment(t, 3);

            var segment = square.Segments()[3];

            // Confirm vertices are correctly moved
            Assert.Equal(s + 1, segment.Start.X);
            Assert.Equal(-s, segment.Start.Y);
            Assert.Equal(s + 1, segment.End.X);
            Assert.Equal(s, segment.End.Y);

            // Confirm area has been correctly modified
            Assert.True(square.Area().ApproximatelyEquals(2));
        }

        [Fact]
        public void TransformSegment_UnitSquare_OutOfRange()
        {
            var s = 0.5;

            var square = new Polygon(new List<Vector3>()
            {
                new Vector3(s, s, 0),
                new Vector3(-s, s, 0),
                new Vector3(-s, -s, 0),
                new Vector3(s, -s, 0)
            });

            var t = new Transform(1, 1, 0);

            square.TransformSegment(t, 100);

            // Confirm area has remained the same
            Assert.True(square.Area().ApproximatelyEquals(1));
        }

        [Fact]
        public void TransformSegment_UnitSquare_AllowsValidNonPlanar()
        {
            var s = 0.5;

            var square = new Polygon(new List<Vector3>()
            {
                new Vector3(s, s, 0),
                new Vector3(-s, s, 0),
                new Vector3(-s, -s, 0),
                new Vector3(s, -s, 0)
            });

            var t = new Transform(1, 1, 1);

            square.TransformSegment(t, 0);
        }

        [Fact]
        public void TransformSegment_RotatedSquare_AllowsPlanarMotion()
        {
            var s = 0.5;

            var square = new Polygon(new List<Vector3>()
            {
                new Vector3(s, s, s),
                new Vector3(-s, s, s),
                new Vector3(-s, -s, -s),
                new Vector3(s, -s, -s)
            });

            var t = new Transform(0, 2, 2);

            square.TransformSegment(t, 0);

            var segment = square.Segments()[0];

            var start = square.Vertices[0];
            var end = square.Vertices[1];

            // Confirm vertices are correctly moved
            Assert.Equal(s + 2, segment.Start.Y);
            Assert.Equal(s + 2, segment.Start.Z);
            Assert.Equal(s + 2, segment.End.Y);
            Assert.Equal(s + 2, segment.End.Z);
        }

        [Fact]
        public void TransformSegment_Circle_ThrowsOnNonPlanar()
        {
            var circle = new Circle(new Vector3(), 1).ToPolygon();

            var t = new Transform(2, 2, 2);

            Assert.Throws<Exception>(() => circle.TransformSegment(t, 0));
        }
    }
}