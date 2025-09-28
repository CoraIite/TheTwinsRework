using Coralite.Helpers;
using Terraria.ModLoader;

namespace TheTwinsRework.Dusts
{
    public class Circle : ModDust
    {
        public override string Texture => AssetDirectory.Assets + "CircleSPA";

        public override bool Update(Dust dust)
        {
            dust.scale = MathHelper.Lerp(dust.scale, 1.2f, 0.2f);

            dust.fadeIn++;
            if (dust.customData is int time && dust.fadeIn > time)
            {
                dust.color *= 0.9f;
            }

            if (dust.color.A < 10)
            {
                dust.active = false;
            }

            return false;
        }

        public override bool PreDraw(Dust dust)
        {
            Texture2D.Value.QuickCenteredDraw(Main.spriteBatch, dust.position - Main.screenPosition
                , dust.color, 0, dust.scale);

            Texture2D.Value.QuickCenteredDraw(Main.spriteBatch, dust.position - Main.screenPosition
                , dust.color with { A = 0 }, 0, dust.scale);

            return false;
        }
    }
}
