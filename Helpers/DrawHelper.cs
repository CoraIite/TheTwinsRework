using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace Coralite.Helpers
{
    public static partial class Helper
    {
        /// <summary>
        /// 快速绘制
        /// </summary>
        /// <param name="tex"></param>
        /// <param name="spriteBatch"></param>
        /// <param name="pos"></param>
        /// <param name="selfColor"></param>
        /// <param name="rotation"></param>
        /// <param name="scale"></param>
        public static void QuickCenteredDraw(this Texture2D tex, SpriteBatch spriteBatch, Vector2 pos, Color? selfColor = null, float rotation = 0, float scale = 1,SpriteEffects effect=SpriteEffects.None)
        {
            spriteBatch.Draw(tex, pos, null, selfColor ?? Color.White, rotation, tex.Size() / 2, scale, effect, 0);
        }

        public static void QuickBottomDraw(this Texture2D tex, SpriteBatch spriteBatch, Vector2 pos, Color? selfColor = null, float rotation = 0, float scale = 1)
        {
            spriteBatch.Draw(tex, pos, null, selfColor ?? Color.White, rotation, new Vector2(tex.Width / 2, tex.Height), scale, 0, 0);
        }

        /// <summary>
        /// 快速绘制
        /// </summary>
        /// <param name="tex"></param>
        /// <param name="spriteBatch"></param>
        /// <param name="pos"></param>
        /// <param name="selfColor"></param>
        /// <param name="rotation"></param>
        /// <param name="scale"></param>
        public static void QuickCenteredDraw(this Texture2D tex, SpriteBatch spriteBatch, Rectangle frame, Vector2 pos, Color? selfColor = null, float rotation = 0, float scale = 1, SpriteEffects effect = SpriteEffects.None)
        {
            var frameBox = tex.Frame(frame.Width, frame.Height, frame.X, frame.Y);
            spriteBatch.Draw(tex, pos, frameBox, selfColor ?? Color.White, rotation, frameBox.Size() / 2, scale, effect, 0);
        }

        public static void QuickCenteredDraw(this Texture2D tex, SpriteBatch spriteBatch, Rectangle frame, Vector2 pos, SpriteEffects effect, Color? selfColor = null, float rotation = 0, float scale = 1)
        {
            var frameBox = tex.Frame(frame.Width, frame.Height, frame.X, frame.Y);
            spriteBatch.Draw(tex, pos, frameBox, selfColor ?? Color.White, rotation, frameBox.Size() / 2, scale, effect, 0);
        }
    }
}
