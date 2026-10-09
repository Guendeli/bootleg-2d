using Cysharp.Threading.Tasks;

namespace TurnBasedStrategyFramework.Unity.Examples.ClashOfHeroes.Units
{
    /// <summary>
    /// Interface for applying heal ability visual effects.
    /// </summary>
    public interface IHealHighlighter
    {
        UniTask ApplyHealEffect();
    }
}