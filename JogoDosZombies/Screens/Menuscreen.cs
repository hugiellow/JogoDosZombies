using JogoDosZombies.Core;
using JogoDosZombies.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace JogoDosZombies.Screens
{
    public class MenuScreen : GameScreen
    {
        private SpriteFont _font;
        private SpriteFont _titleFont;
        private KeyboardState _prevKeys;
        private float _pulse;

        public MenuScreen(Game1 game) : base(game) { }

        public override void LoadContent()
        {
            _font = Game.Content.Load<SpriteFont>("MenuFont");
            _titleFont = Game.Content.Load<SpriteFont>("TitleFont");
        }

        public override void UnloadContent() { }

        public override void Update(GameTime gameTime)
        {
            _pulse += (float)gameTime.ElapsedGameTime.TotalSeconds * 2f;

            var keys = Keyboard.GetState();
            if (keys.IsKeyDown(Keys.Enter) && _prevKeys.IsKeyUp(Keys.Enter))
                Game.ChangeScreen(new PlayScreen(Game));
            _prevKeys = keys;
        }

        public override void Draw(GameTime gameTime)
        {
            var sb = Game.SpriteBatch;
            sb.Begin();

            // Title
            string title = "JOGOS DOS ZOMBIE";
            var titleSize = _titleFont.MeasureString(title);
            float x = (Game.ScreenWidth - titleSize.X) / 2f;
            float y = Game.ScreenHeight * 0.25f;
            sb.DrawString(_titleFont, title, new Vector2(x, y), Color.OrangeRed);

            // Pulsing prompt
            float alpha = (MathF.Sin(_pulse) + 1f) / 2f;
            string prompt = "Pressiona ENTER para jogar";
            var promptSize = _font.MeasureString(prompt);
            sb.DrawString(_font, prompt,
                new Vector2((Game.ScreenWidth - promptSize.X) / 2f, Game.ScreenHeight * 0.55f),
                Color.White * alpha);

            // Controls hint
            string controls =
                "[WASD] Mover    [LMB] Atirar    [ESC] Menu";
            var ctrlSize = _font.MeasureString(controls);
            sb.DrawString(_font, controls,
                new Vector2((Game.ScreenWidth - ctrlSize.X) / 2f, Game.ScreenHeight * 0.75f),
                Color.Gray);

            sb.End();
        }
    }
}
