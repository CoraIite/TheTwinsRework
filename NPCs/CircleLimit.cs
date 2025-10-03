using Coralite.Helpers;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using TheTwinsRework.Misc;
using TheTwinsRework.Projectiles;

namespace TheTwinsRework.NPCs
{
    [AutoloadBossHead]
    public class CircleLimit : ModNPC
    {
        public override string Texture => AssetDirectory.Assets + Name;

        public ref float CircleLength => ref NPC.ai[0];
        public ref float State => ref NPC.ai[1];
        public ref float Eye1Index => ref NPC.ai[2];
        public ref float Eye2Index => ref NPC.ai[3];

        public ref float Timer => ref NPC.localAI[0];
        private Player Target => Main.player[NPC.target];

        public static float MaxLength = 580;

        public static Asset<Texture2D> TwistTex { get; private set; }

        public override void Load()
        {
            if (Main.dedServ)
                return;

            TwistTex = ModContent.Request<Texture2D>(AssetDirectory.Assets + "Twist");
        }

        public override void Unload()
        {
            TwistTex = null;
        }

        public override void SetDefaults()
        {
            NPC.boss = true;
            NPC.lifeMax = 100;
            NPC.width = NPC.height = 16;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.knockBackResist = 0;
            NPC.hide = true;

            Music = MusicID.Boss2;
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            return false;
        }

        public override void ModifyHoverBoundingBox(ref Rectangle boundingBox)
        {
            boundingBox = new Rectangle();
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ItemID.TwinsPetItem, 4));

            npcLoot.Add(ItemDropRule.MasterModeCommonDrop(ItemID.TwinsMasterTrophy));
            npcLoot.Add(ItemDropRule.BossBag(ItemID.TwinsBossBag));
            npcLoot.Add(ItemDropRule.Common(ItemID.TwinMask, 7));

            LeadingConditionRule notExpertRule = new(new Conditions.NotExpert());
            notExpertRule.OnSuccess(ItemDropRule.Common(ItemID.HallowedBar, 1, 15, 30));
            notExpertRule.OnSuccess(ItemDropRule.Common(ItemID.SoulofSight, 1, 25, 40));
            npcLoot.Add(notExpertRule);
        }

        public override void BossLoot(ref int potionType)
        {
            potionType = ItemID.GreaterHealingPotion;
        }

        public override void AI()
        {
            NPC.dontTakeDamage = true;

            switch (State)
            {
                default:
                    break;
                case 0://扩大
                    {
                        NPC.dontTakeDamage = false;

                        CircleLength = Helper.Lerp(CircleLength, MaxLength, 0.2f);
                        if (CircleLength > MaxLength - 1)
                        {
                            CircleLength = MaxLength;

                            //生成眼球
                            int y = (int)NPC.Center.Y - 250;
                            Eye1Index = NPC.NewNPC(NPC.GetSource_FromThis()
                                 , (int)(NPC.Center.X - 1200), y, ModContent.NPCType<FireEye>(), ai1: NPC.whoAmI,ai3: -MathHelper.PiOver4);
                            Eye2Index = NPC.NewNPC(NPC.GetSource_FromThis()
                                 , (int)(NPC.Center.X + 1200), y, ModContent.NPCType<LaserEye>(), ai1: NPC.whoAmI, ai3: MathHelper.Pi + MathHelper.PiOver4);

                            Main.npc[(int)Eye1Index].ai[0] = Eye2Index;
                            Main.npc[(int)Eye2Index].ai[0] = Eye1Index;

                            State = 1;

                            //生成镜头移动
                            if (Main.netMode != NetmodeID.Server)
                            {
                                ScreenMove move = new ScreenMove();
                                move.MainIndex = NPC.whoAmI;

                                Main.instance.CameraModifiers.Add(move);
                            }
                        }
                    }
                    break;
                case 1:
                    {
                        if (!Eye1Index.GetNPCOwner<FireEye>(out NPC eye1, () => Eye1Index = -1)
                            && !Eye2Index.GetNPCOwner<LaserEye>(out _, () => Eye2Index = -1))
                        {
                            State = 2;
                        }

                        SetHealth(eye1);

                        if (eye1 != null)
                        {
                            SwitchPhase(eye1);
                        }

                        Timer++;
                        CheckTarget();

                        if (NPC.target < 0 || NPC.target == 255 || Target.dead || !Target.active || Target.Distance(NPC.Center) > 3000 || Main.dayTime)
                        {
                            NPC.TargetClosest();

                            if (Target.dead || !Target.active || Target.Distance(NPC.Center) > 4500 || Main.dayTime)//没有玩家存活时离开
                            {
                                NPC.active = false;
                                return;
                            }
                        }
                    }
                    break;
                case 2://死亡
                    {
                        CircleLength = Helper.Lerp(CircleLength, 0, 0.075f);
                        if (CircleLength < 1)
                        {
                            NPC.Kill();
                            Helper.PlayPitched("Defeat", 1, 0, NPC.Center);
                        }
                    }
                    break;
                case 3:
                    {
                        Main.audioSystem.PauseAll();
                        for (int i = 0; i < Main.musicFade.Length; i++)
                            Main.musicFade[i] = 0;

                        Timer++;
                        if (Timer > 160)
                        {
                            NPC.NewProjectileInAI<SoundControl>(NPC.Center, Vector2.Zero, 0, 0);
                            State = 2;
                        }
                    }
                    break;
            }
        }

        private void CheckTarget()
        {
            if (Timer > 30)//检测玩家并攻击
            {
                Timer = 0;
                NPC.TargetClosest();
                if (Vector2.Distance(Target.Center, NPC.Center) > MaxLength + 10)
                    Target.Hurt(PlayerDeathReason.ByNPC(NPC.whoAmI), 175, 0,dodgeable:false,armorPenetration:100);
            }
        }

        private void SetHealth(NPC eye1)
        {
            int lifeMax = 0;
            int life = 0;

            if (eye1 != null)
            {
                lifeMax += eye1.lifeMax;
                life += eye1.life;
            }

            if (Eye2Index.GetNPCOwner<LaserEye>(out NPC eye2, () => Eye2Index = -1))
            {
                lifeMax += eye2.lifeMax;
                life += eye2.life;
            }

            NPC.lifeMax = lifeMax;
            NPC.life = life;
        }

        public void SwitchPhase(NPC eye1)
        {
            if (Eye2Index.GetNPCOwner<LaserEye>(out NPC eye2, () => Eye2Index = -1)
                && eye1.ModNPC is BaseTwin bt1 && eye2.ModNPC is BaseTwin bt2)
            {
                switch (bt1.Phase)
                {
                    case BaseTwin.AIPhase.Anmi:
                        break;
                    case BaseTwin.AIPhase.P1://小于三分之一血进入2阶段
                        if (eye1.life <= eye1.lifeMax * 2 / 3 || eye2.life <= eye2.lifeMax * 2 / 3)
                        {
                            bt1.SwitchPhaseP1ToP2();
                            bt2.SwitchPhaseP1ToP2();

                            SoundEngine.PlaySound(CoraliteSoundID.Metal_NPCHit4 with { Pitch = 0.2f }, NPC.Center);

                            int life = eye1.life + eye2.life;
                            life /= 2;
                            eye1.life = eye2.life = life;

                            SwitchBGM(BaseTwin.AIPhase.P2);
                            Helper.PlayPitched("Sting", 1, 0, NPC.Center);
                        }

                        break;
                    case BaseTwin.AIPhase.P2:
                        if (eye1.life <= eye1.lifeMax * 1 / 3 || eye2.life <= eye2.lifeMax * 1 / 3)
                        {
                            bt1.SwitchPhaseP2ToP3();
                            bt2.SwitchPhaseP2ToP3();

                            SoundEngine.PlaySound(CoraliteSoundID.Metal_NPCHit4 with { Pitch = 0.2f }, NPC.Center);

                            int life = eye1.life + eye2.life;
                            life /= 2;
                            eye1.life = eye2.life = life;

                            SwitchBGM(BaseTwin.AIPhase.P3);
                            Helper.PlayPitched("Sting", 1, 0, NPC.Center);
                        }

                        break;
                    case BaseTwin.AIPhase.P3:
                        break;
                    case BaseTwin.AIPhase.P4:
                        break;
                    default:
                        break;
                }
            }
        }

        public void SwitchBGM(BaseTwin.AIPhase phase)
        {
            NPC.boss = true;

            switch (phase)
            {
                case BaseTwin.AIPhase.Anmi:
                    Music = MusicID.Boss2;
                    break;
                case BaseTwin.AIPhase.P1:
                    Music = MusicLoader.GetMusicSlot(Mod, "Sounds/Music/H181ClockworkDancers1stPhase");
                    break;
                case BaseTwin.AIPhase.P2:
                    Music = MusicLoader.GetMusicSlot(Mod, "Sounds/Music/H181ClockworkDancers2ndPhase");
                    break;
                case BaseTwin.AIPhase.P3:
                    Music = MusicLoader.GetMusicSlot(Mod, "Sounds/Music/H181ClockworkDancers3rdPhase");
                    break;
                case BaseTwin.AIPhase.P4:
                    Music = MusicLoader.GetMusicSlot(Mod, "Sounds/Music/H181ClockworkDancers4thPhase");
                    break;
                default:
                    break;
            }

            Main.audioSystem.ResumeAll();
            Main.musicFade[Music] = 1;
        }

        public override void OnKill()
        {
            NPC.SetEventFlagCleared(ref NPC.downedMechBoss2, GameEventClearedID.DefeatedTheTwins);
        }

        public override void DrawBehind(int index)
        {
            Main.instance.DrawCacheNPCsMoonMoon.Add(index);
        }

        public void DrawBack(Vector2 pos, Color circleColor, Color backColor)
        {
            Texture2D texture = TwistTex.Value;
            Effect shader = Filters.Scene["Circle"].GetShader().Shader;

            float dia = MaxLength * 2 + 50;

            shader.Parameters["uTime"].SetValue(Main.GlobalTimeWrappedHourly / 80);
            shader.Parameters["r"].SetValue(CircleLength);
            shader.Parameters["dia"].SetValue(dia);
            shader.Parameters["edgeColor"].SetValue(circleColor.ToVector4());
            shader.Parameters["innerColor"].SetValue(backColor.ToVector4());

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap,
                            Main.spriteBatch.GraphicsDevice.DepthStencilState, RasterizerState.CullNone, shader, Main.GameViewMatrix.TransformationMatrix);

            float scale = dia / texture.Width;
            Main.spriteBatch.Draw(texture, pos
                , null, Color.White, 0, texture.Size() / 2, scale, 0, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, Main.spriteBatch.GraphicsDevice.BlendState, Main.spriteBatch.GraphicsDevice.SamplerStates[0],
                            Main.spriteBatch.GraphicsDevice.DepthStencilState, Main.spriteBatch.GraphicsDevice.RasterizerState, null, Main.GameViewMatrix.TransformationMatrix);
        }



        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            DrawBack(NPC.Center-Main.screenPosition, Color.Red, Color.Red * 0.25f);
            //Texture2D tex = NPC.GetTexture();

            //spriteBatch.Draw(tex, NPC.Center - screenPos, null, Color.Red * (CircleLength / MaxLength)*0.75f
            //    , 0, tex.Size() / 2, CircleLength / (tex.Width / 2 * 0.8f), 0, 0);

            return false;
        }
    }
}
