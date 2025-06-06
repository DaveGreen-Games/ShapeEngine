using System.Numerics;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Core.Structs;

/// <summary>
/// Represents a two-dimensional size with width and height.
/// </summary>
public readonly struct Size : IEquatable<Size>
{
    #region Members
    /// <summary>
    /// The width component of the size.
    /// </summary>
    public readonly float Width;
    /// <summary>
    /// The height component of the size.
    /// </summary>
    public readonly float Height;
    #endregion

    #region Getters
    /// <summary>
    /// Gets the radius, which is equivalent to the width.
    /// </summary>
    public float Radius => Width;
    /// <summary>
    /// Gets the length, which is equivalent to the width.
    /// </summary>
    public float Length => Width;
    /// <summary>
    /// Gets a value indicating whether both width and height are non-negative.
    /// </summary>
    public bool Positive => Width >= 0 && Height >= 0;
    /// <summary>
    /// Gets the area (Width * Height) if both are non-negative; otherwise, returns 0.
    /// </summary>
    public float Area => Width < 0 || Height < 0 ? 0 : Width * Height;
    /// <summary>
    /// Gets a value indicating whether the size is a square (width and height are equal and positive).
    /// </summary>
    public bool IsSquare => Width > 0 && Height > 0 && Math.Abs(Width - Height) < 0.0001f;
    #endregion
    
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="Size"/> struct with width and height set to 0.
    /// </summary>
    public Size()
    {
        Width = 0f;
        Height = 0f;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Size"/> struct with both width and height set to the specified value.
    /// </summary>
    /// <param name="size">The value to set for both width and height.</param>
    public Size(float size)
    {
        Width = size;
        Height = size;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="Size"/> struct with the specified width and height.
    /// </summary>
    /// <param name="w">The width.</param>
    /// <param name="h">The height.</param>
    public Size(float w, float h)
    {
        Width = w;
        Height = h;
    }
    #endregion

    #region Static
    /// <summary>
    /// Gets a <see cref="Size"/> with both width and height set to 0.
    /// </summary>
    public static Size Zero => new(0, 0);
    #endregion
    
    #region Public Functions
    /// <summary>
    /// Returns the greater of the width or height.
    /// </summary>
    public float Max() => MathF.Max(Width, Height);
    /// <summary>
    /// Returns the lesser of the width or height.
    /// </summary>
    public float Min() => MathF.Min(Width, Height);
    /// <summary>
    /// Returns a new <see cref="Size"/> with the maximum width and height from this and another size.
    /// </summary>
    /// <param name="other">The other size to compare.</param>
    public Size Max(Size other)
    {
        return new
            (
                MathF.Max(Width, other.Width),
                MathF.Max(Height, other.Height)
            );
    }
    /// <summary>
    /// Returns a new <see cref="Size"/> with the minimum width and height from this and another size.
    /// </summary>
    /// <param name="other">The other size to compare.</param>
    public Size Min(Size other)
    {
        return new
        (
            MathF.Min(Width, other.Width),
            MathF.Min(Height, other.Height)
        );
    }
    /// <summary>
    /// Returns the size with the greater area between this and another size.
    /// </summary>
    /// <param name="other">The other size to compare.</param>
    public Size MaxArea(Size other) => Area >= other.Area ? this : other;
    /// <summary>
    /// Returns the size with the lesser area between this and another size.
    /// </summary>
    /// <param name="other">The other size to compare.</param>
    public Size MinArea(Size other) => Area <= other.Area ? this : other;
    
    /// <summary>
    /// Clamps the width and height to the specified minimum and maximum values.
    /// </summary>
    /// <param name="min">The minimum value for both width and height.</param>
    /// <param name="max">The maximum value for both width and height.</param>
    public Size Clamp(float min, float max)
    {
        return new
            (
                ShapeMath.Clamp(Width, min, max),
                ShapeMath.Clamp(Height, min,max)
            );
    }
    /// <summary>
    /// Clamps the width and height to the specified minimum and maximum sizes.
    /// </summary>
    /// <param name="min">The minimum size.</param>
    /// <param name="max">The maximum size.</param>
    public Size Clamp(Size min, Size max)
    {
        return new
        (
            ShapeMath.Clamp(Width, min.Width, max.Width),
            ShapeMath.Clamp(Height, min.Height,max.Height)
        );
    }

    /// <summary>
    /// Returns a new <see cref="Size"/> with width and height swapped.
    /// </summary>
    public Size Switch() => new(Height, Width);
    
    /// <summary>
    /// Performs an exponential decay interpolation from this size towards a target size.
    /// The interpolation factor is mapped to a decay rate between 1 and 25.
    /// </summary>
    /// <param name="to">The target size to interpolate towards.</param>
    /// <param name="f">The interpolation factor (typically between 0 and 1).</param>
    /// <param name="dt">The delta time step.</param>
    /// <returns>The interpolated <see cref="Size"/>.</returns>
    public Size ExpDecayLerp(Size to, float f, float dt)
    {
        var decay = ShapeMath.LerpFloat(1, 25, f);
        var scalar = MathF.Exp(-decay * dt);
        return this + (to - this) * scalar;
        
    }
    /// <summary>
    /// Performs an exponential decay interpolation from this size towards a target size using a custom decay rate.
    /// </summary>
    /// <param name="to">The target size to interpolate towards.</param>
    /// <param name="decay">The decay rate (higher values result in faster convergence).</param>
    /// <param name="dt">The delta time step.</param>
    /// <returns>The interpolated <see cref="Size"/>.</returns>
    public Size ExpDecayLerpComplex(Size to, float decay, float dt)
    {
        var scalar = MathF.Exp(-decay * dt);
        return this + (to - this) * scalar;
    }
    /// <summary>
    /// Performs a power-based interpolation from this size towards a target size.
    /// </summary>
    /// <param name="to">The target size to interpolate towards.</param>
    /// <param name="remainder">The remainder factor (typically between 0 and 1, lower values result in faster convergence).</param>
    /// <param name="dt">The delta time step.</param>
    /// <returns>The interpolated <see cref="Size"/>.</returns>
    public Size PowLerp(Size to, float remainder, float dt)
    {
        var scalar = MathF.Pow(remainder, dt);
        return this + (to - this) * scalar;
            
    }
    /// <summary>
    /// Linearly interpolates between this size and a target size.
    /// </summary>
    /// <param name="to">The target size to interpolate towards.</param>
    /// <param name="f">The interpolation factor (0 returns this size, 1 returns the target size).</param>
    /// <returns>The interpolated <see cref="Size"/>.</returns>
    public Size Lerp(Size to, float f)
    {
        return new
            (
                ShapeMath.LerpFloat(Width, to.Width, f),
                ShapeMath.LerpFloat(Height, to.Height, f)
            );
    }
    /// <summary>
    /// Linearly interpolates between this size and a scalar value for both width and height.
    /// </summary>
    /// <param name="to">The target scalar value for both width and height.</param>
    /// <param name="f">The interpolation factor (0 returns this size, 1 returns the target value).</param>
    /// <returns>The interpolated <see cref="Size"/>.</returns>
    public Size Lerp(float to, float f)
    {
        return new
        (
            ShapeMath.LerpFloat(Width, to, f),
            ShapeMath.LerpFloat(Height, to, f)
        );
    }
    
    /// <summary>
    /// Moves this size towards the target size by a specified speed.
    /// </summary>
    /// <param name="to">The target size.</param>
    /// <param name="speed">The movement speed.</param>
    public Size MoveTowards(Size to, float speed)
    {
        var fromVec = this.ToVector2();
        var toVec = to.ToVector2();
        var result = fromVec.MoveTowards(toVec, speed);
        return result.ToSize();
    }

    /// <summary>
    /// Returns a new <see cref="Size"/> with width and height rounded to the nearest integer.
    /// </summary>
    public Size Round()
    {
        return new
            (
                MathF.Round(Width),
                MathF.Round(Height)
            );
    }
    /// <summary>
    /// Returns a new <see cref="Size"/> with width and height rounded up to the nearest integer.
    /// </summary>
    public Size Ceil()
    {
        return new
        (
            MathF.Ceiling(Width),
            MathF.Ceiling(Height)
        );
    }
    /// <summary>
    /// Returns a new <see cref="Size"/> with width and height rounded down to the nearest integer.
    /// </summary>
    public Size Floor()
    {
        return new
        (
            MathF.Floor(Width),
            MathF.Floor(Height)
        );
    }
    /// <summary>
    /// Returns a new <see cref="Size"/> with width and height truncated to their integer parts.
    /// </summary>
    public Size Truncate()
    {
        return new
        (
            MathF.Truncate(Width),
            MathF.Truncate(Height)
        );
    }
    /// <summary>
    /// Returns a new <see cref="Size"/> with width and height rounded to the specified number of decimal places.
    /// </summary>
    /// <param name="decimals">The number of decimal places.</param>
    public Size Round(int decimals)
    {
        return new
        (
            ShapeMath.RoundToDecimals(Width, decimals),
            ShapeMath.RoundToDecimals(Height, decimals)
        );
    }
    
    /// <summary>
    /// Returns a new <see cref="Size"/> with the width set to the specified value.
    /// </summary>
    /// <param name="newRadius">The new width value.</param>
    public Size SetRadius(float newRadius) => new(newRadius, Height);
    /// <summary>
    /// Returns a new <see cref="Size"/> with the width increased by the specified amount.
    /// </summary>
    /// <param name="amount">The amount to add to the width.</param>
    public Size ChangeRadius(float amount) => new(Width + amount, Height);
    /// <summary>
    /// Returns a new <see cref="Size"/> with the width set to the specified value.
    /// </summary>
    /// <param name="newLength">The new width value.</param>
    public Size SetLength(float newLength) => new(newLength, Height);
    /// <summary>
    /// Returns a new <see cref="Size"/> with the width increased by the specified amount.
    /// </summary>
    /// <param name="amount">The amount to add to the width.</param>
    public Size ChangeLength(float amount) => new(Width + amount, Height);
    
    /// <summary>
    /// Converts this size to a <see cref="Vector2"/> representation.
    /// </summary>
    public Vector2 ToVector2() => new(Width, Height);
    /// <summary>
    /// Returns a string representation of the size.
    /// </summary>
    public override string ToString()
    {
        return $"(w: {Width}, h: {Height})";
    }
    /// <summary>
    /// Determines whether the specified <see cref="Size"/> is equal to the current <see cref="Size"/>.
    /// </summary>
    /// <param name="other">The other size to compare.</param>
    public bool Equals(Size other) => Width.Equals(other.Width) && Height.Equals(other.Height);
    /// <summary>
    /// Determines whether the specified object is equal to the current <see cref="Size"/>.
    /// </summary>
    /// <param name="obj">The object to compare.</param>
    public override bool Equals(object? obj) => obj is Size other && Equals(other);
    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    public override int GetHashCode() => HashCode.Combine(Width, Height);
    
    #endregion

    #region Operators

    /// <summary>
    /// Determines whether two <see cref="Size"/> instances are equal.
    /// </summary>
    public static bool operator ==(Size left, Size right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two <see cref="Size"/> instances are not equal.
    /// </summary>
    public static bool operator !=(Size left, Size right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// Determines whether a <see cref="Size"/> and a <see cref="Vector2"/> are equal.
    /// </summary>
    public static bool operator ==(Size left, Vector2 right)
    {
        return Math.Abs(left.Width - right.X) < 0.0001f && Math.Abs(left.Height - right.Y) < 0.0001f;
    }

    /// <summary>
    /// Determines whether a <see cref="Size"/> and a <see cref="Vector2"/> are not equal.
    /// </summary>
    public static bool operator !=(Size left, Vector2 right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Determines whether a <see cref="Vector2"/> and a <see cref="Size"/> are equal.
    /// </summary>
    public static bool operator ==(Vector2 left, Size right)
    {
        return Math.Abs(left.X - right.Width) < 0.0001f && Math.Abs(left.Y - right.Height) < 0.0001f;
    }

    /// <summary>
    /// Determines whether a <see cref="Vector2"/> and a <see cref="Size"/> are not equal.
    /// </summary>
    public static bool operator !=(Vector2 left, Size right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Adds two <see cref="Size"/> instances.
    /// </summary>
    public static Size operator +(Size left, Size right)
    {
        return new
        (
            left.Width + right.Width,
            left.Height + right.Height
        );
    }

    /// <summary>
    /// Subtracts one <see cref="Size"/> from another.
    /// </summary>
    public static Size operator -(Size left, Size right)
    {
        return new
        (
            left.Width - right.Width,
            left.Height - right.Height
        );
    }

    /// <summary>
    /// Multiplies two <see cref="Size"/> instances component-wise.
    /// </summary>
    public static Size operator *(Size left, Size right)
    {
        return new
        (
            left.Width * right.Width,
            left.Height * right.Height
        );
    }

    /// <summary>
    /// Divides one <see cref="Size"/> by another component-wise.
    /// </summary>
    public static Size operator /(Size left, Size right)
    {
        return new
        (
             right.Width == 0 ? left.Width : left.Width / right.Width,
            right.Height == 0 ? left.Height : left.Height / right.Height
        );
    }

    /// <summary>
    /// Adds a <see cref="Size"/> to a <see cref="Vector2"/>.
    /// </summary>
    public static Vector2 operator +(Vector2 left, Size right)
    {
        return new
        (
            left.X + right.Width,
            left.Y + right.Height   
        );
    }

    /// <summary>
    /// Subtracts a <see cref="Size"/> from a <see cref="Vector2"/>.
    /// </summary>
    public static Vector2 operator -(Vector2 left, Size right)
    {
        return new
        (
            left.X - right.Width,
            left.Y - right.Height   
        );
    }

    /// <summary>
    /// Multiplies a <see cref="Vector2"/> by a <see cref="Size"/> component-wise.
    /// </summary>
    public static Vector2 operator *(Vector2 left, Size right)
    {
        return new
        (
            left.X * right.Width,
            left.Y * right.Height   
        );
    }

    /// <summary>
    /// Divides a <see cref="Vector2"/> by a <see cref="Size"/> component-wise.
    /// </summary>
    public static Vector2 operator /(Vector2 left, Size right)
    {
        return new
        (
            right.Width == 0 ? left.X : left.X / right.Width,
            right.Height == 0 ? left.Y : left.Y / right.Height   
        );
    }

    /// <summary>
    /// Adds a <see cref="Size"/> to a <see cref="Vector2"/>.
    /// </summary>
    public static Size operator +(Size left, Vector2 right)
    {
        return new
        (
            left.Width + right.X,
            left.Height + right.Y   
        );
    }

    /// <summary>
    /// Subtracts a <see cref="Vector2"/> from a <see cref="Size"/>.
    /// </summary>
    public static Size operator -(Size left, Vector2 right)
    {
        return new
        (
            left.Width - right.X,
            left.Height - right.Y   
        );
    }

    /// <summary>
    /// Multiplies a <see cref="Size"/> by a <see cref="Vector2"/> component-wise.
    /// </summary>
    public static Size operator *(Size left, Vector2 right)
    {
        return new
        (
            left.Width * right.X,
            left.Height * right.Y   
        );
    }

    /// <summary>
    /// Divides a <see cref="Size"/> by a <see cref="Vector2"/> component-wise.
    /// </summary>
    public static Size operator /(Size left, Vector2 right)
    {
        return new
        (
            right.X == 0 ? left.Width : left.Width / right.X,
            right.Y == 0 ? left.Height : left.Height / right.Y
        );
    }

    /// <summary>
    /// Adds a <see cref="Direction"/> to a <see cref="Size"/>.
    /// </summary>
    public static Size operator +(Size left, Direction right)
    {
        return new
        (
            left.Width + right.Horizontal,
            left.Height + right.Vertical   
        );
    }

    /// <summary>
    /// Subtracts a <see cref="Direction"/> from a <see cref="Size"/>.
    /// </summary>
    public static Size operator -(Size left, Direction right)
    {
        return new
        (
            left.Width - right.Horizontal,
            left.Height - right.Vertical   
        );
    }

    /// <summary>
    /// Multiplies a <see cref="Size"/> by a <see cref="Direction"/> component-wise.
    /// </summary>
    public static Size operator *(Size left, Direction right)
    {
        return new
        (
            left.Width * right.Horizontal,
            left.Height * right.Vertical   
        );
    }

    /// <summary>
    /// Divides a <see cref="Size"/> by a <see cref="Direction"/> component-wise.
    /// </summary>
    public static Size operator /(Size left, Direction right)
    {
        return new
        (
            right.Horizontal == 0 ? left.Width : left.Width / right.Horizontal,
            right.Vertical == 0 ? left.Height : left.Height / right.Vertical
        );
    }

    /// <summary>
    /// Adds a scalar value to both width and height of a <see cref="Size"/>.
    /// </summary>
    public static Size operator +(Size left, float right)
    {
        return new
        (
            left.Width + right,
            left.Height + right   
        );
    }

    /// <summary>
    /// Subtracts a scalar value from both width and height of a <see cref="Size"/>.
    /// </summary>
    public static Size operator -(Size left, float right)
    {
        return new
        (
            left.Width - right,
            left.Height - right   
        );
    }

    /// <summary>
    /// Multiplies both width and height of a <see cref="Size"/> by a scalar value.
    /// </summary>
    public static Size operator *(Size left, float right)
    {
        return new
        (
            left.Width * right,
            left.Height * right   
        );
    }

    /// <summary>
    /// Divides both width and height of a <see cref="Size"/> by a scalar value.
    /// </summary>
    public static Size operator /(Size left, float right)
    {
        if (right == 0) return left;
        return new
        (
            left.Width / right,
            left.Height / right   
        );
    }

    /// <summary>
    /// Adds a <see cref="Size"/> to an <see cref="AnchorPoint"/>.
    /// </summary>
    public static Size operator +(AnchorPoint left, Size right) => new(left.X + right.Width, left.Y + right.Height);

    /// <summary>
    /// Subtracts a <see cref="Size"/> from an <see cref="AnchorPoint"/>.
    /// </summary>
    public static Size operator -(AnchorPoint left, Size right) => new(left.X - right.Width, left.Y - right.Height);

    /// <summary>
    /// Multiplies an <see cref="AnchorPoint"/> by a <see cref="Size"/> component-wise.
    /// </summary>
    public static Size operator *(AnchorPoint left, Size right) => new(left.X * right.Width, left.Y * right.Height);

    /// <summary>
    /// Divides an <see cref="AnchorPoint"/> by a <see cref="Size"/> component-wise.
    /// </summary>
    public static Size operator /(AnchorPoint left, Size right) => new(left.X / (right.Width == 0f ? 1f : right.Width), left.Y / (right.Height == 0f ? 1f : right.Height));

    /// <summary>
    /// Adds an <see cref="AnchorPoint"/> to a <see cref="Size"/>.
    /// </summary>
    public static Size operator +(Size left, AnchorPoint right) => new(left.Width + right.X, left.Height + right.Y);

    /// <summary>
    /// Subtracts an <see cref="AnchorPoint"/> from a <see cref="Size"/>.
    /// </summary>
    public static Size operator -(Size left, AnchorPoint right) => new(left.Width - right.X, left.Height - right.Y);

    /// <summary>
    /// Multiplies a <see cref="Size"/> by an <see cref="AnchorPoint"/> component-wise.
    /// </summary>
    public static Size operator *(Size left, AnchorPoint right) => new(left.Width * right.X, left.Height * right.Y);

    /// <summary>
    /// Divides a <see cref="Size"/> by an <see cref="AnchorPoint"/> component-wise.
    /// </summary>
    public static Size operator /(Size left, AnchorPoint right) => new(left.Width / (right.X == 0f ? 1f : right.X), left.Height / (right.Y == 0f ? 1f : right.Y));

    #endregion
}