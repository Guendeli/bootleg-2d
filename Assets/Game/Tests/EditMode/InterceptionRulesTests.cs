using System.Collections.Generic;
using System.Linq;
using Bootleg.Ball;
using NUnit.Framework;
using TurnBasedStrategyFramework.Common.Cells;
using TurnBasedStrategyFramework.Common.Utilities;

namespace Bootleg.Tests
{
    public class InterceptionRulesTests
    {
        private Dictionary<Vector2IntImpl, TestCell> _grid;
        private HashSet<ICell> _enemies;

        [SetUp]
        public void SetUp()
        {
            _grid = new Dictionary<Vector2IntImpl, TestCell>();
            for (var x = 0; x < 6; x++)
            {
                for (var y = 0; y < 6; y++)
                {
                    _grid[new Vector2IntImpl(x, y)] = new TestCell(x, y);
                }
            }
            _enemies = new HashSet<ICell>();
        }

        private ICell Cell(int x, int y) => _grid[new Vector2IntImpl(x, y)];
        private ICell CellAt(Vector2IntImpl coords) => _grid.TryGetValue(coords, out var cell) ? cell : null;
        private bool HasEnemy(ICell cell) => _enemies.Contains(cell);

        private void Enemies(params ICell[] cells)
        {
            foreach (var cell in cells)
            {
                _enemies.Add(cell);
                cell.IsTaken = true;
            }
        }

        // Path along y = 2 from x = 1 to x = 4.
        private List<ICell> Row2Path() => new List<ICell> { Cell(1, 2), Cell(2, 2), Cell(3, 2), Cell(4, 2) };

        [Test]
        public void AlongBallPath_NoEnemies_NoCandidates()
        {
            Assert.IsEmpty(InterceptionRules.AlongBallPath(Row2Path(), CellAt, HasEnemy));
        }

        [Test]
        public void AlongBallPath_EnemyOnPath_ChallengesAsTheBallApproaches_AndCatchesItOnTheirCell()
        {
            Enemies(Cell(3, 2));

            var candidate = InterceptionRules.AlongBallPath(Row2Path(), CellAt, HasEnemy).Single();

            // (3,2) is on path index 2 but already next to index 1, so it challenges one cell earlier.
            Assert.AreEqual(1, candidate.PathIndex);
            Assert.AreEqual(Cell(3, 2), candidate.InterceptorCell);
            CollectionAssert.AreEqual(new[] { Cell(1, 2), Cell(2, 2), Cell(3, 2) }, InterceptionRules.InterceptedPath(Row2Path(), candidate));
        }

        [Test]
        public void AlongBallPath_EnemyOnFirstPathCell_ChallengesAtIndexZero()
        {
            Enemies(Cell(1, 2));

            var candidate = InterceptionRules.AlongBallPath(Row2Path(), CellAt, HasEnemy).Single();

            Assert.AreEqual(0, candidate.PathIndex);
            CollectionAssert.AreEqual(new[] { Cell(1, 2) }, InterceptionRules.InterceptedPath(Row2Path(), candidate));
        }

        [Test]
        public void AlongBallPath_EnemyNextToPath_ChallengesAtNearestPathCell()
        {
            Enemies(Cell(2, 3));

            var candidate = InterceptionRules.AlongBallPath(Row2Path(), CellAt, HasEnemy).Single();

            Assert.AreEqual(1, candidate.PathIndex);
            Assert.AreEqual(Cell(2, 3), candidate.InterceptorCell);
        }

        [Test]
        public void AlongBallPath_OrderedByWhenTheBallReachesThem_EachEnemyOnce()
        {
            // (4,1) is next to path index 3; (1,3) is next to index 0; (2,2) is on index 1 but already next to index 0,
            // so it challenges there first (same resulting ball path either way).
            Enemies(Cell(4, 1), Cell(1, 3), Cell(2, 2));

            var candidates = InterceptionRules.AlongBallPath(Row2Path(), CellAt, HasEnemy);

            CollectionAssert.AreEqual(new[] { Cell(2, 2), Cell(1, 3), Cell(4, 1) }, candidates.Select(c => c.InterceptorCell));
            CollectionAssert.AreEqual(new[] { 0, 0, 3 }, candidates.Select(c => c.PathIndex));
        }

        [Test]
        public void AlongBallPath_EnemyDiagonalToPath_Ignored()
        {
            Enemies(Cell(5, 3));

            Assert.IsEmpty(InterceptionRules.AlongBallPath(Row2Path(), CellAt, HasEnemy));
        }

        [Test]
        public void InterceptedPath_EnemyOnPath_EndsOnTheirCell()
        {
            var path = InterceptionRules.InterceptedPath(Row2Path(), new InterceptionCandidate(2, Cell(3, 2)));

            CollectionAssert.AreEqual(new[] { Cell(1, 2), Cell(2, 2), Cell(3, 2) }, path);
        }

        [Test]
        public void InterceptedPath_EnemyNextToPath_StepsOntoTheirCell()
        {
            var path = InterceptionRules.InterceptedPath(Row2Path(), new InterceptionCandidate(1, Cell(2, 3)));

            CollectionAssert.AreEqual(new[] { Cell(1, 2), Cell(2, 2), Cell(2, 3) }, path);
        }

        [Test]
        public void EnemiesAround_ReturnsOrthogonalEnemiesOnly()
        {
            Enemies(Cell(3, 2), Cell(2, 1), Cell(3, 3));

            CollectionAssert.AreEquivalent(new[] { Cell(3, 2), Cell(2, 1) }, InterceptionRules.EnemiesAround(Cell(2, 2), CellAt, HasEnemy));
        }

        [Test]
        public void FirstSuccess_ReturnsFirstRollUnderItsChance()
        {
            var rolls = new Queue<float>(new[] { 0.9f, 0.1f, 0.0f });

            Assert.AreEqual(1, InterceptionRules.FirstSuccess(new[] { 0.5f, 0.5f, 0.5f }, () => rolls.Dequeue()));
        }

        [Test]
        public void FirstSuccess_AllFail_ReturnsMinusOne()
        {
            Assert.AreEqual(-1, InterceptionRules.FirstSuccess(new[] { 0.2f, 0.2f }, () => 0.5f));
        }

        [Test]
        public void CombinedChance_IsChanceThatAtLeastOneSucceeds()
        {
            // 1 - 0.75^3
            Assert.AreEqual(0.578125f, InterceptionRules.CombinedChance(new[] { 0.25f, 0.25f, 0.25f }), 1e-6f);
        }

        [Test]
        public void CombinedChance_NoDefenders_IsZero()
        {
            Assert.AreEqual(0f, InterceptionRules.CombinedChance(new float[0]));
        }

        [Test]
        public void CombinedChance_ClampsOutOfRangeChances()
        {
            Assert.AreEqual(1f, InterceptionRules.CombinedChance(new[] { 0.3f, 1.5f }));
            Assert.AreEqual(0.3f, InterceptionRules.CombinedChance(new[] { 0.3f, -2f }), 1e-6f);
        }

        [Test]
        public void Succeeds_ExactAtTheEnds()
        {
            Assert.IsTrue(InterceptionRules.Succeeds(1f, () => 1f), "100% must succeed even if the roll is exactly 1");
            Assert.IsFalse(InterceptionRules.Succeeds(0f, () => 0f), "0% must fail even if the roll is exactly 0");
        }
    }
}
