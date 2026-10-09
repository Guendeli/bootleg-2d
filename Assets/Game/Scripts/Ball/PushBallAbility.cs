using System.Collections.Generic;
using System.Linq;
using Bootleg.Units;
using Bootleg.Units.Stats;
using Cysharp.Threading.Tasks;
using TurnBasedStrategyFramework.Common.Cells;
using TurnBasedStrategyFramework.Common.Controllers;
using TurnBasedStrategyFramework.Common.Units;
using TurnBasedStrategyFramework.Unity.Units.Abilities;
using UnityEngine;

namespace Bootleg.Ball
{
    /// <summary>
    /// Lets a unit push an orthogonally adjacent ball. Push distance is the unit's <see cref="StatType.KickPower"/>.
    /// The pushable ball is marked targetable; hovering it previews the path, clicking it pushes.
    /// </summary>
    public class PushBallAbility : Ability
    {
        [SerializeField, Min(0)] private int _actionCost = 1;
        [Tooltip("Push distance used when the unit is not a TacticsUnit.")]
        [SerializeField, Min(1)] private int _fallbackKickPower = 2;

        private Dictionary<BallUnit, List<ICell>> _pushPaths = new Dictionary<BallUnit, List<ICell>>();
        private List<ICell> _previewedPath = new List<ICell>();

        private int KickPower => UnitReference is TacticsUnit tacticsUnit && tacticsUnit.Stats != null
            ? tacticsUnit.Stats.GetInt(StatType.KickPower)
            : _fallbackKickPower;

        public override void OnAbilitySelected(IGridController gridController)
        {
            _pushPaths = FindPushPaths(gridController);
        }

        public override void OnAbilityDeselected(IGridController gridController)
        {
            _pushPaths = new Dictionary<BallUnit, List<ICell>>();
            _previewedPath = new List<ICell>();
        }

        public override bool CanPerform(IGridController gridController)
        {
            return FindPushPaths(gridController).Count > 0;
        }

        public override void Display(IGridController gridController)
        {
            gridController.UnitManager.MarkAsTargetable(_pushPaths.Keys).Forget();
        }

        public override void CleanUp(IGridController gridController)
        {
            gridController.UnitManager.UnMark(_pushPaths.Keys).Forget();
            ClearPreview(gridController);
        }

        public override void OnUnitHighlighted(IUnit unit, IGridController gridController)
        {
            if (unit is BallUnit ball && _pushPaths.TryGetValue(ball, out var path))
            {
                _previewedPath = path;
                gridController.CellManager.MarkAsPath(path, ball.CurrentCell).Forget();
            }
        }

        public override void OnUnitDehighlighted(IUnit unit, IGridController gridController)
        {
            ClearPreview(gridController);
        }

        public override void OnUnitClicked(IUnit unit, IGridController gridController)
        {
            if (unit is BallUnit ball && _pushPaths.TryGetValue(ball, out var path))
            {
                UnitReference.HumanExecuteAbility(new PushBallCommand(ball, ball.CurrentCell, path, _actionCost), gridController);
            }
        }

        private Dictionary<BallUnit, List<ICell>> FindPushPaths(IGridController gridController)
        {
            var paths = new Dictionary<BallUnit, List<ICell>>();
            if (UnitReference.ActionPoints < _actionCost)
            {
                return paths;
            }

            var kickPower = KickPower;
            foreach (var ball in gridController.UnitManager.GetUnits().OfType<BallUnit>())
            {
                var path = BallPushRules.GetPushPath(UnitReference.CurrentCell, ball.CurrentCell, kickPower, gridController.CellManager.GetCellAt);
                if (path.Count > 0)
                {
                    paths[ball] = path;
                }
            }
            return paths;
        }

        private void ClearPreview(IGridController gridController)
        {
            if (_previewedPath.Count == 0)
            {
                return;
            }

            gridController.CellManager.UnMark(_previewedPath).Forget();

            // Unmarking also cleared the move ability's highlight on these cells; restore it where it applied.
            if (UnitReference.ActionPoints > 0)
            {
                var reachable = UnitReference.GetAvailableDestinations(gridController.CellManager.GetCells());
                gridController.CellManager.MarkAsReachable(_previewedPath.Where(reachable.Contains)).Forget();
            }
            _previewedPath = new List<ICell>();
        }
    }
}
