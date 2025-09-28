using Coralite.Helpers;
using System;
using Terraria.ModLoader;
using TheTwinsRework.Dusts;
using TheTwinsRework.Projectiles;

namespace TheTwinsRework.NPCs
{
    public class LaserEye : BaseTwin
    {
        public override string Texture => AssetDirectory.Vanilla + "NPC_125";

        public override Color HighlightColor => Color.Red;

        public override void PostAI()
        {
            Lighting.AddLight(NPC.Center, Color.Red.ToVector3() * 0.5f);
        }

        public override void P1AI(NPC conrtoller)
        {
            int currState = (int)State % 8;

            if (currState == 4)
                ShootLaserP1(conrtoller);
            else if (currState == 7)
                CombineP1(conrtoller);
            else
                DashAttack(conrtoller, P1AttackTime,24, 0);
        }

        public override void P2AI(NPC conrtoller)
        {
            int currState = (int)State % 10;

            if (currState == 0)
                DashAttack(conrtoller, P2AttackTime, 30, 0.4f, MathHelper.Pi);
            else if (currState == 4 || currState == 7)
                ShootLaserP2(conrtoller);
            else if (currState == 9)
                CombineP2(conrtoller);
            else
                DashAttack(conrtoller, P2AttackTime, 30, 0.4f);
        }

        public override void P3AI(NPC conrtoller)
        {
            if (State == -1)
            {
                Wait(P3AttackTime / 2);
                return;
            }

            int currState = (int)State % 22;

            //0   1  2  3   4   5   6     7  8   9   10  11  12  13  14 15  16 17  18  19  20    21
            //冲 激光 冲 冲  激光 冲 连连看 冲  冲  激光 激光 冲  冲  转圈 冲 激光 冲 冲  激光 冲 连连看 连连看

            //if (currState == 0)
            //    Wait(P3AttackTime);
            if (currState is 1 or 4 or 9 or 10 or 15 or 18)
                ShootLaserP3(conrtoller);
            //else if (currState == 9)
            //    CombineP2(conrtoller);
            else
                DashAttack(conrtoller, P3AttackTime, 30, 0.4f);
        }

        public void ShootLaserP1(NPC controller)
        {
            //向两个角度转向，射出激光
            float halfTime = P1AttackTime;
            float realTime = Timer % halfTime;

            if (realTime == 0)//生成瞄准线
            {
                NPC.velocity = Vector2.Zero;
                Recorder2 = NPC.rotation;
                SPRecorder = MathF.Cos(MathHelper.Pi * Timer / halfTime) * MathHelper.PiOver4 / 2;

                ShineLine((int)halfTime / 2, Color.Red, NPC.whoAmI, (controller.Center - NPC.Center).ToRotation() + SPRecorder
                    , (int)CircleLimitIndex);
            }

            Timer++;
            float targetRot = (controller.Center - NPC.Center).ToRotation() + SPRecorder;//最终目标方向

            if (realTime < halfTime / 2)//转向目标点
            {
                int time = (int)halfTime / (2 * 2);

                if (realTime % time == 0)
                {
                    Recorder2 = NPC.rotation;
                    if (realTime < halfTime * 0.45f)
                        TickSound();
                }

                float factor = Helper.HeavyEase(realTime % time / time);

                //临时目标方向
                float percent = (int)(realTime / time + 1) / 2f;
                float targetRot2 = Recorder2.AngleLerp(targetRot, percent);

                NPC.rotation = Recorder2.AngleLerp(targetRot2, factor);

                return;
            }

            if (realTime < halfTime / 2)
            {
                NPC.rotation = (controller.Center - NPC.Center).ToRotation() + SPRecorder;

                return;
            }

            if (realTime % 10 == 0)
            {
                float velRot = MathF.Sin((realTime - halfTime * 2 / 3) / (halfTime / 3) * MathHelper.TwoPi * 3) * MathHelper.PiOver4 / 8;
                Vector2 velocity = (NPC.rotation + velRot).ToRotationVector2() * 16;
                NPC.NewProjectileInAI<DeathLaserNoTileCollide>(NPC.Center + NPC.rotation.ToRotationVector2() * 50
                    , velocity, Helper.GetProjDamage(100, 125, 150)
                    , 4, ai0: CircleLimitIndex);
            }

            if (Timer >= (int)(P1AttackTime * 2f))
            {
                ExchangeState();
            }
        }

        public void ShootLaserP2(NPC controller)
        {
            //分两次旋转射激光
            float halfTime = P2AttackTime;
            float realTime = Timer;

            if (realTime == 0)//生成瞄准线
            {
                NPC.velocity = Vector2.Zero;
                Recorder2 = NPC.rotation;
                //SPRecorder = MathF.Cos(MathHelper.Pi * Timer / halfTime) * MathHelper.PiOver4 / 2;

                ShineLine((int)halfTime / 2, Color.Red, NPC.whoAmI, (controller.Center - NPC.Center).ToRotation() + SPRecorder
                    , (int)CircleLimitIndex);
            }

            Timer++;
            float targetRot = (controller.Center - NPC.Center).ToRotation() + SPRecorder;//最终目标方向

            if (realTime < halfTime / 2)//转向目标点
            {
                int time = (int)halfTime / (2 * 2);

                if (realTime % time == 0)
                {
                    Recorder2 = NPC.rotation;
                    if (realTime < halfTime * 0.45f)
                        TickSound();
                }

                float factor = Helper.HeavyEase(realTime % time / time);

                //临时目标方向
                float percent = (int)(realTime / time + 1) / 2f;
                float targetRot2 = Recorder2.AngleLerp(targetRot, percent);

                NPC.rotation = Recorder2.AngleLerp(targetRot2, factor);

                return;
            }

            if (realTime == halfTime / 2)//开始旋转
            {
                NPC.rotation = (controller.Center - NPC.Center).ToRotation();
                CanDamage = true;

                return;
            }

            //旋转
            NPC.rotation = (controller.Center - NPC.Center).ToRotation();
            float rot = NPC.rotation + MathHelper.Pi;
            rot += MathHelper.PiOver4 / (halfTime / 2);
            NPC.Center = controller.Center + rot.ToRotationVector2() * (controller.Center - NPC.Center).Length();

            if (realTime % 5 == 0)
            {
                float velRot = MathF.Sin((realTime - halfTime * 2 / 3) / (halfTime / 3) * MathHelper.TwoPi * 3) * MathHelper.PiOver4 / 8;
                Vector2 velocity = (NPC.rotation + velRot).ToRotationVector2() * 16;
                NPC.NewProjectileInAI<P2Laser>(NPC.Center + NPC.rotation.ToRotationVector2() * 50
                    , velocity, Helper.GetProjDamage(100, 125, 150)
                    , 4, ai0: CircleLimitIndex);
            }

            if (Timer >= P2AttackTime )
            {
                ExchangeState();
            }
        }

        public void ShootLaserP3(NPC controller)
        {
            //分两次旋转射激光
            float Time = P3AttackTime * 2;
            float realTime = Timer;

            if (realTime == 0)//生成瞄准线
            {
                NPC.velocity = Vector2.Zero;
                Recorder2 = NPC.rotation;
                SPRecorder = State % 2 == 0 ? MathHelper.PiOver4 : -MathHelper.PiOver4;

                Projectile proj = NPC.NewProjectileDirectInAI<LaserLine>(NPC.Center + NPC.rotation.ToRotationVector2() * 50, NPC.rotation.ToRotationVector2()
                     , Helper.GetProjDamage(100, 100, 100), 0, -1, CircleLimitIndex
                     , Time / 4, NPC.whoAmI);

                (proj.ModProjectile as LaserLine).ShootTime = (int)(Time * 3 / 4);

                //ShineLine((int)halfTime / 2, Color.Red, NPC.whoAmI, (controller.Center - NPC.Center).ToRotation() + SPRecorder
                //    , (int)CircleLimitIndex);
            }

            Timer++;
            float targetRot = (controller.Center - NPC.Center).ToRotation() + SPRecorder;//最终目标方向

            if (realTime < Time / 4)//转向目标点
            {
                if (Timer % 15 == 0)
                {
                    float rot2 = (controller.Center - NPC.Center).ToRotation();
                    Dust d = Dust.NewDustPerfect(NPC.Center, ModContent.DustType<Scan>(), Vector2.Zero
                        , newColor: Color.Red, Scale: 0.1f);
                    d.rotation = rot2;
                }

                int time = (int)Time / (4 * 2);

                if (realTime % time == 0)
                {
                    Recorder2 = NPC.rotation;
                    if (realTime < Time * 0.45f)
                        TickSound();
                }

                float factor = Helper.HeavyEase(realTime % time / time);

                //临时目标方向
                float percent = (int)(realTime / time + 1) / 2f;
                float targetRot2 = Recorder2.AngleLerp(targetRot, percent);

                NPC.rotation = Recorder2.AngleLerp(targetRot2, factor);

                return;
            }

            if (realTime == Time / 4)//开始旋转
            {
                NPC.rotation = (controller.Center - NPC.Center).ToRotation() + SPRecorder;
                CanDamage = true;

                return;
            }

            //旋转
            NPC.rotation += -SPRecorder * 2 / (Time * 3 / 4);
            float rot = (NPC.Center - controller.Center ).ToRotation();
            rot += MathF.Sign(-SPRecorder) * MathHelper.PiOver4 / (Time * 3 / 4);
            NPC.Center = controller.Center + rot.ToRotationVector2() * (controller.Center - NPC.Center).Length();

            if (Timer >= Time)
            {
                ExchangeState();
            }
        }

        public override void CombineP2Attack()
        {
            if (Timer % 5 == 0)
            {
                Vector2 velocity = NPC.rotation.ToRotationVector2() * 16;
                NPC.NewProjectileInAI<P2Laser>(NPC.Center + NPC.rotation.ToRotationVector2() * 50
                    , velocity, Helper.GetProjDamage(100, 125, 150)
                    , 4, ai0: CircleLimitIndex);
            }
        }

        public override void P1State()
        {
            SPRecorder = Main.rand.Next(-1, 2) * MathHelper.PiOver4 / 4;
            if (State % 5 == 0)
            {
                SPRecorder = 0;
            }

            State++;
            //State = 5;


            if (OtherEyeIndex.GetNPCOwner<FireEye>(out NPC friend))
            {
                if (Vector2.Distance(friend.Center, NPC.Center) < 40)
                {
                    if (OtherEyeIndex < NPC.whoAmI)
                        SPRecorder = MathHelper.PiOver4 / 2;
                }
            }

        }

        public override void P2State()
        {
            SPRecorder = Main.rand.Next(-1, 2) * MathHelper.PiOver4 / 4;
            if (State % 5 == 0)
            {
                SPRecorder = 0;
            }

            State++;
            //State = 5;


            if (OtherEyeIndex.GetNPCOwner<FireEye>(out NPC friend))
            {
                if (Vector2.Distance(friend.Center, NPC.Center) < 40)
                {
                    if (OtherEyeIndex < NPC.whoAmI)
                        SPRecorder = MathHelper.PiOver4 / 2;
                    State--;
                }
            }
        }

        public override void P3State()
        {
            SPRecorder = Main.rand.Next(-1, 2) * MathHelper.PiOver4 / 4;
            if (State % 5 == 0)
            {
                SPRecorder = 0;
            }

            State++;
            //State = 5;

            if (combineMove)
            {
                combineMove = false;
                if (NPC.whoAmI < OtherEyeIndex)
                {
                    State = -1;
                }
            }

            if (OtherEyeIndex.GetNPCOwner<FireEye>(out NPC friend))
            {
                if (Vector2.Distance(friend.Center, NPC.Center) < 40)
                {
                    if (OtherEyeIndex < NPC.whoAmI)
                        SPRecorder = MathHelper.PiOver4 / 2;
                    State--;
                }
            }
        }

        public override void ExchangeGore()
        {
            for (int i = 0; i < 2; i++)
            {
                Gore.NewGore(NPC.GetSource_FromAI(), NPC.Center + Main.rand.NextVector2Circular(25, 25)
                    , Helper.NextVec2Dir(3, 6), 143,1.3f);
            }
        }
    }
}
