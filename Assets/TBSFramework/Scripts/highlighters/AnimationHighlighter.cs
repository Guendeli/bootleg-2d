using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TurnBasedStrategyFramework.Unity.Highlighters
{
    /// <summary>
    /// A highlighter that triggers an animation on a given animator by setting a specified trigger parameter.
    /// Optionally, it waits for a delay after triggering the animation.
    /// </summary>
    public class AnimationHighlighter : Highlighter
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private string _parameter;
        [SerializeField] private int _delay;

        public async override UniTask Apply(IHighlightParams @params)
        {
            _animator.SetTrigger(_parameter);
            await UniTask.Delay(_delay);
        }
    }
}