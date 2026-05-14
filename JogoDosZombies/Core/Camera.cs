using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JogoDosZombies.Core
{
    /// <summary>
    /// 2D câmara que segue o jogador num mundo maior que a janela.
    /// Usa uma Matrix de transformação passada ao SpriteBatch.Begin().
    /// </summary>
    public class Camera
    {
        public Vector2 Position { get; private set; }

        // Dimensões do mundo (maior que a janela)
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

        /// <summary>Centra a câmara no alvo, com clamp nas bordas do mundo.</summary>
        public void Follow(Vector2 target)
        {
            float x = target.X - _viewW / 2f;
            float y = target.Y - _viewH / 2f;

            // Clamp: não mostrar fora dos limites do mundo
            x = MathHelper.Clamp(x, 0, WorldWidth - _viewW);
            y = MathHelper.Clamp(y, 0, WorldHeight - _viewH);

            Position = new Vector2(x, y);
        }

        /// <summary>Matrix de transformação para usar em SpriteBatch.Begin(transformMatrix: ...).</summary>
        public Matrix GetTransform()
            => Matrix.CreateTranslation(-Position.X, -Position.Y, 0f);

        /// <summary>Converte posição de ecrã (rato) para posição no mundo.</summary>
        public Vector2 ScreenToWorld(Vector2 screenPos)
            => screenPos + Position;
    }
}