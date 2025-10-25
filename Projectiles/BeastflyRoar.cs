using Coralite.Content.Particles;
using Coralite.Helpers;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace TheTwinsRework.Projectiles
{
    public class BeastflyRoar : ModProjectile
    {
        public override string Texture => AssetDirectory.Assets + "Blank";

        public static LocalizedText text;

        public override void Load()
        {
            if (Main.dedServ)
            {
                return;
            }
            text = this.GetLocalization("NothingHappen");
        }

        public override void SetDefaults()
        {
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            Projectile.Center = Main.player[Projectile.owner].Center;
            if (Projectile.ai[0] == 0)
            {
                Helper.PlayPitched("beastfly_scream_first", 1, 0, Projectile.Center);
            }

            Projectile.ai[0]++;
            if (Projectile.ai[0] % 10 == 0)
            {
                Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<RoaringWave>()
                    , Vector2.Zero, 0, Color.White * 0.8f, 0.4f);
            }

            if (Projectile.ai[0] > 150)
            {
                if (Main.netMode != NetmodeID.Server)
                    CombatText.NewText(new Rectangle((int)Projectile.Center.X, (int)Projectile.Center.Y, 1, 1)
                        , Color.LightGray, text.Value, true);
                Projectile.Kill();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }
    }
}
