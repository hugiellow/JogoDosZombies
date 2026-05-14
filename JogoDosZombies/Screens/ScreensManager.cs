using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

// O "Molde" para todos os ecrãs
public abstract class Screen
{
    public abstract void Update(GameTime gameTime);
    public abstract void Draw(SpriteBatch spriteBatch);
}

// O Gestor que controla qual ecrã está ativo
public static class ScreenManager
{
    public static Screen CurrentScreen { get; private set; }

    public static void LoadScreen(Screen screen)
    {
        CurrentScreen = screen;
    }

    public static void Update(GameTime gameTime)
    {
        CurrentScreen?.Update(gameTime);
    }

    public static void Draw(SpriteBatch spriteBatch)
    {
        CurrentScreen?.Draw(spriteBatch);
    }
}