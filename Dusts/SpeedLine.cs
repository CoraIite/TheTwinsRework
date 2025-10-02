using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;

namespace TheTwinsRework.Dusts
{
    public class SpeedLine : ModDust
    {
        public override string Texture => AssetDirectory.Assets + "Line";

        public override bool Update(Dust dust)
        {
            dust.fadeIn++;
            dust.rotation = dust.velocity.ToRotation();

            if (dust.fadeIn > 8)
            {
                dust.velocity *= 0.96f;
                dust.color *= 0.8f;
            }

            if (dust.color.A < 10)
                dust.active = false;

            dust.position += dust.velocity;
            return false;
        }

        public override bool PreDraw(Dust dust)
        {
            Texture2D mainTex = Texture2D.Value;
            Vector2 origin = mainTex.Size() / 2;
            var pos = dust.position - Main.screenPosition;
            Color c = dust.color * 0.5f;
            Main.spriteBatch.Draw(mainTex, pos, null, dust.color, dust.rotation, origin, dust.scale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(mainTex, pos, null, c, dust.rotation, origin, dust.scale*0.5f, SpriteEffects.None, 0f);

            return false;
        }
    }
}
