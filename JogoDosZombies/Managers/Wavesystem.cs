using JogoDosZombies.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using JogoDosZombies.Entities;

namespace JogoDosZombies.Managers
{
    /// <summary>
    /// Manages zombie waves. Each wave spawns more and faster zombies.
    /// </summary>
    public class WaveSystem
    {
        public int CurrentWave { get; private set; } = 0;
        public int ZombiesLeft => _zombies.Count + _toSpawn;
        public bool WaveComplete => _toSpawn == 0 && _zombies.Count == 0;

        private List<Zombie> _zombies = new();
        private int _toSpawn = 0;
        private float _spawnTimer = 0f;
        private float _spawnDelay = 1.5f;  // seconds between spawns
        private Texture2D _zombieTex;
        private int _screenW, _screenH;
        private Random _rng = new();

        // Expose zombie list for collision checks
        public IReadOnlyList<Zombie> Zombies => _zombies;

        public WaveSystem(Texture2D zombieTex, int screenW, int screenH)
        {
            _zombieTex = zombieTex;
            _screenW = screenW;
            _screenH = screenH;
        }

        public void StartNextWave()
        {
            CurrentWave++;
            _toSpawn = 5 + CurrentWave * 3;   // 8, 11, 14 …
            _spawnTimer = 0f;
        }

        public void Update(GameTime gameTime, Vector2 playerPos, out int damageToPlayer)
        {
            damageToPlayer = 0;
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Spawn pending zombies
            if (_toSpawn > 0)
            {
                _spawnTimer -= dt;
                if (_spawnTimer <= 0)
                {
                    SpawnZombie();
                    _toSpawn--;
                    _spawnTimer = _spawnDelay;
                }
            }

            // Update alive zombies
            for (int i = _zombies.Count - 1; i >= 0; i--)
            {
                _zombies[i].Update(gameTime, playerPos, out bool attacked);
                if (attacked) damageToPlayer += _zombies[i].Damage;
                if (!_zombies[i].IsAlive) _zombies.RemoveAt(i);
            }
        }

        private void SpawnZombie()
        {
            // Spawn on a random screen edge
            Vector2 pos = _rng.Next(4) switch
            {
                0 => new Vector2(_rng.Next(_screenW), -20),                  // top
                1 => new Vector2(_rng.Next(_screenW), _screenH + 20),        // bottom
                2 => new Vector2(-20, _rng.Next(_screenH)),        // left
                _ => new Vector2(_screenW + 20, _rng.Next(_screenH)),       // right
            };

            float speed = 60f + CurrentWave * 8f;
            _zombies.Add(new Zombie(_zombieTex, pos, speed));
        }

        public void RemoveZombie(Zombie z) => _zombies.Remove(z);

        public void Draw(SpriteBatch sb)
        {
            foreach (var z in _zombies) z.Draw(sb);
        }
    }
}