using System.Collections.Generic;
using System.Linq;
using TurnBasedStrategyFramework.Common.Cells;

namespace Bootleg.Units
{
    public static class PathGraph
    {
        /// <summary>
        /// Adds an empty entry for every cell that can be reached but has no outgoing edges (a dead end, such as a
        /// zone-of-control cell). TBSF's MoveComponent.GetGraphEdges leaves such cells out, and DijkstraPathfinding
        /// reads <c>edges[cell]</c> for every cell it reaches, so a missing dead end throws KeyNotFoundException.
        /// </summary>
        public static Dictionary<ICell, Dictionary<ICell, float>> AddDeadEnds(Dictionary<ICell, Dictionary<ICell, float>> graph)
        {
            var deadEnds = graph.Values
                .SelectMany(edges => edges.Keys)
                .Where(cell => !graph.ContainsKey(cell))
                .Distinct()
                .ToList();
            foreach (var cell in deadEnds)
            {
                graph[cell] = new Dictionary<ICell, float>();
            }
            return graph;
        }
    }
}
