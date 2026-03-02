using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace GameEngine.Components.Core {
    public sealed class Camera2D
    {
        public readonly GraphicsDevice graphics;
        private Vector2 _position;
        private float _zoom = 1f;
        private float _rotation = 0f;
        private Matrix _transform;
        private bool _isDirty = true;

        public float MinZoom { get; set; } = 0.5f;
        public float MaxZoom { get; set; } = 2.0f;

        public Vector2 Position
        {
            get => _position;
            set { _position = value; _isDirty = true; }
        }

        public float Zoom
        {
            get => _zoom;
            set
            {
                _zoom = MathHelper.Clamp(value, MinZoom, MaxZoom);
                _isDirty = true;
            }
        }

        public Camera2D(GraphicsDevice graphics)
        {
            this.graphics = graphics;
        }

        public Matrix GetViewMatrix()
        {
            if (!_isDirty) return _transform;

            _transform = Matrix.CreateTranslation(new Vector3(-_position, 0)) *

                         Matrix.CreateRotationZ(_rotation) *

                         Matrix.CreateScale(_zoom, _zoom, 1) *

                         Matrix.CreateTranslation(new Vector3(

                             graphics.Viewport.Width * 0.5f,

                             graphics.Viewport.Height * 0.5f, 0));


            _isDirty = false;

            return _transform;

        }
        public Vector2 ScreenToWorld(Vector2 screenPos) => Vector2.Transform(screenPos, Matrix.Invert(GetViewMatrix()));
    }
}