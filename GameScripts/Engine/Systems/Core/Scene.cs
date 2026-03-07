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

        private IDraggable _currentDraggedLBM;
        private Entity _draggedEntityLBM;
        private Vector2 _dragOffsetLBM;

        private IDraggable _currentDraggedRBM;
        private Entity _draggedEntityRBM;
        private Vector2 _dragOffsetRBM;

        public void Add(GameObject obj) => _toAdd.Add(obj);
        public bool Contains(GameObject obj) => _toAdd.Contains(obj);
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
                            _currentDraggedLBM = draggable;
                            _draggedEntityLBM = rectable as Entity;

                            if (_draggedEntityLBM != null)
                            {
                                _dragOffsetLBM = _draggedEntityLBM.Transform.LocalPosition - mouseWorld;
                                _currentDraggedLBM.OnDragStartLBM();
                            }
                        }
                        break;
                    }
                }
            }
            else if (_draggedEntityLBM != null && InputManager.IsMouseButtonDown(MouseButton.Left))
            {
                _currentDraggedLBM.OnDragUpdateLBM(mouseWorld + _dragOffsetLBM);
            }
            else if (_draggedEntityLBM != null)
            {
                _currentDraggedLBM.OnDragEndLBM();
                _draggedEntityLBM = null;
                _currentDraggedLBM = null;
            }

            if (InputManager.IsMouseButtonPressed(MouseButton.Right))
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
                            _currentDraggedRBM = draggable;
                            _draggedEntityRBM = rectable as Entity;

                            if (_draggedEntityRBM != null)
                            {
                                _dragOffsetRBM = _draggedEntityRBM.Transform.LocalPosition - mouseWorld;
                                _currentDraggedRBM.OnDragStartRBM();
                            }
                        }
                        break;
                    }
                }
            }
            else if (_draggedEntityRBM != null && InputManager.IsMouseButtonDown(MouseButton.Right))
            {
                _currentDraggedRBM.OnDragUpdateRBM(mouseWorld + _dragOffsetRBM);
            }
            else if (_draggedEntityRBM != null)
            {
                _currentDraggedRBM.OnDragEndRBM();
                _draggedEntityRBM = null;
                _currentDraggedRBM = null;
            }
        }

        private void SyncCollections()
        {
            if (_toAdd.Count == 0 && _toRemove.Count == 0) return;

            if (_toRemove.Count > 0)
            {
                for (uint i = 0; i < _toRemove.Count; i++)
                {
                    var obj = _toRemove[(int)i];
                    if (obj is IUpdate u) _updatables.Remove(u);
                    if (obj is IDraw d) _drawables.Remove(d);
                    if (obj is IRect r) _inputTargets.Remove(r);
                }
                _toRemove.Clear();
            }

            if (_toAdd.Count > 0)
            {
                for (uint i = 0; i < _toAdd.Count; i++)
                {
                    var obj = _toAdd[(int)i];
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