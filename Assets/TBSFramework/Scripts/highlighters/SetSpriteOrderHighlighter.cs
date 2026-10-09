using Cysharp.Threading.Tasks;
using TurnBasedStrategyFramework.Unity.Highlighters;
using UnityEngine;

namespace TurnBasedStrategyFramework.Unity.Examples.Legacy.Example4.Highlighters
{
    /// <summary>
    /// A highlighter that sets sorting order for given sprite.
    /// </summary>
    public class SetSpriteOrderHighlighter : Highlighter
    {
        [SerializeField] private SpriteRenderer _targetSprite;
        [SerializeField] private int _order;
        public override UniTask Apply(IHighlightParams @params)
        {
            _targetSprite.sortingOrder = _order;
            return UniTask.CompletedTask;
        }
    }
}