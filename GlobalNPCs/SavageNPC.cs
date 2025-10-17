using Coralite.Helpers;
using Terraria.ModLoader;
using TheTwinsRework.NPCs.QueenBee;

namespace TheTwinsRework.GlobalNPCs
{
    public class SavageNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public int RectLimitIndex = -1;

        public override void PostAI(NPC npc)
        {
            if (!RectLimitIndex.GetNPCOwner<RectangleLimit>(out NPC owner))
                return;

            //限制位置
            if (npc.TopLeft.X < owner.Center.X - RectangleLimit.LimitWidth / 2)
            {
                npc.TopLeft = new Vector2(owner.Center.X - RectangleLimit.LimitWidth / 2, npc.TopLeft.Y);
                npc.collideX = true;
            }
            else if (npc.BottomRight.X > owner.Center.X + RectangleLimit.LimitWidth / 2)
            {
                npc.BottomRight = new Vector2(owner.Center.X + RectangleLimit.LimitWidth / 2, npc.BottomRight.Y);
                npc.collideX = true;
            }

            if (npc.TopLeft.Y < owner.Center.Y - RectangleLimit.LimitHeight / 2)
            {
                npc.TopLeft = new Vector2(npc.TopLeft.X, owner.Center.Y - RectangleLimit.LimitHeight / 2);
                npc.collideY = true;
            }
            else if (npc.BottomRight.Y > owner.Center.Y + RectangleLimit.LimitHeight / 2)
            {
                npc.BottomRight = new Vector2(npc.BottomRight.X, owner.Center.Y + RectangleLimit.LimitHeight / 2);
                npc.collideY = true;
            }
        }

        public override void OnKill(NPC npc)
        {
            RectLimitIndex = -1;
        }
    }
}
