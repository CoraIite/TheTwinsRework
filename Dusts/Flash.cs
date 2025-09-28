using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria.ModLoader;

namespace TheTwinsRework.Dusts
{
    public class LightCiecleParticle : ModDust
    {
        public override string Texture => AssetDirectory.Assets + "HighlightCircleSPA";

        public static Asset<Texture2D> FadeCircle;

        public override void Load()
        {
            if (Main.dedServ)
                return;

            FadeCircle = ModContent.Request<Texture2D>(AssetDirectory.Assets + "FadeCircle");
        }

        public override void Unload()
        {
            FadeCircle = null;
        }

        public override bool Update(Dust dust)
        {
            dust.fadeIn++;
            if (dust.fadeIn > 10)
            {
                dust.color = Color.Lerp(dust.color, Color.Transparent, 0.35f);
                dust.scale += 0.03f;
            }
            else
            {
                dust.scale += 0.1f;
            }

            if (dust.color.A < 2)
                dust.active = false;

            return false;
        }

        public override bool PreDraw(Dust dust)
        {
            Texture2D mainTex = Texture2D.Value;

            Vector2 pos = dust.position - Main.screenPosition;
            Vector2 origin = mainTex.Size() / 2;
            Color c = dust.color;

            float scale = dust.scale;

            Main.spriteBatch.Draw(mainTex, pos
                , null, c, dust.rotation, origin, scale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(mainTex, pos
                , null, c, dust.rotation, origin, scale, SpriteEffects.None, 0f);

            Texture2D exTex = FadeCircle.Value;
            origin = exTex.Size() / 2;
            scale *= 0.6f;

            c = Color.White;
            c.A = 0;
            c *= dust.color.A / 255f*1.5f;

            Main.spriteBatch.Draw(exTex, pos
                , null, c, dust.rotation, origin, scale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(exTex, pos
                , null, c, dust.rotation, origin, scale * 0.5f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(exTex, pos
                , null, c, dust.rotation, origin, scale * 0.75f, SpriteEffects.None, 0f);

            return false;
        }
    }

    public class ScreenLightParticle : ModDust
    {
        public override string Texture => AssetDirectory.Assets + "LightBall";

        public override bool Update(Dust dust)
        {
            dust.fadeIn++;
            if (dust.fadeIn > 8)
            {
                dust.color = Color.Lerp(dust.color, new Color(100, 0, 0, 0), 0.2f);
            }
            else
            {
                //dust.color *= 1.03f;
                //dust.color.A += 3;
            }

            if (dust.color.A < 2)
                dust.active = false;

            return false;
        }

        public override bool PreDraw(Dust dust)
        {
            Texture2D mainTex = Texture2D.Value;
            Vector2 origin = mainTex.Size() / 2;

            Color c = dust.color * MathF.Sin(dust.fadeIn / 18 * MathHelper.Pi);
            c.A = 0;

            Main.spriteBatch.Draw(mainTex, dust.position - Main.screenPosition,
                 null, c, 0, origin, new Vector2(1.4f, 1) * dust.scale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(mainTex, dust.position - Main.screenPosition,
                 null, c, 0, origin, new Vector2(1.4f, 1) * dust.scale / 2, SpriteEffects.None, 0f);

            return false;
        }
    }
}
