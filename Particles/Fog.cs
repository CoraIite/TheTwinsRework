using Coralite.Helpers;
using Microsoft.Xna.Framework.Graphics;
using TheTwinsRework.Core.System_Particle;

namespace TheTwinsRework.Particles
{
    public class Fog : ModParticle
    {
        public override string Texture => AssetDirectory.Assets + Name;

        public override void OnSpawn(Particle particle)
        {
            particle.rotation = Main.rand.NextFloat(6.282f);
            particle.frame = new Rectangle(0, Main.rand.Next(4),0,0);
        }

        public override void Update(Particle particle)
        {
            particle.velocity *= 0.98f;
            particle.rotation += 0.01f;
            particle.scale *= 0.997f;

            particle.fadeIn++;
            if (particle.fadeIn>5)
                particle.color *= 0.94f;

            if (particle.color.A < 10)
                particle.active = false;
        }

        public override void Draw(SpriteBatch spriteBatch, Particle particle)
        {
            Texture2D.Value.QuickCenteredDraw(spriteBatch, new Rectangle(0, particle.frame.Y, 1, 4)
                , particle.center - Main.screenPosition, 0, particle.color, particle.rotation, particle.scale);

            //Texture2D.Value.QuickCenteredDraw(Main.spriteBatch, new Rectangle(0, dust.frame.Y, 1, 4)
            //    , dust.position - Main.screenPosition, 0, dust.color with { A = 0 }*0.25f, dust.rotation, dust.scale);
        }
    }
}
