using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace JogoDosZombies.Entities
{
    public class Zombie
    {
        public Vector2 Position;
        public int Health = 60;
        public bool IsAlive => Health > 0;

        private float _speed;
        private Texture2D _texture;
        private int _size = 30;
        private float _attackCooldown = 0f;
        private float _attackRate = 1.0f;   // 1 hit per second
        public int Damage = 10;

        public Rectangle Bounds => new Rectangle(
            (int)Position.X - _size / 2,
            (int)Position.Y - _size / 2,
            _size, _size);

        public Zombie(Texture2D texture, Vector2 startPos, float speed = 80f)
        {
            _texture = texture;
            Position = startPos;
            _speed = speed;
        }

        public void Update(GameTime gameTime, Vector2 playerPos, out bool didAttack)
        {
            didAttack = false;
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Walk towards player
            Vector2 dir = playerPos - Position;
            if (dir.LengthSquared() > 1f)
            {
                dir.Normalize();
                Position += dir * _speed * dt;
            }

            // Attack cooldown
            if (_attackCooldown > 0) _attackCooldown -= dt;

            // Check melee range
            float dist = Vector2.Distance(Position, playerPos);
            if (dist < 36f && _attackCooldown <= 0)
            {
                didAttack = true;
                _attackCooldown = _attackRate;
            }
        }

        public void TakeDamage(int amount) => Health = Math.Max(0, Health - amount);

        public void Draw(SpriteBatch sb)
        {
            // Body
            sb.Draw(_texture, Bounds, Color.DarkGreen);

            // Eyes (two small red squares)
            int ex = 5, ey = 5;
            sb.Draw(_texture, new Rectangle((int)Position.X - 8, (int)Position.Y - 6, ex, ey), Color.Red);
            sb.Draw(_texture, new Rectangle((int)Position.X + 3, (int)Position.Y - 6, ex, ey), Color.Red);

            // Mini HP bar
            int barW = 30, barH = 4;
            var bg = new Rectangle((int)Position.X - barW / 2, (int)Position.Y - _size / 2 - 8, barW, barH);
            var fg = new Rectangle(bg.X, bg.Y, (int)(barW * (Health / 60f)), barH);
            sb.Draw(_texture, bg, Color.DarkRed);
            sb.Draw(_texture, fg, Color.Red);
        }
    }
}