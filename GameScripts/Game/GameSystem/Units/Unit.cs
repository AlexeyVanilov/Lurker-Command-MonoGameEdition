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
        public Team team;
        public bool isPlayer;
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
                    UnitSystem.Kill(this);
                    return;
                }
                UnitSystem.UpdateText(this);
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
        public Unit unitClone;
        public Unit() : base(Vector2.Zero, Vector2.One) => OrderInLayer = 2;
        public override void Draw(GameTime gameTime, SpriteBatch sb)
        {
            if (!isPlayer && !isVisible) return;
            valueText?.Draw(gameTime, sb);
        }

        public Rectangle GetBounds() => valueText.GetBounds();

        public void Setup(SpriteFont font, Point startPoint, int initialValue)
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
            if (bindedCell != null) UnitSystem.ForceBind(this, bindedCell);
            IsActive = true;
        }
        public void OnDragStartLBM()
        {
            if (!UnitSystem.CanMove(this) || !UnitSystem.CanControl(this)) return;
            valueText.Color *= UnitSystem.draggingColorMultiplier;
            Field.ToggleMoveNotes(currentCell, true, Value);
        }
        public void OnDragUpdateLBM(Vector2 position)
        {
            if (UnitSystem.CanControl(this)) Transform.LocalPosition = position;
        }
        public void OnDragEndLBM()
        {
            valueText.Color = team.TeamColor;
            Field.ToggleMoveNotes(currentCell, false, Value);
            var available = Field.GetAvailableCells(currentCell, Value);
            Cell target = Field.GetCellByWorldPos(Transform.LocalPosition);
            if (target != null && target != currentCell)
            {
                int dist = UnitSystem.GetDistance(currentCell, target);
                if (dist <= Value)
                {
                    if (!target.IsEmpty)
                    {
                        if (target.currentUnit.team == team && dist == 1)
                        {
                            if (UnitSystem.MergeUnit(target.currentUnit, this)) return;
                        }
                        else if (target.currentUnit.team != team && dist <= Value)
                        {
                            UnitSystem.AttackUnit(this, target.currentUnit);
                            return;
                        }
                        UnitSystem.MoveTo(this, currentCell);
                        return;
                    }
                    if (available.Contains(target)) {
                        UnitSystem.MoveUnit(this, target, (sbyte)dist);
                        return;
                    }
                }
            }
            UnitSystem.MoveTo(this, currentCell);
        }

        public void OnDragStartRBM()
        {
            if (!UnitSystem.CanControl(this) || Value < 2) return;
            UnitSystem.HandleInteraction(this);
        }

        public void OnDragUpdateRBM(Vector2 position) { 
            if(unitClone != null) unitClone.Transform.LocalPosition = position;
        }
        public void OnDragEndRBM() => UnitSystem.HandleDrop(this);
        public void OnSpawn() => IsActive = true;
        public void OnDespawn() {
            team?.RemoveUnit(this); 
            currentCell?.Unbind(); 
            currentCell = null; 
            IsActive = false; 
        }
    }
}