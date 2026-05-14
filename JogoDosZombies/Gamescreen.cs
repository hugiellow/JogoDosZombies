using Microsoft.Xna.Framework;

namespace JogoDosZombies.Screens
{
    public abstract class GameScreen
    {
        protected Game1 Game;

        protected GameScreen(Game1 game) { Game = game; }

        public abstract void LoadContent();
        public abstract void UnloadContent();
        public abstract void Update(GameTime gameTime);
        public abstract void Draw(GameTime gameTime);
    }
}

