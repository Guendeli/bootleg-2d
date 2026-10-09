using Cysharp.Threading.Tasks;
using TurnBasedStrategyFramework.Unity.Units;

namespace TurnBasedStrategyFramework.Unity.Examples.ClashOfHeroes.Units
{
    /// <summary>
    /// Interface for applying charge ability visual effects.
    /// </summary>
    public interface IRammedHighlighter
    {
        UniTask ApplyDamageEffect();
        UniTask ApplyKnockbackEffect(MoveHighlightParams @params);
    }
}