using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using TurnBasedStrategyFramework.Common.Controllers;
using TurnBasedStrategyFramework.Common.Units;
using TurnBasedStrategyFramework.Common.Units.Abilities;

namespace Bootleg.Ball
{
    /// <summary>
    /// The ball carrier's move, followed by an interception that was rolled when the move was chosen.
    /// The interceptor tackles the carrier at no action cost: they swap cells and the interceptor takes the ball.
    /// </summary>
    public readonly struct DribbleCommand : ICommand
    {
        private readonly MoveCommand _move;
        private readonly IUnit _interceptor;
        private readonly BallUnit _ball;

        public DribbleCommand(MoveCommand move, IUnit interceptor, BallUnit ball)
        {
            _move = move;
            _interceptor = interceptor;
            _ball = ball;
        }

        public async UniTask Execute(IUnit unit, IGridController controller)
        {
            await _move.Execute(unit, controller);
            await Interception(unit).Execute(_interceptor, controller);
        }

        public async UniTask Undo(IUnit unit, IGridController controller)
        {
            await Interception(unit).Undo(_interceptor, controller);
            await _move.Undo(unit, controller);
        }

        private TackleCommand Interception(IUnit carrier)
        {
            return new TackleCommand(carrier, _ball, succeeded: true, actionCost: 0);
        }

        private static class SerializationKeys
        {
            public const string Move = "move";
            public const string Interceptor = "interceptor";
            public const string Ball = "ball";
        }

        public Dictionary<string, object> Serialize()
        {
            return new Dictionary<string, object>
            {
                { SerializationKeys.Move, _move.Serialize() },
                { SerializationKeys.Interceptor, _interceptor.UnitID },
                { SerializationKeys.Ball, _ball.UnitID }
            };
        }

        public ICommand Deserialize(Dictionary<string, object> actionParams, IGridController gridController)
        {
            IUnit FindUnit(string key)
            {
                var id = Convert.ToInt32(actionParams[key]);
                return gridController.UnitManager.GetUnits().First(u => u.UnitID == id);
            }

            var move = (MoveCommand)new MoveCommand().Deserialize((Dictionary<string, object>)actionParams[SerializationKeys.Move], gridController);
            return new DribbleCommand(move, FindUnit(SerializationKeys.Interceptor), (BallUnit)FindUnit(SerializationKeys.Ball));
        }
    }
}
