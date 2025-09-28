global using Terraria;
global using Microsoft.Xna.Framework;

using Terraria.ModLoader;

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

        public override void Load()
        {
            base.Load();
        }

        public override void Unload()
        {
            base.Unload();
        }
    }
}
