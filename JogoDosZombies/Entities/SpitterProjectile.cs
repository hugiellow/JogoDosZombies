using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace JogoDosZombies.Entities
{
    public class SpitterProjectile
    {
        public Vector2 Position;
        public bool IsAlive = true;
        public int Damage = 15;

        private float _rotation;
        private float _speed = 280f;
        private int _size = 10;

        public Rectangle Bounds => new Rectangle(
            (int)Position.X - _size / 2,
            (int)Position.Y - _size / 2,
            _size, _size);

        public SpitterProjectile(Vector2 startPos, float rotation)
        {
            Position = startPos;
            _rotation = rotation;
        }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Position += new Vector2(MathF.Cos(_rotation), MathF.Sin(_rotation)) * _speed * dt;

        }

        public void Draw(SpriteBatch sb, Texture2D texture)
        {
            sb.Draw(texture, Bounds, Color.GreenYellow);
        }
    }
}
