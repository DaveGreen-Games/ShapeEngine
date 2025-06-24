using System.Numerics;

namespace ShapeEngine.Core.Structs;

/// <summary>
/// Represents a 2D direction using integer horizontal and vertical components.
/// Provides utility methods and operators for direction manipulation and comparison.
/// </summary>
/// <remarks>
/// Useful for grid-based movement, direction checks, and vector math in 2D space.
/// </remarks>
public readonly struct Direction : IEquatable<Direction>
{
    /// <summary>
    /// The horizontal component of the direction.
    /// -1 = left, 0 = none, 1 = right.
    /// </summary>
    public readonly int Horizontal;
    /// <summary>
    /// The vertical component of the direction.
    /// -1 = up, 0 = none, 1 = down.
    /// </summary>
    public readonly int Vertical;

    /// <summary>
    /// Initializes a new instance of the <see cref="Direction"/> struct with zero horizontal and vertical components.
    /// </summary>
    public Direction()
    {
        Horizontal = 0;
        Vertical = 0;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="Direction"/> struct with specified horizontal and vertical components.
    /// </summary>
    /// <param name="hor">The horizontal component (-1, 0, or 1).</param>
    /// <param name="vert">The vertical component (-1, 0, or 1).</param>
    public Direction(int hor, int vert)
    {
        this.Horizontal = hor;
        this.Vertical = vert;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Direction"/> struct from a <see cref="Vector2"/>.
    /// </summary>
    /// <param name="dir">The vector whose X and Y are used as horizontal and vertical components.</param>
    /// <remarks>
    /// Only the integer part of the vector's X and Y are used.
    /// </remarks>
    public Direction(Vector2 dir)
    {
        this.Horizontal = (int)dir.X;
        this.Vertical = (int)dir.X;
    }

    /// <summary>
    /// Gets a value indicating whether the direction is valid (either vertical or horizontal is non-zero).
    /// </summary>
    public bool IsValid => IsVertical || IsHorizontal;
    /// <summary>
    /// Gets a value indicating whether the direction has a non-zero vertical component.
    /// </summary>
    public bool IsVertical => Vertical != 0;
    /// <summary>
    /// Gets a value indicating whether the direction has a non-zero horizontal component.
    /// </summary>
    public bool IsHorizontal => Horizontal != 0;
    
    /// <summary>
    /// Gets a value indicating whether the direction is strictly up (vertical &lt; 0, horizontal == 0).
    /// </summary>
    public bool IsUp => Vertical < 0 && Horizontal == 0;
    /// <summary>
    /// Gets a value indicating whether the direction is strictly down (vertical &gt; 0, horizontal == 0).
    /// </summary>
    public bool IsDown => Vertical > 0 && Horizontal == 0;
    /// <summary>
    /// Gets a value indicating whether the direction is strictly left (horizontal &lt; 0, vertical == 0).
    /// </summary>
    public bool IsLeft => Horizontal < 0 && Vertical == 0;
    /// <summary>
    /// Gets a value indicating whether the direction is strictly right (horizontal &gt; 0, vertical == 0).
    /// </summary>
    public bool IsRight => Horizontal > 0 && Vertical == 0;
    /// <summary>
    /// Gets a value indicating whether the direction is up-left (vertical &lt; 0, horizontal &lt; 0).
    /// </summary>
    public bool IsUpLeft => Vertical < 0 && Horizontal < 0;
    /// <summary>
    /// Gets a value indicating whether the direction is down-left (vertical &gt; 0, horizontal &lt; 0).
    /// </summary>
    public bool IsDownLeft => Vertical > 0 && Horizontal < 0;
    /// <summary>
    /// Gets a value indicating whether the direction is up-right (vertical &lt; 0, horizontal &gt; 0).
    /// </summary>
    public bool IsUpRight => Vertical < 0 && Horizontal > 0;
    /// <summary>
    /// Gets a value indicating whether the direction is down-right (vertical &gt; 0, horizontal &gt; 0).
    /// </summary>
    public bool IsDownRight => Vertical > 0 && Horizontal > 0;

    /// <summary>
    /// Gets an empty direction (0, 0).
    /// </summary>
    public static Direction Empty => new(0, 0);
    /// <summary>
    /// Gets the right direction (1, 0).
    /// </summary>
    public static Direction Right => new(1, 0);
    /// <summary>
    /// Gets the left direction (-1, 0).
    /// </summary>
    public static Direction Left => new(-1, 0);
    /// <summary>
    /// Gets the up direction (0, -1).
    /// </summary>
    public static Direction Up => new(0, -1);
    /// <summary>
    /// Gets the down direction (0, 1).
    /// </summary>
    public static Direction Down => new(0, 1);
    /// <summary>
    /// Gets the up-left direction (-1, -1).
    /// </summary>
    public static Direction UpLeft => new(-1, -1);
    /// <summary>
    /// Gets the up-right direction (1, -1).
    /// </summary>
    public static Direction UpRight => new(1, -1);
    /// <summary>
    /// Gets the down-left direction (-1, 1).
    /// </summary>
    public static Direction DownLeft => new(-1, 1);
    /// <summary>
    /// Gets the down-right direction (1, 1).
    /// </summary>
    public static Direction DownRight => new(1, 1);
    
    /// <summary>
    /// Converts this direction to a <see cref="Vector2"/>.
    /// </summary>
    /// <returns>A <see cref="Vector2"/> with the same horizontal and vertical values.</returns>
    public Vector2 ToVector2() => new(Horizontal, Vertical);
    /// <summary>
    /// Converts this direction to an <see cref="AnchorPoint"/> alignment.
    /// </summary>
    /// <returns>An <see cref="AnchorPoint"/> representing the alignment.</returns>
    /// <remarks> Translates from the value range -1 to 1 that <see cref="Direction"/> uses
    /// to a value range from 0 to 1 that <see cref="AnchorPoint"/> is using.</remarks>
    /// <code>
    /// h = (Horizontal + 1) / 2f;
    /// v = (Vertical + 1) / 2f;
    /// </code>
    public AnchorPoint ToAlignement()
    {
        var h = (Horizontal + 1f) / 2f;
        var v = (Vertical + 1f) / 2f;
        return new AnchorPoint(h, v);
    }
    
    /// <summary>
    /// Inverts the direction and then calculates the alignment.
    /// </summary>
    /// <returns>An <see cref="AnchorPoint"/> representing the alignment of the inverted direction.</returns>
    /// <remarks>
    /// Useful for UI alignment or mirroring logic.
    /// Get more info about translating here -> <see cref="ToAlignement"/>.
    /// </remarks>
    public AnchorPoint ToInvertedAlignement()
    {
        var inverted = Invert();
        var h = (inverted.Horizontal + 1f) / 2f;
        var v = (inverted.Vertical + 1f) / 2f;
        return new AnchorPoint(h, v);
    }
    /// <summary>
    /// Returns a new <see cref="Direction"/> with both components inverted (multiplied by -1).
    /// </summary>
    /// <returns>The inverted direction.</returns>
    public Direction Invert() => new(Horizontal * -1, Vertical * -1);

    /// <summary>
    /// Gets a new <see cref="Direction"/> with each component replaced by its sign (-1, 0, or 1).
    /// </summary>
    public Direction Signed => new(Sign(Horizontal), Sign(Vertical));
    private static int Sign(int value)
    {
        if (value < 0) return -1;
        if (value > 0) return 1;
        return 0;
    }

    #region Operators

    /// <summary>
    /// Adds two <see cref="Direction"/> instances component-wise.
    /// </summary>
    /// <param name="left">The first direction.</param>
    /// <param name="right">The second direction.</param>
    /// <returns>The sum of the two directions.</returns>
    public static Direction operator +(Direction left, Direction right)
    {
        return 
            new
            (
                left.Horizontal + right.Horizontal,
                left.Vertical + right.Vertical
            );
    }

    /// <summary>
    /// Subtracts one <see cref="Direction"/> from another component-wise.
    /// </summary>
    /// <param name="left">The first direction.</param>
    /// <param name="right">The direction to subtract.</param>
    /// <returns>The difference of the two directions.</returns>
    public static Direction operator -(Direction left, Direction right)
    {
        return 
            new
            (
                left.Horizontal - right.Horizontal,
                left.Vertical - right.Vertical
            );
    }
    /// <summary>
    /// Multiplies two <see cref="Direction"/> instances component-wise.
    /// </summary>
    /// <param name="left">The first direction.</param>
    /// <param name="right">The second direction.</param>
    /// <returns>The product of the two directions.</returns>
    public static Direction operator *(Direction left, Direction right)
    {
        return 
            new
            (
                left.Horizontal * right.Horizontal,
                left.Vertical * right.Vertical
            );
    }
    /// <summary>
    /// Divides two <see cref="Direction"/> instances component-wise.
    /// </summary>
    /// <param name="left">The dividend direction.</param>
    /// <param name="right">The divisor direction.
    /// If a component is zero, the corresponding component of <paramref name="left"/> is returned unchanged.</param>
    /// <returns>The quotient of the two directions.</returns>
    public static Direction operator /(Direction left, Direction right)
    {
        return
            new
            (
                right.Horizontal == 0 ? left.Horizontal : left.Horizontal / right.Horizontal,
                right.Vertical == 0 ? left.Vertical : left.Vertical / right.Vertical
            );
    }
    /// <summary>
    /// Adds an integer to both components of a <see cref="Direction"/>.
    /// </summary>
    /// <param name="left">The direction.</param>
    /// <param name="right">The integer to add.</param>
    /// <returns>The resulting direction.</returns>
    public static Direction operator +(Direction left, int right)
    {
        return 
            new
            (
                left.Horizontal + right,
                left.Vertical + right
            );
    }
    /// <summary>
    /// Subtracts an integer from both components of a <see cref="Direction"/>.
    /// </summary>
    /// <param name="left">The direction.</param>
    /// <param name="right">The integer to subtract.</param>
    /// <returns>The resulting direction.</returns>
    public static Direction operator -(Direction left, int right)
    {
        return 
            new
            (
                left.Horizontal - right,
                left.Vertical - right
            );
    }
    /// <summary>
    /// Multiplies both components of a <see cref="Direction"/> by an integer.
    /// </summary>
    /// <param name="left">The direction.</param>
    /// <param name="right">The integer to multiply by.</param>
    /// <returns>The resulting direction.</returns>
    public static Direction operator *(Direction left, int right)
    {
        return 
            new
            (
                left.Horizontal * right,
                left.Vertical * right
            );
    }
    /// <summary>
    /// Divides both components of a <see cref="Direction"/> by an integer.
    /// </summary>
    /// <param name="left">The direction.</param>
    /// <param name="right">The integer to divide by. If zero, the original direction is returned.</param>
    /// <returns>The resulting direction.</returns>
    public static Direction operator /(Direction left, int right)
    {
        if (right == 0) return left;
        return 
            new
            (
                left.Horizontal / right,
                left.Vertical / right
            );
    }
    /// <summary>
    /// Determines whether this instance and another specified <see cref="Direction"/> object have the same value.
    /// </summary>
    /// <param name="other">The direction to compare to this instance.</param>
    /// <returns><c>true</c> if the directions are equal; otherwise, <c>false</c>.</returns>
    public bool Equals(Direction other) => Horizontal == other.Horizontal && Vertical == other.Vertical;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Direction other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Horizontal, Vertical);
    
    /// <summary>
    /// Determines whether two <see cref="Direction"/> instances are equal.
    /// </summary>
    /// <param name="left">The first direction.</param>
    /// <param name="right">The second direction.</param>
    /// <returns><c>true</c> if the directions are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(Direction left, Direction right) => left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="Direction"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first direction.</param>
    /// <param name="right">The second direction.</param>
    /// <returns><c>true</c> if the directions are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(Direction left, Direction right) => !left.Equals(right);

    /// <summary>
    /// Returns a string that represents the current direction.
    /// </summary>
    /// <returns>A string in the format (Horizontal,Vertical).</returns>
    public override string ToString()
    {
        return $"({Horizontal},{Vertical})";
    }
    #endregion
}