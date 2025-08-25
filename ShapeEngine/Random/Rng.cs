using ShapeEngine.StaticLib;
using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.RectDef;

namespace ShapeEngine.Random;

/// <summary>
/// Provides random number generation utilities, including weighted selection, random values, and seeded randomness.
/// </summary>
public class Rng
{
    /// <summary>
    /// Singleton instance of the <see cref="Rng"/> class.
    /// </summary>
    public static readonly Rng Instance = new Rng();
    
    /// <summary>
    /// Gets the underlying <see cref="System.Random"/> instance.
    /// </summary>
    public System.Random Rand { get; private set; }
    
    /// <summary>
    /// Gets the seed used for random number generation.
    /// </summary>
    public int Seed { get; private set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Rng"/> class with a time-based seed.
    /// </summary>
    /// <remarks>
    /// Seed is generated according to the following formula:
    /// <code>
    /// Seed = DateTime.Now.Millisecond * Environment.TickCount;
    /// </code>
    /// </remarks>
    public Rng()
    {
        Seed = DateTime.Now.Millisecond * Environment.TickCount;
        Rand = new System.Random(Seed);
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Rng"/> class using the specified seed value.
    /// </summary>
    /// <param name="seed">
    /// A number used to calculate a starting value for the pseudo-random number sequence. If a negative number
    /// is specified, the absolute value of the number is used.
    /// </param>
    public Rng(int seed)
    {
        Rand = new System.Random(seed);
        Seed = seed;
    }

    /// <summary>
    /// Sets the random number generator to use the specified seed.
    /// </summary>
    /// <param name="seed">The seed for the random number generator.</param>
    public void SetSeed(int seed)
    {
        Rand = new(seed);
        Seed = seed;
    }

    #region Weighted
    /// <summary>
    /// Picks a random item from the given weighted items.
    /// </summary>
    /// <typeparam name="T">The type of the items.</typeparam>
    /// <param name="items">An array of weighted items.</param>
    /// <returns>A randomly selected item, or default if none.</returns>
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
    /// <summary>
    /// Picks a random item from the given items and weights.
    /// </summary>
    /// <typeparam name="T">The type of the items.</typeparam>
    /// <param name="items">An array of tuples containing items and their weights.</param>
    /// <returns>A randomly selected item, or default if none.</returns>
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
    /// <summary>
    /// Picks a random string from the given string-weight pairs.
    /// </summary>
    /// <param name="items">An array of tuples containing string IDs and their weights.</param>
    /// <returns>A randomly selected string, or empty string if none.</returns>
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
    /// <summary>
    /// Picks multiple random items from the given weighted items.
    /// </summary>
    /// <typeparam name="T">The type of the items.</typeparam>
    /// <param name="amount">The number of items to pick.</param>
    /// <param name="items">An array of weighted items.</param>
    /// <returns>A list of randomly selected items.</returns>
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
    /// <summary>
    /// Picks multiple random items from the given items and weights.
    /// </summary>
    /// <typeparam name="T">The type of the items.</typeparam>
    /// <param name="amount">The number of items to pick.</param>
    /// <param name="items">An array of tuples containing items and their weights.</param>
    /// <returns>A list of randomly selected items.</returns>
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
    /// <summary>
    /// Picks multiple random strings from the given string-weight pairs.
    /// </summary>
    /// <param name="amount">The number of strings to pick.</param>
    /// <param name="items">An array of tuples containing string IDs and their weights.</param>
    /// <returns>A list of randomly selected strings.</returns>
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
    /// <summary>
    /// Returns true with the given probability (chance).
    /// </summary>
    /// <param name="value">A float between 0 and 1 representing the chance.</param>
    /// <returns>True with the given probability, otherwise false.</returns>
    public bool Chance(float value) { return RandF() < value; }
    #endregion

    #region Angle
    /// <summary>
    /// Returns a random angle in radians between 0 and 2π.
    /// </summary>
    public float RandAngleRad() { return RandF(0f, 2f * ShapeMath.PI); }
    /// <summary>
    /// Returns a random angle in degrees between 0 and 359.
    /// </summary>
    public float RandAngleDeg() { return RandF(0f, 359f); }
    #endregion

    #region Direction
    /// <summary>
    /// Returns a random float direction, either -1.0 or 1.0.
    /// </summary>
    public float RandDirF() { return RandF() < 0.5f ? -1.0f : 1.0f; }
    /// <summary>
    /// Returns a random integer direction, either -1 or 1.
    /// </summary>
    public int RandDirI() { return RandF() < 0.5f ? -1 : 1; }
    #endregion

    #region Float
    /// <summary>
    /// Returns a random float between 0.0 and 1.0.
    /// </summary>
    public float RandF() { return Rand.NextSingle(); }
    /// <summary>
    /// Returns a random float between 0.0 and max (or max and 0.0 if max is negative).
    /// </summary>
    /// <param name="max">The maximum value.</param>
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
    /// <summary>
    /// Returns a random float between min and max.
    /// </summary>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
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
    /// <summary>
    /// Returns a non-negative random integer.
    /// </summary>
    public int RandI() { return Rand.Next(); }
    /// <summary>
    /// Returns a random integer between 0 and max (or max and 0 if max is negative).
    /// </summary>
    /// <param name="max">The maximum value.</param>
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
    /// <summary>
    /// Returns a random integer between min and max.
    /// </summary>
    /// <param name="min">The (inclusive) minimum value.</param>
    /// <param name="max">The (exclusive) maximum value.</param>
    /// <remarks> See <see cref="Random.Next(int, int)"/>.</remarks>
    public int RandI(int min, int max)
    {
        if (max == min) return max;
        if (max < min)
        {
            (max, min) = (min, max);
        }
        return Rand.Next(min, max);
    }
    #endregion

    #region Vector2
    /// <summary>
    /// Returns a random unit vector.
    /// </summary>
    public Vector2 RandVec2()
    {
        float a = RandF() * 2.0f * MathF.PI;
        return new Vector2(MathF.Cos(a), MathF.Sin(a));
    }
    /// <summary>
    /// Returns a random vector with length between 0 and max.
    /// </summary>
    /// <param name="max">The maximum length. Should be positive!</param>
    public Vector2 RandVec2(float max) { return RandVec2(0, max); }
    /// <summary>
    /// Returns a random vector with length between min and max.
    /// </summary>
    /// <param name="min">The minimum length. Should be positive! Should be smaller than max! </param>
    /// <param name="max">The maximum length. Should be positive! Should be bigger than min!</param>
    public Vector2 RandVec2(float min, float max) { return RandVec2() * RandF(min, max); }
    #endregion

    #region Size
    /// <summary>
    /// Returns a random size with width and height between 0.0 and 1.0.
    /// </summary>
    public Size RandSize() => new(RandF(), RandF());
    /// <summary>
    /// Returns a random size with width and height between 0.0 and max.
    /// </summary>
    /// <param name="max">The maximum value for width and height.</param>
    public Size RandSize(float max) => new(RandF(max), RandF(max));
    /// <summary>
    /// Returns a random size with width and height between min and max.
    /// </summary>
    /// <param name="min">The minimum value for width and height.</param>
    /// <param name="max">The maximum value for width and height.</param>
    public Size RandSize(float min, float max) => new(RandF(min, max), RandF(min, max));
    /// <summary>
    /// Returns a random size with width and height between 0.0 and the specified max size.
    /// </summary>
    /// <param name="max">The maximum size.</param>
    public Size RandSize(Size max) => new(RandF(max.Width), RandF(max.Height));
    /// <summary>
    /// Returns a random size with width and height between the specified min and max sizes.
    /// </summary>
    /// <param name="min">The minimum size.</param>
    /// <param name="max">The maximum size.</param>
    public Size RandSize(Size min, Size max) => new(RandF(min.Width, max.Width), RandF(min.Height, max.Height));
    #endregion
    
    #region Color
    /// <summary>
    /// Returns a color with a random red channel.
    /// </summary>
    /// <param name="colorRgba">The base color.</param>
    public ColorRgba RandColorRed(ColorRgba colorRgba) => colorRgba.SetRed((byte)RandI(0, 255));
    /// <summary>
    /// Returns a color with a random red channel up to the specified max.
    /// </summary>
    /// <param name="colorRgba">The base color.</param>
    /// <param name="max">The maximum red value.</param>
    public ColorRgba RandColorRed(ColorRgba colorRgba, int max) => colorRgba.SetRed((byte)RandI(0, max));
    /// <summary>
    /// Returns a color with a random red channel between min and max.
    /// </summary>
    /// <param name="colorRgba">The base color.</param>
    /// <param name="min">The minimum red value.</param>
    /// <param name="max">The maximum red value.</param>
    public ColorRgba RandColorRed(ColorRgba colorRgba, int min, int max) => colorRgba.SetRed((byte)RandI(min, max));
    /// <summary>
    /// Returns a color with a random green channel.
    /// </summary>
    /// <param name="colorRgba">The base color.</param>
    public ColorRgba RandColorGreen(ColorRgba colorRgba) => colorRgba.SetGreen((byte)RandI(0, 255));
    /// <summary>
    /// Returns a color with a random green channel up to the specified max.
    /// </summary>
    /// <param name="colorRgba">The base color.</param>
    /// <param name="max">The maximum green value.</param>
    public ColorRgba RandColorGreen(ColorRgba colorRgba, int max) => colorRgba.SetGreen((byte)RandI(0, max));
    /// <summary>
    /// Returns a color with a random green channel between min and max.
    /// </summary>
    /// <param name="colorRgba">The base color.</param>
    /// <param name="min">The minimum green value.</param>
    /// <param name="max">The maximum green value.</param>
    public ColorRgba RandColorGreen(ColorRgba colorRgba, int min, int max) => colorRgba.SetGreen((byte)RandI(min, max));
    /// <summary>
    /// Returns a color with a random blue channel.
    /// </summary>
    /// <param name="colorRgba">The base color.</param>
    public ColorRgba RandColorBlue(ColorRgba colorRgba) => colorRgba.SetBlue((byte)RandI(0, 255));
    /// <summary>
    /// Returns a color with a random blue channel up to the specified max.
    /// </summary>
    /// <param name="colorRgba">The base color.</param>
    /// <param name="max">The maximum blue value.</param>
    public ColorRgba RandColorBlue(ColorRgba colorRgba, int max) => colorRgba.SetBlue((byte)RandI(0, max));
    /// <summary>
    /// Returns a color with a random blue channel between min and max.
    /// </summary>
    /// <param name="colorRgba">The base color.</param>
    /// <param name="min">The minimum blue value.</param>
    /// <param name="max">The maximum blue value.</param>
    public ColorRgba RandColorBlue(ColorRgba colorRgba, int min, int max) => colorRgba.SetBlue((byte)RandI(min, max));
    /// <summary>
    /// Returns a color with a random alpha channel.
    /// </summary>
    /// <param name="colorRgba">The base color.</param>
    public ColorRgba RandColorAlpha(ColorRgba colorRgba)  => colorRgba.SetAlpha((byte)RandI(0, 255));
    /// <summary>
    /// Returns a color with a random alpha channel up to the specified max.
    /// </summary>
    /// <param name="colorRgba">The base color.</param>
    /// <param name="max">The maximum alpha value.</param>
    public ColorRgba RandColorAlpha(ColorRgba colorRgba, int max) => colorRgba.SetAlpha((byte)RandI(0, max));
    /// <summary>
    /// Returns a color with a random alpha channel between min and max.
    /// </summary>
    /// <param name="colorRgba">The base color.</param>
    /// <param name="min">The minimum alpha value.</param>
    /// <param name="max">The maximum alpha value.</param>
    public ColorRgba RandColorAlpha(ColorRgba colorRgba, int min, int max) => colorRgba.SetAlpha((byte)RandI(min, max));
    /// <summary>
    /// Returns a random color with all channels between 0 and 255.
    /// </summary>
    public ColorRgba RandColor() => RandColor(0, 255);
    /// <summary>
    /// Returns a random color with all channels between 0 and 255 and the specified alpha.
    /// </summary>
    /// <param name="alpha">The alpha value.</param>
    public ColorRgba RandColor(int alpha) => RandColor(0, 255, alpha); 
    /// <summary>
    /// Returns a random color with all channels between min and max, and optionally a specified alpha.
    /// </summary>
    /// <param name="min">The minimum value for each channel.</param>
    /// <param name="max">The maximum value for each channel.</param>
    /// <param name="alpha">The alpha value, or -1 for random alpha.</param>
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
    /// <summary>
    /// Returns a random point between two vectors.
    /// </summary>
    /// <param name="start">The start vector.</param>
    /// <param name="end">The end vector.</param>
    public Vector2 RandPoint(Vector2 start, Vector2 end)
    {
        return ShapeVec.Lerp(start, end, RandF());
    }
    /// <summary>
    /// Returns a random point offset from the origin by a random unit vector.
    /// </summary>
    /// <param name="origin">The origin vector.</param>
    public Vector2 RandPoint(Vector2 origin)
    {
        return origin + RandVec2();
    }
    /// <summary>
    /// Returns a random point offset from the origin by a random vector with length up to max.
    /// </summary>
    /// <param name="origin">The origin vector.</param>
    /// <param name="max">The maximum offset length.</param>
    public Vector2 RandPoint(Vector2 origin, float max)
    {
        return origin + RandVec2(max);
    }
    /// <summary>
    /// Returns a random point offset from the origin by a random vector with length between min and max.
    /// </summary>
    /// <param name="origin">The origin vector.</param>
    /// <param name="min">The minimum offset length.</param>
    /// <param name="max">The maximum offset length.</param>
    public Vector2 RandPoint(Vector2 origin, float min, float max)
    {
        return origin + RandVec2(min, max);
    }
    #endregion

    #region Rect
    /// <summary>
    /// Returns a random rectangle with random position and size, aligned by the specified anchor point.
    /// </summary>
    /// <param name="alignment">The anchor point for alignment.</param>
    public Rect RandRect(AnchorPoint alignment)
    {
        var pos = RandVec2();
        var size = RandSize();
        return new(pos, size, alignment);
    }
    /// <summary>
    /// Returns a random rectangle at the given origin with random position and size, aligned by the specified anchor point.
    /// </summary>
    /// <param name="origin">The origin vector.</param>
    /// <param name="alignment">The anchor point for alignment.</param>
    public Rect RandRect(Vector2 origin, AnchorPoint alignment)
    {
        var pos = RandVec2();
        var size = RandSize();
        return new(origin + pos, size, alignment);
    }
    /// <summary>
    /// Returns a random rectangle with position and size in the specified ranges, aligned by the specified anchor point.
    /// </summary>
    /// <param name="posMin">Minimum position value.</param>
    /// <param name="posMax">Maximum position value.</param>
    /// <param name="sizeMin">Minimum size value.</param>
    /// <param name="sizeMax">Maximum size value.</param>
    /// <param name="alignment">The anchor point for alignment.</param>
    public Rect RandRect(float posMin, float posMax, float sizeMin, float sizeMax, AnchorPoint alignment)
    {
        var pos = RandVec2(posMin, posMax);
        var size = RandSize(sizeMin, sizeMax);
        return new(pos, size, alignment);
    }
    /// <summary>
    /// Returns a random rectangle at the given origin with position and size in the specified ranges, aligned by the specified anchor point.
    /// </summary>
    /// <param name="origin">The origin vector.</param>
    /// <param name="posMin">Minimum position value.</param>
    /// <param name="posMax">Maximum position value.</param>
    /// <param name="sizeMin">Minimum size value.</param>
    /// <param name="sizeMax">Maximum size value.</param>
    /// <param name="alignment">The anchor point for alignment.</param>
    public Rect RandRect(Vector2 origin, float posMin, float posMax, float sizeMin, float sizeMax, AnchorPoint alignment)
    {
        var pos = RandVec2(posMin, posMax);
        var size = RandSize(sizeMin, sizeMax);
        return new(origin + pos, size, alignment);
    }
    #endregion

    #region Collections
    /// <summary>
    /// Returns a random element from the list, optionally removing it.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="list">The list to pick from.</param>
    /// <param name="pop">Whether to remove the element from the list.</param>
    /// <returns>A randomly selected element, or default if the list is empty.</returns>
    public T? RandCollection<T>(List<T> list, bool pop = false)
    {
        if (list.Count <= 0) return default;
        int index = RandI(0, list.Count);
        var t = list[index];
        if (pop) list.RemoveAt(index);
        return t;
    }
    /// <summary>
    /// Returns a list of random elements from the source list, optionally removing them.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="source">The source list.</param>
    /// <param name="amount">The number of elements to pick.</param>
    /// <param name="pop">Whether to remove the elements from the source list.</param>
    /// <returns>A list of randomly selected elements.</returns>
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
    /// <summary>
    /// Returns a random element from the array.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="array">The array to pick from.</param>
    /// <returns>A randomly selected element, or default if the array is empty.</returns>
    public T? RandCollection<T>(T[] array)
    {
        if (array.Length <= 0) return default;
        return array[RandI(0, array.Length)];
    }
    #endregion
}