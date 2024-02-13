using ShapeEngine.Core.Collision;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.Interfaces;

public interface ICollidable
{
    public ICollider GetCollider();
    public void Overlap(CollisionInformation info);
    public void OverlapEnded(ICollidable other);
    public uint GetCollisionLayer();
    public BitFlag GetCollisionMask();
}