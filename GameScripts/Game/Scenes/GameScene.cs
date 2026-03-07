using GameEngine.Components.Core;
using GameEngine.Services;
using GameEngine.Systems;
using LurkerCommand.GameSystem;
using LurkerCommand.MapSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LurkerCommand.Scenes
{
    public sealed class GameScene : Scene
    {
        private readonly GraphicsDevice _device;

        public GameScene(GraphicsDevice device) => _device = device;

        public override void Load()
        {
            Field.SetMap(this);
            TeamManager.Init();

            Camera2D cm = new Camera2D(_device);
            SetCamera(cm);

            CameraMovement cmMovement = new CameraMovement(cm, new Vector2(Field.MapWidth / 2, Field.MapHeight / 2));
            Add(cmMovement);

            var spawnData = Field.GetBaseSpawnPoints();

            foreach (var data in spawnData)
            {
                Unit newUnit = new Unit();
                newUnit.Setup(AssetManager.GetFont("Arial"), data.position, 3);
                TeamManager.AddUnitToTeam(data.team, newUnit);
                Add(newUnit);

                Field.GetCell(data.position).BindUnit(newUnit);
            }
        }

        public override void Update(GameTime gameTime)
        {
            TeamManager.Update(gameTime);
            base.Update(gameTime);
        }
    }
}