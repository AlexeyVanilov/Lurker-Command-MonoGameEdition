using GameEngine.Components.Core;
using GameEngine.Services;
using GameEngine.Systems;
using LurkerCommand.MapSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

public sealed class CameraMovement : Entity
{
    public Camera2D camera;
    private const float Speed = 15f;
    private const float ZoomFactor = 0.1f;
    private const float EdgeThreshold = 10f;

    public CameraMovement(Camera2D camera, Vector2 startPosition) : base(Vector2.Zero, Vector2.One, 0f, false)
    {
        this.camera = camera;
        MoveCamera(startPosition);
    }

    public override void Update(GameTime gameTime)
    {
        Vector2 movement = Vector2.Zero;

        if (InputManager.IsMouseButtonDown(MouseButton.Middle))
        {
            movement = -InputManager.MouseDelta / camera.Zoom;
        }
        else
        {
            movement = GetKeyboardDirection() + GetEdgeDirection();
            if (movement != Vector2.Zero)
            {
                movement.Normalize();
                movement *= Speed;
            }
        }

        HandleZoom();

        if (movement != Vector2.Zero)
        {
            MoveCamera(camera.Position + movement);
        }
    }

    private Vector2 GetKeyboardDirection()
    {
        Vector2 dir = Vector2.Zero;
        if (InputManager.IsKeyDown(Keys.W) || InputManager.IsKeyDown(Keys.Up)) dir.Y -= 1;
        if (InputManager.IsKeyDown(Keys.S) || InputManager.IsKeyDown(Keys.Down)) dir.Y += 1;
        if (InputManager.IsKeyDown(Keys.A) || InputManager.IsKeyDown(Keys.Left)) dir.X -= 1;
        if (InputManager.IsKeyDown(Keys.D) || InputManager.IsKeyDown(Keys.Right)) dir.X += 1;
        return dir;
    }

    private Vector2 GetEdgeDirection()
    {
        Vector2 dir = Vector2.Zero;
        Vector2 mousePos = InputManager.MousePosition;
        var viewport = camera.graphics.Viewport;

        if (mousePos.X <= EdgeThreshold) dir.X = -1;
        else if (mousePos.X >= viewport.Width - EdgeThreshold) dir.X = 1;

        if (mousePos.Y <= EdgeThreshold) dir.Y = -1;
        else if (mousePos.Y >= viewport.Height - EdgeThreshold) dir.Y = 1;

        return dir;
    }

    private void HandleZoom()
    {
        int scroll = InputManager.ScrollDelta;
        if (scroll != 0)
        {
            camera.Zoom = MathHelper.Clamp(camera.Zoom + (Math.Sign(scroll) * ZoomFactor), camera.MinZoom, camera.MaxZoom);
        }
    }

    public void MoveCamera(Vector2 targetPosition)
    {
        var viewport = camera.graphics.Viewport;
        float invZoom = 1.0f / camera.Zoom;

        float halfViewWidth = (viewport.Width * 0.5f) * invZoom;
        float halfViewHeight = (viewport.Height * 0.5f) * invZoom;

        float minX = halfViewWidth;
        float maxX = Field.MapWidth - halfViewWidth;
        float minY = halfViewHeight;
        float maxY = Field.MapHeight - halfViewHeight;

        camera.Position = new Vector2(
            maxX > minX ? MathHelper.Clamp(targetPosition.X, minX, maxX) : Field.MapWidth * 0.5f,
            maxY > minY ? MathHelper.Clamp(targetPosition.Y, minY, maxY) : Field.MapHeight * 0.5f
        );
    }
}