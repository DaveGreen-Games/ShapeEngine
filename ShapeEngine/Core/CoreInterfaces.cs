

using ShapeEngine.Screen;
using System.Numerics;

namespace ShapeEngine.Core
{
    public interface ISpatial
    {
        /// <summary>
        /// Get the current position of the object.
        /// </summary>
        /// <returns></returns>
        public Vector2 GetPosition();
        /*
        /// <summary>
        /// The current bounding circle of the object.
        /// </summary>
        /// <returns></returns>
        public Circle GetBoundingCircle();
        */
        /// <summary>
        /// Get the current bounding box of the object.
        /// </summary>
        /// <returns></returns>
        public Rect GetBoundingBox();
        /*
        /// <summary>
        /// The current bounding radius of the object. Is used to generate the bounding circle.
        /// </summary>
        /// <returns></returns>
        public float GetBoundingRadius();
        /// <summary>
        /// The current bounding circle of the radius. Is constructed out of the position and bounding radius of the object.
        /// </summary>
        /// <returns></returns>
        public Circle GetBoundingCircle();

        
        */
    }
    public interface IUpdateable
    {
        public void Update(float dt, Vector2 mousePosScreen, ScreenTexture game, ScreenTexture ui);
    }
    public interface IDrawable
    {
        public void DrawGame(Vector2 size, Vector2 mousePos);
        public void DrawUI(Vector2 size, Vector2 mousePos);
        //public void DrawToTexture(ScreenTexture texture);
        //public void DrawToScreen(Vector2 size, Vector2 mousePos);
    }
    public interface IKillable
    {
        /// <summary>
        /// Try to kill the object.
        /// </summary>
        /// <returns>Return true if kill was successful.</returns>
        public bool Kill();
        /// <summary>
        /// Check if object is dead.
        /// </summary>
        /// <returns></returns>
        public bool IsDead();
    }
    
    public interface IScene : IUpdateable, IDrawable
    {

        public Area? GetCurArea();


        public void Activate(IScene oldScene);// { }
        public void Deactivate();

        
        /// <summary>
        /// Used for cleanup. Should be called once right before the scene gets deleted.
        /// </summary>
        public void Close();

        public void DrawToTexture(ScreenTexture texture);
        public void DrawToScreen(Vector2 size, Vector2 mousePos);

    }
    

    public interface IAreaDeltaFactor
    {
        public int ApplyOrder { get; set; }

        public uint GetID();
       
        public bool IsAffectingLayer(int layer);

        /// <summary>
        /// Update the delta factor.
        /// </summary>
        /// <param name="dt"></param>
        /// <returns>Returns true when finished.</returns>
        public bool Update(float dt);

        /// <summary>
        /// Recieves the current total delta factor.
        /// </summary>
        /// <param name="totalFactor">The current total delta factor.</param>
        /// <returns>Returns the new total delta factor.</returns>
        public float Apply(float totalFactor);
    }
    public interface IAreaObject : ISpatial, IUpdateable, IDrawable, IKillable//, IBehaviorReceiver
    {
        
        //public bool IsDrawingToScreen();
        /// <summary>
        /// Tells the area to call DrawGame on this object. Can be used to temporarily not draw the object to the screen,
        /// for instance when it is not inside the camera area.
        /// </summary>
        /// <returns></returns>
        public bool IsDrawingToGameTexture();
        /// <summary>
        /// Tells the area to call DrawUI on this object.
        /// </summary>
        /// <returns></returns>
        public bool IsDrawingToUITexture();

        /// <summary>
        /// The area layer the object is stored in. Higher layers are draw on top of lower layers.
        /// </summary>
        public int AreaLayer { get; set; }
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
        public sealed bool IsInLayer(int layer) { return this.AreaLayer == layer; }

        /// <summary>
        /// Is called when gameobject is added to an area.
        /// </summary>
        public void AddedToArea(Area area);
        /// <summary>
        /// Is called by the area once a game object is dead.
        /// </summary>
        public void RemovedFromArea(Area area);

        /// <summary>
        /// Should this object be checked for leaving the bounds of the area?
        /// </summary>
        /// <returns></returns>
        public bool CheckAreaBounds();
        /// <summary>
        /// Will be called if the object left the bounds of the area. The BoundingCircle is used for this check.
        /// </summary>
        /// <param name="safePosition">The closest position within the bounds.</param>
        /// <param name="collisionPoints">The points where the object left the bounds. Can be 1 or 2 max.</param>
        public void LeftAreaBounds(Vector2 safePosition, CollisionPoints collisionPoints);
        
        /// <summary>
        /// Can be used to adjust the follow position of an attached camera.
        /// </summary>
        /// <param name="camPos"></param>
        /// <returns></returns>
        public Vector2 GetCameraFollowPosition(Vector2 camPos);

        /// <summary>
        /// Should the area add the collidables from this object to the collision system on area entry.
        /// </summary>
        /// <returns></returns>
        public virtual bool HasCollidables() { return false; }
        /// <summary>
        /// All the collidables that should be added to the collision system on area entry.
        /// </summary>
        /// <returns></returns>
        public virtual List<ICollidable> GetCollidables() { return new(); }


        /// <summary>
        ///  Is called right after update if a delta factor was applied to the objects dt.
        /// </summary>
        /// <param name="f">The factor that was applied.</param>
        public void DeltaFactorApplied(float f);
    }
    public interface IBounds
    {
        public Rect Bounds { get; }
        public void ResizeBounds(Rect newBounds);
    }
    
    /*
    public interface IArea : IUpdateable, IDrawable, IBounds
    {
        public int Count { get; }

        public ICollisionHandler? GetCollisionHandler();

        /// <summary>
        /// The parallaxe position for this area. Every IAreaObject that uses parallaxe scales its position based on this position.
        /// For instance could be set to the players position or the cameras position that follows the player.
        /// </summary>
        public Vector2 ParallaxePosition { get; set; }

        public void AddDeltaFactor(IAreaDeltaFactor deltaFactor);
        public bool RemoveDeltaFactor(IAreaDeltaFactor deltaFactor);
        public bool RemoveDeltaFactor(uint id);


        public List<IAreaObject> GetAreaObjects(int layer, Predicate<IAreaObject> match);
        public List<IAreaObject> GetAllGameObjects();
        public List<IAreaObject> GetAllGameObjects(Predicate<IAreaObject> match);


        public bool HasLayer(int layer);

        public void AddAreaObject(IAreaObject areaObjects);
        public void AddAreaObjects(params IAreaObject[] areaObjects);
        public void AddAreaObjects(IEnumerable<IAreaObject> areaObjects);

        public void RemoveAreaObject(IAreaObject areaObject);
        public void RemoveAreaObjects(Predicate<IAreaObject> match);
        public void RemoveAreaObjects(int layer, Predicate<IAreaObject> match);
        public void RemoveAreaObjects(params IAreaObject[] areaObjects);
        public void RemoveAreaObjects(IEnumerable<IAreaObject> areaObjects);

        public void Clear();
        public void ClearLayer(int layer);

        public void DrawToTexture(ScreenTexture texture);

        public void Start();
        public void Close();
    }
    public interface ICollisionHandler : IBounds
    {
        public int Count { get; }


        public void Add(ICollidable collidable);
        public void AddRange(IEnumerable<ICollidable> collidables);
        public void AddRange(params ICollidable[] collidables);
        public void Remove(ICollidable collidable);
        public void RemoveRange(IEnumerable<ICollidable> collidables);
        public void RemoveRange(params ICollidable[] collidables);

        public void Clear();
        public void Close();

        public void Update(float dt);
        public List<QueryInfo> QuerySpace(ICollidable collidable, bool sorted = false);
        public List<QueryInfo> QuerySpace(ICollider collider, bool sorted = false, params uint[] collisionMask);
        public List<QueryInfo> QuerySpace(IShape shape, bool sorted = false, params uint[] collisionMask);
        public List<QueryInfo> QuerySpace(IShape shape, ICollidable[] exceptions, bool sorted = false, params uint[] collisionMask);


        public List<ICollidable> CastSpace(ICollidable collidable, bool sorted = false);
        public List<ICollidable> CastSpace(ICollider collider, bool sorted = false, params uint[] collisionMask);
        public List<ICollidable> CastSpace(IShape castShape, bool sorted = false, params uint[] collisionMask);

    }
    */
   
    public interface ICollidable
    {
        public ICollider GetCollider();
        public void Overlap(CollisionInformation info);
        public void OverlapEnded(ICollidable other);
        public uint GetCollisionLayer();
        public uint[] GetCollisionMask();
    }
    public interface ICollider : IPhysicsObject
    {
        //public Vector2 PrevPos { get; set; }

        /// <summary>
        /// If disabled this collider will not take part in collision detection.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// If disabled this collider will not compute collision but other colliders can still collide with it.
        /// </summary>
        public bool ComputeCollision { get; set; }

        /// <summary>
        /// Should this collider compute intersections with other shapes or just overlaps.
        /// </summary>
        public bool ComputeIntersections { get; set; }

        /// <summary>
        /// Treat all other closed shapes as circles. (not segment or polyline)
        /// Still uses the actual shape for this collider.
        /// If used with CCD any closed shape collision will be a circle/ circle collision
        /// </summary>
        public bool SimplifyCollision { get; set; }

        public bool FlippedNormals { get; set; }

        ///// <summary>
        ///// Enables Continous Collision Detection. Works best for small & fast objects that might tunnel through other shapes especially segments.
        ///// Only works for closed shapes. (not segments or polylines)
        ///// Automatically uses the bounding circle for collision checking, not the actual shape.
        ///// Tunneling occurs when a shape does not collide in the current frame and then moves to the other side of an object in the next frame.
        ///// </summary>
        //public bool CCD { get; set; }

        //public Vector2 GetPrevPos(); // { return Pos; }
        //public void UpdatePrevPos(float dt);// { }
        public IShape GetShape();
        public IShape GetSimplifiedShape();

        public Vector2 GetPreviousPosition();
        public void UpdatePreviousPosition(float dt);

        public void DrawShape(float lineThickness, Raylib_CsLo.Color color);
    }
    public interface IShape
    {
        /// <summary>
        /// All normals face outwards of shapes per default or face right along the direction of segments.
        /// If flipped normals is true all normals face inwards of shapes or face left along the direction of segments.
        /// </summary>
        public bool FlippedNormals { get; set; }
        
        
        public Rect GetBoundingBox();
        public Circle GetBoundingCircle();
        public Vector2 GetCentroid();
        public CollisionPoint GetClosestPoint(Vector2 p);
        public bool ContainsPoint(Vector2 p);
        
        //public Points GetVertices();
        //public Polygon ToPolygon();
        //public Polyline ToPolyline();
        //public Segments GetEdges();
        //public Triangulation Triangulate();
        
        
        //public void DrawShape(float linethickness, Raylib_CsLo.Color color);
        
        
        //public float GetArea();
        //public float GetCircumference();
        //public float GetCircumferenceSquared();
        //public Vector2 GetClosestVertex(Vector2 p);
        //public Vector2 GetRandomPoint();
        //public Points GetRandomPoints(int amount);
        //public Vector2 GetRandomVertex();
        //public Segment GetRandomEdge();
        //public Vector2 GetRandomPointOnEdge();
        //public Points GetRandomPointsOnEdge(int amount);
        
    }
    public interface IPhysicsObject
    {
        public Vector2 Pos { get; set; }
        public Vector2 Vel { get; set; }
        public float Mass { get; set; }
        public float Drag { get; set; }
        public Vector2 ConstAcceleration { get; set; }
        public void AddForce(Vector2 force);
        public void AddImpulse(Vector2 force);
        public bool IsStatic(float deltaSq) { return Vel.LengthSquared() <= deltaSq; }
        public Vector2 GetAccumulatedForce();
        public void ClearAccumulatedForce();
        public void UpdateState(float dt);
    }

}
