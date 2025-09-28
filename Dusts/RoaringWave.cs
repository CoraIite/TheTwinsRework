using Coralite.Helpers;
using ReLogic.Content;
using System;
using Terraria.ModLoader;
using TheTwinsRework;

namespace Coralite.Content.Particles
{
    public class RoaringWave : ModDust
    {
        public override string Texture => AssetDirectory.Assets+Name;

        /// <summary>
        /// 控制大小每帧变大多少
        /// </summary>
        public static float ScaleMul = 1.15f;

        /// <summary>
        /// 经过多少帧后开始消失
        /// </summary>
        public static int SpawnTime = 15;

        /// <summary>
        /// 超过<see cref="SpawnTime"/>时每帧缩小多少
        /// </summary>
        public static float FadePercent = 0.9f;

        public override void OnSpawn(Dust dust)
        {
            dust.rotation = Main.rand.NextFloat(6.282f);
        }

        public override bool Update(Dust dust)
        {
            dust.scale *= ScaleMul;

            if (dust.fadeIn > SpawnTime)
                dust.color *= FadePercent;

            dust.fadeIn++;
            if (dust.color.A < 20)
                dust.active = false;

            return false;
        }

        public override bool PreDraw(Dust dust)
        {
            Texture2D.Value.QuickCenteredDraw(Main.spriteBatch, dust.position - Main.screenPosition
                , dust.color, dust.rotation, dust.scale);

            return false;
        }
    }
}
