using GameEngine.Systems;
using GameEngine.Services;
using LurkerCommand.MapSystem;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace LurkerCommand.GameSystem
{
    public sealed class Team
    {
        private readonly List<Unit> _units = new(32);
        public event Action onTurnPast;
        public event Action<Unit> onUnitAdded;
        public event Action<Unit> onUnitRemoved;
        public int Moves { get; private set; }
        public bool isTurn = false;
        public readonly bool isPlayer;
        public float TimeLeft { get; set; }
        public readonly Color TeamColor;
        public string Name { get; set; }
        public bool IsActive { get; set; }

        public Team(Color color, bool isPlayer)
        {
            TeamColor = color;
            this.isPlayer = isPlayer;
        }

        public void AddUnit(Unit unit)
        {
            unit.SetTeam(this);
            _units.Add(unit);
            onUnitAdded?.Invoke(unit);
        }

        public void RemoveUnit(Unit unit)
        {
            unit.SetTeam(null);
            _units.Remove(unit);
            onUnitRemoved?.Invoke(unit);
        }

        public void RefreshTurn()
        {
            isTurn = true;
            var span = GetUnits();
            for (int i = 0; i < span.Length; i++)
            {
                Moves += span[i].Moves;
                span[i].giveBonus = true;
            }
            TimeLeft = Moves * TeamManager.TimeMultiplier;
        }

        public bool MergeUnit(Unit baseUnit, Unit refUnit)
        {
            if (Moves <= 0) return false;
            baseUnit.Value += (sbyte)(refUnit.Value - 1);
            baseUnit.Moves += refUnit.Moves;
            PoolManager.Return(refUnit);
            Field.UpdateTeamVisibility(this);
            ConsumeMove();
            return true;
        }

        public bool SplitUnit(Unit baseUnit, Cell cell)
        {
            if (Moves <= 0 || baseUnit.Value < 2) return false;
            sbyte taken = (sbyte)(baseUnit.Value / 2);

            if (!cell.IsEmpty && cell.currentUnit.team == this)
            {
                cell.currentUnit.Value += taken;
                ConsumeMove();
                Field.UpdateTeamVisibility(this);
                return true;
            }

            Unit clone = PoolManager.Get<Unit>();
            clone.Setup(baseUnit.valueText.Font, cell.gridPosition, taken);
            clone.SetTeam(this);
            clone.MoveUnit(cell);
            _units.Add(clone);
            SceneManager.Add(clone);
            ConsumeMove();
            return true;
        }

        public void SkipMove()
        {
            isTurn = false;
            TimeLeft = 0f;
            Moves = 0;

            var span = GetUnits();
            for (int i = 0; i < span.Length; i++)
            {
                var unit = span[i];
                var cell = unit.currentCell;
                if (cell == null) continue;

                if (cell.OwnerTeam != this) cell.Capture(this);
                if (unit.giveBonus) unit.Value += (sbyte)cell.idleBonus;
                unit.Moves += unit.Value; 
            }

            onTurnPast?.Invoke();
        }

        public void ConsumeMove() => Moves = Math.Max(0, Moves - 1);
        public ReadOnlySpan<Unit> GetUnits() => CollectionsMarshal.AsSpan(_units);
    }
}