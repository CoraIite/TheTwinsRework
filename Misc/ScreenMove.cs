using Coralite.Helpers;
using System;
using Terraria.Graphics.CameraModifiers;
using TheTwinsRework.NPCs;

namespace TheTwinsRework.Misc
{
    public class ScreenMove : ICameraModifier
    {
        /// <summary>
        /// 核心的索引
        /// </summary>
        public int MainIndex;

        public string UniqueIdentity => "TheTwinsScreenMove";

        public bool Finished
        {
            get
            {
                return false;
            }
        }

        public void Update(ref CameraInfo cameraPosition)
        {
            if (MainIndex.GetNPCOwner<CircleLimit>(out NPC npc))//移动视角
            {
                float distance = Vector2.Distance(npc.Center, Main.LocalPlayer.Center);

                if (distance > CircleLimit.MaxLength + 500)//距离太大直接不动
                    return;

                cameraPosition.CameraPosition = Vector2.Lerp(Main.LocalPlayer.Center, npc.Center
                    , Math.Clamp(Helper.X2Ease( distance / (CircleLimit.MaxLength+300)), 0, 1))
                    - Main.ScreenSize.ToVector2() / 2;
            }
        }
    }
}
