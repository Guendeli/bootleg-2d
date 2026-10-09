using System.Linq;
using Bootleg.Units;
using TurnBasedStrategyFramework.Common.Controllers.GameResolvers;
using TurnBasedStrategyFramework.Common.Units;
using TurnBasedStrategyFramework.Unity.Controllers;
using TurnBasedStrategyFramework.Unity.Players;
using TurnBasedStrategyFramework.Unity.Units;
using UnityEngine;

namespace Bootleg.Match
{
    /// <summary>
    /// Ends the game when only one team has units left. Same rule as TBSF's DominationVictoryCondition,
    /// but ignores neutral units such as the ball, which would otherwise count as a surviving team.
    /// </summary>
    public class TeamEliminationCondition : MonoBehaviour
    {
        [SerializeField] private UnityUnitManager _unitManager;
        [SerializeField] private UnityPlayerManager _playerManager;
        [SerializeField] private UnityGridController _gridController;

        private void Awake()
        {
            _unitManager.UnitRemoved += OnUnitRemoved;
        }

        private void OnDestroy()
        {
            if (_unitManager != null)
            {
                _unitManager.UnitRemoved -= OnUnitRemoved;
            }
        }

        private void OnUnitRemoved(IUnit unit)
        {
            var teamsAlive = _unitManager.GetUnits()
                .Select(u => u.PlayerNumber)
                .Where(n => n != Teams.Neutral)
                .Distinct()
                .ToList();
            if (teamsAlive.Count != 1)
            {
                return;
            }

            var players = _playerManager.GetPlayers().ToList();
            var winner = players.FirstOrDefault(p => p.PlayerNumber == teamsAlive[0]);
            if (winner == null)
            {
                return;
            }

            _gridController.InvokeGameEnded(new GameResult(winner, players.Where(p => p != winner)));
        }
    }
}
