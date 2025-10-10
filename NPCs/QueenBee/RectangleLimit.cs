using Coralite.Helpers;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace TheTwinsRework.NPCs.QueenBee
{
    public class RectangleLimit : ModNPC
    {
        public override string Texture => AssetDirectory.Assets + Name;

        public ref float QueenBeeIndex => ref NPC.ai[0];
        public ref float State => ref NPC.ai[1];
        public ref float Timer => ref NPC.ai[2];

        public Corner TopLeft;
        public Corner DownLeft;
        public Corner TopRight;
        public Corner DownRight;

        public class Corner
        {
            /// <summary>
            /// 刺球的位置
            /// </summary>
            public readonly Vector2 ballPos;
            /// <summary>
            /// 藤蔓延申的方向
            /// </summary>
            public readonly Vector2 vineDir;
            /// <summary>
            /// 最大距离
            /// </summary>
            public readonly float maxLength;

            public Vector2 TipPos;
            public float rot;
            public float scale;

            public Vector2 midPos;

            public Corner(Vector2 ballPos, Vector2 vineDir, float maxLength)
            {
                this.ballPos = ballPos;
                this.vineDir = vineDir;
                this.maxLength = maxLength;
                rot = Main.rand.NextFloat(MathHelper.TwoPi);
            }

            public void Update()
            {

            }

            public void DrawVineLine(SpriteBatch spriteBatch, Vector2 screenPos, Texture2D lineTex, Texture2D TipTex)
            {
                List<ColoredVertex> bars = new();

                float halfLineWidth = lineTex.Width / 2;

                Vector2 startPos = TipPos;
                Vector2 endPos = ballPos;

                Vector2 recordPos = startPos;
                float recordUV = 0;

                int lineLength = (int)(startPos - endPos).Length();   //链条长度
                int pointCount = lineLength / 16 + 3;
                Vector2 controlPos = midPos;

                //贝塞尔曲线
                for (int i = 0; i < pointCount; i++)
                {
                    float factor = (float)i / pointCount;

                    Vector2 P1 = Vector2.Lerp(startPos, controlPos, factor);
                    Vector2 P2 = Vector2.Lerp(controlPos, endPos, factor);

                    Vector2 Center = Vector2.Lerp(P1, P2, factor);
                    var Color = GetStringColor(Center);

                    Vector2 normal = (P2 - P1).SafeNormalize(Vector2.One).RotatedBy(MathHelper.PiOver2);
                    Vector2 Top = Center + normal * halfLineWidth;
                    Vector2 Bottom = Center - normal * halfLineWidth;

                    recordUV += (Center - recordPos).Length() / lineTex.Width;

                    bars.Add(new(Top - screenPos, Color, new Vector3(0, recordUV, 1)));
                    bars.Add(new(Bottom - screenPos, Color, new Vector3(1, recordUV, 1)));

                    recordPos = Center;
                }

                //spriteBatch.End();
                //spriteBatch.Begin(SpriteSortMode.Deferred, spriteBatch.GraphicsDevice.BlendState, Main.spriteBatch.GraphicsDevice.SamplerStates[0],
                //                spriteBatch.GraphicsDevice.DepthStencilState, spriteBatch.GraphicsDevice.RasterizerState, null, Main.GameViewMatrix.TransformationMatrix);

                var state = Main.graphics.GraphicsDevice.SamplerStates[0];
                Main.graphics.GraphicsDevice.Textures[0] = lineTex;
                Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
                Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, bars.ToArray(), 0, bars.Count - 2);
                Main.graphics.GraphicsDevice.SamplerStates[0] = state;
            }

            public virtual Color GetStringColor(Vector2 pos)
            {
                return Lighting.GetColor((int)pos.X / 16, (int)(pos.Y / 16f), Color.White);
            }

            public void DrawBall(SpriteBatch spriteBatch, Vector2 screenPos, Texture2D tex)
            {
                tex.QuickCenteredDraw(spriteBatch, ballPos - screenPos, Lighting.GetColor(ballPos.ToTileCoordinates())
                    , rot, scale);
            }
        }

        public static Asset<Texture2D> VineTex;
        public static Asset<Texture2D> VineTipTex;
        public static Asset<Texture2D> VineBallTex;

        public override void Load()
        {
            if (Main.dedServ)
                return;

            VineTex = ModContent.Request<Texture2D>(AssetDirectory.Assets + "Vine");
            VineTipTex = ModContent.Request<Texture2D>(AssetDirectory.Assets + "VineTip");
            VineBallTex = ModContent.Request<Texture2D>(AssetDirectory.Assets + "VineBall");
        }

        public override void Unload()
        {
        }

        public override void SetDefaults()
        {
            NPC.lifeMax = 1000;
            NPC.width = NPC.height = 16;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.knockBackResist = 0;
            NPC.npcSlots = 20;
            NPC.hide = true;
        }

        public override bool? CanBeHitByItem(Player player, Item item)
            => false;
        public override bool? CanBeHitByProjectile(Projectile projectile)
            => false;
        public override bool CanBeHitByNPC(NPC attacker)
            => false;

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            return false;
        }

        public override void ModifyHoverBoundingBox(ref Rectangle boundingBox)
        {
            boundingBox = new Rectangle();
        }

        public override void AI()
        {
            if (!QueenBeeIndex.GetNPCOwner<BeastlyQueenBee>(out _))
            {

            }
        }



        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D vine = VineTex.Value;
            Texture2D vineTip = VineTipTex.Value;
            Texture2D Ball = VineBallTex.Value;
            TopLeft?.DrawVineLine(spriteBatch, screenPos, vine, vineTip);
            TopRight?.DrawVineLine(spriteBatch, screenPos, vine, vineTip);
            DownLeft?.DrawVineLine(spriteBatch, screenPos, vine, vineTip);
            DownRight?.DrawVineLine(spriteBatch, screenPos, vine, vineTip);

            TopLeft?.DrawBall(spriteBatch, screenPos, Ball);
            TopRight?.DrawBall(spriteBatch, screenPos, Ball);
            DownLeft?.DrawBall(spriteBatch, screenPos, Ball);
            DownRight?.DrawBall(spriteBatch, screenPos, Ball);

            return false;
        }
    }
}
