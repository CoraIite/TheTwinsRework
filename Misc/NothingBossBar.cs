using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace TheTwinsRework.Misc
{
    public class NothingBossBar : ModBossBar
    {
        public override bool PreDraw(SpriteBatch spriteBatch, NPC npc, ref BossBarDrawParams drawParams)
        {
            return false;
        }
    }
}
