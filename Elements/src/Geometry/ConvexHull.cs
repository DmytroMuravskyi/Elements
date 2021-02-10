using System;
using System.Collections.Generic;
using System.Linq;

namespace Elements.Geometry
{
    /// <summary>
    /// A utility class for calculating Convex Hulls from inputs
    /// </summary>
    public static class ConvexHull
    {
        /// <summary>
        /// Calculate a polygon from the 2d convex hull of a collection of points. 
        /// Adapted from https://rosettacode.org/wiki/Convex_hull#C.23
        /// </summary>
        /// <param name="points">A collection of points</param>
        /// <returns>A polygon representing the convex hull of the provided points.</returns>
        public static Polygon FromPoints(IEnumerable<Vector3> points)
        {
            if (points.Count() == 0)
            {
                return null;
            }
            var pointsSorted = points.OrderBy(p => p.X).ToArray();
            List<Vector3> hullPoints = new List<Vector3>();

            Func<Vector3, Vector3, Vector3, bool> Ccw = (Vector3 a, Vector3 b, Vector3 c) => ((b.X - a.X) * (c.Y - a.Y)) > ((b.Y - a.Y) * (c.X - a.X));

            // lower hull
            foreach (var pt in pointsSorted)
            {
                while (hullPoints.Count >= 2 && !Ccw(hullPoints[hullPoints.Count - 2], hullPoints[hullPoints.Count - 1], pt))
                {
                    hullPoints.RemoveAt(hullPoints.Count - 1);
                }
                hullPoints.Add(pt);
            }

            // upper hull
            int t = hullPoints.Count + 1;
            for (int i = pointsSorted.Length - 1; i >= 0; i--)
            {
                Vector3 pt = pointsSorted[i];
                while (hullPoints.Count >= t && !Ccw(hullPoints[hullPoints.Count - 2], hullPoints[hullPoints.Count - 1], pt))
                {
                    hullPoints.RemoveAt(hullPoints.Count - 1);
                }
                hullPoints.Add(pt);
            }

            hullPoints.RemoveAt(hullPoints.Count - 1);
            return new Polygon(hullPoints);
        }

        /// <summary>
        /// Calculate a polygon from the 2d convex hull of a polyline or polygon's vertices. 
        /// </summary>
        /// <param name="p">A polygon</param>
        /// <returns>A polygon representing the convex hull of the provided shape.</returns>
        public static Polygon FromPolyline(Polyline p)
        {
            return FromPoints(p.Vertices);
        }

        /// <summary>
        /// Calculate a polygon from the 2d convex hull of the vertices of a collection of polylines or polygons. 
        /// </summary>
        /// <param name="polylines">A collection of polygons</param>
        /// <returns>A polygon representing the convex hull of the provided shapes.</returns>
        public static Polygon FromPolylines(IEnumerable<Polyline> polylines)
        {
            return FromPoints(polylines.SelectMany(p => p.Vertices));
        }

        /// <summary>
        /// Calculate a polygon from the 2d convex hull of a profile. 
        /// </summary>
        /// <param name="p">A profile</param>
        /// <returns>A polygon representing the convex hull of the provided shape.</returns>
        public static Polygon FromProfile(Profile p)
        {
            // it's safe to consider only the perimeter because the voids must be within it
            return FromPolyline(p.Perimeter);
        }

    }
}