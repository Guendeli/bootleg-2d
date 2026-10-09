using System;
using System.Collections.Generic;
using System.Linq;
using Bootleg.Units;
using TurnBasedStrategyFramework.Common.Cells;
using TurnBasedStrategyFramework.Common.Controllers;
using TurnBasedStrategyFramework.Common.Units;

namespace Bootleg.Ball
{
    /// <summary>The combined chance that a pass, kick or dribble gets intercepted, for previews.</summary>
    public readonly struct InterceptionRisk
    {
        public readonly float Chance;
        public readonly int Defenders;
        /// <summary>Where to show the preview: the landing cell, receiver's cell or dribble destination.</summary>
        public readonly ICell At;

        public InterceptionRisk(float chance, int defenders, ICell at)
        {
            Chance = chance;
            Defenders = defenders;
            At = at;
        }
    }

    /// <summary>
    /// Applies <see cref="InterceptionRules"/> to the live game: finds the enemies, uses each defender's Interception
    /// stat as their chance and rolls. Abilities call this when the player clicks, and bake the result into the command
    /// so every client replays the same outcome. Previews use the same candidates and chances, so they always match the roll.
    /// </summary>
    internal static class BallInterception
    {
        public static IUnit FindEnemy(ICell cell, IUnit of)
        {
            return cell.CurrentUnits.FirstOrDefault(u => u.PlayerNumber != of.PlayerNumber && u.PlayerNumber != Teams.Neutral);
        }

        public static Func<ICell, bool> HasEnemyOf(IUnit unit)
        {
            return cell => FindEnemy(cell, unit) != null;
        }

        /// <summary>Pass or kick: rolls every enemy on or next to the path in order.</summary>
        /// <returns>The path the ball actually travels, and the interceptor (null if nobody intercepts).</returns>
        public static (List<ICell> path, IUnit interceptor) ResolveFlight(IReadOnlyList<ICell> path, IUnit kicker, IGridController gridController)
        {
            var (candidates, interceptors, chances) = FlightCandidates(path, kicker, gridController);
            var winner = InterceptionRules.FirstSuccess(chances, Roll);

            return winner < 0
                ? (path.ToList(), null)
                : (InterceptionRules.InterceptedPath(path, candidates[winner]), interceptors[winner]);
        }

        public static InterceptionRisk FlightRisk(IReadOnlyList<ICell> path, IUnit kicker, IGridController gridController)
        {
            var (_, interceptors, chances) = FlightCandidates(path, kicker, gridController);
            return new InterceptionRisk(InterceptionRules.CombinedChance(chances), interceptors.Count, path[path.Count - 1]);
        }

        /// <summary>Dribble: rolls every enemy next to the carrier's destination in order.</summary>
        /// <returns>The interceptor, or null if nobody intercepts.</returns>
        public static IUnit ResolveDribble(ICell destination, IUnit carrier, IGridController gridController)
        {
            var (interceptors, chances) = DribbleCandidates(destination, carrier, gridController);
            var winner = InterceptionRules.FirstSuccess(chances, Roll);
            return winner < 0 ? null : interceptors[winner];
        }

        public static InterceptionRisk DribbleRisk(ICell destination, IUnit carrier, IGridController gridController)
        {
            var (interceptors, chances) = DribbleCandidates(destination, carrier, gridController);
            return new InterceptionRisk(InterceptionRules.CombinedChance(chances), interceptors.Count, destination);
        }

        /// <summary>Cells next to an enemy of <paramref name="unit"/>: a ball carrier stops on entering one.</summary>
        public static HashSet<ICell> EnemyZoneCells(IUnit unit, ICellManager cellManager)
        {
            var hasEnemy = HasEnemyOf(unit);
            return new HashSet<ICell>(cellManager.GetCells()
                .Where(cell => InterceptionRules.EnemiesAround(cell, cellManager.GetCellAt, hasEnemy).Count > 0));
        }

        public static BallUnit FindCarriedBall(IUnit carrier, IGridController gridController)
        {
            return gridController.UnitManager.GetUnits().OfType<BallUnit>().FirstOrDefault(b => ReferenceEquals(b.Carrier, carrier));
        }

        private static (List<InterceptionCandidate> candidates, List<IUnit> interceptors, List<float> chances) FlightCandidates(
            IReadOnlyList<ICell> path, IUnit kicker, IGridController gridController)
        {
            var candidates = InterceptionRules.AlongBallPath(path, gridController.CellManager.GetCellAt, HasEnemyOf(kicker));
            var interceptors = candidates.Select(c => FindEnemy(c.InterceptorCell, kicker)).ToList();
            return (candidates, interceptors, Chances(interceptors));
        }

        private static (List<IUnit> interceptors, List<float> chances) DribbleCandidates(ICell destination, IUnit carrier, IGridController gridController)
        {
            var interceptors = InterceptionRules.EnemiesAround(destination, gridController.CellManager.GetCellAt, HasEnemyOf(carrier))
                .Select(cell => FindEnemy(cell, carrier))
                .ToList();
            return (interceptors, Chances(interceptors));
        }

        private static List<float> Chances(IEnumerable<IUnit> interceptors)
        {
            return interceptors.Select(u => BallStats.InterceptionChance(u)).ToList();
        }

        private static float Roll()
        {
            return UnityEngine.Random.value;
        }
    }
}
