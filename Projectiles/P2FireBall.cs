using Coralite.Helpers;
using System;
using Terraria.ID;
using TheTwinsRework.NPCs.TheTwins;

namespace TheTwinsRework.Projectiles
{
    public class P2FireBall : SlowFireBall
    {
        public ref float Timer => ref Projectile.ai[2];

        public override void AI()
        {
            if (!CircleIndex.GetNPCOwner<CircleLimit>(out NPC owner, Projectile.Kill))
                return;

            if (Vector2.Distance(Projectile.Center, owner.Center) > CircleLimit.MaxLength + 100)
                Projectile.Kill();

            Projectile.rotation += 0.2f;

            Projectile.velocity = Projectile.velocity.RotatedBy(MathF.Sin(Timer * 0.35f) * 0.2f);
            Timer++;

            for (int i = 0; i < 2; i++)
                Projectile.SpawnTrailDust(DustID.CursedTorch, Main.rand.NextFloat(-0.4f, -0.6f)
                    , Scale: Main.rand.NextFloat(1, 2));
        }
    }
}
