using System.Linq;
using Terraria.ModLoader;
using TheTwinsRework.NPCs;

namespace TheTwinsRework.Projectiles
{
    public class SoundControl : ModProjectile
    {
        public override string Texture => AssetDirectory.Assets + "Blank";

        public override void SetDefaults()
        {
            Projectile.tileCollide = false;
            Projectile.timeLeft = 60 * 16;
        }

        public override void AI()
        {
            Main.audioSystem.PauseAll();
            for (int i = 0; i < Main.musicFade.Length; i++)
                Main.musicFade[i] = 0;

            if (Main.npc.Any(n => n.active && n.type == ModContent.NPCType<CircleLimit>() && n.ai[1] < 2))
                Projectile.Kill();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }
    }
}
