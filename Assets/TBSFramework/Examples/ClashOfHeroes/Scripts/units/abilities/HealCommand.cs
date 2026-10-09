using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TurnBasedStrategyFramework.Common.Controllers;
using TurnBasedStrategyFramework.Common.Units;
using TurnBasedStrategyFramework.Common.Units.Abilities;

namespace TurnBasedStrategyFramework.Unity.Examples.ClashOfHeroes.Units.Abilities
{
    /// <summary>
    /// A command that heals a target unit by a specified amount.
    /// </summary>
    public readonly struct HealCommand : ICommand
    {
        private readonly IUnit _target;
        private readonly int _healAmount;

        private readonly Func<UniTask> _healHighlighter;

        public HealCommand(int healAmount, IUnit target, Func<UniTask> healHighlighter) : this()
        {
            _healAmount = healAmount;
            _target = target;
            _healHighlighter = healHighlighter;
        }

        public async UniTask Execute(IUnit unit, IGridController controller)
        {
            _target.ModifyHealth(Math.Min(_healAmount, _target.MaxHealth - _target.Health), unit);
            await _healHighlighter();
        }


        public UniTask Undo(IUnit unit, IGridController controller)
        {
            throw new System.NotImplementedException();
        }

        public Dictionary<string, object> Serialize()
        {
            throw new NotImplementedException();
        }

        public ICommand Deserialize(Dictionary<string, object> actionParams, IGridController gridController)
        {
            throw new NotImplementedException();
        }
    }
}