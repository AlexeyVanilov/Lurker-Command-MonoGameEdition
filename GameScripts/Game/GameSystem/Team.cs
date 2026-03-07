using GameEngine.Systems;
using LurkerCommand.MapSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace LurkerCommand.GameSystem
{
    public sealed class Team
    {
        private readonly List<Unit> _units = new(32);
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
            var span = GetUnits();
            for (int i = 0; i < span.Length; i++) {
                Moves += span[i].Value;
            }

            TimeLeft = Moves * TeamManager.TimeMultiplier;
        }
        public void MergeUnit(Unit baseUnit, Unit refUnit) {
            if (baseUnit == refUnit) return;
            baseUnit.Value += (sbyte)(refUnit.Value - 1);
            baseUnit.Moves = Math.Max(baseUnit.Moves, refUnit.Moves);
            
            PoolManager.Return(refUnit);

            Field.UpdateTeamVisibility(GetUnits());
            Moves--;
        }
        public void SplitUnit(Unit baseUnit)
        {
            if (baseUnit.Value <= 1) return;

            sbyte oldValue = baseUnit.Value;
            sbyte newValue = (sbyte)(oldValue / 2);

            Point spawnPos = FindEmptyNeighbor(baseUnit.gridPosition);

            if (spawnPos == new Point(-1, -1)) return;

            baseUnit.Value = (sbyte)(oldValue - newValue);
            baseUnit.UpdateText();

            Unit unitClone = PoolManager.Get<Unit>();

            SpriteFont font = baseUnit.valueText.Font;

            unitClone.Setup(font, spawnPos, newValue);
            unitClone.SetTeam(this);
            AddUnit(unitClone);

            Field.UpdateTeamVisibility(GetUnits());
            Moves--;
        }

        private Point FindEmptyNeighbor(Point pos)
        {
            Point[] dirs = { new(1, 0), new(-1, 0), new(0, 1), new(0, -1) };
            foreach (var d in dirs)
            {
                Point np = new Point(pos.X + d.X, pos.Y + d.Y);
                Cell c = Field.GetCell(np);
                if (c != null && c.IsEmpty) return np;
            }
            return new Point(-1, -1);
        }
        public void SkipMove() {
            TimeLeft = 0f;
            TeamManager.NextTurn();
        }
        public void ConsumeMove() => Moves = Math.Max(0, Moves - 1);

        public ReadOnlySpan<Unit> GetUnits() => CollectionsMarshal.AsSpan(_units);
    }
}