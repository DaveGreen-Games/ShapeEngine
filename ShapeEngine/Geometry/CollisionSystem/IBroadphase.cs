using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Geometry.CollisionSystem;

/// <summary>
/// Represents a broadphase collision detection system interface.
/// Inherits from <see cref="IBounds"/>.
/// Provides methods for managing collision objects and retrieving candidate buckets for collision checks.
/// <see cref="CollisionHandlerDef.CollisionHandler"/> is using this interface to perform broadphase collision detection.
/// </summary>
public interface IBroadphase : IBounds
{
    /// <summary>
    /// Populates the broadphase system with the provided collection of collision bodies.
    /// </summary>
    /// <param name="collisionBodies">An enumerable collection of <see cref="CollisionObject"/> instances to add to the broadphase.</param>
    /// <remarks>
    /// The collision handler will call fill every frame before performing collision checks. You can either rebuild the broadphase from scratch or update existing entries.
    /// <paramref name="collisionBodies"/> contains all active collision bodies currently in the system.
    /// </remarks>
    public void Fill(IEnumerable<CollisionObject> collisionBodies);
    /// <summary>
    /// Closes and cleans up the broadphase system, releasing any resources if necessary.
    /// </summary>
    public void Close();
    /// <summary>
    /// Draws debug visualization for the broadphase system using the specified border and fill colors.
    /// </summary>
    /// <param name="border">The color to use for drawing borders.</param>
    /// <param name="fill">The color to use for filling shapes.</param>
    public void DebugDraw(ColorRgba border, ColorRgba fill);

    /// <summary>
    /// Retrieves candidate buckets for collision checks based on the specified <see cref="CollisionObject"/>.
    /// </summary>
    /// <param name="collisionObject">The collision object to check.</param>
    /// <param name="candidateBuckets">A reference to the list to populate with candidate buckets.</param>
    /// <returns>The number of candidate buckets found.</returns>
    public int GetCandidateBuckets(CollisionObject collisionObject, ref List<BroadphaseBucket> candidateBuckets);

    /// <summary>
    /// Retrieves candidate buckets for collision checks based on the specified <see cref="Collider"/>.
    /// </summary>
    /// <param name="collider">The collider to check.</param>
    /// <param name="candidateBuckets">A reference to the list to populate with candidate buckets.</param>
    /// <returns>The number of candidate buckets found.</returns>
    public int GetCandidateBuckets(Collider collider, ref List<BroadphaseBucket> candidateBuckets);

    /// <summary>
    /// Retrieves candidate buckets for collision checks based on the specified point.
    /// </summary>
    /// <param name="point">The point to check.</param>
    /// <param name="candidateBuckets">A reference to the list to populate with candidate buckets.</param>
    /// <returns>The number of candidate buckets found.</returns>
    public int GetCandidateBuckets(Vector2 point, ref List<BroadphaseBucket> candidateBuckets);

    /// <summary>
    /// Retrieves candidate buckets for collision checks based on the specified <see cref="Segment"/>.
    /// </summary>
    /// <param name="segment">The segment to check.</param>
    /// <param name="candidateBuckets">A reference to the list to populate with candidate buckets.</param>
    /// <returns>The number of candidate buckets found.</returns>
    public int GetCandidateBuckets(Segment segment, ref List<BroadphaseBucket> candidateBuckets);

    /// <summary>
    /// Retrieves candidate buckets for collision checks based on the specified <see cref="Line"/>.
    /// </summary>
    /// <param name="line">The line to check.</param>
    /// <param name="candidateBuckets">A reference to the list to populate with candidate buckets.</param>
    /// <returns>The number of candidate buckets found.</returns>
    public int GetCandidateBuckets(Line line, ref List<BroadphaseBucket> candidateBuckets);

    /// <summary>
    /// Retrieves candidate buckets for collision checks based on the specified <see cref="Ray"/>.
    /// </summary>
    /// <param name="ray">The ray to check.</param>
    /// <param name="candidateBuckets">A reference to the list to populate with candidate buckets.</param>
    /// <returns>The number of candidate buckets found.</returns>
    public int GetCandidateBuckets(Ray ray, ref List<BroadphaseBucket> candidateBuckets);

    /// <summary>
    /// Retrieves candidate buckets for collision checks based on the specified <see cref="Circle"/>.
    /// </summary>
    /// <param name="circle">The circle to check.</param>
    /// <param name="candidateBuckets">A reference to the list to populate with candidate buckets.</param>
    /// <returns>The number of candidate buckets found.</returns>
    public int GetCandidateBuckets(Circle circle, ref List<BroadphaseBucket> candidateBuckets);

    /// <summary>
    /// Retrieves candidate buckets for collision checks based on the specified <see cref="Triangle"/>.
    /// </summary>
    /// <param name="triangle">The triangle to check.</param>
    /// <param name="candidateBuckets">A reference to the list to populate with candidate buckets.</param>
    /// <returns>The number of candidate buckets found.</returns>
    public int GetCandidateBuckets(Triangle triangle, ref List<BroadphaseBucket> candidateBuckets);

    /// <summary>
    /// Retrieves candidate buckets for collision checks based on the specified <see cref="Rect"/>.
    /// </summary>
    /// <param name="rect">The rectangle to check.</param>
    /// <param name="candidateBuckets">A reference to the list to populate with candidate buckets.</param>
    /// <returns>The number of candidate buckets found.</returns>
    public int GetCandidateBuckets(Rect rect, ref List<BroadphaseBucket> candidateBuckets);

    /// <summary>
    /// Retrieves candidate buckets for collision checks based on the specified <see cref="Quad"/>.
    /// </summary>
    /// <param name="quad">The quad to check.</param>
    /// <param name="candidateBuckets">A reference to the list to populate with candidate buckets.</param>
    /// <returns>The number of candidate buckets found.</returns>
    public int GetCandidateBuckets(Quad quad, ref List<BroadphaseBucket> candidateBuckets);

    /// <summary>
    /// Retrieves candidate buckets for collision checks based on the specified <see cref="Polygon"/>.
    /// </summary>
    /// <param name="poly">The polygon to check.</param>
    /// <param name="candidateBuckets">A reference to the list to populate with candidate buckets.</param>
    /// <returns>The number of candidate buckets found.</returns>
    public int GetCandidateBuckets(Polygon poly, ref List<BroadphaseBucket> candidateBuckets);

    /// <summary>
    /// Retrieves candidate buckets for collision checks based on the specified <see cref="Polyline"/>.
    /// </summary>
    /// <param name="polyLine">The polyline to check.</param>
    /// <param name="candidateBuckets">A reference to the list to populate with candidate buckets.</param>
    /// <returns>The number of candidate buckets found.</returns>
    public int GetCandidateBuckets(Polyline polyLine, ref List<BroadphaseBucket> candidateBuckets);
}