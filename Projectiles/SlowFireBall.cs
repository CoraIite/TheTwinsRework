using Coralite.Helpers;
using Terraria.ID;
using Terraria.ModLoader;
using TheTwinsRework.NPCs;

namespace TheTwinsRework.Projectiles
{
    public class SlowFireBall : ModProjectile
    {
        public override string Texture => AssetDirectory.Vanilla + "Projectile_96";

        public ref float CircleIndex => ref Projectile.ai[0];

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 18;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            if (!CircleIndex.GetNPCOwner<CircleLimit>(out NPC owner, Projectile.Kill))
                return;

            if (Vector2.Distance(Projectile.Center, owner.Center) > CircleLimit.MaxLength+100)
                Projectile.Kill();

            Projectile.rotation += 0.2f;

            for (int i = 0; i < 2; i++)
                Projectile.SpawnTrailDust(DustID.CursedTorch, Main.rand.NextFloat(-0.4f, -0.6f)
                    , Scale: Main.rand.NextFloat(1, 2));
        }

        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
            target.AddBuff(BuffID.CursedInferno, 60 * 3);

            Projectile.Kill();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Projectile.QuickDraw(lightColor, 0);

            return false;
        }
    }
}
