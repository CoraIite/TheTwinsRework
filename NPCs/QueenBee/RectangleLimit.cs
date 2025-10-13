using Coralite.Helpers;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace TheTwinsRework.NPCs.QueenBee
{
    public class RectangleLimit : ModNPC
    {
        public override string Texture => AssetDirectory.Assets + Name;

        public ref float QueenBeeIndex => ref NPC.ai[0];
        public ref float State => ref NPC.ai[1];
        public ref float Timer => ref NPC.ai[2];

        public Corner[] Lefts;
        public Corner[] Rights;
        public Corner[] Downs;
        public Corner[] Ups;

        public static int LimitWidth = 460;
        public static int LimitHeight = 340;

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
            public Vector2 vel;
            public int State;
            public int Timer;

            public Corner(Vector2 ballPos, Vector2 vineDir, float maxLength)
            {
                this.ballPos = ballPos;
                this.vineDir = vineDir;
                this.maxLength = maxLength;
                rot = Main.rand.NextFloat(MathHelper.TwoPi);
            }

            public void Update()
            {
                const int length = 20;

                const float SpawnTime = 30;
                const float ThronExpandTime = 15;

                switch (State)
                {
                    default:
                    case 0://刺球出现
                        {
                            Timer++;
                            scale = Helper.HeavyEase(Timer / SpawnTime);

                            TipPos = ballPos + vineDir * Timer / SpawnTime * length;
                            midPos = (ballPos + TipPos) / 2;

                            if (Timer > SpawnTime)
                            {
                                Timer = 0;
                                State = 1;
                            }
                        }
                        break;
                    case 1://尖刺展开
                        {
                            Timer++;

                            TipPos = ballPos + vineDir * (length + Timer / ThronExpandTime * (maxLength - length));
                            midPos = (ballPos + TipPos) / 2;

                            if (Timer > ThronExpandTime)
                            {
                                Timer = 0;
                                State = 2;
                            }
                        }
                        break;
                    case 2://不动
                        {

                        }
                        break;
                    case 3://弹出并回弹
                        {
                            Timer++;
                            if (Timer < 6)
                            {
                                vel *= 0.96f;
                                midPos += vel;
                            }
                            else if (Timer < 6 + 8)
                            {
                                midPos = Vector2.Lerp(midPos, (ballPos + TipPos) / 2, 0.2f);
                            }
                            else
                            {
                                midPos = (ballPos + TipPos) / 2;

                                Timer = 0;
                                State = 2;
                            }
                        }
                        break;
                }
            }

            public void CalculateMidPosY(float y)
            {
                float total = MathF.Abs(TipPos.Y - ballPos.Y);
                float yDis = MathF.Abs(y - ballPos.Y);

                midPos = new Vector2(Helper.Lerp(ballPos.X, TipPos.X, yDis / total), y);
            }

            public void CalaulateMidPosX(float x)
            {
                float total = MathF.Abs(TipPos.X - ballPos.X);
                float xDis = MathF.Abs(x - ballPos.X);

                midPos = new Vector2(x, Helper.Lerp(ballPos.Y, TipPos.Y, xDis / total));
            }

            public void Collide(Vector2 velocity)
            {
                vel = velocity;
                State = 3;
            }

            public void DrawVineLine(SpriteBatch spriteBatch, Vector2 screenPos, Texture2D lineTex, Texture2D TipTex)
            {
                List<ColoredVertex> bars = new();

                float halfLineHeight = lineTex.Height / 2;

                Vector2 startPos = TipPos;
                Vector2 endPos = ballPos-vineDir*10;

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
                    Vector2 Top = Center + normal * halfLineHeight;
                    Vector2 Bottom = Center - normal * halfLineHeight;

                    recordUV += (Center - recordPos).Length() / lineTex.Width;

                    bars.Add(new(Top - screenPos, Color, new Vector3(recordUV, 0, 1)));
                    bars.Add(new(Bottom - screenPos, Color, new Vector3(recordUV, 1, 1)));

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

                spriteBatch.Draw(TipTex, startPos - screenPos, null, GetStringColor(startPos), (startPos - endPos).ToRotation()
                    , new Vector2(0, TipTex.Height / 2), 1, 0,0);
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
            //NPC.hide = true;
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
            if (State == 0 && !QueenBeeIndex.GetNPCOwner<BeastlyQueenBee>(out _))
            {
                State = 1;
            }

            if (State == 0)
            {
                //生成刺球
                if (Timer == 0)
                {
                    LimitWidth = 700;
                    LimitHeight = 450;
                    InitSideVine();
                    InitBottonVine();
                    InitTopVine();
                    Timer = 1;
                }
            }
            else
            {
                Timer++;
                if (Timer > 400)
                {
                    NPC.Kill();
                    return;
                }
            }

            UpdateVine();
        }

        public void InitSideVine()
        {
            Lefts = new Corner[4];
            Rights = new Corner[4];

            Vector2 topLeft = NPC.Center + new Vector2(-LimitWidth / 2, LimitHeight / 2);
            Vector2 topRight = NPC.Center + new Vector2(LimitWidth / 2, LimitHeight / 2);
            Vector2 bottomLeft = NPC.Center + new Vector2(-LimitWidth / 2, -LimitHeight / 2);
            Vector2 bottomRight = NPC.Center + new Vector2(LimitWidth / 2, -LimitHeight / 2);

             float Xoffset = 10;
             float offset = 10;

            //大概是一个X字
            for (int i = 0; i < 2; i++)
            {
                float r = MathF.Cos(i * MathHelper.Pi);
                Xoffset = Main.rand.NextFloat(24, 30);
                offset = Main.rand.NextFloat(8, 12);

                Vector2 v1 = topLeft + new Vector2(-Xoffset * (i + 1), r * offset);
                Vector2 v2 = bottomLeft + new Vector2(-offset * (i + 1), r * offset);

                Lefts[0 + i * 2] = new Corner(v1, (v2 - v1).SafeNormalize(Vector2.Zero), Vector2.Distance(v1, v2));

                offset = Main.rand.NextFloat(8, 12);

                v1 = topLeft + new Vector2(-offset * (i + 1), r * offset);
                v2 = bottomLeft + new Vector2(-Xoffset * (i + 1), r * offset);
                Lefts[1 + i * 2] = new Corner(v2, (v1 - v2).SafeNormalize(Vector2.Zero), Vector2.Distance(v1, v2));

                offset = Main.rand.NextFloat(8, 12);

                v1 = topRight + new Vector2(Xoffset * (i + 1), r * offset);
                v2 = bottomRight + new Vector2(offset * (i + 1), r * offset);
                Rights[0 + i * 2] = new Corner(v1, (v2 - v1).SafeNormalize(Vector2.Zero), Vector2.Distance(v1, v2));

                offset = Main.rand.NextFloat(8, 12);

                v1 = topRight + new Vector2(offset * (i + 1), r * offset);
                v2 = bottomRight + new Vector2(Xoffset * (i + 1), r * offset);
                Rights[1 + i * 2] = new Corner(v2, (v1 - v2).SafeNormalize(Vector2.Zero), Vector2.Distance(v1, v2));
            }
        }

        public void InitBottonVine()
        {
            Downs = new Corner[2];

            const float offset = 10;

            Vector2 bottomLeft = NPC.Center + new Vector2(-LimitWidth / 2, LimitHeight / 2+offset);
            Vector2 bottomRight = NPC.Center + new Vector2(LimitWidth / 2, LimitHeight / 2+offset);

            Vector2 v1 = bottomLeft + new Vector2(0, -offset);
            Vector2 v2 = bottomRight + new Vector2(0, offset);
            Downs[0] = new Corner(v1, (v2 - v1).SafeNormalize(Vector2.Zero), Vector2.Distance(v1, v2));

            v1 = bottomLeft + new Vector2(0, offset);
            v2 = bottomRight + new Vector2(0, -offset);
            Downs[1] = new Corner(v2, (v1 - v2).SafeNormalize(Vector2.Zero), Vector2.Distance(v1, v2));
        }

        public void InitTopVine()
        {
            Ups = new Corner[4];

            float offset;
            float yoffset;

            Vector2 topLeft = NPC.Center + new Vector2(-LimitWidth / 2, -LimitHeight / 2);
            Vector2 topRight = NPC.Center + new Vector2(LimitWidth / 2, -LimitHeight / 2);

            for (int i = 0; i < 2; i++)
            {
                offset = Main.rand.NextFloat(18,22);
                yoffset = Main.rand.NextFloat(8, 12);
                float r = MathF.Cos(i * MathHelper.Pi);
                Vector2 v1 = topLeft + new Vector2(r * offset, -offset * (i + 1));
                Vector2 v2 = topRight + new Vector2(r * offset, -yoffset * (i + 1));
                Ups[0 + i * 2] = new Corner(v1, (v2 - v1).SafeNormalize(Vector2.Zero), Vector2.Distance(v1, v2));

                offset = Main.rand.NextFloat(18, 22);
                yoffset = Main.rand.NextFloat(8, 12);
                v1 = topLeft + new Vector2(r * offset, - yoffset* (i + 1));
                v2 = topRight + new Vector2(r * offset, -offset * (i + 1));
                Ups[1 + i * 2] = new Corner(v2, (v1 - v2).SafeNormalize(Vector2.Zero), Vector2.Distance(v1, v2));
            }
        }

        public void UpdateVine()
        {
            if (Ups != null)
                foreach (var v in Ups)
                    v.Update();
            if (Downs != null)
                foreach (var v in Downs)
                    v.Update();
            if (Lefts != null)
                foreach (var v in Lefts)
                    v.Update();
            if (Rights != null)
                foreach (var v in Rights)
                    v.Update();
        }

        public void CollideLeft(float y,float speed)
        {
            if (Lefts != null)
                foreach (var v in Lefts)
                {
                    v.CalculateMidPosY(y + Main.rand.NextFloat(-10, 10));
                    v.Collide(new Vector2(-7.5f - speed / 8, 0));
                }
        }

        public void CollideRight(float y, float speed)
        {
            if (Rights != null)
                foreach (var v in Rights)
                {
                    v.CalculateMidPosY(y + Main.rand.NextFloat(-10, 10));
                    v.Collide(new Vector2(7.5f + speed / 8, 0));
                }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D vine = VineTex.Value;
            Texture2D vineTip = VineTipTex.Value;
            Texture2D Ball = VineBallTex.Value;

            if (Ups != null)
                foreach (var v in Ups)
                    v.DrawVineLine(spriteBatch, screenPos, vine, vineTip);
            if (Downs != null)
                foreach (var v in Downs)
                    v.DrawVineLine(spriteBatch, screenPos, vine, vineTip);
            if (Lefts != null)
                foreach (var v in Lefts)
                    v.DrawVineLine(spriteBatch, screenPos, vine, vineTip);
            if (Rights != null)
                foreach (var v in Rights)
                    v.DrawVineLine(spriteBatch, screenPos, vine, vineTip);

            if (Ups != null)
                foreach (var v in Ups)
                    v.DrawBall(spriteBatch, screenPos, Ball);
            if (Downs != null)
                foreach (var v in Downs)
                    v.DrawBall(spriteBatch, screenPos, Ball);
            if (Lefts != null)
                foreach (var v in Lefts)
                    v.DrawBall(spriteBatch, screenPos, Ball);
            if (Rights != null)
                foreach (var v in Rights)
                    v.DrawBall(spriteBatch, screenPos, Ball);

#if DEBUG

            Vector2 p = NPC.Center - screenPos;
            spriteBatch.Draw(TextureAssets.MagicPixel.Value
                , new Rectangle((int)p.X - LimitWidth / 2, (int)p.Y - LimitHeight / 2, LimitWidth, LimitHeight)
                , Color.White * 0.2f);

#endif

            return false;
        }
    }
}
