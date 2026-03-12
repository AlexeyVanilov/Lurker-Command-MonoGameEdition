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
            UnitSystem.SetTeam(unit, this);
            _units.Add(unit);
            onUnitAdded?.Invoke(unit);
        }

        public void RemoveUnit(Unit unit)
        {
            UnitSystem.SetTeam(unit, null);
            _units.Remove(unit);
            onUnitRemoved?.Invoke(unit);
        }
        public void RefreshTurn()
        {
            isTurn = true;
            var span = GetUnits();
            for (int i = 0; i < span.Length; i++)
            {
                var unit = span[i];
                Moves += unit.Moves;
                unit.giveBonus = true;
                var cell = unit.currentCell;
                if (cell == null) continue;

                if (cell.OwnerTeam != this) cell.Capture(this);
                if (unit.giveBonus) unit.Value += (sbyte)cell.idleBonus;
                unit.Moves += unit.Value;
            }
            TimeLeft = Moves * TeamManager.TimeMultiplier;
        }

        public void SkipMove()
        {
            isTurn = false;
            TimeLeft = 0f;
            Moves = 0;
            onTurnPast?.Invoke();
        }

        public void ConsumeMove() => Moves = Math.Max(0, Moves - 1);
        public ReadOnlySpan<Unit> GetUnits() => CollectionsMarshal.AsSpan(_units);
    }
}