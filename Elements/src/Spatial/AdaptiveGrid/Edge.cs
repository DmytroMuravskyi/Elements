﻿using Elements.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elements.Spatial.AdaptiveGrid
{
    /// <summary>
    /// A unique edge in an adaptive grid, connecting two vertices. Doesn't have a particular direction.
    /// This class is forked from CellComplex.Edge.
    /// </summary>
    public class Edge
    {
        /// <summary>
        /// ID of the start Vertex.
        /// </summary>
        public ulong StartId;

        /// <summary>
        /// ID of the end Vertex.
        /// </summary>
        public ulong EndId;

        /// <summary>
        /// The AdaptiveGrid that this Vertex belongs to.
        /// </summary>
        public AdaptiveGrid AdaptiveGrid { get; private set; }

        /// <summary>
        /// ID of this child.
        /// </summary>
        public ulong Id { get; internal set; }

        internal Edge(AdaptiveGrid adaptiveGrid, ulong id, ulong vertexId1, ulong vertexId2)
        {
            AdaptiveGrid = adaptiveGrid;
            Id = id;

            this.SetVerticesFromIds(vertexId1, vertexId2);
        }

        /// <summary>
        /// Used to handle comparisons for when we make HashSets of this type.
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is Edge edge && StartId.Equals(edge.StartId) && EndId.Equals(edge.EndId);
        }

        /// <summary>
        /// Used to return a unique identifier for when we make HashSets of this type.
        /// </summary>
        public override int GetHashCode()
        {
            return GetHash(new List<ulong> { StartId, EndId }).GetHashCode();
        }

        /// <summary>
        /// Get the unique hash for an Edge with list (of length 2) of its unordered vertex IDs.
        /// NOTE: this function is a copy of CellComplex.Edge.GetHash
        /// </summary>
        /// <param name="vertexIds"></param>
        /// <returns></returns>
        internal static string GetHash(List<ulong> vertexIds)
        {
            var sortedIds = vertexIds.ToList();
            sortedIds.Sort();
            var hash = String.Join(",", sortedIds);
            return hash;
        }

        /// <summary>
        /// Sets the StartVertexId and EndVertexId so that start vertex always has a smaller ID than end vertex.
        /// NOTE: this function is a copy of CellComplex.Edge.SetVerticesFromIds
        /// </summary>
        /// <param name="id1">One of the two applicable vertex IDs.</param>
        /// <param name="id2">The other applicable vertex IDs.</param>
        private void SetVerticesFromIds(ulong id1, ulong id2)
        {
            if (id1 < id2)
            {
                this.StartId = id1;
                this.EndId = id2;
            }
            else
            {
                this.EndId = id1;
                this.StartId = id2;
            }
        }

        /// <summary>
        /// Get associated Vertices.
        /// </summary>
        /// <returns></returns>
        public List<Vertex> GetVertices()
        {
            return new List<Vertex>() {
                this.AdaptiveGrid.GetVertex(this.StartId),
                this.AdaptiveGrid.GetVertex(this.EndId)
            };
        }

        /// <summary>
        /// Get the geometry that represents this Edge or DirectedEdge.
        /// </summary>
        /// <returns></returns>
        public Line GetGeometry()
        {
            return new Line(
                this.AdaptiveGrid.GetVertex(this.StartId).Point,
                this.AdaptiveGrid.GetVertex(this.EndId).Point
            );
        }
    }
}
