using Cysharp.Threading.Tasks;

namespace TurnBasedStrategyFramework.Common.AI.BehaviourTrees
{
    /// <summary>
    /// Represents a node in a behavior tree, used for AI decision-making.
    /// </summary>
    public interface ITreeNode
    {
        /// <summary>
        /// Executes the behavior represented by this tree node.
        /// </summary>
        /// <returns>A task representing the execution, with a boolean result indicating success or failure.</returns>
        UniTask<bool> Execute(bool debugMode);
    }
}
