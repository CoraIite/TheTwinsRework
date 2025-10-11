using Coralite.Content.Particles;
using Coralite.Helpers;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using TheTwinsRework.Core;
using TheTwinsRework.Core.Loader;
using TheTwinsRework.Core.System_Particle;
using TheTwinsRework.Dusts;
using TheTwinsRework.Particles;
using TheTwinsRework.Projectiles;

namespace TheTwinsRework.NPCs.TheTwins
{
    public abstract class BaseTwin : ModNPC
    {
        /// <summary>
        /// 阶段控制器
        /// </summary>
        public AIPhase Phase;

        public enum AIPhase
        {
            /// <summary>
            /// 动画阶段
            /// </summary>
            Anmi,
            /// <summary>
            /// 血肉状态1阶段
            /// </summary>
            P1,
            /// <summary>
            /// 血肉状态1阶段，加速版
            /// </summary>
            P2,
            /// <summary>
            /// 钢铁状态2阶段
            /// </summary>
            P3,
            /// <summary>
            /// 最后一舞
            /// </summary>
            P4,
        }

        /// <summary>
        /// 能否伤害玩家
        /// </summary>
        public bool CanDamage;

        public bool CanDrawTrail;

        public ref float OtherEyeIndex => ref NPC.ai[0];
        public ref float CircleLimitIndex => ref NPC.ai[1];
        public ref float State => ref NPC.ai[2];
        public ref float SPRecorder => ref NPC.ai[3];

        public ref float Timer => ref NPC.localAI[0];
        public ref float Recorder2 => ref NPC.localAI[1];
        public ref float Recorder3 => ref NPC.localAI[2];
        public ref float WaitState => ref NPC.localAI[3];

        public const int P1AttackTime = 60 * 1 + 59;
        public const int P2AttackTime = 60 * 1 + 35;
        public const int P3AttackTime = 60 * 1 + 18;

        public bool SteelPhase;
        public bool combineMove;

        public abstract Color HighlightColor { get; }

        public Vector2 middlePos;
        public SecondOrderDynamics_Vec2 smoother;

        public override void SetStaticDefaults()
        {
            NPC.QuickTrailSets(Helper.NPCTrailingMode.RecordAll, 10);
        }

        public override void SetDefaults()
        {
            NPC.lifeMax = 20000;
            NPC.damage = 95;
            NPC.defDamage = 10;
            NPC.defense = 10;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.hide = true;
            NPC.knockBackResist = 0;
            NPC.width = NPC.height = 100;
        }

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            int expertBaseLife = 20000;
            int masterBaseLife = 20000;

            int expertAddLife = 4000;
            int masterAddLife = 8000;

            NPC.defDamage = 120;
            if (Helper.GetJourneyModeStrangth(out float journeyScale, out NPCStrengthHelper nPCStrengthHelper))
            {
                if (nPCStrengthHelper.IsExpertMode)
                {
                    NPC.lifeMax = (int)((expertBaseLife + numPlayers * expertAddLife) / journeyScale);
                    NPC.damage = (int)(100 / journeyScale);
                }

                if (nPCStrengthHelper.IsMasterMode)
                {
                    NPC.lifeMax = (int)((masterBaseLife + numPlayers * masterAddLife) / journeyScale);
                    NPC.damage = (int)(150 / journeyScale);
                }

                if (Main.getGoodWorld)
                {
                    NPC.damage = (int)(190 / journeyScale);
                }

                return;
            }

            NPC.lifeMax = expertBaseLife + numPlayers * expertAddLife;
            NPC.damage = 120;

            if (Main.masterMode)
            {
                NPC.lifeMax = masterBaseLife + numPlayers * masterAddLife;
                NPC.damage = 150;
            }

            if (Main.getGoodWorld)
            {
                NPC.lifeMax = 20000 + numPlayers * 12000;
                NPC.damage = 190;
            }
        }

        public override bool CheckActive()
        {
            return CircleLimitIndex.GetNPCOwner<CircleLimit>(out _);
        }

        public override void ModifyHoverBoundingBox(ref Rectangle boundingBox)
        {
            boundingBox = new Rectangle();
        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            return false;
        }

        #region AI

        public override void AI()
        {
            if (!CircleLimitIndex.GetNPCOwner<CircleLimit>(out NPC controller) || Main.dayTime)
            {
                NPC.velocity = Vector2.Lerp(NPC.velocity, new Vector2(NPC.whoAmI % 2 == 0 ? -6 : 6, -20), 0.1f);
                NPC.rotation = NPC.velocity.ToRotation();
                CanDamage = true;
                CanDrawTrail = false;

                NPC.EncourageDespawn(30);

                return;
            }

            switch (Phase)
            {
                case AIPhase.Anmi:
                    Anmi(controller);
                    break;
                case AIPhase.P1:
                    P1AI(controller);
                    BloodDust();
                    break;
                case AIPhase.P2:
                    P2AI(controller);
                    BloodDust();
                    break;
                case AIPhase.P3:
                    P3AI(controller);
                    break;
                case AIPhase.P4:
                    P4AI(controller);
                    break;
            }

            UpdateFrame();
            if (NPC.whoAmI < OtherEyeIndex && OtherEyeIndex.GetNPCOwner(out NPC n) && n.ModNPC is BaseTwin)
            {
                Vector2 pos = (NPC.Center + n.Center) / 2
                    + new Vector2(0, Vector2.Distance(NPC.Center, n.Center) / 5);

                if (smoother == null)
                {
                    middlePos = pos;
                    smoother = new SecondOrderDynamics_Vec2(1f, 0.4f, 0, pos);
                }

                middlePos = smoother.Update(1 / 60f, pos);
            }

            void BloodDust()
            {
                if (Main.rand.NextBool(4))
                {
                    Dust d = Dust.NewDustPerfect(NPC.Center + Helper.NextVec2Dir(1, NPC.width / 2+15), DustID.Blood
                         , Helper.NextVec2Dir(0.5f, 1.5f),Scale:Main.rand.NextFloat(1,2f));
                    d.noGravity = true;
                }
            }
        }

        public virtual void P1AI(NPC controller)
        {

        }

        public virtual void P2AI(NPC controller)
        {

        }

        public virtual void P3AI(NPC controller)
        {

        }

        public virtual void P4AI(NPC controller)
        {
        }

        #region 动画部分

        public virtual void Anmi(NPC conrtoller)
        {
            NPC.dontTakeDamage = true;

            switch (Recorder2)
            {
                case 0:
                    Helper.StopMusic();
                    SpawnAnmi(conrtoller);
                    break;
                case 1:
                    ExchangeAnmiP1ToP2(conrtoller);
                    break;
                case 2:
                    ExchangeAnmiP2ToP3(conrtoller);
                    break;
                case 3:
                    ExchangeAnmiP3ToP4(conrtoller);
                    break;
                case 4:
                    KillAnmi();
                    break;
            }
        }


        public void SpawnAnmi(NPC controller)
        {
            const int S1Time = 130;
            const int S2Time = 120;

            switch (State)
            {
                case 0://旋转向目标位置

                    Vector2 dir = (SPRecorder
                        + Math.Sign(SPRecorder) * Helper.SqrtEase(Timer / S1Time) * (MathHelper.TwoPi * 2)).ToRotationVector2() * (CircleLimit.MaxLength - 120);

                    dir += controller.Center;
                    float length = (dir - NPC.Center).Length() / 5;
                    if (length > 45)
                        length = 45;

                    NPC.velocity = Vector2.Lerp(NPC.velocity, (dir - NPC.Center).SafeNormalize(Vector2.Zero) * length, 0.3f);
                    NPC.rotation = NPC.velocity.ToRotation();

                    Timer++;
                    if (Timer > S1Time)
                    {
                        State = 1;
                        Timer = 0;
                        NPC.Center = controller.Center + SPRecorder.ToRotationVector2() * (CircleLimit.MaxLength - 120);
                        NPC.velocity = Vector2.Zero;
                        //生成名字
                        if (NPC.whoAmI < OtherEyeIndex)
                            NPC.NewProjectileInAI<NameShow>(NPC.Center, Vector2.Zero, 0, 0);
                    }
                    break;
                case 1://吼叫

                    if (Timer == 0)
                    {
                        SoundEngine.PlaySound(CoraliteSoundID.ForceRoar, NPC.Center);
                    }

                    if (Timer % 10 == 0)
                    {
                        Dust.NewDustPerfect(NPC.Center, ModContent.DustType<RoaringWave>()
                            , Vector2.Zero, 0, Color.White * 0.8f, 0.4f);
                    }

                    NPC.rotation += MathF.Sin(Timer * 0.75f) * 0.1f;

                    Timer++;
                    if (Timer > S2Time)
                    {
                        Phase = AIPhase.P1;
                        State = 0;
                        Timer = 0;
                        NPC.dontTakeDamage = false;
                        P1State();

                        (controller.ModNPC as CircleLimit).SwitchBGM(Phase);
                    }

                    break;
            }
        }

        public void SwitchPhaseP1ToP2()
        {
            State = 0;
            Timer = 0;
            SPRecorder = 0;
            Recorder2 = 1;
            Recorder3 = 0;
            CanDrawTrail = false;
            CanDamage = false;

            Phase = AIPhase.Anmi;
        }

        public void SwitchPhaseP2ToP3()
        {
            State = 0;
            Timer = 0;
            SPRecorder = 0;
            Recorder2 = 2;
            Recorder3 = 0;
            CanDrawTrail = false;
            CanDamage = false;
            NPC.defense += 10;

            Phase = AIPhase.Anmi;
        }

        public void SwitchPhaseP3ToP4()
        {
            State = 0;
            Timer = 0;
            SPRecorder = 0;
            Recorder2 = 3;
            Recorder3 = 0;
            CanDrawTrail = false;
            CanDamage = false;
            NPC.dontTakeDamage = true;

            Phase = AIPhase.Anmi;
        }

        public void ExchangeAnmiP1ToP2(NPC controller)
        {
            const int maxTime = 160;

            Vector2 TargetPos = controller.Center;

            NPC.velocity = Vector2.Zero;
            NPC.dontTakeDamage = true;
            Timer++;

            if (Timer < (int)(maxTime * 0.85f))
            {
                Main.audioSystem.PauseAll();
                for (int i = 0; i < Main.musicFade.Length; i++)
                    Main.musicFade[i] = 0;
            }
            else if (Timer == (int)(maxTime * 0.85f))
            {
                Main.musicFade[Main.curMusic] = 1;
            }

            if (Timer < maxTime * 2 / 3)
            {
                NPC.rotation += MathF.Sin(Timer * 0.75f) * 0.3f;
                for (int i = 0; i < 3; i++)
                {
                    Vector2 pos = Helper.NextVec2Dir(1, 25);
                    Dust.NewDustPerfect(NPC.Center + pos, DustID.Blood, pos.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(2, 7)
                      , Scale: Main.rand.NextFloat(1, 2.5f));
                }

                if (Timer % 20 == 0)
                {
                    SoundEngine.PlaySound(CoraliteSoundID.Fleshy2_NPCHit7, NPC.Center);
                }
                return;
            }

            //移动和旋转
            if (NPC.whoAmI < OtherEyeIndex)
                NPC.Center = Vector2.Lerp(NPC.Center
                    , TargetPos + (-MathHelper.PiOver4).ToRotationVector2() * (CircleLimit.MaxLength - 120), 0.1f);
            else
                NPC.Center = Vector2.Lerp(NPC.Center
                    , TargetPos + (MathHelper.Pi + MathHelper.PiOver4).ToRotationVector2() * (CircleLimit.MaxLength - 120), 0.1f);

            NPC.rotation = NPC.rotation.AngleLerp((TargetPos - NPC.Center).ToRotation(), 0.1f);

            if (Timer > maxTime)
            {
                Phase = AIPhase.P2;

                NPC.dontTakeDamage = false;
                ExchangeState();
            }
        }

        public void ExchangeAnmiP2ToP3(NPC controller)
        {
            const int maxTime = 300;

            Vector2 TargetPos = controller.Center;

            NPC.velocity = Vector2.Zero;
            NPC.dontTakeDamage = true;
            Timer++;

            if (Timer < maxTime -10)
            {
                Main.audioSystem.PauseAll();
                for (int i = 0; i < Main.musicFade.Length; i++)
                    Main.musicFade[i] = 0;
            }
            else if (Timer == maxTime-10)
            {
                Main.musicFade[Main.curMusic] = 1;
            }

            if (Timer < maxTime * 1 / 5)
            {
                NPC.rotation += MathF.Sin(Timer * 0.75f) * 0.3f;
                for (int i = 0; i < 3; i++)
                {
                    Vector2 pos = Helper.NextVec2Dir(1, 25);
                    Dust.NewDustPerfect(NPC.Center + pos, DustID.Blood, pos.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(2, 4)
                      , Scale: Main.rand.NextFloat(1, 2.5f));
                }

                if (Timer % 20 == 0)
                {
                    SoundEngine.PlaySound(CoraliteSoundID.Fleshy2_NPCHit7, NPC.Center);
                }
                return;
            }

            if (Timer < maxTime * 2 / 5)
            {
                NPC.rotation = NPC.rotation.AngleLerp((TargetPos - NPC.Center).ToRotation(), 0.1f);

                if (NPC.whoAmI < OtherEyeIndex)
                    NPC.Center = Vector2.Lerp(NPC.Center
                        , TargetPos + (-MathHelper.PiOver4).ToRotationVector2() * (CircleLimit.MaxLength - 120), 0.1f);
                else
                    NPC.Center = Vector2.Lerp(NPC.Center
                        , TargetPos + (MathHelper.Pi + MathHelper.PiOver4).ToRotationVector2() * (CircleLimit.MaxLength - 120), 0.1f);

                return;
            }

            if (Timer == maxTime * 2 / 5)
            {
            }

            if (Timer < maxTime * 3 / 5)
            {
                NPC.rotation += (NPC.whoAmI < OtherEyeIndex ? -1 : 1) * Helper.Lerp(0.1f, 0.6f, (Timer - maxTime * 2 / 5) / (maxTime / 5));

                return;
            }

            if (Timer == maxTime * 3 / 5)
            {
                SteelPhase = true;
                NPC.frame.Y = 3;

                for (int i = 0; i < 30; i++)
                {
                    Vector2 pos = Helper.NextVec2Dir(1, 25);
                    Dust.NewDustPerfect(NPC.Center + pos, DustID.Blood, pos.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(2, 7)
                      , Scale: Main.rand.NextFloat(1.5f, 2.5f));
                }

                for (int i = 0; i < 2; i++)
                {
                    Gore.NewGore(NPC.GetSource_FromAI(), NPC.Center + Main.rand.NextVector2Circular(25, 25)
                        , Helper.NextVec2Dir(3, 6), 9, 1.3f);
                }

                ExchangeGore();
            }


            if (Timer < maxTime * 3 / 5 + 30)
            {
                NPC.rotation = NPC.rotation.AngleLerp((TargetPos - NPC.Center).ToRotation(), 0.1f);

                return;
            }

            if (Timer == maxTime * 3 / 5 + 30)
            {
                SoundEngine.PlaySound(CoraliteSoundID.ForceRoar with { Pitch=0.5f}, NPC.Center);

                return;
            }

            if (Timer % 10 == 0)
            {
                Dust.NewDustPerfect(NPC.Center, ModContent.DustType<RoaringWave>()
                    , Vector2.Zero, 0, Color.White * 0.8f, 0.4f);
            }

            NPC.rotation += MathF.Sin(Timer * 0.75f) * 0.1f;


            if (Timer > maxTime)
            {
                Phase = AIPhase.P3;

                combineMove = true;
                NPC.dontTakeDamage = false;
                ExchangeState();
            }
        }

        public void ExchangeAnmiP3ToP4(NPC controller)
        {
            const int maxTime = 320;

            NPC.velocity = Vector2.Zero;
            if (Timer == 0)
            {
                (controller.ModNPC as CircleLimit).SwitchBGM(AIPhase.P4);
            }
            if (Timer < maxTime - 10)
            {
                Main.audioSystem.PauseAll();
                for (int i = 0; i < Main.musicFade.Length; i++)
                    Main.musicFade[i] = 0;
            }
            else if (Timer == maxTime - 10)
            {
                Main.musicFade[Main.curMusic] = 1;
            }

            Vector2 TargetPos = controller.Center;
            if (Timer < maxTime - 160)
            {
                NPC.rotation = NPC.rotation.AngleLerp((TargetPos - NPC.Center).ToRotation(), 0.1f);

                if (NPC.whoAmI < OtherEyeIndex)
                    NPC.Center = Vector2.Lerp(NPC.Center
                        , TargetPos + (-MathHelper.PiOver4).ToRotationVector2() * (CircleLimit.MaxLength - 120), 0.1f);
                else
                    NPC.Center = Vector2.Lerp(NPC.Center
                        , TargetPos + (MathHelper.Pi + MathHelper.PiOver4).ToRotationVector2() * (CircleLimit.MaxLength - 120), 0.1f);
            }

            else if (Timer < maxTime - 40)
            {
                if (Main.netMode!=NetmodeID.MultiplayerClient&&NPC.life < NPC.lifeMax / 3 && Timer % 20 == 0)
                {
                    NPC.life += NPC.lifeMax / (4 * 7);
                    NPC.netUpdate = true;
                }

                NPC.rotation += MathF.Sin((Timer - (maxTime - 160)) / 120 * MathHelper.Pi) * 0.6f;
            }
            else
            {
                NPC.rotation = NPC.rotation.AngleTowards((TargetPos - NPC.Center).ToRotation(), 0.1f);
            }

            Timer++;
            if (Timer > maxTime)
            {
                Phase = AIPhase.P4;

                NPC.dontTakeDamage = false;
                ExchangeState();
            }
        }

        public void SwitchPhaseKill()
        {
            NPC.life = 1;
            State = 0;
            Timer = 0;
            SPRecorder = 0;
            Recorder2 = 4;
            Recorder3 = 0;
            CanDrawTrail = false;
            CanDamage = false;

            Phase = AIPhase.Anmi;
        }

        public void KillAnmi()
        {
            Timer++;

            NPC.velocity = Vector2.Zero;
            NPC.rotation += MathF.Sin(Timer * 0.75f) * 0.3f;

            if (Timer == 0)
            {
                Particle.NewParticle(NPC.Center, Vector2.Zero, Contents.ParticleType<FinalHit>()
                 , newColor: Color.White, Scale: 1.25f);
            }

            for (int i = 0; i < 4; i++)
            {
                Vector2 pos = Helper.NextVec2Dir(1, 25);
                Particle.NewParticle(NPC.Center + pos, pos.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(10, 30), Contents.ParticleType<Fog>()
                 , newColor: Color.White * 0.35f, Scale: Main.rand.NextFloat(2f, 4f));
            }

            for (int i = 0; i < 2; i++)
            {
                Vector2 pos = Helper.NextVec2Dir(1, 30);
                Dust.NewDustPerfect(NPC.Center + pos, DustID.SilverCoin, pos.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(3, 15)
                  , Scale: Main.rand.NextFloat(0.6f, 1.3f));

                pos = Helper.NextVec2Dir(20, 40);
                Dust.NewDustPerfect(NPC.Center + pos, ModContent.DustType<SpeedLine>(), pos.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(7, 30)
                  , newColor: Color.White, Scale: Main.rand.NextFloat(0.1f, 0.3f));
            }

            if (Timer % 10 == 0)
            {
                Dust.NewDustPerfect(NPC.Center, ModContent.DustType<RoaringWave>()
                    , Vector2.Zero, 0, Color.White * 0.2f, 0.4f);
            }

            if (Timer % 20 == 0)
            {
                SoundEngine.PlaySound(CoraliteSoundID.Metal_NPCHit4, NPC.Center);
            }

            if (Timer > 160)
            {
                Helper.PlayPitched("DeathBoom", 1, 0, NPC.Center);

                Dust.NewDustPerfect(NPC.Center, ModContent.DustType<CircleExplode>(), Vector2.Zero
                    , newColor: Color.White * 0.75f, Scale: 0.1f);

                for (int i = -9; i < 10; i++)
                {
                    if (i == 0)
                        continue;
                    Dust.NewDustPerfect(NPC.Center, ModContent.DustType<SpeedLine>()
                        , new Vector2(0, MathF.Sign(i) * 0.1f + i * 2f)
                      , newColor: Color.White * 0.3f, Scale: 1f - MathF.Abs(i) / 9f * 0.5f);
                }

                NPC.Kill();
                KillGore();
            }
        }

        public virtual void KillGore()
        {

        }

        public virtual void ExchangeGore()
        {

        }

        #endregion

        public void DashAttack(NPC controller, int attackTime, float speed, float pitch, float exRot = 0,bool playRoar=false)
        {
            if (Timer == 0)
            {
                Recorder2 = NPC.rotation;
                NPC.velocity = Vector2.Zero;

                if (Main.netMode != NetmodeID.Server)
                {
                    HaloSound();

                    Particle d = Particle.NewParticleDirect(NPC.Center, Vector2.Zero,Contents.ParticleType<AimLine>()
                        , Color.White, 1);
                    d.data = new AimLine.IncomeData()
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

            //if (Timer < attackTime / 2)
            //{
            //    NPC.rotation = (controller.Center - NPC.Center).ToRotation() + SPRecorder;

            //    return;
            //}

            if (Timer == attackTime / 2)
            {
                DashSound();

                if (playRoar)
                    SoundEngine.PlaySound(CoraliteSoundID.ForceRoar with { Pitch = pitch }, NPC.Center);

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

            //只有一只眼睛执行该操作，在冲刺交错的时候释放闪光特效
            if (NPC.whoAmI < OtherEyeIndex && OtherEyeIndex.GetNPCOwner(out NPC other))
            {
                if (Recorder3 == 0)//还没分离
                {
                    if (Vector2.Distance(NPC.Center, other.Center) > 30)
                        Recorder3 = 1;
                }
                else if (Recorder3 == 1)//分离了检测交叉
                {
                    if (Vector2.Distance(NPC.Center, other.Center) < 30)
                    {
                        Color c2 = Color.Red;
                        Dust.NewDustPerfect(NPC.Center, ModContent.DustType<ScreenLightParticle>(), Vector2.Zero,
                          0, c2 * 0.1f, 4f);
                        Dust.NewDustPerfect(NPC.Center, ModContent.DustType<LightCiecleParticle>(), Vector2.Zero,
                          0, c2 * 0.2f, 0.25f);

                        NPC.NewProjectileInAI<Boom>(NPC.Center, Vector2.Zero, Helper.GetProjDamage(100, 125, 150)
                            , 0);


                        if (Phase is AIPhase.P1 or AIPhase.P2)
                        {
                            for (int i = 0; i < 15; i++)
                            {
                                Vector2 pos = Helper.NextVec2Dir(1, 25);
                                Dust.NewDustPerfect(NPC.Center + pos, DustID.Blood, pos.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(2, 4)
                                  , Scale: Main.rand.NextFloat(1, 2));
                            }

                            float dir = NPC.rotation;
                            for (int k = 0; k < 2; k++)
                            {
                                for (int i = 0; i < 24; i++)
                                {
                                    float dir2 = dir;

                                    if (Main.rand.NextBool())
                                        dir2 += MathHelper.Pi;

                                    Dust.NewDustPerfect(NPC.Center, DustID.Blood
                                        , (dir2 + Main.rand.NextFloat(-0.2f, 0.2f)).ToRotationVector2() * Main.rand.NextFloat(4, 10)
                                      , Scale: Main.rand.NextFloat(1.5f, 2.5f));
                                }

                                dir = other.rotation;
                            }
                        }

                        Recorder3 = 2;
                        Helper.PlayPitched("DashTogether", 0.6f, 0, NPC.Center);
                        SoundEngine.PlaySound(CoraliteSoundID.Fleshy_NPCDeath1 with { Pitch = -0.7f }, NPC.Center);
                    }
                }
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

        public void CombineP1(NPC controller)
        {
            int attackTime = P1AttackTime * 3;

            //朝向中间旋转
            if (Timer <= attackTime / 12)
            {
                if (Timer == 0)//生成瞄准线
                {
                    NPC.velocity = Vector2.Zero;

                    CenterCircleSound();
                    ShineLine(attackTime / 6, Color.White, NPC.whoAmI, (controller.Center - NPC.Center).ToRotation());

                    if (NPC.whoAmI < OtherEyeIndex)
                    {
                        Particle p = Particle.NewParticleDirect(controller.Center, Vector2.Zero
                            , Contents.ParticleType<Circle>(),Color.White,0.1f);
                        p.data = attackTime / 6;
                        SPRecorder = Main.rand.Next(8) * MathHelper.PiOver4;
                        if (OtherEyeIndex.GetNPCOwner(out NPC other))
                        {
                            other.ai[3] = SPRecorder;
                        }
                    }

                    Recorder2 = NPC.rotation;
                }

                float targetRot = (controller.Center - NPC.Center).ToRotation();//最终目标方向
                NPC.rotation = Recorder2.AngleLerp(targetRot, Helper.SqrtEase(Timer / (attackTime / 12)));
            }

            Timer++;

            //冲向中心点
            if (Timer == attackTime / 12 * 2)
            {
                float length = Vector2.Distance(NPC.Center, controller.Center);
                float time = attackTime / 12;

                DashSound();

                CanDamage = true;
                CanDrawTrail = true;
                NPC.InitOldPosCache(10, false);
                NPC.InitOldRotCache(10);
                NPC.velocity = (controller.Center - NPC.Center).SafeNormalize(Vector2.Zero) * length / time;
                NPC.rotation = NPC.velocity.ToRotation();
            }

            if (Timer < attackTime / 12 * 3)
            {
                return;
            }

            if (Timer == attackTime / 12 * 3)//设置转转乐的初始值
            {
                CenterMeetSound();

                NPC.velocity = Vector2.Zero;
                if (OtherEyeIndex > NPC.whoAmI)
                    Recorder3 = MathHelper.Pi;
                else
                    Recorder3 = 0;
            }

            if (Timer <= attackTime / 12 * 6)//转转乐
            {
                float factor = Timer - attackTime / 12 * 3;
                factor /= attackTime / 12 * 3;
                factor = Helper.X2Ease(factor);

                float rot = SPRecorder + Recorder3 + factor * MathHelper.TwoPi * 2;
                Vector2 targetPos = controller.Center + rot.ToRotationVector2() * 75;

                NPC.Center = Vector2.SmoothStep(NPC.Center, targetPos, 0.1f + factor * 0.9f);
                NPC.rotation = NPC.rotation.AngleLerp((NPC.Center - controller.Center).ToRotation() + MathHelper.PiOver2, 0.25f);
                return;
            }

            //旋转
            if (Timer == attackTime / 12 * 6)
            {

            }
            if (Timer < attackTime / 12 * 11)
            {
                //旋转向两侧
                float factor = Timer - attackTime / 12 * 6;
                factor /= attackTime / 12 * 5;

                float length = Helper.Lerp(75, CircleLimit.MaxLength - 60, factor);

                factor = Helper.SqrtEase(factor);

                float rot = SPRecorder + Recorder3 + factor * (MathHelper.TwoPi * 2 + MathHelper.PiOver2);

                Vector2 targetPos = controller.Center + rot.ToRotationVector2() * length;

                NPC.Center = Vector2.SmoothStep(NPC.Center, targetPos, 0.25f + factor * 0.75f);
                NPC.rotation = (NPC.position - NPC.oldPosition).ToRotation();
                return;
            }

            {
                Vector2 targetPos = controller.Center + (SPRecorder + Recorder3 + MathHelper.PiOver2).ToRotationVector2() * (CircleLimit.MaxLength - 60);
                NPC.Center = Vector2.SmoothStep(NPC.Center, targetPos, 0.25f);
            }

            if (Timer >= attackTime)
            {
                ExchangeState();
            }
        }

        public void CombineP2(NPC controller)
        {
            int attackTime = P2AttackTime;

            //朝向中间旋转
            if (Timer <= attackTime / 4)
            {
                if (Timer == 0)//生成瞄准线
                {
                    NPC.velocity = Vector2.Zero;

                    CenterCircleSound();
                    ShineLine(attackTime / 2, Color.White, NPC.whoAmI, (controller.Center - NPC.Center).ToRotation());

                    if (NPC.whoAmI < OtherEyeIndex)
                    {
                        Particle p = Particle.NewParticleDirect(controller.Center, Vector2.Zero
                            , Contents.ParticleType<Circle>(), Color.White, 0.1f);
                        p.data = attackTime / 2;
                        SPRecorder = Main.rand.Next(8) * MathHelper.PiOver4;
                        if (OtherEyeIndex.GetNPCOwner(out NPC other))
                        {
                            other.ai[3] = SPRecorder;
                        }
                    }

                    Recorder2 = NPC.rotation;
                }

                float targetRot = (controller.Center - NPC.Center).ToRotation();//最终目标方向
                NPC.rotation = Recorder2.AngleLerp(targetRot, Helper.SqrtEase(Timer / (attackTime / 4)));
            }

            Timer++;

            //冲向中心点
            if (Timer == attackTime / 4 * 2)
            {
                float length = Vector2.Distance(NPC.Center, controller.Center);
                float time = attackTime / 4;

                DashSound();
                CanDamage = true;
                CanDrawTrail = true;
                NPC.InitOldPosCache(10, false);
                NPC.InitOldRotCache(10);
                NPC.velocity = (controller.Center - NPC.Center).SafeNormalize(Vector2.Zero) * length / time;
                NPC.rotation = NPC.velocity.ToRotation();
            }

            if (Timer < attackTime / 4 * 3)
            {
                return;
            }

            if (Timer == attackTime / 4 * 3)//设置转转乐的初始值
            {
                CenterMeetSound();

                NPC.velocity = Vector2.Zero;
                if (OtherEyeIndex > NPC.whoAmI)
                    Recorder3 = MathHelper.Pi;
                else
                    Recorder3 = 0;

                SPRecorder = (Main.player[controller.target].Center - controller.Center).ToRotation() - MathHelper.PiOver4 * 3;
            }

            if (Timer < attackTime / 4 * 6)//转转乐
            {
                float factor = Timer - attackTime / 4 * 3;
                factor /= attackTime / 4 * 3;
                factor = Helper.X2Ease(factor);

                float rot = SPRecorder + Recorder3 + factor * MathHelper.TwoPi;
                Vector2 targetPos = controller.Center + rot.ToRotationVector2() * 75;

                NPC.Center = Vector2.SmoothStep(NPC.Center, targetPos, 0.1f + factor * 0.9f);
                NPC.rotation = NPC.rotation.AngleLerp((NPC.Center - controller.Center).ToRotation() + MathHelper.PiOver2, 0.25f);
                return;
            }

            //旋转
            if (Timer == attackTime / 4 * 6)
            {
                CanDamage = true;
                CanDrawTrail = true;
                NPC.InitOldPosCache(10, false);
                NPC.InitOldRotCache(10);
            }

            if (Timer <= attackTime / 4 * 15)
            {
                //旋转
                float factor = Timer - attackTime / 4 * 6;
                factor /= attackTime / 4 * 9;
                factor = Helper.X2Ease(factor);

                float rot = SPRecorder + Recorder3 + factor * (MathHelper.TwoPi + MathHelper.PiOver4/2);
                Vector2 targetPos = controller.Center + rot.ToRotationVector2() * 75;

                NPC.Center = Vector2.SmoothStep(NPC.Center, targetPos, 0.2f + factor * 0.8f);
                NPC.rotation = NPC.rotation.AngleLerp((NPC.Center - controller.Center).ToRotation(), 0.25f);

                if (Timer > attackTime / 4 * 8)
                    CombineP2Attack();
                return;
            }

            //{
            //    Vector2 targetPos = controller.Center + (SPRecorder + Recorder3 + MathHelper.PiOver2).ToRotationVector2() * (CircleLimit.MaxLength - 60);
            //    NPC.Center = Vector2.SmoothStep(NPC.Center, targetPos, 0.25f);
            //}

            if (Timer >= attackTime * 4)
            {
                ExchangeState();
            }
        }

        public void CombineP3_Rot(NPC controller)
        {
            int attackTime = P3AttackTime;

            if (Timer == 0)
            {
                if (Main.npc[(int)OtherEyeIndex].ai[2] < State)//多等一下
                    return;

                if (Main.npc[(int)OtherEyeIndex].localAI[0] < Timer)//多等一下
                    return;
            }

            //朝向中间旋转
            if (Timer <= attackTime / 4)
            {
                if (Timer == 0)//生成瞄准线
                {
                    NPC.velocity = Vector2.Zero;

                    CenterCircleSound();
                    ShineLine(attackTime / 2, Color.White, NPC.whoAmI, (controller.Center - NPC.Center).ToRotation());

                    if (NPC.whoAmI < OtherEyeIndex)
                    {
                        Particle p = Particle.NewParticleDirect(controller.Center, Vector2.Zero
                            , Contents.ParticleType<Circle>(), Color.White, 0.1f);
                        p.data = attackTime / 2;
                        SPRecorder = Main.rand.Next(8) * MathHelper.PiOver4;
                        if (OtherEyeIndex.GetNPCOwner(out NPC other))
                        {
                            other.ai[3] = SPRecorder;
                        }
                    }

                    Recorder2 = NPC.rotation;
                }

                float targetRot = (controller.Center - NPC.Center).ToRotation();//最终目标方向
                NPC.rotation = Recorder2.AngleLerp(targetRot, Helper.SqrtEase(Timer / (attackTime / 4)));
            }

            Timer++;

            //冲向中心点
            if (Timer == attackTime / 4 * 2)
            {
                float length = Vector2.Distance(NPC.Center, controller.Center);
                float time = attackTime / 4;

                DashSound();
                CanDamage = true;
                CanDrawTrail = true;
                NPC.InitOldPosCache(10, false);
                NPC.InitOldRotCache(10);
                NPC.velocity = (controller.Center - NPC.Center).SafeNormalize(Vector2.Zero) * length / time;
                NPC.rotation = NPC.velocity.ToRotation();
            }

            if (Timer < attackTime / 4 * 3)
            {
                return;
            }

            if (Timer == attackTime / 4 * 3)//设置转转乐的初始值
            {
                CenterMeetSound();

                NPC.velocity = Vector2.Zero;
                if (OtherEyeIndex > NPC.whoAmI)
                    Recorder3 = MathHelper.Pi;
                else
                    Recorder3 = 0;

                SPRecorder = (Main.player[controller.target].Center - controller.Center).ToRotation() - MathHelper.PiOver4 * 3;
            }

            if (Timer < attackTime / 4 * 6)//转转乐
            {
                float factor = Timer - attackTime / 4 * 3;
                factor /= attackTime / 4 * 3;
                factor = Helper.X2Ease(factor);

                float rot = SPRecorder + Recorder3 + factor * MathHelper.TwoPi;
                Vector2 targetPos = controller.Center + rot.ToRotationVector2() * 75;

                NPC.Center = Vector2.SmoothStep(NPC.Center, targetPos, 0.1f + factor * 0.9f);
                NPC.rotation = NPC.rotation.AngleLerp((NPC.Center - controller.Center).ToRotation() + MathHelper.PiOver2, 0.25f);
                return;
            }

            //旋转
            int totalRotTime = attackTime / 4 * 22;
            if (Timer == attackTime / 4 * 6)
            {
                CanDamage = true;
                CanDrawTrail = true;
                NPC.InitOldPosCache(10, false);
                NPC.InitOldRotCache(10);

                CombineP3_RotStart(controller, totalRotTime);
            }

            if (Timer <= attackTime / 4 * 6 + totalRotTime)
            {
                //旋转
                float factor = Timer - attackTime / 4 * 6;
                factor /= totalRotTime;
                //factor = Helper.BezierEase(factor);

                float rot = SPRecorder + Recorder3 + factor * MathHelper.TwoPi * 2;
                Vector2 targetPos = controller.Center + rot.ToRotationVector2() * 75;

                NPC.Center = Vector2.SmoothStep(NPC.Center, targetPos, 0.2f + factor * 0.8f);
                NPC.rotation = NPC.rotation.AngleLerp((NPC.Center - controller.Center).ToRotation(), 0.25f);

                if (Timer > attackTime / 4 * 8)
                    CombineP3_Roting(controller);
                return;
            }

            //{
            //    Vector2 targetPos = controller.Center + (SPRecorder + Recorder3 + MathHelper.PiOver2).ToRotationVector2() * (CircleLimit.MaxLength - 60);
            //    NPC.Center = Vector2.SmoothStep(NPC.Center, targetPos, 0.25f);
            //}

            if (Timer >= attackTime / 4 * 6 + totalRotTime + attackTime / 4)
            {
                combineMove = true;
                ExchangeState();
            }
        }

        public virtual void CombineP3_RotStart(NPC controller,int rotTime) { }
        public virtual void CombineP3_Roting(NPC controller) { }

        public void CombinwP3_Lines(NPC controller, bool setCombineSet)
        {
            int waitTime = P3AttackTime / 4;
            if (Timer == 0)
            {
                if (Main.npc[(int)OtherEyeIndex].ai[2] < State)//多等一下
                    return;

                if (Main.npc[(int)OtherEyeIndex].localAI[0] < Timer)//多等一下
                    return;
            }

            if (Timer < waitTime)
            {
                if (Timer == 0)
                {
                    NPC.velocity = Vector2.Zero;

                    if (NPC.whoAmI > OtherEyeIndex)
                        SPRecorder = MathHelper.PiOver4;
                    else
                        SPRecorder = -MathHelper.PiOver4;

                    SPRecorder += MathF.Sin(State * MathHelper.PiOver2) * MathHelper.PiOver4;

                    Recorder2 = NPC.rotation;
                }

                float targetRot = (controller.Center - NPC.Center).ToRotation() + SPRecorder;//最终目标方向
                NPC.rotation = Recorder2.AngleLerp(targetRot, Helper.SqrtEase(Timer / waitTime));
            }

            Timer++;

            int readyTime = (int)(P3AttackTime * 3f / 4);

            //准备生成
            if (Timer == waitTime)
            {
                NPC.rotation = (controller.Center - NPC.Center).ToRotation() + SPRecorder;

                CombineP3Attack(controller, readyTime);
            }

            //准备时间
            if (Timer < waitTime + readyTime)
            {

            }


            if (Timer > waitTime + readyTime + P3AttackTime)
            {
                if (setCombineSet)
                    combineMove = true;
                ExchangeState();
            }
        }

        public virtual void CombineP3Attack(NPC controller,int readyTime)
        {

        }

        public void CombineP4(NPC controller)
        {
            int attackTime = P1AttackTime;

            //朝向中间旋转
            if (Timer <= attackTime / 4)
            {
                if (Timer == 0)//生成瞄准线
                {
                    NPC.velocity = Vector2.Zero;

                    CenterCircleSound();
                    ShineLine(attackTime / 2, Color.White, NPC.whoAmI, (controller.Center - NPC.Center).ToRotation());

                    Particle p = Particle.NewParticleDirect(controller.Center, Vector2.Zero
                        , Contents.ParticleType<Circle>(), Color.White, 0.1f);
                    p.data = attackTime / 2;
                    SPRecorder = Main.rand.Next(8) * MathHelper.PiOver4;
                    if (OtherEyeIndex.GetNPCOwner(out NPC other))
                    {
                        other.ai[3] = SPRecorder;
                    }

                    Recorder2 = NPC.rotation;
                }

                float targetRot = (controller.Center - NPC.Center).ToRotation();//最终目标方向
                NPC.rotation = Recorder2.AngleLerp(targetRot, Helper.SqrtEase(Timer / (attackTime / 4)));
            }

            Timer++;

            //冲向中心点
            if (Timer == attackTime / 4 * 2)
            {
                float length = Vector2.Distance(NPC.Center, controller.Center);
                float time = attackTime / 4;

                DashSound();
                CanDamage = true;
                CanDrawTrail = true;
                NPC.InitOldPosCache(10, false);
                NPC.InitOldRotCache(10);
                NPC.velocity = (controller.Center - NPC.Center).SafeNormalize(Vector2.Zero) * length / time;
                NPC.rotation = NPC.velocity.ToRotation();
            }

            if (Timer < attackTime / 4 * 3)
            {
                return;
            }

            if (Timer == attackTime / 4 * 3)//设置转转乐的初始值
            {
                CenterMeetSound();

                NPC.velocity = Vector2.Zero;
                if (OtherEyeIndex > NPC.whoAmI)
                    Recorder3 = MathHelper.Pi;
                else
                    Recorder3 = 0;

                SPRecorder = (Main.player[controller.target].Center - controller.Center).ToRotation() - MathHelper.PiOver4 * 3;
            }

            if (Timer < attackTime / 4 * 10)//转转乐
            {
                float factor = Timer - attackTime / 4 * 3;
                factor /= attackTime / 4 * 7;
                factor = Helper.SqrtEase(factor);

                float rot = SPRecorder + Recorder3 + factor * MathHelper.TwoPi*2;
                Vector2 targetPos = controller.Center + rot.ToRotationVector2() * 75;

                NPC.Center = Vector2.SmoothStep(NPC.Center, targetPos, 0.1f + factor * 0.9f);
                NPC.rotation = NPC.rotation.AngleLerp((NPC.Center - controller.Center).ToRotation() + MathHelper.PiOver2, 0.25f);
                return;
            }

            ExchangeState();
        }

        public float GetLength(Vector2 pos, Vector2 dir, NPC circle)
        {
            bool closer = true;

            Vector2 posRecord = pos;

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

            return Vector2.Distance(pos, posRecord);
        }

        public void Wait(int time)
        {
            Timer++;
            NPC.velocity = Vector2.Zero;
            if (Timer>=time)
            {
                WaitState = 0;
                ExchangeState();
                State--;
            }
        }

        public virtual void CombineP2Attack() { }

        public void ShineLine(int attackTime, Color color, int owner, float? rot=null,int? circleIndex=null)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                Particle d = Particle.NewParticleDirect(NPC.Center, Vector2.Zero, Contents.ParticleType<AimLine>()
                    , color, 1);
                d.data = new AimLine.IncomeData()
                {
                    index = owner,
                    circleIndex=circleIndex,
                    maxTime = attackTime,
                    rot = rot
                };
            }
        }

        public void ExchangeState()
        {
            Timer = 0;
            SPRecorder = 0;
            Recorder3 = 0;
            CanDrawTrail = false;
            CanDamage = false;
            NPC.dontTakeDamage = false;

        pre:
            switch (Phase)
            {
                case AIPhase.Anmi:
                    Phase = AIPhase.P1;
                    goto pre;
                case AIPhase.P1:
                    P1State();
                    break;
                case AIPhase.P2:
                    P2State();
                    break;
                case AIPhase.P3:
                    P3State();
                    break;
                case AIPhase.P4:
                    P4State();
                    break;
                default:
                    break;
            }
        }


        public virtual void P1State()
        {

        }

        public virtual void P2State()
        {

        }

        public virtual void P3State()
        {

        }

        public virtual void P4State()
        {
            SPRecorder = Main.rand.Next(-2, 3) * MathHelper.PiOver4 / 4;
            if (State % 5 == 0)
            {
                SPRecorder = 0;
            }

            State++;
        }

        #endregion

        #region 音效部分


        public void CenterMeetSound()
        {
            Helper.PlayPitched("CenterMeet", 0.6f, 0, NPC.Center);
        }

        public void CenterCircleSound()
        {
            Helper.PlayPitched("CenterCircle", 0.8f, 0, NPC.Center);
        }

        public void DashSound()
        {
            Helper.PlayPitched("Dash", 0.7f, 0, NPC.Center);
        }

        public void HaloSound()
        {
            if (Phase == AIPhase.P1)
                Helper.PlayPitched("HaloP1", 1f, 0, NPC.Center);
            else
            {
                int value = Main.rand.Next(1, 5);
                Helper.PlayPitched("HaloP2_"+value.ToString(), 1f, 0, NPC.Center);
            }
        }

        public void TickSound()
        {
            SoundEngine.PlaySound(CoraliteSoundID.Metal_NPCHit4 with { Pitch = -0.3f }, NPC.Center);
        }

        #endregion

        public sealed override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            return CanDamage;
        }

        public override bool? CanFallThroughPlatforms()
        {
            return true;
        }

        /// <summary>
        /// 简单更新帧图
        /// </summary>
        /// <param name="counterMax"></param>
        public void UpdateFrame(int counterMax = 6)
        {
            if (++NPC.frameCounter > counterMax)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y++;
                if (SteelPhase)
                {
                    if (NPC.frame.Y > 5)
                        NPC.frame.Y = 3;
                }
                else
                {
                    if (NPC.frame.Y > 2)
                        NPC.frame.Y = 0;
                }
            }
        }

        public override void ModifyHitByItem(Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            HitEffects(NPC.Center + Main.rand.NextVector2Circular(NPC.width / 2, NPC.height / 2), (NPC.Center - player.Center).ToRotation());
        }

        public override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            HitEffects(NPC.Center + Main.rand.NextVector2Circular(NPC.width / 2, NPC.height / 2), (NPC.Center - projectile.Center).ToRotation());

            if (projectile.friendly)
            {
                Player p = Main.player[projectile.owner];

                float distance = Vector2.Distance(p.Center, NPC.Center);
                if (distance > 250)
                    modifiers.FinalDamage -= Math.Clamp((distance - 250) / 500, 0, 1) * 0.75f;
            }
        }

        public void HitEffects(Vector2 pos,float dir)
        {
            switch (Phase)
            {
                case AIPhase.Anmi:
                case AIPhase.P1:
                case AIPhase.P2:
                    for (int i = 0; i < 10; i++)
                    {
                        float dir2 = dir;

                        if (Main.rand.NextBool(4))
                            dir2 += MathHelper.Pi;

                        Dust.NewDustPerfect(pos, DustID.Blood
                            , (dir2 + Main.rand.NextFloat(-0.2f, 0.2f)).ToRotationVector2() * Main.rand.NextFloat(4, 10)
                          , Scale: Main.rand.NextFloat(1.5f, 2.5f));
                    }

                    SoundEngine.PlaySound(CoraliteSoundID.Fleshy_NPCHit1, NPC.Center);

                    break;
                case AIPhase.P3:
                case AIPhase.P4:
                    for (int i = 0; i < 5; i++)
                    {
                        float dir2 = dir;

                        if (Main.rand.NextBool(4))
                            dir2 += MathHelper.Pi;

                       Dust.NewDustPerfect(pos, DustID.SilverCoin
                            , (dir2 + Main.rand.NextFloat(-0.2f, 0.2f)).ToRotationVector2() * Main.rand.NextFloat(3, 8)
                          , Scale: Main.rand.NextFloat(1f, 1.5f));
                    }
                    SoundEngine.PlaySound(CoraliteSoundID.Metal_NPCHit4, NPC.Center);
                    break;
                default:
                    break;
            }
        }

        public override bool CheckDead()
        {
            if (Phase == AIPhase.Anmi && Recorder2 == 4 && Timer > 150)
                return true;

            NPC.life = 1;

            if (Phase == AIPhase.Anmi && Recorder2 == 4)
                return false;

            SwitchPhaseKill();
            Helper.PlayPitched("Sting", 1, 0, NPC.Center);

            if (OtherEyeIndex.GetNPCOwner(out NPC other) && other.ModNPC is BaseTwin bt)
            {
                if (bt.Phase != AIPhase.Anmi)
                    bt.SwitchPhaseP3ToP4();
            }
            else if (CircleLimitIndex.GetNPCOwner<CircleLimit>(out NPC circle))
            {
                circle.ai[1] = 3;
                circle.localAI[0] = 0;
            }

            return false;
        }

        public override void DrawBehind(int index)
        {
            Main.instance.DrawCacheNPCProjectiles.Add(index);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (NPC.whoAmI < OtherEyeIndex && OtherEyeIndex.GetNPCOwner(out NPC n) && n.ModNPC is BaseTwin)
            {
                DrawLine(TextureAssets.Chain12.Value, NPC.Center, n.Center, screenPos);
            }

            Texture2D tex = NPC.GetTexture();

            Vector2 pos = NPC.Center - screenPos;
            Rectangle frame = tex.Frame(1, 6, 0, NPC.frame.Y);

            float rot = NPC.rotation - MathHelper.PiOver2;

            if (CanDrawTrail)
            {
                int length = NPC.oldPos.Length;
                Vector2 toCenter = NPC.Size / 2 - screenPos;

                for (int i = 0; i < length; i++)
                {
                    spriteBatch.Draw(tex, NPC.oldPos[i] + toCenter, frame, drawColor * (0.5f - 0.5f * i / length)
                        , NPC.oldRot[i] - MathHelper.PiOver2, frame.Size() / 2, NPC.scale, 0, 0);
                }
            }

            spriteBatch.Draw(tex, pos, frame, drawColor, rot, frame.Size() / 2, NPC.scale, 0, 0);

            Effect shader = ShaderLoader.GetShader( "Highlight");

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap,
                            Main.spriteBatch.GraphicsDevice.DepthStencilState, RasterizerState.CullNone, shader, Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(tex, pos, frame, HighlightColor with { A = 0 }, rot, frame.Size() / 2, NPC.scale, 0, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, Main.spriteBatch.GraphicsDevice.BlendState, Main.spriteBatch.GraphicsDevice.SamplerStates[0],
                            Main.spriteBatch.GraphicsDevice.DepthStencilState, Main.spriteBatch.GraphicsDevice.RasterizerState, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        public virtual void DrawLine(Texture2D lineTex, Vector2 startPos, Vector2 endPos, Vector2 screenPos)
        {
            List<ColoredVertex> bars = new();

            float halfLineWidth = lineTex.Width / 2;

            Vector2 recordPos = startPos;
            float recordUV = 0;

            int lineLength = (int)(startPos - endPos).Length();   //链条长度
            int pointCount = lineLength / 16 + 3;
            Vector2 controlPos = middlePos;

            //贝塞尔曲线
            for (int i = 0; i < pointCount; i++)
            {
                float factor = (float)i / pointCount;

                Vector2 P1 = Vector2.Lerp(startPos, controlPos, factor);
                Vector2 P2 = Vector2.Lerp(controlPos, endPos, factor);

                Vector2 Center = Vector2.Lerp(P1, P2, factor);
                var Color = GetStringColor(Center);

                Vector2 normal = (P2 - P1).SafeNormalize(Vector2.One).RotatedBy(MathHelper.PiOver2);
                Vector2 Top = Center + normal * halfLineWidth;
                Vector2 Bottom = Center - normal * halfLineWidth;

                recordUV += (Center - recordPos).Length() / lineTex.Width;

                bars.Add(new(Top-screenPos, Color, new Vector3(0, recordUV, 1)));
                bars.Add(new(Bottom - screenPos, Color, new Vector3(1, recordUV, 1)));

                recordPos = Center;
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, Main.spriteBatch.GraphicsDevice.BlendState, Main.spriteBatch.GraphicsDevice.SamplerStates[0],
                            Main.spriteBatch.GraphicsDevice.DepthStencilState, Main.spriteBatch.GraphicsDevice.RasterizerState, null, Main.GameViewMatrix.TransformationMatrix);

            var state = Main.graphics.GraphicsDevice.SamplerStates[0];
            Main.graphics.GraphicsDevice.Textures[0] = lineTex;
            Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
            Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, bars.ToArray(), 0, bars.Count - 2);
            Main.graphics.GraphicsDevice.SamplerStates[0] = state;
        }

        public virtual Color GetStringColor(Vector2 pos)
        {
            return Lighting.GetColor((int)pos.X / 16, (int)(pos.Y / 16f), Color.White);
        }
    }
}
