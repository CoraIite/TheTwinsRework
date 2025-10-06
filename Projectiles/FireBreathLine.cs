using Coralite.Helpers;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TheTwinsRework.NPCs.TheTwins;

namespace TheTwinsRework.Projectiles
{
    public class FireBreathLine : ModProjectile
    {
        public override string Texture => AssetDirectory.Assets + "Line";

        public ref float CircleIndex => ref Projectile.ai[0];
        public ref float ReadyTime => ref Projectile.ai[1];
        public ref float ShootTime => ref Projectile.ai[2];

        public ref float LengthRecord => ref Projectile.localAI[0];
        public ref float Timer => ref Projectile.localAI[1];
        public ref float State => ref Projectile.localAI[2];

        private bool Record = true;

        public override void SetDefaults()
        {
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.hostile = true;
        }

        public override bool ShouldUpdatePosition() => false;

        public override void AI()
        {
            if (!CircleIndex.GetNPCOwner(out NPC Circle, Projectile.Kill))
                return;

            //确定长度
            if (Record)
            {
                Record = false;

                float rot = Projectile.velocity.ToRotation();
                SetLength(Projectile.velocity, Circle);
                Projectile.rotation = rot;
            }

            switch (State)
            {
                case 0://准备阶段，生成线条
                    {
                        int startTime = 0;
                        if (ReadyTime > 35)
                            startTime += ((int)ReadyTime - 35) / 2;

                        if (Timer == startTime)
                            Helper.PlayPitched("MultiLine", 0.6f, 0, Projectile.Center);

                        Timer++;
                        if (Timer >= ReadyTime)
                        {
                            State = 1;
                            Timer = 0;

                            //SoundEngine.PlaySound(CoraliteSoundID.LaserShoot_Item33, Projectile.Center);

                            if (Main.netMode != NetmodeID.Server)
                            {
                            }
                        }
                    }
                    break;
                case 1://激光射射射
                    {
                        if (Timer % 2 == 0)
                        {
                            float f = Timer / ShootTime;

                            Vector2 pos = Projectile.Center + Projectile.velocity * LengthRecord / 3 * f;

                            Projectile.NewProjectile(Projectile.GetSource_FromAI(), pos
                                , Projectile.velocity * (6 + f * (LengthRecord * 2 / 3 / 40)), ModContent.ProjectileType<FireBreath>(), Helper.GetProjDamage(100, 125, 150)
                                , 4, ai0: CircleIndex, ai1: 40, ai2: 12);
                        }

                        Timer++;
                        if (Timer >= ShootTime)
                        {
                            Projectile.Kill();
                        }
                    }
                    break;
            }
        }

        public void SetLength(Vector2 dir, NPC circle)
        {
            bool closer = true;

            Vector2 pos = Projectile.Center;

            for (int i = 0; i < 100; i++)
            {
                if (closer)//还没远离的时候，记录由远到近
                {
                    if (Vector2.Distance(pos, circle.Center) < CircleLimit.MaxLength)
                    {
                        closer = false;
                    }
                }
                else//近了之后远离
                {
                    if (Vector2.Distance(pos, circle.Center) > CircleLimit.MaxLength)
                        break;
                }

                pos += dir * 16;
            }

            LengthRecord = Vector2.Distance(pos, Projectile.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            switch (State)
            {
                default:
                case 0:
                    {
                        Texture2D tex = Projectile.GetTexture();
                        Vector2 position = Projectile.Center - Main.screenPosition;
                        Vector2 origin = new Vector2(0, tex.Height / 2);
                        Color c = Color.Lime * Helper.SqrtEase(Timer / ReadyTime);

                        float Length = LengthRecord;

                        Vector2 scale = new Vector2(Length / tex.Width, 0.3f);
                        int startTime = 0;
                        if (ReadyTime > 35)
                            startTime += ((int)ReadyTime - 35) / 3;

                        if (Timer < startTime)
                        {
                            scale = Vector2.Zero;
                        }
                        else if (Timer < startTime + 5)
                        {
                            scale = Vector2.Lerp(Vector2.Zero, new Vector2(scale.X * 0.2f, 4), (Timer - startTime) / 5);
                        }
                        else if (Timer < startTime + 10)
                        {
                            scale = Vector2.Lerp(new Vector2(scale.X * 0.2f, 4), scale, (Timer - startTime - 5) / 5);
                        }

                        Main.spriteBatch.Draw(tex, position, null
                                , c * 0.5f, Projectile.rotation, origin, scale, 0, 0);
                        Main.spriteBatch.Draw(tex, position, null
                            , c with { A = 0 } * 0.5f, Projectile.rotation, origin, scale, 0, 0);
                    }
                    break;
                case 1:
                    {
                    }
                    break;
            }

            return false;
        }

    }
}
