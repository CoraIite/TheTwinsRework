using Coralite.Helpers;
using Terraria.ModLoader;

namespace TheTwinsRework.Dusts
{
    public class Fog : ModDust
    {
        public override string Texture => AssetDirectory.Assets + Name;

        public override void OnSpawn(Dust dust)
        {
            dust.rotation = Main.rand.NextFloat(6.282f);
            dust.frame = new Rectangle(0, Main.rand.Next(4),0,0);
        }

        public override bool Update(Dust dust)
        {
            dust.velocity *= 0.98f;
            dust.rotation += 0.01f;
            dust.scale *= 0.997f;
            dust.position += dust.velocity;

            dust.fadeIn++;
            if (dust.fadeIn>5)
                dust.color *= 0.94f;

            if (dust.color.A < 10)
                dust.active = false;
            return false;
        }

        public override bool PreDraw(Dust dust)
        {
            Texture2D.Value.QuickCenteredDraw(Main.spriteBatch, new Rectangle(0, dust.frame.Y, 1, 4)
                , dust.position - Main.screenPosition, 0, dust.color, dust.rotation, dust.scale);

            //Texture2D.Value.QuickCenteredDraw(Main.spriteBatch, new Rectangle(0, dust.frame.Y, 1, 4)
            //    , dust.position - Main.screenPosition, 0, dust.color with { A = 0 }*0.25f, dust.rotation, dust.scale);

            return false;
        }
    }
}
