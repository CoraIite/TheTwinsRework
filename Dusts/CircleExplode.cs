using Coralite.Helpers;
using Terraria.ModLoader;

namespace TheTwinsRework.Dusts
{
    public class CircleExplode : ModDust
    {
        public override string Texture => AssetDirectory.Assets + Name;

        public override bool Update(Dust dust)
        {
            dust.fadeIn++;
            if (dust.fadeIn < 10)
            {
                dust.scale += 0.1f;
            }
            else
            {
                dust.color *= 0.9f;
                dust.scale += 0.04f;
            }

            if (dust.color.A < 10)
                dust.active = false;

            return false;
        }

        public override bool PreDraw(Dust dust)
        {
            Texture2D.Value.QuickCenteredDraw(Main.spriteBatch, dust.position - Main.screenPosition
                , dust.color, 0, dust.scale);

            return false;
        }
    }
}
