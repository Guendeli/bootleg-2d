using System.Collections.Generic;
using System.Linq;
using Bootleg.Ball;
using Cysharp.Threading.Tasks;
using TurnBasedStrategyFramework.Common.Cells;
using TurnBasedStrategyFramework.Common.Controllers;
using TurnBasedStrategyFramework.Common.Controllers.GridStates;
using TurnBasedStrategyFramework.Common.Units;
using TurnBasedStrategyFramework.Common.Units.Abilities;
using TurnBasedStrategyFramework.Unity.Units.Abilities;

namespace Bootleg.Abilities
{
    /// <summary>
    /// Replaces TBSF's MoveAbility for field units. Same click-to-move flow, plus ball rules:
    /// a path that crosses a loose ball stops on the ball's cell, and clicking a reachable loose ball moves onto it.
    /// Ending a move on the ball makes the unit its carrier (handled by <see cref="BallUnit"/>).
    /// A carrier stops on entering a cell next to an enemy (see TacticsUnit.IsCellTraversable), and each of those
    /// enemies may intercept (see <see cref="DribbleCommand"/>).
    /// </summary>
    public class FieldMoveAbility : Ability
    {
        private HashSet<ICell> _reachableCells = new HashSet<ICell>();
        private List<ICell> _previewedPath = new List<ICell>();

        public override void OnAbilitySelected(IGridController gridController)
        {
            _reachableCells = new HashSet<ICell>(FindReachableCells(gridController));
        }

        public override void OnAbilityDeselected(IGridController gridController)
        {
            _reachableCells = new HashSet<ICell>();
            _previewedPath = new List<ICell>();
        }

        public override bool CanPerform(IGridController gridController)
        {
            return FindReachableCells(gridController).Count > 0;
        }

        public override void Display(IGridController gridController)
        {
            gridController.CellManager.MarkAsReachable(_reachableCells).Forget();
        }

        public override void CleanUp(IGridController gridController)
        {
            gridController.CellManager.UnMark(_reachableCells.Union(_previewedPath)).Forget();
            _previewedPath = new List<ICell>();
            InterceptionRiskPreview.Hide();
        }

        public override void OnCellHighlighted(ICell cell, IGridController gridController)
        {
            Preview(cell, gridController);
        }

        public override void OnCellDehighlighted(ICell cell, IGridController gridController)
        {
            ClearPreview(gridController);
        }

        // The ball's collider covers its cell, so hovering/clicking a loose ball arrives as a unit event.
        public override void OnUnitHighlighted(IUnit unit, IGridController gridController)
        {
            if (unit is BallUnit ball && ball.IsLoose)
            {
                Preview(ball.CurrentCell, gridController);
            }
        }

        public override void OnUnitDehighlighted(IUnit unit, IGridController gridController)
        {
            if (unit is BallUnit)
            {
                ClearPreview(gridController);
            }
        }

        public override void OnCellClicked(ICell cell, IGridController gridController)
        {
            if (!_reachableCells.Contains(cell))
            {
                gridController.GridState = new GridStateAwaitInput();
                return;
            }
            MoveTowards(cell, gridController);
        }

        public override void OnUnitClicked(IUnit unit, IGridController gridController)
        {
            if (unit is BallUnit ball)
            {
                if (ball.IsLoose && _reachableCells.Contains(ball.CurrentCell))
                {
                    MoveTowards(ball.CurrentCell, gridController);
                }
                return;
            }

            if (!ReferenceEquals(unit, UnitReference) && gridController.TurnContext.PlayableUnits().Contains(unit))
            {
                gridController.GridState = new GridStateUnitSelected(unit, unit.GetBaseAbilities());
            }
        }

        private List<ICell> FindReachableCells(IGridController gridController)
        {
            if (UnitReference.ActionPoints <= 0)
            {
                return new List<ICell>();
            }
            UnitReference.CachePaths(gridController.CellManager);
            return UnitReference.GetAvailableDestinations(gridController.CellManager.GetCells());
        }

        private List<ICell> PathTo(ICell destination, IGridController gridController)
        {
            return BallMoveRules.TruncateAtLooseBall(UnitReference.FindPath(destination, gridController.CellManager), BallMoveRules.HasLooseBall);
        }

        private void MoveTowards(ICell destination, IGridController gridController)
        {
            var path = PathTo(destination, gridController);
            if (path.Count == 0)
            {
                return;
            }
            // The path may stop short of the clicked cell (at a loose ball).
            var endCell = path[path.Count - 1];
            var move = new MoveCommand(UnitReference.CurrentCell, endCell, path);

            // A ball carrier ending next to enemies gets challenged; the roll happens now so the command replays identically.
            var ball = BallInterception.FindCarriedBall(UnitReference, gridController);
            var interceptor = ball != null ? BallInterception.ResolveDribble(endCell, UnitReference, gridController) : null;
            if (interceptor != null)
            {
                UnitReference.HumanExecuteAbility(new DribbleCommand(move, interceptor, ball), gridController);
            }
            else
            {
                UnitReference.HumanExecuteAbility(move, gridController);
            }
        }

        private void Preview(ICell destination, IGridController gridController)
        {
            if (!_reachableCells.Contains(destination))
            {
                return;
            }
            ClearPreview(gridController);
            _previewedPath = PathTo(destination, gridController);
            gridController.CellManager.MarkAsPath(_previewedPath, UnitReference.CurrentCell).Forget();

            if (_previewedPath.Count > 0 && BallInterception.FindCarriedBall(UnitReference, gridController) != null)
            {
                InterceptionRiskPreview.Show(BallInterception.DribbleRisk(_previewedPath[_previewedPath.Count - 1], UnitReference, gridController));
            }
        }

        private void ClearPreview(IGridController gridController)
        {
            InterceptionRiskPreview.Hide();
            if (_previewedPath.Count == 0)
            {
                return;
            }
            gridController.CellManager.UnMark(_previewedPath).Forget();
            gridController.CellManager.MarkAsReachable(_previewedPath.Where(_reachableCells.Contains)).Forget();
            _previewedPath = new List<ICell>();
        }
    }
}
