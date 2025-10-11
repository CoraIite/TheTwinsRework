using Coralite.Helpers;
using Terraria.ID;
using Terraria.ModLoader;

namespace TheTwinsRework.Items
{
    public class BeastCrest : ModItem
    {
        public override string Texture => AssetDirectory.Assets + Name;

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.maxStack = 1;
            Item.value = Item.sellPrice(0, 2, 0, 0);
            Item.rare = ItemRarityID.Orange;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            if (player.TryGetModPlayer(out BeastCrestPlayer bcp))
            {
                bcp.BeastCrest = true;
            }
        }
    }

    public class BeastCrestGlobalItem : GlobalItem
    {
        public override bool? UseItem(Item item, Player player)
        {
            if (player.ItemAnimationJustStarted && item.healLife > 0)
            {
                if (player.TryGetModPlayer(out BeastCrestPlayer bcp) && bcp.BeastCrest)
                {
                    bcp.HealMax = item.healLife;
                    bcp.healValue = 0;
                    player.AddBuff(ModContent.BuffType<BeastCrestBuff>(), 60 * 17);
                }
            }

            return base.UseItem(item, player);
        }
    }

    public class BeastCrestBuff : ModBuff
    {
        public override string Texture => AssetDirectory.Assets + Name;

        public override void SetStaticDefaults()
        {
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetDamage(DamageClass.Generic) += 0.25f;
            if (player.buffTime[buffIndex] % (60 + 36) == 0&&Main.myPlayer==player.whoAmI)
            {
                Helper.PlayPitched("hornet_beast_mode_loop", 1f, 0);
            }
        }
    }

    public class BeastCrestPlayer : ModPlayer
    {
        public bool BeastCrest;
        public int HealMax;
        public int healValue;

        public override void ResetEffects()
        {
            BeastCrest = false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (BeastCrest && Player.HasBuff<BeastCrestBuff>() && healValue < HealMax)
            {
                int healLife = damageDone / 4;
                if (healLife < 1)
                    healLife = 1;

                if (healValue + healLife > HealMax)
                    healLife = HealMax - healValue;

                healValue += healLife;

                Player.Heal(healLife);
            }
        }

        public override void GetHealLife(Item item, bool quickHeal, ref int healValue)
        {
            if (BeastCrest)
            {
                healValue = 0;
            }
        }
    }
}
