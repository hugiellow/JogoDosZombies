using JogoDosZombies.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace JogoDosZombies.Managers
{
    public class WaveSystem
    {
        // ---- Config ----
        public const int MaxWaves = 10;

        // ---- State ----
        public int CurrentWave { get; private set; } = 0;
        public bool WaveComplete => _toSpawn == 0 && _zombies.Count == 0;
        public bool GameWon => CurrentWave > MaxWaves && WaveComplete;

        private List<Zombie> _zombies = new();
        private int _toSpawn = 0;
        private float _spawnTimer = 0f;
        private float _spawnDelay = 1.5f;

        private Texture2D _zombieTex;
        private int _screenW, _screenH;
        private Random _rng = new();

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
            _zombies.Clear();

            // Mais zombies por onda
            _toSpawn = 5 + CurrentWave * 3;

            // Spawn mais rápido nas ondas avançadas
            _spawnDelay = Math.Max(0.4f, 1.5f - CurrentWave * 0.1f);

            _spawnTimer = 0f;
        }

        public void Update(GameTime gameTime, Vector2 playerPos, out int damageToPlayer)
        {
            damageToPlayer = 0;
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

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
            for (int i = _zombies.Count - 1; i >= 0; i--)
            {
                _zombies[i].Update(gameTime, playerPos, _zombies, out bool attacked);
                if (attacked) damageToPlayer += _zombies[i].Damage;
                if (!_zombies[i].IsAlive) _zombies.RemoveAt(i);
            }
        }

        private void SpawnZombie()
        {
            Vector2 pos = _rng.Next(4) switch
            {
                0 => new Vector2(_rng.Next(_screenW), -20),
                1 => new Vector2(_rng.Next(_screenW), _screenH + 20),
                2 => new Vector2(-20, _rng.Next(_screenH)),
                _ => new Vector2(_screenW + 20, _rng.Next(_screenH)),
            };

            ZombieType type = PickType();
            _zombies.Add(Zombie.Create(_zombieTex, pos, type, CurrentWave));
        }

        private ZombieType PickType()
        {
            // Antes da onda 3: só normais
            if (CurrentWave < 3) return ZombieType.Normal;

            // Probabilidades mudam com a onda
            int roll = _rng.Next(100);

            if (CurrentWave < 5)
            {
                // Onda 3-4: Normal + Rápido
                return roll < 70 ? ZombieType.Normal : ZombieType.Fast;
            }
            else if (CurrentWave < 8)
            {
                // Onda 5-7: Normal, Rápido, Tank
                if (roll < 50) return ZombieType.Normal;
                if (roll < 75) return ZombieType.Fast;
                return ZombieType.Tank;
            }
            else
            {
                // Onda 8+: todos os tipos
                if (roll < 35) return ZombieType.Normal;
                if (roll < 60) return ZombieType.Fast;
                if (roll < 80) return ZombieType.Tank;
                return ZombieType.Spitter;
            }
        }

        public void Draw(SpriteBatch sb)
        {
            foreach (var z in _zombies) z.Draw(sb);
        }
    }
}