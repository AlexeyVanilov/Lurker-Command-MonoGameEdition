using GameEngine.Components.Core;
using GameEngine.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GameEngine.Systems
{
    public abstract class Scene : IDisposable
    {
        private readonly List<IUpdate> _updatables = new(1024);
        private readonly List<IDraw> _drawables = new(1024);
        private readonly List<IRect> _inputTargets = new(512);

        private readonly List<GameObject> _toAdd = new(128);
        private readonly List<GameObject> _toRemove = new(128);

        public Camera2D camera { get; private set; }
        private bool _needsSort;

        private IDraggable _currentDragged;
        private Entity _draggedEntity;
        private Vector2 _dragOffset;

        public void Add(GameObject obj) => _toAdd.Add(obj);
        public void Remove(GameObject obj) => _toRemove.Add(obj);
        public virtual void Update(GameTime gameTime)
        {
            HandleInput();
            SyncCollections();

            var updateSpan = CollectionsMarshal.AsSpan(_updatables);
            for (int i = 0; i < updateSpan.Length; i++)
            {
                updateSpan[i].Update(gameTime);
            }
        }

        private void HandleInput()
        {
            if (camera == null) return;

            Vector2 mouseWorld = camera.ScreenToWorld(InputManager.MousePosition);

            if (InputManager.IsMouseButtonPressed(MouseButton.Left))
            {
                var inputSpan = CollectionsMarshal.AsSpan(_inputTargets);
                for (int i = inputSpan.Length - 1; i >= 0; i--)
                {
                    var rectable = inputSpan[i];
                    if (rectable.GetBounds().Contains(mouseWorld.ToPoint()))
                    {
                        if (rectable is IClickable clickable) clickable.OnPointerDown();

                        if (rectable is IDraggable draggable)
                        {
                            _currentDragged = draggable;
                            _draggedEntity = rectable as Entity;

                            if (_draggedEntity != null)
                            {
                                _dragOffset = _draggedEntity.Transform.LocalPosition - mouseWorld;
                                _currentDragged.OnDragStart();
                            }
                        }
                        break;
                    }
                }
            }
            else if (_draggedEntity != null && InputManager.IsMouseButtonDown(MouseButton.Left))
            {
                _currentDragged.OnDragUpdate(mouseWorld + _dragOffset);
            }
            else if (_draggedEntity != null)
            {
                _currentDragged.OnDragEnd();
                _draggedEntity = null;
                _currentDragged = null;
            }
        }

        private void SyncCollections()
        {
            if (_toAdd.Count == 0 && _toRemove.Count == 0) return;

            if (_toRemove.Count > 0)
            {
                for (int i = 0; i < _toRemove.Count; i++)
                {
                    var obj = _toRemove[i];
                    if (obj is IUpdate u) _updatables.Remove(u);
                    if (obj is IDraw d) _drawables.Remove(d);
                    if (obj is IRect r) _inputTargets.Remove(r);
                }
                _toRemove.Clear();
            }

            if (_toAdd.Count > 0)
            {
                for (int i = 0; i < _toAdd.Count; i++)
                {
                    var obj = _toAdd[i];
                    if (obj is IUpdate u) _updatables.Add(u);
                    if (obj is IDraw d) _drawables.Add(d);
                    if (obj is IRect r) _inputTargets.Add(r);
                }
                _toAdd.Clear();
                _needsSort = true;
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (camera == null) return;

            if (_needsSort)
            {
                _drawables.Sort((a, b) => a.OrderInLayer.CompareTo(b.OrderInLayer));
                _needsSort = false;
            }

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, camera.GetViewMatrix());

            var drawSpan = CollectionsMarshal.AsSpan(_drawables);
            for (int i = 0; i < drawSpan.Length; i++)
            {
                drawSpan[i].Draw(gameTime, spriteBatch);
            }

            spriteBatch.End();
        }

        public virtual void Dispose()
        {
            var drawSpan = CollectionsMarshal.AsSpan(_drawables);
            for (int i = 0; i < drawSpan.Length; i++)
            {
                if (drawSpan[i] is IDisposable d) d.Dispose();
            }
            _updatables.Clear();
            _drawables.Clear();
            _inputTargets.Clear();
        }

        public abstract void Load();
        public void SetCamera(Camera2D cam) => camera = cam;
    }
}