using System.Collections.Generic;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using TheTwinsRework.Items;
using TheTwinsRework.NPCs.TheTwins;
using static Terraria.ModLoader.ModContent;

namespace TheTwinsRework.Compat.BossCheckList
{
    public static class BossCheckListCalls
    {
        public static void CallBossCheckList()
        {
#pragma warning disable CS8974 // 将方法组转换为非委托类型

            if (ModLoader.TryGetMod("BossCheckList", out Mod bcl))
            {
                //兹雷龙
                AddTheTwinsDancer(bcl);
            }
        }

        public static void AddTheTwinsDancer(Mod bcl)
        {
            List<int> Collection = new()
                {
                    ItemID.TwinsBossBag,
                    ItemID.TwinsMasterTrophy,
                    ItemID.TwinsPetItem,
                };

            bcl.Call(
                "LogBoss",
                TheTwinsRework.Instance,
                "双子舞者",
                9.5f,
                () => NPC.downedMechBoss2,
                NPCType<CircleLimit>(),
                new Dictionary<string, object>()
                {
                    ["spawnInfo"] = Language.GetOrRegister($"Mods.TheTwinsRework.Compat.BossChecklist.TheTwinsDancer.SpawnInfo", () => ""),
                    ["despawnMessage"] = Language.GetOrRegister($"Mods.TheTwinsRework.Compat.BossChecklist.TheTwinsDancer.Despawn", () => ""),
                    ["spawnItems"] = ItemType<TheTwinsReworkSummon>(),
                    ["customPortrait"] = TheTwinsPortrait.DrawPortrait,
                    ["collectibles"] = Collection,
                });
        }

#pragma warning restore CS8974 // 将方法组转换为非委托类型
    }
}
