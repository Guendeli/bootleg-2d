using System;
using System.Collections.Generic;
using TurnBasedStrategyFramework.Common.Cells;
using TurnBasedStrategyFramework.Common.Utilities;

namespace Bootleg.Ball
{
    /// <summary>
    /// Push rules for a square grid: a unit orthogonally adjacent to the ball pushes it straight away from itself,
    /// up to <c>power</c> cells, stopping before the map edge or any taken cell (obstacle or unit).
    /// </summary>
    public static class BallPushRules
    {
        /// <returns>
        /// The cells the ball travels through, ending on its landing cell.
        /// Empty if the pusher is not orthogonally adjacent or the first cell is blocked.
        /// </returns>
        public static List<ICell> GetPushPath(ICell pusherCell, ICell ballCell, int power, Func<Vector2IntImpl, ICell> cellAt)
        {
            var path = new List<ICell>();
            var dx = ballCell.GridCoordinates.x - pusherCell.GridCoordinates.x;
            var dy = ballCell.GridCoordinates.y - pusherCell.GridCoordinates.y;
            if (Math.Abs(dx) + Math.Abs(dy) != 1)
            {
                return path;
            }

            for (var step = 1; step <= power; step++)
            {
                var cell = cellAt(new Vector2IntImpl(ballCell.GridCoordinates.x + dx * step, ballCell.GridCoordinates.y + dy * step));
                if (cell == null || cell.IsTaken)
                {
                    break;
                }
                path.Add(cell);
            }
            return path;
        }
    }
}
