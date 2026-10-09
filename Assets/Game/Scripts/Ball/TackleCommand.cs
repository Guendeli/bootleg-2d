using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using TurnBasedStrategyFramework.Common.Cells;
using TurnBasedStrategyFramework.Common.Controllers;
using TurnBasedStrategyFramework.Common.Units;
using TurnBasedStrategyFramework.Common.Units.Abilities;

namespace Bootleg.Ball
{
    /// <summary>
    /// Tackles the unit carrying the ball. On success the tackler and the tackled unit swap cells, so the tackler
    /// ends up on the ball's cell and takes possession. On failure only the action cost is spent.
    /// The success roll happens when the command is created, so every client replays the same result.
    /// </summary>
    public readonly struct TackleCommand : ICommand
    {
        private readonly IUnit _target;
        private readonly BallUnit _ball;
        private readonly bool _succeeded;
        private readonly int _actionCost;

        public TackleCommand(IUnit target, BallUnit ball, bool succeeded, int actionCost)
        {
            _target = target;
            _ball = ball;
            _succeeded = succeeded;
            _actionCost = actionCost;
        }

        public async UniTask Execute(IUnit unit, IGridController controller)
        {
            unit.ActionPoints -= _actionCost;
            if (!_succeeded)
            {
                return;
            }

            var tacklerCell = unit.CurrentCell;
            var targetCell = _target.CurrentCell;

            // The ball rests on its cell while the units swap, then the tackler picks it up.
            _ball.Release();

            var tacklerPath = new[] { targetCell };
            var targetPath = new[] { tacklerCell };
            await UniTask.WhenAll(
                controller.UnitManager.MarkAsMoving(unit, tacklerCell, targetCell, tacklerPath),
                controller.UnitManager.MarkAsMoving(_target, targetCell, tacklerCell, targetPath));
            // MovementAnimation reads CurrentCell, so animate before reassigning cells.
            await UniTask.WhenAll(
                unit.MovementAnimation(tacklerPath, targetCell),
                _target.MovementAnimation(targetPath, tacklerCell));

            Swap(unit, _target);
            // Before raising UnitMoved: the ball follows its carrier's moves, and the tackled unit must not take it along.
            _ball.TakePossession(unit);

            await UniTask.WhenAll(
                controller.UnitManager.UnMarkAsMoving(unit, tacklerCell, targetCell, tacklerPath),
                controller.UnitManager.UnMarkAsMoving(_target, targetCell, tacklerCell, targetPath));
            _target.InvokeUnitMoved(new UnitMovedEventArgs(_target, targetCell, tacklerCell, targetPath));
            unit.InvokeUnitMoved(new UnitMovedEventArgs(unit, tacklerCell, targetCell, tacklerPath));
        }

        public UniTask Undo(IUnit unit, IGridController controller)
        {
            if (_succeeded)
            {
                Swap(unit, _target);
                unit.WorldPosition = unit.CurrentCell.WorldPosition;
                _target.WorldPosition = _target.CurrentCell.WorldPosition;
                _ball.TakePossession(_target);
            }
            unit.ActionPoints += _actionCost;
            return UniTask.CompletedTask;
        }

        /// <summary>Exchanges the two units' cells. Both cells stay taken; the ball stays where it is.</summary>
        private static void Swap(IUnit a, IUnit b)
        {
            var cellA = a.CurrentCell;
            var cellB = b.CurrentCell;

            cellA.CurrentUnits.Remove(a);
            cellB.CurrentUnits.Remove(b);
            a.CurrentCell = cellB;
            b.CurrentCell = cellA;
            cellB.CurrentUnits.Add(a);
            cellA.CurrentUnits.Add(b);
        }

        private static class SerializationKeys
        {
            public const string Target = "target";
            public const string Ball = "ball";
            public const string Succeeded = "succeeded";
            public const string ActionCost = "actionCost";
        }

        public Dictionary<string, object> Serialize()
        {
            return new Dictionary<string, object>
            {
                { SerializationKeys.Target, _target.UnitID },
                { SerializationKeys.Ball, _ball.UnitID },
                { SerializationKeys.Succeeded, _succeeded },
                { SerializationKeys.ActionCost, _actionCost }
            };
        }

        public ICommand Deserialize(Dictionary<string, object> actionParams, IGridController gridController)
        {
            IUnit FindUnit(string key)
            {
                var id = Convert.ToInt32(actionParams[key]);
                return gridController.UnitManager.GetUnits().First(u => u.UnitID == id);
            }

            return new TackleCommand(
                FindUnit(SerializationKeys.Target),
                (BallUnit)FindUnit(SerializationKeys.Ball),
                Convert.ToBoolean(actionParams[SerializationKeys.Succeeded]),
                Convert.ToInt32(actionParams[SerializationKeys.ActionCost]));
        }
    }
}
