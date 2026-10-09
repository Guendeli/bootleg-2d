using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TurnBasedStrategyFramework.Unity.Highlighters
{
    /// <summary>
    /// A highlighter that activates or deactivates a specified GameObject based on the provided activation status.
    /// </summary>
    public class GameObjectActivatorHighlighter : Highlighter
    {
        [SerializeField] private bool _activationStatus;
        [SerializeField] private GameObject _target;

        /// <summary>
        /// Delay in milliseconds
        /// </summary>
        [SerializeField] private float _delay;

        public override async UniTask Apply(IHighlightParams @params)
        {
            _target.SetActive(_activationStatus);
            if(_delay > 0 ) 
            {
                await UniTask.Delay(TimeSpan.FromMilliseconds(_delay));
            }
            await UniTask.CompletedTask;
        }
    }
}