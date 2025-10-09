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
            //NPC.hide = true;

            if (!VisualConfigSystem.ShowBossBar)
            {
                NPC.BossBar = ModContent.GetInstance<NothingBossBar>();
            }

            Music = MusicID.Boss1;
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            return false;
        }

        public void OpenMusic()
        {
            Music = MusicLoader.GetMusicSlot(Mod, "Sounds/Music/RIP AND SHRED");
        }
    }
}
