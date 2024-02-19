using System.Numerics;
using ShapeEngine.Core.Collision;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Core.Interfaces;

// public interface ISpatialTest
// {
//     public Vector2 Position { get; set; }
//     public Rect GetBoundingBox();
//         
// }
// public interface IUpdateableTest
// {
//     public void Update(GameTime time, ScreenInfo game, ScreenInfo ui);
// }
// public interface IDrawableTest
// {
//     public void DrawGame(ScreenInfo game);
//     public void DrawGameUI(ScreenInfo ui);
//     
// }
// public interface IKillableTest
// {
//     public bool Kill();
//     public bool IsDead();
// }
// public interface IGameObjectTest : ISpatialTest, IUpdateableTest, IDrawableTest, IKillableTest//, IBehaviorReceiver
// {
//
//     public bool DrawToGame(Rect gameArea);
//     public bool DrawToGameUI(Rect screenArea);
//         
//     public int Layer { get; set; }
//     public virtual void UpdateParallaxe(Vector2 newParallaxPosition) { }
//     public sealed bool IsInLayer(int layer) { return this.Layer == layer; }
//     public void AddedToHandler(GameObjectHandler gameObjectHandler);
//     public void RemovedFromHandler(GameObjectHandler gameObjectHandler);
//     public bool CheckHandlerBounds();
//     public void LeftHandlerBounds(BoundsCollisionInfo info);
//     // public virtual bool HasCollisionBody() { return false; }
//     // public virtual CollisionBodyTest? GetCollisionBody() { return null; }
//
// }
// public interface IPhysicsObjectTest : IGameObjectTest
// {
//     public Vector2 Velocity { get; set; }
//     public float Mass { get; set; }
//     public float Drag { get; set; }
//     public Vector2 ConstAcceleration { get; set; }
//     public void AddForce(Vector2 force);
//     public void AddImpulse(Vector2 force);
//     public bool IsStatic(float deltaSq) { return Velocity.LengthSquared() <= deltaSq; }
//     public Vector2 GetAccumulatedForce();
//     public void ClearAccumulatedForce();
//     public void UpdateState(float dt);
// }
//
// public class CollisionBodyTest : IPhysicsObjectTest
// {
//     
// }


public interface IGameObject : ISpatial, IUpdateable, IDrawable, IKillable//, IBehaviorReceiver
{

    public bool DrawToGame(Rect gameArea);
    public bool DrawToGameUI(Rect screenArea);
        
    /// <summary>
    /// The area layer the object is stored in. Higher layers are draw on top of lower layers.
    /// </summary>
    public int Layer { get; set; }
    /// <summary>
    /// Is called by the area. Can be used to update the objects position based on the new parallax position.
    /// </summary>
    /// <param name="newParallaxPosition">The new parallax position from the layer the object is in.</param>
    public virtual void UpdateParallaxe(Vector2 newParallaxPosition) { }

    /// <summary>
    /// Check if the object is in a layer.
    /// </summary>
    /// <param name="layer"></param>
    /// <returns></returns>
    public sealed bool IsInLayer(int layer) { return this.Layer == layer; }

    /// <summary>
    /// Is called when gameobject is added to an area.
    /// </summary>
    public void AddedToHandler(GameObjectHandler gameObjectHandler);
    /// <summary>
    /// Is called by the area once a game object is dead.
    /// </summary>
    public void RemovedFromHandler(GameObjectHandler gameObjectHandler);

    /// <summary>
    /// Should this object be checked for leaving the bounds of the area?
    /// </summary>
    /// <returns></returns>
    public bool CheckHandlerBounds();
    /// <summary>
    /// Will be called if the object left the bounds of the area. The BoundingCircle is used for this check.
    /// </summary>
    /// <param name="info">The info about where the object left the bounds.</param>
    public void LeftHandlerBounds(BoundsCollisionInfo info);
        
    ///// <summary>
    ///// Can be used to adjust the follow position of an attached camera.
    ///// </summary>
    ///// <param name="camPos"></param>
    ///// <returns></returns>
    //public Vector2 GetCameraFollowPosition(Vector2 camPos);

    /// <summary>
    /// Should the area add the collidables from this object to the collision system on area entry.
    /// </summary>
    /// <returns></returns>
    public virtual bool HasCollisionBody() { return false; }
    /// <summary>
    /// All the collidables that should be added to the collision system on area entry.
    /// </summary>
    /// <returns></returns>
    public virtual CollisionBody? GetCollisionBody() { return null; }

}