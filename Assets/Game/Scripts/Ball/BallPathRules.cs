using System;
using System.Collections.Generic;
using TurnBasedStrategyFramework.Common.Cells;
using TurnBasedStrategyFramework.Common.Utilities;

namespace Bootleg.Ball
{
    /// <summary>
    /// Straight-line ball travel on a square grid. The ball moves up to <c>power</c> cells and stops before
    /// the map edge or any taken cell (obstacle or unit).
    /// </summary>
    public static class BallPathRules
    {
        private static readonly (int dx, int dy)[] OrthogonalDirections = { (1, 0), (-1, 0), (0, 1), (0, -1) };

        /// <summary>
        /// Push: a unit orthogonally adjacent to the ball sends it straight away from itself.
        /// </summary>
        /// <returns>
        /// The cells the ball travels through, ending on its landing cell.
        /// Empty if the pusher is not orthogonally adjacent or the first cell is blocked.
        /// </returns>
        public static List<ICell> GetPushPath(ICell pusherCell, ICell ballCell, int power, Func<Vector2IntImpl, ICell> cellAt)
        {
            var dx = ballCell.GridCoordinates.x - pusherCell.GridCoordinates.x;
            var dy = ballCell.GridCoordinates.y - pusherCell.GridCoordinates.y;
            if (Math.Abs(dx) + Math.Abs(dy) != 1)
            {
                return new List<ICell>();
            }
            return GetLine(ballCell, dx, dy, power, cellAt);
        }

        /// <summary>
        /// Kick: the carrier sends the ball from its own cell in any orthogonal direction, landing on any free cell
        /// up to <c>power</c> cells away.
        /// </summary>
        /// <returns>Each possible landing cell mapped to the path the ball travels to reach it.</returns>
        public static Dictionary<ICell, List<ICell>> GetKickTargets(ICell carrierCell, int power, Func<Vector2IntImpl, ICell> cellAt)
        {
            var targets = new Dictionary<ICell, List<ICell>>();
            foreach (var (dx, dy) in OrthogonalDirections)
            {
                var line = GetLine(carrierCell, dx, dy, power, cellAt);
                for (var i = 0; i < line.Count; i++)
                {
                    targets[line[i]] = line.GetRange(0, i + 1);
                }
            }
            return targets;
        }

        /// <summary>
        /// Pass: like a kick, but the ball goes to a receiver. In each orthogonal direction the ball crosses free cells;
        /// the first taken cell ends the line, and it is a target only if <paramref name="isReceiver"/> accepts it.
        /// The receiver must be within <c>power</c> cells.
        /// </summary>
        /// <returns>Each receiver's cell mapped to the path the ball travels, ending on that cell.</returns>
        public static Dictionary<ICell, List<ICell>> GetPassTargets(ICell carrierCell, int power, Func<Vector2IntImpl, ICell> cellAt, Func<ICell, bool> isReceiver)
        {
            var targets = new Dictionary<ICell, List<ICell>>();
            foreach (var (dx, dy) in OrthogonalDirections)
            {
                var path = new List<ICell>();
                for (var step = 1; step <= power; step++)
                {
                    var cell = cellAt(new Vector2IntImpl(carrierCell.GridCoordinates.x + dx * step, carrierCell.GridCoordinates.y + dy * step));
                    if (cell == null)
                    {
                        break;
                    }
                    path.Add(cell);
                    if (cell.IsTaken)
                    {
                        if (isReceiver(cell))
                        {
                            targets[cell] = path;
                        }
                        break;
                    }
                }
            }
            return targets;
        }

        private static List<ICell> GetLine(ICell origin, int dx, int dy, int power, Func<Vector2IntImpl, ICell> cellAt)
        {
            var path = new List<ICell>();
            for (var step = 1; step <= power; step++)
            {
                var cell = cellAt(new Vector2IntImpl(origin.GridCoordinates.x + dx * step, origin.GridCoordinates.y + dy * step));
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
