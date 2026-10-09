using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TurnBasedStrategyFramework.Unity.Highlighters
{
    /// <summary>
    /// A simple highlighter for debugging purposes that prints a message when applied.
    /// This class is useful for testing and debugging highlight functionality without any visual effects.
    /// </summary>
    public class DebugHighlighter : Highlighter
    {
        /// <summary>
        /// The message to print when the highlighter is applied.
        /// </summary>
        [SerializeField] private string _message;

        public override UniTask Apply(IHighlightParams args)
        {
            Debug.Log(_message);
            return UniTask.CompletedTask;
        }
    }
}