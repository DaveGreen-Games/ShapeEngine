using System.Numerics;

namespace ShapeEngine.Geometry.CollisionSystem;

/// <summary>
/// Contains the information of an intersection or an overlap between two colliders.
/// </summary>
/// <remarks>
/// Provides access to the colliders, their velocities, collision points, and overlap information.
/// </remarks>
public class Collision
{
    #region Members
    /// <summary>
    /// Indicates whether this is the first contact between the colliders.
    /// </summary>
    public readonly bool FirstContact;
    /// <summary>
    /// Gets the collider instance representing this object in the collision.
    /// </summary>
    public readonly Collider Self;
    /// <summary>
    /// Gets the collider instance representing the other object involved in the collision.
    /// </summary>
    public readonly Collider Other;
    /// <summary>
    /// The velocity of the 'self' collider at the time of collision.
    /// </summary>
    public readonly Vector2 SelfVel;
    /// <summary>
    /// The velocity of the 'other' collider at the time of collision.
    /// </summary>
    public readonly Vector2 OtherVel;
    /// <summary>
    /// The set of collision points, if any, for this collision.
    /// </summary>
    /// <remarks>
    /// If there are no collision points, this instance represents an overlap rather than an intersection.
    /// </remarks>
    public readonly IntersectionPoints? Points;
    /// <summary>
    /// Gets an <see cref="Overlap"/> object representing this collision.
    /// </summary>
    public Overlap Overlap => new(Self, Other, FirstContact);
    #endregion
    
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="Collision"/> class without collision points (Overlap).
    /// </summary>
    /// <param name="self">The 'self' collider.</param>
    /// <param name="other">The 'other' collider.</param>
    /// <param name="firstContact">Whether this is the first contact.</param>
    public Collision(Collider self, Collider other, bool firstContact)
    {
        Self = self;
        Other = other;
        SelfVel = self.Velocity;
        OtherVel = other.Velocity;
        FirstContact = firstContact;
        Points = null;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="Collision"/> class with collision points (Intersection).
    /// </summary>
    /// <param name="self">The 'self' collider.</param>
    /// <param name="other">The 'other' collider.</param>
    /// <param name="firstContact">Whether this is the first contact.</param>
    /// <param name="collisionPoints">
    /// The set of collision points for this collision.
    /// If null or empty (Count &lt;= 0), <see cref="Points"/> will be set to null,
    /// indicating an overlap rather than an intersection.
    /// </param>
    public Collision(Collider self, Collider other, bool firstContact, IntersectionPoints? collisionPoints)
    {
        Self = self;
        Other = other;
        SelfVel = self.Velocity;
        OtherVel = other.Velocity;
        FirstContact = firstContact;
        Points = collisionPoints is not { Count: > 0 } ? null : collisionPoints;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="Collision"/> class by copying another collision.
    /// </summary>
    /// <param name="collision">The collision to copy.</param>
    private Collision(Collision collision)
    {
        Self = collision.Self;
        Other = collision.Other;
        SelfVel = collision.SelfVel;
        OtherVel = collision.OtherVel;
        FirstContact = collision.FirstContact;
        Points = collision.Points?.Copy();
    }
    #endregion

    #region Public Functions
    /// <summary>
    /// Creates a deep copy of this collision.
    /// </summary>
    /// <returns>A new <see cref="Collision"/> instance with copied data.</returns>
    public Collision Copy() => new(this);
    #endregion
    
    #region Validation
    /// <summary>
    /// Validates the collision points using <see cref="SelfVel"/> as reference direction and <see cref="Self"/>.CurTransform.Position as reference point.
    /// </summary>
    /// <param name="combined">The average <see cref="IntersectionPoint"/> of all valid collision points.</param>
    /// <returns>Returns true if there are valid collision points left.</returns>
    public bool Validate(out IntersectionPoint combined)
    {
        if (Points == null || Points.Count <= 0)
        {
            combined = new IntersectionPoint();
            return false;
        }
        return Points.Validate(SelfVel, Self.CurTransform.Position, out combined);
    }
    /// <summary>
    /// Validates the collision points using <see cref="SelfVel"/> as reference direction and <see cref="Self"/>.CurTransform.Position as reference point.
    /// </summary>
    /// <param name="combined">The average <see cref="IntersectionPoint"/> of all valid collision points.</param>
    /// <param name="closest">The closest valid <see cref="IntersectionPoint"/> to the reference point.</param>
    /// <returns>Returns true if there are valid collision points left.</returns>
    public bool Validate(out IntersectionPoint combined, out IntersectionPoint closest)
    {
        if (Points == null || Points.Count <= 0)
        {
            combined = new IntersectionPoint();
            closest = new IntersectionPoint(); 
            return false;
        }
        return Points.Validate(SelfVel, Self.CurTransform.Position, out combined, out closest);
    }
    /// <summary>
    /// Validates the collision points using <see cref="SelfVel"/> as reference direction and <see cref="Self"/>.CurTransform.Position as reference point.
    /// </summary>
    /// <param name="result">A collection of valid collision points.</param>
    /// <returns>Returns true if there are valid collision points left.</returns>
    public bool Validate(out CollisionPointValidationResult result)
    {
        if (Points is not { Count: > 0 })
        {
            result = new CollisionPointValidationResult();
            return false;
        }
        return Points.Validate(SelfVel, Self.CurTransform.Position, out result);
    }
    #endregion
    
    #region IntersectionPoint

    /// <summary>
    /// Checks if there is any collision point that matches the conditions defined by the specified predicate.
    /// </summary>
    /// <param name="match">The predicate that defines the conditions of the collision points to search for.</param>
    /// <returns>true if any collision point matches the predicate; otherwise, false.</returns>
    public bool Exists(Predicate<IntersectionPoint> match)
    {
        return Points is { Count: > 0 } && Points.Exists(match);
    }
    /// <summary>
    /// Finds the first collision point that matches the conditions defined by the specified predicate.
    /// </summary>
    /// <param name="match">The predicate that defines the conditions of the collision point to search for.</param>
    /// <returns>
    /// The first collision point that matches the predicate, or a default <see cref="IntersectionPoint"/> if no match is found.
    /// </returns>
    public IntersectionPoint Find(Predicate<IntersectionPoint> match)
    {
        return Points is not { Count: > 0 } ? new() : Points.Find(match);
    }
    /// <summary>
    /// Finds all collision points that match the conditions defined by the specified predicate.
    /// </summary>
    /// <param name="match">The predicate that defines the conditions of the collision points to search for.</param>
    /// <returns>
    /// A new <see cref="IntersectionPoints"/> instance containing all the collision points that match the predicate,
    /// or null if no points match.
    /// </returns>
    public IntersectionPoints? FindAll(Predicate<IntersectionPoint> match)
    {
        if (Points is not { Count: > 0 }) return null;
        var result = Points.FindAll(match);
        return new(result);
    }
    
    /// <summary>
    /// Calculates the combined collision point from all valid collision points.
    /// </summary>
    /// <returns>
    /// The combined <see cref="IntersectionPoint"/> representing the average position and normal of all valid collision points,
    /// or a default <see cref="IntersectionPoint"/> if there are no valid points.
    /// </returns>
    public IntersectionPoint GetCombinedCollisionPoint()
    {
        return Points is not { Count: > 0 } ? new IntersectionPoint() : Points.GetCombinedCollisionPoint();
    }
    /// <summary>
    /// Finds the collision point closest to the 'self' collider's position.
    /// </summary>
    /// <returns>
    /// The closest <see cref="IntersectionPoint"/> to the 'self' collider's position,
    /// or a default <see cref="IntersectionPoint"/> if there are no collision points.
    /// </returns>
    public IntersectionPoint GetClosestCollisionPoint()
    {
        return Points is not { Count: > 0 } ? new IntersectionPoint() : Points.GetClosestCollisionPoint(Self.CurTransform.Position);
    }
    /// <summary>
    /// Finds the collision point furthest from the 'self' collider's position.
    /// </summary>
    /// <returns>
    /// The furthest <see cref="IntersectionPoint"/> from the 'self' collider's position,
    /// or a default <see cref="IntersectionPoint"/> if there are no collision points.
    /// </returns>
    public IntersectionPoint GetFurthestCollisionPoint()
    {
        return Points is not { Count: > 0 } ? new IntersectionPoint() : Points.GetFurthestCollisionPoint(Self.CurTransform.Position);
    }
    /// <summary>
    /// Finds the collision point closest to the 'self' collider's position and provides the distance squared to that point.
    /// </summary>
    /// <param name="closestDistanceSquared">Outputs the distance squared to the closest collision point.</param>
    /// <returns>
    /// The closest <see cref="IntersectionPoint"/> to the 'self' collider's position,
    /// or a default <see cref="IntersectionPoint"/> if there are no collision points.
    /// </returns>
    public IntersectionPoint GetClosestCollisionPoint(out float closestDistanceSquared)
    {
        closestDistanceSquared = -1;
        return Points is not { Count: > 0 } ? new IntersectionPoint() : Points.GetClosestCollisionPoint(Self.CurTransform.Position, out closestDistanceSquared);
    }
    /// <summary>
    /// Finds the collision point furthest from the 'self' collider's position and provides the distance squared to that point.
    /// </summary>
    /// <param name="furthestDistanceSquared">Outputs the distance squared to the furthest collision point.</param>
    /// <returns>
    /// The furthest <see cref="IntersectionPoint"/> from the 'self' collider's position,
    /// or a default <see cref="IntersectionPoint"/> if there are no collision points.
    /// </returns>
    public IntersectionPoint GetFurthestCollisionPoint(out float furthestDistanceSquared)
    {
        furthestDistanceSquared = -1;
        return Points is not { Count: > 0 } ? new IntersectionPoint() : Points.GetFurthestCollisionPoint(Self.CurTransform.Position, out furthestDistanceSquared);
    }

    /// <summary>
    /// Finds the collision point with the normal facing most in the direction of the <see cref="Self"/> colliders position.
    /// Each collision point normal is checked against the direction from the collision point towards the reference point.
    /// </summary>
    /// <returns>
    /// The <see cref="IntersectionPoint"/> facing most towards the 'self' collider's position,
    /// or a default <see cref="IntersectionPoint"/> if there are no collision points.
    /// </returns>
    public IntersectionPoint GetCollisionPointFacingTowardsSelf()
    {
        return Points is not { Count: > 0 } ? new IntersectionPoint() : Points.GetCollisionPointFacingTowardsPoint(Self.CurTransform.Position);
    }
   
    /// <summary>
    /// Finds the collision point with the normal facing most in the direction of <see cref="SelfVel"/>.
    /// </summary>
    /// <returns>
    /// The <see cref="IntersectionPoint"/> facing most in the direction of <see cref="SelfVel"/>,
    /// or a default <see cref="IntersectionPoint"/> if there are no collision points or if <see cref="SelfVel"/> is zero.
    /// </returns>
    public IntersectionPoint GetCollisionPointFacingTowardsSelfVel()
    {
        if(SelfVel is { X: 0f, Y: 0f } || Points is not { Count: > 0 }) return new IntersectionPoint();
        return Points.GetCollisionPointFacingTowardsDir(SelfVel);
    }
    
    #endregion
}
