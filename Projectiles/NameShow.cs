using Coralite.Helpers;
using System.Collections.Generic;
using Terraria.ModLoader;
using TheTwinsRework.NPCs;

namespace TheTwinsRework.Projectiles
{
    public class NameShow:ModProjectile
    {
        public override string Texture => AssetDirectory.Assets+"Blank";

        public ref float CircleIndex => ref Projectile.ai[0];
        public ref float Alpha => ref Projectile.localAI[0];

        public override void SetDefaults()
        {
            Projectile.tileCollide = false;
            Projectile.timeLeft = 140;
            Projectile.hide = true;
        }

        public override void AI()
        {
            if (!CircleIndex.GetNPCOwner<CircleLimit>(out _))
            {
                Projectile.Kill();
                return;
            }

            if (Projectile.timeLeft > 120)
            {
                Alpha += 1 / 20f;
            }
            else if (Projectile.timeLeft > 20)
            {
                Alpha = 1;
            }
            else
            {
                Alpha -= 1 / 20f;
                if (Alpha < 0)
                    Alpha = 0;
            }
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overWiresUI.Add(index);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 pos = new Vector2(Main.screenWidth / 2, 200);

            if (CircleIndex.GetNPCOwner<CircleLimit>(out NPC circle))
                pos = circle.Center + new Vector2(0, -260) - Main.screenPosition;

            Utils.DrawBorderStringBig(Main.spriteBatch, TheTwinsRework.TheTwinsName.Value
                , pos, Color.Red * Alpha, 1.5f, 0.5f, 0.5f);

            return false;
        }
    }
}
