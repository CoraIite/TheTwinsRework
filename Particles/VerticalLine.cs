using Microsoft.Xna.Framework.Graphics;
using System;
using TheTwinsRework.Core.System_Particle;

namespace TheTwinsRework.Particles
{
    public class VerticalLine:ModParticle
    {
        public override string Texture => AssetDirectory.Assets+Name;

        public override bool ShouldUpdateCenter(Particle particle)
        {
            return false;
        }

        public override void OnSpawn(Particle particle)
        {
            base.OnSpawn(particle);
        }

        public override void Update(Particle particle)
        {
            particle.fadeIn++;
            if (particle.fadeIn > 22)
            {
                particle.active = false;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Particle particle)
        {
            Color c = Color.White*0.5f;
            if (particle.fadeIn < 8)
            {
                c *= (particle.fadeIn / 8);
            }
            if (particle.fadeIn>14)
            {
                c *= (1-(particle.fadeIn - 14) / 8);
            }

            spriteBatch.Draw(Texture2D.Value, particle.center - Main.screenPosition
                , null, c, 0, new Vector2(Texture2D.Width() / 2, 0)
                , new Vector2(1, particle.scale) * MathF.Sin(particle.fadeIn/22 * MathHelper.Pi), 0, 0);
        }
    }
}
