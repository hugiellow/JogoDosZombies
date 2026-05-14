using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace JogoDosZombies.Entities
{
    public class Bullet
    {
        public Vector2 Position;
        public bool IsAlive = true;
        public int Damage = 25;

        private float _rotation;
        private float _speed = 600f;
        private Texture2D _texture;
        private int _size = 8;

        public Rectangle Bounds => new Rectangle(
            (int)Position.X - _size / 2,
            (int)Position.Y - _size / 2,
            _size, _size);

        public Bullet(Vector2 startPos, float rotation)
        {
            Position = startPos;
            _rotation = rotation;
        }

        public void SetTexture(Texture2D tex) => _texture = tex;

        public void Update(GameTime gameTime, int screenW, int screenH)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Position += new Vector2(MathF.Cos(_rotation), MathF.Sin(_rotation)) * _speed * dt;

            // Kill if out of screen
            if (Position.X < 0 || Position.X > screenW ||
                Position.Y < 0 || Position.Y > screenH)
                IsAlive = false;
        }

        public void Draw(SpriteBatch sb)
        {
            if (_texture == null) return;
            sb.Draw(_texture, Bounds, Color.Yellow);
        }
    }
}