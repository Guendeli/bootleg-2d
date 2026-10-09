using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TurnBasedStrategyFramework.Unity.Highlighters
{
    /// <summary>
    /// A highlighter that introduce a realtime delay.
    /// </summary>
    public class DelayHighlighter : Highlighter
    {
        /// <summary>
        /// Delay to apply in milliseconds.
        /// </summary>
        [SerializeField] private int _delay;
        public async override UniTask Apply(IHighlightParams @params)
        {
            await UniTask.Delay(_delay);
        }
    }
}