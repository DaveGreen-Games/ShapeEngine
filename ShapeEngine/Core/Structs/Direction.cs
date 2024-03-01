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
        this.Horizontal = Sign(hor);
        this.Vertical = Sign(vert);
    }

    public Direction(Vector2 dir)
    {
        this.Horizontal = MathF.Sign(dir.X);
        this.Vertical = MathF.Sign(dir.X);
    }

    public bool IsValid => IsVertical || IsHorizontal;
    public bool IsVertical => Vertical != 0;
    public bool IsHorizontal => Horizontal != 0;
    
    public bool IsUp => Vertical == -1 && Horizontal == 0;
    public bool IsDown => Vertical == 1 && Horizontal == 0;
    public bool IsLeft => Horizontal == -1 && Vertical == 0;
    public bool IsRight => Horizontal == 1 && Vertical == 0;
    public bool IsUpLeft => Vertical == -1 && Horizontal == -1;
    public bool IsDownLeft => Vertical == 1 && Horizontal == -1;
    public bool IsUpRight => Vertical == -1 && Horizontal == 1;
    public bool IsDownRight => Vertical == 1 && Horizontal == 1;
    
    public Vector2 ToVector2() => new(Horizontal, Vertical);
    public Vector2 ToAlignement() => new Vector2(Horizontal + 1, Vertical + 1) / 2;
    // public Vector2 ToReverseAlignement() => new Vector2((horizontal * -1) + 1, (vertical * -1) + 1) / 2;

    public Direction Invert() => new(Horizontal * -1, Vertical * -1);
    
    public static Direction GetEmpty() => new(0, 0);
    public static Direction GetLeft() => new(-1, 0);
    public static Direction GetRight() => new(1, 0);
    public static Direction GetUp() => new(0, -1);
    public static Direction GetDown() => new(0, 1);
    
    public static Direction GetUpLeft() => new(-1, -1);
    public static Direction GetDownLeft() => new(-1, 1);
    public static Direction GetUpRight() => new(1, -1);
    public static Direction GetDownRight() => new(1, 1);
    
    private static int Sign(int value)
    {
        if (value < 0) return -1;
        if (value > 0) return 1;
        return 0;
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
}