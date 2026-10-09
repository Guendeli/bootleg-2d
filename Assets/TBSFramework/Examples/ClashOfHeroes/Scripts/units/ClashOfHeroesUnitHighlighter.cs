using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TurnBasedStrategyFramework.Unity.Highlighters;
using TurnBasedStrategyFramework.Unity.Units;
using UnityEngine;

namespace TurnBasedStrategyFramework.Unity.Examples.ClashOfHeroes.Units
{
    /// <summary>
    /// Applies visual highlight effects for generic Clash of Heroes demo unit.
    /// </summary>
    public class ClashOfHeroesUnitHighlighter : MonoBehaviour, IRammedHighlighter
    {
        [SerializeField] private List<Highlighter> _rammedDamageHighlighterFn = new List<Highlighter>();
        [SerializeField] private List<Highlighter> _rammedKnockbackHighlighterFn = new List<Highlighter>();

        public async UniTask ApplyDamageEffect()
        {
            foreach (var fn in _rammedDamageHighlighterFn)
            {
                await fn.Apply(NoParam.Instance);
            }
        }

        public async UniTask ApplyKnockbackEffect(MoveHighlightParams @params)
        {
            foreach (var fn in _rammedKnockbackHighlighterFn)
            {
                await fn.Apply(@params);
            }
        }
    }
}