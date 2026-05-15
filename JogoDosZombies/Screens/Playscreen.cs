using JogoDosZombies.Entities;
using JogoDosZombies.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using JogoDosZombies.Core;
using JogoDosZombies.Managers;
using Microsoft.Xna.Framework.Audio;

namespace JogoDosZombies.Screens
{
    public class PlayScreen : GameScreen
    {
        // ---- Assets ----
        private Texture2D _pixel;
        private SpriteFont _font;

        // ---- Sounds ----
        private SoundEffect _zombieDeathSound;
        private SoundEffect _gameOverSound;
        private SoundEffect _victorySound;

        // ---- Câmara ----
        private Camera _camera;

        // Mundo maior que a janela (requisito 3)
        private const int WorldWidth = 3200;
        private const int WorldHeight = 2400;

        // ---- Entities ----
        private Player _player;
        private List<Bullet> _bullets = new();
        private WaveSystem _waves;

        // ---- State ----
        private bool _gameOver = false;
        private bool _gameWon = false;
        private bool _endSoundPlayed = false;
        private int _score = 0;
        private float _waveDelay = 0f;
        private const float WaveDelayTime = 3f;
        private bool _waitingForWave = true;

        public PlayScreen(Game1 game) : base(game) { }

        // ------------------------------------------------------------------
        public override void LoadContent()
        {
            _pixel = new Texture2D(Game.GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });

            _font = Game.Content.Load<SpriteFont>("MenuFont");

            var shootSound = Game.Content.Load<SoundEffect>("Sounds/shoot");
            var reloadSound = Game.Content.Load<SoundEffect>("Sounds/reload");
            var hurtSound = Game.Content.Load<SoundEffect>("Sounds/dano");
            _zombieDeathSound = Game.Content.Load<SoundEffect>("Sounds/zombie_death");
            _gameOverSound = Game.Content.Load<SoundEffect>("Sounds/gameover");
            _victorySound = Game.Content.Load<SoundEffect>("Sounds/win");

            // Câmara com dimensões da janela e do mundo
            _camera = new Camera(Game.ScreenWidth, Game.ScreenHeight, WorldWidth, WorldHeight);

            // Jogador começa no centro do mundo
            _player = new Player(_pixel, new Vector2(WorldWidth / 2f, WorldHeight / 2f));
            _player.LoadSounds(shootSound, reloadSound, hurtSound);

            // WaveSystem usa dimensões do mundo para spawn nas bordas
            _waves = new WaveSystem(_pixel, WorldWidth, WorldHeight);

            StartNextWave();
        }

        public override void UnloadContent()
        {
            _pixel?.Dispose();
        }

        // ------------------------------------------------------------------
        public override void Update(GameTime gameTime)
        {
            // ---- Ecrã final ----
            if (_gameOver || _gameWon)
            {
                if (!_endSoundPlayed)
                {
                    if (_gameOver) _gameOverSound?.Play();
                    else _victorySound?.Play();
                    _endSoundPlayed = true;
                }
                if (Keyboard.GetState().IsKeyDown(Keys.Enter))
                    Game.ChangeScreen(new MenuScreen(Game));
                return;
            }

            // ---- Contagem antes da onda ----
            if (_waitingForWave)
            {
                _waveDelay -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_waveDelay <= 0)
                {
                    _waves.StartNextWave();
                    _waitingForWave = false;
                }
                return;
            }

            // ---- Câmara segue o jogador ----
            _camera.Follow(_player.Position);

            // ---- Converter posição do rato para coordenadas do mundo ----
            var mouse = Mouse.GetState();
            Vector2 worldMouse = _camera.ScreenToWorld(new Vector2(mouse.X, mouse.Y));

            // ---- Player ----
            _player.Update(gameTime, WorldWidth, WorldHeight, worldMouse);

            if (_player.TryShoot(out Bullet b))
            {
                b.SetTexture(_pixel);
                _bullets.Add(b);
            }

            // ---- Zombies ----
            _waves.Update(gameTime, _player.Position, out int dmg);
            if (dmg > 0) _player.TakeDamage(dmg);

            // ---- Colisão: projéteis do Spitter → jogador ----
            foreach (var z in _waves.Zombies)
            {
                for (int p = z.Projectiles.Count - 1; p >= 0; p--)
                {
                    var proj = z.Projectiles[p];
                    if (proj.IsAlive && proj.Bounds.Intersects(_player.Bounds))
                    {
                        _player.TakeDamage(proj.Damage);
                        proj.IsAlive = false;
                    }
                }
            }

            // ---- Colisão: balas do jogador → zombies ----
            for (int i = _bullets.Count - 1; i >= 0; i--)
            {
                _bullets[i].Update(gameTime, WorldWidth, WorldHeight);
                if (!_bullets[i].IsAlive) { _bullets.RemoveAt(i); continue; }

                foreach (var z in _waves.Zombies)
                {
                    if (z.IsAlive && _bullets[i].Bounds.Intersects(z.Bounds))
                    {
                        z.TakeDamage(_bullets[i].Damage);
                        _bullets[i].IsAlive = false;
                        if (!z.IsAlive)
                        {
                            _score += ScoreForZombie(z.Type);
                            _zombieDeathSound?.Play();
                        }
                        break;
                    }
                }
            }

            // ---- Jogador morreu? ----
            if (!_player.IsAlive) { _gameOver = true; return; }

            // ---- Próxima onda? ----
            if (_waves.WaveComplete)
            {
                if (_waves.CurrentWave >= WaveSystem.MaxWaves)
                    _gameWon = true;
                else
                    StartNextWave();
            }
        }

        private int ScoreForZombie(ZombieType type) => type switch
        {
            ZombieType.Fast => 15,
            ZombieType.Tank => 30,
            ZombieType.Spitter => 20,
            _ => 10,
        };

        private void StartNextWave()
        {
            _player.Heal(20);
            _waitingForWave = true;
            _waveDelay = WaveDelayTime;
        }

        // ------------------------------------------------------------------
        public override void Draw(GameTime gameTime)
        {
            var sb = Game.SpriteBatch;

            // ---- Desenho do mundo (com transformação da câmara) ----
            sb.Begin(transformMatrix: _camera.GetTransform());

            DrawBackground(sb);

            if (!_gameOver && !_gameWon)
            {
                _waves.Draw(sb);
                foreach (var b in _bullets) b.Draw(sb);
                _player.Draw(sb);
            }

            sb.End();

            // ---- HUD e overlays (sem câmara — coordenadas de ecrã fixas) ----
            sb.Begin();

            DrawHUD(sb);

            if (_waitingForWave && !_gameOver && !_gameWon)
                DrawWaveBanner(sb);

            if (_gameWon) DrawVictory(sb);
            else if (_gameOver) DrawGameOver(sb);

            sb.End();
        }

        // ------------------------------------------------------------------
        private void DrawBackground(SpriteBatch sb)
        {
            // Fundo cobre o mundo inteiro (não só a janela)
            int tileSize = 64;
            for (int x = 0; x < WorldWidth; x += tileSize)
                for (int y = 0; y < WorldHeight; y += tileSize)
                {
                    Color c = ((x / tileSize + y / tileSize) % 2 == 0)
                        ? new Color(30, 30, 30) : new Color(24, 24, 24);
                    sb.Draw(_pixel, new Rectangle(x, y, tileSize, tileSize), c);
                }

            // Bordas do mundo (linha de aviso)
            int border = 20;
            sb.Draw(_pixel, new Rectangle(0, 0, WorldWidth, border), Color.DarkRed * 0.6f);
            sb.Draw(_pixel, new Rectangle(0, WorldHeight - border, WorldWidth, border), Color.DarkRed * 0.6f);
            sb.Draw(_pixel, new Rectangle(0, 0, border, WorldHeight), Color.DarkRed * 0.6f);
            sb.Draw(_pixel, new Rectangle(WorldWidth - border, 0, border, WorldHeight), Color.DarkRed * 0.6f);
        }

        private void DrawHUD(SpriteBatch sb)
        {
            // Barra de HP
            int hbW = 200, hbH = 20;
            var hbBg = new Rectangle(20, Game.ScreenHeight - 40, hbW, hbH);
            var hbFg = new Rectangle(20, Game.ScreenHeight - 40,
                (int)(hbW * ((float)_player.Health / _player.MaxHealth)), hbH);
            sb.Draw(_pixel, hbBg, Color.DarkRed);
            sb.Draw(_pixel, hbFg, Color.LimeGreen);
            sb.DrawString(_font, $"HP  {_player.Health}/{_player.MaxHealth}",
                new Vector2(22, Game.ScreenHeight - 42), Color.White);

            // Munição
            string ammoStr = _player.IsReloading
                ? "RECARREGANDO..."
                : $"Munição: {_player.Ammo}/{_player.MaxAmmo}";
            var ammoSize = _font.MeasureString(ammoStr);
            sb.DrawString(_font, ammoStr,
                new Vector2(Game.ScreenWidth - ammoSize.X - 20, Game.ScreenHeight - 42),
                _player.IsReloading ? Color.Orange : Color.White);

            // Pontuação e onda
            sb.DrawString(_font, $"Pontuação: {_score}    Onda: {_waves.CurrentWave}/{WaveSystem.MaxWaves}",
                new Vector2(20, 20), Color.White);

            // Mini-mapa (canto superior direito) — mostra posição no mundo
            DrawMinimap(sb);
        }

        private void DrawMinimap(SpriteBatch sb)
        {
            int mmW = 120, mmH = 90;
            int mmX = Game.ScreenWidth - mmW - 10;
            int mmY = 10;

            // Fundo
            sb.Draw(_pixel, new Rectangle(mmX, mmY, mmW, mmH), Color.Black * 0.6f);

            // Posição do jogador no minimapa
            float px = _player.Position.X / WorldWidth * mmW;
            float py = _player.Position.Y / WorldHeight * mmH;
            sb.Draw(_pixel, new Rectangle(mmX + (int)px - 3, mmY + (int)py - 3, 6, 6), Color.LimeGreen);

            // Zombies no minimapa
            foreach (var z in _waves.Zombies)
            {
                float zx = z.Position.X / WorldWidth * mmW;
                float zy = z.Position.Y / WorldHeight * mmH;
                sb.Draw(_pixel, new Rectangle(mmX + (int)zx - 1, mmY + (int)zy - 1, 3, 3), Color.Red);
            }

            // Borda do minimapa
            sb.Draw(_pixel, new Rectangle(mmX, mmY, mmW, 1), Color.Gray);
            sb.Draw(_pixel, new Rectangle(mmX, mmY + mmH - 1, mmW, 1), Color.Gray);
            sb.Draw(_pixel, new Rectangle(mmX, mmY, 1, mmH), Color.Gray);
            sb.Draw(_pixel, new Rectangle(mmX + mmW - 1, mmY, 1, mmH), Color.Gray);
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
            sb.Draw(_pixel,
                new Rectangle(0, 0, Game.ScreenWidth, Game.ScreenHeight),
                Color.Black * 0.6f);

            string go = "GAME OVER";
            string sc = $"Pontuação final: {_score}";
            string hint = "ENTER para voltar ao menu";

            float cx = Game.ScreenWidth / 2f;
            float cy = Game.ScreenHeight / 2f;

            sb.DrawString(_font, go,
                new Vector2(cx - _font.MeasureString(go).X / 2, cy - 60), Color.OrangeRed);
            sb.DrawString(_font, sc,
                new Vector2(cx - _font.MeasureString(sc).X / 2, cy), Color.White);
            sb.DrawString(_font, hint,
                new Vector2(cx - _font.MeasureString(hint).X / 2, cy + 50), Color.Gray);
        }

        private void DrawVictory(SpriteBatch sb)
        {
            sb.Draw(_pixel,
                new Rectangle(0, 0, Game.ScreenWidth, Game.ScreenHeight),
                Color.Black * 0.7f);

            string title = "SOBREVIVESTE!";
            string sc = $"Pontuação final: {_score}";
            string hint = "ENTER para voltar ao menu";

            float cx = Game.ScreenWidth / 2f;
            float cy = Game.ScreenHeight / 2f;

            sb.DrawString(_font, title,
                new Vector2(cx - _font.MeasureString(title).X / 2, cy - 60), Color.Gold);
            sb.DrawString(_font, sc,
                new Vector2(cx - _font.MeasureString(sc).X / 2, cy), Color.White);
            sb.DrawString(_font, hint,
                new Vector2(cx - _font.MeasureString(hint).X / 2, cy + 50), Color.Gray);
        }
    }
}
