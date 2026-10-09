using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TurnBasedStrategyFramework.Unity.Highlighters
{
    /// <summary>
    /// A highlighter that rotates the target towards a specified cardinal direction.
    /// </summary>
    public class DirectionRotationHighlighter : BaseRotationHighlighter
    {
        [SerializeField] private CardinalDirectionHelper.CardinalDirection _targetDirection;

        public override async UniTask Apply(IHighlightParams @params)
        {
            Vector3 directionToFace = CardinalDirectionHelper.GetDirectionVector(_targetDirection);
            await RotateTowards(directionToFace);
            await UniTask.Delay(_delay);
        }
    }
}