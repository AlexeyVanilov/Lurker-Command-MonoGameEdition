using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace LurkerCommand.GameSystem
{
    public sealed class Team
    {
        private readonly List<Unit> _units = new(16);
        public int Moves { get; private set; }
        public readonly bool isPlayer;
        public float TimeLeft { get; set; }
        public readonly Color TeamColor;
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
        }

        public void RefreshTurn()
        {
            var span = CollectionsMarshal.AsSpan(_units);
            for (int i = 0; i < span.Length; i++)
            {
                Moves += span[i].Value;
            }

            TimeLeft = Moves * TeamManager.TimeMultiplier;
        }

        public void ConsumeMove() => Moves = Math.Max(0, Moves - 1);

        public ReadOnlySpan<Unit> GetUnits() => CollectionsMarshal.AsSpan(_units);
    }
}