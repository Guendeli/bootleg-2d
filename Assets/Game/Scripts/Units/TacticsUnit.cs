using System;
using System.Collections.Generic;
using System.Linq;
using Bootleg.Abilities;
using Bootleg.Ball;
using Bootleg.Units.Stats;
using Cysharp.Threading.Tasks;
using TurnBasedStrategyFramework.Common.Cells;
using TurnBasedStrategyFramework.Common.Controllers;
using TurnBasedStrategyFramework.Common.Controllers.GridStates;
using TurnBasedStrategyFramework.Common.Units;
using TurnBasedStrategyFramework.Common.Units.Abilities;
using TurnBasedStrategyFramework.Unity.Units;
using TurnBasedStrategyFramework.Unity.Units.Abilities;
using UnityEngine;

namespace Bootleg.Units
{
    /// <summary>
    /// One entry in the unit's action menu: a label and the abilities that are active while it is chosen.
    /// </summary>
    [Serializable]
    public class UnitAction
    {
        public string Label;
        public List<Ability> Abilities = new List<Ability>();
    }

    /// <summary>
    /// TBSF unit whose stats come from a <see cref="UnitDefinition"/> plus runtime modifiers,
    /// instead of the stat fields serialized on the prefab.
    /// Its abilities are grouped into <see cref="Actions"/>; only the chosen action's abilities react to input.
    /// </summary>
    public class TacticsUnit : Unit
    {
        [SerializeField] private UnitDefinition _definition;

        [Tooltip("Action menu entries. The first available one is chosen when the unit is selected. " +
                 "Empty = all ability components are active at once (plain TBSF behaviour).")]
        [SerializeField] private List<UnitAction> _actions = new List<UnitAction>();

        private IGridController _gridController;
        private int _chosenActionIndex;

        public IReadOnlyList<UnitAction> Actions => _actions;

        /// <summary>
        /// The action whose abilities are active: the one last chosen, or the first available action if the chosen
        /// one can no longer be performed (e.g. Move after all movement points are spent). -1 if there are no actions.
        /// </summary>
        public int ActiveActionIndex
        {
            get
            {
                if (_actions.Count == 0)
                {
                    return -1;
                }
                if (_gridController == null || CanPerform(_actions[_chosenActionIndex], _gridController))
                {
                    return _chosenActionIndex;
                }
                for (var i = 0; i < _actions.Count; i++)
                {
                    if (CanPerform(_actions[i], _gridController))
                    {
                        return i;
                    }
                }
                return _chosenActionIndex;
            }
        }

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

            _gridController = gridController;
            Stats.Changed += OnStatsChanged;
        }

        public override void Cleanup(IGridController gridController)
        {
            if (Stats != null)
            {
                Stats.Changed -= OnStatsChanged;
            }
            foreach (var ability in InactiveAbilities())
            {
                ability.OnUnitDestroyed(gridController);
            }

            var cell = CurrentCell;
            base.Cleanup(gridController);
            // Unit.Cleanup frees the cell, but a ball this unit carried stays on it.
            if (cell != null)
            {
                cell.IsTaken = cell.CurrentUnits.Count > 0;
            }
        }

        // TBSF selects units with GetBaseAbilities() everywhere (selection, re-selection after an action,
        // switching units), so returning only the chosen action's abilities is enough to drive the menu.
        public override IEnumerable<IAbility> GetBaseAbilities()
        {
            return _actions.Count > 0 ? _actions[ActiveActionIndex].Abilities : base.GetBaseAbilities();
        }

        public bool CanPerform(UnitAction action, IGridController gridController)
        {
            return action.Abilities.Any(a => a.CanPerform(gridController));
        }

        /// <summary>Switches the active action and re-selects the unit with its abilities.</summary>
        public void SelectAction(int index, IGridController gridController)
        {
            _chosenActionIndex = index;
            gridController.GridState = new GridStateUnitSelected(this, GetBaseAbilities());
        }

        /// <summary>Ends this unit's actions for the turn. Points are refilled at turn end as usual.</summary>
        public void Wait(IGridController gridController)
        {
            ActionPoints = 0;
            MovementPoints = 0;
            gridController.GridState = new GridStateAwaitInput();
            gridController.UnitManager.MarkAsFinished(new IUnit[] { this }).Forget();
        }

        // GridController only calls turn/destroy hooks on GetBaseAbilities(), i.e. the active action.
        // Forward them to the other abilities so none miss their lifecycle callbacks.
        public override void OnTurnStart(IGridController gridController)
        {
            _chosenActionIndex = 0;
            base.OnTurnStart(gridController);
            foreach (var ability in InactiveAbilities())
            {
                ability.OnTurnStart(gridController);
            }
        }

        public override void OnTurnEnd(IGridController gridController)
        {
            base.OnTurnEnd(gridController);
            foreach (var ability in InactiveAbilities())
            {
                ability.OnTurnEnd(gridController);
            }
        }

        private IEnumerable<IAbility> InactiveAbilities()
        {
            return _actions.Count > 0 ? base.GetBaseAbilities().Except(GetBaseAbilities()) : Enumerable.Empty<IAbility>();
        }

        // A loose ball's cell is taken, but field units may step onto it to take the ball.
        public override bool IsCellMovableTo(ICell cell)
        {
            return base.IsCellMovableTo(cell) || BallMoveRules.HoldsOnlyLooseBalls(cell);
        }

        // Zone of control for the ball carrier: cells next to an enemy can be entered but not passed through,
        // so the pathfinder routes around defenders where it can. Leaving the starting cell is always allowed.
        // Off-ball units move freely. Only set while CachePaths runs, which is the only time edges are evaluated.
        private HashSet<ICell> _enemyZoneCells;

        public override void CachePaths(ICellManager cellManager)
        {
            _enemyZoneCells = _gridController != null && BallInterception.FindCarriedBall(this, _gridController) != null
                ? BallInterception.EnemyZoneCells(this, cellManager)
                : null;
            try
            {
                base.CachePaths(cellManager);
            }
            finally
            {
                _enemyZoneCells = null;
            }
        }

        // Zone cells can be entered but have no outgoing edges, so they must still be in the graph for Dijkstra.
        public override Dictionary<ICell, Dictionary<ICell, float>> GetGraphEdges(ICellManager cellManager)
        {
            return PathGraph.AddDeadEnds(base.GetGraphEdges(cellManager));
        }

        public override bool IsCellTraversable(ICell source, ICell destination)
        {
            if (_enemyZoneCells != null && _enemyZoneCells.Contains(source) && !source.Equals(CurrentCell))
            {
                return false;
            }
            return base.IsCellTraversable(source, destination) || BallMoveRules.HoldsOnlyLooseBalls(destination);
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

        [ContextMenu("Auto-configure actions")]
        private void AutoConfigureActions()
        {
            var abilities = GetComponents<Ability>();
            _actions.Clear();
            var hasFieldMove = abilities.Any(a => a is FieldMoveAbility);
            AddAction("Move", abilities.Where(a => a is FieldMoveAbility || (!hasFieldMove && a is MoveAbility) || a is AttackRangeHighlightAbility));
            AddAction("Attack", abilities.Where(a => a is AttackAbility));
            AddAction("Push", abilities.Where(a => a is PushBallAbility));
            AddAction("Kick", abilities.Where(a => a is KickAbility));
            AddAction("Pass", abilities.Where(a => a is PassAbility));
            AddAction("Tackle", abilities.Where(a => a is TackleAbility));
            _chosenActionIndex = 0;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif

            void AddAction(string label, IEnumerable<Ability> actionAbilities)
            {
                var list = actionAbilities.ToList();
                if (list.Count > 0)
                {
                    _actions.Add(new UnitAction { Label = label, Abilities = list });
                }
            }
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
                { StatType.Interception, UnitDefinition.DefaultInterception },
            });
        }
    }
}
