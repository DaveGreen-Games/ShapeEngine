using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Raylib_CsLo;

namespace ShapeEngineCore.Globals
{

    public class ChanceList<T>
    {
        private List<T> values = new();
        private (int amount, T value)[] entries;

        private Rng rng;

        public ChanceList(params (int amount, T value)[] entries)
        {
            this.rng = new Rng();
            this.entries = entries;
            Generate();
        }
        public ChanceList(int seed, params (int amount, T value)[] entries)
        {
            this.rng = new Rng(seed);
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
            int index = rng.randI(0, filtered.Count);
            T value = filtered[index];
            //filtered.RemoveAt(index);
            return value;
        }

        public List<T> Next(int min, int max)
        {
            return Next(rng.randI(min, max));
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

            int index = rng.randI(0, values.Count);
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

    public interface RandomNumberGenerator
    {
        public void SetSeed(int seed);
        public bool chance(float value);
        public float randAngleRad();
        public float randAngleDeg();

        public float randDirF();
        public int randDirI();

        public float randF();
        public float randF(float max);
        public float randF(float min, float max);

        public int randI() ;
        public int randI(int max);
        public int randI(int min, int max);

        public Vector2 randVec2();
        public Vector2 randVec2(float max) ;
        public Vector2 randVec2(float min, float max) ;
        public Vector2 randVec2(Rectangle rect);
        public Color randColor() ;
        public Color randColor(int alpha);
        public Color randColor(Color color);
        public Color randColor(Color color, int max);   
        public Color randColor(Color color, int min, int max);
        public Color randColor(int min, int max, int alpha = -1);


        public Vector2 randPoint(Rectangle rect);
        public Vector2 randPoint(Vector2 start, Vector2 end);
        public Vector2 randPoint(Vector2 origin);
        public Vector2 randPoint(Vector2 origin, float max);
        public Vector2 randPoint(Vector2 origin, float min, float max);

        public T? randCollection<T>(List<T> list, bool pop = false);
        public List<T> randCollection<T>(List<T> source, int amount, bool pop = false);
        public T? randCollection<T>(T[] array);

    }

    /*
    public class LehmerRng : RandomNumberGenerator
    {
        private const int a = 16807;
        private const int m = 2147483647;
        private const int q = 127773;
        private const int r = 2836;
        private int seed;
        public LehmerRng(int seed)
        {
            if (seed <= 0 || seed == int.MaxValue)
                throw new Exception("Bad seed");
            this.seed = seed;
        }
        public void SetSeed(int seed) { this.seed = seed; }

        public double NextDouble()
        {
            int hi = seed / q;
            int lo = seed % q;
            seed = (a * lo) - (r * hi);
            if (seed <= 0)
                seed = seed + m;
            return (seed * 1.0) / m;
        }
        public float NextSingle() { return (float)NextDouble(); }
        public int Next() { return (int)NextDouble(); }
    }
    public class WichmannRng
    {
        private int s1 = 1;
        private int s2 = 1;
        private int s3 = 1;
        public WichmannRng(int seed)
        {
            if (seed <= 0 || seed > 30000)
                throw new Exception("Bad seed");
            s1 = seed;
            s2 = seed + 1;
            s3 = seed + 2;
        }

        public double NextDouble()
        {
            s1 = 171 * (s1 % 177) - 2 * (s1 / 177);
            if (s1 < 0) { s1 += 30269; }
            s2 = 172 * (s2 % 176) - 35 * (s2 / 176);
            if (s2 < 0) { s2 += 30307; }
            s3 = 170 * (s3 % 178) - 63 * (s3 / 178);
            if (s3 < 0) { s3 += 30323; }
            double r = (s1 * 1.0) / 30269 + (s2 * 1.0) / 30307 + (s3 * 1.0) / 30323;
            return r - Math.Truncate(r);  // orig uses % 1.0
        }

        public float NextSingle() { return (float)NextDouble(); }

        public int Next() { return (int) NextDouble(); }
    }
    public class LinearConRng
    {
        private const long a = 25214903917;
        private const long c = 11;
        private long seed;
        public LinearConRng(long seed)
        {
            if (seed < 0)
                throw new Exception("Bad seed");
            this.seed = seed;
        }
        private int next(int bits) // helper
        {
            seed = (seed * a + c) & ((1L << 48) - 1);
            return (int)(seed >> (48 - bits));
        }
        public double NextDouble() { return (((long)next(26) << 27) + next(27)) / (double)(1L << 53); }

        public float NextSingle() { return (float)NextDouble(); }

        public int Next() { return (int)NextDouble(); }
    }
    */
    
    public class Rng : RandomNumberGenerator
    {
        private Random rand;

        /// <summary>Initializes a new instance of the <see cref="Rng"/> class using a default seed value.</summary>
        public Rng() { rand = new Random(); }
        
        
        /// <summary>Initializes a new instance of the Random class, using the specified seed value.</summary>
        /// <param name="seed">
        /// A number used to calculate a starting value for the pseudo-random number sequence. If a negative number
        /// is specified, the absolute value of the number is used.
        /// </param>
        public Rng(int seed)
        {
            rand = new Random(seed);
        }
        public void SetSeed(int seed) { rand = new(seed); }
        public bool chance(float value) { return randF() < value;}
        public float randAngleRad() { return randF(0f, 2f * PI); }
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
            return Vec.Lerp(start, end, randF());
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

    /*
    public class RngBase : RandomNumberGenerator
    {
        public virtual double NextDouble() { return 0; }
        public virtual float NextSingle() { return (float)NextDouble(); }
        public virtual int Next() { return (int)NextDouble(); }

        public void SetSeed(int seed) {  }
        public bool chance(float value) { return randF() < value; }
        public float randAngleRad() { return randF(0f, 2f * PI); }
        public float randAngleDeg() { return randF(0f, 359f); }

        public float randDirF() { return randF() < 0.5f ? -1.0f : 1.0f; }
        public int randDirI() { return randF() < 0.5f ? -1 : 1; }

        public float randF() { return NextSingle(); }
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

        public int randI() { return Next(); }
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
            return Next(min, max);
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
            return Vec.Lerp(start, end, randF());
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
    */

    public static class RNG
    {
        //private static Random rand = new Random();
        private static RandomNumberGenerator rand = new Rng();

        public static void SetNumberGenerator(RandomNumberGenerator rng) { rand = rng; }
        public static void SetSeed(int seed) { rand.SetSeed(seed); }

        public static bool chance(float value) { return rand.chance(value); }
        public static float randAngleRad() { return rand.randAngleRad(); }
        public static float randAngleDeg() { return rand.randAngleDeg(); }

        public static float randDirF() { return rand.randDirF(); }
        public static int randDirI() { return rand.randDirI(); }

        public static float randF() { return rand.randF(); }
        public static float randF(float max) { return rand.randF(max); }
        public static float randF(float min, float max) { return rand.randF(min, max); }

        public static int randI() { return rand.randI(); }
        public static int randI(int max) { return rand.randI(max); }
        public static int randI(int min, int max) { return rand.randI(min, max); }

        public static Vector2 randVec2() { return rand.randVec2(); }
        public static Vector2 randVec2(float max) { return rand.randVec2(max); }
        public static Vector2 randVec2(float min, float max) { return rand.randVec2(min, max); }
        public static Vector2 randVec2(Rectangle rect) { return rand.randVec2(rect); }
        public static Color randColor() { return rand.randColor(); }
        public static Color randColor(int alpha) { return rand.randColor(alpha); }
        public static Color randColor(Color color) { return rand.randColor(color); }
        public static Color randColor(Color color, int max) { return rand.randColor(color, max); }
        public static Color randColor(Color color, int min, int max) { return rand.randColor(color, min, max); }
        public static Color randColor(int min, int max, int alpha = -1) { return rand.randColor(min, max, alpha); }


        public static Vector2 randPoint(Rectangle rect) { return rand.randPoint(rect); }
        public static Vector2 randPoint(Vector2 start, Vector2 end) { return rand.randPoint(start, end); }
        public static Vector2 randPoint(Vector2 origin) { return rand.randPoint(origin); }
        public static Vector2 randPoint(Vector2 origin, float max) { return rand.randPoint(origin, max); }
        public static Vector2 randPoint(Vector2 origin, float min, float max) { return rand.randPoint(origin, min, max); }

        public static T? randCollection<T>(List<T> list, bool pop = false) { return rand.randCollection<T>(list, pop); }
        public static List<T> randCollection<T>(List<T> source, int amount, bool pop = false) { return rand.randCollection<T>(source, amount, pop); }
        public static T? randCollection<T>(T[] array) { return rand.randCollection(array); }

    }
}
