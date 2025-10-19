using Coralite.Helpers;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.ModLoader;
using TheTwinsRework.NPCs.TheTwins;

namespace TheTwinsRework.Projectiles
{
    public class DeathLaserNoTileCollide : ModProjectile
    {
        public override string Texture => AssetDirectory.Vanilla + "Projectile_83";

        public ref float CircleIndex => ref Projectile.ai[0];

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 4;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 2;
            Projectile.scale = 1.7f;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            if (!CircleIndex.GetNPCOwner<CircleLimit>(out NPC owner, Projectile.Kill))
                return;

            if (Projectile.soundDelay == 0)
            {
                Projectile.soundDelay = 100000;
                Helper.PlayPitched(CoraliteSoundID.LaserShoot_Item33, Projectile.Center);
            }

            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.alpha += 10;
            if (Projectile.alpha > 255)
                Projectile.alpha = 0;

            if (Vector2.Distance(Projectile.Center, owner.Center) > CircleLimit.MaxLength + 100)
                Projectile.Kill();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            lightColor = Color.White;
            Texture2D tex = Projectile.GetTexture();

            Vector2 origin = new Vector2(tex.Width / 2, 0);

            Vector2 pos = Projectile.Center - Main.screenPosition;
            Color c = Color.White * (Projectile.alpha / 255f);
            Color c2 = Color.White * (Projectile.alpha / 255f) * 0.3f;
            c2.A = 0;
            float rot = Projectile.rotation + MathHelper.PiOver2;
            float length = MathF.Sin((int)Main.timeForVisualEffects * 0.2f) * 3;

            for (int i = 0; i < 8; i++)
            {
                Vector2 pos2 = pos + (Projectile.rotation + MathHelper.PiOver4 * i).ToRotationVector2() * length;
                Main.spriteBatch.Draw(tex, pos2, null, c2, rot, origin, Projectile.scale, 0, 0);
            }

            Main.spriteBatch.Draw(tex, pos, null, c, rot, origin, Projectile.scale, 0, 0);

            return false;
        }
    }
}
