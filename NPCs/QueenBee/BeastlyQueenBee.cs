using Coralite.Content.Particles;
using Coralite.Helpers;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TheTwinsRework.Configs;
using TheTwinsRework.Core.Loader;
using TheTwinsRework.Core.System_Particle;
using TheTwinsRework.Misc;
using TheTwinsRework.Particles;
using TheTwinsRework.Projectiles;

namespace TheTwinsRework.NPCs.QueenBee
{
    public class BeastlyQueenBee : ModNPC
    {
        public override string Texture => AssetDirectory.Vanilla + "NPC_222";

        public AIStates State { get; set; }

        public static int SpawnAnmiTime = 100;

        public ref float RectLimitIndex => ref NPC.ai[0];
        public bool IsDashing
        {
            get => NPC.ai[1] == 1;
            set
            {
                if (value)
                    NPC.ai[1] = 1;
                else
                    NPC.ai[1] = 0;
            }
        }
        public ref float Recorder => ref NPC.ai[2];
        public ref float Recorder2 => ref NPC.ai[3];

        public ref float Timer => ref NPC.localAI[0];
        public ref float AngerNum => ref NPC.localAI[1];
        public ref float Recorder3 => ref NPC.localAI[2];
        private Player Target => Main.player[NPC.target];

        #region tml Hooks

        public override void SetDefaults()
        {
            NPC.boss = true;
            NPC.lifeMax = 10000;
            NPC.width = NPC.height = 80;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.knockBackResist = 0;
            //NPC.hide = true;

            if (!VisualConfigSystem.ShowBossBar)
            {
                NPC.BossBar = ModContent.GetInstance<NothingBossBar>();
            }

            Music = MusicID.Boss1;
        }

        #endregion

        #region AI

        public enum AIStates
        {
            /// <summary>
            /// 生成动画
            /// </summary>
            SpawnAnmi,
            /// <summary>
            /// 射刺
            /// </summary>
            ShootSpikes,
            /// <summary>
            /// 招小怪
            /// </summary>
            SummonMinions,
            /// <summary>
            /// 冲刺
            /// </summary>
            Dash,
            /// <summary>
            /// 下砸
            /// </summary>
            SmashDown,
            /// <summary>
            /// 死亡动画
            /// </summary>
            KillAnmi,
        }

        public override void AI()
        {
            if (!RectLimitIndex.GetNPCOwner<RectangleLimit>(out NPC controller))
            {
                NPC.velocity = Vector2.Lerp(NPC.velocity, new Vector2(NPC.whoAmI % 2 == 0 ? -6 : 6, -20), 0.1f);
                NPC.rotation = NPC.rotation.AngleLerp(0, 0.1f);
                NPC.spriteDirection = MathF.Sign(NPC.velocity.X);

                NPC.EncourageDespawn(30);

                return;
            }

            switch (State)
            {
                case AIStates.SpawnAnmi:
                    SpawnAnmi(controller);
                    break;
                case AIStates.ShootSpikes:
                    break;
                case AIStates.SummonMinions:
                    break;
                case AIStates.Dash:
                    Dash(controller);
                    break;
                case AIStates.SmashDown:
                    SmashDown(controller);
                    break;
                case AIStates.KillAnmi:
                    break;
                default:
                    break;
            }

            UpdateFrame();
        }

        public void SpawnAnmi(NPC controller)
        {
            Vector2 targetPos = controller.Center;

            Vector2 endPos = targetPos
                    + new Vector2(160 - RectangleLimit.LimitWidth / 2, 120 - RectangleLimit.LimitHeight / 2);
            Vector2 startPos = endPos + new Vector2(-1600, 1450);
            Vector2 controlPos = endPos + new Vector2(-700, -2000);

            NPC.spriteDirection = 1;

            if (Timer == 0)
            {
                NPC.Center = startPos;
            }

            Timer++;

            //飘到指定位置
            SpawnAnmiTime = 45;
            if (Timer < SpawnAnmiTime)
            {
                NPC.velocity = Vector2.Zero;

                Helper.StopMusic();

                float factor = Timer / SpawnAnmiTime;
                Recorder = factor * 0.06f;
                factor = Helper.SqrtEase(factor);

                Vector2 v1 = Vector2.Lerp(startPos, controlPos, factor);
                NPC.Center = Vector2.Lerp(v1, endPos, factor);
                return;
            }

            const int waitTime = 30;
            if (Timer < SpawnAnmiTime + waitTime)
            {
                NPC.rotation += 0.01f;
                NPC.velocity.Y += 0.06f;
                return;
            }

            if (Timer == SpawnAnmiTime + waitTime)
            {
                NPC.velocity.Y = -1.4f;
                NPC.rotation = 0;
                SoundEngine.PlaySound(CoraliteSoundID.BugsScream_Item173, NPC.Center);
            }

            //吼叫动画
            if (Timer < SpawnAnmiTime + 120 + waitTime)
            {
                NPC.velocity.Y *= 0.95f;
                Helper.StopMusic();

                if (Timer % 10 == 0)
                {
                    Dust.NewDustPerfect(NPC.Center, ModContent.DustType<RoaringWave>()
                        , Vector2.Zero, 0, Color.White * 0.8f, 0.4f);
                }

                NPC.rotation += MathF.Sin(Timer * 0.75f) * 0.1f;
                return;
            }

            if (Timer == SpawnAnmiTime + 120 + waitTime)
            {
                Music = MusicLoader.GetMusicSlot(Mod, "Sounds/Music/RIP AND SHRED");
            }
            if (Timer == SpawnAnmiTime + 120 + waitTime + 2)
            {
                OpenMusic();
            }

            if (Timer > SpawnAnmiTime + 120 + 20 + waitTime)
            {
                NPC.rotation = NPC.rotation.AngleLerp(0, 0.2f);
                ExchangeState();
            }
        }

        /// <summary>
        /// 冲冲冲
        /// </summary>
        public void Dash(NPC controller)
        {
            //设置初始值
            if (Timer == 0)
            {
                //目标Y值
                Recorder2 = (Recorder % 2 == 0 ? -1 : 1) * RectangleLimit.LimitHeight / 4;
            }

            Timer++;

            //朝玩家飘一下
            int beforeDashTime = (int)(60 / AngerNum);

            if (Timer < beforeDashTime)
            {
                SetDirection(Target.Center, out Vector2 dis);
                NPC.spriteDirection = NPC.direction;

                NPC.rotation = NPC.rotation.AngleLerp(0, 0.2f);

                float speed = 3 + AngerNum * 2f;
                float a = 0.1f + 0.1f * AngerNum;

                //int i = 0;
                if (MathF.Abs(NPC.Center.X - controller.Center.X) > RectangleLimit.LimitWidth / 2 - 140)
                {
                    //i = 1;
                    Helper.Movement_SimpleOneLine(ref NPC.velocity.X, MathF.Sign(controller.Center.X - NPC.Center.X), speed, a, a * 2, 0.97f);
                }
                else if (dis.X > RectangleLimit.LimitWidth / 2)
                {
                    //i = 2;
                    Helper.Movement_SimpleOneLine(ref NPC.velocity.X, NPC.direction, speed, a, a * 2, 0.97f);
                }
                else if (dis.X < 100)
                    Helper.Movement_SimpleOneLine(ref NPC.velocity.X, -NPC.direction, speed, a, a * 2, 0.97f);
                else
                    NPC.velocity.X *= 0.95f;

                //Main.NewText(i);

                //控制Y方向的移动
                if (dis.Y > 50)
                    Helper.Movement_SimpleOneLine(ref NPC.velocity.Y, NPC.directionY
                        , speed / 2, a / 2, a, 0.97f);
                else
                    NPC.velocity.Y *= 0.96f;

                return;
            }

            if (Timer == beforeDashTime)//根据玩家位置做偏移
            {
                NPC.velocity *= 0.2f;
                float p = 0.5f;
                if (AngerNum>1f)
                    p = 0.75f;
                Recorder2 = Helper.Lerp(Recorder2, Target.Center.Y-controller.Center.Y, p);
            }

            //向后拉，准备冲
            int makeBackTime = (int)(30 / AngerNum);
            if (Timer < beforeDashTime + makeBackTime)
            {
                SetDirection(new Vector2(Target.Center.X, controller.Center.Y + Recorder2), out Vector2 dis);
                NPC.spriteDirection = NPC.direction;

                if (MathF.Abs(NPC.Center.X - controller.Center.X) < RectangleLimit.LimitWidth / 2 - NPC.width / 2 - 60
                    && dis.X > 120)
                    Helper.Movement_SimpleOneLine(ref NPC.velocity.X, -NPC.direction
                        , 4.5f + AngerNum * 2f, 0.3f + AngerNum * 0.2f, 0.5f + AngerNum * 0.3f, 0.97f);
                else
                    NPC.velocity.X *= 0.9f;

                //控制Y方向的移动
                if (dis.Y > 40)
                    Helper.Movement_SimpleOneLine(ref NPC.velocity.Y, NPC.directionY
                        , 6.5f + AngerNum * 2f, 0.3f + AngerNum * 0.2f, 0.5f + AngerNum * 0.3f, 0.97f);
                else
                    NPC.velocity.Y *= 0.96f;

                return;
            }

            if (Timer == beforeDashTime + makeBackTime)//开始冲刺
            {
                SetDirection(new Vector2(controller.Center.X, controller.Center.Y + Recorder2), out _);
                NPC.spriteDirection = NPC.direction;

                Helper.PlayPitched("beastfly_horiz_dash_attack", 1, 0, NPC.Center);

                IsDashing = true;
                NPC.velocity *= 0.2f;
            }

            //冲
            int dashTime = (int)(80 / AngerNum);
            if (Timer < beforeDashTime + makeBackTime + dashTime)
            {
                Helper.Movement_SimpleOneLine(ref NPC.velocity.X, NPC.direction
                    , 16.5f * AngerNum, 0.8f * AngerNum, 1.2f * AngerNum, 0.97f);

                //撞墙
                Vector2 tempPos = NPC.Center + NPC.velocity;
                int dir = Math.Sign(NPC.velocity.X);
                bool collide = false;

                if (dir == 1)
                {
                    if (tempPos.X + NPC.width / 2 > controller.Center.X + RectangleLimit.LimitWidth / 2)
                    {
                        Particle.NewParticle<Slam>(NPC.Right+new Vector2(-30,30), new Vector2(-1, 0),Scale: 0.6f);
                        (controller.ModNPC as RectangleLimit).CollideRight(NPC.Center.Y, MathF.Abs(NPC.velocity.X));
                        collide = true;
                    }
                }
                else
                {
                    if (tempPos.X - NPC.width / 2 < controller.Center.X - RectangleLimit.LimitWidth / 2)
                    {
                        Particle.NewParticle<Slam>(NPC.Left + new Vector2(30, 30), new Vector2(1, 0), Scale: 0.6f);
                        (controller.ModNPC as RectangleLimit).CollideLeft(NPC.Center.Y, MathF.Abs(NPC.velocity.X));
                        collide = true;
                    }
                }

                if (collide)
                {
                    NPC.velocity = new Vector2(-dir * 3, 0);
                    Timer = beforeDashTime + makeBackTime + dashTime;

                    Helper.PlayPitched("beastfly_close_wall_hit_" + Main.rand.Next(1, 3).ToString()
                        , 1, 0, NPC.Center);

                    Recorder3 = 25;

                    NPC.NewProjectileInAI<SpikeBallProj>(
                        new Vector2(Math.Clamp(Target.Center.X, controller.Center.X - RectangleLimit.LimitWidth / 2 + 20, controller.Center.X + RectangleLimit.LimitWidth / 2 - 20),controller.Center.Y-RectangleLimit.LimitHeight/2-24)
                        , Vector2.Zero, Helper.GetProjDamage(80, 90, 120), 0, ai0: RectLimitIndex, ai2: Main.rand.Next(10));

                    if (AngerNum > 1)
                        for (int i = 0; i < 2; i++)
                            NPC.NewProjectileInAI<SpikeBallProj>(
                                new Vector2(controller.Center.X + Main.rand.NextFloat(-RectangleLimit.LimitWidth / 2 + 20, RectangleLimit.LimitWidth / 2 - 20), controller.Center.Y - RectangleLimit.LimitHeight / 2 - 24)
                                , Vector2.Zero, Helper.GetProjDamage(80, 90, 120), 0, ai0: RectLimitIndex, ai2: Main.rand.Next(10));
                    return;
                }

                if (MathF.Sign(Target.Center.X - NPC.Center.X) * NPC.direction < 0
                    && MathF.Abs(Target.Center.X - NPC.Center.X) > 265)
                {
                    NPC.velocity *= 0.3f;
                    Timer = beforeDashTime + makeBackTime + dashTime + 10;
                    IsDashing = false;
                    Recorder3 = 0;
                    Helper.PlayPitched("beastfly_horiz_dash_attack", 1, 0, NPC.Center);
                }

                return;
            }

            if (Timer < beforeDashTime + makeBackTime + dashTime + 30 + Recorder3)
            {
                if (Timer == beforeDashTime + makeBackTime + dashTime + 8)
                {
                    IsDashing = false;
                }

                NPC.velocity *= 0.95f;
                if (Recorder3 == 0)
                {
                    float factor = (Timer - beforeDashTime + makeBackTime + dashTime + 10) / 20;
                    NPC.rotation = Helper.SinEase(factor) * -NPC.spriteDirection * 0.5f;
                }
                return;
            }

            Recorder--;
            Timer = 0;
            if (Recorder <1)
            {
                ExchangeState();
            }
        }

        /// <summary>
        /// 下砸
        /// </summary>
        /// <param name="controller"></param>
        public void SmashDown(NPC controller)
        {
            if (Timer == 0)
            {
                //目标X值
                Recorder2 = Target.Center.X;
            }

            //向玩家飘
            int beforeDashTime = (int)(60 / AngerNum);

            if (Timer < beforeDashTime)
            {
                SetDirection(Target.Center, out Vector2 dis);
                NPC.spriteDirection = NPC.direction;

                NPC.rotation = NPC.rotation.AngleLerp(0, 0.2f);

                float speed = 3 + AngerNum * 2f;
                float a = 0.1f + 0.1f * AngerNum;

                //int i = 0;
                if (dis.X > RectangleLimit.LimitWidth / 2)
                {
                    //i = 2;
                    Helper.Movement_SimpleOneLine(ref NPC.velocity.X, NPC.direction, speed, a, a * 2, 0.97f);
                }
                else if (dis.X < 50)
                    Helper.Movement_SimpleOneLine(ref NPC.velocity.X, -NPC.direction, speed, a, a * 2, 0.97f);
                else
                    NPC.velocity.X *= 0.95f;

                //Main.NewText(i);

                //控制Y方向的移动
                if (dis.Y > 50)
                    Helper.Movement_SimpleOneLine(ref NPC.velocity.Y, NPC.directionY
                        , speed / 2, a / 2, a, 0.97f);
                else
                    NPC.velocity.Y *= 0.96f;

                float distance = Vector2.Distance(NPC.Center, Target.Center);
                if (distance < 100)
                    Timer += 2;

                if (distance < 300)
                    Timer++;

                Timer++;

                if (Timer > beforeDashTime)
                {
                    Recorder2 = Target.Center.X;
                    Timer = beforeDashTime;
                }

                return;
            }

            Timer++;

            //上升并翻滚
            int rollingTime = (int)(35 / AngerNum);
            if (Timer < beforeDashTime + rollingTime)
            {
                NPC.rotation = NPC.spriteDirection * (MathHelper.TwoPi+MathHelper.PiOver2)
                    * (Timer - beforeDashTime) / rollingTime;

                float targetY = controller.Center.Y - RectangleLimit.LimitHeight / 2 - 200;
                SetDirection(new Vector2(Recorder2, targetY), out Vector2 dis);

                float speed = 4 + AngerNum * 2f;
                float a = 0.1f + 0.1f * AngerNum;

                if (dis.X > 50)
                    Helper.Movement_SimpleOneLine(ref NPC.velocity.X, NPC.direction, speed, a, a * 2, 0.97f);
                else
                    NPC.velocity.X *= 0.93f;

                speed = 6 + AngerNum * 6f;
                a = 0.8f + 0.8f * AngerNum;

                //控制Y方向的移动
                if (NPC.Center.Y > targetY)
                    Helper.Movement_SimpleOneLine(ref NPC.velocity.Y, NPC.directionY
                        , speed / 2, a / 2, a, 0.97f);
                else
                    NPC.velocity.Y *= 0.8f;

                return;
            }

            //短暂前摇
            const int Pre = 20;
            if (Timer < beforeDashTime + rollingTime + Pre)
            {
                IsDashing = true;
                float speed = 6 + AngerNum * 6f;
                float a = 0.8f + 0.8f * AngerNum;

                NPC.velocity.X *= 0.95f;

                if (NPC.Center.Y > controller.Center.Y - RectangleLimit.LimitHeight / 2 - 120)
                    Helper.Movement_SimpleOneLine(ref NPC.velocity.Y, NPC.directionY
                        , speed / 2, a / 2, a, 0.97f);
                else
                    NPC.velocity.Y *= 0.8f;

                return;
            }

            if (Timer == beforeDashTime + rollingTime + Pre)
            {
                NPC.velocity = new Vector2(0, 20);

                Helper.PlayPitched("brkn_wand_down_stab_dash", 1, 0, NPC.Center);
                return;
            }

            //下砸
            const int SmashDownTime = 60;
            if (Timer < beforeDashTime + rollingTime + Pre + SmashDownTime)
            {
                Vector2 tempPos = NPC.Center + NPC.velocity;

                if (tempPos.Y + NPC.height / 2 > controller.Center.Y + RectangleLimit.LimitHeight / 2)
                {
                    (controller.ModNPC as RectangleLimit).CollideBottom(NPC.Center.X, MathF.Abs(NPC.velocity.Y));
                    NPC.velocity = new Vector2(0, -6);
                    Timer = beforeDashTime + rollingTime + Pre + SmashDownTime;

                    Helper.PlayPitched("beastfly_close_wall_hit_" + Main.rand.Next(1, 3).ToString()
                        , 1, 0, NPC.Center);

                    Recorder--;
                    IsDashing = false;

                    float dis = Math.Abs(NPC.Center.X - Target.Center.X);
                    if (dis < RectangleLimit.LimitWidth / 4)
                        Recorder2 = Target.Center.X;
                    else
                        Recorder2 = NPC.Center.X + Math.Sign(Target.Center.X - NPC.Center.X) * RectangleLimit.LimitWidth / 4;

                    Particle.NewParticle<Slam>(NPC.Bottom, new Vector2(0, -1), Scale: 0.6f);
                }

                return;
            }

            //砸完后
            if (Recorder > 0)
            {
                Timer = beforeDashTime;
                return;
            }

            if (Timer < beforeDashTime + rollingTime + Pre + SmashDownTime + 40)
            {
                if (Timer < beforeDashTime + rollingTime + Pre + SmashDownTime + 20)
                {
                    NPC.rotation = NPC.spriteDirection * (MathHelper.TwoPi - MathHelper.PiOver2)
                        * (Timer - beforeDashTime + rollingTime + Pre + SmashDownTime) / 20;
                }
                else
                    NPC.rotation = NPC.rotation.AngleLerp(0, 0.2f);

                IsDashing = false;
                NPC.velocity *= 0.95f;
                return;
            }

            ExchangeState();
        }

        public void ExchangeState()
        {
            NPC.TargetClosest();
            Timer = 0;
            Recorder = 0;
            Recorder2 = 0;
            Recorder3 = 0;
            IsDashing = false;

            AngerNum = 1;
            if (NPC.life < NPC.lifeMax / 3)
                AngerNum += 0.1f;
            if (NPC.life < NPC.lifeMax / 2)
                AngerNum += 0.15f;
            if (NPC.life < NPC.lifeMax / 5)
                AngerNum += 0.15f;

            switch (State)
            {
                case AIStates.SpawnAnmi:
                    State = AIStates.Dash;
                    break;
                case AIStates.ShootSpikes:
                    break;
                case AIStates.SummonMinions:
                    break;
                case AIStates.Dash:
                    State = Main.rand.NextFromList(AIStates.SmashDown);
                    break;
                case AIStates.SmashDown:
                    State = Main.rand.NextFromList(AIStates.Dash);
                    break;
                case AIStates.KillAnmi:
                    break;
                default:
                    break;
            }

            OnStateStart();
        }

        public void OnStateStart()
        {
            switch (State)
            {
                case AIStates.SpawnAnmi:
                    break;
                case AIStates.ShootSpikes:
                    break;
                case AIStates.SummonMinions:
                    break;
                case AIStates.Dash:
                    if (AngerNum > 1f)
                        Recorder = Main.rand.Next(1, 6);
                    else
                        Recorder = Main.rand.Next(1, 4);

                    break;
                case AIStates.SmashDown:
                    if (AngerNum > 1f)
                        Recorder = Main.rand.Next(1, 4);
                    else
                        Recorder = 3;

                    break;
                case AIStates.KillAnmi:
                    break;
                default:
                    break;
            }
        }

        public void SetDirection(Vector2 targetPos, out Vector2 distance)
        {
            if (MathF.Abs(targetPos.X - NPC.Center.X) > 16)
                NPC.direction = targetPos.X > NPC.Center.X ? 1 : -1;
            if (MathF.Abs(targetPos.Y - NPC.Center.Y) > 16)
                NPC.directionY = targetPos.Y > NPC.Center.Y ? 1 : -1;

            distance = new Vector2(Math.Abs(targetPos.X - NPC.Center.X), Math.Abs(targetPos.Y - NPC.Center.Y));
        }

        public void UpdateFrame(int counterMax = 3)
        {
            if (!IsDashing && NPC.frame.Y < 4)
                NPC.frame.Y = 4;

            if (++NPC.frameCounter > counterMax)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y++;
                if (IsDashing)
                {
                    if (NPC.frame.Y > 3)
                        NPC.frame.Y = 0;
                }
                else
                {
                    if (NPC.frame.Y > 11)
                        NPC.frame.Y = 4;
                }
            }
        }

        public void OpenMusic()
        {
            Main.musicFade[Main.curMusic] = 1;
        }

        #endregion

        #region Draw

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = NPC.GetTexture();

            Vector2 pos = NPC.Center - screenPos;
            Rectangle frame = tex.Frame(1, 12, 0, NPC.frame.Y);
            SpriteEffects eff = NPC.spriteDirection > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            float scale = NPC.scale;

            if (State == AIStates.SpawnAnmi && Timer < SpawnAnmiTime)
            {
                Effect shader = ShaderLoader.GetShader("CosmosBlur");

                shader.Parameters["blur"].SetValue(Recorder);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap,
                                Main.spriteBatch.GraphicsDevice.DepthStencilState, RasterizerState.CullNone, shader, Main.GameViewMatrix.TransformationMatrix);

                scale = Helper.Lerp(10, scale, Timer / SpawnAnmiTime);
                if (Timer < 20)
                {
                    drawColor = Color.Black;
                }
                else
                    drawColor = Color.Lerp(Color.Black, drawColor, Helper.SqrtEase((Timer - 20) / (SpawnAnmiTime - 20)));

                spriteBatch.Draw(tex, pos, frame, drawColor, NPC.rotation
                    , frame.Size() / 2, scale, eff, 0);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, Main.spriteBatch.GraphicsDevice.BlendState, Main.spriteBatch.GraphicsDevice.SamplerStates[0],
                                Main.spriteBatch.GraphicsDevice.DepthStencilState, Main.spriteBatch.GraphicsDevice.RasterizerState, null, Main.GameViewMatrix.TransformationMatrix);
            }
            else
            {
                spriteBatch.Draw(tex, pos, frame, drawColor, NPC.rotation
                    , frame.Size() / 2, scale, eff, 0);
            }

            return false;
        }

        #endregion
    }
}
