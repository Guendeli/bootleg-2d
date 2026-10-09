using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TurnBasedStrategyFramework.Unity.Highlighters
{
    /// <summary>
    /// A highlighter that changes the color of the target sprite renderer.
    /// </summary>
    public class SpriteRendererHighlighter : Highlighter
    {
        [SerializeField] private SpriteRenderer _sprite;
        [SerializeField] private Color _color;

        public void SetColor(Color color)
        {
            _color = color;
        }

        public override UniTask Apply(IHighlightParams @params)
        {
            _sprite.color = _color;
            return UniTask.CompletedTask;
        }
    }
}