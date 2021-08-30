using System;
using System.Collections.Generic;
using Elements.Geometry;
using Elements.Spatial;

namespace Elements
{
    /// <summary>
    /// Operations on sets of edges.
    /// </summary>
    public static class SetOperations
    {
        /// <summary>
        /// Intersect all segments in each polygon against all segments
        /// in the other polygon, splitting segments, and classify 
        /// all the resulting segments.
        /// </summary>
        public static List<(Vector3 from, Vector3 to, SetClassification classification)> ClassifySegments2d(Polygon a, Polygon b, Func<(Vector3 from, Vector3 to, SetClassification classification), bool> filter = null)
        {
            var ap = a.Plane();
            var bp = b.Plane();
            if (!ap.IsCoplanar(bp))
            {
                throw new System.Exception("Set classification failed. The polygons are not coplanar.");
            }

            var r = new System.Random();
            var classifications = new List<(Vector3 from, Vector3 Topography, SetClassification classification)>();

            ClassifySet(ap, r, a, b, Elements.SetClassification.AInsideB, Elements.SetClassification.AOutsideB, classifications, filter);
            ClassifySet(ap, r, b, a, Elements.SetClassification.BInsideA, Elements.SetClassification.BOutsideA, classifications, filter);

            return classifications;
        }

        private static void ClassifySet(Plane p,
                                        System.Random r,
                                        Polygon a,
                                        Polygon b,
                                        SetClassification insideClassification,
                                        SetClassification outsideClassification,
                                        List<(Vector3 from, Vector3 to, SetClassification classification)> classifications,
                                        Func<(Vector3 from, Vector3 to, SetClassification classification), bool> filter)
        {
            // A tests
            for (var i = 0; i < a.Vertices.Count; i++)
            {
                var intersections = 0;
                var v1 = a.Vertices[i];
                var v2 = i == a.Vertices.Count - 1 ? a.Vertices[0] : a.Vertices[i + 1];
                var ray = GetTestRay(v1.Average(v2), p.Normal);

                // B tests
                for (var j = 0; j < b.Vertices.Count; j++)
                {
                    var v3 = b.Vertices[j];
                    var v4 = j == b.Vertices.Count - 1 ? b.Vertices[0] : b.Vertices[j + 1];
                    if (ray.Intersects(v3, v4, out _))
                    {
                        intersections++;
                    }
                }
                if (intersections % 2 == 0)
                {
                    var edge = (v1, v2, outsideClassification);
                    if (filter != null)
                    {
                        if (filter(edge))
                        {
                            classifications.Add(edge);
                        }
                    }
                    else
                    {
                        classifications.Add(edge);
                    }
                }
                else
                {
                    var edge = (v1, v2, insideClassification);
                    if (filter != null)
                    {
                        if (filter(edge))
                        {
                            classifications.Add(edge);
                        }
                    }
                    else
                    {
                        classifications.Add(edge);
                    }
                }
            }
        }

        private static Ray GetTestRay(Vector3 origin, Vector3 normal)
        {
            var v1 = normal.IsAlmostEqualTo(Vector3.XAxis) ? Vector3.YAxis : Vector3.XAxis;
            var d = v1.Cross(normal);
            return new Ray(origin, d);
        }

        /// <summary>
        /// Build a half edge graph from a collection of segments.
        /// </summary>
        /// <param name="set">A collection of classified segments.</param>
        /// <param name="shared">The classification of shared segments.</param>
        /// <returns></returns>
        public static HalfEdgeGraph2d BuildGraph(List<(Vector3 from, Vector3 to, SetClassification classification)> set,
                                                 SetClassification shared)
        {
            var vertices = new List<Vector3>();
            var edgesPerVertex = new List<List<(int from, int to, int? tag)>>();

            // Fill vertex collection
            foreach (var edge in set)
            {
                if (!vertices.Contains(edge.from))
                {
                    vertices.Add(edge.from);
                    edgesPerVertex.Add(new List<(int from, int to, int? tag)>());
                }

                if (!vertices.Contains(edge.to))
                {
                    vertices.Add(edge.to);
                    edgesPerVertex.Add(new List<(int from, int to, int? tag)>());
                }
            }

            // Create edges
            foreach (var edge in set)
            {
                var l = edgesPerVertex[vertices.IndexOf(edge.from)];
                l.Add((vertices.IndexOf(edge.from), vertices.IndexOf(edge.to), null));

                if (edge.classification == shared)
                {
                    var l1 = edgesPerVertex[vertices.IndexOf(edge.to)];
                    l1.Add((vertices.IndexOf(edge.to), vertices.IndexOf(edge.from), null));
                }
            }

            var heg = new HalfEdgeGraph2d()
            {
                Vertices = vertices,
                EdgesPerVertex = edgesPerVertex
            };

            return heg;
        }
    }
}