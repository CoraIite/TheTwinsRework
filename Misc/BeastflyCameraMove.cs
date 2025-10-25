using Coralite.Helpers;
using Terraria.Graphics.CameraModifiers;
using TheTwinsRework.NPCs.QueenBee;
using TheTwinsRework.NPCs.TheTwins;

namespace TheTwinsRework.Misc
{
    public class BeastflyCameraMove : ICameraModifier
    {
        /// <summary>
        /// 核心的索引
        /// </summary>
        public int MainIndex;
        private Vector2 oldPos;

        private bool over = false;
        private int FadeTime;
        private int Timer;

        public string UniqueIdentity => "BeastflyScreenMove";

        public bool Finished
        {
            get
            {
                return over;
            }
        }

        public void Update(ref CameraInfo cameraPosition)
        {
            if (FadeTime == 0 && MainIndex.GetNPCOwner<RectangleLimit>(out NPC npc))//移动视角
            {
                if (Timer<120)
                    Timer++;
                oldPos = cameraPosition.CameraPosition = Vector2.Lerp(Main.LocalPlayer.Center, npc.Center, Timer/120f)
                      - Main.ScreenSize.ToVector2() / 2;
            }
            else
            {
                FadeTime++;
                cameraPosition.CameraPosition = Vector2.Lerp(oldPos, Main.LocalPlayer.Center - Main.ScreenSize.ToVector2() / 2, FadeTime / 60f);
                if (FadeTime > 60)
                    over = true;
            }
        }
    }
}
