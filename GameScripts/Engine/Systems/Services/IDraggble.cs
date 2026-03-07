using Microsoft.Xna.Framework;

namespace GameEngine.Services {
    public interface IDraggable
    {
        void OnDragStartLBM();
        void OnDragUpdateLBM(Vector2 position);
        void OnDragEndLBM();
        void OnDragStartRBM();
        void OnDragUpdateRBM(Vector2 position);
        void OnDragEndRBM();
    }
}