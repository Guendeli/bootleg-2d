using System;
using System.Collections.Generic;
using Bootleg.Units.Stats;
using TurnBasedStrategyFramework.Common.Cells;
using TurnBasedStrategyFramework.Common.Controllers;
using TurnBasedStrategyFramework.Common.Units;
using TurnBasedStrategyFramework.Unity.Units;
using UnityEngine;

namespace Bootleg.Units
{
    /// <summary>
    /// TBSF unit whose stats come from a <see cref="UnitDefinition"/> plus runtime modifiers,
    /// instead of the stat fields serialized on the prefab.
    /// </summary>
    public class TacticsUnit : Unit
    {
        [SerializeField] private UnitDefinition _definition;

        public UnitDefinition Definition => _definition;

        /// <summary>Available once the unit has been initialized by the grid controller.</summary>
        public UnitStats Stats { get; private set; }

        /// <summary>
        /// Assigns the definition for a unit spawned at runtime. Call before the unit is added to the unit manager.
        /// </summary>
        public void SetDefinition(UnitDefinition definition)
        {
            if (Stats != null)
            {
                throw new InvalidOperationException($"{name}: definition must be set before the unit is initialized.");
            }
            _definition = definition;
        }

        public override void Initialize(IGridController gridController)
        {
            Stats = _definition != null ? _definition.CreateStats() : CreateStatsFromSerializedFields();

            // Unit.Initialize copies the current Health / AP / MP into their Max values,
            // so start the unit at full values before calling it.
            Health = Stats.Get(StatType.MaxHealth);
            ActionPoints = Stats.Get(StatType.MaxActionPoints);
            MovementPoints = Stats.Get(StatType.MaxMovementPoints);
            ApplyCombatStats();

            base.Initialize(gridController);

            Stats.Changed += OnStatsChanged;
        }

        public override void Cleanup(IGridController gridController)
        {
            if (Stats != null)
            {
                Stats.Changed -= OnStatsChanged;
            }
            base.Cleanup(gridController);
        }

        public override bool IsUnitAttackable(IUnit otherUnit, ICell otherUnitCell, ICell attackSourceCell)
        {
            return otherUnit.PlayerNumber != Teams.Neutral && base.IsUnitAttackable(otherUnit, otherUnitCell, attackSourceCell);
        }

        private void OnStatsChanged(UnitStats stats)
        {
            ApplyCombatStats();

            MaxActionPoints = stats.Get(StatType.MaxActionPoints);
            ActionPoints = Mathf.Min(ActionPoints, MaxActionPoints);

            MaxMovementPoints = stats.Get(StatType.MaxMovementPoints);
            MovementPoints = Mathf.Min(MovementPoints, MaxMovementPoints);

            // Keep current health, clamped to the new max. Raise HealthChanged even when health
            // itself is unchanged so health bars pick up the new max.
            var previousHealth = Health;
            MaxHealth = stats.Get(StatType.MaxHealth);
            Health = Mathf.Min(Health, MaxHealth);
            InvokeHealthChanged(new HealthChangedEventArgs(this, null, Health - previousHealth));
        }

        private void ApplyCombatStats()
        {
            AttackRange = Stats.GetInt(StatType.AttackRange);
            AttackFactor = Stats.GetInt(StatType.Attack);
            DefenceFactor = Stats.GetInt(StatType.Defence);
        }

        /// <summary>Fallback so a plain Unit prefab switched to TacticsUnit keeps its inspector values.</summary>
        private UnitStats CreateStatsFromSerializedFields()
        {
            return new UnitStats(new Dictionary<StatType, float>
            {
                { StatType.MaxHealth, Health },
                { StatType.MaxActionPoints, ActionPoints },
                { StatType.MaxMovementPoints, MovementPoints },
                { StatType.AttackRange, AttackRange },
                { StatType.Attack, AttackFactor },
                { StatType.Defence, DefenceFactor },
                { StatType.KickPower, UnitDefinition.DefaultKickPower },
            });
        }
    }
}
