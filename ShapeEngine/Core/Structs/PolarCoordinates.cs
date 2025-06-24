using System.Numerics;
using ShapeEngine.StaticLib;
using ShapeEngine.Random;

namespace ShapeEngine.Core.Structs;

/// <summary>
/// Represents a 2D polar coordinate (radius and angle in radians).
/// Provides methods for conversion, manipulation, and arithmetic in polar space.
/// </summary>
/// <remarks>
/// Polar coordinates are useful for representing positions and directions in circular or angular systems, such as minimaps, radars, and rotations.
/// Angle is always stored in radians and wrapped to [0, 2π).
/// </remarks>
public readonly struct PolarCoordinates
{
    #region Members
    /// <summary>
    /// The angle in radians, wrapped to [0, 2π).
    /// </summary>
    public readonly float AngleRad;
    /// <summary>
    /// The radius (distance from the origin).
    /// </summary>
    public readonly float Radius;
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new instance of <see cref="PolarCoordinates"/> at the origin (radius 0, angle 0).
    /// </summary>
    public PolarCoordinates()
    {
        AngleRad = 0f;
        Radius = 0f;
    }
    /// <summary>
    /// Initializes a new instance of <see cref="PolarCoordinates"/> with a given angle (radians) and unit radius.
    /// </summary>
    /// <param name="angleRad">The angle in radians.</param>
    public PolarCoordinates(float angleRad)
    {
        AngleRad = ShapeMath.WrapAngleRad(angleRad);
        Radius = 1f;
    }
    /// <summary>
    /// Initializes a new instance of <see cref="PolarCoordinates"/> with a given radius and angle (radians).
    /// If radius is negative, the angle is rotated by 180° and radius is made positive.
    /// </summary>
    /// <param name="radius">The radius (distance from origin).</param>
    /// <param name="angleRad">The angle in radians.</param>
    public PolarCoordinates(float radius, float angleRad)
    {
        if (radius < 0)
        {
            //flip it 180° around 
            Radius = radius * -1;
            AngleRad = ShapeMath.WrapAngleRad(angleRad + ShapeMath.PI);
        }
        else
        {
            AngleRad = ShapeMath.WrapAngleRad(angleRad);
            Radius = radius;
        }
        
    }
    /// <summary>
    /// Initializes a new instance of <see cref="PolarCoordinates"/> from a <see cref="Vector2"/>.
    /// </summary>
    /// <param name="v">The vector to convert.</param>
    public PolarCoordinates(Vector2 v)
    {
        Radius = MathF.Sqrt(v.X * v.X + v.Y * v.Y);
        AngleRad = MathF.Atan2(v.Y, v.X);
    }
    #endregion

    #region Public
    /// <summary>
    /// Converts the polar coordinates to a <see cref="Vector2"/>.
    /// </summary>
    /// <returns>The cartesian vector representation.</returns>
    public Vector2 ToVector2()
    {
        var x = Radius * MathF.Cos(AngleRad);
        var y = Radius * MathF.Sin(AngleRad);

        return new(x, y);
    }
    /// <summary>
    /// Converts the polar coordinates to a <see cref="Vector2"/> relative to a given origin.
    /// </summary>
    /// <param name="origin">The origin to offset from.</param>
    /// <returns>The cartesian vector representation relative to <paramref name="origin"/>.</returns>
    public Vector2 ToVector2(Vector2 origin) => origin + ToVector2();
    /// <summary>
    /// Returns a new <see cref="PolarCoordinates"/> flipped by 180° (angle + π).
    /// </summary>
    /// <returns>The flipped polar coordinates.</returns>
    public PolarCoordinates Flip() => new(Radius, AngleRad + ShapeMath.PI);
    /// <summary>
    /// Returns a new <see cref="PolarCoordinates"/> with the angle increased by the specified amount (radians).
    /// </summary>
    /// <param name="amount">The amount to add to the angle, in radians.</param>
    /// <returns>The rotated polar coordinates.</returns>
    public PolarCoordinates ChangeAngleRad(float amount) => new(Radius, AngleRad + amount);
    /// <summary>
    /// Returns a new <see cref="PolarCoordinates"/> with the angle increased by the specified amount (degrees).
    /// </summary>
    /// <param name="amount">The amount to add to the angle, in degrees.</param>
    /// <returns>The rotated polar coordinates.</returns>
    public PolarCoordinates ChangeAngleDeg(float amount) => new(Radius, AngleRad + (amount * ShapeMath.DEGTORAD) );
    /// <summary>
    /// Returns a new <see cref="PolarCoordinates"/> with the radius increased by the specified amount.
    /// </summary>
    /// <param name="amount">The amount to add to the radius.</param>
    /// <returns>The scaled polar coordinates.</returns>
    public PolarCoordinates ChangeRadius(float amount) => new(Radius + amount, AngleRad);
    /// <summary>
    /// Returns a new <see cref="PolarCoordinates"/> with the radius scaled by the specified factor.
    /// </summary>
    /// <param name="factor">The factor to scale the radius by.</param>
    /// <returns>The scaled polar coordinates.</returns>
    public PolarCoordinates ScaleRadius(float factor) => new(Radius * factor, AngleRad);
    /// <summary>
    /// Returns a new <see cref="PolarCoordinates"/> with the angle set to the specified value (radians).
    /// </summary>
    /// <param name="newAngle">The new angle in radians.</param>
    /// <returns>The polar coordinates with updated angle.</returns>
    public PolarCoordinates SetAngleRad(float newAngle)  => new(Radius, ShapeMath.WrapAngleRad(newAngle));
    /// <summary>
    /// Returns a new <see cref="PolarCoordinates"/> with the angle set to the specified value (degrees).
    /// </summary>
    /// <param name="newAngle">The new angle in degrees.</param>
    /// <returns>The polar coordinates with updated angle.</returns>
    public PolarCoordinates SetAngleDeg(float newAngle) => new(Radius, ShapeMath.WrapAngleRad(newAngle * ShapeMath.DEGTORAD) );
    /// <summary>
    /// Returns a new <see cref="PolarCoordinates"/> with the radius set to the specified value.
    /// </summary>
    /// <param name="newRadius">The new radius.</param>
    /// <returns>The polar coordinates with updated radius.</returns>
    public PolarCoordinates SetRadius(float newRadius) => new(newRadius, AngleRad);
    /// <summary>
    /// Transforms a target position to polar coordinates relative to an origin, useful for minimap/radar.
    /// </summary>
    /// <param name="origin">The origin of the radar.</param>
    /// <param name="target">The target location.</param>
    /// <param name="maxRange">The maximum range of the radar.</param>
    /// <returns>Returns relative polar coordinates for the target. The radius is in 0-1 range if target is within max range.</returns>
    public PolarCoordinates Radar(Vector2 origin, Vector2 target, float maxRange)
    {
        if (maxRange <= 0) return new();
        
        var w = target - origin;
        var pc = w.ToPolarCoordinates();
        float f = pc.Radius / maxRange;
        return pc.SetRadius(f);
    }
    #endregion
    
    #region Static
    /// <summary>
    /// Returns a <see cref="PolarCoordinates"/> at the origin (radius 0, angle 0).
    /// </summary>
    public static PolarCoordinates Zero() => new PolarCoordinates();
    /// <summary>
    /// Returns a unit <see cref="PolarCoordinates"/> with the specified angle (radians).
    /// </summary>
    /// <param name="angleRad">The angle in radians.</param>
    public static PolarCoordinates Unit(float angleRad) => new PolarCoordinates(angleRad);
    /// <summary>
    /// Returns a unit <see cref="PolarCoordinates"/> pointing right (angle 0).
    /// </summary>
    public static PolarCoordinates Right => new PolarCoordinates(1f, 0f);
    /// <summary>
    /// Returns a unit <see cref="PolarCoordinates"/> pointing down (angle π/2).
    /// </summary>
    public static PolarCoordinates Down => new PolarCoordinates(1f, ShapeMath.PI / 2);
    /// <summary>
    /// Returns a unit <see cref="PolarCoordinates"/> pointing left (angle π).
    /// </summary>
    public static PolarCoordinates Left => new PolarCoordinates(1f, ShapeMath.PI);
    /// <summary>
    /// Returns a unit <see cref="PolarCoordinates"/> pointing up (angle 3π/2).
    /// </summary>
    public static PolarCoordinates Up => new PolarCoordinates(1f, ShapeMath.PI + ShapeMath.PI / 2);
    /// <summary>
    /// Returns a random <see cref="PolarCoordinates"/> with random radius [0,1) and random angle [0,2π).
    /// </summary>
    public static PolarCoordinates Random() => new PolarCoordinates(Rng.Instance.RandF(), Rng.Instance.RandAngleRad());
    /// <summary>
    /// Returns a random <see cref="PolarCoordinates"/> with radius in [minR, maxR) and random angle [0,2π).
    /// </summary>
    /// <param name="minR">Minimum radius.</param>
    /// <param name="maxR">Maximum radius.</param>
    public static PolarCoordinates Random(float minR, float maxR) => new PolarCoordinates(Rng.Instance.RandF(minR, maxR), Rng.Instance.RandAngleRad());
    /// <summary>
    /// Returns a unit <see cref="Vector2"/> from an angle in radians.
    /// </summary>
    /// <param name="angleRad">The angle in radians.</param>
    /// <returns>The unit vector.</returns>
    public static Vector2 GetVector2(float angleRad) => new Vector2(MathF.Cos(angleRad), MathF.Sin(angleRad));
    /// <summary>
    /// Returns a <see cref="Vector2"/> from a radius and angle in radians.
    /// </summary>
    /// <param name="radius">The radius (length).</param>
    /// <param name="angleRad">The angle in radians.</param>
    /// <returns>The vector.</returns>
    public static Vector2 GetVector2(float radius, float angleRad) => new Vector2(radius * MathF.Cos(angleRad), radius * MathF.Sin(angleRad));
    #endregion

    #region Operators

    /// <summary>
    /// Adds two <see cref="PolarCoordinates"/> (radius and angle are added separately).
    /// </summary>
    /// <param name="left">The first operand.</param>
    /// <param name="right">The second operand.</param>
    /// <returns>The sum of the two polar coordinates.</returns>
    public static PolarCoordinates operator +(PolarCoordinates left, PolarCoordinates right)
    {
        return new
        (
            left.Radius + right.Radius,
            left.AngleRad + right.AngleRad   
        );
    }
    /// <summary>
    /// Subtracts two <see cref="PolarCoordinates"/> (radius and angle are subtracted separately).
    /// </summary>
    /// <param name="left">The first operand.</param>
    /// <param name="right">The second operand.</param>
    /// <returns>The difference of the two polar coordinates.</returns>
    public static PolarCoordinates operator -(PolarCoordinates left, PolarCoordinates right)
    {
        return new
        (
            left.Radius - right.Radius,
            left.AngleRad - right.AngleRad   
        );
    }
    /// <summary>
    /// Multiplies two <see cref="PolarCoordinates"/> (radius and angle are multiplied separately).
    /// </summary>
    /// <param name="left">The first operand.</param>
    /// <param name="right">The second operand.</param>
    /// <returns>The product of the two polar coordinates.</returns>
    public static PolarCoordinates operator *(PolarCoordinates left, PolarCoordinates right)
    {
        return new
        (
            left.Radius * right.Radius,
            left.AngleRad * right.AngleRad   
        );
    }
    /// <summary>
    /// Divides two <see cref="PolarCoordinates"/> (radius and angle are divided separately).
    /// </summary>
    /// <param name="left">The numerator.</param>
    /// <param name="right">The denominator.</param>
    /// <returns>The quotient of the two polar coordinates. If denominator is zero, returns 0 for that component.</returns>
    public static PolarCoordinates operator /(PolarCoordinates left, PolarCoordinates right)
    {
        return new
        (
            right.Radius == 0 ? 0f : left.Radius / right.Radius,
            right.AngleRad == 0 ? 0f : left.AngleRad / right.AngleRad   
        );
    }
    /// <summary>
    /// Adds a <see cref="Vector2"/> to <see cref="PolarCoordinates"/> (converts vector to polar first).
    /// </summary>
    /// <param name="left">The polar coordinates.</param>
    /// <param name="right">The vector to add.</param>
    /// <returns>The sum as polar coordinates.</returns>
    public static PolarCoordinates operator +(PolarCoordinates left, Vector2 right)
    {
        return left + right.ToPolarCoordinates();
    }
    /// <summary>
    /// Subtracts a <see cref="Vector2"/> from <see cref="PolarCoordinates"/> (converts vector to polar first).
    /// </summary>
    /// <param name="left">The polar coordinates.</param>
    /// <param name="right">The vector to subtract.</param>
    /// <returns>The difference as polar coordinates.</returns>
    public static PolarCoordinates operator -(PolarCoordinates left, Vector2 right)
    {
        return left - right.ToPolarCoordinates();
    }
    /// <summary>
    /// Multiplies <see cref="PolarCoordinates"/> by a <see cref="Vector2"/> (converts vector to polar first).
    /// </summary>
    /// <param name="left">The polar coordinates.</param>
    /// <param name="right">The vector to multiply.</param>
    /// <returns>The product as polar coordinates.</returns>
    public static PolarCoordinates operator *(PolarCoordinates left, Vector2 right)
    {
        return left * right.ToPolarCoordinates();
    }
    /// <summary>
    /// Divides <see cref="PolarCoordinates"/> by a <see cref="Vector2"/> (converts vector to polar first).
    /// </summary>
    /// <param name="left">The polar coordinates.</param>
    /// <param name="right">The vector to divide by.</param>
    /// <returns>The quotient as polar coordinates. If denominator is zero, returns 0 for that component.</returns>
    public static PolarCoordinates operator /(PolarCoordinates left, Vector2 right)
    {
        return left / right.ToPolarCoordinates();
    }
    /// <summary>
    /// Adds <see cref="PolarCoordinates"/> to a <see cref="Vector2"/> (converts polar to vector first).
    /// </summary>
    /// <param name="left">The vector.</param>
    /// <param name="right">The polar coordinates to add.</param>
    /// <returns>The sum as a vector.</returns>
    public static Vector2 operator +(Vector2 left, PolarCoordinates right)
    {
        return left + right.ToVector2();
    }
    /// <summary>
    /// Subtracts <see cref="PolarCoordinates"/> from a <see cref="Vector2"/> (converts polar to vector first).
    /// </summary>
    /// <param name="left">The vector.</param>
    /// <param name="right">The polar coordinates to subtract.</param>
    /// <returns>The difference as a vector.</returns>
    public static Vector2 operator -(Vector2 left, PolarCoordinates right)
    {
        return left - right.ToVector2();
    }
    /// <summary>
    /// Multiplies a <see cref="Vector2"/> by <see cref="PolarCoordinates"/> (converts polar to vector first).
    /// </summary>
    /// <param name="left">The vector.</param>
    /// <param name="right">The polar coordinates to multiply.</param>
    /// <returns>The product as a vector.</returns>
    public static Vector2 operator *(Vector2 left, PolarCoordinates right)
    {
        return left * right.ToVector2();
    }
    /// <summary>
    /// Divides a <see cref="Vector2"/> by <see cref="PolarCoordinates"/> (converts polar to vector first).
    /// </summary>
    /// <param name="left">The vector.</param>
    /// <param name="right">The polar coordinates to divide by.</param>
    /// <returns>The quotient as a vector. If denominator is zero, returns 0 for that component.</returns>
    public static Vector2 operator /(Vector2 left, PolarCoordinates right)
    {
        return left / right.ToVector2();
    }
    /// <summary>
    /// Adds a scalar to both the radius and angle of <see cref="PolarCoordinates"/>.
    /// </summary>
    /// <param name="left">The polar coordinates.</param>
    /// <param name="right">The scalar to add.</param>
    /// <returns>The sum as polar coordinates.</returns>
    public static PolarCoordinates operator +(PolarCoordinates left, float right)
    {
        return new
        (
            left.Radius + right,
            left.AngleRad + right   
        );
    }
    /// <summary>
    /// Subtracts a scalar from both the radius and angle of <see cref="PolarCoordinates"/>.
    /// </summary>
    /// <param name="left">The polar coordinates.</param>
    /// <param name="right">The scalar to subtract.</param>
    /// <returns>The difference as polar coordinates.</returns>
    public static PolarCoordinates operator -(PolarCoordinates left, float right)
    {
        return new
        (
            left.Radius - right,
            left.AngleRad - right   
        );
    }
    /// <summary>
    /// Multiplies both the radius and angle of <see cref="PolarCoordinates"/> by a scalar.
    /// </summary>
    /// <param name="left">The polar coordinates.</param>
    /// <param name="right">The scalar to multiply.</param>
    /// <returns>The product as polar coordinates.</returns>
    public static PolarCoordinates operator *(PolarCoordinates left, float right)
    {
        return new
        (
            left.Radius * right,
            left.AngleRad * right   
        );
    }
    /// <summary>
    /// Divides both the radius and angle of <see cref="PolarCoordinates"/> by a scalar.
    /// </summary>
    /// <param name="left">The polar coordinates.</param>
    /// <param name="right">The scalar to divide by.</param>
    /// <returns>The quotient as polar coordinates. If denominator is zero, returns 0 for that component.</returns>
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