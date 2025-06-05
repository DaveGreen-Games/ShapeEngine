using System.Numerics;
using ShapeEngine.Random;

namespace ShapeEngine.Core.Structs;

/// <summary>
/// Used for ui anchor points and rect alignments. 0, 0 is top left, 1, 1 is bottom right.
/// </summary>
/// <param name="x">Range 0-1. 0 is left, 0.5 is center, 1 is right.</param>
/// <param name="y">Range 0-1. 0 is top, 0.5 is center, 1 is bottom.</param>
public readonly struct AnchorPoint(float x, float y) : IEquatable<AnchorPoint>
{
   /// <summary>
   /// Anchor point at (0, 0). Top-left corner.
   /// </summary>
   public static AnchorPoint Zero = new(0, 0);
   
   /// <summary>
   /// Anchor point at (1, 1). Bottom-right corner.
   /// </summary>
   public static AnchorPoint One = new(1, 1);
   
   /// <summary>
   /// Anchor point at (0, 0). Top-left corner.
   /// </summary>
   public static AnchorPoint TopLeft = new(0, 0);
   
   /// <summary>
   /// Anchor point at (0.5, 0). Top-center.
   /// </summary>
   public static AnchorPoint TopCenter = new(0.5f, 0);
   
   /// <summary>
   /// Anchor point at (1, 0). Top-right corner.
   /// </summary>
   public static AnchorPoint TopRight = new(1, 0);
   
   /// <summary>
   /// Anchor point at (0, 0.5). Center-left.
   /// </summary>
   public static AnchorPoint Left = new(0, 0.5f);
   
   /// <summary>
   /// Anchor point at (0.5, 0.5). Center.
   /// </summary>
   public static AnchorPoint Center = new(0.5f, 0.5f);
   
   /// <summary>
   /// Anchor point at (1, 0.5). Center-right.
   /// </summary>
   public static AnchorPoint Right = new(1, 0.5f);
   
   /// <summary>
   /// Anchor point at (0, 1). Bottom-left corner.
   /// </summary>
   public static AnchorPoint BottomLeft = new(0, 1f);
   
   /// <summary>
   /// Anchor point at (0.5, 1). Bottom-center.
   /// </summary>
   public static AnchorPoint BottomCenter = new(0.5f, 1f);
   
   /// <summary>
   /// Anchor point at (1, 1). Bottom-right corner.
   /// </summary>
   public static AnchorPoint BottomRight = new(1f, 1f);
   
   /// <summary>
   /// X coordinate of the anchor point, clamped between 0 and 1.
   /// </summary>
   public readonly float X = x < 0f ? 0f : x > 1f ? 1f : x;
   
   /// <summary>
   /// Y coordinate of the anchor point, clamped between 0 and 1.
   /// </summary>
   public readonly float Y = y < 0f ? 0f : y > 1f ? 1f : y;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="AnchorPoint"/> struct at (0, 0).
    /// </summary>
    public AnchorPoint() : this(0f, 0f) { }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="AnchorPoint"/> struct with both coordinates set to <paramref name="v"/>.
    /// </summary>
    /// <param name="v">The value for both X and Y coordinates.</param>
    public AnchorPoint(float v): this(v, v) { }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="AnchorPoint"/> struct from a <see cref="Vector2"/>.
    /// </summary>
    /// <param name="v">The vector whose X and Y are used for the anchor point.</param>
    public AnchorPoint(Vector2 v) : this(v.X, v.Y) { }
    
    /// <summary>
    /// Returns a new <see cref="AnchorPoint"/> with X and Y swapped.
    /// </summary>
    public AnchorPoint Flip() => new(Y, X);
    
    /// <summary>
    /// Returns a new <see cref="AnchorPoint"/> with both X and Y inverted (1 - X, 1 - Y).
    /// </summary>
    public AnchorPoint Invert() => new(1f - X, 1f - Y);
    
    /// <summary>
    /// Returns a new <see cref="AnchorPoint"/> with X inverted (1 - X).
    /// </summary>
    public AnchorPoint InvertX() => new(1f - X, Y);
    
    /// <summary>
    /// Returns a new <see cref="AnchorPoint"/> with Y inverted (1 - Y).
    /// </summary>
    public AnchorPoint InvertY() => new(X, 1f - Y);
    
    /// <summary>
    /// Returns a new <see cref="AnchorPoint"/> with the X coordinate set to <paramref name="x"/>.
    /// </summary>
    /// <param name="x">The new X coordinate.</param>
    public AnchorPoint SetX(float x) => new(x, Y);
    
    /// <summary>
    /// Returns a new <see cref="AnchorPoint"/> with the Y coordinate set to <paramref name="y"/>.
    /// </summary>
    /// <param name="y">The new Y coordinate.</param>
    public AnchorPoint SetY(float y) => new(X, y);
    
    /// <summary>
    /// Returns a new <see cref="AnchorPoint"/> with the X coordinate changed by <paramref name="amount"/>.
    /// </summary>
    /// <param name="amount">The amount to add to X.</param>
    public AnchorPoint ChangeX(float amount) => new(X + amount, Y);
    
    /// <summary>
    /// Returns a new <see cref="AnchorPoint"/> with the Y coordinate changed by <paramref name="amount"/>.
    /// </summary>
    /// <param name="amount">The amount to add to Y.</param>
    public AnchorPoint ChangeY(float amount) => new(X, Y + amount);
    
    /// <summary>
    /// Returns a new <see cref="AnchorPoint"/> with random X and Y values between 0 and 1.
    /// </summary>
    public AnchorPoint Random() => new(Rng.Instance.RandF(), Rng.Instance.RandF());
    
    /// <summary>
    /// Returns a new <see cref="AnchorPoint"/> with random X and Y values between 0 and <paramref name="max"/>.
    /// </summary>
    /// <param name="max">The maximum value for X and Y.</param>
    public AnchorPoint Random(float max) => new(Rng.Instance.RandF(max), Rng.Instance.RandF(max));
    
    /// <summary>
    /// Returns a new <see cref="AnchorPoint"/> with random X and Y values between <paramref name="min"/> and <paramref name="max"/>.
    /// </summary>
    /// <param name="min">The minimum value for X and Y.</param>
    /// <param name="max">The maximum value for X and Y.</param>
    public AnchorPoint Random(float min, float max) => new(Rng.Instance.RandF(min, max), Rng.Instance.RandF(min, max));
    
    /// <summary>
    /// Returns a new <see cref="AnchorPoint"/> with random X and Y values within the specified ranges.
    /// </summary>
    /// <param name="minX">The minimum value for X.</param>
    /// <param name="maxX">The maximum value for X.</param>
    /// <param name="minY">The minimum value for Y.</param>
    /// <param name="maxY">The maximum value for Y.</param>
    public AnchorPoint Random(float minX, float maxX, float minY, float maxY) => new(Rng.Instance.RandF(minX, maxX), Rng.Instance.RandF(minY, maxY));
    
    /// <summary>
    /// Converts this <see cref="AnchorPoint"/> to a <see cref="Vector2"/>.
    /// </summary>
    /// <returns>A <see cref="Vector2"/> with the same X and Y values.</returns>
    public Vector2 ToVector2() => new(X, Y);
    
    /// <summary>
    /// Returns an <see cref="AnchorPoint"/> mapped from a numeric keypad layout (7-8-9 on top, 1-2-3 on bottom).
    /// <para>
    /// Keypad layout:
    /// 7 , 8 , 9 (top row)
    /// 4 , 5 , 6 (middle row)
    /// 1 , 2 , 3 (bottom row)
    /// </para>
    /// <para>
    /// 7 is top-left (0,0), 3 is bottom-right (1,1).
    /// </para>
    /// </summary>
    /// <param name="keypadNumber">
    /// The keypad number (1-9) corresponding to the anchor position.
    /// </param>
    /// <returns>
    /// The corresponding <see cref="AnchorPoint"/>. Returns <see cref="TopLeft"/> if the number is out of range.
    /// </returns>
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
    /// Returns an <see cref="AnchorPoint"/> mapped from a numeric keypad layout (1-2-3 on top, 7-8-9 on bottom).
    /// <para>
    /// Keypad layout:
    /// 1 , 2 , 3 (top row)
    /// 4 , 5 , 6 (middle row)
    /// 7 , 8 , 9 (bottom row)
    /// </para>
    /// <para>
    /// 1 is top-left (0,0), 9 is bottom-right (1,1).
    /// </para>
    /// </summary>
    /// <param name="keypadNumber">
    /// The keypad number (1-9) corresponding to the anchor position.
    /// </param>
    /// <returns>
    /// The corresponding <see cref="AnchorPoint"/>. Returns <see cref="TopLeft"/> if the number is out of range.
    /// </returns>
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

    /// <summary>
    /// Returns a string representation of the <see cref="AnchorPoint"/>.
    /// </summary>
    public override string ToString() => $"({X}, {Y})";
    
    /// <summary>
    /// Determines whether the specified <see cref="AnchorPoint"/> is equal to the current <see cref="AnchorPoint"/>.
    /// </summary>
    /// <param name="other">The <see cref="AnchorPoint"/> to compare with the current <see cref="AnchorPoint"/>.</param>
    /// <returns><c>true</c> if the specified <see cref="AnchorPoint"/> is equal to the current <see cref="AnchorPoint"/>; otherwise, <c>false</c>.</returns>
    public bool Equals(AnchorPoint other) => X.Equals(other.X) && Y.Equals(other.Y);
    
    /// <summary>
    /// Determines whether the specified object is equal to the current <see cref="AnchorPoint"/>.
    /// </summary>
    /// <param name="obj">The object to compare with the current <see cref="AnchorPoint"/>.</param>
    /// <returns><c>true</c> if the specified object is equal to the current <see cref="AnchorPoint"/>; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj) => obj is AnchorPoint other && Equals(other);
    
    /// <summary>
    /// Returns a hash code for the current <see cref="AnchorPoint"/>.
    /// </summary>
    /// <returns>A hash code for the current <see cref="AnchorPoint"/>.</returns>
    public override int GetHashCode() => HashCode.Combine(X, Y);
    
    /// <summary>
    /// Determines whether two <see cref="AnchorPoint"/> instances are equal.
    /// </summary>
    public static bool operator ==(AnchorPoint left, AnchorPoint right) => left.Equals(right);
    
    /// <summary>
    /// Determines whether two <see cref="AnchorPoint"/> instances are not equal.
    /// </summary>
    public static bool operator !=(AnchorPoint left, AnchorPoint right) => left.Equals(right);
    
    /// <summary>
    /// Adds two <see cref="AnchorPoint"/> instances.
    /// </summary>
    public static AnchorPoint operator +(AnchorPoint left, AnchorPoint right) => new(left.X + right.X, left.Y + right.Y);
    
    /// <summary>
    /// Subtracts one <see cref="AnchorPoint"/> from another.
    /// </summary>
    public static AnchorPoint operator -(AnchorPoint left, AnchorPoint right) => new(left.X - right.X, left.Y - right.Y);
    
    /// <summary>
    /// Multiplies two <see cref="AnchorPoint"/> instances.
    /// </summary>
    public static AnchorPoint operator *(AnchorPoint left, AnchorPoint right) => new(left.X * right.X, left.Y * right.Y);
    
    /// <summary>
    /// Divides one <see cref="AnchorPoint"/> by another, avoiding division by zero.
    /// </summary>
    public static AnchorPoint operator /(AnchorPoint left, AnchorPoint right) => new(left.X / (right.X == 0f ? 1f : right.X), left.Y / (right.Y == 0f ? 1f : right.Y));
    
    /// <summary>
    /// Adds a scalar value to both coordinates of an <see cref="AnchorPoint"/>.
    /// </summary>
    public static AnchorPoint operator +(AnchorPoint left, float right) => new(left.X + right, left.Y + right);
    
    /// <summary>
    /// Subtracts a scalar value from both coordinates of an <see cref="AnchorPoint"/>.
    /// </summary>
    public static AnchorPoint operator -(AnchorPoint left, float right) => new(left.X - right, left.Y - right);
    
    /// <summary>
    /// Multiplies both coordinates of an <see cref="AnchorPoint"/> by a scalar value.
    /// </summary>
    public static AnchorPoint operator *(AnchorPoint left, float right) => new(left.X * right, left.Y * right);
    
    /// <summary>
    /// Divides both coordinates of an <see cref="AnchorPoint"/> by a scalar value. Returns <see cref="Zero"/> if the scalar is zero.
    /// </summary>
    public static AnchorPoint operator /(AnchorPoint left, float right)
    {
        if(right == 0) return Zero;
        return new(left.X / right, left.Y / right);
    }
    
    /// <summary>
    /// Adds a <see cref="Vector2"/> to an <see cref="AnchorPoint"/>.
    /// </summary>
    public static Vector2 operator +(AnchorPoint left, Vector2 right) => new(left.X + right.X, left.Y + right.Y);
    
    /// <summary>
    /// Subtracts a <see cref="Vector2"/> from an <see cref="AnchorPoint"/>.
    /// </summary>
    public static Vector2 operator -(AnchorPoint left, Vector2 right) => new(left.X - right.X, left.Y - right.Y);
    
    /// <summary>
    /// Multiplies an <see cref="AnchorPoint"/> by a <see cref="Vector2"/>.
    /// </summary>
    public static Vector2 operator *(AnchorPoint left, Vector2 right) => new(left.X * right.X, left.Y * right.Y);
    
    /// <summary>
    /// Divides an <see cref="AnchorPoint"/> by a <see cref="Vector2"/>, avoiding division by zero.
    /// </summary>
    public static Vector2 operator /(AnchorPoint left, Vector2 right) => new(left.X / (right.X == 0f ? 1f : right.X), left.Y / (right.Y == 0f ? 1f : right.Y));
    
    /// <summary>
    /// Adds an <see cref="AnchorPoint"/> to a <see cref="Vector2"/>.
    /// </summary>
    public static Vector2 operator +(Vector2 left, AnchorPoint right) => new(left.X + right.X, left.Y + right.Y);
    
    /// <summary>
    /// Subtracts an <see cref="AnchorPoint"/> from a <see cref="Vector2"/>.
    /// </summary>
    public static Vector2 operator -(Vector2 left, AnchorPoint right) => new(left.X - right.X, left.Y - right.Y);
    
    /// <summary>
    /// Multiplies a <see cref="Vector2"/> by an <see cref="AnchorPoint"/>.
    /// </summary>
    public static Vector2 operator *(Vector2 left, AnchorPoint right) => new(left.X * right.X, left.Y * right.Y);
    
    /// <summary>
    /// Divides a <see cref="Vector2"/> by an <see cref="AnchorPoint"/>, avoiding division by zero.
    /// </summary>
    public static Vector2 operator /(Vector2 left, AnchorPoint right) => new(left.X / (right.X == 0f ? 1f : right.X), left.Y / (right.Y == 0f ? 1f : right.Y));
}