using GameEngine.Components.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace LurkerCommand.GameScripts.Engine.Components.UI
{
    public sealed class Button : Image {
        private ButtonState _lastMouseState;

        public Text text;
        public event Action onClicked;
        public readonly Color hoveredColor = Color.Gray;
        public readonly Color defaultColor;
        public Button(Texture2D texture, Vector2 position, Vector2 scale, Color color, 
            SpriteFont font = null, string text = "", float rotation = 0, bool isStatic = false) 
            : base(texture, position, scale, color, rotation, isStatic) 
        {
            if(font != null) {
                Vector2 center = new Vector2(GetBounds().X / 2, GetBounds().Y / 2);
                this.text = new Text(font, text, center, Color.White, isStatic);
                this.text.Transform.Parent = Transform;

                defaultColor = color;
            }
        }
        public override void Draw(GameTime gameTime, SpriteBatch sb) {
            base.Draw(gameTime, sb);
            text?.Draw(gameTime, sb);
        }
        public override void Update(GameTime gameTime)
        {
            MouseState mouse = Mouse.GetState();
            Point mousePosition = new Point(mouse.X, mouse.Y);
            bool isHovered = GetBounds().Contains(mousePosition);
            if(isHovered) {
                if (Color == defaultColor) return;
                Color = hoveredColor;
                if (mouse.LeftButton == ButtonState.Released && _lastMouseState == ButtonState.Pressed)
                {
                    onClicked?.Invoke();
                }
            }
            else {
                if (Color == defaultColor) return;
                Color = defaultColor;
            }
            _lastMouseState = mouse.LeftButton;
        }
    }
}
