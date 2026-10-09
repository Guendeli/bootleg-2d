using System;
using System.Collections.Generic;
using System.Linq;
using TurnBasedStrategyFramework.Common.Cells;
using TurnBasedStrategyFramework.Common.Units;
using TurnBasedStrategyFramework.Common.Utilities;

namespace Bootleg.Tests
{
    /// <summary>Minimal ICell for rule tests that run without a scene.</summary>
    internal class TestCell : ICell
    {
        public TestCell(int x, int y)
        {
            GridCoordinates = new Vector2IntImpl(x, y);
        }

        public event Action<ICell> CellHighlighted { add { } remove { } }
        public event Action<ICell> CellDehighlighted { add { } remove { } }
        public event Action<ICell> CellClicked { add { } remove { } }
        public void InvokeCellHighlighted() { }
        public void InvokeCellDehighlighted() { }
        public void InvokeCellClicked() { }

        public Vector2IntImpl GridCoordinates { get; set; }
        public bool IsTaken { get; set; }
        public IList<IUnit> CurrentUnits { get; } = new List<IUnit>();
        public float MovementCost { get; set; } = 1;
        public Vector3Impl WorldPosition { get; set; }

        public int GetDistance(ICell otherCell) =>
            Math.Abs(GridCoordinates.x - otherCell.GridCoordinates.x) + Math.Abs(GridCoordinates.y - otherCell.GridCoordinates.y);

        public IEnumerable<ICell> GetNeighbours(ICellManager cellManager) => Enumerable.Empty<ICell>();

        public bool Equals(ICell other) => other != null && GridCoordinates.Equals(other.GridCoordinates);

        public override string ToString() => $"({GridCoordinates.x}, {GridCoordinates.y})";
    }
}
