using System.Collections.Generic;
using Bootleg.Units.Stats;
using UnityEngine;

namespace Bootleg.Units
{
    /// <summary>
    /// Read-only template for a kind of unit. Never write runtime state here: one asset is shared
    /// by every unit that uses it, and play-mode edits persist on the asset in the editor.
    /// </summary>
    [CreateAssetMenu(fileName = "UnitDefinition", menuName = "Bootleg/Unit Definition")]
    public class UnitDefinition : ScriptableObject
    {
        [SerializeField] private string _displayName;

        [Header("Base stats")]
        [SerializeField, Min(1)] private float _maxHealth = 10;
        [SerializeField, Min(0)] private float _maxActionPoints = 1;
        [SerializeField, Min(0)] private float _maxMovementPoints = 5;
        [SerializeField, Min(0)] private int _attackRange = 1;
        [SerializeField, Min(0)] private int _attack = 1;
        [SerializeField, Min(0)] private int _defence = 1;
        [SerializeField, Min(0)] private int _kickPower = DefaultKickPower;
        [SerializeField, Range(0f, 1f)] private float _interception = DefaultInterception;

        public const int DefaultKickPower = 2;
        public const float DefaultInterception = 0.25f;

        public string DisplayName => string.IsNullOrEmpty(_displayName) ? name : _displayName;

        public UnitStats CreateStats()
        {
            return new UnitStats(new Dictionary<StatType, float>
            {
                { StatType.MaxHealth, _maxHealth },
                { StatType.MaxActionPoints, _maxActionPoints },
                { StatType.MaxMovementPoints, _maxMovementPoints },
                { StatType.AttackRange, _attackRange },
                { StatType.Attack, _attack },
                { StatType.Defence, _defence },
                { StatType.KickPower, _kickPower },
                { StatType.Interception, _interception },
            });
        }
    }
}
