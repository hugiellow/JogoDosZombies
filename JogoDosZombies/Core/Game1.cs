using JogoDosZombies.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace JogoDosZombies.Core
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        public SpriteBatch SpriteBatch;
        private Texture2D _pixelTexture;
        private SpriteFont _font;

        private GameScreen _currentScreen;
        public static Game1 Instance;

        private bool _isPaused = false;
        private KeyboardState _previousKeyboard;

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
            _previousKeyboard = Keyboard.GetState();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            _pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });

            // Carrega a font — o nome tem de corresponder ao ficheiro no Content Pipeline
            _font = Content.Load<SpriteFont>("MenuFont");

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
            var kb = Keyboard.GetState();

            // Toggle pausa com ESC (só funciona se não estiver no MenuScreen)
            if (kb.IsKeyDown(Keys.Escape) && _previousKeyboard.IsKeyUp(Keys.Escape))
            {
                if (_currentScreen is not MenuScreen)
                    _isPaused = !_isPaused;
            }

            _previousKeyboard = kb;

            if (_isPaused)
                return;

            _currentScreen?.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _currentScreen?.Draw(gameTime);

            if (_isPaused)
            {
                SpriteBatch.Begin();

                // Overlay escuro
                SpriteBatch.Draw(
                    _pixelTexture,
                    new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height),
                    Color.Black * 0.5f
                );

                // Texto "PAUSADO"
                Vector2 textSize = _font.MeasureString("PAUSADO");
                SpriteBatch.DrawString(
                    _font,
                    "PAUSADO",
                    new Vector2(
                        GraphicsDevice.Viewport.Width / 2f - textSize.X / 2f,
                        GraphicsDevice.Viewport.Height / 2f - textSize.Y / 2f
                    ),
                    Color.White
                );

                // Instrução para continuar
                Vector2 hintSize = _font.MeasureString("Prima ESC para continuar");
                SpriteBatch.DrawString(
                    _font,
                    "Prima ESC para continuar",
                    new Vector2(
                        GraphicsDevice.Viewport.Width / 2f - hintSize.X / 2f,
                        GraphicsDevice.Viewport.Height / 2f + 40
                    ),
                    Color.LightGray
                );

                SpriteBatch.End();
            }

            base.Draw(gameTime);
        }
    }
}