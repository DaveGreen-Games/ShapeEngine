using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Geometry.CollisionSystem.CollisionHandlerDef;

public partial class CollisionHandler
{
    /// <summary>
    /// Performs intersection queries for all colliders of a <see cref="CollisionObject"/> against the collision system.
    /// </summary>
    /// <param name="colObject">The collision object whose colliders are used for intersection queries</param>
    /// <param name="origin">The origin point for the intersection result.</param>
    /// <returns>An <see cref="IntersectSpaceResult"/> containing intersection data, or null if no intersections are found.</returns>
    /// <remarks>
    /// Only enabled colliders on <paramref name="colObject"/> with <c>ComputeIntersections</c> set to true are used.
    /// Only enabled colliders are considered for intersection checks in this query.
    /// </remarks>
    public IntersectSpaceResult? IntersectSpace(CollisionObject colObject, Vector2 origin)
    {
        if (colObject.Colliders.Count <= 0) return null;

        foreach (var collider in colObject.Colliders)
        {
            if (!collider.Enabled) continue;
            if (!collider.ComputeIntersections) continue;

            collisionCandidateBuckets.Clear();
            collisionCandidateCheckRegister.Clear();

            broadphaseSpatialHash.GetCandidateBuckets(collider, ref collisionCandidateBuckets);
            if (collisionCandidateBuckets.Count <= 0) return null;
            var collisionMask = collider.CollisionMask;
            foreach (var bucket in collisionCandidateBuckets)
            {
                foreach (var candidate in bucket)
                {
                    if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                    if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                    var parent = candidate.Parent;
                    if (parent == null) continue;

                    var collisionPoints = collider.Intersect(candidate);

                    if (collisionPoints == null || !collisionPoints.Valid) continue;

                    var entry = new IntersectSpaceEntry(candidate, collisionPoints);
                    if (intersectSpaceRegisters.TryGetValue(parent, out var register))
                    {
                        register.Add(entry);
                    }
                    else
                    {
                        var newRegister = new IntersectSpaceRegister(parent, 2);
                        newRegister.Add(entry);
                        intersectSpaceRegisters.Add(parent, newRegister);
                    }
                }
            }
        }


        if (intersectSpaceRegisters.Count <= 0) return null;

        var result = new IntersectSpaceResult(origin, intersectSpaceRegisters.Count);
        foreach (var register in intersectSpaceRegisters.Values)
        {
            result.Add(register);
        }

        intersectSpaceRegisters.Clear();
        return result;
    }

    /// <summary>
    /// Performs an intersection query for a single <see cref="Collider"/> against the collision system.
    /// </summary>
    /// <param name="collider">The collider to test for intersections. The collider needs to be enabled!</param>
    /// <param name="origin">The origin point for the intersection result.</param>
    /// <returns>An <see cref="IntersectSpaceResult"/> containing intersection data, or null if no intersections are found.</returns>
    /// <remarks>
    /// Only enabled colliders are considered for intersection checks in this query.
    /// </remarks>
    public IntersectSpaceResult? IntersectSpace(Collider collider, Vector2 origin)
    {
        if (!collider.Enabled) return null;
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();

        broadphaseSpatialHash.GetCandidateBuckets(collider, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return null;
        var collisionMask = collider.CollisionMask;
        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                var parent = candidate.Parent;
                if (parent == null) continue;

                var collisionPoints = collider.Intersect(candidate);

                if (collisionPoints == null || !collisionPoints.Valid) continue;

                var entry = new IntersectSpaceEntry(candidate, collisionPoints);
                if (intersectSpaceRegisters.TryGetValue(parent, out var register))
                {
                    register.Add(entry);
                }
                else
                {
                    var newRegister = new IntersectSpaceRegister(parent, 2);
                    newRegister.Add(entry);
                    intersectSpaceRegisters.Add(parent, newRegister);
                }
            }
        }

        if (intersectSpaceRegisters.Count <= 0) return null;

        var result = new IntersectSpaceResult(origin, intersectSpaceRegisters.Count);
        foreach (var register in intersectSpaceRegisters.Values)
        {
            result.Add(register);
        }

        intersectSpaceRegisters.Clear();
        return result;
    }

    /// <summary>
    /// Performs an intersection query for a <see cref="Segment"/> shape against the collision system.
    /// </summary>
    /// <param name="shape">The segment shape to test for intersections.</param>
    /// <param name="origin">The origin point for the intersection result.</param>
    /// <param name="collisionMask">The collision mask to filter candidate colliders.</param>
    /// <returns>An <see cref="IntersectSpaceResult"/> containing intersection data, or null if no intersections are found.</returns>
    /// <remarks>
    /// Only enabled colliders are considered for intersection checks in this query.
    /// </remarks>
    public IntersectSpaceResult? IntersectSpace(Segment shape, Vector2 origin, BitFlag collisionMask)
    {
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();

        broadphaseSpatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return null;

        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                var parent = candidate.Parent;
                if (parent == null) continue;

                var collisionPoints = shape.Intersect(candidate);

                if (collisionPoints == null || !collisionPoints.Valid) continue;

                var entry = new IntersectSpaceEntry(candidate, collisionPoints);
                if (intersectSpaceRegisters.TryGetValue(parent, out var register))
                {
                    register.Add(entry);
                }
                else
                {
                    var newRegister = new IntersectSpaceRegister(parent, 2);
                    newRegister.Add(entry);
                    intersectSpaceRegisters.Add(parent, newRegister);
                }
            }
        }

        if (intersectSpaceRegisters.Count <= 0) return null;

        var result = new IntersectSpaceResult(origin, intersectSpaceRegisters.Count);
        foreach (var register in intersectSpaceRegisters.Values)
        {
            result.Add(register);
        }

        intersectSpaceRegisters.Clear();
        return result;
    }

    /// <summary>
    /// Performs an intersection query for a <see cref="Line"/> shape against the collision system.
    /// </summary>
    /// <param name="line">The line shape to test for intersections.</param>
    /// <param name="origin">The origin point for the intersection result.</param>
    /// <param name="collisionMask">The collision mask to filter candidate colliders.</param>
    /// <returns>An <see cref="IntersectSpaceResult"/> containing intersection data, or null if no intersections are found.</returns>
    /// <remarks>
    /// Only enabled colliders are considered for intersection checks in this query.
    /// </remarks>
    public IntersectSpaceResult? IntersectSpace(Line line, Vector2 origin, BitFlag collisionMask)
    {
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();

        broadphaseSpatialHash.GetCandidateBuckets(line, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return null;

        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                var parent = candidate.Parent;
                if (parent == null) continue;

                var collisionPoints = line.Intersect(candidate);

                if (collisionPoints == null || !collisionPoints.Valid) continue;

                var entry = new IntersectSpaceEntry(candidate, collisionPoints);
                if (intersectSpaceRegisters.TryGetValue(parent, out var register))
                {
                    register.Add(entry);
                }
                else
                {
                    var newRegister = new IntersectSpaceRegister(parent, 2);
                    newRegister.Add(entry);
                    intersectSpaceRegisters.Add(parent, newRegister);
                }
            }
        }

        if (intersectSpaceRegisters.Count <= 0) return null;

        var result = new IntersectSpaceResult(origin, intersectSpaceRegisters.Count);
        foreach (var register in intersectSpaceRegisters.Values)
        {
            result.Add(register);
        }

        intersectSpaceRegisters.Clear();
        return result;
    }

    /// <summary>
    /// Performs an intersection query for a <see cref="Ray"/> shape against the collision system.
    /// </summary>
    /// <param name="ray">The ray shape to test for intersections.</param>
    /// <param name="origin">The origin point for the intersection result.</param>
    /// <param name="collisionMask">The collision mask to filter candidate colliders.</param>
    /// <returns>An <see cref="IntersectSpaceResult"/> containing intersection data, or null if no intersections are found.</returns>
    /// <remarks>
    /// Only enabled colliders are considered for intersection checks in this query.
    /// </remarks>
    public IntersectSpaceResult? IntersectSpace(Ray ray, Vector2 origin, BitFlag collisionMask)
    {
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();

        broadphaseSpatialHash.GetCandidateBuckets(ray, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return null;

        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                var parent = candidate.Parent;
                if (parent == null) continue;

                var collisionPoints = ray.Intersect(candidate);

                if (collisionPoints == null || !collisionPoints.Valid) continue;

                var entry = new IntersectSpaceEntry(candidate, collisionPoints);
                if (intersectSpaceRegisters.TryGetValue(parent, out var register))
                {
                    register.Add(entry);
                }
                else
                {
                    var newRegister = new IntersectSpaceRegister(parent, 2);
                    newRegister.Add(entry);
                    intersectSpaceRegisters.Add(parent, newRegister);
                }
            }
        }

        if (intersectSpaceRegisters.Count <= 0) return null;

        var result = new IntersectSpaceResult(origin, intersectSpaceRegisters.Count);
        foreach (var register in intersectSpaceRegisters.Values)
        {
            result.Add(register);
        }

        intersectSpaceRegisters.Clear();
        return result;
    }

    /// <summary>
    /// Performs an intersection query for a <see cref="Triangle"/> shape against the collision system.
    /// </summary>
    /// <param name="shape">The triangle shape to test for intersections.</param>
    /// <param name="origin">The origin point for the intersection result.</param>
    /// <param name="collisionMask">The collision mask to filter candidate colliders.</param>
    /// <returns>An <see cref="IntersectSpaceResult"/> containing intersection data, or null if no intersections are found.</returns>
    /// <remarks>
    /// Only enabled colliders are considered for intersection checks in this query.
    /// </remarks>
    public IntersectSpaceResult? IntersectSpace(Triangle shape, Vector2 origin, BitFlag collisionMask)
    {
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();

        broadphaseSpatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return null;

        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                var parent = candidate.Parent;
                if (parent == null) continue;

                var collisionPoints = shape.Intersect(candidate);

                if (collisionPoints == null || !collisionPoints.Valid) continue;

                var entry = new IntersectSpaceEntry(candidate, collisionPoints);
                if (intersectSpaceRegisters.TryGetValue(parent, out var register))
                {
                    register.Add(entry);
                }
                else
                {
                    var newRegister = new IntersectSpaceRegister(parent, 2);
                    newRegister.Add(entry);
                    intersectSpaceRegisters.Add(parent, newRegister);
                }
            }
        }

        if (intersectSpaceRegisters.Count <= 0) return null;

        var result = new IntersectSpaceResult(origin, intersectSpaceRegisters.Count);
        foreach (var register in intersectSpaceRegisters.Values)
        {
            result.Add(register);
        }

        intersectSpaceRegisters.Clear();
        return result;
    }

    /// <summary>
    /// Performs an intersection query for a <see cref="Circle"/> shape against the collision system.
    /// </summary>
    /// <param name="shape">The circle shape to test for intersections.</param>
    /// <param name="origin">The origin point for the intersection result.</param>
    /// <param name="collisionMask">The collision mask to filter candidate colliders.</param>
    /// <returns>An <see cref="IntersectSpaceResult"/> containing intersection data, or null if no intersections are found.</returns>
    /// <remarks>
    /// Only enabled colliders are considered for intersection checks in this query.
    /// </remarks>
    public IntersectSpaceResult? IntersectSpace(Circle shape, Vector2 origin, BitFlag collisionMask)
    {
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();

        broadphaseSpatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return null;

        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                var parent = candidate.Parent;
                if (parent == null) continue;

                var collisionPoints = shape.Intersect(candidate);

                if (collisionPoints == null || !collisionPoints.Valid) continue;

                var entry = new IntersectSpaceEntry(candidate, collisionPoints);
                if (intersectSpaceRegisters.TryGetValue(parent, out var register))
                {
                    register.Add(entry);
                }
                else
                {
                    var newRegister = new IntersectSpaceRegister(parent, 2);
                    newRegister.Add(entry);
                    intersectSpaceRegisters.Add(parent, newRegister);
                }
            }
        }

        if (intersectSpaceRegisters.Count <= 0) return null;

        var result = new IntersectSpaceResult(origin, intersectSpaceRegisters.Count);
        foreach (var register in intersectSpaceRegisters.Values)
        {
            result.Add(register);
        }

        intersectSpaceRegisters.Clear();
        return result;
    }

    /// <summary>
    /// Performs an intersection query for a <see cref="Rect"/> shape against the collision system.
    /// </summary>
    /// <param name="shape">The rectangle shape to test for intersections.</param>
    /// <param name="origin">The origin point for the intersection result.</param>
    /// <param name="collisionMask">The collision mask to filter candidate colliders.</param>
    /// <returns>An <see cref="IntersectSpaceResult"/> containing intersection data, or null if no intersections are found.</returns>
    /// <remarks>
    /// Only enabled colliders are considered for intersection checks in this query.
    /// </remarks>
    public IntersectSpaceResult? IntersectSpace(Rect shape, Vector2 origin, BitFlag collisionMask)
    {
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();

        broadphaseSpatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return null;

        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                var parent = candidate.Parent;
                if (parent == null) continue;

                var collisionPoints = shape.Intersect(candidate);

                if (collisionPoints == null || !collisionPoints.Valid) continue;

                var entry = new IntersectSpaceEntry(candidate, collisionPoints);
                if (intersectSpaceRegisters.TryGetValue(parent, out var register))
                {
                    register.Add(entry);
                }
                else
                {
                    var newRegister = new IntersectSpaceRegister(parent, 2);
                    newRegister.Add(entry);
                    intersectSpaceRegisters.Add(parent, newRegister);
                }
            }
        }

        if (intersectSpaceRegisters.Count <= 0) return null;

        var result = new IntersectSpaceResult(origin, intersectSpaceRegisters.Count);
        foreach (var register in intersectSpaceRegisters.Values)
        {
            result.Add(register);
        }

        intersectSpaceRegisters.Clear();
        return result;
    }

    /// <summary>
    /// Performs an intersection query for a <see cref="Quad"/> shape against the collision system.
    /// </summary>
    /// <param name="shape">The quad shape to test for intersections.</param>
    /// <param name="origin">The origin point for the intersection result.</param>
    /// <param name="collisionMask">The collision mask to filter candidate colliders.</param>
    /// <returns>An <see cref="IntersectSpaceResult"/> containing intersection data, or null if no intersections are found.</returns>
    /// <remarks>
    /// Only enabled colliders are considered for intersection checks in this query.
    /// </remarks>
    public IntersectSpaceResult? IntersectSpace(Quad shape, Vector2 origin, BitFlag collisionMask)
    {
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();

        broadphaseSpatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return null;

        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                var parent = candidate.Parent;
                if (parent == null) continue;

                var collisionPoints = shape.Intersect(candidate);

                if (collisionPoints == null || !collisionPoints.Valid) continue;

                var entry = new IntersectSpaceEntry(candidate, collisionPoints);
                if (intersectSpaceRegisters.TryGetValue(parent, out var register))
                {
                    register.Add(entry);
                }
                else
                {
                    var newRegister = new IntersectSpaceRegister(parent, 2);
                    newRegister.Add(entry);
                    intersectSpaceRegisters.Add(parent, newRegister);
                }
            }
        }

        if (intersectSpaceRegisters.Count <= 0) return null;

        var result = new IntersectSpaceResult(origin, intersectSpaceRegisters.Count);
        foreach (var register in intersectSpaceRegisters.Values)
        {
            result.Add(register);
        }

        intersectSpaceRegisters.Clear();
        return result;
    }

    /// <summary>
    /// Performs an intersection query for a <see cref="Polygon"/> shape against the collision system.
    /// </summary>
    /// <param name="shape">The polygon shape to test for intersections.</param>
    /// <param name="origin">The origin point for the intersection result.</param>
    /// <param name="collisionMask">The collision mask to filter candidate colliders.</param>
    /// <returns>An <see cref="IntersectSpaceResult"/> containing intersection data, or null if no intersections are found.</returns>
    /// <remarks>
    /// Only enabled colliders are considered for intersection checks in this query.
    /// </remarks>
    public IntersectSpaceResult? IntersectSpace(Polygon shape, Vector2 origin, BitFlag collisionMask)
    {
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();

        broadphaseSpatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return null;

        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                var parent = candidate.Parent;
                if (parent == null) continue;

                var collisionPoints = shape.Intersect(candidate);

                if (collisionPoints == null || !collisionPoints.Valid) continue;

                var entry = new IntersectSpaceEntry(candidate, collisionPoints);
                if (intersectSpaceRegisters.TryGetValue(parent, out var register))
                {
                    register.Add(entry);
                }
                else
                {
                    var newRegister = new IntersectSpaceRegister(parent, 2);
                    newRegister.Add(entry);
                    intersectSpaceRegisters.Add(parent, newRegister);
                }
            }
        }

        if (intersectSpaceRegisters.Count <= 0) return null;

        var result = new IntersectSpaceResult(origin, intersectSpaceRegisters.Count);
        foreach (var register in intersectSpaceRegisters.Values)
        {
            result.Add(register);
        }

        intersectSpaceRegisters.Clear();
        return result;
    }

    /// <summary>
    /// Performs an intersection query for a <see cref="Polyline"/> shape against the collision system.
    /// </summary>
    /// <param name="shape">The polyline shape to test for intersections.</param>
    /// <param name="origin">The origin point for the intersection result.</param>
    /// <param name="collisionMask">The collision mask to filter candidate colliders.</param>
    /// <returns>An <see cref="IntersectSpaceResult"/> containing intersection data, or null if no intersections are found.</returns>
    /// <remarks>
    /// Only enabled colliders are considered for intersection checks in this query.
    /// </remarks>
    public IntersectSpaceResult? IntersectSpace(Polyline shape, Vector2 origin, BitFlag collisionMask)
    {
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();

        broadphaseSpatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return null;

        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                var parent = candidate.Parent;
                if (parent == null) continue;

                var collisionPoints = shape.Intersect(candidate);

                if (collisionPoints == null || !collisionPoints.Valid) continue;

                var entry = new IntersectSpaceEntry(candidate, collisionPoints);
                if (intersectSpaceRegisters.TryGetValue(parent, out var register))
                {
                    register.Add(entry);
                }
                else
                {
                    var newRegister = new IntersectSpaceRegister(parent, 2);
                    newRegister.Add(entry);
                    intersectSpaceRegisters.Add(parent, newRegister);
                }
            }
        }

        if (intersectSpaceRegisters.Count <= 0) return null;

        var result = new IntersectSpaceResult(origin, intersectSpaceRegisters.Count);
        foreach (var register in intersectSpaceRegisters.Values)
        {
            result.Add(register);
        }

        intersectSpaceRegisters.Clear();
        return result;
    }

}