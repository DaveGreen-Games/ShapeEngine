using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Core.CollisionSystem;

/// <summary>
/// Contains the information of a collision between two collision objects in the form of a list of collisions.
/// Each collision is an intersection or overlap between two colliders.
/// </summary>
/// <remarks>
/// Provides methods for validation, filtering, and aggregation of collision points across multiple collisions.
/// </remarks>
public class CollisionInformation : List<Collision>
{
    private static readonly CollisionPoints filterList = new CollisionPoints(64);
    #region Members
    /// <summary>
    /// The collision object representing 'self' in the collision information.
    /// </summary>
    public readonly CollisionObject Self;
    /// <summary>
    /// The collision object representing 'other' in the collision information.
    /// </summary>
    public readonly CollisionObject Other;
    /// <summary>
    /// The velocity of the 'self' collision object.
    /// </summary>
    public readonly Vector2 SelfVel;
    /// <summary>
    /// The velocity of the 'other' collision object.
    /// </summary>
    public readonly Vector2 OtherVel;
    /// <summary>
    /// Indicates whether this is the first contact between the objects.
    /// </summary>
    public readonly bool FirstContact;
    /// <summary>
    /// The total number of collision points across all collisions in this information.
    /// </summary>
    public int TotalCollisionPointCount { get; private set; }
    /// <summary>
    /// The filtered collision point.
    /// Only valid when the collision object has <see cref="CollisionObject.FilterCollisionPoints"/> enabled and
    /// the collider has <see cref="Collider.ComputeIntersections"/> enabled.
    /// </summary>
    public CollisionPoint FilteredCollisionPoint { get; private set; }
    #endregion
    
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="CollisionInformation"/> class.
    /// </summary>
    /// <param name="self">The 'self' collision object.</param>
    /// <param name="other">The 'other' collision object.</param>
    /// <param name="firstContact">Whether this is the first contact.</param>
    public CollisionInformation(CollisionObject self, CollisionObject other, bool firstContact)
    {
        Self = self;
        SelfVel = self.Velocity;
        Other = other;
        OtherVel = other.Velocity;
        FirstContact = firstContact;
        FilteredCollisionPoint = new CollisionPoint();
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="CollisionInformation"/> class with a list of collisions.
    /// </summary>
    /// <param name="self">The 'self' collision object.</param>
    /// <param name="other">The 'other' collision object.</param>
    /// <param name="firstContact">Whether this is the first contact.</param>
    /// <param name="collisions">The list of collisions to add.</param>
    public CollisionInformation(CollisionObject self, CollisionObject other, bool firstContact, List<Collision> collisions)
    {
        Self = self;
        SelfVel = self.Velocity;
        Other = other;
        OtherVel = other.Velocity;
        FirstContact = firstContact;
        AddRange(collisions);
        FilteredCollisionPoint = new CollisionPoint();
    }
    #endregion

    /// <summary>
    /// Generates a filtered collision point based on the specified filter type and reference point.
    /// </summary>
    /// <param name="filterType">The filter type to use for selecting the collision point.</param>
    /// <param name="referencePoint">The reference point for filtering.</param>
    internal void GenerateFilteredCollisionPoint(CollisionPointsFilterType filterType, Vector2 referencePoint)
    {
        if (TotalCollisionPointCount <= 0) return;
        foreach (var collision in this)
        {
            if(collision.Points != null && collision.Points.Count > 0) filterList.AddRange(collision.Points);
        }
        if (filterList.Count > 0)
        {
            FilteredCollisionPoint = filterList.Filter(filterType, referencePoint);
            filterList.Clear();
        }
    }
    #region Validation
    /// <summary>
    /// Validates the collisions and removes invalid ones using <see cref="SelfVel"/> as reference direction and <see cref="Self"/>.CurTransform.Position as reference point.
    /// </summary>
    /// <param name="combined">The average <see cref="CollisionPoint"/> of all valid collision points.</param>
    /// <returns>Returns true if there are valid collision points left.</returns>
    public bool Validate(out CollisionPoint combined)
    {
        combined = new CollisionPoint();
        if(Count <= 0) return false;
        for (int i = Count - 1; i >= 0; i--)
        {
            var collision = this[i];
            if (collision.Validate(out CollisionPoint combinedCollisionPoint))
            {
                combined = combinedCollisionPoint.Combine(combined);
            }
            else RemoveAt(i);
        }
        return Count > 0;
    }
    /// <summary>
    /// Validates the collisions and removes invalid ones using <see cref="SelfVel"/> as reference direction and <see cref="Self"/>.CurTransform.Position as reference point.
    /// </summary>
    /// <param name="combined">The average <see cref="CollisionPoint"/> of all valid collision points.</param>
    /// <param name="closest">The closest valid <see cref="CollisionPoint"/> to the reference point.</param>
    /// <returns>Returns true if there are valid collision points left.</returns>
    public bool Validate(out CollisionPoint combined, out CollisionPoint closest)
    {
        combined = new CollisionPoint();
        closest = new CollisionPoint();
        var closestDistanceSquared = -1f;
        if(Count <= 0) return false;
        for (int i = Count - 1; i >= 0; i--)
        {
            var collision = this[i];
            if (collision.Validate(out CollisionPoint combinedCollisionPoint, out var closestCollisionPoint))
            {
                combined = combinedCollisionPoint.Combine(combined);
                var dis = (collision.Self.CurTransform.Position - closestCollisionPoint.Point).LengthSquared();
                if (closestDistanceSquared < 0f || dis < closestDistanceSquared)
                {
                    closestDistanceSquared = dis;
                    closest = closestCollisionPoint;
                }
            }
            else RemoveAt(i);
        }
        return Count > 0;
    }
    /// <summary>
    /// Validates the collisions and removes invalid ones using <see cref="SelfVel"/> as reference direction and <see cref="Self"/>.CurTransform.Position as reference point.
    /// </summary>
    /// <param name="result">A collection of valid collision points.</param>
    /// <returns>Returns true if there are valid collision points left.</returns>
    public bool Validate(out CollisionPointValidationResult result)
    {
        result = new CollisionPointValidationResult();
        var combined = new CollisionPoint();
        var closest = new CollisionPoint();
        var furthest = new CollisionPoint();
        var pointing = new CollisionPoint();
        var closestDistanceSquared = -1f;
        var furthestDistanceSquared = -1f;
        var maxDot = -1f;
        if(Count <= 0) return false;
        for (int i = Count - 1; i >= 0; i--)
        {
            var collision = this[i];
            if (collision.Validate(out CollisionPointValidationResult validationResult))
            {
                combined = validationResult.Combined.Combine(combined);
                if (collision.SelfVel.X != 0 || collision.SelfVel.Y != 0)
                {
                    var dot = collision.SelfVel.Dot(validationResult.PointingTowards.Normal);
                    if (dot > maxDot)
                    {
                        maxDot = dot;
                        pointing = validationResult.PointingTowards;
                    }
                }
                var dis = (collision.Self.CurTransform.Position - validationResult.Closest.Point).LengthSquared();
                if (closestDistanceSquared < 0f || dis < closestDistanceSquared)
                {
                    closestDistanceSquared = dis;
                    closest = validationResult.Closest;
                }
                else if (furthestDistanceSquared < 0f || dis > furthestDistanceSquared)
                {
                    furthestDistanceSquared = dis;
                    furthest = validationResult.Furthest;
                }
            }
            else RemoveAt(i);
        }
        result = new(combined, closest, furthest, pointing);
        return Count > 0;
    }
    #endregion
    
    #region Public Functions

    /// <summary>
    /// Adds a collision to the collision information.
    /// </summary>
    /// <param name="collision">The collision to add.</param>
    public new void Add(Collision collision)
    {
        base.Add(collision);
        if(collision.Points != null) TotalCollisionPointCount += collision.Points.Count;
    }
    /// <summary>
    /// Creates a copy of the collision information with duplicated collisions.
    /// </summary>
    /// <returns>A new instance of <see cref="CollisionInformation"/> with duplicated collisions.</returns>
    public CollisionInformation Copy()
    {
        var newCollisions = new List<Collision>();
        foreach (var collision in this)
        {
            newCollisions.Add(collision.Copy());
        }
        return new CollisionInformation(Self, Other, FirstContact,  newCollisions);
    }
    
    /// <summary>
    /// Filters the collisions based on a predicate.
    /// </summary>
    /// <param name="match">The predicate to match collisions against.</param>
    /// <returns>A list of collisions that match the predicate, or null if none match.</returns>
    public List<Collision>? FilterCollisions(Predicate<Collision> match)
    {
        if(Count <= 0) return null;
        List<Collision>? filtered = null;
        foreach (var c in this)
        {
            if (match(c))
            {
                filtered??= new();
                filtered.Add(c);
            }
        }
        return filtered;
    }
    /// <summary>
    /// Returns a set of all unique colliders from the 'other' collision object involved in the collisions.
    /// </summary>
    /// <returns>A <see cref="HashSet{Collider}"/> containing all unique other colliders, or null if there are no collisions.</returns>
    public HashSet<Collider>? GetAllOtherColliders()
    {
        if(Count <= 0) return null;
        HashSet<Collider> others = [];
        foreach (var c in this)
        {
            others.Add(c.Other);
        }
        return others;
    }
    /// <summary>
    /// Gets all collisions that are marked as first contact.
    /// </summary>
    /// <returns>A list of collisions that are first contacts, or null if there are none.</returns>
    public List<Collision>? GetAllFirstContactCollisions()
    {
        return FilterCollisions((c) => c.FirstContact);
    }
    /// <summary>
    /// Gets all other colliders involved in collisions that are marked as first contact.
    /// </summary>
    /// <returns>A set of all other colliders involved in first contact collisions, or null if there are none.</returns>
    public HashSet<Collider>? GetAllOtherFirstContactColliders()
    {
        var filtered = GetAllFirstContactCollisions();
        if(filtered == null) return null;
        HashSet<Collider> others = new();
        foreach (var c in filtered)
        {
            others.Add(c.Other);
        }
        return others;
    }

    
    /// <summary>
    /// Determines whether any collision point in any collision matches the specified predicate.
    /// </summary>
    /// <param name="match">A predicate to test each collision point.</param>
    /// <returns>True if a matching collision point exists; otherwise, false.</returns>
    public bool ExistsCollisionPoint(Predicate<CollisionPoint> match)
    {
        if(Count <= 0) return false;
        foreach (var collision in this)
        {
            if (collision.Exists(match)) return true;
        }

        return false;
    }
    /// <summary>
    /// Returns the first <see cref="CollisionPoint"/> that matches the given predicate across all collisions.
    /// If no matching collision point is found, returns an empty <see cref="CollisionPoint"/>.
    /// </summary>
    /// <param name="match">Predicate to test each collision point.</param>
    /// <returns>The first matching <see cref="CollisionPoint"/>, or an empty one if none found.</returns>
    public CollisionPoint FindCollisionPoint(Predicate<CollisionPoint> match)
    {
        if(Count <= 0) return new();
        foreach (var collision in this)
        {
            var p = collision.Find(match);
            if (p.Valid) return p;
        }

        return new();
    }
    /// <summary>
    /// Finds all collision points that match the given predicate across all valid collisions.
    /// Returns a <see cref="CollisionPoints"/> collection if any are found; otherwise, returns null.
    /// </summary>
    /// <param name="match">Predicate to test each collision point.</param>
    /// <returns>A <see cref="CollisionPoints"/> collection of matching points, or null if none found.</returns>
    public CollisionPoints? FindAllCollisionPoints(Predicate<CollisionPoint> match)
    {
        if (Count <= 0) return null;
        CollisionPoints? result = null;
        foreach (var collision in this)
        {
           var points = collision.FindAll(match);
           if(points == null) continue;
           
           result??= new CollisionPoints();
           result.AddRange(points);
        }

        return result;
    }

    
    /// <summary>
    /// Returns the closest <see cref="CollisionPoint"/> to <see cref="Self"/>.Transform.Position across all valid collisions.
    /// </summary>
    /// <returns>The closest <see cref="CollisionPoint"/>, or an empty one if none exist.</returns>
    public CollisionPoint GetClosestCollisionPoint()
    {
        if(Count <= 0) return new CollisionPoint();
        
        var result = new CollisionPoint();
        var closestDistanceSquared = -1f;
        foreach (var collision in this)
        {
            if(collision.Points is not { Count: > 0 }) continue;
            var cp = collision.Points.GetClosestCollisionPoint(Self.Transform.Position, out float minDistanceSquared);
            if (!cp.Valid) continue;
            if (minDistanceSquared < closestDistanceSquared || closestDistanceSquared < 0f)
            {
                closestDistanceSquared = minDistanceSquared;
                result = cp;
            }
        }

        return result;
    }

    /// <summary>
    /// Returns the furthest <see cref="CollisionPoint"/> from <see cref="Self"/>.Transform.Position across all valid collisions.
    /// </summary>
    /// <returns>The furthest <see cref="CollisionPoint"/>, or an empty one if none exist.</returns>
    public CollisionPoint GetFurthestCollisionPoint()
    {
        if(Count <= 0) return new CollisionPoint();
        
        var result = new CollisionPoint();
        var furthestDistanceSquared = -1f;
        foreach (var collision in this)
        {
            if(collision.Points is not { Count: > 0 }) continue;
            var cp = collision.Points.GetFurthestCollisionPoint(Self.Transform.Position, out float maxDistanceSquared);
            if (!cp.Valid) continue;
            if (maxDistanceSquared < furthestDistanceSquared || furthestDistanceSquared < 0f)
            {
                furthestDistanceSquared = maxDistanceSquared;
                result = cp;
            }
        }

        return result;
    }
    /// <summary>
    /// Calculates and returns the average (combined) collision point from all valid collision points
    /// across all valid collisions in this collection.
    /// </summary>
    /// <returns>The combined <see cref="CollisionPoint"/> if any exist; otherwise, an empty <see cref="CollisionPoint"/>.</returns>
    public CollisionPoint GetCombinedCollisionPoint()
    {
        if(Count <= 0) return new CollisionPoint();
        
        var result = new CollisionPoint();
        foreach (var collision in this)
        {
            if(collision.Points is not { Count: > 0 }) continue;
            var cp = collision.Points.GetCombinedCollisionPoint();
            if (!cp.Valid) continue;
            result = result.Combine(cp);
        }

        return result;
    }
    /// <summary>
    /// Returns the closest <see cref="CollisionPoint"/> to the specified reference point across all valid collisions.
    /// </summary>
    /// <param name="referencePoint">The reference point to measure distance from.</param>
    /// <returns>The closest <see cref="CollisionPoint"/>, or an empty one if none exist.</returns>
    public CollisionPoint GetClosestCollisionPoint(Vector2 referencePoint)
    {
        if(Count <= 0) return new CollisionPoint();
        
        var result = new CollisionPoint();
        var closestDistanceSquared = -1f;
        foreach (var collision in this)
        {
            if(collision.Points is not { Count: > 0 }) continue;
            var cp = collision.Points.GetClosestCollisionPoint(referencePoint, out float minDistanceSquared);
            if (!cp.Valid) continue;
            if (minDistanceSquared < closestDistanceSquared || closestDistanceSquared < 0f)
            {
                closestDistanceSquared = minDistanceSquared;
                result = cp;
            }
        }

        return result;
    }
    /// <summary>
    /// Gets the furthest collision point from the specified reference point within all valid collisions.
    /// </summary>
    /// <param name="referencePoint">The reference point to measure distance from.</param>
    /// <returns>The furthest <see cref="CollisionPoint"/> from the reference point, or an empty <see cref="CollisionPoint"/> if none exist.</returns>
    public CollisionPoint GetFurthestCollisionPoint(Vector2 referencePoint)
    {
        if(Count <= 0) return new CollisionPoint();
        
        var result = new CollisionPoint();
        var furthestDistanceSquared = -1f;
        foreach (var collision in this)
        {
            if(collision.Points == null || collision.Points.Count <= 0) continue;
            var cp = collision.Points.GetFurthestCollisionPoint(referencePoint, out float maxDistanceSquared);
            if (!cp.Valid) continue;
            if (maxDistanceSquared < furthestDistanceSquared || furthestDistanceSquared < 0f)
            {
                furthestDistanceSquared = maxDistanceSquared;
                result = cp;
            }
        }

        return result;
    }
    
    /// <summary>
    /// Returns the closest <see cref="CollisionPoint"/> to <see cref="Self"/>.Transform.Position within all valid collisions,
    /// and outputs the squared distance to that point. If no valid collision points exist, returns an empty <see cref="CollisionPoint"/>
    /// and sets <paramref name="closestDistanceSquared"/> to a negative value.
    /// </summary>
    /// <param name="closestDistanceSquared">The squared distance to the closest collision point, or a negative value if none found.</param>
    /// <returns>The closest <see cref="CollisionPoint"/>, or an empty one if none exist.</returns>
    public CollisionPoint GetClosestCollisionPoint(out float closestDistanceSquared)
    {
        closestDistanceSquared = -1f;
        if(Count <= 0) return new CollisionPoint();
        
        var result = new CollisionPoint();
        foreach (var collision in this)
        {
            if(collision.Points == null || collision.Points.Count <= 0) continue;
            var cp = collision.Points.GetClosestCollisionPoint(Self.Transform.Position, out float minDistanceSquared);
            if (!cp.Valid) continue;
            if (minDistanceSquared < closestDistanceSquared || closestDistanceSquared < 0f)
            {
                closestDistanceSquared = minDistanceSquared;
                result = cp;
            }
        }

        return result;
    }

    /// <summary>
    /// Returns the furthest <see cref="CollisionPoint"/> from <see cref="Self"/>.Transform.Position across all valid collisions,
    /// and outputs the squared distance to that point. If no valid collision points exist, returns an empty <see cref="CollisionPoint"/>
    /// and sets <paramref name="furthestDistanceSquared"/> to a negative value.
    /// </summary>
    /// <param name="furthestDistanceSquared">The squared distance to the furthest collision point, or a negative value if none found.</param>
    /// <returns>The furthest <see cref="CollisionPoint"/> from <see cref="Self"/>.Transform.Position, or an empty one if none exist.</returns>
    public CollisionPoint GetFurthestCollisionPoint(out float furthestDistanceSquared)
    {
        furthestDistanceSquared = -1f;
        if(Count <= 0) return new CollisionPoint();
        
        var result = new CollisionPoint();
        foreach (var collision in this)
        {
            if(collision.Points == null || collision.Points.Count <= 0) continue;
            var cp = collision.Points.GetFurthestCollisionPoint(Self.Transform.Position, out float maxDistanceSquared);
            if (!cp.Valid) continue;
            if (maxDistanceSquared < furthestDistanceSquared || furthestDistanceSquared < 0f)
            {
                furthestDistanceSquared = maxDistanceSquared;
                result = cp;
            }
        }

        return result;
    }
    
    /// <summary>
    /// Returns the closest <see cref="CollisionPoint"/> to the specified reference point within all valid collisions,
    /// and outputs the squared distance to that point.
    /// If no valid collision points exist, returns an empty <see cref="CollisionPoint"/>
    /// and sets <paramref name="closestDistanceSquared"/> to a negative value.
    /// </summary>
    /// <param name="referencePoint">The reference point for finding the closest collision point.</param>
    /// <param name="closestDistanceSquared">
    /// The squared distance between the closest collision point and the reference point. Negative if no valid point is found.
    /// </param>
    /// <returns>The closest <see cref="CollisionPoint"/>, or an empty one if none exist.</returns>
    public CollisionPoint GetClosestCollisionPoint(Vector2 referencePoint, out float closestDistanceSquared)
    {
        closestDistanceSquared = -1f;
        if(Count <= 0) return new CollisionPoint();
        
        var result = new CollisionPoint();
        foreach (var collision in this)
        {
            if(collision.Points == null || collision.Points.Count <= 0) continue;
            var cp = collision.Points.GetClosestCollisionPoint(referencePoint, out float minDistanceSquared);
            if (!cp.Valid) continue;
            if (minDistanceSquared < closestDistanceSquared || closestDistanceSquared < 0f)
            {
                closestDistanceSquared = minDistanceSquared;
                result = cp;
            }
        }

        return result;
    }

    /// <summary>
    /// Returns the furthest <see cref="CollisionPoint"/> from the specified reference point within all valid collisions,
    /// and outputs the squared distance to that point.
    /// If no valid collision points exist, returns an empty <see cref="CollisionPoint"/>
    /// and sets <paramref name="furthestDistanceSquared"/> to a negative value.
    /// </summary>
    /// <param name="referencePoint">The reference point for finding the furthest collision point.</param>
    /// <param name="furthestDistanceSquared">
    /// The squared distance between the furthest collision point and the reference point. Negative if no valid point is found.
    /// </param>
    /// <returns>The furthest <see cref="CollisionPoint"/>, or an empty one if none exist.</returns>
    public CollisionPoint GetFurthestCollisionPoint(Vector2 referencePoint, out float furthestDistanceSquared)
    {
        furthestDistanceSquared = -1f;
        if(Count <= 0) return new CollisionPoint();
        
        var result = new CollisionPoint();
        foreach (var collision in this)
        {
            if(collision.Points == null || collision.Points.Count <= 0) continue;
            var cp = collision.Points.GetFurthestCollisionPoint(Self.Transform.Position, out float maxDistanceSquared);
            if (!cp.Valid) continue;
            if (maxDistanceSquared < furthestDistanceSquared || furthestDistanceSquared < 0f)
            {
                furthestDistanceSquared = maxDistanceSquared;
                result = cp;
            }
        }

        return result;
    }
    #endregion
}
