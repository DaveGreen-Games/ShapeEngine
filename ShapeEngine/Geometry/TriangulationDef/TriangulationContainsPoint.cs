using System.Numerics;

namespace ShapeEngine.Geometry.TriangulationDef;

public partial class Triangulation
{
    public bool ContainsPoint(Vector2 p)
    {
        foreach (var tri in this)
        {
            if (tri.ContainsPoint(p)) return true;
        }

        return false;
    }

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