using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using TheTwinsRework.NPCs.QueenBee;
using TheTwinsRework.NPCs.TheTwins;

namespace TheTwinsRework.Items
{
    public class Beastfly:ModItem
    {
        public override string Texture => AssetDirectory.Assets+Name;
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
            ItemID.Sets.SortingPriorityBossSpawns[Type] = 12;

            NPCID.Sets.MPAllowedEnemies[ModContent.NPCType<BeastlyQueenBee>()] = true;
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
            Item.rare = ItemRarityID.Orange;
        }

        public override bool CanUseItem(Player player)
        {
            return !NPC.AnyNPCs(ModContent.NPCType<BeastlyQueenBee>());
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                //if (!player.ZoneJungle)//不在丛林给予玩家效果
                //{


                //    return true;
                //}

                SpawnBoss(player, player.Center);
            }

            return true;
        }

        public static void SpawnBoss(Player player,Vector2 playerCenter)
        {
            int type = ModContent.NPCType<BeastlyQueenBee>();

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
               NPC controller= NPC.NewNPCDirect(new EntitySource_BossSpawn(player), (int)player.Center.X, player.Center.ToTileCoordinates().Y * 16
                    , ModContent.NPCType<RectangleLimit>());

               int index= NPC.NewNPC(new EntitySource_BossSpawn(player), (int)player.Center.X, (int)player.Center.Y
                    , type,ai0:controller.whoAmI);

                controller.ai[0] = index;
            }
            else
                NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, number: player.whoAmI, number2: type);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Wood, 15)
                .Register();
        }
    }
}
