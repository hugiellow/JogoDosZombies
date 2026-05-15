# JogoDosZombies
Um jogo de sobrevivência top-down desenvolvido em **MonoGame / C#**. Sobrevive a 10 ondas de zombies cada vez mais difíceis, com diferentes tipos de inimigos.

---

## 🎮 Gameplay
Controla um sobrevivente visto de cima. O objetivo é simples — elimina todos os zombies de cada onda sem morrer. A dificuldade aumenta progressivamente: os inimigos ficam mais rápidos, mais resistentes.

---

## 🕹️ Controlos
| Tecla / Ação | Função |
|---|---|
| `W A S D` | Mover o jogador |
| `Rato` | Apontar |
| `Botão esquerdo` | Disparar |
| `R` | Recarregar manualmente |
| `ESC` | Pausar / Retomar |

---


## 👾 Tipos de Zombies

| Tipo | Cor | Comportamento |
|---|---|---|
| **Normal** | Verde escuro | Normal|
| **Rápido** | Laranja | Muito veloz mas pouca vida |
| **Tank** | Roxo | Lento, muita vida e com dano elevado |
| **Spitter** | Verde-amarelo | Ataca à distância |

Os tipos de zombies são introduzidos gradualmente:

- **Ondas 1–2** — Normal
- **Ondas 3–4** — Normal, Rápido
- **Ondas 5–7** — Normal, Rápido, Tank
- **Ondas 8–10** — Todos os tipos


Cada tipo é criado através de um **factory method** que aplica os stats base:

```csharp
public static Zombie Create(Texture2D texture, Vector2 pos, ZombieType type, int wave)
{
    return type switch
    {
        ZombieType.Fast    => new Zombie(texture, pos, type,
                                speed:  130f + wave * 12f,
                                health: 30  + wave * 5,
                                damage: 8   + wave,
                                rate:   0.8f, size: 22),
        ZombieType.Tank    => new Zombie(texture, pos, type,
                                speed:  40f  + wave * 4f,
                                health: 180  + wave * 30,
                                damage: 25   + wave * 4,
                                rate:   1.5f, size: 42),

        ZombieType.Spitter => new Zombie(texture, pos, type,
                                speed:  55f  + wave * 5f,
                                health: 50   + wave * 10,
                                damage: 0, rate: 99f, size: 26),


        _ =>                  new Zombie(texture, pos, type,  // Normal

                                speed:  60f  + wave * 10f,

                                health: 60   + wave * 15,

                                damage: 10   + wave * 2,

                                rate:   Math.Max(0.4f, 1.0f - wave * 0.05f),

                                size:   30),

    };

}

```

---

## 📈 Dificuldade por Onda

Cada onda que passa, os zombies escalam automaticamente:

| Atributo | Onda 1 | Onda 5 | Onda 10 |
|---|---|---|---|
| Zombies spawned | 8 | 20 | 35 |
| Velocidade | 70 | 110 | 160 |
| Vida | 75 | 135 | 210 |
| Dano | 12 | 20 | 30 |
| Intervalo de spawn | 1.4s | 1.0s | 0.5s |


```

---

## 🤖 Separação entre Zombies

Para evitar que os zombies se sobreponham, cada um aplica uma força de repulsão em relação aos vizinhos próximos:

```csharp
private void Separate(List<Zombie> allZombies, float dt)
{
    float radius = _size + 5f;
    float force  = 80f;

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

```

---

## 🎯 Spitter — Ataque à Distância


O zombie Spitter mantém distância e dispara projéteis em direção ao jogador:

```csharp

private void UpdateSpitter(GameTime gameTime, Vector2 playerPos, float dt)

{
    float dist = Vector2.Distance(Position, playerPos);

    // Foge se o jogador está muito perto
    f (dist < SpitFleeRange)
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
}

```


---


## ❤️ Sistema de Vida

- O jogador começa com **100 HP**
- Ao completar uma onda, **recupera 20 HP** 
- Se morrer, aparece o ecrã de **Game Over** com a pontuação final
- Se completar as 10 ondas, aparece o ecrã de **Vitória**

```csharp

// Recuperação de vida entre ondas — nunca passa do máximo
public void Heal(int amount)
{
    Health = Math.Min(MaxHealth, Health + amount);
}

// Chamado no início de cada nova onda
private void StartNextWave()
{
    _player.Heal(20);
    _waitingForWave = true;
    _waveDelay = WaveDelayTime;
}

```


---


## 🏆 Pontuação

| Zombie eliminado | Pontos |
|---|---|
| Normal | 10 |
| Rápido | 15 |
| Spitter | 20 |
| Tank | 30 |


---

## ⏸️ Sistema de Pausa

A pausa bloqueia o `Update` do jogo mas continua a desenhar o overlay:

```csharp
protected override void Update(GameTime gameTime)
{
    var kb = Keyboard.GetState();

    if (kb.IsKeyDown(Keys.Escape) && _previousKeyboard.IsKeyUp(Keys.Escape))
        if (_currentScreen is not MenuScreen)
            _isPaused = !_isPaused;

    _previousKeyboard = kb;
    if (_isPaused) return; // Para tudo enquanto pausado

    _currentScreen?.Update(gameTime);
    base.Update(gameTime);
}

```

---


## 🔊 Sons
O jogo inclui efeitos sonoros para tiro, recarregamento, dano no jogador, morte de zombie, Game Over e Vitória. Os sons são carregados no `LoadContent` e distribuídos pelas entidades:

```csharp
var shootSound    = Game.Content.Load<SoundEffect>("Sounds/shoot");
var reloadSound   = Game.Content.Load<SoundEffect>("Sounds/reload");
var hurtSound     = Game.Content.Load<SoundEffect>("Sounds/dano");
_zombieDeathSound = Game.Content.Load<SoundEffect>("Sounds/zombie_death");
_gameOverSound    = Game.Content.Load<SoundEffect>("Sounds/gameover");
_victorySound     = Game.Content.Load<SoundEffect>("Sounds/win");
_player.LoadSounds(shootSound, reloadSound, hurtSound);

```


---


## 🗂️ Estrutura do Projeto


```

JogoDosZombies/

├── Core/
│   ├── Camera.cs             # Basicamente é a câmera
│   └── Game1.cs              # Loop principal, gestão de ecrãs, pausa
├── Entities/
│   ├── Player.cs             # Jogador, movimento, tiro, animações
│   ├── Zombie.cs             # Lógica dos zombies e separação entre eles
│   ├── ZombieType.cs         # Enum dos tipos de zombie
│   ├── Bullet.cs             # Projéteis do jogador
│   └── SpitterProjectile.cs  # Projéteis do zombie Spitter   
├── Managers/
│   └── WaveSystem.cs         # Gestão de ondas, spawn e dificuldade
├── Screens/
│   ├── GameScreen.cs         # Classe base dos ecrãs
│   ├── MenuScreen.cs         # Menu principal
│   └── PlayScreen.cs         # Ecrã de jogo principal
└── Content/
    └── Sounds/
        ├── shoot.wav
        ├── reload.wav
        ├── dano.wav
        ├── zombie_death.wav
        ├── gameover.wav
        └── win.wav

```


---


## 📋 Funcionalidades Ideias Futuras

- [ ] Power-ups (cura, munição, velocidade)
- [ ] Armas diferentes (espingarda, metralhadora)
- [ ] Boss a cada 5 ondas
- [ ] Obstáculos no mapa
- [ ] Registo de recordes

---

---

## 📢 Créditos

* **Efeitos Sonoros:** Todos os áudios utilizados no jogo (tiros, recarregamento, danos, etc.) foram descarregados da plataforma [Freesound](https://freesound.org/).

---

## 👤 Autor
| [Hugo Alexandre Dias Rodrigues] | [31480] |


**Unidade Curricular:** Técnicas de Desenvolvimento de Videojogos  
**Curso:** [EDJD]  
**Instituição:** IPCA — Instituto Politécnico do Cávado e do Ave
