using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JogoDosZombies.Core
{
   
    public class Camera
    {
        public Vector2 Position { get; private set; }

        public int WorldWidth { get; }
        public int WorldHeight { get; }

        private readonly int _viewW;
        private readonly int _viewH;

        public Camera(int viewW, int viewH, int worldW, int worldH)
        {
            _viewW = viewW;
            _viewH = viewH;
            WorldWidth = worldW;
            WorldHeight = worldH;
        }

        public void Follow(Vector2 target)
        {
            float x = target.X - _viewW / 2f;
            float y = target.Y - _viewH / 2f;

            
            x = MathHelper.Clamp(x, 0, WorldWidth - _viewW);
            y = MathHelper.Clamp(y, 0, WorldHeight - _viewH);

            Position = new Vector2(x, y);
        }

        public Matrix GetTransform()
            => Matrix.CreateTranslation(-Position.X, -Position.Y, 0f);

      
        public Vector2 ScreenToWorld(Vector2 screenPos)
            => screenPos + Position;
    }
}
