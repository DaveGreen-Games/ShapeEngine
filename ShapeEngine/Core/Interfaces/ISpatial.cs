using System.Numerics;
using ShapeEngine.Core.Shapes;
namespace ShapeEngine.Core.Interfaces;

public interface ISpatial
{
    /// <summary>
    /// Get the current position of the object.
    /// </summary>
    /// <returns></returns>
    public Vector2 GetPosition();
        
    /// <summary>
    /// Get the current bounding box of the object.
    /// </summary>
    /// <returns></returns>
    public Rect GetBoundingBox();
        
}