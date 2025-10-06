using Coralite.Helpers;
using Terraria.ID;
using Terraria.ModLoader;
using TheTwinsRework.NPCs.TheTwins;

namespace TheTwinsRework.Projectiles
{
    public class DeathLaserNoTileCollide:ModProjectile
    {
        public override string Texture => AssetDirectory.Vanilla+"Projectile_83";

        public ref float CircleIndex => ref Projectile.ai[0];

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.EyeLaser);
            Projectile.tileCollide = false;
            AIType = ProjectileID.EyeLaser;
        }

        public override bool PreAI()
        {
            if (!CircleIndex.GetNPCOwner<CircleLimit>(out NPC owner, Projectile.Kill))
                return false;

            if (Vector2.Distance(Projectile.Center, owner.Center) > CircleLimit.MaxLength + 100)
                Projectile.Kill();

            return base.PreAI();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            lightColor = Color.White;
            return base.PreDraw(ref lightColor);
        }
    }
}
