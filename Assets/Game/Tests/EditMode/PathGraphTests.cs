using System.Collections.Generic;
using Bootleg.Units;
using NUnit.Framework;
using TurnBasedStrategyFramework.Common.Cells;
using TurnBasedStrategyFramework.Common.Pathfinding.Algorithms;

namespace Bootleg.Tests
{
    public class PathGraphTests
    {
        private readonly ICell _start = new TestCell(0, 0);
        private readonly ICell _open = new TestCell(1, 0);
        private readonly ICell _deadEnd = new TestCell(2, 0);

        // start -> open -> deadEnd, and deadEnd has no outgoing edges, so GetGraphEdges leaves it out.
        private Dictionary<ICell, Dictionary<ICell, float>> GraphWithMissingDeadEnd() =>
            new Dictionary<ICell, Dictionary<ICell, float>>
            {
                { _start, new Dictionary<ICell, float> { { _open, 1 } } },
                { _open, new Dictionary<ICell, float> { { _start, 1 }, { _deadEnd, 1 } } },
            };

        [Test]
        public void Dijkstra_ThrowsOnMissingDeadEnd_ReproducesTheCrash()
        {
            Assert.Throws<KeyNotFoundException>(() => new DijkstraPathfinding().FindAllPaths(GraphWithMissingDeadEnd(), _start));
        }

        [Test]
        public void AddDeadEnds_LetsDijkstraReachTheDeadEnd()
        {
            var graph = PathGraph.AddDeadEnds(GraphWithMissingDeadEnd());

            var (cameFrom, costSoFar) = new DijkstraPathfinding().FindAllPaths(graph, _start);

            Assert.AreEqual(2f, costSoFar[_deadEnd]);
            Assert.AreEqual(_open, cameFrom[_deadEnd]);
            Assert.IsEmpty(graph[_deadEnd]);
        }

        [Test]
        public void AddDeadEnds_LeavesExistingEdgesAlone()
        {
            var graph = PathGraph.AddDeadEnds(GraphWithMissingDeadEnd());

            Assert.AreEqual(3, graph.Count);
            CollectionAssert.AreEquivalent(new[] { _start, _deadEnd }, graph[_open].Keys);
        }
    }
}
