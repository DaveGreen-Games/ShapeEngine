using System.Numerics;
using Raylib_CsLo;

namespace ShapeEngineCore.Globals
{

    public class ChanceList<T>
    {
        private List<T> values = new();
        private (int amount, T value)[] entries;

        public ChanceList(params (int amount, T value)[] entries)
        {
            this.entries = entries;
            Generate();
        }

        public void Refill()
        {
            values.Clear();
            Generate();
        }

        public T? Next(Predicate<T> match)
        {
            if (values.Count <= 0) Generate();
            List<T> filtered = values.FindAll(match);
            if (filtered.Count <= 0) return default;
            int index = RNG.randI(0, filtered.Count);
            T value = filtered[index];
            //filtered.RemoveAt(index);
            return value;
        }

        public List<T> Next(int min, int max)
        {
            return Next(RNG.randI(min, max));
        }
        public List<T> Next(int amount)
        {
            List<T> picked = new();
            for (int i = 0; i < amount; i++)
            {
                picked.Add(Next());
            }
            return picked;
        }
        public T Next()
        {
            if (values.Count <= 0) Generate();

            int index = RNG.randI(0, values.Count);
            T value = values[index];
            values.RemoveAt(index);
            return value;
        }
        private void Generate()
        {
            foreach (var entry in entries)
            {
                for (int i = 0; i < entry.amount; i++)
                {
                    values.Add(entry.value);
                }
            }
        }
    }

    //public class RandomRectangleArea
    //{
    //
    //}

    public static class RNG
    {
        private static Random rand = new Random();

        public static bool chance(float value) { return randF() < value; }
        public static float randAngleRad() { return randF(0f, 2f * PI); }
        public static float randAngleDeg() { return randF(0f, 359f); }

        public static float randDirF() { return randF() < 0.5f ? -1.0f : 1.0f; }
        public static int randDirI() { return randF() < 0.5f ? -1 : 1; }

        public static float randF() { return rand.NextSingle(); }
        public static float randF(float max)
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
        public static float randF(float min, float max)
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

        public static int randI() { return rand.Next(); }
        public static int randI(int max)
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
        public static int randI(int min, int max)
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

        public static Vector2 randVec2()
        {
            float a = randF() * 2.0f * MathF.PI;
            return new Vector2(MathF.Cos(a), MathF.Sin(a));
        }
        public static Vector2 randVec2(float max) { return randVec2(0, max); }
        public static Vector2 randVec2(float min, float max) { return randVec2() * randF(min, max); }
        public static Vector2 randVec2(Rectangle rect)
        {
            return new(randF(rect.x, rect.x + rect.width), randF(rect.y, rect.y + rect.height));
        }
        public static Color randColor() { return randColor(0, 255); }
        public static Color randColor(int alpha) { return randColor(0, 255, alpha); }
        public static Color randColor(Color color)
        {
            color.a = (byte)randI(0, 255);
            return color;
        }
        public static Color randColor(Color color, int max)
        {
            color.a = (byte)randI(0, max);
            return color;
        }
        public static Color randColor(Color color, int min, int max)
        {
            color.a = (byte)randI(min, max);
            return color;
        }
        public static Color randColor(int min, int max, int alpha = -1)
        {
            if (alpha < 0)
            {
                return new Color(randI(min, max), randI(min, max), randI(min, max), randI(min, max));
            }
            return new Color(randI(min, max), randI(min, max), randI(min, max), alpha);
        }


        public static Vector2 randPoint(Rectangle rect)
        {
            float x = randF(rect.x, rect.x + rect.width);
            float y = randF(rect.y, rect.y + rect.height);
            return new(x, y);
        }
        public static Vector2 randPoint(Vector2 start, Vector2 end)
        {
            return Vec.Lerp(start, end, randF());
        }
        public static Vector2 randPoint(Vector2 origin)
        {
            return origin + randVec2();
        }
        public static Vector2 randPoint(Vector2 origin, float max)
        {
            return origin + randVec2(max);
        }
        public static Vector2 randPoint(Vector2 origin, float min, float max)
        {
            return origin + randVec2(min, max);
        }

        public static T? randCollection<T>(List<T> list, bool pop = false)
        {
            if (list == null || list.Count <= 0) return default;
            int index = randI(0, list.Count);
            T t = list[index];
            if (pop) list.RemoveAt(index);
            return t;
        }
        public static List<T> randCollection<T>(List<T> source, int amount, bool pop = false)
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
        public static T? randCollection<T>(T[] array)
        {
            if (array == null || array.Length <= 0) return default;
            return array[randI(0, array.Length)];
        }
    }
}
