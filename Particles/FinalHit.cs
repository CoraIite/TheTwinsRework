using Coralite.Helpers;
using Microsoft.Xna.Framework.Graphics;
using TheTwinsRework.Core.System_Particle;

namespace TheTwinsRework.Particles
{
    public class FinalHit : ModParticle
    {
        public override string Texture => AssetDirectory.Assets + Name;

        public override void OnSpawn(Particle particle)
        {
            particle.frame.Y = 0;
        }

        public override void Update(Particle particle)
        {
            particle.fadeIn++;
            particle.scale += 0.01f;

            if (particle.fadeIn < 15)
            {
                particle.scale += 0.05f;
            }
            else if (particle.fadeIn<25)
            {
                particle.scale += 0.025f;
            }
            if (particle.fadeIn == 15)
            {
                particle.frame.Y++;
            }
            else if (particle.fadeIn > 25)
            {
                particle.active = false;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Particle particle)
        {
            Texture2D.Value.QuickCenteredDraw(spriteBatch, new Rectangle(0, particle.frame.Y, 1, 2), particle.center - Main.screenPosition
                , Color.White, 0, particle.scale, 0);
        }
    }
}
