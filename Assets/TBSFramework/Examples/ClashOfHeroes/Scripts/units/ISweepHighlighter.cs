using Cysharp.Threading.Tasks;
using TurnBasedStrategyFramework.Unity.Units;
using UnityEngine;

namespace TurnBasedStrategyFramework.Unity.Examples.ClashOfHeroes.Units
{
    /// <summary>
    /// Interface for applying sweep ability visual effects.
    /// </summary>
    public interface ISweepHighlighter
    {
        UniTask ApplySweepEffect(GameObject target, CombatHighlightParams @params);
    }
}