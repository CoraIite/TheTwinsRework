using ReLogic.Utilities;
using System;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Terraria.Utilities;

namespace Coralite.Helpers
{
    public static partial class Helper
    {
        /// <summary> 角度转弧度系数 </summary>
        public const float Deg2Rad = 0.0174532924f;

        /// <summary>
        /// 将value限定在min和max之间
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static float Clamp(float value, float min, float max)
        {
            if (value < min)
            {
                value = min;
            }
            else if (value > max)
            {
                value = max;
            }

            return value;
        }

        /// <summary>
        /// 随机一个角度的方向向量
        /// </summary>
        /// <returns></returns>
        public static Vector2 NextVec2Dir()
        {
            return Main.rand.NextFloat(MathHelper.TwoPi).ToRotationVector2();
        }

        /// <summary>
        /// 随机一个角度的方向向量，并随机长度
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static Vector2 NextVec2Dir(float min, float max)
        {
            return Main.rand.NextFloat(MathHelper.TwoPi).ToRotationVector2() * Main.rand.NextFloat(min, max);
        }

        /// <summary>
        /// 把一个向量随机旋转
        /// </summary>
        /// <param name="baseVec2"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static Vector2 RotateByRandom(this Vector2 baseVec2, float min, float max)
        {
            return baseVec2.RotatedBy(Main.rand.NextFloat(min, max));
        }

        /// <summary>
        /// f为0时返回a,为1时返回b
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public static float Lerp(float a, float b, float f)
        {
            return (a * (1.0f - f)) + (b * f);
        }

        public static bool IsPointOnScreen(Vector2 pos) => pos.X > -16 && pos.X < Main.screenWidth + 16 && pos.Y > -16 && pos.Y < Main.screenHeight + 16;

        public static bool IsRectangleOnScreen(Rectangle rect) => rect.Intersects(new Rectangle(0, 0, Main.screenWidth, Main.screenHeight));

        public static bool IsAreaOnScreen(Vector2 pos, Vector2 size) => IsRectangleOnScreen(new Rectangle((int)pos.X, (int)pos.Y, (int)size.X, (int)size.Y));

        public static Rectangle QuickMouseRectangle()
           => new((int)Main.MouseWorld.X, (int)Main.MouseWorld.Y, 2, 2);

        /// <summary>
        /// 将你的值根据不同模式来改变
        /// </summary>
        /// <param name="normalModeValue">普通模式</param>
        /// <param name="expertModeValue">专家模式</param>
        /// <param name="masterModeValue">大师模式</param>
        /// <param name="FTWModeValue">For The Worthy模式（传奇难度）</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T ScaleValueForDiffMode<T>(T normalModeValue, T expertModeValue, T masterModeValue, T FTWModeValue)
        {
            T value = normalModeValue;
            if (Main.expertMode)
                value = expertModeValue;

            if (Main.masterMode)
                value = masterModeValue;

            if (Main.getGoodWorld)
                value = FTWModeValue;

            return value;
        }

        public static Point NextInRectangleEdge(this UnifiedRandom rand, Point topLeft, Point size)
        {
            return rand.Next(4) switch
            {
                0 => new Point(topLeft.X + rand.Next(size.X), topLeft.Y),//顶部一条
                1 => new Point(topLeft.X, topLeft.Y + rand.Next(size.Y)),//左边一条
                2 => new Point(topLeft.X + size.X, topLeft.Y + rand.Next(size.Y)),//右边一条
                _ => new Point(topLeft.X + rand.Next(size.X), topLeft.Y + size.Y),//底部一条
            };
        }

        /// <summary>
        /// 获得一个物品，需要指定数量，还可以指定类型<br></br>
        /// 如果数量不足则会只拿出该拿的
        /// </summary>
        /// <param name="stack"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Item GetItem(this Chest chest, int stack, int? type = null)
        {
            Item[] items = chest.item;
            for (int i = 0; i < items.Length; i++)
            {
                Item item = items[i];
                if (item == null || item.IsAir)
                    continue;

                if (type != null)//有指定的类型
                {
                    if (item.type == type.Value)//正好对上了
                        return NewItem(stack, item);

                    continue;
                }

                return NewItem(stack, item);
            }

            return null;

            static Item NewItem(int stack, Item item)
            {
                if (item.stack > stack)//数量多，直接减少
                {
                    item.stack -= stack;

                    Item item1 = item.Clone();
                    item1.stack = stack;
                    return item1;
                }
                else//数量不够，全部返回，自身重置
                {
                    Item item1 = item.Clone();
                    item.TurnToAir();
                    return item1;
                }
            }
        }

        /// <summary>
        /// 能否放入一个物品
        /// </summary>
        /// <param name="chest"></param>
        /// <param name="itemType"></param>
        /// <param name="stack"></param>
        /// <returns></returns>
        public static bool CanAddItem(this Chest chest, int itemType, int stack)
        {
            for (int i = 0; i < chest.item.Length; i++)
            {
                Item item = chest.item[i];//有空物品或者容量足够就放入
                if (item == null)
                    continue;

                if (item.IsAir || item.type == itemType && item.stack < item.maxStack - stack)
                    return true;
            }

            return false;
        }

        public static void SaveBools(this TagCompound tag, bool[] bools, string name)
        {
            int length = bools.Length;
            int count = length / 8 + 1;

            int k = 0;

            for (int i = 0; i < count; i++)
            {
                BitsByte b = new BitsByte();
                for (int j = 0; j < 8; j++)
                {
                    b[j] = bools[k];
                    k++;
                    if (k > length - 1)
                    {
                        tag.Add(name + i.ToString(), (byte)b);
                        goto over;
                    }
                }

                tag.Add(name + i.ToString(), (byte)b);
            }

        over:
            ;
        }

        public static SlotId PlayPitched(string path, float volume, float pitch, Vector2? position = null)
        {
            if (Main.netMode==NetmodeID.Server)
                return SlotId.Invalid;

            var style = new SoundStyle($"{nameof(TheTwinsRework)}/Sounds/{path}")
            {
                Volume = volume,
                Pitch = pitch,
                MaxInstances = 0
            };

            return SoundEngine.PlaySound(style, position);
        }

        public static SlotId PlayPitched(SoundStyle style, Vector2? position = null, float? volume = null, float? pitch = null, float volumeAdjust = 0, float pitchAdjust = 0)
        {
            if (Main.netMode == NetmodeID.Server)
                return SlotId.Invalid;

            if (volume.HasValue)
                style.Volume = volume.Value;

            if (pitch.HasValue)
                style.Pitch = pitch.Value;

            style.Volume += volumeAdjust;
            style.Pitch += pitchAdjust;

            return SoundEngine.PlaySound(style, position);
        }

    }
}