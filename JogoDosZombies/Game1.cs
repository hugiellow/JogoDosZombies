using JogoDosZombies.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using JogoDosZombies.Screens;

namespace JogoDosZombies
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        public SpriteBatch SpriteBatch;

        // Screen management
        private GameScreen _currentScreen;
        public static Game1 Instance;

        public int ScreenWidth => _graphics.PreferredBackBufferWidth;
        public int ScreenHeight => _graphics.PreferredBackBufferHeight;

        public Game1()
        {
            Instance = this;
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            // Start with the menu screen
            ChangeScreen(new MenuScreen(this));
        }

        public void ChangeScreen(GameScreen screen)
        {
            _currentScreen?.UnloadContent();
            _currentScreen = screen;
            _currentScreen.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape) && _currentScreen is not MenuScreen)
                ChangeScreen(new MenuScreen(this));

            _currentScreen?.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _currentScreen?.Draw(gameTime);
            base.Draw(gameTime);
        }
    }
}