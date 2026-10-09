using System.Collections.Generic;
using System.Linq;
using Bootleg.Ball;
using NUnit.Framework;
using TurnBasedStrategyFramework.Common.Cells;

namespace Bootleg.Tests
{
    public class BallMoveRulesTests
    {
        private static List<ICell> Path(int length) =>
            Enumerable.Range(0, length).Select(i => (ICell)new TestCell(i, 0)).ToList();

        [Test]
        public void NoBall_ReturnsWholePath()
        {
            var path = Path(4);

            CollectionAssert.AreEqual(path, BallMoveRules.TruncateAtLooseBall(path, _ => false));
        }

        [Test]
        public void BallMidPath_StopsOnBallCell()
        {
            var path = Path(5);
            var ballCell = path[2];

            CollectionAssert.AreEqual(path.Take(3), BallMoveRules.TruncateAtLooseBall(path, c => c == ballCell));
        }

        [Test]
        public void BallOnDestination_ReturnsWholePath()
        {
            var path = Path(3);
            var ballCell = path[2];

            CollectionAssert.AreEqual(path, BallMoveRules.TruncateAtLooseBall(path, c => c == ballCell));
        }

        [Test]
        public void BallOnFirstStep_StopsThere()
        {
            var path = Path(3);

            CollectionAssert.AreEqual(path.Take(1), BallMoveRules.TruncateAtLooseBall(path, c => c == path[0]));
        }

        [Test]
        public void EmptyPath_ReturnsEmpty()
        {
            Assert.IsEmpty(BallMoveRules.TruncateAtLooseBall(new List<ICell>(), _ => true));
        }
    }
}
