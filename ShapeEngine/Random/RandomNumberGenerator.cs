using ShapeEngine.Core.Shapes;
using ShapeEngine.Lib;
using System.Numerics;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Random
{
    public class RandomNumberGenerator
    {
        private System.Random rand;

        /// <summary>Initializes a new instance of the <see cref="ShapeRandom"/> class using a default seed value.</summary>
        public RandomNumberGenerator() { rand = new System.Random(); }


        /// <summary>Initializes a new instance of the Random class, using the specified seed value.</summary>
        /// <param name="seed">
        /// A number used to calculate a starting value for the pseudo-random number sequence. If a negative number
        /// is specified, the absolute value of the number is used.
        /// </param>
        public RandomNumberGenerator(int seed)
        {
            rand = new System.Random(seed);
        }
        public void SetSeed(int seed) { rand = new(seed); }

        #region Weighted
        public T? PickRandomItem<T>(params WeightedItem<T>[] items)
        {
            int totalWeight = 0;
            foreach (var item in items)
            {
                totalWeight += item.weight;
            }

            int ticket = RandI(0, totalWeight);

            int curWeight = 0;
            foreach (var item in items)
            {
                curWeight += item.weight;
                if (ticket <= curWeight) return item.item;
            }

            return default(T);
        }
        public T? PickRandomItem<T>(params (T item, int weight)[] items)
        {
            int totalWeight = 0;
            foreach (var item in items)
            {
                totalWeight += item.weight;
            }

            int ticket = RandI(0, totalWeight);

            int curWeight = 0;
            foreach (var item in items)
            {
                curWeight += item.weight;
                if (ticket <= curWeight) return item.item;
            }

            return default(T);
        }
        public string PickRandomItem(params (string id, int weight)[] items)
        {
            int totalWeight = 0;
            foreach (var item in items)
            {
                totalWeight += item.weight;
            }

            int ticket = RandI(0, totalWeight);

            int curWeight = 0;
            foreach (var item in items)
            {
                curWeight += item.weight;
                if (ticket <= curWeight) return item.id;
            }

            return "";
        }
        
        public List<T> PickRandomItems<T>(int amount, params WeightedItem<T>[] items)
        {
            List<T> chosen = new();
            int totalWeight = 0;
            foreach (var item in items)
            {
                totalWeight += item.weight;
            }

            for (int i = 0; i < amount; i++)
            {
                int ticket = RandI(0, totalWeight);

                int curWeight = 0;
                foreach (var item in items)
                {
                    curWeight += item.weight;
                    if (ticket <= curWeight) 
                    { 
                        chosen.Add(item.item);
                        break;
                    }
                }
            }
            return chosen;
        }
        public List<T> PickRandomItems<T>(int amount, params (T item, int weight)[] items)
        {
            List<T> chosen = new();
            int totalWeight = 0;
            foreach (var item in items)
            {
                totalWeight += item.weight;
            }


            for (int i = 0; i < amount; i++)
            {
                int ticket = RandI(0, totalWeight);

                int curWeight = 0;
                foreach (var item in items)
                {
                    curWeight += item.weight;
                    if (ticket <= curWeight)
                    {
                        chosen.Add(item.item);
                        break;
                    }
                }
            }

            return chosen;
        }
        public List<string> PickRandomItems(int amount, params (string id, int weight)[] items)
        {
            List<string> chosen = new();
            int totalWeight = 0;
            foreach (var item in items)
            {
                totalWeight += item.weight;
            }


            for (int i = 0; i < amount; i++)
            {
                int ticket = RandI(0, totalWeight);

                int curWeight = 0;
                foreach (var item in items)
                {
                    curWeight += item.weight;
                    if (ticket <= curWeight)
                    {
                        chosen.Add(item.id);
                        break;
                    }
                }
            }

            return chosen;
        }
        #endregion

        #region Chance
        public bool Chance(float value) { return RandF() < value; }
        #endregion

        #region Angle
        public float RandAngleRad() { return RandF(0f, 2f * ShapeMath.PI); }
        public float RandAngleDeg() { return RandF(0f, 359f); }
        #endregion

        #region Direction
        public float RandDirF() { return RandF() < 0.5f ? -1.0f : 1.0f; }
        public int RandDirI() { return RandF() < 0.5f ? -1 : 1; }
        #endregion

        #region Float
        public float RandF() { return rand.NextSingle(); }
        public float RandF(float max)
        {
            if (max < 0.0f)
            {
                return RandF(max, 0.0f);
            }
            else if (max > 0.0f)
            {
                return RandF(0.0f, max);
            }
            else return 0.0f;
        }
        public float RandF(float min, float max)
        {
            if (max == min) return max;
            else if (max < min)
            {
                float temp = max;
                max = min;
                min = temp;
            }
            return min + (float)rand.NextDouble() * (max - min);
        }
        #endregion

        #region Int
        public int RandI() { return rand.Next(); }
        public int RandI(int max)
        {
            if (max < 0)
            {
                return RandI(max, 0);
            }
            else if (max > 0)
            {
                return RandI(0, max);
            }
            else return 0;
        }
        public int RandI(int min, int max)
        {
            if (max == min) return max;
            else if (max < min)
            {
                int temp = max;
                max = min;
                min = temp;
            }
            return rand.Next(min, max);
        }
        #endregion

        #region Vector2
        public Vector2 RandVec2()
        {
            float a = RandF() * 2.0f * MathF.PI;
            return new Vector2(MathF.Cos(a), MathF.Sin(a));
        }
        public Vector2 RandVec2(float max) { return RandVec2(0, max); }
        public Vector2 RandVec2(float min, float max) { return RandVec2() * RandF(min, max); }
        //public Vector2 randVec2(Rectangle rect)
        //{
        //    return new(randF(rect.x, rect.x + rect.width), randF(rect.y, rect.y + rect.height));
        //}
        #endregion

        #region Size

        public Size RandSize() => new(RandF(), RandF());
        public Size RandSize(float max) => new(RandF(max), RandF(max));
        public Size RandSize(float min, float max) => new(RandF(min, max), RandF(min, max));
        public Size RandSize(Size max) => new(RandF(max.Width), RandF(max.Height));
        public Size RandSize(Size min, Size max) => new(RandF(min.Width, max.Width), RandF(min.Height, max.Height));
        #endregion
        
        #region Color

        public ColorRgba RandColorRed(ColorRgba colorRgba) => colorRgba.SetRed((byte)RandI(0, 255));
        public ColorRgba RandColorRed(ColorRgba colorRgba, int max) => colorRgba.SetRed((byte)RandI(0, max));
        public ColorRgba RandColorRed(ColorRgba colorRgba, int min, int max) => colorRgba.SetRed((byte)RandI(min, max));
        
        public ColorRgba RandColorGreen(ColorRgba colorRgba) => colorRgba.SetGreen((byte)RandI(0, 255));
        public ColorRgba RandColorGreen(ColorRgba colorRgba, int max) => colorRgba.SetGreen((byte)RandI(0, max));
        public ColorRgba RandColorGreen(ColorRgba colorRgba, int min, int max) => colorRgba.SetGreen((byte)RandI(min, max));
        
        public ColorRgba RandColorBlue(ColorRgba colorRgba) => colorRgba.SetBlue((byte)RandI(0, 255));
        public ColorRgba RandColorBlue(ColorRgba colorRgba, int max) => colorRgba.SetBlue((byte)RandI(0, max));
        public ColorRgba RandColorBlue(ColorRgba colorRgba, int min, int max) => colorRgba.SetBlue((byte)RandI(min, max));
        
        public ColorRgba RandColorAlpha(ColorRgba colorRgba)  => colorRgba.SetAlpha((byte)RandI(0, 255));
        public ColorRgba RandColorAlpha(ColorRgba colorRgba, int max) => colorRgba.SetAlpha((byte)RandI(0, max));
        public ColorRgba RandColorAlpha(ColorRgba colorRgba, int min, int max) => colorRgba.SetAlpha((byte)RandI(min, max));
        
        public ColorRgba RandColor() => RandColor(0, 255);
        public ColorRgba RandColor(int alpha) => RandColor(0, 255, alpha); 
        public ColorRgba RandColor(int min, int max, int alpha = -1)
        {
            if (alpha < 0)
            {
                return new ColorRgba(RandI(min, max), RandI(min, max), RandI(min, max), RandI(min, max));
            }
            return new ColorRgba(RandI(min, max), RandI(min, max), RandI(min, max), alpha);
        }
        #endregion

        #region Point
        public Vector2 RandPoint(Vector2 start, Vector2 end)
        {
            return ShapeVec.Lerp(start, end, RandF());
        }
        public Vector2 RandPoint(Vector2 origin)
        {
            return origin + RandVec2();
        }
        public Vector2 RandPoint(Vector2 origin, float max)
        {
            return origin + RandVec2(max);
        }
        public Vector2 RandPoint(Vector2 origin, float min, float max)
        {
            return origin + RandVec2(min, max);
        }
        #endregion

        #region Rect
        public Rect RandRect(Vector2 alignement)
        {
            var pos = RandVec2();
            var size = RandSize();
            return new(pos, size, alignement);
        }
        public Rect RandRect(Vector2 origin, Vector2 alignement)
        {
            var pos = RandVec2();
            var size = RandSize();
            return new(origin + pos, size, alignement);
        }
        public Rect RandRect(float posMin, float posMax, float sizeMin, float sizeMax, Vector2 alignement)
        {
            var pos = RandVec2(posMin, posMax);
            var size = RandSize(sizeMin, sizeMax);
            return new(pos, size, alignement);
        }
        public Rect RandRect(Vector2 origin, float posMin, float posMax, float sizeMin, float sizeMax, Vector2 alignement)
        {
            var pos = RandVec2(posMin, posMax);
            var size = RandSize(sizeMin, sizeMax);
            return new(origin + pos, size, alignement);
        }
        #endregion

        #region Collections
        public T? RandCollection<T>(List<T> list, bool pop = false)
        {
            if (list == null || list.Count <= 0) return default;
            int index = RandI(0, list.Count);
            T t = list[index];
            if (pop) list.RemoveAt(index);
            return t;
        }
        public List<T> RandCollection<T>(List<T> source, int amount, bool pop = false)
        {
            if (source == null || source.Count <= 0 || amount <= 0) return new List<T>();
            if (pop) amount = Math.Min(amount, source.Count);
            List<T> list = new List<T>();
            for (int i = 0; i < amount; i++)
            {
                int index = RandI(0, source.Count);
                T element = source[index];
                list.Add(element);
                if (pop) source.RemoveAt(index);
            }
            return list;

        }
        public T? RandCollection<T>(T[] array)
        {
            if (array == null || array.Length <= 0) return default;
            return array[RandI(0, array.Length)];
        }
        #endregion
    }

}
