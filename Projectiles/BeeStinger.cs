using Coralite.Helpers;
using Terraria.ID;
using Terraria.ModLoader;
using TheTwinsRework.NPCs.QueenBee;

namespace TheTwinsRework.Projectiles
{
    public class BeeStinger:ModProjectile
    {
        public override string Texture => AssetDirectory.Vanilla+"Projectile_719";

        public ref float RectIndex => ref Projectile.ai[0];
        /// <summary>
        /// QB
        /// </summary>
        public ref float QBIndex => ref Projectile.ai[1];

        public override void SetStaticDefaults()
        {
            Projectile.QuickTrailSets(Helper.TrailingMode.RecordAll, 6);
        }

        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.hostile = true;

            Projectile.width = Projectile.height = 8;
            Projectile.tileCollide = false;
        }

        public override bool? CanHitNPC(NPC target)
        {
            if (target.whoAmI == QBIndex)
                return false;
            return base.CanHitNPC(target);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!target.friendly)
            {
                if (target.boss)
                    modifiers.SourceDamage += 2.5f;
                else
                    modifiers.SourceDamage += 5;
            }
        }

        public override void AI()
        {
            if (!RectIndex.GetNPCOwner<RectangleLimit>(out NPC owner, Projectile.Kill))
                return;

            if (Projectile.Center.X<owner.Center.X-RectangleLimit.LimitWidth/2
                || Projectile.Center.X > owner.Center.X + RectangleLimit.LimitWidth / 2
                || Projectile.Center.Y < owner.Center.Y - RectangleLimit.LimitHeight / 2
                || Projectile.Center.Y > owner.Center.Y + RectangleLimit.LimitHeight / 2)
            {
                Projectile.Kill();
            }

            if (Main.rand.NextBool())
                Projectile.SpawnTrailDust(DustID.Honey, Main.rand.NextFloat(-0.2f, -0.4f)
                    , Alpha: Main.rand.Next(100,175), Scale: Main.rand.NextFloat(1, 2));

            Projectile.rotation = Projectile.velocity.ToRotation();

            if (Projectile.velocity.Y<8)
            {
                Projectile.velocity.Y += 0.075f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Projectile.DrawShadowTrails(Color.Gold with { A = 0 }, 0.5f, 0.5f / 6, 1, 6, 1, MathHelper.PiOver2, -1);
            Projectile.QuickDraw((Color.Gold * 0.1f) with { A = 0 }, 2.5f, MathHelper.PiOver2);
            Projectile.QuickDraw((Color.Gold * 0.3f) with { A = 0 }, 2f, MathHelper.PiOver2);
            Projectile.QuickDraw((Color.Gold * 0.5f) with { A = 0 }, 1.6f, MathHelper.PiOver2);
            Projectile.QuickDraw(lightColor, MathHelper.PiOver2);
            return false;
        }
    }
}
