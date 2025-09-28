using Terraria.ID;
using Terraria.ModLoader;

namespace TheTwinsRework.Projectiles
{
    public class Boom : ModProjectile
    {
        public override string Texture => AssetDirectory.Assets + "Blank";

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 300;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 10;
            Projectile.hostile = true;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }
    }
}
