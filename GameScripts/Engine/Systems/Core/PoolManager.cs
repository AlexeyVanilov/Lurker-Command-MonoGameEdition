using System.Collections.Generic;

namespace GameEngine.Systems
{
    public static class PoolManager
    {
        private static class Cache<T> where T : class, IPoolable, new()
        {
            public static readonly Queue<T> Nodes = new();
        }

        public static T Get<T>() where T : class, IPoolable, new()
        {
            if (!Cache<T>.Nodes.TryDequeue(out T obj)) obj = new T();
            obj.IsInPool = false;
            obj.OnSpawn();
            return obj;
        }

        public static void Return<T>(T obj) where T : class, IPoolable, new()
        {
            if (obj == null || obj.IsInPool) return;
            obj.OnDespawn();
            obj.IsInPool = true;
            Cache<T>.Nodes.Enqueue(obj);
        }
    }
}