using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GameEngine.Services
{
    public static class InputManager
    {
        private static KeyboardState _currentKeyEntry;
        private static KeyboardState _prevKeyEntry;
        private static MouseState _currentMouseEntry;
        private static MouseState _prevMouseEntry;

        public static int ScrollDelta => _currentMouseEntry.ScrollWheelValue - _prevMouseEntry.ScrollWheelValue;
        public static Vector2 MousePosition => new Vector2(_currentMouseEntry.X, _currentMouseEntry.Y);
        public static Vector2 MouseDelta => new Vector2(_currentMouseEntry.X - _prevMouseEntry.X, _currentMouseEntry.Y - _prevMouseEntry.Y);

        public static void Update()
        {
            _prevKeyEntry = _currentKeyEntry;
            _prevMouseEntry = _currentMouseEntry;

            _currentKeyEntry = Keyboard.GetState();
            _currentMouseEntry = Mouse.GetState();
        }

        public static bool IsKeyDown(Keys key) => _currentKeyEntry.IsKeyDown(key);

        public static bool IsKeyPressed(Keys key) => _currentKeyEntry.IsKeyDown(key) && _prevKeyEntry.IsKeyUp(key);

        public static bool IsMouseButtonDown(MouseButton button) => button switch
        {
            MouseButton.Left => _currentMouseEntry.LeftButton == ButtonState.Pressed,
            MouseButton.Right => _currentMouseEntry.RightButton == ButtonState.Pressed,
            MouseButton.Middle => _currentMouseEntry.MiddleButton == ButtonState.Pressed,
            _ => false
        };

        public static bool IsMouseButtonPressed(MouseButton button) => button switch
        {
            MouseButton.Left => _currentMouseEntry.LeftButton == ButtonState.Pressed && _prevMouseEntry.LeftButton == ButtonState.Released,
            MouseButton.Right => _currentMouseEntry.RightButton == ButtonState.Pressed && _prevMouseEntry.RightButton == ButtonState.Released,
            MouseButton.Middle => _currentMouseEntry.MiddleButton == ButtonState.Pressed && _prevMouseEntry.MiddleButton == ButtonState.Released,
            _ => false
        };
    }

    public enum MouseButton { Left, Right, Middle }
}