using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using Terraria.ModLoader;

namespace TheTwinsRework.Dusts
{
    public class Scan : ModDust
    {
        public override string Texture => AssetDirectory.Assets + Name;

        public override bool Update(Dust dust)
        {
            dust.fadeIn++;
            dust.scale += 0.17f;
            dust.color *= 0.95f;
            if (dust.color.A < 10)
            {
                dust.active = false;
            }

            return false;
        }

        public override bool PreDraw(Dust dust)
        {
            Texture2D tex = Texture2D.Value;

            Main.spriteBatch.Draw(tex, dust.position - Main.screenPosition, null, dust.color, dust.rotation, new Vector2(0, tex.Height / 2)
                , dust.scale, 0, 0);

            return false;
        }
    }
}
