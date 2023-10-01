using Raylib_CsLo;
using ShapeEngine.Core;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Lib;
using System.Numerics;

namespace ShapeEngine.Random
{
    public struct WeightedItem <T>
    {
        public T item { get; set; }
        public int weight { get; set; }
        public WeightedItem(T item,  int weight)
        {
            this.item = item;
            this.weight = ShapeMath.AbsInt(weight);
        }
    }


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

            int ticket = randI(0, totalWeight);

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

            int ticket = randI(0, totalWeight);

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

            int ticket = randI(0, totalWeight);

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
                int ticket = randI(0, totalWeight);

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
                int ticket = randI(0, totalWeight);

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
                int ticket = randI(0, totalWeight);

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
        public bool chance(float value) { return randF() < value; }
        #endregion

        #region Angle
        public float randAngleRad() { return randF(0f, 2f * RayMath.PI); }
        public float randAngleDeg() { return randF(0f, 359f); }
        #endregion

        #region Direction
        public float randDirF() { return randF() < 0.5f ? -1.0f : 1.0f; }
        public int randDirI() { return randF() < 0.5f ? -1 : 1; }
        #endregion

        #region Float
        public float randF() { return rand.NextSingle(); }
        public float randF(float max)
        {
            if (max < 0.0f)
            {
                return randF(max, 0.0f);
            }
            else if (max > 0.0f)
            {
                return randF(0.0f, max);
            }
            else return 0.0f;
        }
        public float randF(float min, float max)
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
        public int randI() { return rand.Next(); }
        public int randI(int max)
        {
            if (max < 0)
            {
                return randI(max, 0);
            }
            else if (max > 0)
            {
                return randI(0, max);
            }
            else return 0;
        }
        public int randI(int min, int max)
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
        public Vector2 randVec2()
        {
            float a = randF() * 2.0f * MathF.PI;
            return new Vector2(MathF.Cos(a), MathF.Sin(a));
        }
        public Vector2 randVec2(float max) { return randVec2(0, max); }
        public Vector2 randVec2(float min, float max) { return randVec2() * randF(min, max); }
        //public Vector2 randVec2(Rectangle rect)
        //{
        //    return new(randF(rect.x, rect.x + rect.width), randF(rect.y, rect.y + rect.height));
        //}
        #endregion

        #region Color
        public Raylib_CsLo.Color randColor() { return randColor(0, 255); }
        public Raylib_CsLo.Color randColor(int alpha) { return randColor(0, 255, alpha); }
        public Raylib_CsLo.Color randColor(Raylib_CsLo.Color color)
        {
            color.a = (byte)randI(0, 255);
            return color;
        }
        public Raylib_CsLo.Color randColor(Raylib_CsLo.Color color, int max)
        {
            color.a = (byte)randI(0, max);
            return color;
        }
        public Raylib_CsLo.Color randColor(Raylib_CsLo.Color color, int min, int max)
        {
            color.a = (byte)randI(min, max);
            return color;
        }
        public Raylib_CsLo.Color randColor(int min, int max, int alpha = -1)
        {
            if (alpha < 0)
            {
                return new Raylib_CsLo.Color(randI(min, max), randI(min, max), randI(min, max), randI(min, max));
            }
            return new Raylib_CsLo.Color(randI(min, max), randI(min, max), randI(min, max), alpha);
        }
        #endregion

        #region Point
        public Vector2 randPoint(Vector2 start, Vector2 end)
        {
            return ShapeVec.Lerp(start, end, randF());
        }
        public Vector2 randPoint(Vector2 origin)
        {
            return origin + randVec2();
        }
        public Vector2 randPoint(Vector2 origin, float max)
        {
            return origin + randVec2(max);
        }
        public Vector2 randPoint(Vector2 origin, float min, float max)
        {
            return origin + randVec2(min, max);
        }
        #endregion

        #region Rect
        public Rect randRect(Vector2 alignement)
        {
            Vector2 pos = randVec2();
            Vector2 size = randVec2();
            return new(pos, size, alignement);
        }
        public Rect randRect(Vector2 origin, Vector2 alignement)
        {
            Vector2 pos = randVec2();
            Vector2 size = randVec2();
            return new(origin + pos, size, alignement);
        }
        public Rect randRect(float posMin, float posMax, float sizeMin, float sizeMax, Vector2 alignement)
        {
            Vector2 pos = randVec2(posMin, posMax);
            Vector2 size = randVec2(sizeMin, sizeMax);
            return new(pos, size, alignement);
        }
        public Rect randRect(Vector2 origin, float posMin, float posMax, float sizeMin, float sizeMax, Vector2 alignement)
        {
            Vector2 pos = randVec2(posMin, posMax);
            Vector2 size = randVec2(sizeMin, sizeMax);
            return new(origin + pos, size, alignement);
        }
        #endregion

        #region Collections
        public T? randCollection<T>(List<T> list, bool pop = false)
        {
            if (list == null || list.Count <= 0) return default;
            int index = randI(0, list.Count);
            T t = list[index];
            if (pop) list.RemoveAt(index);
            return t;
        }
        public List<T> randCollection<T>(List<T> source, int amount, bool pop = false)
        {
            if (source == null || source.Count <= 0 || amount <= 0) return new List<T>();
            if (pop) amount = Math.Min(amount, source.Count);
            List<T> list = new List<T>();
            for (int i = 0; i < amount; i++)
            {
                int index = randI(0, source.Count);
                T element = source[index];
                list.Add(element);
                if (pop) source.RemoveAt(index);
            }
            return list;

        }
        public T? randCollection<T>(T[] array)
        {
            if (array == null || array.Length <= 0) return default;
            return array[randI(0, array.Length)];
        }
        #endregion
    }

}
