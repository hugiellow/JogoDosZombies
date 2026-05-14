using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JogoDosZombies.Components
{
    /// <summary>
    /// Animador por spritesheet simples.
    /// Cada "frame" é um rectângulo numa textura horizontal.
    /// </summary>
    public class Animator
    {
        private Texture2D _texture;
        private int _frameW;
        private int _frameH;
        private int _frameCount;
        private float _fps;

        private int _currentFrame = 0;
        private float _timer = 0f;

        public Animator(Texture2D spritesheet, int frameW, int frameH, int frameCount, float fps)
        {
            _texture = spritesheet;
            _frameW = frameW;
            _frameH = frameH;
            _frameCount = frameCount;
            _fps = fps;
        }

        public void Update(GameTime gameTime)
        {
            _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_timer >= 1f / _fps)
            {
                _timer = 0f;
                _currentFrame = (_currentFrame + 1) % _frameCount;
            }
        }

        /// <summary>Devolve o source rectangle do frame actual.</summary>
        public Rectangle CurrentFrame =>
            new Rectangle(_currentFrame * _frameW, 0, _frameW, _frameH);

        public void Draw(SpriteBatch sb, Vector2 position, float rotation,
                         Color color, float scale = 1f,
                         SpriteEffects effects = SpriteEffects.None)
        {
            var origin = new Vector2(_frameW / 2f, _frameH / 2f);
            sb.Draw(_texture, position, CurrentFrame, color,
                    rotation, origin, scale, effects, 0f);
        }
    }
}