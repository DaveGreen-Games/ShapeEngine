using System.Numerics;

namespace ShapeEngine.Core.Interfaces
{
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
   