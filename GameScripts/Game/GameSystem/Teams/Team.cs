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

        public void AddUnit(Unit unit) {
            unit.SetTeam(this);
            _units.Add(unit);
        }

        public void RefreshTurn() {
            var span = GetUnits();
            for (int i = 0; i < span.Length; i++) {
                Moves += span[i].Value;
            }

            TimeLeft = Moves * TeamManager.TimeMultiplier;
        }
        public bool MergeUnit(Unit baseUnit, Unit refUnit) {
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

            sbyte total = baseUnit.Value;
            sbyte taken = (sbyte)(total / 2);

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

            if (!SceneManager.CurrentScene.Contains(clone))
                SceneManager.Add(clone);

            ConsumeMove();
            return true;
        }
        public void SkipMove() {
            TimeLeft = 0f;
            TeamManager.NextTurn();
        }
        public void ConsumeMove() => Moves = Math.Max(0, Moves - 1);

        public ReadOnlySpan<Unit> GetUnits() => CollectionsMarshal.AsSpan(_units);
    }
}