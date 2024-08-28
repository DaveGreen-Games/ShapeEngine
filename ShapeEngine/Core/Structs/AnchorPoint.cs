using System.Numerics;
using ShapeEngine.Random;

namespace ShapeEngine.Core.Structs;

/// <summary>
/// Used for ui anchor points and rect alignments. 0, 0 is top left, 1, 1 is bottom right.
/// </summary>
/// <param name="x">Range 0 - 1. 0 is left, 0.5 is center, 1 is right.</param>
/// <param name="y">Range 0 - 1. 0 is top, 0.5 is center, 1 is bottom.</param>
public readonly struct AnchorPoint(float x, float y) : IEquatable<AnchorPoint>
{
    public static AnchorPoint Zero = new(0, 0);
    public static AnchorPoint One = new(1, 1);
    
    public static AnchorPoint TopLeft = new(0, 0);
    public static AnchorPoint TopCenter = new(0.5f, 0);
    public static AnchorPoint TopRight = new(1, 0);
    
    public static AnchorPoint Left = new(0, 0.5f);
    public static AnchorPoint Center = new(0.5f, 0.5f);
    public static AnchorPoint Right = new(1, 0.5f);
    
    public static AnchorPoint BottomLeft = new(0, 1f);
    public static AnchorPoint BottomCenter = new(0.5f, 1f);
    public static AnchorPoint BottomRight = new(1f, 1f);

    public readonly float X = x < 0f ? 0f : x > 1f ? 1f : x;
    public readonly float Y = y < 0f ? 0f : y > 1f ? 1f : y;
    
    public AnchorPoint() : this(0f, 0f) { }
    public AnchorPoint(float v): this(v, v) { }
    public AnchorPoint(Vector2 v) : this(v.X, v.Y) { }

    public AnchorPoint Flip() => new(Y, X);
    public AnchorPoint Invert() => new(1f - X, 1f - Y);
    public AnchorPoint InvertX() => new(1f - X, Y);
    public AnchorPoint InvertY() => new(X, 1f - Y);

    public AnchorPoint SetX(float x) => new(x, Y);
    public AnchorPoint SetY(float y) => new(X, y);
    public AnchorPoint ChangeX(float amount) => new(X + amount, Y);
    public AnchorPoint ChangeY(float amount) => new(X, Y + amount);
    public AnchorPoint Random() => new(Rng.Instance.RandF(), Rng.Instance.RandF());
    public AnchorPoint Random(float max) => new(Rng.Instance.RandF(max), Rng.Instance.RandF(max));
    public AnchorPoint Random(float min, float max) => new(Rng.Instance.RandF(min, max), Rng.Instance.RandF(min, max));
    public AnchorPoint Random(float minX, float maxX, float minY, float maxY) => new(Rng.Instance.RandF(minX, maxX), Rng.Instance.RandF(minY, maxY));
    public Vector2 ToVector2() => new Vector2(X, Y);
    
    /// <summary>
    /// Returns a vector2 between 0,0 and 1,1.
    /// 7 , 8 , 9
    /// 4 , 5 , 6
    /// 1 , 2 , 3
    /// 7 is top left and returns 0, 0
    /// 3 is bottom right and returns 1, 1
    /// </summary>
    /// <param name="keypadNumber"></param>
    /// <returns></returns>
    public static AnchorPoint GetKeypadAnchorPosition(int keypadNumber)
    {
        if(keypadNumber < 1 || keypadNumber > 9) return TopLeft;
        
        if(keypadNumber == 1) return new (0f, 1f);
        if(keypadNumber == 2) return new (0.5f, 1f);
        if(keypadNumber == 3) return new (1f, 1f);
        if(keypadNumber == 4) return new (0f, 0.5f);
        if(keypadNumber == 5) return new (0.5f, 0.5f);
        if(keypadNumber == 6) return new (1f, 0.5f);
        if(keypadNumber == 7) return new (0f, 0f);
        if(keypadNumber == 8) return new (0.5f, 0f);
        return new AnchorPoint(1f, 0f);
    }
   
    /// <summary>
    /// Returns a vector2 between 0,0 and 1,1.
    /// 1 , 2 , 3
    /// 4 , 5 , 6
    /// 7 , 8 , 9
    /// 1 is top left and returns 0, 0
    /// 9 is bottom right and returns 1, 1
    /// </summary>
    /// <param name="keypadNumber"></param>
    /// <returns></returns>
    public static AnchorPoint GetKeypadAnchorPositionReversed(int keypadNumber)
    {
        if(keypadNumber < 1 || keypadNumber > 9) return TopLeft;
        
        if(keypadNumber == 1) return new (0f, 0f);
        if(keypadNumber == 2) return new (0.5f, 0f);
        if(keypadNumber == 3) return new (1f, 0f);
        if(keypadNumber == 4) return new (0f, 0.5f);
        if(keypadNumber == 5) return new (0.5f, 0.5f);
        if(keypadNumber == 6) return new (1f, 0.5f);
        if(keypadNumber == 7) return new (0f, 1f);
        if(keypadNumber == 8) return new (0.5f, 1f);
        return new AnchorPoint(1f, 1f);
    }

    public override string ToString() => $"({X}, {Y})";

    public bool Equals(AnchorPoint other) => X.Equals(other.X) && Y.Equals(other.Y);

    public override bool Equals(object? obj) => obj is AnchorPoint other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(X, Y);
}