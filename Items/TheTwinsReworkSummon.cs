using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using TheTwinsRework.NPCs;

namespace TheTwinsRework.Items
{
    public class TheTwinsReworkSummon : ModItem
    {
        public override string Texture => AssetDirectory.Assets + Name;

        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
            ItemID.Sets.SortingPriorityBossSpawns[Type] = 12;

            NPCID.Sets.MPAllowedEnemies[ModContent.NPCType<CircleLimit>()] = true;
            ItemID.Sets.ShimmerTransformToItem[ItemID.MechanicalEye] = Type;
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = 1;
            Item.value = Item.sellPrice(0, 0, 1, 0);
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.rare = ItemRarityID.Pink;
        }

        public override bool CanUseItem(Player player)
        {
            return !NPC.AnyNPCs(ModContent.NPCType<CircleLimit>());
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                int type = ModContent.NPCType<CircleLimit>();

                if (Main.netMode != NetmodeID.MultiplayerClient)
                    NPC.NewNPC(new EntitySource_BossSpawn(player), (int)player.Center.X, (int)player.Center.Y
                        , ModContent.NPCType<CircleLimit>());
                else
                    NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, number: player.whoAmI, number2: type);
            }

            return true;
        }
    }
}
