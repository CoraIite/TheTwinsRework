using Coralite.Helpers;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria.ModLoader;
using TheTwinsRework.Core.System_Particle;
using TheTwinsRework.NPCs.TheTwins;

namespace TheTwinsRework.Particles
{
    public class AimLine : ModParticle
    {
        public override string Texture => AssetDirectory.Assets + Name;

        public static Asset<Texture2D> LineTex;

        public struct IncomeData
        {
            public int index, maxTime;
            public int? circleIndex;
            public float? rot;
        }

        public override bool NotDrawOutofScreen => false;

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

        public override void Update(Particle particle)
        {
            if (particle.data == null)
            {
                particle.active = false;
                return;
            }
            IncomeData data = (IncomeData)particle.data;
            if (data.index.GetNPCOwner(out NPC owner))
            {
                if (data.rot.HasValue)
                {
                    particle.rotation = data.rot.Value;
                }
                else
                    particle.rotation = owner.rotation;
                particle.center = owner.Center;
            }
            else
                particle.active = false;

            particle.fadeIn++;
            if (particle.fadeIn > data.maxTime)
            {
                particle.active = false;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Particle particle)
        {
            if (particle.data == null)
            {
                return;
            }

            Texture2D tex = Texture2D.Value;
            IncomeData data = (IncomeData)particle.data;
            Color c = particle.color * Helper.SqrtEase(particle.fadeIn / data.maxTime);

            float scale2 = 0.9f + 0.015f * MathF.Sin((int)Main.timeForVisualEffects*0.35f);

            Vector2 position = particle.center - Main.screenPosition;
            Main.spriteBatch.Draw(tex, position, null
                , c, particle.rotation, tex.Size() / 2, scale2, 0, 0);
            Main.spriteBatch.Draw(tex, position, null
                , c with { A = 0 } * 0.5f, particle.rotation, tex.Size() / 2, scale2, SpriteEffects.FlipVertically, 0);

            tex = LineTex.Value;
            position += particle.rotation.ToRotationVector2() * 20;
            Vector2 origin = new Vector2(0, tex.Height / 2);

            float Length = CircleLimit.MaxLength-60;
            if (data.circleIndex.HasValue && data.circleIndex.Value.GetNPCOwner(out NPC owner))//测量长度
            {
                Vector2 pos = particle.center;
                Vector2 dir = particle.rotation.ToRotationVector2() * 16;

                for (int i = 0; i < 100; i++)
                {
                    if (Vector2.Distance(pos, owner.Center) > CircleLimit.MaxLength)
                        break;

                    pos += dir;
                }

                Length = Vector2.Distance(pos, particle.center);
            }

            Vector2 scale = new Vector2(Length / tex.Width, 0.3f);
            if (particle.fadeIn < 5)
            {
                scale = Vector2.Lerp(Vector2.Zero, new Vector2(scale.X * 0.2f, 4), particle.fadeIn / 5);
            }
            else if (particle.fadeIn < 10)
            {
                scale = Vector2.Lerp(new Vector2(scale.X * 0.2f, 4), scale, (particle.fadeIn - 5) / 5);
            }


            Main.spriteBatch.Draw(tex, position, null
                    , c * 0.5f, particle.rotation, origin, scale, 0, 0);
            Main.spriteBatch.Draw(tex, position, null
                , c with { A = 0 } * 0.5f, particle.rotation, origin, scale, 0, 0);
        }
    }
}
