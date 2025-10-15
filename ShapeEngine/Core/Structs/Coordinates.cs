using System.Numerics;

namespace ShapeEngine.Core.Structs;

/// <summary>
/// Represents a coordinate on a 2d grid.
/// </summary>
/// <remarks>
/// Provides arithmetic and comparison operators, as well as conversion to <see cref="Vector2"/>.
/// </remarks>
public readonly struct Coordinates : IEquatable<Coordinates>
{
    public readonly int X;
    public readonly int Y;

    /// <summary>
    /// The row index of the coordinate.
    /// </summary>
    public int Row => Y;

    /// <summary>
    /// The column index of the coordinate.
    /// </summary>
    public int Col => X;
    /// <summary>
    /// Gets whether the coordinate is valid (both row and column are non-negative).
    /// </summary>
    public bool IsPositive => X >= 0 && Y >= 0;

    /// <summary>
    /// Gets the product of the absolute values of row and column.
    /// </summary>
    public int Count
    {
        get
        {
            int x = X < 0 ? X * -1 : X;
            int y = Y < 0 ? Y * -1 : Y;
                
            return x * y;
        }
    }
    /// <summary>
    /// Gets the sum of the absolute values of row and column.
    /// </summary>
    public int Distance
    {
        get
        {
            int x = X < 0 ? X * -1 : X;
            int y = Y < 0 ? Y * -1 : Y;
                
            return x + y;
        }
    }

    /// <summary>
    /// Initializes a coordinate with (0, 0)
    /// </summary>
    public Coordinates()
    {
        X = 0;
        Y = 0;
    }
    /// <summary>
    /// Initializes a coordinate with the specified column and row.
    /// </summary>
    /// <param name="x">The column/horizontal index.</param>
    /// <param name="y">The row/vertical index.</param>
    public Coordinates(int x, int y)
    {
        X = x;
        Y = y;
    }

    /// <summary>
    /// Converts this coordinate to a <see cref="Vector2"/>.
    /// </summary>
    /// <returns>A <see cref="Vector2"/> with (X/Col, Y/Row).</returns>
    public Vector2 ToVector2() => new(X, Y);

    #region Operators
    /// <summary>
    /// Determines whether this coordinate is equal to another coordinate.
    /// </summary>
    /// <param name="other">The coordinate to compare with.</param>
    /// <returns>True if the coordinates are equal; otherwise, false.</returns>
    public bool Equals(Coordinates other) => Y == other.Y && X == other.X;

    /// <summary>
    /// Determines whether this coordinate is equal to another object.
    /// </summary>
    /// <param name="obj">The object to compare with.</param>
    /// <returns>True if the object is a <see cref="Coordinates"/> and is equal; otherwise, false.</returns>
    public override bool Equals(object? obj) => obj is Coordinates other && Equals(other);

    /// <summary>
    /// Returns a hash code for this coordinate.
    /// </summary>
    /// <returns>A hash code for the current coordinate.</returns>
    public override int GetHashCode() => HashCode.Combine(Y, X);
        
    /// <summary>
    /// Determines whether two coordinates are equal.
    /// </summary>
    /// <param name="left">The first coordinate to compare.</param>
    /// <param name="right">The second coordinate to compare.</param>
    /// <returns>True if the coordinates are equal; otherwise, false.</returns>
    public static bool operator ==(Coordinates left, Coordinates right) => left.Equals(right);

    /// <summary>
    /// Determines whether two coordinates are not equal.
    /// </summary>
    /// <param name="left">The first coordinate to compare.</param>
    /// <param name="right">The second coordinate to compare.</param>
    /// <returns>True if the coordinates are not equal; otherwise, false.</returns>
    public static bool operator !=(Coordinates left, Coordinates right) => !left.Equals(right);
        
    /// <summary>
    /// Returns a string representation of the coordinate.
    /// </summary>
    /// <returns>A string describing the coordinate's column and row.</returns>
    public override string ToString()
    {
        return $"({X},{Y})";
    }

    /// <summary>
    /// Adds two coordinates together.
    /// </summary>
    /// <param name="left">The first coordinate.</param>
    /// <param name="right">The second coordinate.</param>
    /// <returns>The sum of the two coordinates.</returns>
    public static Coordinates operator +(Coordinates left, Coordinates right)
    {
        return 
            new
            (
                left.X + right.X,
                left.Y + right.Y
            );
    }
    /// <summary>
    /// Subtracts one coordinate from another.
    /// </summary>
    /// <param name="left">The coordinate to subtract from.</param>
    /// <param name="right">The coordinate to subtract.</param>
    /// <returns>The difference of the two coordinates.</returns>
    public static Coordinates operator -(Coordinates left, Coordinates right)
    {
        return 
            new
            (
                left.X - right.X,
                left.Y - right.Y
            );
    }
    /// <summary>
    /// Multiplies two coordinates component-wise.
    /// </summary>
    /// <param name="left">The first coordinate.</param>
    /// <param name="right">The second coordinate.</param>
    /// <returns>The product of the two coordinates.</returns>
    public static Coordinates operator *(Coordinates left, Coordinates right)
    {
        return 
            new
            (
                left.X * right.X,
                left.Y * right.Y
            );
    }
    /// <summary>
    /// Divides one coordinate by another component-wise.
    /// </summary>
    /// <param name="left">The coordinate to divide.</param>
    /// <param name="right">The coordinate to divide by.</param>
    /// <returns>The quotient of the two coordinates. If a component of <paramref name="right"/> is zero,
    /// the corresponding component of <paramref name="left"/> is returned unchanged.</returns>
    public static Coordinates operator /(Coordinates left, Coordinates right)
    {
        return 
            new
            (
                right.X == 0 ? left.X : left.X / right.X,
                right.Y == 0 ? left.Y : left.Y / right.Y
            );
    }
    /// <summary>
    /// Adds a direction to a coordinate.
    /// </summary>
    /// <param name="left">The coordinate.</param>
    /// <param name="right">The direction to add.</param>
    /// <returns>The resulting coordinate.</returns>
    public static Coordinates operator +(Coordinates left, Direction right)
    {
        return 
            new
            (
                left.X + right.Horizontal,
                left.Y + right.Vertical
            );
    }
    /// <summary>
    /// Subtracts a direction from a coordinate.
    /// </summary>
    /// <param name="left">The coordinate.</param>
    /// <param name="right">The direction to subtract.</param>
    /// <returns>The resulting coordinate.</returns>
    public static Coordinates operator -(Coordinates left, Direction right)
    {
        return 
            new
            (
                left.X - right.Horizontal,
                left.Y - right.Vertical
            );
    }
    /// <summary>
    /// Multiplies a coordinate by a direction component-wise.
    /// </summary>
    /// <param name="left">The coordinate.</param>
    /// <param name="right">The direction to multiply by.</param>
    /// <returns>The resulting coordinate.</returns>
    public static Coordinates operator *(Coordinates left, Direction right)
    {
        return 
            new
            (
                left.X * right.Horizontal,
                left.Y * right.Vertical
            );
    }
    /// <summary>
    /// Divides a coordinate by a direction component-wise.
    /// </summary>
    /// <param name="left">The coordinate.</param>
    /// <param name="right">The direction to divide by.</param>
    /// <returns>The resulting coordinate. If a component of <paramref name="right"/> is zero, the corresponding component of <paramref name="left"/> is returned unchanged.</returns>
    public static Coordinates operator /(Coordinates left, Direction right)
    {
        return 
            new
            (
                right.Horizontal == 0 ? left.X : left.X / right.Horizontal,
                right.Vertical == 0 ? left.Y : left.Y / right.Vertical
            );
    }
    /// <summary>
    /// Adds an integer to both components of a coordinate.
    /// </summary>
    /// <param name="left">The coordinate.</param>
    /// <param name="right">The integer to add.</param>
    /// <returns>The resulting coordinate.</returns>
    public static Coordinates operator +(Coordinates left, int right)
    {
        return 
            new
            (
                left.X + right,
                left.Y + right
            );
    }
    /// <summary>
    /// Subtracts an integer from both components of a coordinate.
    /// </summary>
    /// <param name="left">The coordinate.</param>
    /// <param name="right">The integer to subtract.</param>
    /// <returns>The resulting coordinate.</returns>
    public static Coordinates operator -(Coordinates left, int right)
    {
        return 
            new
            (
                left.X - right,
                left.Y - right
            );
    }
    /// <summary>
    /// Multiplies both components of a coordinate by an integer.
    /// </summary>
    /// <param name="left">The coordinate.</param>
    /// <param name="right">The integer to multiply by.</param>
    /// <returns>The resulting coordinate.</returns>
    public static Coordinates operator *(Coordinates left, int right)
    {
        return 
            new
            (
                left.X * right,
                left.Y * right
            );
    }
    /// <summary>
    /// Divides both components of a coordinate by an integer.
    /// </summary>
    /// <param name="left">The coordinate.</param>
    /// <param name="right">The integer to divide by.</param>
    /// <returns>The resulting coordinate. If <paramref name="right"/> is zero, the original coordinate is returned.</returns>
    public static Coordinates operator /(Coordinates left, int right)
    {
        if (right == 0) return left;
        return 
            new
            (
                left.X / right,
                left.Y / right
            );
    }
        
    #endregion
}