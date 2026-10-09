using System;
using System.Collections.Generic;
using System.Linq;
using Bootleg.Ball;
using NUnit.Framework;
using TurnBasedStrategyFramework.Common.Cells;
using TurnBasedStrategyFramework.Common.Units;
using TurnBasedStrategyFramework.Common.Utilities;

namespace Bootleg.Tests
{
    public class BallPushRulesTests
    {
        private Dictionary<Vector2IntImpl, FakeCell> _grid;

        [SetUp]
        public void SetUp()
        {
            // 6x6 open grid
            _grid = new Dictionary<Vector2IntImpl, FakeCell>();
            for (var x = 0; x < 6; x++)
            {
                for (var y = 0; y < 6; y++)
                {
                    _grid[new Vector2IntImpl(x, y)] = new FakeCell(x, y);
                }
            }
        }

        private ICell Cell(int x, int y) => _grid[new Vector2IntImpl(x, y)];
        private ICell CellAt(Vector2IntImpl coords) => _grid.TryGetValue(coords, out var cell) ? cell : null;
        private List<ICell> Push(ICell pusher, ICell ball, int power) => BallPushRules.GetPushPath(pusher, ball, power, CellAt);

        [Test]
        public void PushesAwayFromPusher_UpToPower()
        {
            var path = Push(Cell(1, 2), Cell(2, 2), 2);

            CollectionAssert.AreEqual(new[] { Cell(3, 2), Cell(4, 2) }, path);
        }

        [TestCase(0, 1, 1, 2)]
        [TestCase(0, -1, 1, 0)]
        [TestCase(1, 0, 2, 1)]
        [TestCase(-1, 0, 0, 1)]
        public void WorksInAllFourDirections(int dx, int dy, int expectedX, int expectedY)
        {
            var ball = Cell(1, 1);
            var pusher = Cell(1 - dx, 1 - dy);

            var path = Push(pusher, ball, 1);

            CollectionAssert.AreEqual(new[] { Cell(expectedX, expectedY) }, path);
        }

        [Test]
        public void StopsBeforeTakenCell()
        {
            Cell(4, 2).IsTaken = true;

            var path = Push(Cell(1, 2), Cell(2, 2), 5);

            CollectionAssert.AreEqual(new[] { Cell(3, 2) }, path);
        }

        [Test]
        public void StopsAtMapEdge()
        {
            var path = Push(Cell(2, 2), Cell(3, 2), 10);

            CollectionAssert.AreEqual(new[] { Cell(4, 2), Cell(5, 2) }, path);
        }

        [Test]
        public void EmptyWhenFirstCellBlocked()
        {
            Cell(3, 2).IsTaken = true;

            Assert.IsEmpty(Push(Cell(1, 2), Cell(2, 2), 3));
        }

        [Test]
        public void EmptyWhenBallAgainstEdge()
        {
            Assert.IsEmpty(Push(Cell(4, 2), Cell(5, 2), 3));
        }

        [Test]
        public void EmptyWhenDiagonal()
        {
            Assert.IsEmpty(Push(Cell(1, 1), Cell(2, 2), 3));
        }

        [Test]
        public void EmptyWhenNotAdjacent()
        {
            Assert.IsEmpty(Push(Cell(0, 2), Cell(2, 2), 3));
        }

        [Test]
        public void EmptyWhenPowerIsZero()
        {
            Assert.IsEmpty(Push(Cell(1, 2), Cell(2, 2), 0));
        }

        private class FakeCell : ICell
        {
            public FakeCell(int x, int y)
            {
                GridCoordinates = new Vector2IntImpl(x, y);
            }

            public event Action<ICell> CellHighlighted { add { } remove { } }
            public event Action<ICell> CellDehighlighted { add { } remove { } }
            public event Action<ICell> CellClicked { add { } remove { } }
            public void InvokeCellHighlighted() { }
            public void InvokeCellDehighlighted() { }
            public void InvokeCellClicked() { }

            public Vector2IntImpl GridCoordinates { get; set; }
            public bool IsTaken { get; set; }
            public IList<IUnit> CurrentUnits { get; } = new List<IUnit>();
            public float MovementCost { get; set; } = 1;
            public Vector3Impl WorldPosition { get; set; }

            public int GetDistance(ICell otherCell) =>
                Math.Abs(GridCoordinates.x - otherCell.GridCoordinates.x) + Math.Abs(GridCoordinates.y - otherCell.GridCoordinates.y);

            public IEnumerable<ICell> GetNeighbours(ICellManager cellManager) => Enumerable.Empty<ICell>();

            public bool Equals(ICell other) => other != null && GridCoordinates.Equals(other.GridCoordinates);

            public override string ToString() => $"({GridCoordinates.x}, {GridCoordinates.y})";
        }
    }
}
