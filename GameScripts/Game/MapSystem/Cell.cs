using GameEngine.Components.UI;
using GameEngine.Services;
using GameEngine.Systems;
using LurkerCommand.GameSystem;
using LurkerCommand.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LurkerCommand.MapSystem
{
    public sealed class Cell : Entity, IGrid {
        public Point gridPosition { get; set; }
        public Image cellImage;
        public readonly Color defaultColor = Color.White;
        public readonly Color hiddenColor = new Color(175, 175, 175, 255);
        private const float moveNoteOffset = 0.25f;
        private Unit currentUnit = null;
        private bool isVisible = false;
        private bool isEmpty = true;
        private const string dot = "·";
        public bool IsVisible {
            get => isVisible;
            set {
                if (isVisible == value) return;
                isVisible = value;

                cellImage.Color = value ? defaultColor : hiddenColor;
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
        private Text moveNote;
        public Cell(Texture2D texture, Vector2 position, Vector2 scale) : base(position, scale, 0f, true)
        {
            cellImage = new Image(texture, Vector2.Zero, scale, new Color(175, 175, 175, 255));
            cellImage.Transform.Parent = Transform;
            cellImage.OrderInLayer = 0;
            Init();
        }

        private void Init()
        {
            moveNote = new Text(AssetManager.GetFont("Arial"), dot, Vector2.Zero, Color.LightGreen, true);
            moveNote.Transform.Parent = Transform;
            moveNote.Transform.LocalScale = Transform.LocalScale;
            Vector2 cellSize = cellImage.GetSize().ToVector2();
            Vector2 center = cellSize * moveNoteOffset;
            moveNote.Transform.LocalPosition = center;

            Toggle(false);
        }
        public void BindUnit(Unit unit) {
            currentUnit = unit;
            IsEmpty = false;
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
