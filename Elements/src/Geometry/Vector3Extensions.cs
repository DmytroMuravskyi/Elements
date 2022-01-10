using System;
using System.Collections.Generic;
using System.Linq;

namespace Elements.Geometry
{
    /// <summary>
    /// Extension methods for Vector3.
    /// </summary>
    public static class Vector3Extensions
    {
        /// <summary>
        /// Are the provided points on the same plane?
        /// </summary>
        /// <param name="points"></param>
        public static bool AreCoplanar(this IList<Vector3> points)
        {
            if (points.Count < 3) return true;

            //TODO: https://github.com/hypar-io/sdk/issues/54
            // Ensure that all triple products are equal to 0.
            // a.Dot(b.Cross(c));
            var a = points[0];
            var b = points[1];
            var c = points[2];
            var ab = b - a;
            var ac = c - a;
            for (var i = 3; i < points.Count; i++)
            {
                var d = points[i];
                var cd = d - a;
                var tp = ab.Dot(ac.Cross(cd));
                if (Math.Abs(tp) > Vector3.EPSILON)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Are the provided points along the same line?
        /// </summary>
        /// <param name="points"></param>
        public static bool AreCollinear(this IList<Vector3> points)
        {
            if (points == null || points.Count == 0)
            {
                throw new ArgumentException("Cannot test collinearity of an empty list");
            }
            if (points.Distinct(new Vector3Comparer()).Count() < 3)
            {
                return true;
            }

            var fitLine = points.FitLine(out var directions);
            var fitDir = fitLine.Direction();
            var epsilonSquared = Vector3.EPSILON * Vector3.EPSILON;
            return directions.All(d =>
            {
                var dot = d.Dot(fitDir);
                var lengthSquared = d.LengthSquared();
                return lengthSquared - (dot * dot) < epsilonSquared;
            });
        }

        /// <summary>
        /// Return an approximate fit line through a set of points. 
        /// Not intended for statistical regression purposes. 
        /// Note that the line is unit length: it shouldn't be expected
        /// to span the length of the points.
        /// </summary>
        /// <param name="points">The points to fit.</param>
        /// <returns>A line roughly running through the set of points.</returns>
        public static Line FitLine(this IList<Vector3> points)
        {
            return FitLine(points, out _);
        }
        private static Line FitLine(this IList<Vector3> points, out IEnumerable<Vector3> directionsFromMean)
        {
            // get the mean point, presumably near the center of the pts
            var meanPt = points.Average();
            // get the points minus their mean (direction from the mean to the other points)
            var ptsMinusMean = points.Select(pt => pt - meanPt);
            // pick any non-zero vector as an alignment guide, so that a set of directions
            // that's perfectly symmetrical about the mean doesn't average out to zero
            var alignmentVector = ptsMinusMean.First(p => !p.IsZero());
            // flip the directions so they're all pointing in the same direction as the alignment vector
            var ptsMinusMeanAligned = ptsMinusMean.Select(p => p.Dot(alignmentVector) < 0 ? p * -1 : p);
            // get average direction
            var averageDirFromMean = ptsMinusMeanAligned.Average();

            directionsFromMean = ptsMinusMean;
            return new Line(meanPt, meanPt + averageDirFromMean.Unitized());
        }

        /// <summary>
        /// Compute a transform with the origin at points[0], with
        /// an X axis along points[1]->points[0], and a normal
        /// computed using the vectors points[2]->points[1] and
        /// points[1]->points[0].
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static Transform ToTransform(this IList<Vector3> points)
        {
            var a = (points[1] - points[0]).Unitized();
            // We need to search for a second vector that is not colinear
            // with the first. If all the vectors are tried, and one isn't
            // found that's not parallel to the first, you'll
            // get a zero-length normal.
            Vector3 b = new Vector3();
            for (var i = 2; i < points.Count; i++)
            {
                b = (points[i] - points[1]).Unitized();
                var dot = b.Dot(a);
                if (dot > -1 + Vector3.EPSILON && dot < 1 - Vector3.EPSILON)
                {
                    // Console.WriteLine("Found valid second vector.");
                    break;
                }
            }

            var n = b.Cross(a);
            var t = new Transform(points[0], a, n);
            return t;
        }

        /// <summary>
        /// Find the average of a collection of Vector3.
        /// </summary>
        /// <param name="points">The Vector3 collection to average.</param>
        /// <returns>A Vector3 representing the average.</returns>
        public static Vector3 Average(this IEnumerable<Vector3> points)
        {
            double x = 0.0, y = 0.0, z = 0.0;
            foreach (var p in points)
            {
                x += p.X;
                y += p.Y;
                z += p.Z;
            }
            var count = points.Count();
            return new Vector3(x / count, y / count, z / count);
        }

        /// <summary>
        /// Shrink a collection of Vector3 towards their average.
        /// </summary>
        /// <param name="points">The collection of Vector3 to shrink.</param>
        /// <param name="distance">The distance to shrink along the vector to average.</param>
        /// <returns></returns>
        public static Vector3[] Shrink(this Vector3[] points, double distance)
        {
            var avg = points.Average();
            var shrink = new Vector3[points.Length];
            for (var i = 0; i < shrink.Length; i++)
            {
                var p = points[i];
                shrink[i] = p + (avg - p).Unitized() * distance;
            }
            return shrink;
        }

        /// <summary>
        /// Convert a collection of Vector3 to a flat array of double.
        /// </summary>
        /// <param name="points">The collection of Vector3 to convert.</param>
        /// <returns>An array containing x,y,z,x1,y1,z1,x2,y2,z2,...</returns>
        public static double[] ToArray(this IList<Vector3> points)
        {
            var arr = new double[points.Count * 3];
            var c = 0;
            for (var i = 0; i < points.Count; i++)
            {
                var v = points[i];
                arr[c] = v.X;
                arr[c + 1] = v.Y;
                arr[c + 2] = v.Z;
                c += 3;
            }
            return arr;
        }

        internal static GraphicsBuffers ToGraphicsBuffers(this IList<Vector3> vertices, bool lineLoop)
        {
            var gb = new GraphicsBuffers();

            for (var i = 0; i < vertices.Count; i++)
            {
                var v = vertices[i];
                gb.AddVertex(v, default(Vector3), default(UV), null);

                var write = lineLoop ? (i < vertices.Count - 1) : (i % 2 == 0 && i < vertices.Count - 1);
                if (write)
                {
                    gb.AddIndex((ushort)i);
                    gb.AddIndex((ushort)(i + 1));
                }
            }
            return gb;
        }

        /// <summary>
        /// Calculate the normal of the plane containing a set of points.
        /// </summary>
        /// <param name="points">The points in the plane.</param>
        /// <returns>The normal of the plane containing the points.</returns>
        internal static Vector3 NormalFromPlanarWoundPoints(this IList<Vector3> points)
        {
            var normal = new Vector3();
            for (int i = 0; i < points.Count; i++)
            {
                var p0 = points[i];
                var p1 = points[(i + 1) % points.Count];
                normal.X += (p0.Y - p1.Y) * (p0.Z + p1.Z);
                normal.Y += (p0.Z - p1.Z) * (p0.X + p1.X);
                normal.Z += (p0.X - p1.X) * (p0.Y + p1.Y);
            }
            return normal.Unitized();
        }
    }

    internal class Vector3Comparer : EqualityComparer<Vector3>
    {
        public override bool Equals(Vector3 x, Vector3 y)
        {
            return x.IsAlmostEqualTo(y);
        }

        public override int GetHashCode(Vector3 obj)
        {
            return obj.GetHashCode();
        }
    }
}