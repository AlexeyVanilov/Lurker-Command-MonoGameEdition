using GameEngine.Components.UI;
using GameEngine.Services;
using GameEngine.Systems;
using LurkerCommand.MapSystem;
using LurkerCommand.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.CompilerServices;

namespace LurkerCommand.GameSystem
{
    public sealed class Unit : Entity, IGrid, IDraggable, IRect, IPoolable
    {
        private UnitStats _stats;
        public Team team { get; private set; }
        private bool isPlayer;
        private const float draggingColorMultiplier = 0.6f;
        public Text valueText;
        public int Value
        {
            get => _stats.value;
            set
            {
                _stats.value = value;
                if (_stats.value > UnitStats.maxValue) {
                    _stats.value = 1;
                }
                else if(_stats.value < 1) {
                    Kill(this);
                    return;
                }
                UpdateText();
            }
        }
        public int Moves
        {
            get => _stats.moves;
            set {
                _stats.moves = MathHelper.Clamp(value, 0, UnitStats.maxMoves);
                team?.ConsumeMove();
            }
        }
        public bool giveBonus = true;
        public Point gridPosition { get; set; }
        public bool IsInPool { get; set; }
        public Cell currentCell;
        public bool isVisible = true;
        private Unit unitClone;
        private const byte splitMergeRange = 1;
        public Unit() : base(Vector2.Zero, Vector2.One) => OrderInLayer = 2;
        private void ForceBind(Cell cell)
        {
            currentCell?.Unbind();
            currentCell = cell;
            cell.BindUnit(this);
            gridPosition = cell.gridPosition;
            MoveTo(cell);
        }

        public void SetTeam(Team team)
        {
            this.team = team;
            if(team != null) {
                valueText.Color = team.TeamColor;
                isPlayer = team.isPlayer;
                GetVision();
            }
        }

        public void BindCell(Cell cell)
        {
            if (currentCell == cell) return;
            currentCell?.Unbind();
            currentCell = cell;
            cell.BindUnit(this);
        }

        public override void Draw(GameTime gameTime, SpriteBatch sb)
        {
            if (!isPlayer && !isVisible) return;
            valueText?.Draw(gameTime, sb);
        }

        public Rectangle GetBounds() => valueText.GetBounds();
        public void GetVision() => Field.UpdateTeamVisibility(team);

        public void MoveUnit(Cell cell, sbyte steps = 0)
        {
            if (CanMove())
            {
                BindCell(cell);
                MoveTo(cell);
                Value -= steps;
                Moves -= steps;
                gridPosition = cell.gridPosition;
                GetVision();
                giveBonus = false;
            }
            else MoveTo(currentCell);
        }

        public void Setup(SpriteFont font, Point startPoint, sbyte initialValue)
        {
            gridPosition = startPoint;
            if (valueText == null)
            {
                valueText = new Text(font, "", Vector2.Zero) { OrderInLayer = OrderInLayer + 1 };
                valueText.Transform.Parent = Transform;
            }
            else valueText.Font = font;

            Value = initialValue;
            Moves = initialValue;
            giveBonus = true;

            Cell bindedCell = Field.GetCell(startPoint);
            if (bindedCell != null) ForceBind(bindedCell);
            IsActive = true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanMove() => Value > 1 && Moves > 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanControl() => team.isTurn && isPlayer;
        private void MoveTo(Cell cell) => Transform.LocalPosition = cell.Transform.LocalPosition + cell.cellImage.GetSize().ToVector2() * 0.5f;
        public void UpdateText() => valueText.text = StringCache.Get(Value);
        public void OnDragStartLBM()
        {
            if (!CanMove() || !CanControl()) return;
            valueText.Color *= draggingColorMultiplier;
            Field.ToggleMoveNotes(currentCell, true, Value);
        }
        public void OnDragUpdateLBM(Vector2 position)
        {
            if (CanControl()) Transform.LocalPosition = position;
        }
        public void OnDragEndLBM()
        {
            valueText.Color = team.TeamColor;
            Field.ToggleMoveNotes(currentCell, false, Value);
            var available = Field.GetAvailableCells(currentCell, Value);
            Cell target = Field.GetCellByWorldPos(Transform.LocalPosition);
            if (target != null && target != currentCell)
            {
                int dist = Math.Abs(target.gridPosition.X - currentCell.gridPosition.X) + Math.Abs(target.gridPosition.Y - currentCell.gridPosition.Y);
                if (dist <= Value)
                {
                    if (!target.IsEmpty)
                    {
                        if (target.currentUnit.team == team && dist == 1)
                        {
                            if (team.MergeUnit(target.currentUnit, this)) return;
                        }
                        else if (target.currentUnit.team != team && dist <= Value)
                        {
                            team.AttackUnit(this, target.currentUnit);
                            return;
                        }
                        MoveTo(currentCell);
                        return;
                    }
                    if (available.Contains(target)) {
                        MoveUnit(target, (sbyte)dist);
                        return;
                    }
                }
            }
            MoveTo(currentCell);
        }

        public void OnDragStartRBM()
        {
            if (!CanControl() || Value < 2) return;
            unitClone = PoolManager.Get<Unit>();
            unitClone.Setup(valueText.Font, gridPosition, (sbyte)(Value / 2));
            unitClone.SetTeam(team);
            Field.ToggleMoveNotes(currentCell, true, splitMergeRange);
        }

        public void OnDragUpdateRBM(Vector2 position) { 
            if(unitClone != null) unitClone.Transform.LocalPosition = position;
        }
        public void OnDragEndRBM()
        {
            Field.ToggleMoveNotes(currentCell, false, splitMergeRange);
            Cell targetCell = Field.GetCellByWorldPos(unitClone.Transform.LocalPosition);
            if (targetCell != null && targetCell != currentCell)
            {
                int dist = Math.Abs(targetCell.gridPosition.X - currentCell.gridPosition.X) + Math.Abs(targetCell.gridPosition.Y - currentCell.gridPosition.Y);
                if (dist <= splitMergeRange && team.SplitUnit(this, targetCell)) { Value -= unitClone.Value; }
            }
            Kill(unitClone);
        }
        public void Kill(Unit unit) {
            PoolManager.Return(unit);
        }
        public void OnSpawn() => IsActive = true;
        public void OnDespawn() {
            team?.RemoveUnit(this); 
            currentCell?.Unbind(); 
            currentCell = null; 
            IsActive = false; 
        }
    }
}