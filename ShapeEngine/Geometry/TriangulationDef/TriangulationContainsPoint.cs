using System.Numerics;

namespace ShapeEngine.Geometry.TriangulationDef;


public partial class Triangulation
{
    /// <summary>
    /// Determines whether the specified point is contained within any triangle in the triangulation.
    /// </summary>
    /// <param name="p">The point to check for containment.</param>
    /// <returns><c>true</c> if the point is contained in any triangle; otherwise, <c>false</c>.</returns>
    /// <remarks>Returns as soon as a containing triangle is found.</remarks>
    public bool ContainsPoint(Vector2 p)
    {
        foreach (var tri in this)
        {
            if (tri.ContainsPoint(p)) return true;
        }

        return false;
    }

    /// <summary>
    /// Determines whether the specified point is contained within any triangle in the triangulation and returns the index of the containing triangle.
    /// </summary>
    /// <param name="p">The point to check for containment.</param>
    /// <param name="triangleIndex">The index of the triangle that contains the point, or -1 if not found.</param>
    /// <returns><c>true</c> if the point is contained in any triangle; otherwise, <c>false</c>.</returns>
    /// <remarks>Returns as soon as a containing triangle is found. The index is set to -1 if the point is not contained in any triangle.</remarks>
    public bool ContainsPoint(Vector2 p, out int triangleIndex)
    {
        for (int i = 0; i < Count; i++)
        {
            var tri = this[i];
            if (tri.ContainsPoint(p))
            {
                triangleIndex = i;
                return true;
            }
        }

        triangleIndex = -1;
        return false;
    }

}