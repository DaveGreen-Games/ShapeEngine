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
    /// <summary>
    /// The column / horizontal component of the coordinate.
    /// Read-only field representing the X (column) value.
    /// </summary>
    public readonly int X;
    
    /// <summary>
    /// The row / vertical component of the coordinate.
    /// Read-only field representing the Y (row) value.
    /// </summary>
    public readonly int Y;

    /// <summary>
    /// The origin coordinate (0,0).
    /// </summary>
    public static Coordinates Zero => new(0, 0);
    
    /// <summary>
    /// A coordinate with both components set to one (1,1).
    /// </summary>
    public static Coordinates One => new(1, 1);
    
    /// <summary>
    /// A unit step upward on the grid (0,1).
    /// </summary>
    public static Coordinates Up => new(0, 1);
    
    /// <summary>
    /// A unit step downward on the grid (0,-1).
    /// </summary>
    public static Coordinates Down => new(0, -1);
    
    /// <summary>
    /// A unit step to the left on the grid (-1,0).
    /// </summary>
    public static Coordinates Left => new(-1, 0);
    
    /// <summary>
    /// A unit step to the right on the grid (1,0).
    /// </summary>
    public static Coordinates Right => new(1, 0);
    
    /// <summary>
    /// The maximum representable coordinate (int.MaxValue,int.MaxValue).
    /// </summary>
    public static Coordinates MaxValue => new(int.MaxValue, int.MaxValue);
    
    /// <summary>
    /// The minimum representable coordinate (int.MinValue,int.MinValue).
    /// </summary>
    public static Coordinates MinValue => new(int.MinValue, int.MinValue);
    
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
    /// <summary>
    /// Calculates the 1D index in a row-major order for this coordinate, given the number of columns.
    /// </summary>
    /// <param name="cols">The total number of columns in the grid.</param>
    /// <returns>The 1D index corresponding to this coordinate.</returns>
    public int GetIndexCols(int cols)
    {
        return X + Y * cols;
    }
    /// <summary>
    /// Calculates the 1D index in a column-major order for this coordinate, given the number of rows.
    /// </summary>
    /// <param name="rows">The total number of rows in the grid.</param>
    /// <returns>The 1D index corresponding to this coordinate in column-major order.</returns>
    public int GetIndexRows(int rows)
    {
        return Y + X * rows;
    }


    /// <summary>
    /// Returns a coordinate composed of the component-wise minimum between this and <paramref name="other"/>.
    /// </summary>
    /// <param name="other">The coordinate to compare with.</param>
    /// <returns>
    /// A new <see cref="Coordinates"/> where each component is the minimum of the corresponding components.
    /// </returns>
    public Coordinates Min(Coordinates other)
    {
        return new Coordinates(Math.Min(X, other.X), Math.Min(Y, other.Y));
    }
    /// <summary>
    /// Returns a coordinate composed of the component-wise maximum between this and <paramref name="other"/>.
    /// </summary>
    /// <param name="other">The coordinate to compare with.</param>
    /// <returns>
    /// A new <see cref="Coordinates"/> where each component is the maximum of the corresponding components.
    /// </returns>
    public Coordinates Max(Coordinates other)
    {
        return new Coordinates(Math.Max(X, other.X), Math.Max(Y, other.Y));
    }
    
    /// <summary>
    /// Converts a 2D point to grid coordinates based on the grid's origin, spacing, and dimensions.
    /// </summary>
    /// <param name="x">The x (horizontal) position of the point.</param>
    /// <param name="y">The y (vertical) position of the point.</param>
    /// <param name="origin">The origin (top-left corner) of the grid in world coordinates.</param>
    /// <param name="spacing">The spacing between grid cells (width, height).</param>
    /// <param name="cols">The number of columns in the grid.</param>
    /// <param name="rows">The number of rows in the grid.</param>
    /// <returns>The corresponding <see cref="Coordinates"/> on the grid.</returns>
    public static Coordinates PointToCoordinates(float x, float y, Vector2 origin, Vector2 spacing, int cols, int rows)
    {
        int xi = Math.Clamp((int)Math.Floor((x - origin.X) / spacing.X), 0, cols - 1);
        int yi = Math.Clamp((int)Math.Floor((y - origin.Y) / spacing.Y), 0, rows - 1);
        return new(xi, yi);
    }
    
    /// <summary>
    /// Converts a 2D point to grid coordinates based on the grid's origin, spacing, and dimensions.
    /// </summary>
    /// <param name="point">The 2D point in world coordinates.</param>
    /// <param name="origin">The origin (top-left corner) of the grid in world coordinates.</param>
    /// <param name="spacing">The spacing between grid cells (width, height).</param>
    /// <param name="cols">The number of columns in the grid.</param>
    /// <param name="rows">The number of rows in the grid.</param>
    /// <returns>The corresponding <see cref="Coordinates"/> on the grid.</returns>
    public static Coordinates PointToCoordinates(Vector2 point, Vector2 origin, Vector2 spacing, int cols, int rows) => PointToCoordinates(point.X, point.Y, origin, spacing, cols, rows);
    /// <summary>
    /// Converts a 2D point to grid coordinates based on the cell size.
    /// </summary>
    /// <param name="x">The x (horizontal) position of the point.</param>
    /// <param name="y">The y (vertical) position of the point.</param>
    /// <param name="cellSize">The size of each grid cell.</param>
    /// <returns>The corresponding <see cref="Coordinates"/> on the grid.</returns>
    public static Coordinates PointToCoordinates(float x, float y, Size cellSize)
    {
        var cx = (int)Math.Floor(x / cellSize.Width);
        var cy = (int)Math.Floor(y / cellSize.Width);
        return new Coordinates(cx, cy);
    }
    /// <summary>
    /// Converts a 2D point to grid coordinates based on the cell size.
    /// </summary>
    /// <param name="point">The 2D point in world coordinates.</param>
    /// <param name="cellSize">The size of each grid cell.</param>
    /// <returns>The corresponding <see cref="Coordinates"/> on the grid.</returns>
    public static Coordinates PointToCoordinates(Vector2 point, Size cellSize) => PointToCoordinates(point.X, point.Y, cellSize);
    
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