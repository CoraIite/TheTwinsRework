using Microsoft.Xna.Framework.Graphics;
using TheTwinsRework.Core.System_Particle;

namespace TheTwinsRework.Projectiles
{
    public class Scan : ModParticle
    {
        public override string Texture => AssetDirectory.Assets + Name;

        public override void Update(Particle particle)
        {
            particle.fadeIn++;
            particle.scale += 0.17f;
            particle.color *= 0.95f;
            if (particle.color.A < 10)
            {
                particle.active = false;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Particle particle)
        {
            Texture2D tex = Texture2D.Value;

            spriteBatch.Draw(tex, particle.center - Main.screenPosition, null, particle.color, particle.rotation, new Vector2(0, tex.Height / 2)
                , particle.scale, 0, 0);
        }
    }
}
