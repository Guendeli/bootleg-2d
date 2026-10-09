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
    /// Lets the ball carrier pass to a teammate in a straight orthogonal line within its KickPower. The ball flies past
    /// enemies, and each enemy on or next to the path may intercept it; obstacles block the pass. Receivers are marked
    /// targetable; hovering one previews the path, clicking passes. The receiver (or interceptor) takes possession.
    /// </summary>
    public class PassAbility : Ability
    {
        [SerializeField, Min(0)] private int _actionCost = 1;
        [Tooltip("Pass distance used when the unit is not a TacticsUnit.")]
        [SerializeField, Min(1)] private int _fallbackKickPower = 2;

        private BallUnit _ball;
        private Dictionary<IUnit, List<ICell>> _receivers = new Dictionary<IUnit, List<ICell>>();
        private List<ICell> _previewedPath = new List<ICell>();

        public override void OnAbilitySelected(IGridController gridController)
        {
            (_ball, _receivers) = FindReceivers(gridController);
        }

        public override void OnAbilityDeselected(IGridController gridController)
        {
            _ball = null;
            _receivers = new Dictionary<IUnit, List<ICell>>();
            _previewedPath = new List<ICell>();
        }

        public override bool CanPerform(IGridController gridController)
        {
            return FindReceivers(gridController).receivers.Count > 0;
        }

        public override void Display(IGridController gridController)
        {
            gridController.UnitManager.MarkAsTargetable(_receivers.Keys).Forget();
        }

        public override void CleanUp(IGridController gridController)
        {
            gridController.UnitManager.UnMark(_receivers.Keys).Forget();
            ClearPreview(gridController);
        }

        public override void OnUnitHighlighted(IUnit unit, IGridController gridController)
        {
            if (_receivers.TryGetValue(unit, out var path))
            {
                ClearPreview(gridController);
                _previewedPath = path;
                gridController.CellManager.MarkAsPath(path, UnitReference.CurrentCell).Forget();
                InterceptionRiskPreview.Show(BallInterception.FlightRisk(path, UnitReference, gridController));
            }
        }

        public override void OnUnitDehighlighted(IUnit unit, IGridController gridController)
        {
            ClearPreview(gridController);
        }

        public override void OnUnitClicked(IUnit unit, IGridController gridController)
        {
            if (_receivers.TryGetValue(unit, out var path))
            {
                var (travelledPath, interceptor) = BallInterception.ResolveFlight(path, UnitReference, gridController);
                UnitReference.HumanExecuteAbility(
                    new BallTravelCommand(_ball, _ball.CurrentCell, travelledPath, _actionCost, receiver: interceptor ?? unit),
                    gridController);
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

        private (BallUnit ball, Dictionary<IUnit, List<ICell>> receivers) FindReceivers(IGridController gridController)
        {
            var receivers = new Dictionary<IUnit, List<ICell>>();
            var ball = gridController.UnitManager.GetUnits()
                .OfType<BallUnit>()
                .FirstOrDefault(b => ReferenceEquals(b.Carrier, UnitReference));
            if (ball == null || UnitReference.ActionPoints < _actionCost)
            {
                return (null, receivers);
            }

            var power = BallStats.KickPower(UnitReference, _fallbackKickPower);
            var targets = BallPathRules.GetPassTargets(UnitReference.CurrentCell, power, gridController.CellManager.GetCellAt,
                cell => FindTeammate(cell) != null, BallInterception.HasEnemyOf(UnitReference));
            foreach (var target in targets)
            {
                receivers[FindTeammate(target.Key)] = target.Value;
            }
            return (ball, receivers);
        }

        private IUnit FindTeammate(ICell cell)
        {
            return cell.CurrentUnits.FirstOrDefault(u =>
                !ReferenceEquals(u, UnitReference)
                && u.PlayerNumber == UnitReference.PlayerNumber
                && u.PlayerNumber != Teams.Neutral);
        }

        private void ClearPreview(IGridController gridController)
        {
            InterceptionRiskPreview.Hide();
            if (_previewedPath.Count > 0)
            {
                gridController.CellManager.UnMark(_previewedPath).Forget();
                _previewedPath = new List<ICell>();
            }
        }
    }
}
