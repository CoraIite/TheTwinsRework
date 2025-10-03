using Coralite.Helpers;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria.ModLoader;

namespace TheTwinsRework.Dusts
{
    public class Circle : ModDust
    {
        public override string Texture => AssetDirectory.Assets + "CircleSPA";

        public static Asset<Texture2D> LightBallTex;

        public override void Load()
        {
            if (Main.dedServ)
            {
                return;
            }

            LightBallTex = ModContent.Request<Texture2D>(AssetDirectory.Assets + "LightBall2");
        }

        public override bool Update(Dust dust)
        {
            dust.scale = MathHelper.Lerp(dust.scale, 1.2f, 0.2f);

            dust.fadeIn++;

            if (dust.customData is int time)
            {
                if (dust.fadeIn > time)
                {
                    dust.scale += 0.02f;
                    dust.color *= 0.9f;
                }
            }

            if (dust.color.A < 10)
            {
                dust.active = false;
            }

            return false;
        }

        public override bool PreDraw(Dust dust)
        {
            float rot = (int)Main.timeForVisualEffects * 0.05f;

            Texture2D.Value.QuickCenteredDraw(Main.spriteBatch, dust.position - Main.screenPosition
                , dust.color*0.75f, rot, dust.scale);

            Texture2D.Value.QuickCenteredDraw(Main.spriteBatch, dust.position - Main.screenPosition
                , dust.color with { A = 0 }*0.2f, -rot, dust.scale);

            if (dust.customData is int time)
            {
                float alpha = 1;
                if (dust.fadeIn < time)
                    alpha = Helper.SqrtEase(dust.fadeIn / time);

                float scale2 = 0.8f + 0.015f * MathF.Sin((int)Main.timeForVisualEffects * 0.35f);

                LightBallTex.Value.QuickCenteredDraw(Main.spriteBatch, dust.position - Main.screenPosition
                    , dust.color * alpha, rot, scale2);
            }

            return false;
        }
    }
}
