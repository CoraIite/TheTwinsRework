global using Terraria;
global using Microsoft.Xna.Framework;

using Terraria.ModLoader;
using Terraria.Localization;
using TheTwinsRework.Compat.BossCheckList;

namespace TheTwinsRework
{
	public class TheTwinsRework : Mod
	{
        private static TheTwinsRework _instance;

        //µ¥ÀýÄ£Ê½£¡
        public static TheTwinsRework Instance
        {
            get
            {
                _instance ??= (TheTwinsRework)ModLoader.GetMod(nameof(TheTwinsRework));

                return _instance;
            }
            set
            {
                _instance = value;
            }
        }

        public static LocalizedText TheTwinsName;

        public override void Load()
        {
            if (Main.dedServ)
            {
                return;
            }

            TheTwinsName = GetLocalization(nameof(TheTwinsName));
        }

        public override void Unload()
        {
            TheTwinsName = null;
        }

        public override void PostSetupContent()
        {
            BossCheckListCalls.CallBossCheckList();
        }
    }
}
