using Coralite.Helpers;
using Terraria.GameContent.ItemDropRules;
using Terraria.ModLoader;
using TheTwinsRework.NPCs.QueenBee;

namespace TheTwinsRework.GlobalNPCs
{
    public class SavageNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public int RectLimitIndex = -1;

        public override void Load()
        {
            On_ItemDropResolver.TryDropping += On_ItemDropResolver_TryDropping;
        }

        private void On_ItemDropResolver_TryDropping(On_ItemDropResolver.orig_TryDropping orig, ItemDropResolver self, DropAttemptInfo info)
        {
            if (info.npc != null && info.npc.TryGetGlobalNPC(out SavageNPC snpc) && snpc.RectLimitIndex != -1)
            {
                snpc.RectLimitIndex = -1;
                return;
            }

            orig.Invoke(self, info);
        }

        public override void Unload()
        {
            On_ItemDropResolver.TryDropping -= On_ItemDropResolver_TryDropping;
        }

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
                npc.velocity.Y *= -0.5f;
            }
        }

        public override void OnKill(NPC npc)
        {
            RectLimitIndex = -1;
        }
    }
}
