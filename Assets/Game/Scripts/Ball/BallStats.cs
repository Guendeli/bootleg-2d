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

        /// <summary>The unit's Interception stat (0..1), or <paramref name="fallback"/> for units without TacticsUnit stats.</summary>
        public static float InterceptionChance(IUnit unit, float fallback = UnitDefinition.DefaultInterception)
        {
            return unit is TacticsUnit tacticsUnit && tacticsUnit.Stats != null
                ? tacticsUnit.Stats.Get(StatType.Interception)
                : fallback;
        }
    }
}
