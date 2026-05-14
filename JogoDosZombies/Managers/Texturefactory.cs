using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JogoDosZombies.Managers
{
    /// <summary>
    /// Gera spritesheets programaticamente para que o jogo não precise
    /// de ficheiros de imagem externos.
    /// Cada spritesheet é uma textura horizontal com N frames de frameSize×frameSize.
    /// </summary>
    public static class TextureFactory
    {
        // ------------------------------------------------------------------ //
        //  JOGADOR — 4 frames de "andar" desenhados com círculo + pernas      //
        // ------------------------------------------------------------------ //
        public static Texture2D CreatePlayerSheet(GraphicsDevice gd,
                                                   int frameSize = 48,
                                                   int frames = 4)
        {
            int w = frameSize * frames;
            int h = frameSize;
            var data = new Color[w * h];

            for (int f = 0; f < frames; f++)
            {
                int ox = f * frameSize;
                int cx = frameSize / 2;
                int cy = frameSize / 2;

                // Corpo (círculo verde)
                FillCircle(data, w, ox + cx, cy, 14, new Color(60, 200, 60));

                // Capacete (arco escuro no topo)
                FillCircle(data, w, ox + cx, cy - 6, 8, new Color(30, 100, 30));

                // Mira / cano (rectângulo que muda por frame para simular brilho)
                int barLen = 10 + f * 2;
                int barThk = 3;
                FillRect(data, w, h, ox + cx, cy - barThk / 2, barLen, barThk,
                         new Color(200, 200, 50));

                // Pernas (dois rectângulos que alternam posição)
                int legOff = f % 2 == 0 ? 4 : -4;
                FillRect(data, w, h, ox + cx - 8, cy + 12 + legOff, 6, 10, new Color(40, 140, 40));
                FillRect(data, w, h, ox + cx + 3, cy + 12 - legOff, 6, 10, new Color(40, 140, 40));
            }

            var tex = new Texture2D(gd, w, h);
            tex.SetData(data);
            return tex;
        }

        // ------------------------------------------------------------------ //
        //  ZOMBIE NORMAL — 4 frames, cor verde putrefacta                     //
        // ------------------------------------------------------------------ //
        public static Texture2D CreateZombieSheet(GraphicsDevice gd,
                                                   int frameSize = 40,
                                                   int frames = 4)
        {
            int w = frameSize * frames;
            int h = frameSize;
            var data = new Color[w * h];

            for (int f = 0; f < frames; f++)
            {
                int ox = f * frameSize;
                int cx = frameSize / 2;
                int cy = frameSize / 2;

                // Corpo
                FillCircle(data, w, ox + cx, cy, 13, new Color(50, 110, 30));
                // Cara mais clara
                FillCircle(data, w, ox + cx, cy - 3, 7, new Color(80, 150, 50));
                // Olhos vermelhos
                FillRect(data, w, h, ox + cx - 6, cy - 6, 4, 4, Color.Red);
                FillRect(data, w, h, ox + cx + 2, cy - 6, 4, 4, Color.Red);
                // Boca (linha escura)
                FillRect(data, w, h, ox + cx - 4, cy + 1, 8, 2, new Color(20, 40, 10));

                // Braços a balançar
                int armOff = f % 2 == 0 ? -5 : 5;
                FillRect(data, w, h, ox + cx - 18, cy - 2 + armOff, 8, 4, new Color(50, 110, 30));
                FillRect(data, w, h, ox + cx + 10, cy - 2 - armOff, 8, 4, new Color(50, 110, 30));

                // Pernas
                int legOff = f % 2 == 0 ? 3 : -3;
                FillRect(data, w, h, ox + cx - 7, cy + 11 + legOff, 5, 9, new Color(40, 90, 25));
                FillRect(data, w, h, ox + cx + 2, cy + 11 - legOff, 5, 9, new Color(40, 90, 25));
            }

            var tex = new Texture2D(gd, w, h);
            tex.SetData(data);
            return tex;
        }

        // ------------------------------------------------------------------ //
        //  ZOMBIE RÁPIDO — mais pequeno, cor alaranjada                       //
        // ------------------------------------------------------------------ //
        public static Texture2D CreateFastZombieSheet(GraphicsDevice gd,
                                                       int frameSize = 32,
                                                       int frames = 4)
        {
            int w = frameSize * frames;
            int h = frameSize;
            var data = new Color[w * h];

            for (int f = 0; f < frames; f++)
            {
                int ox = f * frameSize;
                int cx = frameSize / 2;
                int cy = frameSize / 2;

                FillCircle(data, w, ox + cx, cy, 10, new Color(180, 80, 20));
                FillCircle(data, w, ox + cx, cy - 2, 6, new Color(220, 120, 50));
                FillRect(data, w, h, ox + cx - 4, cy - 4, 3, 3, Color.OrangeRed);
                FillRect(data, w, h, ox + cx + 1, cy - 4, 3, 3, Color.OrangeRed);

                int legOff = f % 2 == 0 ? 4 : -4;
                FillRect(data, w, h, ox + cx - 5, cy + 9 + legOff, 4, 7, new Color(160, 60, 15));
                FillRect(data, w, h, ox + cx + 1, cy + 9 - legOff, 4, 7, new Color(160, 60, 15));
            }

            var tex = new Texture2D(gd, w, h);
            tex.SetData(data);
            return tex;
        }

        // ------------------------------------------------------------------ //
        //  ZOMBIE TANQUE — maior, cor cinzenta                                //
        // ------------------------------------------------------------------ //
        public static Texture2D CreateTankZombieSheet(GraphicsDevice gd,
                                                       int frameSize = 56,
                                                       int frames = 4)
        {
            int w = frameSize * frames;
            int h = frameSize;
            var data = new Color[w * h];

            for (int f = 0; f < frames; f++)
            {
                int ox = f * frameSize;
                int cx = frameSize / 2;
                int cy = frameSize / 2;

                FillCircle(data, w, ox + cx, cy, 20, new Color(80, 80, 80));
                FillCircle(data, w, ox + cx, cy - 4, 11, new Color(110, 110, 110));
                FillRect(data, w, h, ox + cx - 8, cy - 8, 5, 5, Color.DarkRed);
                FillRect(data, w, h, ox + cx + 3, cy - 8, 5, 5, Color.DarkRed);

                int legOff = f % 2 == 0 ? 2 : -2;
                FillRect(data, w, h, ox + cx - 10, cy + 17 + legOff, 8, 12, new Color(60, 60, 60));
                FillRect(data, w, h, ox + cx + 2, cy + 17 - legOff, 8, 12, new Color(60, 60, 60));

                // Braços grossos
                FillRect(data, w, h, ox + cx - 24, cy - 2, 10, 6, new Color(80, 80, 80));
                FillRect(data, w, h, ox + cx + 14, cy - 2, 10, 6, new Color(80, 80, 80));
            }

            var tex = new Texture2D(gd, w, h);
            tex.SetData(data);
            return tex;
        }

        // ------------------------------------------------------------------ //
        //  BALA — 2 frames (brilho alternado)                                //
        // ------------------------------------------------------------------ //
        public static Texture2D CreateBulletSheet(GraphicsDevice gd)
        {
            int fw = 12, fh = 6, frames = 2;
            int w = fw * frames;
            var data = new Color[w * fh];

            // Frame 0 — amarelo vivo
            FillRect(data, w, fh, 0, 0, fw, fh, Color.Yellow);
            // Frame 1 — branco (brilho)
            FillRect(data, w, fh, fw, 0, fw, fh, Color.White);

            var tex = new Texture2D(gd, w, fh);
            tex.SetData(data);
            return tex;
        }

        // ------------------------------------------------------------------ //
        //  Helpers                                                            //
        // ------------------------------------------------------------------ //
        private static void FillCircle(Color[] data, int texW,
                                        int cx, int cy, int r, Color c)
        {
            int h = data.Length / texW;
            for (int dy = -r; dy <= r; dy++)
                for (int dx = -r; dx <= r; dx++)
                {
                    if (dx * dx + dy * dy <= r * r)
                    {
                        int px = cx + dx;
                        int py = cy + dy;
                        if (px >= 0 && px < texW && py >= 0 && py < h)
                            data[py * texW + px] = c;
                    }
                }
        }

        private static void FillRect(Color[] data, int texW, int texH,
                                      int x, int y, int rw, int rh, Color c)
        {
            for (int dy = 0; dy < rh; dy++)
                for (int dx = 0; dx < rw; dx++)
                {
                    int px = x + dx, py = y + dy;
                    if (px >= 0 && px < texW && py >= 0 && py < texH)
                        data[py * texW + px] = c;
                }
        }
    }
}