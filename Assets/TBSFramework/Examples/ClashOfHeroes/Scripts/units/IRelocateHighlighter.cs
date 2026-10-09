using Cysharp.Threading.Tasks;
using TurnBasedStrategyFramework.Unity.Units;

namespace TurnBasedStrategyFramework.Unity.Examples.ClashOfHeroes.Units
{
    /// <summary>
    /// Interface for applying relocate ability visual effects.
    /// </summary>
    public interface IRelocateHighlighter
    {
        UniTask ApplyRelocateEffect(MoveHighlightParams @params);
    }
}