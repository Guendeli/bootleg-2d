using Cysharp.Threading.Tasks;

namespace TurnBasedStrategyFramework.Common.AI.BehaviourTrees
{
    /// <summary>
    /// Introduces a realtime delay to AI decision making.
    /// </summary>
    public readonly struct RealtimeDelayNode : ITreeNode
    {
        /// <summary>
        /// A behavior tree node that introduces a real-time delay, unaffected by Time.timeScale.
        /// Runs on Unity's player loop via UniTask, so it also works in WebGL builds.
        /// </summary>
        private readonly int _delay;

        public RealtimeDelayNode(int delay)
        {
            _delay = delay;
        }

        public async UniTask<bool> Execute(bool debugMode)
        {
            await UniTask.Delay(_delay, DelayType.Realtime);
            return true;
        }
    }
}