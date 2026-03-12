using GameEngine.Components.UI;
using GameEngine.Services;
using GameEngine.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace LurkerCommand.GameSystem {
    public static class GameUI {
        public static event Action<GameObject> onUIAdd;
        private static Text _timeText;
        private static Button _skipMoveButton;

        private const string timeLeftMessage = "Time Left: ";
        private const string secondMessage = "s";
        public static void UpdateTime(string team, int time)
        {
            if (time >= 30)
            {
                _timeText.Color = Color.Green;
            }
            else if (time < 30 && time > 10)
            {
                _timeText.Color = Color.Yellow;
            }
            else
            {
                _timeText.Color = Color.Red;
            }
            _timeText.text = team + " | " + timeLeftMessage + time.ToString() + secondMessage;
        }

        public static void Init(GraphicsDevice _device) {
            float screenWidth = _device.Viewport.Width;
            float screenHeight = _device.Viewport.Height;

            SpriteFont font = AssetManager.GetFont("Pixel");
            Texture2D buttonTexture = AssetManager.GetTexture("rectangle-hexagon");

            Vector2 textPosition = new Vector2(screenWidth / 2, 20);
            _timeText = new Text(font, timeLeftMessage + "00", textPosition, Color.Green);
            _timeText.Transform.LocalScale = new Vector2(0.5f, 0.5f);

            float buttonWidth = buttonTexture.Width;
            Vector2 buttonPosition = new Vector2(screenWidth - buttonWidth - 40, 20);

            _skipMoveButton = new Button(buttonTexture, buttonPosition, new Vector2(0.2f, 0.2f), Color.White, font, "Skip Move");
            _skipMoveButton.text.Transform.LocalScale = new Vector2(0.2f, 0.2f);
            _skipMoveButton.text.UpdateBounds();
            _skipMoveButton.onClicked += TeamManager.GetTeamByIndex(0).SkipMove;

            AddUI(_timeText);
            AddUI(_skipMoveButton);
        }
        public static void Dispose() {
            onUIAdd = null;
            _timeText = null;
            _skipMoveButton = null;
        }
        public static void AddUI(GameObject ui) {
            onUIAdd?.Invoke(ui);
        }
    }
}
