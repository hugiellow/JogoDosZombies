using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace JogoDosZombies.Entities
{
    public class Zombie
    {
        public Vector2 Position;
        public int Health;
        private int _maxHealth;
        public bool IsAlive => Health > 0;
        public ZombieType Type { get; private set; }

        private float _speed;
        private Texture2D _texture;
        private int _size;
        private float _attackCooldown = 0f;
        private float _attackRate;
        public int Damage;

        // Spitter projectile state
        private float _spitCooldown = 0f;
        private const float SpitRate = 2.0f;
        private const float SpitRange = 200f;
        private const float SpitFleeRange = 120f;
        public List<SpitterProjectile> Projectiles { get; } = new();

        public Rectangle Bounds => new Rectangle(
            (int)Position.X - _size / 2,
            (int)Position.Y - _size / 2,
            _size, _size);

        // Factory — cria o zombie certo com os stats base + escalamento de onda
        public static Zombie Create(Texture2D texture, Vector2 pos, ZombieType type, int wave)
        {
            return type switch
            {
                ZombieType.Fast => new Zombie(texture, pos, type,
                                        speed: 130f + wave * 12f,
                                        health: 30 + wave * 5,
                                        damage: 8 + wave,
                                        rate: 0.8f,
                                        size: 22),

                ZombieType.Tank => new Zombie(texture, pos, type,
                                        speed: 40f + wave * 4f,
                                        health: 180 + wave * 30,
                                        damage: 25 + wave * 4,
                                        rate: 1.5f,
                                        size: 42),

                ZombieType.Spitter => new Zombie(texture, pos, type,
                                        speed: 55f + wave * 5f,
                                        health: 50 + wave * 10,
                                        damage: 0,        // dano vem dos projéteis
                                        rate: 99f,      // nunca ataca corpo a corpo
                                        size: 26),

                _ => new Zombie(texture, pos, type,   // Normal
                                        speed: 60f + wave * 10f,
                                        health: 60 + wave * 15,
                                        damage: 10 + wave * 2,
                                        rate: Math.Max(0.4f, 1.0f - wave * 0.05f),
                                        size: 30),
            };
        }

        private Zombie(Texture2D texture, Vector2 pos, ZombieType type,
                       float speed, int health, int damage, float rate, int size)
        {
            _texture = texture;
            Position = pos;
            Type = type;
            _speed = speed;
            Health = health;
            _maxHealth = health;
            Damage = damage;
            _attackRate = rate;
            _size = size;
        }

        public void Update(GameTime gameTime, Vector2 playerPos,
                           List<Zombie> allZombies, out bool didAttack)
        {
            didAttack = false;
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (Type == ZombieType.Spitter)
                UpdateSpitter(gameTime, playerPos, dt);
            else
                MoveTowards(playerPos, dt);

            // Separação entre zombies
            Separate(allZombies, dt);

            // Ataque corpo a corpo (não spitter)
            if (Type != ZombieType.Spitter)
            {
                if (_attackCooldown > 0) _attackCooldown -= dt;
                if (Vector2.Distance(Position, playerPos) < 36f && _attackCooldown <= 0)
                {
                    didAttack = true;
                    _attackCooldown = _attackRate;
                }
            }
        }

        private void MoveTowards(Vector2 target, float dt)
        {
            Vector2 dir = target - Position;
            if (dir.LengthSquared() > 1f)
            {
                dir.Normalize();
                Position += dir * _speed * dt;
            }
        }

        private void UpdateSpitter(GameTime gameTime, Vector2 playerPos, float dt)
        {
            float dist = Vector2.Distance(Position, playerPos);

            // Foge se o jogador está muito perto
            if (dist < SpitFleeRange)
            {
                Vector2 away = Position - playerPos;
                away.Normalize();
                Position += away * _speed * dt;
            }
            // Aproxima-se se está muito longe
            else if (dist > SpitRange)
            {
                MoveTowards(playerPos, dt);
            }
            // Na zona de alcance — dispara
            else
            {
                _spitCooldown -= dt;
                if (_spitCooldown <= 0)
                {
                    float angle = MathF.Atan2(
                        playerPos.Y - Position.Y,
                        playerPos.X - Position.X);
                    Projectiles.Add(new SpitterProjectile(Position, angle));
                    _spitCooldown = SpitRate;
                }
            }

            // Atualiza projéteis
            for (int i = Projectiles.Count - 1; i >= 0; i--)
            {
                Projectiles[i].Update(gameTime);
                if (!Projectiles[i].IsAlive) Projectiles.RemoveAt(i);
            }
        }

        private void Separate(List<Zombie> allZombies, float dt)
        {
            float radius = _size + 5f;
            float force = 80f;
            foreach (var other in allZombies)
            {
                if (other == this || !other.IsAlive) continue;
                Vector2 diff = Position - other.Position;
                float dist = diff.Length();
                if (dist < radius && dist > 0.01f)
                {
                    diff.Normalize();
                    Position += diff * force * dt;
                }
            }
        }

        public void TakeDamage(int amount) => Health = Math.Max(0, Health - amount);

        public void Draw(SpriteBatch sb)
        {
            // Cor por tipo
            Color bodyColor = Type switch
            {
                ZombieType.Fast => Color.OrangeRed,
                ZombieType.Tank => Color.Purple,
                ZombieType.Spitter => Color.Olive,
                _ => Color.DarkGreen,
            };

            // Fica mais claro quando tem pouca vida
            if (Health < _maxHealth * 0.3f)
                bodyColor = Color.Lerp(bodyColor, Color.White, 0.3f);

            sb.Draw(_texture, Bounds, bodyColor);

            // Olhos
            sb.Draw(_texture, new Rectangle((int)Position.X - 8, (int)Position.Y - 6, 5, 5), Color.Red);
            sb.Draw(_texture, new Rectangle((int)Position.X + 3, (int)Position.Y - 6, 5, 5), Color.Red);

            // Símbolo por tipo (desenhado com pixel)
            if (Type == ZombieType.Tank)
            {
                // Cruz no peito
                sb.Draw(_texture, new Rectangle((int)Position.X - 1, (int)Position.Y - 6, 3, 12), Color.White * 0.4f);
                sb.Draw(_texture, new Rectangle((int)Position.X - 6, (int)Position.Y - 1, 12, 3), Color.White * 0.4f);
            }
            else if (Type == ZombieType.Spitter)
            {
                // Ponto verde na boca
                sb.Draw(_texture, new Rectangle((int)Position.X - 2, (int)Position.Y + 4, 5, 5), Color.LimeGreen);
            }

            // Projéteis do spitter
            foreach (var p in Projectiles) p.Draw(sb, _texture);

            // Barra de HP
            int barW = _size, barH = 4;
            var bg = new Rectangle((int)Position.X - barW / 2, (int)Position.Y - _size / 2 - 8, barW, barH);
            var fg = new Rectangle(bg.X, bg.Y, (int)(barW * ((float)Health / _maxHealth)), barH);
            sb.Draw(_texture, bg, Color.DarkRed);
            sb.Draw(_texture, fg, Color.Red);
        }
    }
}