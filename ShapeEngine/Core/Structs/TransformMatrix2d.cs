using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using ShapeEngine.Lib;
using ShapeVec = ShapeEngine.Lib.ShapeVec;


//Source: https://github.com/godotengine/godot/blob/master/modules/mono/glue/GodotSharp/GodotSharp/Core/TransformMatrix2D.cs


namespace ShapeEngine.Core.Structs;


public struct TransformMatrix2D : IEquatable<TransformMatrix2D>
{
    /// <summary>
    /// The basis matrix's X vector (column 0). Equivalent to array index <c>[0]</c>.
    /// </summary>
    public Vector2 X;

    /// <summary>
    /// The basis matrix's Y vector (column 1). Equivalent to array index <c>[1]</c>.
    /// </summary>
    public Vector2 Y;

    /// <summary>
    /// The origin vector (column 2, the third column). Equivalent to array index <c>[2]</c>.
    /// The origin vector represents translation.
    /// </summary>
    public Vector2 Origin;

    /// <summary>
    /// Returns the transform's rotation (in radians).
    /// </summary>
    public readonly float Rotation => MathF.Atan2(X.Y, X.X);

    /// <summary>
    /// Returns the scale.
    /// </summary>
    public readonly Vector2 Scale
    {
        get
        {
            float detSign = MathF.Sign(BasisDeterminant());
            return new Vector2(X.Length(), detSign * Y.Length());
        }
    }

    /// <summary>
    /// Returns the transform's skew (in radians).
    /// </summary>
    public readonly float Skew
    {
        get
        {
            float detSign = MathF.Sign(BasisDeterminant());

            var xn = ShapeVec.Normalize(X);
            var yn = ShapeVec.Normalize(Y);
            return MathF.Acos(xn.Dot(detSign * yn)) - MathF.PI * 0.5f;
        }
    }

    /// <summary>
    /// Access whole columns in the form of <see cref="Vector2"/>.
    /// The third column is the <see cref="Origin"/> vector.
    /// </summary>
    /// <param name="column">Which column vector.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="column"/> is not 0, 1 or 2.
    /// </exception>
    public Vector2 this[int column]
    {
        readonly get
        {
            switch (column)
            {
                case 0:
                    return X;
                case 1:
                    return Y;
                case 2:
                    return Origin;
                default:
                    throw new ArgumentOutOfRangeException(nameof(column));
            }
        }
        set
        {
            switch (column)
            {
                case 0:
                    X = value;
                    return;
                case 1:
                    Y = value;
                    return;
                case 2:
                    Origin = value;
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(column));
            }
        }
    }

    /// <summary>
    /// Access matrix elements in column-major order.
    /// The third column is the <see cref="Origin"/> vector.
    /// </summary>
    /// <param name="column">Which column, the matrix horizontal position.</param>
    /// <param name="row">Which row, the matrix vertical position.</param>
    public float this[int column, int row]
    {
        readonly get => this[column][row];
        set
        {
            var columnVector = this[column];
            columnVector[row] = value;
            this[column] = columnVector;
        }
    }

    /// <summary>
    /// Returns the inverse of the transform, under the assumption that
    /// the basis is invertible (must have non-zero determinant).
    /// </summary>
    /// <seealso cref="Inverse"/>
    /// <returns>The inverse transformation matrix.</returns>
    public readonly TransformMatrix2D AffineInverse()
    {
        float det = BasisDeterminant();

        if (det == 0)
            throw new InvalidOperationException("Matrix determinant is zero and cannot be inverted.");

        var inv = this;

        inv[0, 0] = this[1, 1];
        inv[1, 1] = this[0, 0];

        float detInv = 1.0f / det;

        inv[0] *= new Vector2(detInv, -detInv);
        inv[1] *= new Vector2(-detInv, detInv);

        inv[2] = inv.BasisXform(-inv[2]);

        return inv;
    }

    /// <summary>
    /// Returns the determinant of the basis matrix. If the basis is
    /// uniformly scaled, its determinant is the square of the scale.
    ///
    /// A negative determinant means the Y scale is negative.
    /// A zero determinant means the basis isn't invertible,
    /// and is usually considered invalid.
    /// </summary>
    /// <returns>The determinant of the basis matrix.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly float BasisDeterminant() => (X.X * Y.Y) - (X.Y * Y.X);

    /// <summary>
    /// Returns a vector transformed (multiplied) by the basis matrix.
    /// This method does not account for translation (the <see cref="Origin"/> vector).
    /// </summary>
    /// <seealso cref="BasisXformInv(Vector2)"/>
    /// <param name="v">A vector to transform.</param>
    /// <returns>The transformed vector.</returns>
    public readonly Vector2 BasisXform(Vector2 v) => new Vector2(Tdotx(v), Tdoty(v));

    /// <summary>
    /// Returns a vector transformed (multiplied) by the inverse basis matrix,
    /// under the assumption that the basis is orthonormal (i.e. rotation/reflection
    /// is fine, scaling/skew is not).
    /// This method does not account for translation (the <see cref="Origin"/> vector).
    /// <c>transform.BasisXformInv(vector)</c> is equivalent to <c>transform.Inverse().BasisXform(vector)</c>. See <see cref="Inverse"/>.
    /// For non-orthonormal transforms (e.g. with scaling) <c>transform.AffineInverse().BasisXform(vector)</c> can be used instead. See <see cref="AffineInverse"/>.
    /// </summary>
    /// <seealso cref="BasisXform(Vector2)"/>
    /// <param name="v">A vector to inversely transform.</param>
    /// <returns>The inversely transformed vector.</returns>
    public readonly Vector2 BasisXformInv(Vector2 v) => new(X.Dot(v), Y.Dot(v));

    /// <summary>
    /// Interpolates this transform to the other <paramref name="transform"/> by <paramref name="weight"/>.
    /// </summary>
    /// <param name="transform">The other transform.</param>
    /// <param name="weight">A value on the range of 0.0 to 1.0, representing the amount of interpolation.</param>
    /// <returns>The interpolated transform.</returns>
    public readonly TransformMatrix2D InterpolateWith(TransformMatrix2D transform, float weight)
    {
        return new TransformMatrix2D
        (
            ShapeMath.LerpAngleRad(Rotation, transform.Rotation, weight),
            Vector2.Lerp(Scale, transform.Scale, weight),
            ShapeMath.LerpAngleRad(Skew, transform.Skew, weight),
            Vector2.Lerp(Origin, transform.Origin, weight)
        );
    }

    /// <summary>
    /// Returns the inverse of the transform, under the assumption that
    /// the transformation basis is orthonormal (i.e. rotation/reflection
    /// is fine, scaling/skew is not). Use <see cref="AffineInverse"/> for
    /// non-orthonormal transforms (e.g. with scaling).
    /// </summary>
    /// <returns>The inverse matrix.</returns>
    public readonly TransformMatrix2D Inverse()
    {
        TransformMatrix2D inv = this;

        // Swap
        inv.X.Y = Y.X;
        inv.Y.X = X.Y;

        inv.Origin = inv.BasisXform(-inv.Origin);

        return inv;
    }

    /// <summary>
    /// Returns <see langword="true"/> if this transform is finite, by calling
    /// <see cref="Mathf.IsFinite(real_t)"/> on each component.
    /// </summary>
    /// <returns>Whether this vector is finite or not.</returns>
    public readonly bool IsFinite() => X.IsFinite() && Y.IsFinite() && Origin.IsFinite();

    /// <summary>
    /// Returns the transform with the basis orthogonal (90 degrees),
    /// and normalized axis vectors (scale of 1 or -1).
    /// </summary>
    /// <returns>The orthonormalized transform.</returns>
    public readonly TransformMatrix2D Orthonormalized()
    {
        TransformMatrix2D ortho = this;

        Vector2 orthoX = ortho.X;
        Vector2 orthoY = ortho.Y;

        orthoX.Normalize();
        orthoY = orthoY - orthoX * orthoX.Dot(orthoY);
        orthoY.Normalize();

        ortho.X = orthoX;
        ortho.Y = orthoY;

        return ortho;
    }

    /// <summary>
    /// Rotates the transform by <paramref name="angleRad"/> (in radians).
    /// The operation is done in the parent/global frame, equivalent to
    /// multiplying the matrix from the left.
    /// </summary>
    /// <param name="angleRad">The angle to rotate, in radians.</param>
    /// <returns>The rotated transformation matrix.</returns>
    public readonly TransformMatrix2D Rotated(float angleRad)
    {
        return new TransformMatrix2D(angleRad, new Vector2()) * this;
    }

    /// <summary>
    /// Rotates the transform by <paramref name="angleRad"/> (in radians).
    /// The operation is done in the local frame, equivalent to
    /// multiplying the matrix from the right.
    /// </summary>
    /// <param name="angleRad">The angle to rotate, in radians.</param>
    /// <returns>The rotated transformation matrix.</returns>
    public readonly TransformMatrix2D RotatedLocal(float angleRad)
    {
        return this * new TransformMatrix2D(angleRad, new Vector2());
    }

    /// <summary>
    /// Scales the transform by the given scaling factor.
    /// The operation is done in the parent/global frame, equivalent to
    /// multiplying the matrix from the left.
    /// </summary>
    /// <param name="scale">The scale to introduce.</param>
    /// <returns>The scaled transformation matrix.</returns>
    public readonly TransformMatrix2D Scaled(Vector2 scale)
    {
        TransformMatrix2D copy = this;
        copy.X *= scale;
        copy.Y *= scale;
        copy.Origin *= scale;
        return copy;
    }

    /// <summary>
    /// Scales the transform by the given scaling factor.
    /// The operation is done in the local frame, equivalent to
    /// multiplying the matrix from the right.
    /// </summary>
    /// <param name="scale">The scale to introduce.</param>
    /// <returns>The scaled transformation matrix.</returns>
    public readonly TransformMatrix2D ScaledLocal(Vector2 scale)
    {
        TransformMatrix2D copy = this;
        copy.X *= scale;
        copy.Y *= scale;
        return copy;
    }

    private readonly float Tdotx(Vector2 with) => (this[0, 0] * with[0]) + (this[1, 0] * with[1]);

    private readonly float Tdoty(Vector2 with) => (this[0, 1] * with[0]) + (this[1, 1] * with[1]);

    /// <summary>
    /// Translates the transform by the given <paramref name="offset"/>.
    /// The operation is done in the parent/global frame, equivalent to
    /// multiplying the matrix from the left.
    /// </summary>
    /// <param name="offset">The offset to translate by.</param>
    /// <returns>The translated matrix.</returns>
    public readonly TransformMatrix2D Translated(Vector2 offset)
    {
        TransformMatrix2D copy = this;
        copy.Origin += offset;
        return copy;
    }

    /// <summary>
    /// Translates the transform by the given <paramref name="offset"/>.
    /// The operation is done in the local frame, equivalent to
    /// multiplying the matrix from the right.
    /// </summary>
    /// <param name="offset">The offset to translate by.</param>
    /// <returns>The translated matrix.</returns>
    public readonly TransformMatrix2D TranslatedLocal(Vector2 offset)
    {
        TransformMatrix2D copy = this;
        copy.Origin += copy.BasisXform(offset);
        return copy;
    }

    // Constants
    private static readonly TransformMatrix2D _identity = new TransformMatrix2D(1, 0, 0, 1, 0, 0);
    private static readonly TransformMatrix2D _flipX = new TransformMatrix2D(-1, 0, 0, 1, 0, 0);
    private static readonly TransformMatrix2D _flipY = new TransformMatrix2D(1, 0, 0, -1, 0, 0);

    /// <summary>
    /// The identity transform, with no translation, rotation, or scaling applied.
    /// This is used as a replacement for <c>TransformMatrix2D()</c> in GDScript.
    /// Do not use <c>new TransformMatrix2D()</c> with no arguments in C#, because it sets all values to zero.
    /// </summary>
    /// <value>Equivalent to <c>new TransformMatrix2D(Vector2.Right, Vector2.Down, Vector2.Zero)</c>.</value>
    public static TransformMatrix2D Identity { get { return _identity; } }
    /// <summary>
    /// The transform that will flip something along the X axis.
    /// </summary>
    /// <value>Equivalent to <c>new TransformMatrix2D(Vector2.Left, Vector2.Down, Vector2.Zero)</c>.</value>
    public static TransformMatrix2D FlipX { get { return _flipX; } }
    /// <summary>
    /// The transform that will flip something along the Y axis.
    /// </summary>
    /// <value>Equivalent to <c>new TransformMatrix2D(Vector2.Right, Vector2.Up, Vector2.Zero)</c>.</value>
    public static TransformMatrix2D FlipY { get { return _flipY; } }

    /// <summary>
    /// Constructs a transformation matrix from 3 vectors (matrix columns).
    /// </summary>
    /// <param name="xAxis">The X vector, or column index 0.</param>
    /// <param name="yAxis">The Y vector, or column index 1.</param>
    /// <param name="originPos">The origin vector, or column index 2.</param>
    public TransformMatrix2D(Vector2 xAxis, Vector2 yAxis, Vector2 originPos)
    {
        X = xAxis;
        Y = yAxis;
        Origin = originPos;
    }

    /// <summary>
    /// Constructs a transformation matrix from the given components.
    /// Arguments are named such that xy is equal to calling <c>X.Y</c>.
    /// </summary>
    /// <param name="xx">The X component of the X column vector, accessed via <c>t.X.X</c> or <c>[0][0]</c>.</param>
    /// <param name="xy">The Y component of the X column vector, accessed via <c>t.X.Y</c> or <c>[0][1]</c>.</param>
    /// <param name="yx">The X component of the Y column vector, accessed via <c>t.Y.X</c> or <c>[1][0]</c>.</param>
    /// <param name="yy">The Y component of the Y column vector, accessed via <c>t.Y.Y</c> or <c>[1][1]</c>.</param>
    /// <param name="ox">The X component of the origin vector, accessed via <c>t.Origin.X</c> or <c>[2][0]</c>.</param>
    /// <param name="oy">The Y component of the origin vector, accessed via <c>t.Origin.Y</c> or <c>[2][1]</c>.</param>
    public TransformMatrix2D(float xx, float xy, float yx, float yy, float ox, float oy)
    {
        X = new Vector2(xx, xy);
        Y = new Vector2(yx, yy);
        Origin = new Vector2(ox, oy);
    }

    /// <summary>
    /// Constructs a transformation matrix from a <paramref name="rotationRad"/> value and
    /// <paramref name="origin"/> vector.
    /// </summary>
    /// <param name="rotationRad">The rotation of the new transform, in radians.</param>
    /// <param name="origin">The origin vector, or column index 2.</param>
    public TransformMatrix2D(float rotationRad, Vector2 origin)
    {
        (float sin, float cos) = MathF.SinCos(rotationRad);
        X.X = Y.Y = cos;
        X.Y = Y.X = sin;
        Y.X *= -1;
        Origin = origin;
    }

    /// <summary>
    /// Constructs a transformation matrix from a <paramref name="rotationRad"/> value,
    /// <paramref name="scale"/> vector, <paramref name="skew"/> value, and
    /// <paramref name="origin"/> vector.
    /// </summary>
    /// <param name="rotationRad">The rotation of the new transform, in radians.</param>
    /// <param name="scale">The scale of the new transform.</param>
    /// <param name="skew">The skew of the new transform, in radians.</param>
    /// <param name="origin">The origin vector, or column index 2.</param>
    public TransformMatrix2D(float rotationRad, Vector2 scale, float skew, Vector2 origin)
    {
        (float rotationSin, float rotationCos) = MathF.SinCos(rotationRad);
        (float rotationSkewSin, float rotationSkewCos) = MathF.SinCos(rotationRad + skew);
        X.X = rotationCos * scale.X;
        Y.Y = rotationSkewCos * scale.Y;
        Y.X = -rotationSkewSin * scale.Y;
        X.Y = rotationSin * scale.X;
        Origin = origin;
    }

    /// <summary>
    /// Composes these two transformation matrices by multiplying them
    /// together. This has the effect of transforming the second transform
    /// (the child) by the first transform (the parent).
    /// </summary>
    /// <param name="left">The parent transform.</param>
    /// <param name="right">The child transform.</param>
    /// <returns>The composed transform.</returns>
    public static TransformMatrix2D operator *(TransformMatrix2D left, TransformMatrix2D right)
    {
        left.Origin = left * right.Origin;

        float x0 = left.Tdotx(right.X);
        float x1 = left.Tdoty(right.X);
        float y0 = left.Tdotx(right.Y);
        float y1 = left.Tdoty(right.Y);

        left.X.X = x0;
        left.X.Y = x1;
        left.Y.X = y0;
        left.Y.Y = y1;

        return left;
    }

    /// <summary>
    /// Returns a Vector2 transformed (multiplied) by the transformation matrix.
    /// </summary>
    /// <param name="transform">The transformation to apply.</param>
    /// <param name="vector">A Vector2 to transform.</param>
    /// <returns>The transformed Vector2.</returns>
    public static Vector2 operator *(TransformMatrix2D transform, Vector2 vector)
    {
        return new Vector2(transform.Tdotx(vector), transform.Tdoty(vector)) + transform.Origin;
    }

    /// <summary>
    /// Returns a Vector2 transformed (multiplied) by the inverse transformation matrix,
    /// under the assumption that the transformation basis is orthonormal (i.e. rotation/reflection
    /// is fine, scaling/skew is not).
    /// <c>vector * transform</c> is equivalent to <c>transform.Inverse() * vector</c>. See <see cref="Inverse"/>.
    /// For transforming by inverse of an affine transformation (e.g. with scaling) <c>transform.AffineInverse() * vector</c> can be used instead. See <see cref="AffineInverse"/>.
    /// </summary>
    /// <param name="vector">A Vector2 to inversely transform.</param>
    /// <param name="transform">The transformation to apply.</param>
    /// <returns>The inversely transformed Vector2.</returns>
    public static Vector2 operator *(Vector2 vector, TransformMatrix2D transform)
    {
        Vector2 vInv = vector - transform.Origin;
        return new Vector2(transform.X.Dot(vInv), transform.Y.Dot(vInv));
    }

    // /// <summary>
    // /// Returns a Rect2 transformed (multiplied) by the transformation matrix.
    // /// </summary>
    // /// <param name="transform">The transformation to apply.</param>
    // /// <param name="rect">A Rect2 to transform.</param>
    // /// <returns>The transformed Rect2.</returns>
    // public static Rect operator *(TransformMatrix2D transform, Rect rect)
    // {
    //     var pos = transform * rect.TopLeft;
    //     var toX = transform.X * rect.Size.Width;
    //     var toY = transform.Y * rect.Size.Height;
    //
    //     return new Rect(pos, new Vector2()).Expand(pos + toX).Expand(pos + toY).Expand(pos + toX + toY);
    // }
    //
    // /// <summary>
    // /// Returns a Rect2 transformed (multiplied) by the inverse transformation matrix,
    // /// under the assumption that the transformation basis is orthonormal (i.e. rotation/reflection
    // /// is fine, scaling/skew is not).
    // /// <c>rect * transform</c> is equivalent to <c>transform.Inverse() * rect</c>. See <see cref="Inverse"/>.
    // /// For transforming by inverse of an affine transformation (e.g. with scaling) <c>transform.AffineInverse() * rect</c> can be used instead. See <see cref="AffineInverse"/>.
    // /// </summary>
    // /// <param name="rect">A Rect2 to inversely transform.</param>
    // /// <param name="transform">The transformation to apply.</param>
    // /// <returns>The inversely transformed Rect2.</returns>
    // public static Rect2 operator *(Rect2 rect, TransformMatrix2D transform)
    // {
    //     Vector2 pos = rect.Position * transform;
    //     Vector2 to1 = new Vector2(rect.Position.X, rect.Position.Y + rect.Size.Y) * transform;
    //     Vector2 to2 = new Vector2(rect.Position.X + rect.Size.X, rect.Position.Y + rect.Size.Y) * transform;
    //     Vector2 to3 = new Vector2(rect.Position.X + rect.Size.X, rect.Position.Y) * transform;
    //
    //     return new Rect2(pos, new Vector2()).Expand(to1).Expand(to2).Expand(to3);
    // }

    
    
    /// <summary>
    /// Returns a copy of the given Vector2[] transformed (multiplied) by the transformation matrix.
    /// </summary>
    /// <param name="transform">The transformation to apply.</param>
    /// <param name="array">A Vector2[] to transform.</param>
    /// <returns>The transformed copy of the Vector2[].</returns>
    public static Vector2[] operator *(TransformMatrix2D transform, Vector2[] array)
    {
        Vector2[] newArray = new Vector2[array.Length];

        for (int i = 0; i < array.Length; i++)
        {
            newArray[i] = transform * array[i];
        }

        return newArray;
    }

    /// <summary>
    /// Returns a copy of the given Vector2[] transformed (multiplied) by the inverse transformation matrix,
    /// under the assumption that the transformation basis is orthonormal (i.e. rotation/reflection
    /// is fine, scaling/skew is not).
    /// <c>array * transform</c> is equivalent to <c>transform.Inverse() * array</c>. See <see cref="Inverse"/>.
    /// For transforming by inverse of an affine transformation (e.g. with scaling) <c>transform.AffineInverse() * array</c> can be used instead. See <see cref="AffineInverse"/>.
    /// </summary>
    /// <param name="array">A Vector2[] to inversely transform.</param>
    /// <param name="transform">The transformation to apply.</param>
    /// <returns>The inversely transformed copy of the Vector2[].</returns>
    public static Vector2[] operator *(Vector2[] array, TransformMatrix2D transform)
    {
        Vector2[] newArray = new Vector2[array.Length];

        for (int i = 0; i < array.Length; i++)
        {
            newArray[i] = array[i] * transform;
        }

        return newArray;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the transforms are exactly equal.
    /// Note: Due to floating-point precision errors, consider using
    /// <see cref="IsEqualApprox"/> instead, which is more reliable.
    /// </summary>
    /// <param name="left">The left transform.</param>
    /// <param name="right">The right transform.</param>
    /// <returns>Whether or not the transforms are exactly equal.</returns>
    public static bool operator ==(TransformMatrix2D left, TransformMatrix2D right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Returns <see langword="true"/> if the transforms are not equal.
    /// Note: Due to floating-point precision errors, consider using
    /// <see cref="IsEqualApprox"/> instead, which is more reliable.
    /// </summary>
    /// <param name="left">The left transform.</param>
    /// <param name="right">The right transform.</param>
    /// <returns>Whether or not the transforms are not equal.</returns>
    public static bool operator !=(TransformMatrix2D left, TransformMatrix2D right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// Returns <see langword="true"/> if the transform is exactly equal
    /// to the given object (<paramref name="obj"/>).
    /// Note: Due to floating-point precision errors, consider using
    /// <see cref="IsEqualApprox"/> instead, which is more reliable.
    /// </summary>
    /// <param name="obj">The object to compare with.</param>
    /// <returns>Whether or not the transform and the object are exactly equal.</returns>
    public override readonly bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is TransformMatrix2D other && Equals(other);
    }

    /// <summary>
    /// Returns <see langword="true"/> if the transforms are exactly equal.
    /// Note: Due to floating-point precision errors, consider using
    /// <see cref="IsEqualApprox"/> instead, which is more reliable.
    /// </summary>
    /// <param name="other">The other transform to compare.</param>
    /// <returns>Whether or not the matrices are exactly equal.</returns>
    public readonly bool Equals(TransformMatrix2D other)
    {
        return X.Equals(other.X) && Y.Equals(other.Y) && Origin.Equals(other.Origin);
    }

    /// <summary>
    /// Returns <see langword="true"/> if this transform and <paramref name="other"/> are approximately equal,
    /// by running <see cref="Vector2.IsEqualApprox(Vector2)"/> on each component.
    /// </summary>
    /// <param name="other">The other transform to compare.</param>
    /// <returns>Whether or not the matrices are approximately equal.</returns>
    public readonly bool IsEqualApprox(TransformMatrix2D other)
    {
        return X.IsSimilar(other.X) && Y.IsSimilar(other.Y) && Origin.IsSimilar(other.Origin);
    }

    /// <summary>
    /// Serves as the hash function for <see cref="TransformMatrix2D"/>.
    /// </summary>
    /// <returns>A hash code for this transform.</returns>
    public readonly override int GetHashCode() => HashCode.Combine(X, Y, Origin);

    /// <summary>
    /// Converts this <see cref="TransformMatrix2D"/> to a string.
    /// </summary>
    /// <returns>A string representation of this transform.</returns>
    public readonly override string ToString() => ToString(null);

    /// <summary>
    /// Converts this <see cref="TransformMatrix2D"/> to a string with the given <paramref name="format"/>.
    /// </summary>
    /// <returns>A string representation of this transform.</returns>
    public readonly string ToString(string? format) => $"[X: {X.ToString(format)}, Y: {Y.ToString(format)}, O: {Origin.ToString(format)}]";
}







    
    
    
    
    
    
    