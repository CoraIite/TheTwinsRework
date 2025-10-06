using Coralite.Helpers;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria.ModLoader;
using TheTwinsRework.Core.System_Particle;

namespace TheTwinsRework.Particles
{
    public class Circle : ModParticle
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

        public override void Update(Particle particle)
        {
            particle.scale = MathHelper.Lerp(particle.scale, 1.2f, 0.2f);

            particle.fadeIn++;

            if (particle.data is int time)
            {
                if (particle.fadeIn > time)
                {
                    particle.scale += 0.02f;
                    particle.color *= 0.9f;
                }
            }

            if (particle.color.A < 10)
            {
                particle.active = false;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Particle particle)
        {
            float rot = (int)Main.timeForVisualEffects * 0.05f;

            Texture2D.Value.QuickCenteredDraw(spriteBatch, particle.center - Main.screenPosition
                , particle.color*0.75f, rot, particle.scale);

            Texture2D.Value.QuickCenteredDraw(spriteBatch, particle.center - Main.screenPosition
                , particle.color with { A = 0 }*0.2f, -rot, particle.scale);

            if (particle.data is int time)
            {
                float alpha = 1;
                if (particle.fadeIn < time)
                    alpha = Helper.SqrtEase(particle.fadeIn / time);

                float scale2 = 0.8f + 0.015f * MathF.Sin((int)Main.timeForVisualEffects * 0.35f);

                LightBallTex.Value.QuickCenteredDraw(Main.spriteBatch, particle.center - Main.screenPosition
                    , particle.color * alpha, rot, scale2);
            }
        }
    }
}
