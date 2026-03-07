using GameEngine.Components.UI;
using GameEngine.Services;
using GameEngine.Systems;
using LurkerCommand.GameScripts.Game.MapSystem.Cells;
using LurkerCommand.GameSystem;
using LurkerCommand.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LurkerCommand.MapSystem
{
    public class Cell : Entity, IGrid {
        public virtual bool canCaptured { get; set; } = true;
        public virtual CellType cellType { get; set; } = CellType.DefaultCell;
        public virtual Color defaultColor { get; set; } = Color.White;
        public virtual Color hiddenColor { get; set; } = new Color(colorHiddenEffect, colorHiddenEffect, colorHiddenEffect);
        public Point gridPosition { get; set; }
        public Image cellImage;
        public const byte colorHiddenEffect = 175;
        public Unit currentUnit = null;
        
        private bool isVisible = false;
        private bool isEmpty = true;
        private const string dot = "·";
        public bool IsVisible {
            get => isVisible;
            set {
                if (isVisible == value) return;
                isVisible = value;

                cellImage.Color = value ? defaultColor : hiddenColor;
                currentUnit?.isVisible = value;
            }
        }
        public override void Draw(GameTime gameTime, SpriteBatch sb) {
            cellImage.Draw(gameTime, sb);
            if (moveNote.IsActive) {
                moveNote.Draw(gameTime, sb);
            }
        }
        public bool IsEmpty {
            get => isEmpty;
            set => isEmpty = value;
        }
        protected Text moveNote;
        public Cell(Texture2D texture, Vector2 position, Vector2 scale) : base(position, scale, 0f, true)
        {
            cellImage = new Image(texture, Vector2.Zero, scale, new Color(175, 175, 175, 255));
            cellImage.Transform.Parent = Transform;
            cellImage.OrderInLayer = 0;
            Init();
        }

        protected void Init()
        {
            moveNote = new Text(AssetManager.GetFont("Arial"), dot, Vector2.Zero, Color.LightGreen, true);
            moveNote.Transform.Parent = Transform;
            moveNote.Transform.LocalScale = Transform.LocalScale;
            Vector2 cellSize = cellImage.GetSize().ToVector2();
            Vector2 center = cellSize / 2;
            moveNote.Transform.LocalPosition = center;

            Toggle(false);
        }
        public void BindUnit(Unit unit)
        {
            currentUnit = unit;
            IsEmpty = false;
            if (unit != null) unit.isVisible = this.IsVisible;
        }
        public void Unbind() {
            currentUnit = null;
            IsEmpty = true;
        }
        public void Toggle(bool toggle) {
            moveNote.IsActive = toggle;
        }
        public void Toggle() {
            moveNote.IsActive = !moveNote.IsActive;
        }
    }
}
