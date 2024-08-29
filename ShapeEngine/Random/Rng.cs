using ShapeEngine.Core.Shapes;
using ShapeEngine.Lib;
using System.Numerics;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Random;

public class Rng
{
    public static readonly Rng Instance = new Rng();

    public System.Random Rand { get; private set; }
    public int Seed { get; private set; }

    public Rng()
    {
        Seed = DateTime.Now.Millisecond * Environment.TickCount;
        Rand = new System.Random(Seed);
    }

    /// <summary>Initializes a new instance of the Random class, using the specified seed value.</summary>
    /// <param name="seed">
    /// A number used to calculate a starting value for the pseudo-random number sequence. If a negative number
    /// is specified, the absolute value of the number is used.
    /// </param>
    public Rng(int seed)
    {
        Rand = new System.Random(seed);
        Seed = seed;
    }

    public void SetSeed(int seed)
    {
        Rand = new(seed);
        Seed = seed;
    }

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
    public float RandF() { return Rand.NextSingle(); }
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
        if (Math.Abs(max - min) < 0.0001f) return max;
        else if (max < min)
        {
            (max, min) = (min, max);
        }
        return min + (float)Rand.NextDouble() * (max - min);
    }
    #endregion

    #region Int
    public int RandI() { return Rand.Next(); }
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
            (max, min) = (min, max);
        }
        return Rand.Next(min, max);
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
    public Rect RandRect(AnchorPoint alignement)
    {
        var pos = RandVec2();
        var size = RandSize();
        return new(pos, size, alignement);
    }
    public Rect RandRect(Vector2 origin, AnchorPoint alignement)
    {
        var pos = RandVec2();
        var size = RandSize();
        return new(origin + pos, size, alignement);
    }
    public Rect RandRect(float posMin, float posMax, float sizeMin, float sizeMax, AnchorPoint alignement)
    {
        var pos = RandVec2(posMin, posMax);
        var size = RandSize(sizeMin, sizeMax);
        return new(pos, size, alignement);
    }
    public Rect RandRect(Vector2 origin, float posMin, float posMax, float sizeMin, float sizeMax, AnchorPoint alignement)
    {
        var pos = RandVec2(posMin, posMax);
        var size = RandSize(sizeMin, sizeMax);
        return new(origin + pos, size, alignement);
    }
    #endregion

    #region Collections
    
    public T? RandCollection<T>(List<T> list, bool pop = false)
    {
        if (list.Count <= 0) return default;
        int index = RandI(0, list.Count);
        var t = list[index];
        if (pop) list.RemoveAt(index);
        return t;
    }
    public List<T> RandCollection<T>(List<T> source, int amount, bool pop = false)
    {
        if (source.Count <= 0 || amount <= 0) return [];
        if (pop) amount = Math.Min(amount, source.Count);
        var list = new List<T>();
        for (var i = 0; i < amount; i++)
        {
            int index = RandI(0, source.Count);
            var element = source[index];
            list.Add(element);
            if (pop) source.RemoveAt(index);
        }
        return list;

    }
    public T? RandCollection<T>(T[] array)
    {
        if (array.Length <= 0) return default;
        return array[RandI(0, array.Length)];
    }
    #endregion
}


/* Implementing custom rng
using System;
   
   namespace ConsoleApplication1
   {
       public class ConsistantRandom: Random
       {
           private const int MBIG = Int32.MaxValue;
           private const int MSEED = 161803398;
           private const int MZ = 0;
   
           private int inext;
           private int inextp;
           private int[] SeedArray = new int[56];
   
           public ConsistantRandom()
               : this(Environment.TickCount)
           {
           }
   
           public ConsistantRandom(int seed)
           {
               int ii;
               int mj, mk;
   
               int subtraction = (seed == Int32.MinValue) ? Int32.MaxValue : Math.Abs(seed);
               mj = MSEED - subtraction;
               SeedArray[55] = mj;
               mk = 1;
               for (int i = 1; i < 55; i++)
               {
                   ii = (21 * i) % 55;
                   SeedArray[ii] = mk;
                   mk = mj - mk;
                   if (mk < 0) mk += MBIG;
                   mj = SeedArray[ii];
               }
               for (int k = 1; k < 5; k++)
               {
                   for (int i = 1; i < 56; i++)
                   {
                       SeedArray[i] -= SeedArray[1 + (i + 30) % 55];
                       if (SeedArray[i] < 0) SeedArray[i] += MBIG;
                   }
               }
               inext = 0;
               inextp = 21;
           }
           protected override double Sample()
           {
               return (InternalSample() * (1.0 / MBIG));
           }
   
           private int InternalSample()
           {
               int retVal;
               int locINext = inext;
               int locINextp = inextp;
   
               if (++locINext >= 56) locINext = 1;
               if (++locINextp >= 56) locINextp = 1;
   
               retVal = SeedArray[locINext] - SeedArray[locINextp];
   
               if (retVal == MBIG) retVal--;
               if (retVal < 0) retVal += MBIG;
   
               SeedArray[locINext] = retVal;
   
               inext = locINext;
               inextp = locINextp;
   
               return retVal;
           }
   
           public override int Next()
           {
               return InternalSample();
           }
   
           private double GetSampleForLargeRange()
           {
               int result = InternalSample();
               bool negative = (InternalSample() % 2 == 0) ? true : false;
               if (negative)
               {
                   result = -result;
               }
               double d = result;
               d += (Int32.MaxValue - 1);
               d /= 2 * (uint)Int32.MaxValue - 1;
               return d;
           }
   
   
           public override int Next(int minValue, int maxValue)
           {
               if (minValue > maxValue)
               {
                   throw new ArgumentOutOfRangeException("minValue");
               }
   
               long range = (long)maxValue - minValue;
               if (range <= (long)Int32.MaxValue)
               {
                   return ((int)(Sample() * range) + minValue);
               }
               else
               {
                   return (int)((long)(GetSampleForLargeRange() * range) + minValue);
               }
           }
           public override void NextBytes(byte[] buffer)
           {
               if (buffer == null) throw new ArgumentNullException("buffer");
               for (int i = 0; i < buffer.Length; i++)
               {
                   buffer[i] = (byte)(InternalSample() % (Byte.MaxValue + 1));
               }
           }
       }
 */