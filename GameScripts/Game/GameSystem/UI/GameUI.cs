using GameEngine.Components.UI;
using GameEngine.Services;
using GameEngine.Systems;
using LurkerCommand.GameScripts.Engine.Components.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LurkerCommand.GameSystem {
    public static class GameUI {
        private static Text _timeText;
        private static Button _skipMoveButton;

        private const string timeLeftMessage = "Time Left: ";
        public static void InitUI()
        {
            SpriteFont font = AssetManager.GetFont("Arial");
            Texture2D buttonTexture = AssetManager.GetTexture("rectangle-hexagon");

            Vector2 textPosition = Vector2.One;
            _timeText = new Text(font, timeLeftMessage + "00", textPosition, Color.White);
            _timeText.OrderInLayer = 10;

            float buttonWidth = buttonTexture.Width * 1.0f;
            float spacing = 20f;

            Vector2 buttonPosition = new Vector2(
                textPosition.X - buttonWidth - spacing,
                textPosition.Y
            );

            _skipMoveButton = new Button(buttonTexture, buttonPosition, Vector2.One, Color.White, font, "Skip Move");
            _skipMoveButton.onClicked += TeamManager.NextTurn;
            _skipMoveButton.OrderInLayer = 10;

            SceneManager.Add(_timeText);
            SceneManager.Add(_skipMoveButton);
        }
        public static void UpdateTime(float time) {
            _timeText.text = timeLeftMessage + time.ToString();
        }
    }
}
