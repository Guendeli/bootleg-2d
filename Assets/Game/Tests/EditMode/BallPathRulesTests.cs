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
    public class BallPathRulesTests
    {
        private Dictionary<Vector2IntImpl, TestCell> _grid;

        [SetUp]
        public void SetUp()
        {
            // 6x6 open grid
            _grid = new Dictionary<Vector2IntImpl, TestCell>();
            for (var x = 0; x < 6; x++)
            {
                for (var y = 0; y < 6; y++)
                {
                    _grid[new Vector2IntImpl(x, y)] = new TestCell(x, y);
                }
            }
        }

        private ICell Cell(int x, int y) => _grid[new Vector2IntImpl(x, y)];
        private ICell CellAt(Vector2IntImpl coords) => _grid.TryGetValue(coords, out var cell) ? cell : null;
        private List<ICell> Push(ICell pusher, ICell ball, int power) => BallPathRules.GetPushPath(pusher, ball, power, CellAt);

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
        public void Kick_TargetsEveryFreeCellInFourLines()
        {
            var targets = BallPathRules.GetKickTargets(Cell(2, 2), 2, CellAt);

            CollectionAssert.AreEquivalent(
                new[] { Cell(3, 2), Cell(4, 2), Cell(1, 2), Cell(0, 2), Cell(2, 3), Cell(2, 4), Cell(2, 1), Cell(2, 0) },
                targets.Keys);
        }

        [Test]
        public void Kick_PathToTargetIncludesCellsBefore()
        {
            var targets = BallPathRules.GetKickTargets(Cell(2, 2), 3, CellAt);

            CollectionAssert.AreEqual(new[] { Cell(2, 3), Cell(2, 4), Cell(2, 5) }, targets[Cell(2, 5)]);
            CollectionAssert.AreEqual(new[] { Cell(2, 3) }, targets[Cell(2, 3)]);
        }

        [Test]
        public void Kick_LineStopsBeforeTakenCell()
        {
            Cell(4, 2).IsTaken = true;

            var targets = BallPathRules.GetKickTargets(Cell(2, 2), 5, CellAt);

            Assert.IsTrue(targets.ContainsKey(Cell(3, 2)));
            Assert.IsFalse(targets.ContainsKey(Cell(4, 2)));
            Assert.IsFalse(targets.ContainsKey(Cell(5, 2)));
        }

        [Test]
        public void Kick_NoTargetsWhenSurrounded()
        {
            Cell(1, 2).IsTaken = Cell(3, 2).IsTaken = Cell(2, 1).IsTaken = Cell(2, 3).IsTaken = true;

            Assert.IsEmpty(BallPathRules.GetKickTargets(Cell(2, 2), 3, CellAt));
        }

        private Dictionary<ICell, List<ICell>> Pass(ICell carrier, int power, params ICell[] receivers)
        {
            foreach (var receiver in receivers)
            {
                receiver.IsTaken = true;
            }
            return BallPathRules.GetPassTargets(carrier, power, CellAt, c => receivers.Contains(c));
        }

        [Test]
        public void Pass_ReachesTeammateWithinPower_PathEndsOnReceiver()
        {
            var targets = Pass(Cell(1, 2), 3, Cell(4, 2));

            CollectionAssert.AreEqual(new[] { Cell(2, 2), Cell(3, 2), Cell(4, 2) }, targets[Cell(4, 2)]);
        }

        [Test]
        public void Pass_TeammateBeyondPower_NotATarget()
        {
            Assert.IsEmpty(Pass(Cell(1, 2), 2, Cell(4, 2)));
        }

        [Test]
        public void Pass_BlockedByNonReceiverInBetween()
        {
            Cell(2, 2).IsTaken = true;

            Assert.IsEmpty(Pass(Cell(1, 2), 5, Cell(4, 2)));
        }

        [Test]
        public void Pass_OnlyNearestUnitInEachLineCounts()
        {
            var targets = Pass(Cell(0, 2), 5, Cell(2, 2), Cell(4, 2));

            CollectionAssert.AreEquivalent(new[] { Cell(2, 2) }, targets.Keys);
        }

        [Test]
        public void Pass_FindsReceiversInSeveralDirections()
        {
            var targets = Pass(Cell(2, 2), 3, Cell(2, 4), Cell(0, 2));

            CollectionAssert.AreEquivalent(new[] { Cell(2, 4), Cell(0, 2) }, targets.Keys);
        }

        [Test]
        public void Pass_DiagonalTeammate_NotATarget()
        {
            Assert.IsEmpty(Pass(Cell(2, 2), 3, Cell(3, 3)));
        }

        [Test]
        public void Kick_FliesOverEnemy_ButCannotLandOnIt()
        {
            var enemy = Cell(3, 2);
            enemy.IsTaken = true;

            var targets = BallPathRules.GetKickTargets(Cell(2, 2), 3, CellAt, c => c == enemy);

            Assert.IsFalse(targets.ContainsKey(enemy));
            CollectionAssert.AreEqual(new[] { Cell(3, 2), Cell(4, 2) }, targets[Cell(4, 2)]);
        }

        [Test]
        public void Pass_FliesOverEnemyToTeammateBehind()
        {
            var enemy = Cell(2, 2);
            enemy.IsTaken = true;
            var receiver = Cell(4, 2);
            receiver.IsTaken = true;

            var targets = BallPathRules.GetPassTargets(Cell(1, 2), 5, CellAt, c => c == receiver, c => c == enemy);

            CollectionAssert.AreEqual(new[] { Cell(2, 2), Cell(3, 2), Cell(4, 2) }, targets[receiver]);
        }

        [Test]
        public void EmptyWhenPowerIsZero()
        {
            Assert.IsEmpty(Push(Cell(1, 2), Cell(2, 2), 0));
        }
    }
}
