using System.Collections.Generic;
using System.Linq;
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
    /// Lets the ball carrier kick the ball from its own cell in a straight orthogonal line, up to its KickPower.
    /// Every free cell on those lines is a landing target; hovering previews the path, clicking kicks.
    /// The carrier loses possession.
    /// </summary>
    public class KickAbility : Ability
    {
        [SerializeField, Min(0)] private int _actionCost = 1;
        [Tooltip("Kick distance used when the unit is not a TacticsUnit.")]
        [SerializeField, Min(1)] private int _fallbackKickPower = 2;

        private BallUnit _ball;
        private Dictionary<ICell, List<ICell>> _targets = new Dictionary<ICell, List<ICell>>();
        private List<ICell> _previewedPath = new List<ICell>();

        public override void OnAbilitySelected(IGridController gridController)
        {
            (_ball, _targets) = FindTargets(gridController);
        }

        public override void OnAbilityDeselected(IGridController gridController)
        {
            _ball = null;
            _targets = new Dictionary<ICell, List<ICell>>();
            _previewedPath = new List<ICell>();
        }

        public override bool CanPerform(IGridController gridController)
        {
            return FindTargets(gridController).targets.Count > 0;
        }

        public override void Display(IGridController gridController)
        {
            gridController.CellManager.MarkAsReachable(_targets.Keys).Forget();
        }

        public override void CleanUp(IGridController gridController)
        {
            gridController.CellManager.UnMark(_targets.Keys).Forget();
            _previewedPath = new List<ICell>();
        }

        public override void OnCellHighlighted(ICell cell, IGridController gridController)
        {
            if (_targets.TryGetValue(cell, out var path))
            {
                _previewedPath = path;
                gridController.CellManager.MarkAsPath(path, UnitReference.CurrentCell).Forget();
            }
        }

        public override void OnCellDehighlighted(ICell cell, IGridController gridController)
        {
            if (_previewedPath.Count > 0)
            {
                gridController.CellManager.MarkAsReachable(_previewedPath).Forget();
                _previewedPath = new List<ICell>();
            }
        }

        public override void OnCellClicked(ICell cell, IGridController gridController)
        {
            if (!_targets.TryGetValue(cell, out var path))
            {
                gridController.GridState = new GridStateAwaitInput();
                return;
            }
            UnitReference.HumanExecuteAbility(new BallTravelCommand(_ball, _ball.CurrentCell, path, _actionCost, isKick: true), gridController);
        }

        public override void OnUnitClicked(IUnit unit, IGridController gridController)
        {
            if (!ReferenceEquals(unit, UnitReference) && gridController.TurnContext.PlayableUnits().Contains(unit))
            {
                gridController.GridState = new GridStateUnitSelected(unit, unit.GetBaseAbilities());
            }
        }

        private (BallUnit ball, Dictionary<ICell, List<ICell>> targets) FindTargets(IGridController gridController)
        {
            var ball = gridController.UnitManager.GetUnits()
                .OfType<BallUnit>()
                .FirstOrDefault(b => ReferenceEquals(b.Carrier, UnitReference));
            if (ball == null || UnitReference.ActionPoints < _actionCost)
            {
                return (null, new Dictionary<ICell, List<ICell>>());
            }

            var power = BallStats.KickPower(UnitReference, _fallbackKickPower);
            return (ball, BallPathRules.GetKickTargets(UnitReference.CurrentCell, power, gridController.CellManager.GetCellAt));
        }
    }
}
