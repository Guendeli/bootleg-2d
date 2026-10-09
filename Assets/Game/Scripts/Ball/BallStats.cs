using Bootleg.Units;
using Bootleg.Units.Stats;
using TurnBasedStrategyFramework.Common.Units;

namespace Bootleg.Ball
{
    internal static class BallStats
    {
        /// <summary>The unit's KickPower stat, or <paramref name="fallback"/> for units without TacticsUnit stats.</summary>
        public static int KickPower(IUnit unit, int fallback)
        {
            return unit is TacticsUnit tacticsUnit && tacticsUnit.Stats != null
                ? tacticsUnit.Stats.GetInt(StatType.KickPower)
                : fallback;
        }
    }
}
