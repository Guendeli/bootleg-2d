using System.Collections.Generic;
using System.Linq;
using Bootleg.Units;
using Cysharp.Threading.Tasks;
using TurnBasedStrategyFramework.Common.Cells;
using TurnBasedStrategyFramework.Common.Controllers;
using TurnBasedStrategyFramework.Common.Controllers.GridStates;
using TurnBasedStrategyFramework.Common.Units;
using TurnBasedStrategyFramework.Unity.Units.Abilities;
using UnityEngine;

namespace Bootleg.Ball
{
    /// <summary>
    /// Lets a unit tackle an orthogonally adjacent enemy who carries the ball. On success the two units swap cells
    /// and the tackler takes the ball (see <see cref="TackleCommand"/>). Valid targets are marked targetable.
    /// </summary>
    public class TackleAbility : Ability
    {
        [SerializeField, Min(0)] private int _actionCost = 1;
        [SerializeField, Range(0f, 1f)] private float _successChance = 1f;

        private Dictionary<IUnit, BallUnit> _targets = new Dictionary<IUnit, BallUnit>();

        public override void OnAbilitySelected(IGridController gridController)
        {
            _targets = FindTargets(gridController);
        }

        public override void OnAbilityDeselected(IGridController gridController)
        {
            _targets = new Dictionary<IUnit, BallUnit>();
        }

        public override bool CanPerform(IGridController gridController)
        {
            return FindTargets(gridController).Count > 0;
        }

        public override void Display(IGridController gridController)
        {
            gridController.UnitManager.MarkAsTargetable(_targets.Keys).Forget();
        }

        public override void CleanUp(IGridController gridController)
        {
            gridController.UnitManager.UnMark(_targets.Keys).Forget();
        }

        public override void OnUnitClicked(IUnit unit, IGridController gridController)
        {
            if (_targets.TryGetValue(unit, out var ball))
            {
                var succeeded = Random.value < _successChance;
                UnitReference.HumanExecuteAbility(new TackleCommand(unit, ball, succeeded, _actionCost), gridController);
            }
            else if (!ReferenceEquals(unit, UnitReference) && gridController.TurnContext.PlayableUnits().Contains(unit))
            {
                gridController.GridState = new GridStateUnitSelected(unit, unit.GetBaseAbilities());
            }
        }

        public override void OnCellClicked(ICell cell, IGridController gridController)
        {
            gridController.GridState = new GridStateAwaitInput();
        }

        private Dictionary<IUnit, BallUnit> FindTargets(IGridController gridController)
        {
            var targets = new Dictionary<IUnit, BallUnit>();
            if (UnitReference.ActionPoints < _actionCost)
            {
                return targets;
            }

            foreach (var ball in gridController.UnitManager.GetUnits().OfType<BallUnit>())
            {
                var carrier = ball.Carrier;
                if (carrier != null
                    && carrier.PlayerNumber != UnitReference.PlayerNumber
                    && carrier.PlayerNumber != Teams.Neutral
                    && carrier.CurrentCell.GetDistance(UnitReference.CurrentCell) == 1)
                {
                    targets[carrier] = ball;
                }
            }
            return targets;
        }
    }
}
