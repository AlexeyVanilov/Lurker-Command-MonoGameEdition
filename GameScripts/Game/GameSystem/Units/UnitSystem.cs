using GameEngine.Services;
using GameEngine.Systems;
using LurkerCommand.MapSystem;
using System;
using System.Runtime.CompilerServices;

namespace LurkerCommand.GameSystem {
    public static class UnitSystem {
        public const byte splitMergeRange = 1;
        public const float draggingColorMultiplier = 0.6f;

        public static void Kill(Unit unit) {
            PoolManager.Return(unit);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetDistance(Cell currentCell, Cell targetCell) => 
            Math.Abs(targetCell.gridPosition.X - currentCell.gridPosition.X) + Math.Abs(targetCell.gridPosition.Y - currentCell.gridPosition.Y);
        public static void AttackUnit(Unit ally, Unit enemy)
        {
            int aVal = ally.Value;
            int eVal = enemy.Value;

            if (aVal > eVal)
            {
                enemy.Value = 0;
                ally.Value = aVal + eVal;
            }
            else if (aVal < eVal)
            {
                ally.Value = 0;
            }
            else
            {
                ally.Value = 0;
                enemy.Value = 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CanMove(Unit unit) => unit.Value > 1 && unit.Moves > 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CanControl(Unit unit) => unit.team.isTurn && unit.isPlayer;
        public static void MoveTo(Unit unit, Cell cell) => unit.Transform.LocalPosition = cell.Transform.LocalPosition + cell.cellImage.GetSize().ToVector2() * 0.5f;
        public static void BindCell(Unit unit, Cell cell)
        {
            if (unit.currentCell == cell) return;
            unit.currentCell?.Unbind();
            unit.currentCell = cell;
            cell.BindUnit(unit);
        }
        public static void MoveUnit(Unit unit, Cell cell, int steps = 0)
        {
            if (CanMove(unit))
            {
                BindCell(unit, cell);
                MoveTo(unit, cell);
                unit.Value -= steps;
                unit.Moves -= steps;
                unit.gridPosition = cell.gridPosition;
                GetVision(unit);
                unit.giveBonus = false;
            }
            else MoveTo(unit, unit.currentCell);
        }
        public static void GetVision(Unit unit) => Field.UpdateTeamVisibility(unit.team);
        public static void SetTeam(Unit unit, Team team)
        {
            unit.team = team;
            if (team != null)
            {
                unit.valueText.Color = team.TeamColor;
                unit.isPlayer = team.isPlayer;
                GetVision(unit);
            }
        }
        public static void ForceBind(Unit unit, Cell cell)
        {
            unit.currentCell?.Unbind();
            unit.currentCell = cell;
            cell.BindUnit(unit);
            unit.gridPosition = cell.gridPosition;
            MoveTo(unit, cell);
        }
        public static Unit HandleInteraction(Unit unit) {
            Unit unitClone = PoolManager.Get<Unit>();
            unitClone.Setup(unit.valueText.Font, unit.gridPosition, unit.Value / 2);
            SetTeam(unitClone, unit.team);
            Field.ToggleMoveNotes(unit.currentCell, true, splitMergeRange);

            return unitClone;
        }
        public static void HandleDrop(Unit unit) {
            Field.ToggleMoveNotes(unit.currentCell, false, splitMergeRange);
            Cell targetCell = Field.GetCellByWorldPos(unit.unitClone.Transform.LocalPosition);
            if (targetCell != null && targetCell != unit.currentCell) {
                if (GetDistance(unit.currentCell, targetCell) <= splitMergeRange 
                    && SplitUnit(unit, targetCell)) 
                {
                    unit.Value -= unit.unitClone.Value; 
                }
            }
            Kill(unit.unitClone);
        }
        public static void UpdateText(Unit unit) => unit.valueText.text = StringCache.Get(unit.Value);
        public static bool MergeUnit(Unit baseUnit, Unit refUnit)
        {
            Team team = baseUnit.team;
            if (team.Moves <= 0) return false;
            baseUnit.Value += (sbyte)(refUnit.Value - 1);
            baseUnit.Moves += refUnit.Moves;
            PoolManager.Return(refUnit);
            Field.UpdateTeamVisibility(team);
            team.ConsumeMove();
            return true;
        }

        public static bool SplitUnit(Unit baseUnit, Cell cell)
        {
            Team team = baseUnit.team;
            if (team.Moves <= 0 || baseUnit.Value < 2) return false;
            sbyte taken = (sbyte)(baseUnit.Value / 2);

            if (!cell.IsEmpty && cell.currentUnit.team == team)
            {
                cell.currentUnit.Value += taken;
                team.ConsumeMove();
                Field.UpdateTeamVisibility(team);
                return true;
            }

            Unit clone = PoolManager.Get<Unit>();
            clone.Setup(baseUnit.valueText.Font, cell.gridPosition, taken);
            SetTeam(clone, team);
            MoveUnit(clone, cell);
            team.AddUnit(clone);
            SceneManager.Add(clone);
            team.ConsumeMove();
            return true;
        }
    }
}
