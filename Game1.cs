using GameEngine.Services;
using GameEngine.Systems;
using LurkerCommand.Scenes;
using LurkerCommand.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LurkerCommand
{
    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Keys _fullScreenKey;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            ConfigManager.Initialize();
            ApplyInitialSettings();
        }

        private void ApplyInitialSettings()
        {
            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 600;
            _graphics.IsFullScreen = ConfigManager.Get<bool>("FullScreen");
            _graphics.HardwareModeSwitch = false;

            Window.AllowAltF4 = ConfigManager.Get<bool>("AltF4");
            Window.AllowUserResizing = ConfigManager.Get<bool>("AllowResizing");
            Window.Title = ConfigManager.Get<string>("WindowTitle");
            _fullScreenKey = ConfigManager.Get<Keys>("FullScreenKey");
        }

        protected override void Initialize()
        {
            AssetManager.Init(Content);
            _graphics.ApplyChanges();

            SceneManager.SetScene(new GameScene(GraphicsDevice));
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            InputManager.Update();

            if (InputManager.IsKeyPressed(_fullScreenKey))
            {
                _graphics.ToggleFullScreen();
                ConfigManager.Save();
            }

            SceneManager.CurrentScene?.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            if (!IsActive) return;

            GraphicsDevice.Clear(Color.Black);
            SceneManager.CurrentScene?.Draw(gameTime, _spriteBatch);
            base.Draw(gameTime);
        }
    }
}