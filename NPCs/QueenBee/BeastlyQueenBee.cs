using Coralite.Helpers;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ID;
using Terraria.ModLoader;
using TheTwinsRework.Configs;
using TheTwinsRework.Misc;

namespace TheTwinsRework.NPCs.QueenBee
{
    public class BeastlyQueenBee : ModNPC
    {
        public override string Texture => AssetDirectory.Vanilla + "NPC_222";

        public AIStates State {  get; set; }

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
        public ref float Recorder2 => ref NPC.ai[2];
        public ref float Recorder3 => ref NPC.ai[3];

        public ref float Timer => ref NPC.localAI[0];
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
            if (RectLimitIndex.GetNPCOwner<RectangleLimit>(out NPC controller))
            {

            }
        }

        public void SpawnAnmi()
        {

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
            Music = MusicLoader.GetMusicSlot(Mod, "Sounds/Music/RIP AND SHRED");
        }

        #endregion

        #region Draw

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {


            return false;
        }

        #endregion

    }
}
