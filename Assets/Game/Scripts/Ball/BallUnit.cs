using Bootleg.Units;
using TurnBasedStrategyFramework.Common.Controllers;
using TurnBasedStrategyFramework.Common.Units;
using TurnBasedStrategyFramework.Unity.Units;

namespace Bootleg.Ball
{
    /// <summary>
    /// The ball: a neutral TBSF unit. It never gets a turn, has no abilities and cannot be damaged.
    /// Units interact with it through abilities such as <see cref="PushBallAbility"/>.
    /// </summary>
    public class BallUnit : Unit
    {
        public override void Initialize(IGridController gridController)
        {
            PlayerNumber = Teams.Neutral;
            base.Initialize(gridController);
        }

        public override void ModifyHealth(float healthChangeAmount, IUnit sourceUnit)
        {
        }

        // Hides Unit.Reset, which adds attack/move abilities and an AI brain when the component is added.
        private void Reset()
        {
        }
    }
}
