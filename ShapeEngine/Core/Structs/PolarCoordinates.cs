using System.Numerics;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.Structs;

public readonly struct PolarCoordinates
{
    
    #region Members
    public readonly float AngleRad;
    public readonly float Radius;
    #endregion

    #region Constructors
    public PolarCoordinates()
    {
        AngleRad = 0f;
        Radius = 0f;
    }
    public PolarCoordinates(float angleRad)
    {
        AngleRad = ShapeMath.WrapAngleRad(angleRad);
        Radius = 1f;
    }
    public PolarCoordinates(float radius, float angleRad)
    {
        AngleRad = ShapeMath.WrapAngleRad(angleRad);
        Radius = radius;
    }
    
    public PolarCoordinates(Vector2 v)
    {
        Radius = MathF.Sqrt(v.X * v.X + v.Y * v.Y);
        AngleRad = MathF.Atan2(v.Y, v.X);
    }
    #endregion

    #region Public
    public Vector2 ToVector2()
    {
        var x = Radius * MathF.Cos(AngleRad);
        var y = Radius * MathF.Sin(AngleRad);

        return new(x, y);
    }

    public Vector2 ToVector2(Vector2 origin) => origin + ToVector2();
    
    public PolarCoordinates ChangeAngleRad(float amount) => new(Radius, ShapeMath.WrapAngleRad(AngleRad + amount));

    public PolarCoordinates ChangeAngleDeg(float amount) => new(Radius, ShapeMath.WrapAngleRad(AngleRad + (amount * ShapeMath.DEGTORAD)) );

    public PolarCoordinates ChangeRadius(float amount)
    {
        float r = Radius + amount;
        if (r < 0)
        {
            return new(r * -1f, ShapeMath.WrapAngleRad(AngleRad + ShapeMath.PI));
        }
        else
        {
            return new(r, AngleRad);
        }
    }
    
    public PolarCoordinates SetAngleRad(float newAngle)  => new(Radius, ShapeMath.WrapAngleRad(newAngle));

    public PolarCoordinates SetAngleDeg(float newAngle) => new(Radius, ShapeMath.WrapAngleRad(newAngle * ShapeMath.DEGTORAD) );

    public PolarCoordinates SetRadius(float newRadius)
    {
        if (newRadius < 0)
        {
            return new(newRadius * -1f, ShapeMath.WrapAngleRad(AngleRad + ShapeMath.PI));
        }
        else
        {
            return new(newRadius, AngleRad);
        }
    }
    #endregion
    
    #region Static
    public static PolarCoordinates Zero() => new PolarCoordinates();
    public static PolarCoordinates Unit(float angleRad) => new PolarCoordinates(angleRad);
    public static PolarCoordinates Right => new PolarCoordinates(1f, 0f);
    public static PolarCoordinates Down => new PolarCoordinates(1f, ShapeMath.PI / 2);
    public static PolarCoordinates Left => new PolarCoordinates(1f, ShapeMath.PI);
    public static PolarCoordinates Up => new PolarCoordinates(1f, ShapeMath.PI + ShapeMath.PI / 2);
    public static PolarCoordinates Random() => new PolarCoordinates(ShapeRandom.RandF(), ShapeRandom.RandAngleRad());
    public static PolarCoordinates Random(float minR, float maxR) => new PolarCoordinates(ShapeRandom.RandF(minR, maxR), ShapeRandom.RandAngleRad());
    public static Vector2 GetVector2(float angleRad) => new Vector2(MathF.Cos(angleRad), MathF.Sin(angleRad));
    public static Vector2 GetVector2(float radius, float angleRad) => new Vector2(radius * MathF.Cos(angleRad), radius * MathF.Sin(angleRad));
    #endregion

    #region Operators

    public static PolarCoordinates operator +(PolarCoordinates left, PolarCoordinates right)
    {
        return new
        (
            left.Radius + right.Radius,
            left.AngleRad + right.AngleRad   
        );
    }
    public static PolarCoordinates operator -(PolarCoordinates left, PolarCoordinates right)
    {
        return new
        (
            left.Radius - right.Radius,
            left.AngleRad - right.AngleRad   
        );
    }
    public static PolarCoordinates operator *(PolarCoordinates left, PolarCoordinates right)
    {
        return new
        (
            left.Radius * right.Radius,
            left.AngleRad * right.AngleRad   
        );
    }
    public static PolarCoordinates operator /(PolarCoordinates left, PolarCoordinates right)
    {
        return new
        (
            right.Radius == 0 ? 0f : left.Radius / right.Radius,
            right.AngleRad == 0 ? 0f : left.AngleRad / right.AngleRad   
        );
    }

    public static PolarCoordinates operator +(PolarCoordinates left, Vector2 right)
    {
        return left + right.ToPolarCoordinates();
    }
    public static PolarCoordinates operator -(PolarCoordinates left, Vector2 right)
    {
        return left - right.ToPolarCoordinates();
    }
    public static PolarCoordinates operator *(PolarCoordinates left, Vector2 right)
    {
        return left * right.ToPolarCoordinates();
    }
    public static PolarCoordinates operator /(PolarCoordinates left, Vector2 right)
    {
        return left / right.ToPolarCoordinates();
    }
   
    public static Vector2 operator +(Vector2 left, PolarCoordinates right)
    {
        return left + right.ToVector2();
    }
    public static Vector2 operator -(Vector2 left, PolarCoordinates right)
    {
        return left - right.ToVector2();
    }
    public static Vector2 operator *(Vector2 left, PolarCoordinates right)
    {
        return left * right.ToVector2();
    }
    public static Vector2 operator /(Vector2 left, PolarCoordinates right)
    {
        return left / right.ToVector2();
    }

    
    public static PolarCoordinates operator +(PolarCoordinates left, float right)
    {
        return new
        (
            left.Radius + right,
            left.AngleRad + right   
        );
    }
    public static PolarCoordinates operator -(PolarCoordinates left, float right)
    {
        return new
        (
            left.Radius - right,
            left.AngleRad - right   
        );
    }
    public static PolarCoordinates operator *(PolarCoordinates left, float right)
    {
        return new
        (
            left.Radius * right,
            left.AngleRad * right   
        );
    }
    public static PolarCoordinates operator /(PolarCoordinates left, float right)
    {
        return new
        (
            right == 0 ? 0f : left.Radius / right,
            right == 0 ? 0f : left.AngleRad / right   
        );
    }

    #endregion
    
}