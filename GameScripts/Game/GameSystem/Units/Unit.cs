using GameEngine.Components.UI;
using GameEngine.Services;
using GameEngine.Systems;
using LurkerCommand.MapSystem;
using LurkerCommand.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace LurkerCommand.GameSystem
{
    public sealed class Unit : Entity, IGrid, IDraggable, IRect, IPoolable
    {
        private UnitStats _stats;
        public Team team { get; private set; }
        private bool isPlayer;
        private const float draggingColorMultiplier = 0.6f;
        public Text valueText;
        public sbyte Value
        {
            get => _stats.value;
            set
            {
                _stats.value = value;
                if (value > UnitStats.maxValue) {
                    _stats.value = 1;
                }
                UpdateText();
            }
        }
        public sbyte Moves {
            get => _stats.moves;
            set {
                _stats.moves = (sbyte)MathHelper.Clamp(value, 0, UnitStats.maxMoves);
            }
        }
        public Action onMoved;
        public Point gridPosition { get; set; }
        public bool IsInPool { get; set; }

        public Cell currentCell;
        public bool isVisible = true;

        private Unit unitClone;
        private const byte splitMergeRange = 1;
        public Unit() : base(Vector2.Zero, Vector2.One)
        {
            OrderInLayer = 2;
        }

        private void ForceBind(Cell cell)
        {
            currentCell?.Unbind();
            currentCell = cell;
            cell.BindUnit(this);
            gridPosition = cell.gridPosition;

            MoveTo(cell);
        }
        public void SetTeam(Team team) {
            this.team = team;
            valueText.Color = team.TeamColor;
            isPlayer = team.isPlayer;
            GetVision();
        }
        public void BindCell(Cell cell)
        {
            if (currentCell == cell && !cell.IsEmpty) return;
            cell.BindUnit(this);
            
            currentCell?.Unbind();
            currentCell = cell;
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
            if (CanMove()) {
                BindCell(cell);
                MoveTo(cell);

                Value -= steps;
                Moves -= steps;

                gridPosition = cell.gridPosition;

                GetVision();

                onMoved?.Invoke();
            }
            else
            {
                MoveTo(currentCell);
            }
        }
        public void Setup(SpriteFont font, Point startPoint, sbyte initialValue)
        {
            gridPosition = startPoint;

            if (valueText == null)
            {
                valueText = new Text(font, "", Vector2.Zero);
                valueText.Transform.Parent = Transform;
                valueText.OrderInLayer = OrderInLayer + 1;
            }
            else
            {
                valueText.Font = font;
            }

            Value = initialValue;
            Moves = initialValue;

            Cell bindedCell = Field.GetCell(startPoint);
            if (bindedCell != null) ForceBind(bindedCell);

            OnSpawn();
        }
        public bool CanMove() => CanControl() && Value > 1 && Moves > 0;
        public bool CanControl() => isPlayer;
        private void MoveTo(Cell cell) {
            Transform.LocalPosition = cell.Transform.LocalPosition + cell.cellImage.GetSize().ToVector2() * 0.5f;
        }
        public void UpdateText() {
            valueText.text = Value.ToString();
        }

        public void OnDragStartLBM() {
            if (!CanMove()) return;
            valueText.Color *= draggingColorMultiplier;
            Field.ToggleMoveNotes(currentCell, true, Value);
        }

        public void OnDragUpdateLBM(Vector2 position) {
            Transform.LocalPosition = position;
        }
        public void OnDragEndLBM()
        {
            valueText.Color = team.TeamColor;
            Field.ToggleMoveNotes(currentCell, false, Moves);

            Cell target = Field.GetCellByWorldPos(Transform.LocalPosition);

            if (target != null && target != currentCell)
            {
                int dist = Math.Abs(target.gridPosition.X - currentCell.gridPosition.X) +
                           Math.Abs(target.gridPosition.Y - currentCell.gridPosition.Y);

                if (dist <= Moves)
                {
                    if (!target.IsEmpty && target.currentUnit.team == team && dist == 1)
                    {
                        bool val = team.MergeUnit(target.currentUnit, this);
                        if (!val) {
                            MoveTo(currentCell);
                        }
                        return;
                    }

                    if (target.IsEmpty)
                    {
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

            sbyte taken = (sbyte)(Value / 2);
            sbyte left = (sbyte)(Value - taken);

            unitClone.Setup(valueText.Font, gridPosition, taken);
            unitClone.SetTeam(team);

            Field.ToggleMoveNotes(currentCell, true, splitMergeRange);
        }
        public void OnDragUpdateRBM(Vector2 position) {
            unitClone.Transform.LocalPosition = position;
        }
        public void OnDragEndRBM()
        {
            Field.ToggleMoveNotes(currentCell, false, splitMergeRange);
            Cell targetCell = Field.GetCellByWorldPos(unitClone.Transform.LocalPosition);

            if (targetCell != null && targetCell != currentCell)
            {
                int dist = Math.Abs(targetCell.gridPosition.X - currentCell.gridPosition.X) +
                           Math.Abs(targetCell.gridPosition.Y - currentCell.gridPosition.Y);

                if (dist <= splitMergeRange)
                {
                    bool val = team.SplitUnit(this, targetCell);
                    if (!val) {
                        PoolManager.Return(unitClone);
                        return;
                    }
                    sbyte taken = unitClone.Value;
                    Value -= taken;
                    
                }
            }

            PoolManager.Return(unitClone);
        }
        public void OnSpawn() {
            IsActive = true;
        }

        public void OnDespawn() {
            team = null;
            currentCell?.Unbind();
            currentCell = null;
            IsActive = false;
            onMoved = null;
        }
    }
}