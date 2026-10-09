using System;
using Cysharp.Threading.Tasks;
using TurnBasedStrategyFramework.Common.Controllers;
using TurnBasedStrategyFramework.Common.Units.Abilities;
using TurnBasedStrategyFramework.Common.Controllers.GridStates;
using TurnBasedStrategyFramework.Common.Units;

namespace TurnBasedStrategyFramework.Unity.Units
{
    /// <summary>
    /// Helper class for unit ability execution handling common ExecuteAbility workflows.
    /// </summary>
    public static class UnitHelper
    {
        /// <summary>
        /// Invokes the AbilityUsed event and completes immediately.
        /// </summary>
        public static UniTask ExecuteAbility(IUnit unit, ICommand command, Func<IGridController, UniTask> preAction, Func<IGridController, UniTask> postAction, bool isNetworkInvoked = false)
        {
            unit.InvokeAbilityUsed(new AbilityUsedEventArgs(unit, command, preAction, postAction, isNetworkInvoked));
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// Executes a human-initiated ability with default pre and post actions (no-op).
        /// </summary>
        public static UniTask HumanExecuteAbility(IUnit unit, ICommand command, IGridController gridController, bool isNetworkInvoked = false)
        {
            return HumanExecuteAbility(unit, command, gridController, _ => UniTask.CompletedTask, _ => UniTask.CompletedTask, isNetworkInvoked);
        }

        /// <summary>
        /// Executes a human-initiated ability with custom pre and post actions.
        /// PreAction blocks grid input; PostAction restores unit selection state.
        /// </summary>
        public static UniTask HumanExecuteAbility(IUnit unit, ICommand command, IGridController gridController,
            Func<IGridController, UniTask> preAction, Func<IGridController, UniTask> postAction, bool isNetworkInvoked = false)
        {
            return ExecuteAbility(unit,
                command,
                controller => {
                    preAction(gridController);
                    gridController.GridState = new GridStateBlockInput();
                    return UniTask.CompletedTask;
                },
                controller => {
                    postAction(gridController);
                    gridController.GridState = new GridStateUnitSelected(unit, unit.GetBaseAbilities());
                    return UniTask.CompletedTask;
                },
                isNetworkInvoked);
        }

        /// <summary>
        /// Executes an AI-initiated ability with default pre and post actions (no-op), signaling completion via TaskCompletionSource.
        /// </summary>
        public static UniTask AIExecuteAbility(IUnit unit, ICommand command, IGridController gridController, UniTaskCompletionSource<bool> tcs, bool isNetworkInvoked = false)
        {
            return AIExecuteAbility(unit, command, gridController, tcs, _ => UniTask.CompletedTask, _ => UniTask.CompletedTask, isNetworkInvoked);
        }

        /// <summary>
        /// Executes an AI-initiated ability with custom pre and post actions.
        /// PostAction sets TaskCompletionSource result to signal completion.
        /// </summary>
        public static UniTask AIExecuteAbility(IUnit unit, ICommand command, IGridController gridController, UniTaskCompletionSource<bool> tcs,
            Func<IGridController, UniTask> preAction, Func<IGridController, UniTask> postAction, bool isNetworkInvoked = false)
        {
            return ExecuteAbility(unit,
                command,
                _ => {
                    preAction(gridController);
                    return UniTask.CompletedTask;
                },
                _ => {
                    postAction(gridController);
                    tcs.TrySetResult(true);
                    return UniTask.CompletedTask;
                },
                isNetworkInvoked);
        }
    }
}
