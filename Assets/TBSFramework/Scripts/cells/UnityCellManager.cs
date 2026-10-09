using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TurnBasedStrategyFramework.Common.Cells;
using TurnBasedStrategyFramework.Common.Controllers;
using TurnBasedStrategyFramework.Common.Utilities;
using UnityEngine;

namespace TurnBasedStrategyFramework.Unity.Cells
{
    /// <summary>
    /// Abstract, Unity-specific implementation of the cell manager, responsible for managing cells within a game grid and handling their visual states.
    /// </summary>
    public abstract class UnityCellManager : MonoBehaviour, ICellManager
    {
        public abstract event Action<ICell> CellAdded;
        public abstract event Action<ICell> CellRemoved;

        public abstract void Initialize(IGridController gridController);
        public abstract ICell GetCellAt(Vector2IntImpl coords);
        public abstract IEnumerable<ICell> GetCells();

        public abstract UniTask MarkAsPath(IEnumerable<ICell> cells, ICell originCell);
        public abstract UniTask MarkAsReachable(IEnumerable<ICell> cells);
        public abstract UniTask MarkAsReachable(ICell cell);
        public abstract UniTask MarkAsHighlighted(ICell cell);
        public abstract UniTask UnMarkAsHighlighted(ICell cell);
        public abstract UniTask UnMark(IEnumerable<ICell> cells);
        public abstract UniTask UnMark(ICell cell);
        public abstract void SetColor(ICell cell, float r, float g, float b, float a);
    }
}