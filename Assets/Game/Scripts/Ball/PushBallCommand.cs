using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using TurnBasedStrategyFramework.Common.Cells;
using TurnBasedStrategyFramework.Common.Controllers;
using TurnBasedStrategyFramework.Common.Units;
using TurnBasedStrategyFramework.Common.Units.Abilities;
using TurnBasedStrategyFramework.Common.Utilities;

namespace Bootleg.Ball
{
    /// <summary>
    /// Pushes the ball along a precomputed path. Executed by the pushing unit, which pays the action cost;
    /// the ball is the unit that moves.
    /// </summary>
    public readonly struct PushBallCommand : ICommand
    {
        private readonly IUnit _ball;
        private readonly ICell _source;
        private readonly IReadOnlyList<ICell> _path;
        private readonly int _actionCost;

        public PushBallCommand(IUnit ball, ICell source, IReadOnlyList<ICell> path, int actionCost)
        {
            _ball = ball;
            _source = source;
            _path = path;
            _actionCost = actionCost;
        }

        private ICell Destination => _path[_path.Count - 1];

        public async UniTask Execute(IUnit unit, IGridController controller)
        {
            unit.ActionPoints -= _actionCost;

            var destination = Destination;
            _source.IsTaken = false;
            _source.CurrentUnits.Remove(_ball);

            await controller.UnitManager.MarkAsMoving(_ball, _source, destination, _path);
            await _ball.MovementAnimation(_path, destination);

            destination.IsTaken = true;
            _ball.CurrentCell = destination;
            destination.CurrentUnits.Add(_ball);

            await controller.UnitManager.UnMarkAsMoving(_ball, _source, destination, _path);
            _ball.InvokeUnitMoved(new UnitMovedEventArgs(_ball, _source, destination, _path));
        }

        public UniTask Undo(IUnit unit, IGridController controller)
        {
            var destination = Destination;
            destination.IsTaken = false;
            destination.CurrentUnits.Remove(_ball);

            _ball.CurrentCell = _source;
            _ball.WorldPosition = _source.WorldPosition;
            _source.IsTaken = true;
            _source.CurrentUnits.Add(_ball);

            unit.ActionPoints += _actionCost;
            return UniTask.CompletedTask;
        }

        private static class SerializationKeys
        {
            public const string Ball = "ball";
            public const string Source = "source";
            public const string Path = "path";
            public const string ActionCost = "actionCost";

            public const string X = "x";
            public const string Y = "y";
        }

        public Dictionary<string, object> Serialize()
        {
            static Dictionary<string, int> SerializeCoordinates(ICell cell) =>
                new Dictionary<string, int>
                {
                    { SerializationKeys.X, cell.GridCoordinates.x },
                    { SerializationKeys.Y, cell.GridCoordinates.y }
                };

            return new Dictionary<string, object>
            {
                { SerializationKeys.Ball, _ball.UnitID },
                { SerializationKeys.Source, SerializeCoordinates(_source) },
                { SerializationKeys.Path, _path.Select(SerializeCoordinates).ToArray() },
                { SerializationKeys.ActionCost, _actionCost }
            };
        }

        public ICommand Deserialize(Dictionary<string, object> actionParams, IGridController gridController)
        {
            ICell GetCell(Dictionary<string, object> coords)
            {
                var x = Convert.ToInt32(coords[SerializationKeys.X]);
                var y = Convert.ToInt32(coords[SerializationKeys.Y]);
                return gridController.CellManager.GetCellAt(new Vector2IntImpl(x, y));
            }

            var ballId = Convert.ToInt32(actionParams[SerializationKeys.Ball]);
            var ball = gridController.UnitManager.GetUnits().First(u => u.UnitID == ballId);
            var source = GetCell(actionParams[SerializationKeys.Source] as Dictionary<string, object>);
            var path = ((IEnumerable<object>)actionParams[SerializationKeys.Path])
                .Cast<Dictionary<string, object>>()
                .Select(GetCell)
                .ToList();
            var actionCost = Convert.ToInt32(actionParams[SerializationKeys.ActionCost]);

            return new PushBallCommand(ball, source, path, actionCost);
        }
    }
}
