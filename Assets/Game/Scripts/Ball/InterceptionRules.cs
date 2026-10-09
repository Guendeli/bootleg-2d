using System;
using System.Collections.Generic;
using TurnBasedStrategyFramework.Common.Cells;
using TurnBasedStrategyFramework.Common.Utilities;

namespace Bootleg.Ball
{
    /// <summary>An enemy who gets a chance to intercept, and how far along the ball's path that happens.</summary>
    public readonly struct InterceptionCandidate
    {
        /// <summary>Index of the path cell the ball is on when this enemy challenges it.</summary>
        public readonly int PathIndex;
        public readonly ICell InterceptorCell;

        public InterceptionCandidate(int pathIndex, ICell interceptorCell)
        {
            PathIndex = pathIndex;
            InterceptorCell = interceptorCell;
        }
    }

    /// <summary>
    /// Who can intercept, in what order. Pure rules with no randomness: callers roll each candidate's chance in order
    /// and the first success intercepts.
    /// </summary>
    public static class InterceptionRules
    {
        // Fixed order so candidate order (and therefore roll order) is deterministic.
        private static readonly (int dx, int dy)[] OrthogonalDirections = { (1, 0), (-1, 0), (0, 1), (0, -1) };

        /// <summary>
        /// Ball in flight (pass/kick): every enemy standing on a path cell or orthogonally next to one, each counted once,
        /// in the order the ball reaches them. Enemies on the path come before neighbours of the same cell.
        /// </summary>
        public static List<InterceptionCandidate> AlongBallPath(IReadOnlyList<ICell> path, Func<Vector2IntImpl, ICell> cellAt, Func<ICell, bool> hasEnemy)
        {
            var candidates = new List<InterceptionCandidate>();
            var seen = new HashSet<ICell>();
            for (var i = 0; i < path.Count; i++)
            {
                if (hasEnemy(path[i]) && seen.Add(path[i]))
                {
                    candidates.Add(new InterceptionCandidate(i, path[i]));
                }
                foreach (var neighbour in OrthogonalNeighbours(path[i], cellAt))
                {
                    if (hasEnemy(neighbour) && seen.Add(neighbour))
                    {
                        candidates.Add(new InterceptionCandidate(i, neighbour));
                    }
                }
            }
            return candidates;
        }

        /// <summary>Dribble: the enemies orthogonally next to the cell the carrier ends its move on.</summary>
        public static List<ICell> EnemiesAround(ICell cell, Func<Vector2IntImpl, ICell> cellAt, Func<ICell, bool> hasEnemy)
        {
            var enemies = new List<ICell>();
            foreach (var neighbour in OrthogonalNeighbours(cell, cellAt))
            {
                if (hasEnemy(neighbour))
                {
                    enemies.Add(neighbour);
                }
            }
            return enemies;
        }

        /// <summary>
        /// The path the ball actually travels when <paramref name="candidate"/> intercepts: up to the challenge point,
        /// then onto the interceptor's cell if they stand next to the path rather than on it.
        /// </summary>
        public static List<ICell> InterceptedPath(IReadOnlyList<ICell> path, InterceptionCandidate candidate)
        {
            var intercepted = new List<ICell>(candidate.PathIndex + 2);
            for (var i = 0; i <= candidate.PathIndex; i++)
            {
                intercepted.Add(path[i]);
            }
            if (!path[candidate.PathIndex].Equals(candidate.InterceptorCell))
            {
                intercepted.Add(candidate.InterceptorCell);
            }
            return intercepted;
        }

        /// <summary>Rolls each chance in order.</summary>
        /// <returns>Index of the first success, or -1 if every roll fails.</returns>
        public static int FirstSuccess(IReadOnlyList<float> chances, Func<float> random01)
        {
            for (var i = 0; i < chances.Count; i++)
            {
                if (Succeeds(chances[i], random01))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Chance that at least one of the rolls succeeds: 1 - (1 - p1)(1 - p2)... Each chance is clamped to 0..1.
        /// </summary>
        public static float CombinedChance(IEnumerable<float> chances)
        {
            var allFail = 1f;
            foreach (var chance in chances)
            {
                allFail *= 1f - Math.Clamp(chance, 0f, 1f);
            }
            return 1f - allFail;
        }

        /// <summary>One roll. Exact at the ends: 0 never succeeds and 1 always does, even if the roll returns 0 or 1.</summary>
        public static bool Succeeds(float chance, Func<float> random01)
        {
            return chance >= 1f || (chance > 0f && random01() < chance);
        }

        private static IEnumerable<ICell> OrthogonalNeighbours(ICell cell, Func<Vector2IntImpl, ICell> cellAt)
        {
            foreach (var (dx, dy) in OrthogonalDirections)
            {
                var neighbour = cellAt(new Vector2IntImpl(cell.GridCoordinates.x + dx, cell.GridCoordinates.y + dy));
                if (neighbour != null)
                {
                    yield return neighbour;
                }
            }
        }
    }
}
