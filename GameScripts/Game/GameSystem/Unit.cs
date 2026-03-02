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
    public sealed class Unit : Entity, IGrid, IDraggable, IRect
    {
        private int value;
        private int maxMoves = 25;
        private int moves;
        private Team team;
        private const float draggingColorMultiplier = 0.6f;
        public Text valueText;
        public const int maxValue = 9;
        public int Value
        {
            get => value;
            set
            {
                this.value = value > maxValue ? 1 : value;
                UpdateText();
            }
        }
        public int Moves {
            get => moves;
            set {
                moves = MathHelper.Clamp(value, 0, maxMoves);
            }
        }
        public Action onMoved;
        public Point gridPosition { get; set; }
        public Cell currentCell;
        public bool isVisible;
        public Unit(SpriteFont font, Point gridPosition, int Value, Team team) : base(Vector2.Zero, Vector2.One)
        {
            valueText = new Text(font, "", Vector2.Zero);
            valueText.Transform.Parent = Transform;
            valueText.OrderInLayer = 1;

            this.Value = Value;
            Moves = Value;

            Cell bindedCell = Field.GetCell(gridPosition);
            MoveUnit(bindedCell);

            valueText.Color = team.teamColor;
            this.team = team;
        }
        public void SetTeam(Team team) {
            this.team = team;
            valueText.Color = team.teamColor;
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
            valueText.Draw(gameTime, sb);
        }
        public Rectangle GetBounds() => valueText.GetBounds();
        public void MoveUnit(Cell cell, int steps = 0) {
            if (CanMove()) {
                BindCell(cell);
                MoveTo(cell);
                Value -= steps;
                Moves -= steps;
                gridPosition = cell.gridPosition;
                Field.UpdateVisibility(this);
                onMoved?.Invoke();
            }
            else {
                MoveTo(currentCell);
            }
        }
        public bool CanMove() => Value > 1 && Moves > 0;
        private void MoveTo(Cell cell) {
            Transform.LocalPosition = cell.Transform.LocalPosition + cell.cellImage.GetSize().ToVector2() * 0.5f;
        }
        public void UpdateText() {
            valueText.text = value.ToString();
        }

        public void OnDragStart() {
            if (!CanMove()) return;
            valueText.Color *= draggingColorMultiplier;
            Field.ToggleMoveNotes(currentCell, true, Value);
        }

        public void OnDragUpdate(Vector2 position) {
            if (!CanMove()) return;
            Transform.LocalPosition = position;
        }

        public void OnDragEnd()
        {
            valueText.Color = team.teamColor;
            Field.ToggleMoveNotes(currentCell, false, Moves);

            Cell targetCell = Field.GetCellByWorldPos(Transform.LocalPosition);
            var availableCells = Field.GetAvailableCells(currentCell, Moves);

            if (targetCell != null && CanMove() && availableCells.Contains(targetCell))
            {
                int distance = Math.Abs(targetCell.gridPosition.X - currentCell.gridPosition.X) +
                               Math.Abs(targetCell.gridPosition.Y - currentCell.gridPosition.Y);

                MoveUnit(targetCell, distance);
            }
            else MoveTo(currentCell);
        }
    }
}