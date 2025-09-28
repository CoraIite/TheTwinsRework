using Coralite.Helpers;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TheTwinsRework.NPCs;

namespace TheTwinsRework.Projectiles
{
    public class FireBreathLine:ModProjectile
    {
        public override string Texture => AssetDirectory.Assets + "Line";

        public ref float CircleIndex => ref Projectile.ai[0];
        public ref float ReadyTime => ref Projectile.ai[1];
        public ref float ShootTime => ref Projectile.ai[2];

        public ref float LengthRecord => ref Projectile.localAI[0];
        public ref float Timer => ref Projectile.localAI[1];
        public ref float State => ref Projectile.localAI[2];

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
            }

            switch (State)
            {
                case 0://准备阶段，生成线条
                    {
                        Timer++;
                        if (Timer >= ReadyTime)
                        {
                            State = 1;
                            Timer = 0;

                            SoundEngine.PlaySound(CoraliteSoundID.LaserShoot_Item33, Projectile.Center);

                            if (Main.netMode != NetmodeID.Server)
                            {
                                thunderTrails = new ThunderTrail[4];
                                var tex = ModContent.Request<Texture2D>(AssetDirectory.Assets + "ThunderTrailB2");

                                for (int i = 0; i < 2; i++)
                                {
                                    thunderTrails[i] = new ThunderTrail(tex
                                        , f => Helper.SinEase(f) * 25, ThunderColorFuncBack, GetAlpha);

                                    thunderTrails[i].CanDraw = false;
                                    thunderTrails[i].UseNonOrAdd = true;
                                    thunderTrails[i].SetRange((10, 25));
                                    thunderTrails[i].SetExpandWidth(4);
                                    thunderTrails[i].BasePositions =
                                    [
                                    Projectile.Center,Projectile.Center,Projectile.Center
                                    ];
                                }
                                for (int i = 2; i < 4; i++)
                                {
                                    thunderTrails[i] = new ThunderTrail(tex
                                        , f => Helper.SinEase(f) * 15, ThunderColorFuncRed, GetAlpha);

                                    thunderTrails[i].CanDraw = false;
                                    //thunderTrails[i].UseNonOrAdd = true;

                                    thunderTrails[i].SetRange((0, 30));
                                    thunderTrails[i].SetExpandWidth(4);
                                    thunderTrails[i].BasePositions =
                                    [
                                    Projectile.Center,Projectile.Center,Projectile.Center
                                    ];
                                }
                            }
                        }
                    }
                    break;
                case 1://激光射射射
                    {
                        if (Main.netMode != NetmodeID.Server)
                        {
                            List<Vector2> pos = new()
                            {
                                Projectile.Center
                            };

                            int count = (int)(LengthRecord / 55);
                            for (int i = 1; i < count; i++)
                            {
                                pos.Add(Projectile.Center + Projectile.velocity * i * 55);
                            }

                            foreach (var trail in thunderTrails)
                            {
                                trail.UpdateThunderToNewPosition([.. pos]);
                            }

                            if (Timer % 4 == 0)
                            {
                                foreach (var trail in thunderTrails)
                                {
                                    trail.CanDraw = Main.rand.NextBool();
                                    trail.RandomThunder();
                                }
                            }
                        }

                        Lighting.AddLight(Projectile.Center, new Vector3(1.5f, 0, 0) * LaserWidth / 40);
                        int time = ShootTime;
                        if (Timer < 10)
                        {
                            LaserWidth = Helper.Lerp(0, 80, Timer / 10);
                        }
                        else if (Timer < 15)
                        {
                            LaserWidth = Helper.Lerp(80, 30, (Timer - 10) / 5);
                        }
                        else if (Timer > time * 0.8f)
                        {
                            LaserWidth = Helper.Lerp(30, 0, (Timer - time * 0.8f) / (time * 0.2f));
                        }

                        Timer++;
                        if (Timer >= time)
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

    }
}
