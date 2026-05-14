using JogoDosZombies.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace JogoDosZombies.Entities
{
    public class Player
    {
        public Vector2 Position;
        public float Rotation;
        public int Health = 100;
        public int MaxHealth = 100;
        public bool IsAlive => Health > 0;

        // Gun stats
        public int Ammo = 30;
        public int MaxAmmo = 30;
        public float FireCooldown = 0f;
        public float FireRate = 0.25f;   // seconds between shots
        public float ReloadTimer = 0f;
        public bool IsReloading = false;
        public float ReloadTime = 1.5f;

        private Texture2D _texture;
        private float _speed = 200f;
        private int _size = 32;

        public Rectangle Bounds => new Rectangle(
            (int)Position.X - _size / 2,
            (int)Position.Y - _size / 2,
            _size, _size);

        public Player(Texture2D texture, Vector2 startPos)
        {
            _texture = texture;
            Position = startPos;
        }

        public void Update(GameTime gameTime, int screenW, int screenH)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // ---- Movement (WASD) ----
            var kb = Keyboard.GetState();
            var dir = Vector2.Zero;
            if (kb.IsKeyDown(Keys.W)) dir.Y -= 1;
            if (kb.IsKeyDown(Keys.S)) dir.Y += 1;
            if (kb.IsKeyDown(Keys.A)) dir.X -= 1;
            if (kb.IsKeyDown(Keys.D)) dir.X += 1;
            if (dir != Vector2.Zero) dir.Normalize();
            Position += dir * _speed * dt;

            // Keep inside screen
            Position.X = MathHelper.Clamp(Position.X, _size / 2, screenW - _size / 2);
            Position.Y = MathHelper.Clamp(Position.Y, _size / 2, screenH - _size / 2);

            // ---- Aim at mouse ----
            var mouse = Mouse.GetState();
            Vector2 toMouse = new Vector2(mouse.X, mouse.Y) - Position;
            if (toMouse != Vector2.Zero)
                Rotation = MathF.Atan2(toMouse.Y, toMouse.X);

            // ---- Cooldowns ----
            if (FireCooldown > 0) FireCooldown -= dt;

            if (IsReloading)
            {
                ReloadTimer -= dt;
                if (ReloadTimer <= 0)
                {
                    Ammo = MaxAmmo;
                    IsReloading = false;
                }
            }

            // Auto-reload when empty
            if (Ammo <= 0 && !IsReloading)
                StartReload();

            // Manual reload (R)
            if (kb.IsKeyDown(Keys.R) && !IsReloading && Ammo < MaxAmmo)
                StartReload();
        }

        public void StartReload()
        {
            IsReloading = true;
            ReloadTimer = ReloadTime;
        }

        /// <summary>Returns true and spawns a bullet if the player can shoot.</summary>
        public bool TryShoot(out Bullet bullet)
        {
            bullet = null;
            var mouse = Mouse.GetState();
            if (mouse.LeftButton == ButtonState.Pressed
                && FireCooldown <= 0
                && Ammo > 0
                && !IsReloading)
            {
                Ammo--;
                FireCooldown = FireRate;
                // Muzzle offset (front of sprite)
                var muzzle = Position + new Vector2(MathF.Cos(Rotation), MathF.Sin(Rotation)) * 24f;
                bullet = new Bullet(muzzle, Rotation);
                return true;
            }
            return false;
        }

        public void TakeDamage(int amount) => Health = Math.Max(0, Health - amount);

        public void Draw(SpriteBatch sb)
        {
            // Draw body
            sb.Draw(_texture,
                destinationRectangle: Bounds,
                sourceRectangle: null,
                color: Color.LimeGreen,
                rotation: Rotation,
                origin: new Vector2(_texture.Width / 2f, _texture.Height / 2f),
                effects: SpriteEffects.None,
                layerDepth: 0f);

            DrawHealthBar(sb);
        }

        private void DrawHealthBar(SpriteBatch sb)
        {
            int barW = 50, barH = 6;
            var bg = new Rectangle((int)Position.X - barW / 2, (int)Position.Y - _size / 2 - 12, barW, barH);
            var fg = new Rectangle(bg.X, bg.Y, (int)(barW * ((float)Health / MaxHealth)), barH);
            sb.Draw(_texture, bg, Color.DarkRed);
            sb.Draw(_texture, fg, Color.LimeGreen);
        }
    }
}