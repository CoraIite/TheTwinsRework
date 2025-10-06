using Terraria.ModLoader;
using TheTwinsRework.Core.System_Particle;

namespace TheTwinsRework.Core
{
    public static class Contents
    {
        /// <summary>
        /// 根据类型获取这个粒子的ID（type）。假设一个类一个实例。
        /// </summary>
        public static int ParticleType<T>() where T : ModParticle => ModContent.GetInstance<T>()?.Type ?? 0;

    }
}
