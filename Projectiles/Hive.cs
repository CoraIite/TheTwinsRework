using Coralite.Helpers;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TheTwinsRework.Core.System_Particle;
using TheTwinsRework.GlobalNPCs;
using TheTwinsRework.NPCs.QueenBee;
using TheTwinsRework.Particles;

namespace TheTwinsRework.Projectiles
{
    public class Hive : ModProjectile
    {
        public override string Texture => AssetDirectory.Vanilla + "Projectile_655";

        public ref float RectIndex => ref Projectile.ai[0];
        public ref float SpawnNPCType => ref Projectile.ai[1];
        public ref float Alpha => ref Projectile.localAI[1];
        public ref float Timer => ref Projectile.ai[2];


        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.tileCollide = false;
            Projectile.hostile = true;
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

            if (Projectile.soundDelay == 0)
            {
                if (SpawnNPCType != -1)
                {
                    NPC n = ContentSamples.NpcsByNetId[(int)SpawnNPCType];

                    float scale = MathHelper.Max(n.width / 32f, n.height / 32f);
                    if (scale < 1)
                        scale = 1;

                    Projectile.scale = scale;
                    int width = (int)(32 * scale);
                    Projectile.Resize(width, width);
                }

                Projectile.soundDelay = 100000;
                Helper.PlayPitched("bone_boulder_shake_n_break", 0.8f, 0, Projectile.Center);
            }

            Timer++;

            const int ReadyTime = 65;

            if (Timer < ReadyTime)
            {
                Lighting.AddLight(Projectile.Center, Timer / ReadyTime * new Vector3(1.2f, 1.2f, 0.1f));
                Projectile.velocity = Vector2.Zero;

                if (Timer % 6 == 0)
                {
                    Particle.NewParticle<VerticalLine>(Projectile.Center + Main.rand.NextVector2Circular(24, 24)
                        , new Vector2(0, 1), Scale: Main.rand.NextFloat(0.4f, 1.5f));
                }

                if (Timer % 4 == 0)
                {
                    Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(16, 16)
                          , DustID.JunglePlants, new Vector2(0, Main.rand.NextFloat(2, 5)), Scale: Main.rand.NextFloat(1.5f, 2.5f));
                    d.noGravity = true;
                }

                return;
            }

            //掉落
            Lighting.AddLight(Projectile.Center, new Vector3(1.2f, 1.2f, 0.1f) * Alpha);

            if (Timer < ReadyTime + 120)
            {
                Projectile.velocity = new Vector2(0, 15);
                if (Alpha < 1)
                {
                    Alpha += 0.1f;
                    if (Alpha > 1)
                        Alpha = 1;
                }

                if (Projectile.Bottom.Y + Projectile.velocity.Y > npc.Center.Y + RectangleLimit.LimitHeight / 2)
                {
                    Helper.PlayPitched(CoraliteSoundID.Fleshy_NPCDeath1, Projectile.Center);

                    if (SpawnNPCType != -1)
                    {
                        NPC n = NPC.NewNPCDirect(Projectile.GetSource_FromAI(), (int)Projectile.Center.X
                         , (int)Projectile.Center.Y, (int)SpawnNPCType);
                        n.velocity = new Vector2(0, -5);
                        n.SpawnedFromStatue = true;
                        n.value = 0;

                        if (n.boss)
                            n.life = n.lifeMax /= 5;

                        if (n.TryGetGlobalNPC(out SavageNPC snpc))
                            snpc.RectLimitIndex = (int)RectIndex;
                    }

                    Projectile.Kill();
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 30; i++)
            {
                int index = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.t_Honey);
                if (Main.rand.NextBool(2))
                    Main.dust[index].scale *= 1.4f;
            }

            if (Main.netMode != NetmodeID.MultiplayerClient && !Projectile.wet && SpawnNPCType == -1)
            {
                int spawnCount = 2;
                if (Main.rand.NextBool(3))
                    spawnCount++;

                if (Main.rand.NextBool(3))
                    spawnCount++;

                int availableAmountOfNPCsToSpawnUpToSlot = NPC.GetAvailableAmountOfNPCsToSpawnUpToSlot(spawnCount);
                Vector2 pos = Projectile.Top;
                for (int num576 = 0; num576 < availableAmountOfNPCsToSpawnUpToSlot; num576++)
                {
                    int type = Main.rand.Next(NPCID.Bee, NPCID.BeeSmall + 1);
                    int index = NPC.NewNPC(Projectile.GetSource_FromAI(), (int)pos.X, (int)pos.Y
                        , type, 1);
                    Main.npc[index].velocity.X = Main.rand.Next(-200, 201) * 0.002f;
                    Main.npc[index].velocity.Y = Main.rand.Next(-200, 201) * 0.002f;
                    Main.npc[index].netUpdate = true;
                    Main.npc[index].friendly = false;
                    if (Main.npc[index].TryGetGlobalNPC(out SavageNPC snpc))
                        snpc.RectLimitIndex = (int)RectIndex;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Color c = (Color.Yellow * Alpha) with { A = 0 };
            Projectile.QuickDraw(c, Projectile.scale * 1.2f, 0f);
            Projectile.QuickDraw(c, Projectile.scale * 1.2f, 0f);
            Projectile.QuickDraw(c, Projectile.scale * 1.1f, MathHelper.PiOver4 / 2);
            Projectile.QuickDraw(c, Projectile.scale * 1.1f, MathHelper.PiOver4 / 2);
            Projectile.QuickDraw(lightColor * Alpha, 0);

            return false;
        }
    }
}