using Coralite.Content.Particles;
using Coralite.Helpers;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using TheTwinsRework.Configs;
using TheTwinsRework.Core;
using TheTwinsRework.Core.Loader;
using TheTwinsRework.Core.System_Particle;
using TheTwinsRework.Dusts;
using TheTwinsRework.GlobalNPCs;
using TheTwinsRework.Items;
using TheTwinsRework.Misc;
using TheTwinsRework.Particles;
using TheTwinsRework.Projectiles;

namespace TheTwinsRework.NPCs.QueenBee
{
    [AutoloadBossHead()]
    public class BeastlyQueenBee : ModNPC
    {
        public override string Texture => AssetDirectory.Vanilla + "NPC_222";
        public override string BossHeadTexture => AssetDirectory.Assets + Name + "_Head_Boss";
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

        public WeightedRandom<AIStates> wr = new WeightedRandom<AIStates>();

        #region tml Hooks

        public override void SetStaticDefaults()
        {
            NPCID.Sets.HurtingBees[Type] = true;
        }

        public override void SetDefaults()
        {
            NPC.boss = true;
            NPC.lifeMax = 3400;
            NPC.damage = 60;

            if (Main.hardMode)
            {
                NPC.lifeMax = 8000;
                NPC.damage = 80;
            }

            NPC.width = NPC.height = 80;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.knockBackResist = 0;
            NPC.damage = 95;
            //NPC.hide = true;

            if (!VisualConfigSystem.ShowBossBar)
            {
                NPC.BossBar = ModContent.GetInstance<NothingBossBar>();
            }

            Music = MusicID.Boss1;
        }

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            int expertBaseLife = 3500;
            int masterBaseLife = 5400;

            int expertAddLife = 800;
            int masterAddLife = 1000;

            int expertDamage = 75;
            int MasterDamage = 95;

            if (Main.hardMode)
            {
                expertBaseLife = 9500;
                masterBaseLife = 11050;

                expertAddLife = 1200;
                masterAddLife = 2030;

                expertDamage = 105;
                MasterDamage = 135;
            }

            NPC.defDamage = 60;
            if (Helper.GetJourneyModeStrangth(out float journeyScale, out NPCStrengthHelper nPCStrengthHelper))
            {
                if (nPCStrengthHelper.IsExpertMode)
                {
                    NPC.lifeMax = (int)((expertBaseLife + numPlayers * expertAddLife) / journeyScale);
                    NPC.damage = (int)(expertDamage / journeyScale);
                }

                if (nPCStrengthHelper.IsMasterMode)
                {
                    NPC.lifeMax = (int)((masterBaseLife + numPlayers * masterAddLife) / journeyScale);
                    NPC.damage = (int)(MasterDamage / journeyScale);
                }

                if (Main.getGoodWorld)
                {
                    if (Main.hardMode)
                        NPC.damage = (int)(170 / journeyScale);
                    else
                        NPC.damage = (int)(120 / journeyScale);
                }

                return;
            }

            NPC.lifeMax = expertBaseLife + numPlayers * expertAddLife;
            NPC.damage = expertDamage;

            if (Main.masterMode)
            {
                NPC.lifeMax = masterBaseLife + numPlayers * masterAddLife;
                NPC.damage = MasterDamage;
            }

            if (Main.getGoodWorld)
            {
                if (Main.hardMode)
                {
                    NPC.lifeMax = 13000 + numPlayers * 2800;
                    NPC.damage = 170;
                }
                else
                {
                    NPC.lifeMax = 6500 + numPlayers * 1200;
                    NPC.damage = 120;
                }
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ItemID.QueenBeePetItem, 4));

            npcLoot.Add(ItemDropRule.ByCondition(new Conditions.IsExpert()
                , ModContent.ItemType<BeastCrest>()));
            npcLoot.Add(ItemDropRule.MasterModeCommonDrop(ItemID.QueenBeeMasterTrophy));
            npcLoot.Add(ItemDropRule.BossBag(ItemID.QueenBeeBossBag));
            npcLoot.Add(ItemDropRule.Common(ItemID.BeeMask, 7));
            npcLoot.Add(ItemDropRule.Common(ItemID.QueenBeeTrophy, 10));

            LeadingConditionRule notExpertRule = new(new Conditions.NotExpert());
            notExpertRule.OnSuccess(ItemDropRule.OneFromOptions(1, ItemID.BeeGun, ItemID.BeeKeeper, ItemID.BeesKnees));
            notExpertRule.OnSuccess(ItemDropRule.OneFromOptions(1, ItemID.HiveWand, ItemID.BeeHat, ItemID.BeeShirt, ItemID.BeePants));
            notExpertRule.OnSuccess(ItemDropRule.Common(ItemID.HoneyComb, 3));
            notExpertRule.OnSuccess(ItemDropRule.Common(ItemID.Nectar, 8));
            notExpertRule.OnSuccess(ItemDropRule.Common(ItemID.HoneyedGoggles, 10));
            notExpertRule.OnSuccess(ItemDropRule.Common(ItemID.BeeWax, 1, 20, 30));
            notExpertRule.OnSuccess(ItemDropRule.Common(ItemID.BottledHoney, 1, 8, 18));
            notExpertRule.OnSuccess(new CommonDrop(ItemID.Beenade, 4, 20, 40, 5));
            npcLoot.Add(notExpertRule);
        }

        public override void ModifyHoverBoundingBox(ref Rectangle boundingBox)
        {
            boundingBox = new Rectangle();
        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            return false;
        }

        public override bool CanBeHitByNPC(NPC attacker)
        {
            return false;
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
                    ShootSpike(controller);
                    break;
                case AIStates.SummonMinions:
                    Summon(controller);
                    break;
                case AIStates.Dash:
                    Dash(controller);
                    break;
                case AIStates.SmashDown:
                    SmashDown(controller);
                    break;
                case AIStates.KillAnmi:
                    KillAnmi(controller);
                    break;
                default:
                    break;
            }

            UpdateFrame();
        }

        public void SpawnAnmi(NPC controller)
        {
            NPC.dontTakeDamage = true;
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
                else if (dis.X > 80)
                {
                    //i = 2;
                    Helper.Movement_SimpleOneLine(ref NPC.velocity.X, NPC.direction, speed, a, a * 2, 0.97f);
                }
                else if (dis.X < 60)
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

                LimitPos(controller);

                return;
            }

            if (Timer == beforeDashTime)//根据玩家位置做偏移
            {
                NPC.velocity *= 0.2f;
                float p = 0.5f;
                if (AngerNum > 1f)
                    p = 0.75f;
                Recorder2 = Helper.Lerp(Recorder2, Target.Center.Y - controller.Center.Y, p);
            }

            //向后拉，准备冲
            int makeBackTime = (int)(30 / AngerNum);
            if (Timer < beforeDashTime + makeBackTime)
            {
                SetDirection(new Vector2(Target.Center.X, controller.Center.Y + Recorder2), out Vector2 dis);
                NPC.spriteDirection = NPC.direction;

                if (MathF.Abs(NPC.Center.X - controller.Center.X) < RectangleLimit.LimitWidth / 2 - NPC.width / 2 - 60
                    && dis.X > 40)
                    Helper.Movement_SimpleOneLine(ref NPC.velocity.X, -NPC.direction
                        , 6.5f + AngerNum * 2f, 0.5f + AngerNum * 0.3f, 0.5f + AngerNum * 0.3f, 0.97f);
                else
                    NPC.velocity.X *= 0.9f;

                //控制Y方向的移动
                if (dis.Y > 40)
                    Helper.Movement_SimpleOneLine(ref NPC.velocity.Y, NPC.directionY
                        , 6.5f + AngerNum * 2f, 0.5f + AngerNum * 0.3f, 0.5f + AngerNum * 0.3f, 0.97f);
                else
                    NPC.velocity.Y *= 0.9f;

                LimitPosY(controller);

                return;
            }

            if (Timer == beforeDashTime + makeBackTime)//开始冲刺
            {
                SetDirection(new Vector2(Target.Center.X, controller.Center.Y + Recorder2), out _);
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
                LimitPosY(controller);

                //撞墙
                Vector2 tempPos = NPC.Center + NPC.velocity;
                int dir = Math.Sign(NPC.velocity.X);
                bool collide = false;

                if (dir == 1)
                {
                    if (tempPos.X + NPC.width / 2 > controller.Center.X + RectangleLimit.LimitWidth / 2)
                    {
                        Particle.NewParticle<Slam>(NPC.Right + new Vector2(-30, 30), new Vector2(-1, 0), Scale: 0.6f);
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
                        new Vector2(Math.Clamp(Target.Center.X, controller.Center.X - RectangleLimit.LimitWidth / 2 + 20, controller.Center.X + RectangleLimit.LimitWidth / 2 - 20), controller.Center.Y - RectangleLimit.LimitHeight / 2 - 24)
                        , Vector2.Zero, Helper.GetProjDamage(80, 90, 120), 0, ai0: RectLimitIndex, ai2: Main.rand.Next(10));

                    if (AngerNum > 1)
                    {
                        int count = 2;
                        if (AngerNum > 1.5f)
                            count = 3;
                        for (int i = 0; i < count; i++)
                            NPC.NewProjectileInAI<SpikeBallProj>(
                                new Vector2(controller.Center.X + Main.rand.NextFloat(-RectangleLimit.LimitWidth / 2 + 20, RectangleLimit.LimitWidth / 2 - 20), controller.Center.Y - RectangleLimit.LimitHeight / 2 - 24)
                                , Vector2.Zero, Helper.GetProjDamage(80, 90, 120), 0, ai0: RectLimitIndex, ai2: Main.rand.Next(10));
                    }
                    return;
                }

                float xReverse = 265 / AngerNum;
                if (Recorder > 1 && MathF.Sign(Target.Center.X - NPC.Center.X) * NPC.direction < 0
                    && MathF.Abs(Target.Center.X - NPC.Center.X) > xReverse)
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
                    float factor = (Timer - beforeDashTime - makeBackTime - dashTime - 10) / 20;
                    NPC.rotation = Helper.SinEase(factor) * -NPC.spriteDirection * 0.5f;
                }

                LimitPos(controller);
                return;
            }

            Recorder--;
            Timer = 0;
            if (Recorder < 1)
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
                if (dis.X > 80)
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

                LimitPos(controller);

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
                NPC.rotation = NPC.spriteDirection * (MathHelper.TwoPi + MathHelper.PiOver2)
                    * (Timer - beforeDashTime) / rollingTime;

                float targetY = controller.Center.Y - RectangleLimit.LimitHeight / 2 - 200;
                SetDirection(new Vector2(Recorder2, targetY), out Vector2 dis);

                float speed = 4 + AngerNum * 2f;
                float a = 0.1f + 0.1f * AngerNum;

                if (dis.X > 60)
                    Helper.Movement_SimpleOneLine(ref NPC.velocity.X, NPC.direction, speed, a, a * 2, 0.97f);
                else
                    NPC.velocity.X *= 0.93f;

                speed = 6 + AngerNum * 3f;
                a = 0.6f + 0.6f * AngerNum;

                //控制Y方向的移动
                if (NPC.Center.Y > targetY)
                    Helper.Movement_SimpleOneLine(ref NPC.velocity.Y, NPC.directionY
                        , speed / 2, a / 2, a, 0.97f);
                else
                    NPC.velocity.Y *= 0.8f;

                LimitPos(controller);

                return;
            }

            //短暂前摇
            const int Pre = 20;
            if (Timer < beforeDashTime + rollingTime + Pre)
            {
                IsDashing = true;
                float speed = 12 + AngerNum * 12f;
                float a = 1.4f + 1.4f * AngerNum;

                NPC.velocity.X *= 0.95f;

                if (NPC.Top.Y > controller.Center.Y - RectangleLimit.LimitHeight / 2 - 30)
                    Helper.Movement_SimpleOneLine(ref NPC.velocity.Y, NPC.directionY
                        , speed / 2, a / 2, a, 0.97f);
                else
                    NPC.velocity.Y *= 0.8f;

                LimitPos(controller);

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
                Rectangle r = NPC.getRect();
                r.X -= 40;
                r.Width += 80;
                r.Y -= 20;
                r.Height += 40;
                foreach (var npc in Main.ActiveNPCs)
                {
                    if (!npc.friendly && r.Intersects(npc.getRect())
                        && npc.TryGetGlobalNPC(out SavageNPC snpc) && snpc.RectLimitIndex == RectLimitIndex)
                        npc.SimpleStrikeNPC(npc.lifeMax * 2, 0, true, 0, damageVariation: false);
                }

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

                LimitPosX(controller);
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

        public void ShootSpike(NPC controller)
        {
            Timer++;

            int beforeDashTime = (int)(85 / AngerNum);

            if (Timer < beforeDashTime)
            {
                SetDirection(new Vector2(Target.Center.X, controller.Center.Y - RectangleLimit.LimitHeight / 2 + 120), out Vector2 dis);
                NPC.spriteDirection = NPC.direction;

                NPC.rotation = NPC.rotation.AngleLerp(0, 0.2f);

                float speed = 4 + AngerNum * 2f;
                float a = 0.1f + 0.1f * AngerNum;

                //int i = 0;
                if (MathF.Abs(NPC.Center.X - controller.Center.X) > RectangleLimit.LimitWidth / 2 - 140)
                {
                    //i = 1;
                    Helper.Movement_SimpleOneLine(ref NPC.velocity.X, MathF.Sign(controller.Center.X - NPC.Center.X), speed, a, a * 2, 0.97f);
                }
                else if (dis.X > 300)
                {
                    //i = 2;
                    Helper.Movement_SimpleOneLine(ref NPC.velocity.X, NPC.direction, speed, a, a * 2, 0.97f);
                }
                else if (dis.X < 240)
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

                LimitPos(controller);

                return;
            }

            if (Timer == beforeDashTime)//根据玩家位置做偏移
            {
                NPC.velocity *= 0.2f;
                Recorder2 = Main.rand.NextFloat(MathHelper.TwoPi);
                IsDashing = false;

                Helper.PlayPitched(CoraliteSoundID.BugsScream_Item173, NPC.Center, pitch: -0.5f);

                return;
            }

            const int readyTime = 60;
            if (Timer < beforeDashTime + readyTime)
            {
                NPC.velocity *= 0.9f;
                NPC.rotation += MathF.Sin(Timer * 0.75f) * 0.1f;

                return;
            }


            const int shootTime = 120;
            int perTime = (int)(20 / AngerNum);

            if (Timer < beforeDashTime + readyTime + shootTime)
            {
                NPC.velocity *= 0.9f;
                NPC.rotation = NPC.rotation.AngleLerp(0, 0.15f);

                if (Timer % perTime == 0)
                {
                    if (Timer % (perTime * 5) == 0)
                    {
                        Recorder2 = Main.rand.NextFloat(MathHelper.TwoPi);
                    }

                    Vector2 pos = NPC.Center + new Vector2(NPC.direction * 20, 60);
                    int damage = Helper.GetProjDamage(80, 90, 100);
                    for (int i = 0; i < 4; i++)
                    {
                        NPC.NewProjectileDirectInAI<BeeStinger>(pos, (Recorder2 + i * MathHelper.PiOver2).ToRotationVector2() * 8f
                            , damage, 0, ai0: RectLimitIndex, ai1: NPC.whoAmI);
                    }

                    Helper.PlayPitched(CoraliteSoundID.Stinger_Item17, NPC.Center);
                    Recorder2 += MathHelper.PiOver4 + Main.rand.NextFloat(-0.1f, 0.1f);
                }

                return;
            }

            int restTime = (int)(60 / AngerNum);
            if (Timer < beforeDashTime + readyTime + shootTime + restTime)
            {
                return;
            }

            ExchangeState();
        }

        /// <summary>
        /// 召唤
        /// </summary>
        /// <param name="controller"></param>
        public void Summon(NPC controller)
        {
            LimitPos(controller);

            Timer++;

            if (Timer < 35)
            {
                SetDirection(Target.Center, out _);
                NPC.spriteDirection = NPC.direction;
                NPC.rotation += 0.01f;

                NPC.velocity *= 0.95f;
                return;
            }

            if (Timer == 35)//生成蜂巢
            {
                NPC.rotation = 0;

                NPC.NewProjectileInAI<Hive>(
                    new Vector2(Math.Clamp(Target.Center.X, controller.Center.X - RectangleLimit.LimitWidth / 2 + 20, controller.Center.X + RectangleLimit.LimitWidth / 2 - 20), controller.Center.Y - RectangleLimit.LimitHeight / 2 - 24)
                    , Vector2.Zero, Helper.GetProjDamage(80, 90, 120), 0, ai0: RectLimitIndex, ai1: GetSpawnNPCType(), ai2: Main.rand.Next(10));

                if (AngerNum > 1.5f)
                {
                    NPC.NewProjectileInAI<Hive>(
                        new Vector2(Math.Clamp(controller.Center.X + Main.rand.NextFloat(-RectangleLimit.LimitWidth / 2 + 20, RectangleLimit.LimitWidth / 2 - 20), controller.Center.X - RectangleLimit.LimitWidth / 2 + 20, controller.Center.X + RectangleLimit.LimitWidth / 2 - 20), controller.Center.Y - RectangleLimit.LimitHeight / 2 - 24)
                        , Vector2.Zero, Helper.GetProjDamage(80, 90, 120), 0, ai0: RectLimitIndex, ai1: GetSpawnNPCType(), ai2: Main.rand.Next(10));
                }

                Dust.NewDustPerfect(NPC.Center, ModContent.DustType<RoaringWave>()
                    , Vector2.Zero, 0, Color.White * 0.8f, 0.4f);

                Helper.PlayPitched("beastfly_scream_short", 0.8f, 1, NPC.Center);

                return;
            }

            if (Timer < 35 + 45)
            {
                NPC.rotation += MathF.Sin(Timer * 0.75f) * 0.1f;
                return;
            }

            if (Timer < 35 + 45 + 20)
            {
                NPC.rotation = NPC.rotation.AngleLerp(0, 0.2f);

                return;
            }

            ExchangeState();
        }

        public void KillAnmi(NPC controller)
        {
            Helper.StopMusic();

            NPC.velocity = Vector2.Zero;
            NPC.rotation += MathF.Sin(Timer * 0.75f) * 0.2f;

            if (Timer == 0)
            {
                Particle.NewParticle(NPC.Center, Vector2.Zero, Contents.ParticleType<FinalHit>()
                 , newColor: Color.White, Scale: 1.5f);
            }

            Timer++;

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
                Helper.PlayPitched(CoraliteSoundID.Fleshy_NPCHit1, NPC.Center);
            }

            if (Timer > 160)
            {
                Helper.PlayPitched("DeathBoom", 1, 0, NPC.Center);
                Helper.PlayPitched(CoraliteSoundID.QueenBee_NPCDeath66, NPC.Center);

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
                Gore.NewGore(NPC.GetSource_FromAI(), NPC.Center + Main.rand.NextVector2Circular(25, 25)
                    , Helper.NextVec2Dir(3, 6), 303, 1.3f);
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 5; j++)
                        Gore.NewGore(NPC.GetSource_FromAI(), NPC.Center + Main.rand.NextVector2Circular(25, 25)
                            , Helper.NextVec2Dir(3, 6), 304 + j, 1.3f);
                }
            }
        }

        public void ExchangeState()
        {
            NPC.TargetClosest();
            NPC.dontTakeDamage = false;

            Timer = 0;
            Recorder = 0;
            Recorder2 = 0;
            Recorder3 = 0;
            IsDashing = false;

            AngerNum = 1;
            float add1 = 0.1f;
            float add2 = 0.15f;
            if (Main.hardMode)
            {
                add1 = 0.2f;
                add2 = 0.25f;
            }
            if (NPC.life < NPC.lifeMax * 2 / 3)
                AngerNum += add1;
            if (NPC.life < NPC.lifeMax / 2)
                AngerNum += add2;
            if (NPC.life < NPC.lifeMax / 5)
                AngerNum += add2;

            wr.Clear();
            if (State != AIStates.Dash)
                wr.Add(AIStates.Dash);
            if (State != AIStates.SmashDown)
                wr.Add(AIStates.SmashDown);
            if (State != AIStates.ShootSpikes)
                wr.Add(AIStates.ShootSpikes);
            if (NPC.life < NPC.lifeMax * 2 / 3 && State != AIStates.SummonMinions)
                wr.Add(AIStates.SummonMinions, 0.5f);

            State = wr.Get();

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

        public static void OpenMusic()
        {
            Main.musicFade[Main.curMusic] = 1;
        }

        public void LimitPosX(NPC controller)
        {
            if (NPC.Left.X + NPC.velocity.X < controller.Center.X - RectangleLimit.LimitWidth / 2)
            {
                NPC.Left = new Vector2(controller.Center.X - RectangleLimit.LimitWidth / 2, NPC.Left.Y);
                NPC.velocity.X *= 0.5f;
            }
            if (NPC.Right.X + NPC.velocity.X > controller.Center.X + RectangleLimit.LimitWidth / 2)
            {
                NPC.Right = new Vector2(controller.Center.X + RectangleLimit.LimitWidth / 2, NPC.Right.Y);
                NPC.velocity.X *= 0.5f;
            }
        }

        public void LimitPosY(NPC controller)
        {
            if (NPC.Top.Y + NPC.velocity.Y < controller.Center.Y - RectangleLimit.LimitHeight / 2)
            {
                NPC.Top = new Vector2(NPC.Top.X, controller.Center.Y - RectangleLimit.LimitHeight / 2);
                NPC.velocity.Y *= 0.5f;
            }
            if (NPC.Bottom.Y + NPC.velocity.Y > controller.Center.Y + RectangleLimit.LimitHeight / 2)
            {
                NPC.Bottom = new Vector2(NPC.Bottom.X, controller.Center.Y + RectangleLimit.LimitHeight / 2);
                NPC.velocity.Y *= 0.5f;
            }
        }

        public void LimitPos(NPC controller)
        {
            LimitPosX(controller);
            LimitPosY(controller);
        }

        public static int GetSpawnNPCType()
        {
            if (Main.hardMode)
            {
                if (Main.masterMode && Main.getGoodWorld)
                    return Main.rand.NextFromList(NPCID.QueenBee
                        , NPCID.TinyMossHornet, NPCID.GiantMossHornet
                        , NPCID.JungleCreeper, NPCID.GiantTortoise, NPCID.Moth, NPCID.Derpling, NPCID.GiantFlyingFox);

                return Main.rand.NextFromList(NPCID.MossHornet
                        , NPCID.TinyMossHornet, NPCID.GiantMossHornet, NPCID.Derpling, -1);
            }
            else
            {
                if (Main.masterMode && Main.getGoodWorld)
                    return Main.rand.NextFromList(NPCID.SpikedJungleSlime
                        , NPCID.JungleBat
                        , NPCID.LittleStinger, NPCID.LittleHornetHoney, NPCID.LittleHornetLeafy, NPCID.LittleHornetSpikey, NPCID.LittleHornetStingy);

                return Main.rand.NextFromList(NPCID.JungleSlime
                        , NPCID.Hornet, NPCID.HornetHoney, NPCID.HornetLeafy, NPCID.HornetSpikey, NPCID.HornetStingy, -1);
            }
        }

        public override bool CheckDead()
        {
            if (State != AIStates.KillAnmi)
            {
                NPC.life = 1;
                NPC.dontTakeDamage = true;
                State = AIStates.KillAnmi;
                Timer = 0;
                Recorder = 0;
                Recorder2 = 0;
                Recorder3 = 0;
                IsDashing = false;
                NPC.rotation = 0;

                Helper.PlayPitched("boss_final_hit", 1, 0, NPC.Center);

                return false;
            }

            if (State == AIStates.KillAnmi && Timer < 159)
            {
                NPC.life = 1;
                return false;
            }

            return base.CheckDead();
        }

        public override void BossLoot(ref int potionType)
        {
            potionType = ItemID.BottledHoney;
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
