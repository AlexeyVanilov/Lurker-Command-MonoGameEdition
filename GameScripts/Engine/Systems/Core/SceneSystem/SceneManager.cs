using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace GameEngine.Systems
{
    public static class SceneManager
    {
        private static readonly List<Scene> _scenes = new List<Scene>();

        private static Scene _activeScene;

        public static void RegisterScene(Scene scene) => _scenes.Add(scene);
        public static void UnregisterScene(Scene scene) => _scenes.Remove(scene);

        public static void LoadScene(byte index)
        {
            if (index >= _scenes.Count) return;

            _activeScene?.Dispose();

            _activeScene = _scenes[index];

            _activeScene.Load();
        }
        public static void Update(GameTime gt) => _activeScene?.Update(gt);
        public static void Draw(GameTime gt, SpriteBatch sb) => _activeScene?.Draw(gt, sb);
        public static void Add(GameObject obj) => _activeScene?.Add(obj);
        public static void Remove(GameObject obj) => _activeScene?.Remove(obj);
    }
}