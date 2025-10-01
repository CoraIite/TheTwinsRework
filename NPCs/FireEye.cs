using Coralite.Helpers;
using System;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TheTwinsRework.Dusts;
using TheTwinsRework.Projectiles;

namespace TheTwinsRework.NPCs
{
    public class FireEye : BaseTwin
    {
        public override string Texture => AssetDirectory.Vanilla + "NPC_126";

        public override Color HighlightColor => Color.White;

        public override void PostAI()
        {
            Lighting.AddLight(NPC.Center, Color.Lime.ToVector3() * 0.5f);
        }


        public void FireShootSound()
        {
            Helper.PlayPitched("FireBall", 0.5f, 0, NPC.Center);
        }

        public void FireBreathSound()
        {
            Helper.PlayPitched("FireBreath", 0.6f, 0, NPC.Center);
        }


        public override void P1AI(NPC conrtoller)
        {
            int currState = (int)State % 8;

            if (currState == 4)
                ShootFireP1(conrtoller);
            else if (currState == 7)
                CombineP1(conrtoller);
            else
                DashAttack(conrtoller, P1AttackTime,24,0);
        }

        public override void P2AI(NPC conrtoller)
        {
            int currState = (int)State % 10;

            if (currState == 0)
                DashAttack(conrtoller, P2AttackTime, 30, 0.4f, MathHelper.Pi);
            else if (currState == 3 || currState == 6)
                ShootFireP2(conrtoller);
            else if (currState == 9)
                CombineP2(conrtoller);
            else
                DashAttack(conrtoller, P2AttackTime, 30, 0.4f);
        }

        public override void P3AI(NPC conrtoller)
        {
            if (WaitState == 1)
            {
                Wait(P3AttackTime / 2);
                return;
            }

            int currState = (int)State % 30;
            //ShootFireP3(conrtoller);
            //return;
            // 0 1  2 3  4   5  6  7  8   9      10  11  12 13 14  15 16 17 18     19  20  21 22  23  24 25 26 27  28    29
            //冲 冲 冲 冲 喷火 冲 冲 冲 冲 连连看    冲  冲  冲 喷火 冲  冲 冲 冲 转圈    冲  冲  冲  冲  喷火 冲 冲 冲  冲 连连看 连连看

            switch (currState)
            {
                default:
                    DashAttackP3(conrtoller, P3AttackTime, 30);
                    break;
                case 4:
                case 11:
                case 15:
                case 23:
                    ShootFireP3(conrtoller);
                    break;
                case 18:
                    CombineP3_Rot(conrtoller);
                    break;
                case 19:
                    DashAttackP3(conrtoller, P3AttackTime, 30,MathHelper.Pi);
                    break;
                case 9:
                    CombinwP3_Lines(conrtoller, true);
                    break;
                case 28://不添加额外偏移时间
                    CombinwP3_Lines(conrtoller, false);
                    break;
                case 29://不添加额外前摇时间
                    CombinwP3_Lines(conrtoller, true);
                    break;
            }
        }



        public void ShootFireP1(NPC controller)
        {
            //向两个角度转向，射出激光
            float halfTime = P1AttackTime;
            float realTime = Timer % halfTime;

            if (realTime == 0)//生成瞄准线
            {
                NPC.velocity = Vector2.Zero;
                Recorder2 = NPC.rotation;
                SPRecorder = MathF.Cos(MathHelper.Pi * Timer / halfTime) * MathHelper.PiOver4 / 2;

                ShineLine((int)halfTime / 2, Color.LimeGreen, NPC.whoAmI, (controller.Center - NPC.Center).ToRotation() + SPRecorder
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
                    switch (realTime / time)
                    {
                        default:
                            break;
                        case 0:
                            HaloSound();
                            break;
                        case 1:
                            TickSound();
                            break;
                    }
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

            if (realTime % 5 == 0)
            {
                FireShootSound();
                float velRot = MathF.Sin((realTime - halfTime * 2 / 3) / (halfTime / 3) * MathHelper.TwoPi) * MathHelper.PiOver4 / 8;
                Vector2 velocity = (NPC.rotation + velRot).ToRotationVector2() * 4;

                NPC.NewProjectileInAI<SlowFireBall>(NPC.Center + NPC.rotation.ToRotationVector2() * 50
                    , velocity, Helper.GetProjDamage(100, 125, 150)
                    , 4, ai0: CircleLimitIndex);
            }

            if (Timer >= (int)(P1AttackTime * 2f))
            {
                ExchangeState();
            }
        }

        public void ShootFireP2(NPC controller)
        {
            //分两次旋转射激光
            float halfTime = P2AttackTime;
            float realTime = Timer;

            if (realTime == 0)//生成瞄准线
            {
                NPC.velocity = Vector2.Zero;
                Recorder2 = NPC.rotation;
                //SPRecorder = MathF.Cos(MathHelper.Pi * Timer / halfTime) * MathHelper.PiOver4 / 2;

                ShineLine((int)halfTime / 2, Color.LimeGreen, NPC.whoAmI, (controller.Center - NPC.Center).ToRotation() + SPRecorder
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
                    switch (realTime / time)
                    {
                        default:
                            break;
                        case 0:
                            HaloSound();
                            break;
                        case 1:
                            TickSound();
                            break;
                    }
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
            rot += MathHelper.PiOver2 / (halfTime / 2);
            NPC.Center = controller.Center + rot.ToRotationVector2() * (controller.Center - NPC.Center).Length();

            if (realTime % 5 == 0)
            {
                FireShootSound();

                Vector2 velocity = NPC.rotation.ToRotationVector2() * 4f;
                NPC.NewProjectileInAI<P2FireBall>(NPC.Center + NPC.rotation.ToRotationVector2() * 50
                    , velocity, Helper.GetProjDamage(100, 125, 150)
                    , 4, ai0: CircleLimitIndex);
            }

            if (Timer >= P2AttackTime)
            {
                ExchangeState();
            }
        }

        public void ShootFireP3(NPC controller)
        {
            //吐火
            float halfTime = P3AttackTime * 2;
            float Time = Timer;

            if (Time == 0)//生成瞄准线
            {
                NPC.velocity = Vector2.Zero;
                Recorder2 = NPC.rotation;
                SPRecorder = State % 2 == 0 ? MathHelper.PiOver4 : -MathHelper.PiOver4;

                ShineLine((int)halfTime / 4, Color.LimeGreen, NPC.whoAmI, (controller.Center - NPC.Center).ToRotation() + SPRecorder
                    , (int)CircleLimitIndex);
            }

            Timer++;
            float targetRot = (controller.Center - NPC.Center).ToRotation() + SPRecorder;//最终目标方向

            if (Time < halfTime / 4)//转向目标点
            {
                if (Timer % 15 == 0)
                {
                    float rot2 = (controller.Center - NPC.Center).ToRotation();
                    Dust d = Dust.NewDustPerfect(NPC.Center, ModContent.DustType<Scan>(), Vector2.Zero
                        , newColor: Color.Lime, Scale: 0.1f);
                    d.rotation = rot2;
                }


                int time = (int)halfTime / (4 * 2);

                if (Time % time == 0)
                {
                    Recorder2 = NPC.rotation;
                    switch (Time / time)
                    {
                        default:
                            break;
                        case 0:
                            HaloSound();
                            break;
                        case 1:
                            TickSound();
                            break;
                    }
                }

                float factor = Helper.HeavyEase(Time % time / time);

                //临时目标方向
                float percent = (int)(Time / time + 1) / 2f;
                float targetRot2 = Recorder2.AngleLerp(targetRot, percent);

                NPC.rotation = Recorder2.AngleLerp(targetRot2, factor);

                return;
            }

            if (Time == halfTime / 4)//开始旋转
            {
                NPC.rotation = (controller.Center - NPC.Center).ToRotation() + SPRecorder;
                CanDamage = true;
                CanDrawTrail = true;

                return;
            }

            //旋转
            NPC.rotation += -SPRecorder * 2 / (Time * 3 / 4);
            float rot = (NPC.Center - controller.Center).ToRotation();
            rot += MathF.Sign(-SPRecorder) * MathHelper.PiOver4 / (Time * 3 / 4);
            NPC.Center = controller.Center + rot.ToRotationVector2() * (controller.Center - NPC.Center).Length();

            //喷火器类火焰
            if (Time % 2 == 0)
            {
                if (Time % 10 == 0)
                    FireBreathSound();
                float speed = MathF.Sin(Time * 0.6f) * 2 + 7;
                float exRot = MathF.Sin(Time * 0.5f) * 0.2f;
                int time = (int)(MathF.Sin(Time * 0.4f) * 10) + 40;
                Vector2 velocity = (NPC.rotation + exRot-SPRecorder * 4 / (Time * 3 / 4)).ToRotationVector2() * speed;
                NPC.NewProjectileInAI<FireBreath>(NPC.Center + NPC.rotation.ToRotationVector2() * 50
                    , velocity, Helper.GetProjDamage(100, 125, 150)
                    , 4, ai0: CircleLimitIndex, ai1: time, ai2: 12);
            }

            //火球
            if (Time % 10 == 0)
            {
                float speed = MathF.Sin(Time * 0.2f) * 3 + 6;
                float exRot = MathF.Sin(Time * 0.4f) * 0.4f;
                Vector2 velocity = (NPC.rotation + exRot).ToRotationVector2() * speed;
                NPC.NewProjectileInAI<P2FireBall>(NPC.Center + NPC.rotation.ToRotationVector2() * 75
                    , velocity, Helper.GetProjDamage(100, 125, 150)
                    , 4, ai0: CircleLimitIndex);
            }

            if (Timer >= halfTime)
                ExchangeState();
        }

        public void DashAttackP3(NPC controller, int attackTime, float speed, float exRot = 0)
        {
            if (Timer == 0)
            {
                Recorder2 = NPC.rotation;
                NPC.velocity = Vector2.Zero;

                if (Main.netMode != NetmodeID.Server)
                {
                    HaloSound();

                    Dust d = Dust.NewDustPerfect(NPC.Center, ModContent.DustType<AimLine>()
                        , Vector2.Zero, 0, Color.White, 1);
                    d.customData = new AimLine.IncomeData()
                    {
                        index = NPC.whoAmI,
                        circleIndex = (int)CircleLimitIndex,
                        maxTime = attackTime / 2,
                        rot = (controller.Center - NPC.Center).ToRotation() + SPRecorder + exRot
                    };
                }
            }

            Timer++;

            if (Timer < attackTime / 2)//旋转向目标点
            {
                float targetRot = (controller.Center - NPC.Center).ToRotation() + SPRecorder + exRot;//最终目标方向
                int time = attackTime / (2 * 2);

                if (Timer % time == 0)
                {
                    Recorder2 = NPC.rotation;
                    if (Timer < attackTime * 0.45f)
                        TickSound();
                }

                float factor = Helper.HeavyEase(Timer % time / time);

                //临时目标方向
                float percent = (int)(Timer / time + 1) / 2f;
                float targetRot2 = Recorder2.AngleLerp(targetRot, percent);

                NPC.rotation = Recorder2.AngleLerp(targetRot2, factor);

                return;
            }

            if (Timer == attackTime / 2)
            {
                DashSound();

                SoundEngine.PlaySound(CoraliteSoundID.ForceRoar with { Pitch = 0.4f }, NPC.Center);

                float targetRot = (controller.Center - NPC.Center).ToRotation() + SPRecorder + exRot;//最终目标方向
                NPC.rotation = targetRot;
                NPC.velocity = targetRot.ToRotationVector2() * speed;
                NPC.rotation = targetRot;
                CanDrawTrail = true;
                CanDamage = true;
                NPC.InitOldPosCache(10, false);
                NPC.InitOldRotCache(10);
                Recorder2 = 0;

                return;
            }

            //喷火器类火焰
            if (Timer % 2 == 0&& NPC.velocity.Length() > 1)
            {
                if (Timer % 12 == 0)
                    FireBreathSound();
                Vector2 velocity = NPC.rotation.ToRotationVector2() * 23;
                NPC.NewProjectileInAI<FireBreath>(NPC.Center + NPC.rotation.ToRotationVector2() * 50
                    , velocity, Helper.GetProjDamage(100, 125, 150)
                    , 4, ai0: CircleLimitIndex, ai1: 20, ai2: 12);
            }

            float distance = Vector2.Distance(NPC.Center, controller.Center);
            if (Recorder2 == 0)
            {
                if (distance < CircleLimit.MaxLength - 60)
                    Recorder2 = 1;
            }
            else if (distance >= CircleLimit.MaxLength - 60)
            {
                CanDamage = false;
                NPC.Center = controller.Center + (NPC.Center - controller.Center).SafeNormalize(Vector2.Zero) * (CircleLimit.MaxLength - 59);
                NPC.velocity = Vector2.Zero;
                NPC.rotation += 0.03f;
            }

            if (Timer >= attackTime)
                ExchangeState();
        }

        public override void CombineP2Attack()
        {
            if (Timer % 4 == 0)
            {
                FireShootSound();

                Vector2 velocity = NPC.rotation.ToRotationVector2() * 5.5f;
                NPC.NewProjectileInAI<P2FireBall>(NPC.Center + NPC.rotation.ToRotationVector2() * 50
                    , velocity, Helper.GetProjDamage(100, 125, 150)
                    , 4, ai0: CircleLimitIndex);
            }
        }

        public override void CombineP3Attack(NPC controller, int readyTime)
        {
            int count = Helper.ScaleValueForDiffMode(3,3,5,7);

            Vector2 startPos = NPC.Center + NPC.rotation.ToRotationVector2() * 85;
            float rot = NPC.rotation;

            for (int i = 0; i < count; i++)
            {
                NPC.NewProjectileInAI<FireBreathLine>(startPos, rot.ToRotationVector2()
                     , Helper.GetProjDamage(100, 100, 100), 0, -1, CircleLimitIndex
                     , readyTime+i*6, 25);

                float length = GetLength(startPos, rot.ToRotationVector2(), controller);
                //Vector2 temp = startPos;
                startPos += rot.ToRotationVector2() * length;
                startPos += (controller.Center - startPos).SafeNormalize(Vector2.Zero) * 9;
                startPos = controller.Center + (startPos - controller.Center).SafeNormalize(Vector2.Zero) * (CircleLimit.MaxLength - 9);


                Vector2 toCenter = (startPos - controller.Center).SafeNormalize(Vector2.Zero);
                float angle = Utils.AngleTo(rot.ToRotationVector2(), toCenter);


                rot += angle * 2 + MathHelper.Pi;

                //随机加减角度
                rot += Main.rand.Next(-3, 4) * MathHelper.PiOver4 / 2;
                //防止最终角度垂直于半径
                if (Vector2.Dot(rot.ToRotationVector2(), toCenter) > -0.2f)
                    rot = (controller.Center - startPos).ToRotation() + MathF.Cos(i * MathHelper.PiOver2) * MathHelper.PiOver4;
            }
        }

        public override void CombineP3_Roting(NPC controller)
        {

            //喷火器类火焰
            if (Timer % 2 == 0)
            {
                if (Timer % 18 == 0)
                    FireBreathSound();
                float speed = MathF.Sin(Timer * 0.6f) * 2 + 5;
                float exRot = MathF.Sin(Timer * 0.5f) * 0.2f;
                int time = (int)(MathF.Sin(Timer * 0.4f) * 10) + 40;
                Vector2 velocity = (NPC.rotation + exRot - SPRecorder * 4 / (Timer * 3 / 4)).ToRotationVector2() * speed;
                NPC.NewProjectileInAI<FireBreath>(NPC.Center + NPC.rotation.ToRotationVector2() * 50
                    , velocity, Helper.GetProjDamage(100, 125, 150)
                    , 4, ai0: CircleLimitIndex, ai1: time, ai2: 12);
            }

            //火球
            if (Timer % 10 == 0)
            {
                float speed = MathF.Sin(Timer * 0.2f) * 4 + 6;
                float exRot = MathF.Sin(Timer * 0.4f) * 0.4f;
                Vector2 velocity = (NPC.rotation + exRot).ToRotationVector2() * speed;
                NPC.NewProjectileInAI<P2FireBall>(NPC.Center + NPC.rotation.ToRotationVector2() * 75
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

            if (OtherEyeIndex.GetNPCOwner<LaserEye>(out NPC friend))
            {
                if (Vector2.Distance(friend.Center, NPC.Center) < 40)
                {
                    if (OtherEyeIndex > NPC.whoAmI)
                    {
                        SPRecorder = -MathHelper.PiOver4 / 2;
                    }
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

            if (OtherEyeIndex.GetNPCOwner<LaserEye>(out NPC friend))
            {
                if (Vector2.Distance(friend.Center, NPC.Center) < 40)
                {
                    if (OtherEyeIndex > NPC.whoAmI)
                        SPRecorder = -MathHelper.PiOver4 / 2;

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
            //Main.NewText(State, Color.Lime);
            State++;
            //State = 5;

            if (combineMove)
            {
                combineMove = false;
                int state = (int)State % 2;
                if (state == 0)
                {
                    if (NPC.whoAmI < OtherEyeIndex)
                        WaitState = 1;
                }
                else
                {
                    if (NPC.whoAmI > OtherEyeIndex)
                        WaitState = 1;
                }
            }

            if (OtherEyeIndex.GetNPCOwner<LaserEye>(out NPC friend))
            {
                if (Vector2.Distance(friend.Center, NPC.Center) < 40)
                {
                    if (OtherEyeIndex > NPC.whoAmI)
                        SPRecorder = -MathHelper.PiOver4 / 2;

                    State--;
                }
            }
        }

        public override void ExchangeGore()
        {
            for (int i = 0; i < 2; i++)
            {
                Gore.NewGore(NPC.GetSource_FromAI(), NPC.Center + Main.rand.NextVector2Circular(25, 25)
                    , Helper.NextVec2Dir(3, 6), 144,1.3f);
            }
        }
    }
}
