using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;

namespace Elements.Spatial
{
    /// <summary>
    /// Represents a 2D Graph with Half-edge connectivity, useful for finding polygons
    /// bounded by intersecting or overlapping edges.
    /// </summary>
    public class HalfEdgeGraph2d
    {
        private HalfEdgeGraph2d()
        {
            Vertices = new List<Vector3>();
            EdgesPerVertex = new List<List<(int from, int to)>>();
        }

        /// <summary>
        /// The unique vertices of this graph
        /// </summary>
        /// <value></value>
        public List<Vector3> Vertices { get; set; }

        /// <summary>
        /// The index pairs, grouped by starting vertex, representing unique half edges.
        /// </summary>
        public List<List<(int from, int to)>> EdgesPerVertex { get; set; }

        /// <summary>
        /// Construct a 2D Half Edge Graph from a polygon and an intersecting polyline.
        /// </summary>
        /// <param name="pg">The polygon.</param>
        /// <param name="pl">The polyline.</param>
        public static HalfEdgeGraph2d Construct(Polygon pg, Polyline pl)
        {
            return Construct(new[] { pg }, new[] { pl });
        }

        /// <summary>
        /// Construct a 2D Half Edge Graph from a collection of polygons and a collection of intersecting polylines.
        /// </summary>
        /// <param name="pg">The polygons.</param>
        /// <param name="pl">The polylines.</param>
        public static HalfEdgeGraph2d Construct(IEnumerable<Polygon> pg, IEnumerable<Polyline> pl)
        {
            var plArray = pl.ToArray();
            var plSegments = pl.SelectMany(p => p.Segments()).ToArray();
            var graph = new HalfEdgeGraph2d();
            var vertices = graph.Vertices;
            var edgesPerVertex = graph.EdgesPerVertex;

            // Check each polygon segment against each polyline segment for intersections. 
            // Build up a half-edge structure.

            // for each segment, store a list of vertices. If an intersection is found, additional vertices will be added to the list for that segment.

            var polylineSplitPoints = plSegments.Select(p => new List<Vector3> { p.Start, p.End }).ToArray();
            // first we check polyline-polyline intersections, and add those to split points
            var plCount = plArray.Length;
            if (plCount > 1)
            {
                var flatListPosition = 0;
                for (int i = 0; i < plCount - 1; i++)
                {
                    // check each segment in this polyline with all segments starting after this polyline
                    // to avoid checking for intersections between a polyline and its own segments.
                    // flatListPosition keeps track of how far along the flat list of segments we should start.
                    var segmentsA = plArray[i].Segments();
                    for (int j = flatListPosition; j < plSegments.Count(); j++)
                    {
                        var otherSegment = plSegments[j];
                        for (int segAIndex = 0; segAIndex < segmentsA.Length; segAIndex++)
                        {
                            Line segA = (Line)segmentsA[segAIndex];
                            if (segA.Intersects(otherSegment, out var intersectionPt, false, true))
                            {
                                polylineSplitPoints[flatListPosition + segAIndex].Add(intersectionPt);
                                polylineSplitPoints[j].Add(intersectionPt);
                            }
                        }
                    }
                    flatListPosition += segmentsA.Count();
                }
            }
            // next we check each polygon against all polyline segments
            foreach (var polygon in pg)
            {
                var pgSegments = polygon.Segments();
                for (int i = 0; i < pgSegments.Length; i++)
                {
                    var polygonSegment = pgSegments[i];
                    // collect the vertices of each segment — if an intersection is found, additional vertices will be added to this list.
                    var polygonSegmentSplitPoints = new List<Vector3> { polygonSegment.Start, polygonSegment.End };
                    for (int j = 0; j < plSegments.Length; j++)
                    {
                        var polylineSegment = plSegments[j];
                        if (polygonSegment.Intersects(polylineSegment, out var intersectionPt, false, true))
                        {
                            polylineSplitPoints[j].Add(intersectionPt);
                            polygonSegmentSplitPoints.Add(intersectionPt);
                        }
                    }
                    // sort the unique polygon edge vertices along the segment's length, and start the halfEdge graph.
                    var pgIntersectionsOrdered = polygonSegmentSplitPoints.Distinct().OrderBy(sp => sp.DistanceTo(polygonSegment.Start)).ToArray();
                    for (int k = 0; k < pgIntersectionsOrdered.Length - 1; k++)
                    {
                        var from = pgIntersectionsOrdered[k];
                        var to = pgIntersectionsOrdered[k + 1];
                        var fromIndex = vertices.FindIndex(v => v.IsAlmostEqualTo(from));
                        if (fromIndex == -1)
                        {
                            fromIndex = vertices.Count;
                            vertices.Add(from);
                            edgesPerVertex.Add(new List<(int from, int to)>());
                        }
                        var toIndex = vertices.FindIndex(v => v.IsAlmostEqualTo(to));
                        if (toIndex == -1)
                        {
                            toIndex = vertices.Count;
                            vertices.Add(to);
                            edgesPerVertex.Add(new List<(int from, int to)>());
                        }
                        // only add one set of polygon halfEdges, so we don't wind up with an outer loop.
                        if (fromIndex != toIndex && !edgesPerVertex[fromIndex].Contains((fromIndex, toIndex)))
                        {
                            edgesPerVertex[fromIndex].Add((fromIndex, toIndex));
                        }
                    }
                }
            }
            // do the same with the polyline's vertices — sort and add to the halfEdge graph.
            foreach (var splitSet in polylineSplitPoints)
            {
                var splitSetOrdered = splitSet.Distinct().OrderBy(v => v.DistanceTo(splitSet[0])).ToArray();
                for (int i = 0; i < splitSetOrdered.Length - 1; i++)
                {
                    var from = splitSetOrdered[i];
                    var to = splitSetOrdered[i + 1];

                    var fromIndex = vertices.FindIndex(v => v.IsAlmostEqualTo(from));
                    if (fromIndex == -1)
                    {
                        fromIndex = vertices.Count;
                        vertices.Add(from);
                        edgesPerVertex.Add(new List<(int from, int to)>());
                    }
                    var toIndex = vertices.FindIndex(v => v.IsAlmostEqualTo(to));
                    if (toIndex == -1)
                    {
                        toIndex = vertices.Count;
                        vertices.Add(to);
                        edgesPerVertex.Add(new List<(int from, int to)>());
                    }
                    // add both half edges for polyline segments. If we have a splitter 
                    // lying exactly on a polygon edge, don't add it. 
                    if (fromIndex != toIndex && !edgesPerVertex[fromIndex].Contains((fromIndex, toIndex)) && !edgesPerVertex[toIndex].Contains((toIndex, fromIndex)))
                    {
                        edgesPerVertex[fromIndex].Add((fromIndex, toIndex));
                        edgesPerVertex[toIndex].Add((toIndex, fromIndex));
                    }
                }
            }
            return graph;
        }


        /// <summary>
        /// Calculate the closed polygons in this graph.
        /// </summary>
        public List<Polygon> Polygonize()
        {
            var edgesPerVertex = new List<List<(int from, int to)>>(this.EdgesPerVertex);
            var vertices = this.Vertices;
            var newPolygons = new List<Polygon>();
            // construct polygons from half edge graph.
            // remove edges from edgesPerVertex as they get "consumed" by a polygon,
            // and stop when you run out of edges. 
            // Guranteed to terminate because every loop step removes at least 1 edge, and
            // edges are never added.
            while (edgesPerVertex.Any(l => l.Count > 0))
            {
                var currentEdgeList = new List<(int from, int to)>();
                // pick a starting point
                var startingSet = edgesPerVertex.First(l => l.Count > 0);
                var currentSegment = startingSet[0];
                startingSet.RemoveAt(0);
                var initialFrom = currentSegment.from;

                // loop until we reach the point at which we started for this polygon loop.
                // Since we have a finite set of edges, and we consume / remove every edge we traverse,
                // we must eventually either find an edge that points back to our start, or hit
                // a dead end where no more edges are available (in which case we throw an exception) 
                while (currentSegment.to != initialFrom)
                {
                    currentEdgeList.Add(currentSegment);
                    var toVertex = vertices[currentSegment.to];
                    var fromVertex = vertices[currentSegment.from];

                    var vectorToTest = fromVertex - toVertex;
                    // get all segments pointing outwards from our "to" vertex
                    var possibleNextSegments = edgesPerVertex[currentSegment.to];
                    if (possibleNextSegments.Count == 0)
                    {
                        // this should never happen.
                        throw new Exception("Something went wrong building polygons from split results. Unable to proceed.");
                    }
                    // at every node, we pick the next segment forming the largest counter-clockwise angle with our opposite.
                    var nextSegment = possibleNextSegments.OrderBy(cand => vectorToTest.PlaneAngleTo(vertices[cand.to] - vertices[cand.from])).Last();
                    possibleNextSegments.Remove(nextSegment);
                    currentSegment = nextSegment;
                }
                currentEdgeList.Add(currentSegment);
                var currentVertexList = new List<Vector3>();

                // remove duplicate edges in the same new polygon, 
                // which will occur if we have a polyline that doesn't cross all the way through.
                var validEdges = new List<(int from, int to)>(currentEdgeList);
                int i = 0;
                // guaranteed to terminate, since at every step we either increment i by one, or make validEdges.Count smaller by 2 (and decrement i by 1).
                // validEdges.Count-i always gets smaller, every step, until 0. 
                while (validEdges.Count > 0 && i < validEdges.Count)
                {
                    var index = (i + validEdges.Count) % validEdges.Count;
                    var nextIndex = (i + 1 + validEdges.Count) % validEdges.Count;
                    var thisEdge = validEdges[index];
                    var nextEdge = validEdges[nextIndex];
                    if (thisEdge.from == nextEdge.to)
                    {
                        // we found a degenerate section — two duplicate edges, joined at a vertex. 
                        // we remove the two duplicate edges. we have to do this in a descending sorted order 
                        // so the removal of the first one doesn't shift the position of the second one,
                        // and if we're straddling the end of the list eg (5, 0), "nextIndex" is before "index". 
                        foreach (var indexToRemove in new[] { index, nextIndex }.OrderByDescending(v => v))
                        {
                            validEdges.RemoveAt(indexToRemove);
                        }
                        // it's conceivable that the two other edges on either side of these removed edges are ALSO identical.
                        // in this case, we actually step backwards — to compare "the one before the first one we just removed" and
                        // "the one after the second one we just removed", which will now be adjacent in the list. 
                        i--;
                    }
                    else
                    {
                        i++;
                    }

                }

                foreach (var edge in validEdges)
                {
                    currentVertexList.Add(vertices[edge.to]);
                }
                // if we have a wholly-contained polyline, this cleanup can result in a totally empty list,
                // so we check before trying to construct a polygon.
                if (currentVertexList.Count > 0)
                {
                    newPolygons.Add(new Polygon(currentVertexList));
                }
            }

            return newPolygons;
        }
    }
}