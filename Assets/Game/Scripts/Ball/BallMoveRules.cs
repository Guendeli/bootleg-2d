using System;
using System.Collections.Generic;
using TurnBasedStrategyFramework.Common.Cells;

namespace Bootleg.Ball
{
    public static class BallMoveRules
    {
        /// <summary>
        /// A unit whose path crosses a loose ball stops on the ball's cell (and takes it).
        /// </summary>
        /// <returns>The path up to and including the first cell holding a loose ball, or the whole path if none does.</returns>
        public static List<ICell> TruncateAtLooseBall(IReadOnlyList<ICell> path, Func<ICell, bool> hasLooseBall)
        {
            var truncated = new List<ICell>(path.Count);
            foreach (var cell in path)
            {
                truncated.Add(cell);
                if (hasLooseBall(cell))
                {
                    break;
                }
            }
            return truncated;
        }

        public static bool HasLooseBall(ICell cell)
        {
            foreach (var unit in cell.CurrentUnits)
            {
                if (unit is BallUnit ball && ball.IsLoose)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>True if the only things on the cell are loose balls, so a unit may step onto it to take the ball.</summary>
        public static bool HoldsOnlyLooseBalls(ICell cell)
        {
            if (cell.CurrentUnits.Count == 0)
            {
                return false;
            }
            foreach (var unit in cell.CurrentUnits)
            {
                if (!(unit is BallUnit ball && ball.IsLoose))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
