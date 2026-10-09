using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TurnBasedStrategyFramework.Common.Controllers;

namespace TurnBasedStrategyFramework.Common.Units.Abilities
{
    /// <summary>
    /// Represents a command to end the current player's turn.
    /// </summary>
    public class EndTurnCommand : ICommand
    {
        public UniTask Execute(IUnit unit, IGridController controller)
        {
            controller.MakeTurnTransition();
            return UniTask.CompletedTask;
        }

        public UniTask Undo(IUnit unit, IGridController controller)
        {
            return UniTask.CompletedTask;
        }

        public Dictionary<string, object> Serialize()
        {
            return new Dictionary<string, object> { };
        }

        public ICommand Deserialize(Dictionary<string, object> actionParams, IGridController gridController)
        {
            throw new System.NotImplementedException();
        }
    }
}