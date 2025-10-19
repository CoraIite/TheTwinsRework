using Coralite.Helpers;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TheTwinsRework.Core.System_Particle;
using TheTwinsRework.NPCs.QueenBee;
using TheTwinsRework.Particles;

namespace TheTwinsRework.Projectiles
{
    public class SpikeBallProj : ModProjectile
    {
        public override string Texture => AssetDirectory.Assets + Name;

        public ref float RectIndex => ref Projectile.ai[0];
        public ref float Alpha => ref Projectile.ai[1];
        public ref float Timer => ref Projectile.ai[2];

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 26;
            Projectile.tileCollide = false;
            Projectile.hostile = true;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override bool? CanDamage()
        {
            if (Timer < 65 || Timer > 65 + 120)
                return false;

            return null;
        }

        public override void AI()
        {
            if (!RectIndex.GetNPCOwner<RectangleLimit>(out NPC npc, Projectile.Kill))
            {
                return;
            }

            if (Projectile.soundDelay==0)
            {
                Projectile.soundDelay = 100000;
                Helper.PlayPitched("bone_boulder_shake_n_break", 0.8f, 0, Projectile.Center);
            }

            Timer++;

            const int ReadyTime = 65;

            if (Timer < ReadyTime)
            {
                Lighting.AddLight(Projectile.Center, Timer/ReadyTime*new Vector3(1.2f, 0.1f, 0.1f));
                Projectile.velocity = Vector2.Zero;

                if (Timer % 6 == 0)
                {
                    Particle.NewParticle<VerticalLine>(Projectile.Center + Main.rand.NextVector2Circular(24, 24)
                        , new Vector2(0, 1), Scale: Main.rand.NextFloat(0.4f, 1.5f));
                }

                if (Timer % 4 == 0)
                {
                    Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(16, 16)
                          , DustID.JunglePlants, new Vector2(0, Main.rand.NextFloat(2, 5)),Scale:Main.rand.NextFloat(1.5f,2.5f));
                    d.noGravity = true;
                }

                return;
            }

            //掉落
            Lighting.AddLight(Projectile.Center, new Vector3(1.2f, 0.1f, 0.1f)*Alpha);

            if (Timer < ReadyTime + 120)
            {
                Projectile.velocity = new Vector2(0, 15);
                if (Alpha < 1)
                {
                    Alpha += 0.1f;
                    if (Alpha > 1)
                        Alpha = 1;
                }
                Projectile.rotation += 0.1f;

                if (Projectile.Bottom.Y + Projectile.velocity.Y > npc.Center.Y + RectangleLimit.LimitHeight / 2)
                {
                    Helper.PlayPitched("bone_boulder_ground_impact", 0.8f, 0, Projectile.Center);

                    Timer = ReadyTime + 120;
                    Alpha = 1;
                    Projectile.velocity = new Vector2(Main.rand.NextFloat(-6, 6), -7);
                }

                return;
            }

            if (Timer < ReadyTime + 120 + 30)
            {
                Alpha -= 1 / 30f;
                if (Alpha < 0)
                    Alpha = 0;

                Projectile.rotation += 0.1f;

                Projectile.velocity.X *= 0.95f;
                if (Projectile.velocity.Y < 14)
                    Projectile.velocity.Y += 0.7f;

                return;
            }

            Projectile.Kill();
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

        public override bool PreDraw(ref Color lightColor)
        {
            Color c = (Color.Red * Alpha) with { A = 0 };
            Projectile.QuickDraw(c, Projectile.scale * 1.2f, 0f);
            Projectile.QuickDraw(c, Projectile.scale * 1.2f, 0f);
            Projectile.QuickDraw(c, Projectile.scale * 1.1f, MathHelper.PiOver4 / 2);
            Projectile.QuickDraw(c, Projectile.scale * 1.1f, MathHelper.PiOver4 / 2);
            Projectile.QuickDraw(lightColor * Alpha, 0);

            return false;
        }
    }
}
