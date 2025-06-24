using System.Numerics;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Core.Structs;

/// <summary>
/// Represents a 2D transformation, including position, rotation, base size, and scale.
/// Immutable and implements value equality.
/// </summary>
public readonly struct Transform2D : IEquatable<Transform2D>
{
    #region Members

    /// <summary>
    /// The position of the transform in 2D space.
    /// </summary>
    public readonly Vector2 Position;

    /// <summary>
    /// The rotation of the transform in radians.
    /// </summary>
    public readonly float RotationRad;

    /// <summary>
    /// The base size of the transform before scaling.
    /// </summary>
    public readonly Size BaseSize;

    /// <summary>
    /// The size of the transform after scaling.
    /// </summary>
    public readonly Size ScaledSize;

    /// <summary>
    /// The 2D scale factors for the transform.
    /// </summary>
    public readonly Vector2 Scale2d;

    /// <summary>
    /// Is the same as ScaleX. Can be used if only 1 component scale is needed.
    /// </summary>
    public float Scale => Scale2d.X;

    /// <summary>
    /// The scale factor along the X axis.
    /// </summary>
    public float ScaleX => Scale2d.X;

    /// <summary>
    /// The scale factor along the Y axis.
    /// </summary>
    public float ScaleY => Scale2d.Y;

    /// <summary>
    /// The rotation of the transform in degrees.
    /// </summary>
    public float RotationDeg => RotationRad * ShapeMath.RADTODEG;

    /// <summary>
    /// Gets the direction vector from the current rotation.
    /// </summary>
    public Vector2 GetDirection() => ShapeVec.VecFromAngleRad(RotationRad);

    /// <summary>
    /// Determines whether the transform is empty (zero position, zero rotation, and zero size).
    /// </summary>
    public bool IsEmpty() => Position == Vector2.Zero && RotationRad == 0f && BaseSize == Size.Zero;

    #endregion
    
    #region Constructors
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Transform2D"/> struct with default values.
    /// Position is (0,0), rotation is 0 radians, base size is 0, and scale is (1,1).
    /// </summary>
    public Transform2D()
    {
        this.Position = new(0f);
        this.RotationRad = 0f;
        this.BaseSize = new(0f);
        this.Scale2d = new(1f, 1f);
        this.ScaledSize = this.BaseSize * this.Scale2d;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Transform2D"/> struct with a specified position.
    /// Rotation is 0 radians, base size is 0, and scale is (1,1).
    /// </summary>
    /// <param name="pos">The position of the transform.</param>
    public Transform2D(Vector2 pos)
    {
        this.Position = pos;
        this.RotationRad = 0f;
        this.BaseSize = new(0f);
        this.Scale2d = new(1f, 1f);
        this.ScaledSize = this.BaseSize * this.Scale2d;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Transform2D"/> struct with a specified rotation in radians.
    /// Position is (0,0), base size is 0, and scale is (1,1).
    /// </summary>
    /// <param name="rotRad">The rotation in radians.</param>
    public Transform2D(float rotRad)
    {
        this.Position = new(0f);
        this.RotationRad = rotRad;
        this.BaseSize = new(0f);
        this.Scale2d = new(1f, 1f);
        this.ScaledSize = this.BaseSize * this.Scale2d;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Transform2D"/> struct with a specified position and rotation.
    /// Base size is 0, and scale is (1,1).
    /// </summary>
    /// <param name="pos">The position of the transform.</param>
    /// <param name="rotRad">The rotation in radians.</param>
    public Transform2D(Vector2 pos, float rotRad)
    {
        this.Position = pos; 
        this.RotationRad = rotRad;
        this.BaseSize = new(0f);
        this.Scale2d = new(1f, 1f);
        this.ScaledSize = this.BaseSize * this.Scale2d;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Transform2D"/> struct with a specified position, rotation, and uniform scale.
    /// Base size is 0.
    /// </summary>
    /// <param name="pos">The position of the transform.</param>
    /// <param name="rotRad">The rotation in radians.</param>
    /// <param name="scale">The uniform scale factor.</param>
    public Transform2D(Vector2 pos, float rotRad, float scale)
    {
        this.Position = pos; 
        this.RotationRad = rotRad;
        this.BaseSize = new(0f);
        this.Scale2d = new(scale, scale);
        this.ScaledSize = this.BaseSize * this.Scale2d;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Transform2D"/> struct with a specified position, rotation, and base size.
    /// Scale is (1,1).
    /// </summary>
    /// <param name="pos">The position of the transform.</param>
    /// <param name="rotRad">The rotation in radians.</param>
    /// <param name="baseSize">The base size before scaling.</param>
    public Transform2D(Vector2 pos, float rotRad, Size baseSize)
    {
        this.Position = pos; 
        this.RotationRad = rotRad; 
        this.BaseSize = baseSize;
        this.Scale2d = new(1f, 1f);
        this.ScaledSize = this.BaseSize * this.Scale2d;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Transform2D"/> struct with a specified position, rotation, base size, and uniform scale.
    /// </summary>
    /// <param name="pos">The position of the transform.</param>
    /// <param name="rotRad">The rotation in radians.</param>
    /// <param name="baseSize">The base size before scaling.</param>
    /// <param name="scale">The uniform scale factor.</param>
    public Transform2D(Vector2 pos, float rotRad, Size baseSize, float scale)
    {
        this.Position = pos; 
        this.RotationRad = rotRad; 
        this.BaseSize = baseSize;
        this.Scale2d = new(scale, scale);
        this.ScaledSize = this.BaseSize * this.Scale2d;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Transform2D"/> struct with a specified position, rotation, and non-uniform scale.
    /// Base size is 0.
    /// </summary>
    /// <param name="pos">The position of the transform.</param>
    /// <param name="rotRad">The rotation in radians.</param>
    /// <param name="scaleX">The scale factor along the X axis.</param>
    /// <param name="scaleY">The scale factor along the Y axis.</param>
    public Transform2D(Vector2 pos, float rotRad, float scaleX, float scaleY)
    {
        this.Position = pos; 
        this.RotationRad = rotRad;
        this.BaseSize = new(0f);
        this.Scale2d = new(scaleX, scaleY);
        this.ScaledSize = this.BaseSize * this.Scale2d;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Transform2D"/> struct with a specified position, rotation, base size, and non-uniform scale.
    /// </summary>
    /// <param name="pos">The position of the transform.</param>
    /// <param name="rotRad">The rotation in radians.</param>
    /// <param name="baseSize">The base size before scaling.</param>
    /// <param name="scaleX">The scale factor along the X axis.</param>
    /// <param name="scaleY">The scale factor along the Y axis.</param>
    public Transform2D(Vector2 pos, float rotRad, Size baseSize, float scaleX, float scaleY)
    {
        this.Position = pos; 
        this.RotationRad = rotRad; 
        this.BaseSize = baseSize;
        this.Scale2d = new(scaleX, scaleY);
        this.ScaledSize = this.BaseSize * this.Scale2d;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Transform2D"/> struct with a specified position, rotation, base size, and scale vector.
    /// </summary>
    /// <param name="pos">The position of the transform.</param>
    /// <param name="rotRad">The rotation in radians.</param>
    /// <param name="baseSize">The base size before scaling.</param>
    /// <param name="scale2d">The 2D scale vector.</param>
    public Transform2D(Vector2 pos, float rotRad, Size baseSize, Vector2 scale2d)
    {
        this.Position = pos; 
        this.RotationRad = rotRad; 
        this.BaseSize = baseSize;
        this.Scale2d = scale2d;
        this.ScaledSize = this.BaseSize * this.Scale2d;
    }
    #endregion

    #region Lerp

    /// <summary>
    /// Linearly interpolates between this transform and the target transform.
    /// Interpolates position, rotation (in radians), and base size using the given factor.
    /// </summary>
    /// <param name="to">The target <see cref="Transform2D"/> to interpolate towards.</param>
    /// <param name="f">The interpolation factor (0 = this, 1 = to).</param>
    /// <returns>A new <see cref="Transform2D"/> representing the interpolated state.</returns>
    public Transform2D Lerp(Transform2D to, float f)
    {
        return new Transform2D
        (
            Position.Lerp(to.Position, f),
            ShapeMath.LerpFloat(RotationRad, to.RotationRad, f),
            BaseSize.Lerp(to.BaseSize, f)
        );
    }

    /// <summary>
    /// Performs a framerate-independent interpolation between this transform and the target transform using exponential decay (MathF.Pow).
    /// This method is more expensive than <see cref="ExpDecayLerpComplex"/>.
    /// </summary>
    /// <param name="to">The target <see cref="Transform2D"/> to interpolate towards.</param>
    /// <param name="remainder">Fraction to remain after 1 second (0-1).</param>
    /// <param name="dt">Delta time in seconds.</param>
    /// <returns>A new <see cref="Transform2D"/> representing the interpolated state.</returns>
    public Transform2D PowLerp(Transform2D to, float remainder, float dt)
    {
        var scalar = MathF.Pow(remainder, dt);
        
        return new Transform2D
        (
            Position + (to.Position - Position) * scalar,
            RotationRad + (to.RotationRad - RotationRad) * scalar,
            BaseSize + (to.BaseSize - BaseSize) * scalar
        );
    }

    /// <summary>
    /// Performs a framerate-independent interpolation between this transform and the target transform using exponential decay (MathF.Exp).
    /// Allows specifying the decay rate directly.
    /// </summary>
    /// <param name="to">The target <see cref="Transform2D"/> to interpolate towards.</param>
    /// <param name="decay">Decay value (recommended between 1 and 25).</param>
    /// <param name="dt">Delta time in seconds.</param>
    /// <returns>A new <see cref="Transform2D"/> representing the interpolated state.</returns>
    public Transform2D ExpDecayLerpComplex(Transform2D to, float decay, float dt)
    {
        var scalar = MathF.Exp(-decay * dt);
        
        return new Transform2D
        (
            Position + (to.Position - Position) * scalar,
            RotationRad + (to.RotationRad - RotationRad) * scalar,
            BaseSize + (to.BaseSize - BaseSize) * scalar
        );
    }

    /// <summary>
    /// Performs a framerate-independent interpolation between this transform and the target transform using exponential decay.
    /// The decay value is interpolated between 1 and 25 based on the given fraction.
    /// </summary>
    /// <param name="to">The target <see cref="Transform2D"/> to interpolate towards.</param>
    /// <param name="f">Fraction (0-1) used to interpolate the decay value.</param>
    /// <param name="dt">Delta time in seconds.</param>
    /// <returns>A new <see cref="Transform2D"/> representing the interpolated state.</returns>
    public Transform2D ExpDecayLerp(Transform2D to, float f, float dt)
    {
        var decay = ShapeMath.LerpFloat(1, 25, f);
        var scalar = MathF.Exp(-decay * dt);
        
        return new Transform2D
        (
            Position + (to.Position - Position) * scalar,
            RotationRad + (to.RotationRad - RotationRad) * scalar,
            BaseSize + (to.BaseSize - BaseSize) * scalar
        );
    }

    #endregion
        
    #region Math

    /// <summary>
    /// Calculates the absolute transform for a child object based on its relative offset and the parent’s absolute transform.
    /// </summary>
    /// <param name="parentTransform">The absolute <see cref="Transform2D"/> of the parent.</param>
    /// <param name="childOffset">The relative <see cref="Transform2D"/> offset of the child to the parent.</param>
    /// <param name="moves">If true, the child moves with the parent; if false, the child remains at a fixed position.</param>
    /// <param name="rotates">If true, the child rotates with the parent; if false, the child retains its own rotation.</param>
    /// <param name="scales">If true, the child scales with the parent; if false, the child retains its own scale and size.</param>
    /// <returns>The absolute <see cref="Transform2D"/> of the child, computed from the parent transform and the child offset.</returns>
    public static Transform2D UpdateTransform(Transform2D parentTransform, Transform2D childOffset, bool moves = true, bool rotates = true, bool scales = true)
    {
        var rot = rotates ? parentTransform.RotationRad + childOffset.RotationRad : childOffset.RotationRad;
        var size = scales ? parentTransform.BaseSize + childOffset.BaseSize : childOffset.BaseSize;
        var scale = scales ? parentTransform.Scale2d * childOffset.Scale2d : childOffset.Scale2d;
        if (moves)
        {
            if (childOffset.Position.LengthSquared() <= 0) return new(parentTransform.Position, rot, size, scale);
            
            var pos = parentTransform.Position + childOffset.Position.Rotate(rot) * scale;
            return new(pos, rot, size, scale);
        }
        return new(childOffset.Position, rot, size, scale);
    }
    
    /// <summary>
    /// Converts a relative transform (offset) into an absolute transform for a child object,
    /// using this instance as the parent transform.
    /// </summary>
    /// <param name="childOffset">The relative <see cref="Transform2D"/> offset of the child with respect to the parent.</param>
    /// <returns>
    /// The absolute <see cref="Transform2D"/> for the child, calculated by applying the offset to the parent transform.
    /// </returns>
    public Transform2D GetChildTransform(Transform2D childOffset)
    {
        var rot = RotationRad + childOffset.RotationRad;
        var size = BaseSize + childOffset.BaseSize;
        var scale = Scale2d * childOffset.Scale2d ;
        if (childOffset.Position.LengthSquared() <= 0) return new(Position, rot, size, scale);
        
        var pos = Position + childOffset.Position.Rotate(rot) * scale;
        return new(pos, rot, size, scale);
    }
  
    /// <summary>
    /// Reverts a transformed position back to the local space of this transform.
    /// </summary>
    /// <param name="position">The world position to revert.</param>
    /// <returns>The position in the local space of this transform.</returns>
    public Vector2 RevertPosition(Vector2 position)
    {
        var w = (position - Position).Rotate(-RotationRad) / ScaledSize.Length;
        return Position + w;
    }
    
    /// <summary>
    /// Applies this transform to a relative position, converting it to world space.
    /// </summary>
    /// <param name="relative">The position relative to this transform.</param>
    /// <returns>The transformed position in world space.</returns>
    public Vector2 ApplyTransformTo(Vector2 relative)
    {
        if (relative.LengthSquared() == 0f) return Position;
        return Position + (relative * ScaledSize.Length).Rotate(RotationRad);
    }
    
    /// <summary>
    /// Returns a new <see cref="Transform2D"/> with the position changed by the specified amount.
    /// </summary>
    /// <param name="amount">The amount to add to the position.</param>
    /// <returns>A new <see cref="Transform2D"/> with the updated position.</returns>
    public Transform2D ChangePosition(Vector2 amount) => new(Position + amount, RotationRad, BaseSize, Scale2d);
    
    /// <summary>
    /// Returns a new <see cref="Transform2D"/> with the X component of the position changed by the specified amount.
    /// </summary>
    /// <param name="amount">The amount to add to the X position.</param>
    /// <returns>A new <see cref="Transform2D"/> with the updated X position.</returns>
    public Transform2D ChangePositionX(float amount) => new(Position with { X = Position.X + amount }, RotationRad, BaseSize, Scale2d);
    
    /// <summary>
    /// Returns a new <see cref="Transform2D"/> with the Y component of the position changed by the specified amount.
    /// </summary>
    /// <param name="amount">The amount to add to the Y position.</param>
    /// <returns>A new <see cref="Transform2D"/> with the updated Y position.</returns>
    public Transform2D ChangePositionY(float amount) => new(Position with { Y = Position.Y + amount }, RotationRad, BaseSize, Scale2d);
    
    /// <summary>
    /// Returns a new <see cref="Transform2D"/> with the base size changed by the specified amount.
    /// </summary>
    /// <param name="amount">The amount to add to the base size.</param>
    /// <returns>A new <see cref="Transform2D"/> with the updated base size.</returns>
    public Transform2D ChangeSize(Size amount) => new(Position, RotationRad, BaseSize + amount, Scale2d);
    
    /// <summary>
    /// Returns a new <see cref="Transform2D"/> with the base size changed by the specified scalar amount (applied to both dimensions).
    /// </summary>
    /// <param name="amount">The scalar amount to add to the base size.</param>
    /// <returns>A new <see cref="Transform2D"/> with the updated base size.</returns>
    public Transform2D ChangeSize(float amount) => new(Position, RotationRad, BaseSize + new Vector2(amount), Scale2d);
    
    /// <summary>
    /// Returns a new <see cref="Transform2D"/> with the width of the base size changed by the specified amount.
    /// </summary>
    /// <param name="amount">The amount to add to the width.</param>
    /// <returns>A new <see cref="Transform2D"/> with the updated width.</returns>
    public Transform2D ChangeSizeX(float amount) => new(Position, RotationRad, new Size(Position.X + amount, BaseSize.Height), Scale2d);
    
    /// <summary>
    /// Returns a new <see cref="Transform2D"/> with the height of the base size changed by the specified amount.
    /// </summary>
    /// <param name="amount">The amount to add to the height.</param>
    /// <returns>A new <see cref="Transform2D"/> with the updated height.</returns>
    public Transform2D ChangeSizeY(float amount) => new(Position, RotationRad, new Size(BaseSize.Width, Position.Y + amount), Scale2d);
    
    /// <summary>
    /// Returns a new <see cref="Transform2D"/> with the scale multiplied by the specified factor (applied to both X and Y).
    /// </summary>
    /// <param name="factor">The scale factor.</param>
    /// <returns>A new <see cref="Transform2D"/> with the updated scale.</returns>
    public Transform2D MultiplyScale(float factor) => new(Position, RotationRad, BaseSize, Scale2d * factor);
    
    /// <summary>
    /// Returns a new <see cref="Transform2D"/> with the scale changed by the specified amount (added to both X and Y).
    /// </summary>
    /// <param name="amount">The amount to add to the scale.</param>
    /// <returns>A new <see cref="Transform2D"/> with the updated scale.</returns>
    public Transform2D ChangeScale(float amount) => new(Position, RotationRad, BaseSize, Scale2d.X + amount, Scale2d.Y + amount);
    
    /// <summary>
    /// Returns a new <see cref="Transform2D"/> with the scale multiplied by the specified 2D vector.
    /// </summary>
    /// <param name="factor">The 2D scale factor.</param>
    /// <returns>A new <see cref="Transform2D"/> with the updated scale.</returns>
    public Transform2D MultiplyScale2d(Vector2 factor) => new(Position, RotationRad, BaseSize, Scale2d * factor);
    
    /// <summary>
    /// Returns a new <see cref="Transform2D"/> with the scale changed by the specified 2D amount (added to both X and Y).
    /// </summary>
    /// <param name="amount">The 2D amount to add to the scale.</param>
    /// <returns>A new <see cref="Transform2D"/> with the updated scale.</returns>
    public Transform2D ChangeScale2d(Vector2 amount) => new(Position, RotationRad, BaseSize, Scale2d + amount);
    
    /// <summary>
    /// Returns a new <see cref="Transform2D"/> with the rotation in radians changed by the specified amount.
    /// </summary>
    /// <param name="amount">The amount to add to the rotation in radians.</param>
    /// <returns>A new <see cref="Transform2D"/> with the updated rotation.</returns>
    public Transform2D ChangeRotationRad(float amount) => new(Position, RotationRad + amount, BaseSize, Scale2d);
    
    /// <summary>
    /// Returns a new <see cref="Transform2D"/> with the rotation in degrees changed by the specified amount.
    /// </summary>
    /// <param name="amount">The amount to add to the rotation in degrees.</param>
    /// <returns>A new <see cref="Transform2D"/> with the updated rotation.</returns>
    public Transform2D ChangeRotationDeg(float amount) => new(Position, RotationRad + (amount * ShapeMath.DEGTORAD), BaseSize, Scale2d);
    
    /// <summary>
    /// Returns a new <see cref="Transform2D"/> with the position set to the specified value.
    /// </summary>
    /// <param name="newPosition">The new position.</param>
    /// <returns>A new <see cref="Transform2D"/> with the updated position.</returns>
    public Transform2D SetPosition(Vector2 newPosition) => new(newPosition, RotationRad, BaseSize, Scale2d);
    
    /// <summary>
    /// Returns a new <see cref="Transform2D"/> with the rotation in radians set to the specified value.
    /// </summary>
    /// <param name="newRotationRad">The new rotation in radians.</param>
    /// <returns>A new <see cref="Transform2D"/> with the updated rotation.</returns>
    public Transform2D SetRotationRad(float newRotationRad) => new(Position, newRotationRad, BaseSize, Scale2d);
    
    /// <summary>
    /// Returns a new <see cref="Transform2D"/> with the rotation in degrees set to the specified value.
    /// </summary>
    /// <param name="newRotationDeg">The new rotation in degrees.</param>
    /// <returns>A new <see cref="Transform2D"/> with the updated rotation.</returns>
    public Transform2D SetRotationDeg(float newRotationDeg) => new(Position, newRotationDeg * ShapeMath.DEGTORAD, BaseSize, Scale2d);
    
    /// <summary>
    /// Returns a new <see cref="Transform2D"/> with the rotation wrapped to the range [0, 2π).
    /// </summary>
    /// <returns>A new <see cref="Transform2D"/> with the wrapped rotation.</returns>
    public Transform2D WrapRotationRad() => new(Position, ShapeMath.WrapAngleRad(RotationRad), BaseSize, Scale2d);
    
    /// <summary>
    /// Returns a new <see cref="Transform2D"/> with the base size set to the specified value.
    /// </summary>
    /// <param name="newSize">The new base size.</param>
    /// <returns>A new <see cref="Transform2D"/> with the updated base size.</returns>
    public Transform2D SetSize(Size newSize) => new(Position, RotationRad, newSize, Scale2d);
    
    /// <summary>
    /// Returns a new <see cref="Transform2D"/> with the base size set to the specified scalar value (applied to both dimensions).
    /// </summary>
    /// <param name="newSize">The new base size as a scalar.</param>
    /// <returns>A new <see cref="Transform2D"/> with the updated base size.</returns>
    public Transform2D SetSize(float newSize) => new(Position, RotationRad, new(newSize), Scale2d);
    
    /// <summary>
    /// Returns a new <see cref="Transform2D"/> with the scale set to the specified value (applied to both X and Y).
    /// </summary>
    /// <param name="newScale">The new scale value.</param>
    /// <returns>A new <see cref="Transform2D"/> with the updated scale.</returns>
    public Transform2D SetScale(float newScale) => new(Position, RotationRad, BaseSize, newScale);
    
    /// <summary>
    /// Returns a new <see cref="Transform2D"/> with the specified offset added to position, rotation, base size, and scale.
    /// </summary>
    /// <remarks>
    ///<code>
    /// Position + offset.Position,
    /// RotationRad + offset.RotationRad,
    /// BaseSize + offset.BaseSize,
    /// Scale2d * offset.Scale2d
    ///</code>
    /// </remarks>
    /// <param name="offset">The offset to add.</param>
    /// <returns>A new <see cref="Transform2D"/> with the offset applied.</returns>
    public Transform2D AddOffset(Transform2D offset)
    {
        return new
        (
            Position + offset.Position,
            RotationRad + offset.RotationRad,
            BaseSize + offset.BaseSize,
            Scale2d * offset.Scale2d
        );
    }
    
    /// <summary>
    /// Returns a new <see cref="Transform2D"/> with the specified offset subtracted from position, rotation, base size, and scale.
    /// </summary>
    /// <remarks>
    ///<code>
    /// Position - offset.Position,
    /// RotationRad - offset.RotationRad,
    /// BaseSize - offset.BaseSize,
    /// Scale2d.DivideSafe(offset.Scale2d)
    ///</code>
    /// </remarks>
    /// <param name="offset">The offset to remove.</param>
    /// <returns>A new <see cref="Transform2D"/> with the offset removed.</returns>
    public Transform2D RemoveOffset(Transform2D offset)
    {
        return new
        (
            Position - offset.Position,
            RotationRad - offset.RotationRad,
            BaseSize - offset.BaseSize,
            Scale2d.DivideSafe(offset.Scale2d)
        );
    }
    #endregion

    #region Operators

    /// <summary>
    /// Multiplies all components (position, rotation, base size, and scale) of this <see cref="Transform2D"/> by the specified factor.
    /// </summary>
    /// <param name="factor">The scalar value to multiply each component by.</param>
    /// <returns>A new <see cref="Transform2D"/> with all components multiplied by the factor.</returns>
    public readonly Transform2D Multiply(float factor)
    {
        return new
        (
            Position * factor,
            RotationRad * factor,
            BaseSize * factor,
            Scale2d * factor
        );
    }

    /// <summary>
    /// Divides all components (position, rotation, base size, and scale) of this <see cref="Transform2D"/> by the specified divisor.
    /// If the divisor is zero, returns this instance unchanged.
    /// </summary>
    /// <param name="divisor">The scalar value to divide each component by.</param>
    /// <returns>A new <see cref="Transform2D"/> with all components divided by the divisor, or this instance if divisor is zero.</returns>
    public readonly Transform2D Divide(float divisor)
    {
        if(divisor == 0) return this;
        return new
        (
            Position / divisor,
            RotationRad / divisor,
            BaseSize / divisor,
            Scale2d / divisor
        );
    }

    /// <summary>
    /// Adds the corresponding components of two <see cref="Transform2D"/> instances.
    /// </summary>
    /// <param name="left">The first <see cref="Transform2D"/>.</param>
    /// <param name="right">The second <see cref="Transform2D"/>.</param>
    /// <returns>A new <see cref="Transform2D"/> representing the sum.</returns>
    public static Transform2D operator +(Transform2D left, Transform2D right)
    {
        return new
        (
            left.Position + right.Position,
            left.RotationRad + right.RotationRad,
            left.BaseSize + right.BaseSize,
            left.Scale2d + right.Scale2d
        );
    }

    /// <summary>
    /// Subtracts the corresponding components of two <see cref="Transform2D"/> instances.
    /// </summary>
    /// <param name="left">The first <see cref="Transform2D"/>.</param>
    /// <param name="right">The second <see cref="Transform2D"/>.</param>
    /// <returns>A new <see cref="Transform2D"/> representing the difference.</returns>
    public static Transform2D operator -(Transform2D left, Transform2D right)
    {
        return new
        (
            left.Position - right.Position,
            left.RotationRad - right.RotationRad,
            left.BaseSize - right.BaseSize,
            left.Scale2d - right.Scale2d
        );
    }

    /// <summary>
    /// Divides the corresponding components of two <see cref="Transform2D"/> instances.
    /// Uses DivideSafe for scale to avoid division by zero.
    /// </summary>
    /// <param name="left">The dividend <see cref="Transform2D"/>.</param>
    /// <param name="right">The divisor <see cref="Transform2D"/>.</param>
    /// <returns>A new <see cref="Transform2D"/> representing the quotient.</returns>
    public static Transform2D operator /(Transform2D left, Transform2D right)
    {
        return new
        (
            left.Position / right.Position,
            left.RotationRad / right.RotationRad,
            left.BaseSize / right.BaseSize,
            left.Scale2d.DivideSafe(right.Scale2d)
        );
    }

    /// <summary>
    /// Multiplies the corresponding components of two <see cref="Transform2D"/> instances.
    /// </summary>
    /// <param name="left">The first <see cref="Transform2D"/>.</param>
    /// <param name="right">The second <see cref="Transform2D"/>.</param>
    /// <returns>A new <see cref="Transform2D"/> representing the product.</returns>
    public static Transform2D operator *(Transform2D left, Transform2D right)
    {
        return new
        (
            left.Position * right.Position,
            left.RotationRad * right.RotationRad,
            left.BaseSize * right.BaseSize,
            left.Scale2d * right.Scale2d
        );
    }

    /// <summary>
    /// Adds a <see cref="Vector2"/> to the position component only.
    /// </summary>
    /// <param name="left">The <see cref="Transform2D"/>.</param>
    /// <param name="right">The <see cref="Vector2"/> to add to the position.</param>
    /// <returns>A new <see cref="Transform2D"/> with updated position.</returns>
    public static Transform2D operator +(Transform2D left, Vector2 right)
    {
        return new
        (
            left.Position + right,
            left.RotationRad,
            left.BaseSize,
            left.Scale2d
        );
    }

    /// <summary>
    /// Subtracts a <see cref="Vector2"/> from the position component only.
    /// </summary>
    /// <param name="left">The <see cref="Transform2D"/>.</param>
    /// <param name="right">The <see cref="Vector2"/> to subtract from the position.</param>
    /// <returns>A new <see cref="Transform2D"/> with updated position.</returns>
    public static Transform2D operator -(Transform2D left, Vector2 right)
    {
        return new
        (
            left.Position - right,
            left.RotationRad,
            left.BaseSize,
            left.Scale2d
        );
    }

    /// <summary>
    /// Multiplies the position component by a <see cref="Vector2"/>.
    /// </summary>
    /// <param name="left">The <see cref="Transform2D"/>.</param>
    /// <param name="right">The <see cref="Vector2"/> to multiply the position by.</param>
    /// <returns>A new <see cref="Transform2D"/> with updated position.</returns>
    public static Transform2D operator *(Transform2D left, Vector2 right)
    {
        return new
        (
            left.Position * right,
            left.RotationRad,
            left.BaseSize,
            left.Scale2d
        );
    }

    /// <summary>
    /// Divides the position component by a <see cref="Vector2"/>.
    /// </summary>
    /// <param name="left">The <see cref="Transform2D"/>.</param>
    /// <param name="right">The <see cref="Vector2"/> to divide the position by.</param>
    /// <returns>A new <see cref="Transform2D"/> with updated position.</returns>
    public static Transform2D operator /(Transform2D left, Vector2 right)
    {
        return new
        (
            left.Position / right,
            left.RotationRad,
            left.BaseSize,
            left.Scale2d
        );
    }

    /// <summary>
    /// Adds a scalar to the rotation component only.
    /// </summary>
    /// <param name="left">The <see cref="Transform2D"/>.</param>
    /// <param name="right">The scalar to add to the rotation (in radians).</param>
    /// <returns>A new <see cref="Transform2D"/> with updated rotation.</returns>
    public static Transform2D operator +(Transform2D left, float right)
    {
        return new
        (
            left.Position,
            left.RotationRad + right,
            left.BaseSize,
            left.Scale2d
        );
    }

    /// <summary>
    /// Subtracts a scalar from the rotation component only.
    /// </summary>
    /// <param name="left">The <see cref="Transform2D"/>.</param>
    /// <param name="right">The scalar to subtract from the rotation (in radians).</param>
    /// <returns>A new <see cref="Transform2D"/> with updated rotation.</returns>
    public static Transform2D operator -(Transform2D left, float right)
    {
        return new
        (
            left.Position,
            left.RotationRad - right,
            left.BaseSize,
            left.Scale2d
        );
    }

    /// <summary>
    /// Multiplies the rotation component by a scalar.
    /// </summary>
    /// <param name="left">The <see cref="Transform2D"/>.</param>
    /// <param name="right">The scalar to multiply the rotation by.</param>
    /// <returns>A new <see cref="Transform2D"/> with updated rotation.</returns>
    public static Transform2D operator *(Transform2D left, float right)
    {
        return new
        (
            left.Position,
            left.RotationRad * right,
            left.BaseSize,
            left.Scale2d
        );
    }

    /// <summary>
    /// Divides the rotation component by a scalar.
    /// </summary>
    /// <param name="left">The <see cref="Transform2D"/>.</param>
    /// <param name="right">The scalar to divide the rotation by.</param>
    /// <returns>A new <see cref="Transform2D"/> with updated rotation.</returns>
    public static Transform2D operator /(Transform2D left, float right)
    {
        return new
        (
            left.Position,
            left.RotationRad / right,
            left.BaseSize ,
            left.Scale2d
        );
    }

    /// <summary>
    /// Adds a <see cref="Size"/> to the base size component only.
    /// </summary>
    /// <param name="left">The <see cref="Transform2D"/>.</param>
    /// <param name="right">The <see cref="Size"/> to add to the base size.</param>
    /// <returns>A new <see cref="Transform2D"/> with updated base size.</returns>
    public static Transform2D operator +(Transform2D left, Size right)
    {
        return new
        (
            left.Position,
            left.RotationRad,
            left.BaseSize + right,
            left.Scale2d
        );
    }

    /// <summary>
    /// Subtracts a <see cref="Size"/> from the base size component only.
    /// </summary>
    /// <param name="left">The <see cref="Transform2D"/>.</param>
    /// <param name="right">The <see cref="Size"/> to subtract from the base size.</param>
    /// <returns>A new <see cref="Transform2D"/> with updated base size.</returns>
    public static Transform2D operator -(Transform2D left, Size right)
    {
        return new
        (
            left.Position,
            left.RotationRad,
            left.BaseSize - right,
            left.Scale2d
        );
    }

    /// <summary>
    /// Multiplies the base size component by a <see cref="Size"/>.
    /// </summary>
    /// <param name="left">The <see cref="Transform2D"/>.</param>
    /// <param name="right">The <see cref="Size"/> to multiply the base size by.</param>
    /// <returns>A new <see cref="Transform2D"/> with updated base size.</returns>
    public static Transform2D operator *(Transform2D left, Size right)
    {
        return new
        (
            left.Position,
            left.RotationRad,
            left.BaseSize * right,
            left.Scale2d
        );
    }

    /// <summary>
    /// Divides the base size component by a <see cref="Size"/>.
    /// </summary>
    /// <param name="left">The <see cref="Transform2D"/>.</param>
    /// <param name="right">The <see cref="Size"/> to divide the base size by.</param>
    /// <returns>A new <see cref="Transform2D"/> with updated base size.</returns>
    public static Transform2D operator /(Transform2D left, Size right)
    {
        return new
        (
            left.Position,
            left.RotationRad,
            left.BaseSize / right,
            left.Scale2d
        );
    }

    /// <summary>
    /// Determines whether two <see cref="Transform2D"/> instances are equal.
    /// </summary>
    /// <param name="left">The first <see cref="Transform2D"/>.</param>
    /// <param name="right">The second <see cref="Transform2D"/>.</param>
    /// <returns>True if all components are equal; otherwise, false.</returns>
    public static bool operator ==(Transform2D left, Transform2D right) => right.Equals(left);

    /// <summary>
    /// Determines whether two <see cref="Transform2D"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first <see cref="Transform2D"/>.</param>
    /// <param name="right">The second <see cref="Transform2D"/>.</param>
    /// <returns>True if any component is not equal; otherwise, false.</returns>
    public static bool operator !=(Transform2D left, Transform2D right) => !(left == right);

    #endregion

    #region Equals & Hash Code

    /// <summary>
    /// Determines whether this instance and another specified <see cref="Transform2D"/> have the same value.
    /// </summary>
    /// <param name="other">The <see cref="Transform2D"/> to compare to this instance.</param>
    /// <returns>
    /// <c>true</c> if the value of the <paramref name="other"/> parameter is the same as this instance; otherwise, <c>false</c>.
    /// </returns>
    public bool Equals(Transform2D other) => Position.Equals(other.Position) && RotationRad.Equals(other.RotationRad) && BaseSize.Equals(other.BaseSize) && Scale2d.Equals(other.Scale2d);

    /// <summary>
    /// Determines whether this instance and a specified object, which must also be a <see cref="Transform2D"/>, have the same value.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns>
    /// <c>true</c> if <paramref name="obj"/> is a <see cref="Transform2D"/> and has the same value as this instance; otherwise, <c>false</c>.
    /// </returns>
    public override bool Equals(object? obj) => obj is Transform2D other && Equals(other);

    /// <summary>
    /// Returns the hash code for this <see cref="Transform2D"/>.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(Position, RotationRad, BaseSize, Scale2d);

    #endregion
}


