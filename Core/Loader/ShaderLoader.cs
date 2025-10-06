using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using System.Reflection;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using static Terraria.ModLoader.Core.TmodFile;

namespace TheTwinsRework.Core.Loader
{
   public class ShaderLoader : ModSystem
    {
        public override void Load()
        {
            if (Main.dedServ)
                return;

            MethodInfo info = typeof(Mod).GetProperty("File", BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod(true);
            var file = (TmodFile)info.Invoke(TheTwinsRework.Instance, null);

            var shaders = file.Where(n => n.Name.StartsWith("Effects/") && n.Name.EndsWith(".xnb"));

            foreach (FileEntry entry in shaders)
            {
                var name = entry.Name.Replace(".xnb", "").Replace("Effects/", "");
                var path = entry.Name.Replace(".xnb", "");
                LoadShader(name, path);
            }
        }

        public override void Unload()
        {

        }

        /// <summary>
        /// 加载shader
        /// </summary>
        /// <param name="name"></param>
        /// <param name="path"></param>
        public static void LoadShader(string name, string path)
        {
            var screenRef = TheTwinsRework.Instance.Assets.Request<Effect>(path, ReLogic.Content.AssetRequestMode.ImmediateLoad);
            name = nameof(TheTwinsRework) + name;
            Filters.Scene[name] = new Filter(new ScreenShaderData(screenRef, name + "Pass"), EffectPriority.High);
            Filters.Scene[name].Load();
        }

        public static Effect GetShader(string name)
        {
            return Filters.Scene[nameof(TheTwinsRework)+name].GetShader().Shader;
        }
    }
}
