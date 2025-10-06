using Coralite.Helpers;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using TheTwinsRework.NPCs.TheTwins;

namespace TheTwinsRework.Projectiles
{
    /// <summary>
    /// ai0环索引，ai1射击时间,ai1消失时间
    /// </summary>
    public class FireBreath : ModProjectile
    {
        public override string Texture => AssetDirectory.Assets+"Blank";

        public ref float CircleIndex => ref Projectile.ai[0];
        public ref float ShootTime => ref Projectile.ai[1];
        public ref float FadeTime => ref Projectile.ai[2];

        public ref float Timer => ref Projectile.localAI[0];

        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.aiStyle = -1;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.alpha = 255;
            Projectile.penetrate = 1;
            Projectile.extraUpdates = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.tileCollide = false;
        }

        public override bool? CanDamage()
        {
            if (Timer>ShootTime)
            {
                return false;
            }

            return null;
        }

        public override void AI()
        {
            if (!CircleIndex.GetNPCOwner(out NPC circle, Projectile.Kill))
                return;

            if (Timer < ShootTime && Vector2.Distance(Projectile.Center, circle.Center) > CircleLimit.MaxLength)
            {
                Timer = ShootTime;
            }

            Timer += 1f;
            int totalTime = (int)ShootTime + (int)FadeTime;
            if (Timer >= totalTime)
                Projectile.Kill();

            if (Timer >= ShootTime)
                Projectile.velocity *= 0.95f;

            Lighting.AddLight(Projectile.Center, Color.MediumPurple.ToVector3());

            if (Timer < ShootTime && Main.rand.NextFloat() < 0.25f)
            {
                short type = DustID.CursedTorch;
                Dust dust = Dust.NewDustDirect(Projectile.Center + (Main.rand.NextVector2Circular(60f, 60f) * Utils.Remap(Projectile.localAI[0], 0f, 72f, 0.5f, 1f)), 4, 4, type, Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f, 100);
                if (Main.rand.NextBool(4))
                {
                    dust.noGravity = true;
                    dust.scale *= 1.2f;
                    dust.velocity.X *= 2f;
                    dust.velocity.Y *= 2f;
                }
                //else
                //dust.scale *= 1f;

                //dust.scale *= 1.5f;
                dust.velocity *= 1.2f;
                dust.velocity += Projectile.velocity * 1f * Utils.Remap(Timer, 0f, ShootTime * 0.75f, 1f, 0.1f) * Utils.Remap(Projectile.localAI[0], 0f, ShootTime * 0.1f, 0.1f, 1f);
                dust.customData = 1;
            }

            if (Timer >= ShootTime && Main.rand.NextBool())
            {
                Vector2 center = Main.player[Projectile.owner].Center;
                Vector2 vector = (Projectile.Center - center).SafeNormalize(Vector2.Zero).RotatedByRandom(0.19634954631328583) * 7f;
                short num7 = DustID.Smoke;
                Dust dust2 = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(50f, 50f) - (vector * 2f), 4, 4, num7, 0f, 0f, 150, Color.Lime);
                dust2.noGravity = true;
                dust2.velocity = vector;
                dust2.scale *= 1.1f + (Main.rand.NextFloat() * 0.2f);
                dust2.customData = -0.3f - (0.15f * Main.rand.NextFloat());
            }

            Projectile.frameCounter++;
            if (Projectile.frameCounter > 7)
            {
                Projectile.frameCounter = 0;
                if (Projectile.frame < 7)
                    Projectile.frame++;
            }
        }

        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
            target.AddBuff(BuffID.CursedInferno, 60 * 3);

            if (Timer < ShootTime + 1)
            {
                Timer = ShootTime + 1;
            }
        }

        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            int num = (int)Utils.Remap(Timer, 0f, 72f, 10f, 40f);
            hitbox.Inflate(num, num);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (!projHitbox.Intersects(targetHitbox))
                return false;

            return Collision.CanHit(Projectile.Center, 0, 0, targetHitbox.Center.ToVector2(), 0, 0);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 pos = Projectile.Center - Main.screenPosition;

            Main.instance.LoadProjectile(ProjectileID.Flames);
            Texture2D mainTex = TextureAssets.Projectile[ProjectileID.Flames].Value;
            //Rectangle frameBox = mainTex.Frame(1, 7, 0, Projectile.frame);
            //Main.spriteBatch.Draw(mainTex, pos, frameBox, Color.MediumPurple, Projectile.rotation, frameBox.Size()/2, Projectile.scale, 0, 0);

            float totalTime = ShootTime + FadeTime;
            Color transparent = Color.Transparent;
            Color color = new(100, 255, 90, 200);
            Color color2 = new(130, 200, b: 80, 70);
            Color color3 = Color.Lerp(new Color(100, 255, 90, 100), color2, 0.25f);
            Color color4 = new(80, 80, 80, 100);
            float num3 = 0.35f;
            float num4 = 0.7f;
            float num5 = 0.85f;
            float num6 = (Timer > ShootTime - 10f) ? 0.175f : 0.2f;

            int verticalFrames = 7;
            float num9 = Utils.Remap(Timer, ShootTime, totalTime, 1f, 0f);
            float num10 = Math.Min(Timer, 20f);
            float num11 = Utils.Remap(Timer, 0f, totalTime, 0f, 1f);
            float num12 = Utils.Remap(num11, 0.2f, 0.5f, 0.5f, 1.25f);
            Rectangle frameBox = mainTex.Frame(1, verticalFrames, 0, 3);
            if (!(num11 < 1f))
                return false;

            for (int i = 0; i < 2; i++)
            {
                for (float j = 1f; j >= 0f; j -= num6)
                {
                    transparent = (num11 < 0.1f) ?
                        Color.Lerp(Color.Transparent, color, Utils.GetLerpValue(0f, 0.1f, num11, clamped: true)) :
                        ((num11 < 0.2f) ?
                            Color.Lerp(color, color2, Utils.GetLerpValue(0.1f, 0.2f, num11, clamped: true)) :
                            ((num11 < num3) ?
                                color2 :
                                ((num11 < num4) ?
                                    Color.Lerp(color2, color3, Utils.GetLerpValue(num3, num4, num11, clamped: true)) :
                                    ((num11 < num5) ?
                                        Color.Lerp(color3, color4, Utils.GetLerpValue(num4, num5, num11, clamped: true)) :
                                        ((!(num11 < 1f)) ? Color.Transparent : Color.Lerp(color4, Color.Transparent, Utils.GetLerpValue(num5, 1f, num11, clamped: true)))))));
                    float num14 = (1f - j) * Utils.Remap(num11, 0f, 0.2f, 0f, 1f);
                    Vector2 vector = pos + (Projectile.velocity * (0f - num10) * j);
                    Color color5 = transparent * num14;
                    Color color6 = color5;
                    color6.G /= 2;
                    color6.B /= 2;
                    color6.A = (byte)Math.Min(color5.A + (80f * num14), 255f);
                    Utils.Remap(Timer, 20f, totalTime, 0f, 1f);

                    float factor = 1f / num6 * (j + 1f);
                    float num16 = Projectile.rotation + (j * MathHelper.PiOver2) + (Main.GlobalTimeWrappedHourly * factor * 2f);
                    float num17 = Projectile.rotation - (j * MathHelper.PiOver2) - (Main.GlobalTimeWrappedHourly * factor * 2f);
                    switch (i)
                    {
                        case 0:
                            Main.EntitySpriteDraw(mainTex, vector + (Projectile.velocity * (0f - num10) * num6 * 0.5f), frameBox, color6 * num9 * 0.25f, num16 + ((float)Math.PI / 4f), frameBox.Size() / 2f, num12, SpriteEffects.None);
                            Main.EntitySpriteDraw(mainTex, vector, frameBox, color6 * num9, num17, frameBox.Size() / 2f, num12, SpriteEffects.None);
                            break;
                        case 1:
                            Main.EntitySpriteDraw(mainTex, vector + (Projectile.velocity * (0f - num10) * num6 * 0.2f), frameBox, color5 * num9 * 0.25f, num16 + ((float)Math.PI / 2f), frameBox.Size() / 2f, num12 * 0.75f, SpriteEffects.None);
                            Main.EntitySpriteDraw(mainTex, vector, frameBox, color5 * num9, num17 + ((float)Math.PI / 2f), frameBox.Size() / 2f, num12 * 0.75f, SpriteEffects.None);
                            break;
                    }
                }
            }

            return false;
        }
    }
}
