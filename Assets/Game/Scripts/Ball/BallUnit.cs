using System;
using System.Collections.Generic;
using Bootleg.Units;
using TurnBasedStrategyFramework.Common.Controllers;
using TurnBasedStrategyFramework.Common.Units;
using TurnBasedStrategyFramework.Unity.Units;
using UnityEngine;

namespace Bootleg.Ball
{
    /// <summary>
    /// The ball: a neutral TBSF unit. It never gets a turn, has no abilities and cannot be damaged.
    /// A unit that ends a move on the loose ball's cell becomes its carrier; the ball then shares the carrier's cell
    /// and moves with it. If the carrier is removed from the game, the ball drops loose on its cell.
    /// </summary>
    public class BallUnit : Unit
    {
        [Tooltip("Where the ball sits relative to its carrier, so it stays visible at the carrier's feet.")]
        [SerializeField] private Vector3 _carriedOffset = new Vector3(0.35f, 0f, 0f);

        private readonly HashSet<IUnit> _trackedUnits = new HashSet<IUnit>();
        private Transform _carrierTransform;

        public IUnit Carrier { get; private set; }
        public bool IsLoose => Carrier == null;

        public event Action<BallUnit> CarrierChanged;

        public override void Initialize(IGridController gridController)
        {
            PlayerNumber = Teams.Neutral;
            base.Initialize(gridController);

            foreach (var unit in gridController.UnitManager.GetUnits())
            {
                Track(unit);
            }
            gridController.UnitManager.UnitAdded += Track;
            gridController.UnitManager.UnitRemoved += OnUnitRemoved;
        }

        public override void ModifyHealth(float healthChangeAmount, IUnit sourceUnit)
        {
        }

        private void Track(IUnit unit)
        {
            if (!ReferenceEquals(unit, this) && _trackedUnits.Add(unit))
            {
                unit.UnitMoved += OnUnitMoved;
            }
        }

        private void OnUnitRemoved(IUnit unit)
        {
            if (_trackedUnits.Remove(unit))
            {
                unit.UnitMoved -= OnUnitMoved;
            }
            if (ReferenceEquals(unit, Carrier))
            {
                SetCarrier(null);
            }
        }

        private void OnUnitMoved(UnitMovedEventArgs args)
        {
            if (ReferenceEquals(args.AffectedUnit, Carrier))
            {
                CurrentCell.CurrentUnits.Remove(this);
                CurrentCell = args.TargetCell;
                CurrentCell.CurrentUnits.Add(this);
            }
            else if (IsLoose && args.TargetCell.Equals(CurrentCell))
            {
                SetCarrier(args.AffectedUnit);
            }
        }

        private void SetCarrier(IUnit carrier)
        {
            Carrier = carrier;
            _carrierTransform = (carrier as Component)?.transform;

            // While carried, clicks on the shared cell must reach the carrier, not the ball.
            foreach (var collider in GetComponentsInChildren<Collider>())
            {
                collider.enabled = IsLoose;
            }
            foreach (var collider in GetComponentsInChildren<Collider2D>())
            {
                collider.enabled = IsLoose;
            }

            if (IsLoose)
            {
                WorldPosition = CurrentCell.WorldPosition;
            }
            CarrierChanged?.Invoke(this);
        }

        private void LateUpdate()
        {
            if (Application.isPlaying && _carrierTransform != null)
            {
                transform.position = _carrierTransform.position + _carriedOffset;
            }
        }

        // Hides Unit.Reset, which adds attack/move abilities and an AI brain when the component is added.
        private void Reset()
        {
        }
    }
}
