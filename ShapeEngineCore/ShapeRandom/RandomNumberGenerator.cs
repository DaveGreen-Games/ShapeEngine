using Raylib_CsLo;
using ShapeLib;
using System.Numerics;

namespace ShapeRandom
{
    public class RandomNumberGenerator
    {
        private Random rand;

        /// <summary>Initializes a new instance of the <see cref="SRNG"/> class using a default seed value.</summary>
        public RandomNumberGenerator() { rand = new Random(); }


        /// <summary>Initializes a new instance of the Random class, using the specified seed value.</summary>
        /// <param name="seed">
        /// A number used to calculate a starting value for the pseudo-random number sequence. If a negative number
        /// is specified, the absolute value of the number is used.
        /// </param>
        public RandomNumberGenerator(int seed)
        {
            rand = new Random(seed);
        }
        public void SetSeed(int seed) { rand = new(seed); }
        public bool chance(float value) { return randF() < value; }
        public float randAngleRad() { return randF(0f, 2f * RayMath.PI); }
        public float randAngleDeg() { return randF(0f, 359f); }

        public float randDirF() { return randF() < 0.5f ? -1.0f : 1.0f; }
        public int randDirI() { return randF() < 0.5f ? -1 : 1; }

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

        public Vector2 randVec2()
        {
            float a = randF() * 2.0f * MathF.PI;
            return new Vector2(MathF.Cos(a), MathF.Sin(a));
        }
        public Vector2 randVec2(float max) { return randVec2(0, max); }
        public Vector2 randVec2(float min, float max) { return randVec2() * randF(min, max); }
        public Vector2 randVec2(Rectangle rect)
        {
            return new(randF(rect.x, rect.x + rect.width), randF(rect.y, rect.y + rect.height));
        }
        public Color randColor() { return randColor(0, 255); }
        public Color randColor(int alpha) { return randColor(0, 255, alpha); }
        public Color randColor(Color color)
        {
            color.a = (byte)randI(0, 255);
            return color;
        }
        public Color randColor(Color color, int max)
        {
            color.a = (byte)randI(0, max);
            return color;
        }
        public Color randColor(Color color, int min, int max)
        {
            color.a = (byte)randI(min, max);
            return color;
        }
        public Color randColor(int min, int max, int alpha = -1)
        {
            if (alpha < 0)
            {
                return new Color(randI(min, max), randI(min, max), randI(min, max), randI(min, max));
            }
            return new Color(randI(min, max), randI(min, max), randI(min, max), alpha);
        }


        public Vector2 randPoint(Rectangle rect)
        {
            float x = randF(rect.x, rect.x + rect.width);
            float y = randF(rect.y, rect.y + rect.height);
            return new(x, y);
        }
        public Vector2 randPoint(Vector2 start, Vector2 end)
        {
            return SVec.Lerp(start, end, randF());
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

        public Rectangle randRect(Vector2 alignement)
        {
            Vector2 pos = randVec2();
            Vector2 size = randVec2();
            return SRect.ConstructRect(pos, size, alignement);
        }
        public Rectangle randRect(Vector2 origin, Vector2 alignement)
        {
            Vector2 pos = randVec2();
            Vector2 size = randVec2();
            return SRect.ConstructRect(origin + pos, size, alignement);
        }
        public Rectangle randRect(float posMin, float posMax, float sizeMin, float sizeMax, Vector2 alignement)
        {
            Vector2 pos = randVec2(posMin, posMax);
            Vector2 size = randVec2(sizeMin, sizeMax);
            return SRect.ConstructRect(pos, size, alignement);
        }
        public Rectangle randRect(Vector2 origin, float posMin, float posMax, float sizeMin, float sizeMax, Vector2 alignement)
        {
            Vector2 pos = randVec2(posMin, posMax);
            Vector2 size = randVec2(sizeMin, sizeMax);
            return SRect.ConstructRect(origin + pos, size, alignement);
        }

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

    }

}
