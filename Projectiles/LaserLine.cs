using Coralite.Helpers;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using TheTwinsRework.NPCs;

namespace TheTwinsRework.Projectiles
{
    /// <summary>
    /// ai0传入圆圈索引，ai1传入准备时间，ai2传入持有者
    /// </summary>
    public class LaserLine : ModProjectile
    {
        public override string Texture => AssetDirectory.Assets + "Line";

        public ref float CircleIndex => ref Projectile.ai[0];
        public ref float ReadyTime => ref Projectile.ai[1];
        /// <summary>
        /// 如果不为-1代表跟踪NPC持有者
        /// </summary>
        public ref float FollowIndex => ref Projectile.ai[2];

        public ref float LengthRecord => ref Projectile.localAI[0];
        public ref float Timer => ref Projectile.localAI[1];
        public ref float State => ref Projectile.localAI[2];

        public int ShootTime;

        private bool Record = true;
        private float LaserWidth;

        public ThunderTrail[] thunderTrails;

        public static Asset<Texture2D> gradientTex;
        public static Asset<Texture2D> laserTex;
        public static Asset<Texture2D> flowTex;
        public static Asset<Texture2D> ballTex;

        public override void Load()
        {
            if (!Main.dedServ)
            {
                gradientTex = ModContent.Request<Texture2D>(AssetDirectory.Assets + "LaserGradient");
                laserTex = ModContent.Request<Texture2D>(AssetDirectory.Assets + "ThunderTrailB2");
                flowTex = ModContent.Request<Texture2D>(AssetDirectory.Assets + "VanillaFlowA");
                ballTex = ModContent.Request<Texture2D>(AssetDirectory.Assets + "RedBall");
            }
        }

        public override void Unload()
        {
            if (!Main.dedServ)
            {
                gradientTex = null;
                laserTex = null;
            }
        }

        public override void SetDefaults()
        {
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.hostile = true;
        }

        public override bool ShouldUpdatePosition() => false;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (State != 1)
                return false;

            float a = 0;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center
                , Projectile.Center + Projectile.velocity * LengthRecord, LaserWidth, ref a);
        }

        public override void AI()
        {
            if (!CircleIndex.GetNPCOwner(out NPC Circle, Projectile.Kill))
                return;

            //确定长度
            if (FollowIndex.GetNPCOwner(out NPC owner, () => { FollowIndex = -1; }))
            {
                Projectile.rotation = owner.rotation;
                Projectile.velocity = Projectile.rotation.ToRotationVector2();
                Projectile.Center = owner.Center + owner.rotation.ToRotationVector2() * 90;
                SetLength(Projectile.velocity, Circle);
            }
            else if (Record)
            {
                Record = false;

                float rot = Projectile.velocity.ToRotation();
                SetLength(Projectile.velocity, Circle);
            }

            switch (State)
            {
                case 0://准备阶段，生成线条
                    {
                        Timer++;
                        if (Timer >= ReadyTime)
                        {
                            State = 1;
                            Timer = 0;

                            if (Main.netMode != NetmodeID.Server)
                            {
                                SoundEngine.PlaySound(CoraliteSoundID.LaserShoot_Item33, Projectile.Center);
                                Helper.PlayPitched("Lightning_Zap" + Main.rand.Next(1, 4).ToString()
                                    , 0.9f, 0, Projectile.Center);

                                thunderTrails = new ThunderTrail[4];
                                var tex = ModContent.Request<Texture2D>(AssetDirectory.Assets + "ThunderTrailB2");

                                for (int i = 0; i < 2; i++)
                                {
                                    thunderTrails[i] = new ThunderTrail(tex
                                        , f => Helper.SinEase(f) * 25, ThunderColorFuncBack, GetAlpha);

                                    thunderTrails[i].CanDraw = false;
                                    thunderTrails[i].UseNonOrAdd = true;
                                    thunderTrails[i].SetRange((10, 25));
                                    thunderTrails[i].SetExpandWidth(4);
                                    thunderTrails[i].BasePositions =
                                    [
                                    Projectile.Center,Projectile.Center,Projectile.Center
                                    ];
                                }
                                for (int i = 2; i < 4; i++)
                                {
                                    thunderTrails[i] = new ThunderTrail(tex
                                        , f => Helper.SinEase(f) * 15, ThunderColorFuncRed, GetAlpha);

                                    thunderTrails[i].CanDraw = false;
                                    //thunderTrails[i].UseNonOrAdd = true;

                                    thunderTrails[i].SetRange((0, 30));
                                    thunderTrails[i].SetExpandWidth(4);
                                    thunderTrails[i].BasePositions =
                                    [
                                    Projectile.Center,Projectile.Center,Projectile.Center
                                    ];
                                }
                            }
                        }
                    }
                    break;
                case 1://激光射射射
                    {
                        if (Main.netMode != NetmodeID.Server)
                        {
                            List<Vector2> pos = new()
                            {
                                Projectile.Center
                            };

                            int count = (int)(LengthRecord / 55);
                            for (int i = 1; i < count; i++)
                            {
                                pos.Add(Projectile.Center + Projectile.velocity * i * 55);
                            }

                            foreach (var trail in thunderTrails)
                            {
                                trail.UpdateThunderToNewPosition([.. pos]);
                            }

                            if (Timer % 4 == 0)
                            {
                                foreach (var trail in thunderTrails)
                                {
                                    trail.CanDraw = Main.rand.NextBool();
                                    trail.RandomThunder();
                                }
                            }
                        }

                        Lighting.AddLight(Projectile.Center, new Vector3(1.5f, 0, 0) * LaserWidth / 40);
                        int time = ShootTime;
                        if (Timer < 10)
                        {
                            LaserWidth = Helper.Lerp(0, 80, Timer / 10);
                        }
                        else if (Timer < 15)
                        {
                            LaserWidth = Helper.Lerp(80, 30, (Timer - 10) / 5);
                        }
                        else if (Timer > time * 0.8f)
                        {
                            LaserWidth = Helper.Lerp(30, 0, (Timer - time * 0.8f) / (time * 0.2f));
                        }

                        if (Timer % 25 == 0 && Main.rand.NextBool())
                            Helper.PlayPitched("LightningAntic", 0.8f, 0, Projectile.Center);

                        Timer++;
                        if (Timer >= time)
                        {
                            Projectile.Kill();
                        }
                    }
                    break;
            }
        }

        public void SetLength(Vector2 dir, NPC circle)
        {
            bool closer = true;

            Vector2 pos = Projectile.Center;

            for (int i = 0; i < 100; i++)
            {
                if (closer)//还没远离的时候，记录由远到近
                {
                    if (Vector2.Distance(pos, circle.Center) < CircleLimit.MaxLength)
                    {
                        closer = false;
                    }
                }
                else//近了之后远离
                {
                    if (Vector2.Distance(pos, circle.Center) > CircleLimit.MaxLength)
                        break;
                }

                pos += dir * 16;
            }

            LengthRecord = Vector2.Distance(pos, Projectile.Center);
        }

        public Color ThunderColorFuncRed(float factor)
        {
            return Color.Red;
        }

        public Color ThunderColorFuncBack(float factor)
        {
            return new Color(125,15,45);
        }

        public float GetAlpha(float factor)
        {
            return 1 - Timer / ShootTime;
        }


        public override bool PreDraw(ref Color lightColor)
        {
            if (thunderTrails != null)
            {
                thunderTrails[0].DrawThunder(Main.instance.GraphicsDevice);
                thunderTrails[1].DrawThunder(Main.instance.GraphicsDevice);
            }

            switch (State)
            {
                default:
                case 0:
                    {
                        Texture2D tex = Projectile.GetTexture();
                        Vector2 position = Projectile.Center - Main.screenPosition;
                        Vector2 origin = new Vector2(0, tex.Height / 2);
                        Color c = Color.Red * Helper.SqrtEase(Timer / ReadyTime);

                        float Length = LengthRecord;

                        Vector2 scale = new Vector2(Length / tex.Width, 0.3f);
                        if (Timer < 5)
                        {
                            scale = Vector2.Lerp(Vector2.Zero, new Vector2(scale.X * 0.2f, 4), Timer / 5);
                        }
                        else if (Timer < 10)
                        {
                            scale = Vector2.Lerp(new Vector2(scale.X * 0.2f, 4), scale, (Timer - 5) / 5);
                        }

                        Main.spriteBatch.Draw(tex, position, null
                                , c * 0.5f, Projectile.rotation, origin, scale, 0, 0);
                        Main.spriteBatch.Draw(tex, position, null
                            , c with { A = 0 } * 0.5f, Projectile.rotation, origin, scale, 0, 0);
                    }
                    break;
                case 1:
                    {
                        DrawPrimitive(Main.spriteBatch);
                    }
                    break;
            }

            if (thunderTrails != null)
            {
                thunderTrails[2].DrawThunder(Main.instance.GraphicsDevice);
                thunderTrails[3].DrawThunder(Main.instance.GraphicsDevice);
            }

            return false;
        }

        public virtual void DrawPrimitive(SpriteBatch spriteBatch)
        {
            Texture2D tex = ballTex.Value;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, default, Main.GameViewMatrix.ZoomMatrix);

            RasterizerState originalState = Main.graphics.GraphicsDevice.RasterizerState;
            List<VertexPositionColorTexture> bars = new();
            float count = 40;
            Vector2 dir = (Projectile.rotation + 1.57f).ToRotationVector2();

            for (int i = 0; i <= count; i++)
            {
                float factor = 1f - (i / count);
                Vector2 Center = Vector2.Lerp(Projectile.Center, Projectile.Center + Projectile.velocity * LengthRecord, i / count);
                Vector2 width = LaserWidth * dir;
                Vector2 Top = Center + width;
                Vector2 Bottom = Center - width;

                bars.Add(new(Top.Vec3(), Color.White, new Vector2(factor, 0)));
                bars.Add(new(Bottom.Vec3(), Color.White, new Vector2(factor, 1)));
            }

            if (bars.Count > 2)
            {
                Effect effect = Filters.Scene["LaserAlpha"].GetShader().Shader;

                Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
                Matrix view = Main.GameViewMatrix.TransformationMatrix;
                Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

                effect.Parameters["uTime"].SetValue(Main.GlobalTimeWrappedHourly * 2);
                effect.Parameters["exAdd"].SetValue(0.2f);
                effect.Parameters["transformMatrix"].SetValue(world * view * projection);
                effect.Parameters["sampleTexture"].SetValue(laserTex.Value);
                effect.Parameters["gradientTexture"].SetValue(gradientTex.Value);
                effect.Parameters["extTexture"].SetValue(flowTex.Value);

                Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
                foreach (EffectPass pass in effect.CurrentTechnique.Passes) //应用shader，并绘制顶点
                {
                    pass.Apply();
                    Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, bars.ToArray(), 0, bars.Count - 2);
                }

                Main.graphics.GraphicsDevice.RasterizerState = originalState;
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            float scale = LaserWidth * 2.8f / (tex.Width - 10);
            Vector2 origin = tex.Size() / 2;
            Vector2 pos = Projectile.Center - Main.screenPosition;
            spriteBatch.Draw(tex, pos, null, Color.White, 0, origin, scale, 0, 0);

            pos += Projectile.velocity * LengthRecord;
            spriteBatch.Draw(tex, pos, null, Color.White, 0, origin, scale, 0, 0);


            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}
