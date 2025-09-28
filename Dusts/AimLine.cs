using Coralite.Helpers;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;
using TheTwinsRework.NPCs;

namespace TheTwinsRework.Dusts
{
    public class AimLine : ModDust
    {
        public override string Texture => AssetDirectory.Assets + Name;

        public static Asset<Texture2D> LineTex;

        public struct IncomeData
        {
            public int index, maxTime;
            public int? circleIndex;
            public float? rot;
        }

        public override void Load()
        {
            if (Main.dedServ)
                return;

            LineTex = ModContent.Request<Texture2D>(AssetDirectory.Assets + "Line");
        }

        public override void Unload()
        {
            LineTex = null;
        }

        public override bool Update(Dust dust)
        {
            IncomeData data = (IncomeData)dust.customData;
            if (data.index.GetNPCOwner(out NPC owner))
            {
                if (data.rot.HasValue)
                {
                    dust.rotation = data.rot.Value;
                }
                else
                    dust.rotation = owner.rotation;
                dust.position = owner.Center;
            }
            else
                dust.active = false;

            dust.fadeIn++;
            if (dust.fadeIn > data.maxTime)
            {
                dust.active = false;
            }



            return false;
        }

        public override bool PreDraw(Dust dust)
        {
            Texture2D tex = Texture2D.Value;
            IncomeData data = (IncomeData)dust.customData;
            Color c = dust.color * Helper.SqrtEase(dust.fadeIn / data.maxTime);

            Vector2 position = dust.position - Main.screenPosition;
            Main.spriteBatch.Draw(tex, position, null
                , c, dust.rotation, tex.Size() / 2, 0.9f, 0, 0);
            Main.spriteBatch.Draw(tex, position, null
                , c with { A = 0 }, dust.rotation, tex.Size() / 2, 0.9f, SpriteEffects.FlipVertically, 0);

            tex = LineTex.Value;
            position += dust.rotation.ToRotationVector2() * 20;
            Vector2 origin = new Vector2(0, tex.Height / 2);

            float Length = CircleLimit.MaxLength-60;
            if (data.circleIndex.HasValue && data.circleIndex.Value.GetNPCOwner(out NPC owner))//测量长度
            {
                Vector2 pos = dust.position;
                Vector2 dir = dust.rotation.ToRotationVector2() * 16;

                for (int i = 0; i < 100; i++)
                {
                    if (Vector2.Distance(pos, owner.Center) > CircleLimit.MaxLength)
                        break;

                    pos += dir;
                }

                Length = Vector2.Distance(pos, dust.position);
            }

            Vector2 scale = new Vector2(Length / tex.Width, 0.3f);
            if (dust.fadeIn < 5)
            {
                scale = Vector2.Lerp(Vector2.Zero, new Vector2(scale.X * 0.2f, 4), dust.fadeIn / 5);
            }
            else if (dust.fadeIn < 10)
            {
                scale = Vector2.Lerp(new Vector2(scale.X * 0.2f, 4), scale, (dust.fadeIn - 5) / 5);
            }


            Main.spriteBatch.Draw(tex, position, null
                    , c * 0.5f, dust.rotation, origin, scale, 0, 0);
            Main.spriteBatch.Draw(tex, position, null
                , c with { A = 0 } * 0.5f, dust.rotation, origin, scale, 0, 0);

            return false;
        }
    }
}
