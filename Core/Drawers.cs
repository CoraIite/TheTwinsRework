using Coralite.Helpers;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
using TheTwinsRework.Core.Loader;
using TheTwinsRework.Core.System_Particle;

namespace TheTwinsRework.Core
{
    public class Drawers : ModSystem
    {
        public override void Load()
        {
            if (Main.dedServ)
                return;

            On_Main.DrawDust += Drawer;
        }

        /// <summary>
        /// 在绘制粒子之后插一段，使用自己的渲染方式
        /// </summary>
        private void Drawer(On_Main.orig_DrawDust orig, Main self)
        {
            orig(self);

            if (Main.gameMenu)
                return;

            //绘制拖尾
            SpriteBatch spriteBatch = Main.spriteBatch;
            Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            //for (int k = 0; k < Main.maxProjectiles; k++) // Projectiles.
            //    if (Main.projectile[k].active && Main.projectile[k].ModProjectile is IDrawPrimitive)
            //        (Main.projectile[k].ModProjectile as IDrawPrimitive).DrawPrimitives();

            //for (int k = 0; k < Main.maxNPCs; k++) // NPCs.
            //    if (Main.npc[k].active && Main.npc[k].ModNPC is IDrawPrimitive)
            //        (Main.npc[k].ModNPC as IDrawPrimitive).DrawPrimitives();

            for (int k = 0; k < TheTwinsRework.maxParticle - 1; k++) // Particles.
                if (ParticleSystem.Particles[k].active)
                {
                    ModParticle modParticle = ParticleLoader.GetParticle(ParticleSystem.Particles[k].type);
                    if (modParticle is IDrawParticlePrimitive)
                        (modParticle as IDrawParticlePrimitive).DrawPrimitives(ParticleSystem.Particles[k]);
                }

            //绘制Non
            //spriteBatch.Begin(default, BlendState.NonPremultiplied, SamplerState.PointWrap, default, default, default, Main.GameViewMatrix.ZoomMatrix);

            //for (int k = 0; k < Main.maxProjectiles; k++) //Projectiles
            //    if (Main.projectile[k].active && Main.projectile[k].ModProjectile is IDrawNonPremultiplied)
            //        (Main.projectile[k].ModProjectile as IDrawNonPremultiplied).DrawNonPremultiplied(Main.spriteBatch);

            //for (int k = 0; k < Main.maxNPCs; k++) //NPCs
            //    if (Main.npc[k].active && Main.npc[k].ModNPC is IDrawNonPremultiplied)
            //        (Main.npc[k].ModNPC as IDrawNonPremultiplied).DrawNonPremultiplied(Main.spriteBatch);

            //spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.Transform);

            //绘制自己的粒子
            ArmorShaderData armorShaderData = null;
            for (int i = 0; i < TheTwinsRework.maxParticle; i++)
            {
                Particle particle = ParticleSystem.Particles[i];
                if (!particle.active)
                    continue;

                // public static bool OnScreen(Vector2 pos) => pos.X > -16 && pos.X < Main.screenWidth + 16 && pos.Y > -16 && pos.Y < Main.screenHeight + 16;
                // 星光河里抄来的onScreen方法，随便找个静态类丢进去就行了
                ModParticle mp = ParticleLoader.GetParticle(particle.type);

                if (mp.NotDrawOutofScreen && !Helper.IsPointOnScreen(particle.center - Main.screenPosition))
                    continue;

                if (particle.shader != armorShaderData)
                {
                    spriteBatch.End();
                    armorShaderData = particle.shader;
                    if (armorShaderData == null)
                        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, default, default, default, Main.GameViewMatrix.ZoomMatrix);
                    else
                    {
                        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.Transform);
                        particle.shader.Apply(null);
                    }
                }

                mp.Draw(spriteBatch, particle);
            }

            spriteBatch.End();
        }
    }
}

