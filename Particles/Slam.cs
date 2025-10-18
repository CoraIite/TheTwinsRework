using Coralite.Helpers;
using Microsoft.Xna.Framework.Graphics;
using TheTwinsRework.Core.System_Particle;

namespace TheTwinsRework.Particles
{
    public class Slam : ModParticle
    {
        public override string Texture => AssetDirectory.Assets + Name;

        public override void OnSpawn(Particle particle)
        {
            particle.rotation = particle.velocity.ToRotation() + MathHelper.PiOver2;
        }

        public override bool ShouldUpdateCenter(Particle particle)
        {
            return false;
        }

        public override void Update(Particle particle)
        {
            particle.fadeIn++;
            if (particle.fadeIn > 4 * 2)
            {
                particle.active = false;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Particle particle)
        {
            Texture2D.Value.QuickCenteredDraw(spriteBatch
                , new Rectangle(0, (int)(particle.fadeIn / 2), 1, 5), particle.center - Main.screenPosition
                , Color.White, particle.rotation, particle.scale);
        }
    }
}
