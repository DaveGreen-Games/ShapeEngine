using System.Numerics;

namespace ShapeEngine.Core.Structs;

public readonly struct Direction : IEquatable<Direction>
{
    public readonly int Horizontal;
    public readonly int Vertical;

    public Direction()
    {
        Horizontal = 0;
        Vertical = 0;
    }
    public Direction(int hor, int vert)
    {
        this.Horizontal = hor;
        this.Vertical = vert;
    }

    public Direction(Vector2 dir)
    {
        this.Horizontal = (int)dir.X;
        this.Vertical = (int)dir.X;
    }

    public bool IsValid => IsVertical || IsHorizontal;
    public bool IsVertical => Vertical != 0;
    public bool IsHorizontal => Horizontal != 0;
    
    public bool IsUp => Vertical < 0 && Horizontal == 0;
    public bool IsDown => Vertical > 0 && Horizontal == 0;
    public bool IsLeft => Horizontal < 0 && Vertical == 0;
    public bool IsRight => Horizontal > 0 && Vertical == 0;
    public bool IsUpLeft => Vertical < 0 && Horizontal < 0;
    public bool IsDownLeft => Vertical > 0 && Horizontal < 0;
    public bool IsUpRight => Vertical < 0 && Horizontal > 0;
    public bool IsDownRight => Vertical > 0 && Horizontal > 0;

    public static Direction Empty => new(0, 0);
    public static Direction Right => new(1, 0);
    public static Direction Left => new(-1, 0);
    public static Direction Up => new(0, -1);
    public static Direction Down => new(0, 1);
    public static Direction UpLeft => new(-1, -1);
    public static Direction UpRight => new(1, -1);
    public static Direction DownLeft => new(-1, 1);
    public static Direction DownRight => new(1, 1);
    
    public Vector2 ToVector2() => new(Horizontal, Vertical);
    public AnchorPoint ToAlignement()
    {
        var h = (Horizontal + 1f) / 2f;
        var v = (Vertical + 1f) / 2f;
        return new AnchorPoint(h, v);
    }
    
    /// <summary>
    /// Inverts the direction and then calculates the alignement
    /// </summary>
    /// <returns></returns>
    public AnchorPoint ToInvertedAlignement()
    {
        var inverted = Invert();
        var h = (inverted.Horizontal + 1f) / 2f;
        var v = (inverted.Vertical + 1f) / 2f;
        return new AnchorPoint(h, v);
    }
    public Direction Invert() => new(Horizontal * -1, Vertical * -1);

    public Direction Signed => new(Sign(Horizontal), Sign(Vertical));
    private static int Sign(int value)
    {
        if (value < 0) return -1;
        if (value > 0) return 1;
        return 0;
    }

    #region Operators

    public static Direction operator +(Direction left, Direction right)
    {
        return 
            new
            (
                left.Horizontal + right.Horizontal,
                left.Vertical + right.Vertical
            );
    }

    public static Direction operator -(Direction left, Direction right)
    {
        return 
            new
            (
                left.Horizontal - right.Horizontal,
                left.Vertical - right.Vertical
            );
    }
    public static Direction operator *(Direction left, Direction right)
    {
        return 
            new
            (
                left.Horizontal * right.Horizontal,
                left.Vertical * right.Vertical
            );
    }
    public static Direction operator /(Direction left, Direction right)
    {
        return
            new
            (
                right.Horizontal == 0 ? left.Horizontal : left.Horizontal / right.Horizontal,
                right.Vertical == 0 ? left.Vertical : left.Vertical / right.Vertical
            );
    }
    public static Direction operator +(Direction left, int right)
    {
        return 
            new
            (
                left.Horizontal + right,
                left.Vertical + right
            );
    }
    public static Direction operator -(Direction left, int right)
    {
        return 
            new
            (
                left.Horizontal - right,
                left.Vertical - right
            );
    }
    public static Direction operator *(Direction left, int right)
    {
        return 
            new
            (
                left.Horizontal * right,
                left.Vertical * right
            );
    }
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
    public bool Equals(Direction other) => Horizontal == other.Horizontal && Vertical == other.Vertical;

    public override bool Equals(object? obj) => obj is Direction other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Horizontal, Vertical);
    
    public static bool operator ==(Direction left, Direction right) => left.Equals(right);

    public static bool operator !=(Direction left, Direction right) => !left.Equals(right);

    public override string ToString()
    {
        return $"({Horizontal},{Vertical})";
    }
    #endregion
}