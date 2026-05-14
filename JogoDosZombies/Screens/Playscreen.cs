using JogoDosZombies.Entities;
using JogoDosZombies.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using JogoDosZombies.Entities;
using JogoDosZombies.Core;
using JogoDosZombies.Managers;

namespace JogoDosZombies.Screens
{
    public class PlayScreen : GameScreen
    {
        // ---- Assets ----
        private Texture2D _pixel;       // 1×1 white texture used for everything
        private SpriteFont _font;

        // ---- Entities ----
        private Player _player;
        private List<Bullet> _bullets = new();
        private WaveSystem _waves;

        // ---- State ----
        private bool _gameOver = false;
        private int _score = 0;
        private float _waveDelay = 0f;   // countdown before next wave starts
        private const float WaveDelayTime = 3f;

        // Between-wave message
        private bool _waitingForWave = true;

        public PlayScreen(Game1 game) : base(game) { }

        // ------------------------------------------------------------------
        public override void LoadContent()
        {
            // 1×1 pixel texture — used for ALL drawing (shapes coloured in Draw)
            _pixel = new Texture2D(Game.GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });

            _font = Game.Content.Load<SpriteFont>("MenuFont");

            _player = new Player(_pixel, new Vector2(Game.ScreenWidth / 2f, Game.ScreenHeight / 2f));
            _waves = new WaveSystem(_pixel, Game.ScreenWidth, Game.ScreenHeight);

            // Start first wave immediately
            StartNextWave();
        }

        public override void UnloadContent()
        {
            _pixel?.Dispose();
        }

        // ------------------------------------------------------------------
        public override void Update(GameTime gameTime)
        {
            if (_gameOver)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Enter))
                    Game.ChangeScreen(new MenuScreen(Game));
                return;
            }

            // ---- Wave management ----
            if (_waitingForWave)
            {
                _waveDelay -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_waveDelay <= 0)
                {
                    _waves.StartNextWave();
                    _waitingForWave = false;
                }
                return;   // Don't update gameplay while waiting
            }

            // ---- Player ----
            _player.Update(gameTime, Game.ScreenWidth, Game.ScreenHeight);

            // Shooting
            if (_player.TryShoot(out Bullet b))
            {
                b.SetTexture(_pixel);
                _bullets.Add(b);
            }

            // ---- Bullets ----
            for (int i = _bullets.Count - 1; i >= 0; i--)
            {
                _bullets[i].Update(gameTime, Game.ScreenWidth, Game.ScreenHeight);
                if (!_bullets[i].IsAlive) { _bullets.RemoveAt(i); continue; }

                // Bullet ↔ Zombie collision
                foreach (var z in _waves.Zombies)
                {
                    if (z.IsAlive && _bullets[i].Bounds.Intersects(z.Bounds))
                    {
                        z.TakeDamage(_bullets[i].Damage);
                        _bullets[i].IsAlive = false;
                        if (!z.IsAlive) _score += 10;
                        break;
                    }
                }
            }

            // ---- Zombies ----
            _waves.Update(gameTime, _player.Position, out int dmg);
            if (dmg > 0) _player.TakeDamage(dmg);

            if (!_player.IsAlive) { _gameOver = true; return; }

            // ---- Next wave? ----
            if (_waves.WaveComplete)
                StartNextWave();
        }

        private void StartNextWave()
        {
            _waitingForWave = true;
            _waveDelay = WaveDelayTime;
        }

        // ------------------------------------------------------------------
        public override void Draw(GameTime gameTime)
        {
            var sb = Game.SpriteBatch;
            sb.Begin();

            DrawBackground(sb);
            if (!_gameOver) _waves.Draw(sb);
            if (!_gameOver) foreach (var b in _bullets) b.Draw(sb);
            if (!_gameOver) _player.Draw(sb);

            DrawHUD(sb);

            if (_waitingForWave && !_gameOver)
                DrawWaveBanner(sb);

            if (_gameOver)
                DrawGameOver(sb);

            sb.End();
        }

        private void DrawBackground(SpriteBatch sb)
        {
            // Dark tiled "floor"
            int tileSize = 64;
            for (int x = 0; x < Game.ScreenWidth; x += tileSize)
                for (int y = 0; y < Game.ScreenHeight; y += tileSize)
                {
                    Color c = ((x / tileSize + y / tileSize) % 2 == 0)
                        ? new Color(30, 30, 30) : new Color(24, 24, 24);
                    sb.Draw(_pixel, new Rectangle(x, y, tileSize, tileSize), c);
                }
        }

        private void DrawHUD(SpriteBatch sb)
        {
            // ---- Health bar (bottom-left) ----
            int hbW = 200, hbH = 20;
            var hbBg = new Rectangle(20, Game.ScreenHeight - 40, hbW, hbH);
            var hbFg = new Rectangle(20, Game.ScreenHeight - 40,
                (int)(hbW * ((float)_player.Health / _player.MaxHealth)), hbH);
            sb.Draw(_pixel, hbBg, Color.DarkRed);
            sb.Draw(_pixel, hbFg, Color.LimeGreen);
            sb.DrawString(_font, $"HP  {_player.Health}/{_player.MaxHealth}",
                new Vector2(22, Game.ScreenHeight - 42), Color.White);

            // ---- Ammo (bottom-right) ----
            string ammoStr = _player.IsReloading
                ? "RECARREGANDO..."
                : $"Munição: {_player.Ammo}/{_player.MaxAmmo}";
            var ammoSize = _font.MeasureString(ammoStr);
            sb.DrawString(_font, ammoStr,
                new Vector2(Game.ScreenWidth - ammoSize.X - 20, Game.ScreenHeight - 42),
                _player.IsReloading ? Color.Orange : Color.White);

            // ---- Score & Wave (top-left) ----
            sb.DrawString(_font, $"Pontuação: {_score}    Onda: {_waves.CurrentWave}",
                new Vector2(20, 20), Color.White);
        }

        private void DrawWaveBanner(SpriteBatch sb)
        {
            string msg = _waves.CurrentWave == 0
                ? "PREPARA-TE!"
                : $"ONDA {_waves.CurrentWave + 1} EM {(int)_waveDelay + 1}s…";
            var size = _font.MeasureString(msg);
            sb.DrawString(_font, msg,
                new Vector2((Game.ScreenWidth - size.X) / 2f, Game.ScreenHeight * 0.45f),
                Color.Yellow);
        }

        private void DrawGameOver(SpriteBatch sb)
        {
            // Dim overlay
            sb.Draw(_pixel,
                new Rectangle(0, 0, Game.ScreenWidth, Game.ScreenHeight),
                Color.Black * 0.6f);

            string go = "GAME OVER";
            string sc = $"Pontuação final: {_score}";
            string hint = "ENTER para voltar ao menu";

            var goSize = _font.MeasureString(go);
            var scSize = _font.MeasureString(sc);
            var hintSize = _font.MeasureString(hint);

            float cx = Game.ScreenWidth / 2f;
            float cy = Game.ScreenHeight / 2f;

            sb.DrawString(_font, go, new Vector2(cx - goSize.X / 2, cy - 60), Color.OrangeRed);
            sb.DrawString(_font, sc, new Vector2(cx - scSize.X / 2, cy), Color.White);
            sb.DrawString(_font, hint, new Vector2(cx - hintSize.X / 2, cy + 50), Color.Gray);
        }
    }
}