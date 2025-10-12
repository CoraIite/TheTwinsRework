using Coralite.Content.Particles;
using Coralite.Helpers;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TheTwinsRework.Configs;
using TheTwinsRework.Core.Loader;
using TheTwinsRework.Misc;

namespace TheTwinsRework.NPCs.QueenBee
{
    public class BeastlyQueenBee : ModNPC
    {
        public override string Texture => AssetDirectory.Vanilla + "NPC_222";

        public AIStates State { get; set; }

        public static int SpawnAnmiTime = 100;

        public ref float RectLimitIndex => ref NPC.ai[0];
        public bool Dashing
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
        private Player Target => Main.player[NPC.target];

        #region tml Hooks

        public override void SetDefaults()
        {
            NPC.boss = true;
            NPC.lifeMax = 100;
            NPC.width = NPC.height = 16;
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

            if (Timer > SpawnAnmiTime + 120 + 20+ waitTime)
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
            int dashCount = 3;
            if (AngerNum > 1.1f)
                dashCount += 2;

            //设置初始值
            if (Timer==0)
            {
                //目标Y值
                Recorder2 = (Recorder % 2 == 0 ? -1 : 1) * RectangleLimit.LimitHeight / 4;
            }

            Timer++;

            //朝玩家飘一下
            int beforeDashTime = (int)(60 / AngerNum);

            if (Timer<beforeDashTime)
            {
                SetDirection(new Vector2(Target.Center.X, controller.Center.Y + Recorder2),out Vector2 dis);
                if (dis.X > 50)
                    Helper.Movement_SimpleOneLine(ref NPC.velocity.X, NPC.direction, 3.5f, 0.05f, 0.1f, 0.97f);
                else
                    NPC.velocity.X *= 0.96f;

                //控制Y方向的移动
                if (dis.Y > 50)
                    Helper.Movement_SimpleOneLine(ref NPC.velocity.Y, NPC.directionY, 4.5f, 0.1f, 0.15f, 0.97f);
                else
                    NPC.velocity.Y *= 0.96f;

                return;
            }

            if (Timer==beforeDashTime)//根据玩家位置做偏移
            {
                NPC.velocity *= 0.2f;
                if (AngerNum > 1.1f)
                    Recorder2 = Helper.Lerp(Recorder2, Target.Center.Y, 0.5f);
            }

            //向后拉，准备冲
            int makeBackTime = (int)(30 / AngerNum);
            if (Timer < beforeDashTime + makeBackTime)
            {
                SetDirection(new Vector2(controller.Center.X, controller.Center.Y + Recorder2), out Vector2 dis);
                if (dis.X < RectangleLimit.LimitWidth / 2 - 180)
                    Helper.Movement_SimpleOneLine(ref NPC.velocity.X, -NPC.direction
                        , 3.5f, 0.2f, 0.15f, 0.97f);
                else
                    NPC.velocity.X *= 0.96f;

                //控制Y方向的移动
                float yLength = Math.Abs(controller.Center.Y + Recorder2 - NPC.Center.Y);
                if (yLength > 50)
                    Helper.Movement_SimpleOneLine(ref NPC.velocity.Y, NPC.directionY, 4.5f, 0.3f, 0.5f, 0.97f);
                else
                    NPC.velocity.Y *= 0.96f;

                return;
            }

            if (Timer == beforeDashTime + makeBackTime)//开始冲刺
            {
                SetDirection(new Vector2(controller.Center.X, controller.Center.Y + Recorder2), out _);

                Dashing = true;
                NPC.velocity *= 0.2f;
            }

            //冲
            int dashTime = (int)(80 / AngerNum);
            if (Timer < beforeDashTime + makeBackTime + dashTime)
            {
                Helper.Movement_SimpleOneLine(ref NPC.velocity.X, NPC.direction
                    , 10.5f * AngerNum, 0.6f * AngerNum, 0.8f, 0.97f);

                //撞墙
                Vector2 tempPos = NPC.Center + NPC.velocity;

            }



        }

        public void ExchangeState()
        {
            Timer = 0;
            Recorder=0;
            Recorder2 = 0;
            Dashing = false;

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
                    break;
                case AIStates.SmashDown:
                    break;
                case AIStates.KillAnmi:
                    break;
                default:
                    break;
            }
        }

        public void SetDirection(Vector2 targetPos,out Vector2 distance)
        {
            if (MathF.Abs(targetPos.X - NPC.Center.X) > 16)
                NPC.direction = targetPos.X > NPC.Center.X ? 1 : -1;
            if (MathF.Abs(targetPos.Y - NPC.Center.Y) > 16)
                NPC.directionY = targetPos.Y > NPC.Center.Y ? 1 : -1;

            distance =new Vector2( Math.Abs(targetPos.X - NPC.Center.X),Math.Abs(targetPos.Y - NPC.Center.Y));
        }

        public void UpdateFrame(int counterMax = 4)
        {
            if (!Dashing && NPC.frame.Y < 4)
                NPC.frame.Y = 4;

            if (++NPC.frameCounter > counterMax)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y++;
                if (Dashing)
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

            if (State == AIStates.SpawnAnmi&&Timer<SpawnAnmiTime)
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
