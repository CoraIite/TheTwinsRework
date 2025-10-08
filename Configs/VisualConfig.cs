using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace TheTwinsRework.Configs
{
    public class VisualConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [DefaultValue(true)]
        public bool ScreenMove;
        [DefaultValue(true)]
        public bool ShowBossBar;

        public override void OnChanged()
        {
            VisualConfigSystem.ScreenMove = ScreenMove;
            VisualConfigSystem.ShowBossBar = ShowBossBar;
        }
    }

    public class VisualConfigSystem
    {
        public static bool ScreenMove;
        public static bool ShowBossBar;
    }
}
