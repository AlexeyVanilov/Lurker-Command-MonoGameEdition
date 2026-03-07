using GameEngine.Services;
using GameEngine.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GameEngine.Components.UI
{
    public sealed class Text : Entity, IRect
    {
        private string _text = string.Empty;
        private Vector2 _size;
        private Vector2 _origin;
        private Rectangle _cachedBounds;
        private bool _boundsDirty = true;

        public Color Color;
        public SpriteFont Font;

        public string text
        {
            get => _text;
            set
            {
                if (value == null) value = string.Empty;
                if (ReferenceEquals(_text, value) || _text == value) return;

                _text = value;
                UpdateMetrics();
            }
        }

        public Vector2 Origin { get => _origin; set { _origin = value; _boundsDirty = true; } }

        public Text(SpriteFont font, string text, Vector2 position, Color color = default, bool isStatic = false)
            : base(position, Vector2.One, 0f, isStatic)
        {
            Font = font ?? throw new ArgumentNullException(nameof(font));
            Color = color;
            _text = text ?? string.Empty;
            UpdateMetrics();
        }

        public void UpdateMetrics()
        {
            if (string.IsNullOrEmpty(_text))
            {
                _size = Vector2.Zero;
                _origin = Vector2.Zero;
            }
            else
            {
                _size = Font.MeasureString(_text);
                _origin = new Vector2(
                    (float)Math.Floor(_size.X * 0.5f),
                    (float)Math.Floor(_size.Y * 0.5f)
                );
            }
            _boundsDirty = true;
        }

        public Rectangle GetBounds()
        {
            if (_boundsDirty)
            {
                UpdateBounds();
            }
            return _cachedBounds;
        }

        public void UpdateBounds()
        {
            Vector2 worldPos = Transform.WorldPosition;
            Vector2 scale = Transform.LocalScale;

            _cachedBounds = new Rectangle(
                (int)(worldPos.X - _origin.X * scale.X),
                (int)(worldPos.Y - _origin.Y * scale.Y),
                (int)(_size.X * scale.X),
                (int)(_size.Y * scale.Y)
            );
            _boundsDirty = false;
        }
        public override void Draw(GameTime gameTime, SpriteBatch sb)
        {
            if (string.IsNullOrEmpty(_text)) return;

            sb.DrawString(Font, _text, Transform.WorldPosition, Color,
                Transform.LocalRotation, _origin, Transform.LocalScale, SpriteEffects.None, 0f);
        }
    }
}