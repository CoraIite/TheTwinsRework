using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;

namespace TheTwinsRework.Compat.BossCheckList
{
    public static class TheTwinsPortrait
    {
        public static int frameCounter;
        public static int frame;

        public static void DrawPortrait(SpriteBatch spriteBatch, Rectangle rect, Color color)
        {
            Vector2 center = rect.Center();

            Texture2D mainTex = TextureAssets.Npc[125].Value;

            if (++frameCounter > 6)
            {
                frameCounter = 0;
                if (++frame > 2)
                    frame = 0;
            }

            Rectangle frameBox = mainTex.Frame(1, 6, 0, frame);
            Vector2 origin = frameBox.Size() / 2;

            float rot = Main.GlobalTimeWrappedHourly ;
            spriteBatch.Draw(mainTex, center + Main.GlobalTimeWrappedHourly.ToRotationVector2() * 80
                , frameBox, color, rot, origin, 1, SpriteEffects.None, 0f);

            spriteBatch.Draw(TextureAssets.Npc[126].Value, center + (Main.GlobalTimeWrappedHourly + MathHelper.Pi).ToRotationVector2() * 90
                , frameBox, color, rot + MathHelper.Pi, origin, 1, SpriteEffects.None, 0f);
        }
    }
}
