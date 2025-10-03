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
        private Vector2 oldPos;

        private bool over = false;
        private int FadeTime;

        public string UniqueIdentity => "TheTwinsScreenMove";

        public bool Finished
        {
            get
            {
                return over;
            }
        }

        public void Update(ref CameraInfo cameraPosition)
        {
            if (FadeTime == 0 && MainIndex.GetNPCOwner<CircleLimit>(out NPC npc))//移动视角
            {
                float distance = Vector2.Distance(npc.Center, Main.LocalPlayer.Center);

                if (distance > CircleLimit.MaxLength + 500)//距离太大直接不动
                    return;

                oldPos = cameraPosition.CameraPosition = Vector2.Lerp(Main.LocalPlayer.Center, npc.Center
                      , Math.Clamp(Helper.X2Ease(distance / (CircleLimit.MaxLength + 300)), 0, 1))
                      - Main.ScreenSize.ToVector2() / 2;
            }
            else
            {
                FadeTime++;
                cameraPosition.CameraPosition = Vector2.Lerp(oldPos, Main.LocalPlayer.Center - Main.ScreenSize.ToVector2() / 2, FadeTime/60f);
                if (FadeTime > 60)
                    over = true;
            }
        }
    }
}
