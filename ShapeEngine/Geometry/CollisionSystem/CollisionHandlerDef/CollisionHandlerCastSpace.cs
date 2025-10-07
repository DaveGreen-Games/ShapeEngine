using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.CollisionSystem.CollisionHandlerDef;

public partial class CollisionHandler
{
    /// <summary>
    /// Performs an overlap query for all colliders of a <see cref="CollisionObject"/> against the collision system and stores the results in the provided <see cref="CastSpaceResult"/>.
    /// </summary>
    /// <param name="collisionBody">The collision object whose colliders are used for the cast query. Only enabled colliders from the collisionBody are used!</param>
    /// <param name="result">A reference to the result object that will be populated with colliders that overlap.</param>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Only enabled colliders of the <paramref name="collisionBody"/> are considered.</item>
    /// <item>Only enabled colliders in the collision system are checked.</item>
    /// <item>The result is cleared before being populated.</item>
    /// </list>
    /// </remarks>
    public void CastSpace(CollisionObject collisionBody, ref CastSpaceResult result)
    {
        if (!collisionBody.HasColliders) return;
        CastSpace(collisionBody.Colliders, ref result);
    }

    /// <summary>
    /// Performs an overlap query for a list of <see cref="Collider"/> instances against the collision system and stores the results in the provided <see cref="CastSpaceResult"/>.
    /// </summary>
    /// <param name="colliders">The list of colliders to use for the cast query. Only enabled colliders are used!</param>
    /// <param name="result">A reference to the result object that will be populated with colliders that overlap.</param>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Only enabled colliders of the <paramref name="colliders"/> list are considered.</item>
    /// <item>Only enabled colliders in the collision system are checked.</item>
    /// <item>The result is cleared before being populated.</item>
    /// </list>
    /// </remarks>
    public void CastSpace(List<Collider> colliders, ref CastSpaceResult result)
    {
        if (colliders.Count <= 0) return;
        if (result.Count > 0) result.Clear();

        foreach (var collider in colliders)
        {
            if (!collider.Enabled) continue;

            collisionCandidateBuckets.Clear();
            collisionCandidateCheckRegister.Clear();
            spatialHash.GetCandidateBuckets(collider, ref collisionCandidateBuckets);
            if (collisionCandidateBuckets.Count <= 0) return;

            var mask = collider.CollisionMask;
            foreach (var bucket in collisionCandidateBuckets)
            {
                foreach (var candidate in bucket)
                {
                    if (candidate.Parent == null) continue;
                    if (candidate == collider) continue;
                    if (!mask.Has(candidate.CollisionLayer)) continue;
                    if (!collisionCandidateCheckRegister.Add(candidate)) continue;

                    if (collider.Overlap(candidate))
                    {
                        result.AddCollider(candidate);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Performs an overlap query for a set of <see cref="Collider"/> instances against the collision system and stores the results in the provided <see cref="CastSpaceResult"/>.
    /// </summary>
    /// <param name="colliders">The set of colliders to use for the cast query. Only enabled colliders are used!</param>
    /// <param name="result">A reference to the result object that will be populated with colliders that overlap.</param>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Only enabled colliders of the <paramref name="colliders"/> set are considered.</item>
    /// <item>Only enabled colliders in the collision system are checked.</item>
    /// <item>The result is cleared before being populated.</item>
    /// </list>
    /// </remarks>
    public void CastSpace(HashSet<Collider> colliders, ref CastSpaceResult result)
    {
        if (colliders.Count <= 0) return;
        if (result.Count > 0) result.Clear();

        foreach (var collider in colliders)
        {
            if (!collider.Enabled) continue;
            collisionCandidateBuckets.Clear();
            collisionCandidateCheckRegister.Clear();
            spatialHash.GetCandidateBuckets(collider, ref collisionCandidateBuckets);
            if (collisionCandidateBuckets.Count <= 0) return;

            var mask = collider.CollisionMask;
            foreach (var bucket in collisionCandidateBuckets)
            {
                foreach (var candidate in bucket)
                {
                    if (candidate.Parent == null) continue;
                    if (candidate == collider) continue;
                    if (!mask.Has(candidate.CollisionLayer)) continue;
                    if (!collisionCandidateCheckRegister.Add(candidate)) continue;

                    if (collider.Overlap(candidate))
                    {
                        result.AddCollider(candidate);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Performs an overlap query for a single <see cref="Collider"/> against the collision system and stores the results in the provided <see cref="CastSpaceResult"/>.
    /// </summary>
    /// <param name="collider">The collider to use for the cast query. The collider needs to be enabled!</param>
    /// <param name="result">A reference to the result object that will be populated with colliders that overlap.</param>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Only enabled colliders in the collision system are checked.</item>
    /// <item>The result is cleared before being populated.</item>
    /// </list>
    /// </remarks>
    public void CastSpace(Collider collider, ref CastSpaceResult result)
    {
        if (!collider.Enabled) return;
        if (result.Count > 0) result.Clear();
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();

        spatialHash.GetCandidateBuckets(collider, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return;
        var mask = collider.CollisionMask;
        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (candidate.Parent == null) continue;
                if (candidate == collider) continue;
                if (!mask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;

                if (collider.Overlap(candidate))
                {
                    result.AddCollider(candidate);
                }
            }
        }
    }

    /// <summary>
    /// Performs an overlap query for a <see cref="Segment"/> shape against the collision system and stores the results in the provided <see cref="CastSpaceResult"/>.
    /// </summary>
    /// <param name="shape">The segment shape to use for the cast query.</param>
    /// <param name="collisionMask">The collision mask to filter candidate colliders.</param>
    /// <param name="result">A reference to the result object that will be populated with colliders that overlap.</param>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Only enabled colliders in the collision system are checked.</item>
    /// <item>The result is cleared before being populated.</item>
    /// </list>
    /// </remarks>
    public void CastSpace(Segment shape, BitFlag collisionMask, ref CastSpaceResult result)
    {
        if (result.Count > 0) result.Clear();
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();

        spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return;
        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (candidate.Parent == null) continue;
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;

                if (candidate.Overlap(shape))
                {
                    result.AddCollider(candidate);
                }
            }
        }
    }

    /// <summary>
    /// Performs an overlap query for a <see cref="Line"/> shape against the collision system and stores the results in the provided <see cref="CastSpaceResult"/>.
    /// </summary>
    /// <param name="line">The line shape to use for the cast query.</param>
    /// <param name="collisionMask">The collision mask to filter candidate colliders.</param>
    /// <param name="result">A reference to the result object that will be populated with colliders that overlap.</param>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Only enabled colliders in the collision system are checked.</item>
    /// <item>The result is cleared before being populated.</item>
    /// </list>
    /// </remarks>
    public void CastSpace(Line line, BitFlag collisionMask, ref CastSpaceResult result)
    {
        if (result.Count > 0) result.Clear();
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();

        spatialHash.GetCandidateBuckets(line, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return;
        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (candidate.Parent == null) continue;
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;

                if (candidate.Overlap(line))
                {
                    result.AddCollider(candidate);
                }
            }
        }
    }

    /// <summary>
    /// Performs an overlap query for a <see cref="Ray"/> shape against the collision system and stores the results in the provided <see cref="CastSpaceResult"/>.
    /// </summary>
    /// <param name="ray">The ray shape to use for the cast query.</param>
    /// <param name="collisionMask">The collision mask to filter candidate colliders.</param>
    /// <param name="result">A reference to the result object that will be populated with colliders that overlap.</param>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Only enabled colliders in the collision system are checked.</item>
    /// <item>The result is cleared before being populated.</item>
    /// </list>
    /// </remarks>
    public void CastSpace(Ray ray, BitFlag collisionMask, ref CastSpaceResult result)
    {
        if (result.Count > 0) result.Clear();
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();

        spatialHash.GetCandidateBuckets(ray, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return;
        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (candidate.Parent == null) continue;
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;

                if (candidate.Overlap(ray))
                {
                    result.AddCollider(candidate);
                }
            }
        }
    }

    /// <summary>
    /// Performs an overlap query for a <see cref="Triangle"/> shape against the collision system and stores the results in the provided <see cref="CastSpaceResult"/>.
    /// </summary>
    /// <param name="shape">The triangle shape to use for the cast query.</param>
    /// <param name="collisionMask">The collision mask to filter candidate colliders.</param>
    /// <param name="result">A reference to the result object that will be populated with colliders that overlap.</param>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Only enabled colliders in the collision system are checked.</item>
    /// <item>The result is cleared before being populated.</item>
    /// </list>
    /// </remarks>
    public void CastSpace(Triangle shape, BitFlag collisionMask, ref CastSpaceResult result)
    {
        if (result.Count > 0) result.Clear();
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();

        spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return;
        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (candidate.Parent == null) continue;
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;

                if (candidate.Overlap(shape))
                {
                    result.AddCollider(candidate);
                }
            }
        }
    }

    /// <summary>
    /// Performs an overlap query for a <see cref="Circle"/> shape against the collision system and stores the results in the provided <see cref="CastSpaceResult"/>.
    /// </summary>
    /// <param name="shape">The circle shape to use for the cast query.</param>
    /// <param name="collisionMask">The collision mask to filter candidate colliders.</param>
    /// <param name="result">A reference to the result object that will be populated with colliders that overlap.</param>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Only enabled colliders in the collision system are checked.</item>
    /// <item>The result is cleared before being populated.</item>
    /// </list>
    /// </remarks>
    public void CastSpace(Circle shape, BitFlag collisionMask, ref CastSpaceResult result)
    {
        if (result.Count > 0) result.Clear();
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();

        spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return;
        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (candidate.Parent == null) continue;
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;

                if (candidate.Overlap(shape))
                {
                    result.AddCollider(candidate);
                }
            }
        }
    }

    /// <summary>
    /// Performs an overlap query for a <see cref="Rect"/> shape against the collision system and stores the results in the provided <see cref="CastSpaceResult"/>.
    /// </summary>
    /// <param name="shape">The rectangle shape to use for the cast query.</param>
    /// <param name="collisionMask">The collision mask to filter candidate colliders.</param>
    /// <param name="result">A reference to the result object that will be populated with colliders that overlap.</param>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Only enabled colliders in the collision system are checked.</item>
    /// <item>The result is cleared before being populated.</item>
    /// </list>
    /// </remarks>
    public void CastSpace(Rect shape, BitFlag collisionMask, ref CastSpaceResult result)
    {
        if (result.Count > 0) result.Clear();
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();

        spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return;
        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (candidate.Parent == null) continue;
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;

                if (candidate.Overlap(shape))
                {
                    result.AddCollider(candidate);
                }
            }
        }
    }

    /// <summary>
    /// Performs an overlap query for a <see cref="Polygon"/> shape against the collision system and stores the results in the provided <see cref="CastSpaceResult"/>.
    /// </summary>
    /// <param name="shape">The polygon shape to use for the cast query.</param>
    /// <param name="collisionMask">The collision mask to filter candidate colliders.</param>
    /// <param name="result">A reference to the result object that will be populated with colliders that overlap.</param>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Only enabled colliders in the collision system are checked.</item>
    /// <item>The result is cleared before being populated.</item>
    /// </list>
    /// </remarks>
    public void CastSpace(Polygon shape, BitFlag collisionMask, ref CastSpaceResult result)
    {
        if (result.Count > 0) result.Clear();
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();

        spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return;
        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (candidate.Parent == null) continue;
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;

                if (candidate.Overlap(shape))
                {
                    result.AddCollider(candidate);
                }
            }
        }
    }

    /// <summary>
    /// Performs an overlap query for a <see cref="Polyline"/> shape against the collision system and stores the results in the provided <see cref="CastSpaceResult"/>.
    /// </summary>
    /// <param name="shape">The polyline shape to use for the cast query.</param>
    /// <param name="collisionMask">The collision mask to filter candidate colliders.</param>
    /// <param name="result">A reference to the result object that will be populated with colliders that overlap.</param>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Only enabled colliders in the collision system are checked.</item>
    /// <item>The result is cleared before being populated.</item>
    /// </list>
    /// </remarks>
    public void CastSpace(Polyline shape, BitFlag collisionMask, ref CastSpaceResult result)
    {
        if (result.Count > 0) result.Clear();
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();

        spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return;
        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (candidate.Parent == null) continue;
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;

                if (candidate.Overlap(shape))
                {
                    result.AddCollider(candidate);
                }
            }
        }
    }

    /// <summary>
    /// Performs an overlap query for all colliders of a <see cref="CollisionObject"/> against the collision system.
    /// </summary>
    /// <param name="collisionBody">The collision object whose colliders are used for the cast query. Only enabled colliders from the collisionBody are used!</param>
    /// <returns>The number of colliders in the system that overlap with any collider of the given <paramref name="collisionBody"/>.</returns>
    /// <remarks>
    /// Only enabled colliders of the <paramref name="collisionBody"/> are considered.
    /// Only enabled colliders in the collision system are checked.
    /// </remarks>
    public int CastSpace(CollisionObject collisionBody)
    {
        if (!collisionBody.HasColliders) return 0;

        int count = 0;
        foreach (var collider in collisionBody.Colliders)
        {
            if (!collider.Enabled) continue;

            collisionCandidateBuckets.Clear();
            collisionCandidateCheckRegister.Clear();
            spatialHash.GetCandidateBuckets(collider, ref collisionCandidateBuckets);
            if (collisionCandidateBuckets.Count <= 0) return 0;

            var mask = collider.CollisionMask;
            foreach (var bucket in collisionCandidateBuckets)
            {
                foreach (var candidate in bucket)
                {
                    if (candidate == collider) continue;
                    if (!mask.Has(candidate.CollisionLayer)) continue;
                    if (!collisionCandidateCheckRegister.Add(candidate)) continue;

                    if (collider.Overlap(candidate))
                    {
                        count++;
                    }
                }
            }
        }

        return count;
    }

    /// <summary>
    /// Performs an overlap query for a single <see cref="Collider"/> against the collision system.
    /// </summary>
    /// <param name="collider">The collider to use for the cast query. The collider needs to be enabled!</param>
    /// <returns>The number of colliders in the system that overlap with the given <paramref name="collider"/>.</returns>
    /// <remarks>
    /// Only enabled colliders in the collision system are checked.
    /// </remarks>
    public int CastSpace(Collider collider)
    {
        if (!collider.Enabled) return 0;
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();

        spatialHash.GetCandidateBuckets(collider, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return 0;

        int count = 0;
        var mask = collider.CollisionMask;
        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (candidate == collider) continue;
                if (!mask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;

                if (collider.Overlap(candidate))
                {
                    count++;
                }
            }
        }

        return count;
    }

    /// <summary>
    /// Performs an overlap query for a <see cref="Segment"/> shape against the collision system.
    /// </summary>
    /// <param name="shape">The segment shape to use for the cast query.</param>
    /// <param name="collisionMask">The collision mask to filter candidate colliders.</param>
    /// <returns>The number of colliders in the system that overlap with the given <paramref name="shape"/>.</returns>
    /// <remarks>
    /// Only enabled colliders in the collision system are checked.
    /// </remarks>
    public int CastSpace(Segment shape, BitFlag collisionMask)
    {
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();

        spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return 0;

        int count = 0;
        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;

                if (candidate.Overlap(shape))
                {
                    count++;
                }
            }
        }

        return count;
    }

    /// <summary>
    /// Performs an overlap query for a <see cref="Line"/> shape against the collision system.
    /// </summary>
    /// <param name="line">The segment shape to use for the cast query.</param>
    /// <param name="collisionMask">The collision mask to filter candidate colliders.</param>
    /// <returns>The number of colliders in the system that overlap with the given <paramref name="line"/>.</returns>
    /// <remarks>
    /// Only enabled colliders in the collision system are checked.
    /// </remarks>
    public int CastSpace(Line line, BitFlag collisionMask)
    {
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();

        spatialHash.GetCandidateBuckets(line, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return 0;

        int count = 0;
        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;

                if (candidate.Overlap(line))
                {
                    count++;
                }
            }
        }

        return count;
    }

    /// <summary>
    /// Performs an overlap query for a <see cref="Ray"/> shape against the collision system.
    /// </summary>
    /// <param name="ray">The segment shape to use for the cast query.</param>
    /// <param name="collisionMask">The collision mask to filter candidate colliders.</param>
    /// <returns>The number of colliders in the system that overlap with the given <paramref name="ray"/>.</returns>
    /// <remarks>
    /// Only enabled colliders in the collision system are checked.
    /// </remarks>
    public int CastSpace(Ray ray, BitFlag collisionMask)
    {
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();

        spatialHash.GetCandidateBuckets(ray, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return 0;

        int count = 0;
        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;

                if (candidate.Overlap(ray))
                {
                    count++;
                }
            }
        }

        return count;
    }

    /// <summary>
    /// Performs an overlap query for a <see cref="Triangle"/> shape against the collision system.
    /// </summary>
    /// <param name="shape">The segment shape to use for the cast query.</param>
    /// <param name="collisionMask">The collision mask to filter candidate colliders.</param>
    /// <returns>The number of colliders in the system that overlap with the given <paramref name="shape"/>.</returns>
    /// <remarks>
    /// Only enabled colliders in the collision system are checked.
    /// </remarks>
    public int CastSpace(Triangle shape, BitFlag collisionMask)
    {
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();

        spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return 0;

        int count = 0;
        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;

                if (candidate.Overlap(shape))
                {
                    count++;
                }
            }
        }

        return count;
    }

    /// <summary>
    /// Performs an overlap query for a <see cref="Circle"/> shape against the collision system.
    /// </summary>
    /// <param name="shape">The segment shape to use for the cast query.</param>
    /// <param name="collisionMask">The collision mask to filter candidate colliders.</param>
    /// <returns>The number of colliders in the system that overlap with the given <paramref name="shape"/>.</returns>
    /// <remarks>
    /// Only enabled colliders in the collision system are checked.
    /// </remarks>
    public int CastSpace(Circle shape, BitFlag collisionMask)
    {
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();

        spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return 0;

        int count = 0;
        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;

                if (candidate.Overlap(shape))
                {
                    count++;
                }
            }
        }

        return count;
    }

    /// <summary>
    /// Performs an overlap query for a <see cref="Rect"/> shape against the collision system.
    /// </summary>
    /// <param name="shape">The segment shape to use for the cast query.</param>
    /// <param name="collisionMask">The collision mask to filter candidate colliders.</param>
    /// <returns>The number of colliders in the system that overlap with the given <paramref name="shape"/>.</returns>
    /// <remarks>
    /// Only enabled colliders in the collision system are checked.
    /// </remarks>
    public int CastSpace(Rect shape, BitFlag collisionMask)
    {
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();

        spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return 0;

        int count = 0;
        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;

                if (candidate.Overlap(shape))
                {
                    count++;
                }
            }
        }

        return count;
    }

    /// <summary>
    /// Performs an overlap query for a <see cref="Polygon"/> shape against the collision system.
    /// </summary>
    /// <param name="shape">The segment shape to use for the cast query.</param>
    /// <param name="collisionMask">The collision mask to filter candidate colliders.</param>
    /// <returns>The number of colliders in the system that overlap with the given <paramref name="shape"/>.</returns>
    /// <remarks>
    /// Only enabled colliders in the collision system are checked.
    /// </remarks>
    public int CastSpace(Polygon shape, BitFlag collisionMask)
    {
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();

        spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return 0;

        int count = 0;
        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;

                if (candidate.Overlap(shape))
                {
                    count++;
                }
            }
        }

        return count;
    }

    /// <summary>
    /// Performs an overlap query for a <see cref="Polyline"/> shape against the collision system.
    /// </summary>
    /// <param name="shape">The segment shape to use for the cast query.</param>
    /// <param name="collisionMask">The collision mask to filter candidate colliders.</param>
    /// <returns>The number of colliders in the system that overlap with the given <paramref name="shape"/>.</returns>
    /// <remarks>
    /// Only enabled colliders in the collision system are checked.
    /// </remarks>
    public int CastSpace(Polyline shape, BitFlag collisionMask)
    {
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();

        spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return 0;

        int count = 0;
        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;

                if (candidate.Overlap(shape))
                {
                    count++;
                }
            }
        }

        return count;
    }

    /// <summary>
    /// Sorts the given list of <see cref="Collider"/> instances in-place by their distance to the specified <paramref name="sortOrigin"/>.
    /// </summary>
    /// <param name="result">A reference to the list of colliders to sort.</param>
    /// <param name="sortOrigin">The origin point used for sorting by distance.</param>
    public void SortCastResult(ref List<Collider> result, Vector2 sortOrigin)
    {
        if (result.Count > 1)
        {
            result.Sort
            ((a, b) =>
                {
                    float la = (sortOrigin - a.CurTransform.Position).LengthSquared();
                    float lb = (sortOrigin - b.CurTransform.Position).LengthSquared();

                    if (la > lb) return 1;
                    if (ShapeMath.EqualsF(la, lb)) return 0;
                    return -1;
                }
            );
        }
    }

}