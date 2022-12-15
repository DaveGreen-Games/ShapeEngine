using ShapeLib;
using System.Numerics;
using Raylib_CsLo;

namespace ShapeCollision
{
    public class CollisionHandler
    {
        protected List<ICollidable> collidables = new();
        protected List<ICollidable> tempHolding = new();
        protected List<ICollidable> tempRemoving = new();
        protected SpatialHash spatialHash;

        protected List<OverlapInfo> overlapInfos = new();
        public CollisionHandler(float x, float y, float w, float h, int rows, int cols)
        {
            spatialHash = new(x, y, w, h, rows, cols);
        }
        public void UpdateArea(Rectangle newArea)
        {
            int rows = spatialHash.GetRows();
            int cols = spatialHash.GetCols();
            spatialHash.Close();
            spatialHash = new(newArea.x, newArea.y, newArea.width, newArea.height, rows, cols);
        }
        public SpatialHash GetSpatialHash() { return spatialHash; }
        public void Add(ICollidable collider)
        {
            if (collidables.Contains(collider)) return;
            tempHolding.Add(collider);
        }
        public void AddRange(List<ICollidable> colliders)
        {
            foreach (ICollidable collider in colliders)
            {
                Add(collider);
            }
        }
        public void Remove(ICollidable collider)
        {
            tempRemoving.Add(collider);
        }
        public void RemoveRange(List<ICollidable> colliders)
        {
            tempRemoving.AddRange(colliders);
        }
        public void Clear()
        {
            collidables.Clear();
            tempHolding.Clear();
            tempRemoving.Clear();
            overlapInfos.Clear();
        }
        public void Close()
        {
            Clear();
            spatialHash.Close();
        }
        public virtual void Update(float dt)
        {
            spatialHash.Clear();

            for (int i = collidables.Count - 1; i >= 0; i--)
            {
                var collider = collidables[i];
                if (collider.GetCollider().IsEnabled())
                {
                    spatialHash.Add(collider);
                }
            }

            for (int i = 0; i < collidables.Count; i++)
            {
                ICollidable collider = collidables[i];
                if (!collider.GetCollider().IsEnabled()) continue;
                string[] collisionMask = collider.GetCollisionMask();


                List<ICollidable> others = spatialHash.GetObjects(collider);
                foreach (ICollidable other in others)
                {
                    string otherLayer = other.GetCollisionLayer();
                    if (collisionMask.Length > 0)
                    {
                        if (!collisionMask.Contains(otherLayer)) continue;
                    }//else collide with everything

                    bool overlap = SGeometry.Overlap(collider, other);
                    if (overlap)
                    {
                        overlapInfos.Add(new(true, other, collider));
                    }
                }
            }
        }
        public virtual void Resolve()
        {
            //collidables.AddRange(tempHolding);
            foreach (var collider in tempHolding)
            {
                collidables.Add(collider);
            }
            tempHolding.Clear();

            foreach (var collider in tempRemoving)
            {
                collidables.Remove(collider);
            }
            tempRemoving.Clear();


            foreach (OverlapInfo info in overlapInfos)
            {
                if (info.other == null || info.self == null) continue;
                //info.other.Overlap(info);
                info.self.Overlap(info);
            }
            overlapInfos.Clear();
            
        }


        public List<ICollidable> CastSpace(ICollidable caster)
        {
            List<ICollidable> bodies = new();
            List<ICollidable> objects = spatialHash.GetObjects(caster);
            foreach (ICollidable obj in objects)
            {
                if (caster.GetCollisionMask().Length <= 0)
                {
                    if (SGeometry.Overlap(caster.GetCollider(), obj.GetCollider()))
                    {
                        bodies.Add(obj);
                    }
                }
                else
                {
                    if (SGeometry.Overlap(caster.GetCollider(), obj.GetCollider()))
                    {
                        if (caster.GetCollisionMask().Contains(obj.GetCollisionLayer()))
                        {
                            bodies.Add(obj);
                        }
                    }
                }
            }
            return bodies;
        }
        public List<ICollidable> CastSpace(Collider collider, string[] collisionMask)
        {
            List<ICollidable> bodies = new();
            List<ICollidable> objects = spatialHash.GetObjects(collider);
            foreach (ICollidable obj in objects)
            {
                if (collisionMask.Length <= 0)
                {
                    if (SGeometry.Overlap(collider, obj.GetCollider()))
                    {
                        bodies.Add(obj);
                    }
                }
                else
                {
                    if (SGeometry.Overlap(collider, obj.GetCollider()))
                    {
                        if (collisionMask.Contains(obj.GetCollisionLayer()))
                        {
                            bodies.Add(obj);
                        }
                    }
                }
            }
            return bodies;
        }
        public List<ICollidable> CastSpace(Rectangle rect, string[] collisionMask)
        {
            List<ICollidable> bodies = new();
            RectCollider collider = new(rect);
            List<ICollidable> objects = spatialHash.GetObjects(collider);
            foreach (ICollidable obj in objects)
            {
                if (collisionMask.Length <= 0)
                {
                    if (SGeometry.Overlap(collider, obj.GetCollider()))
                    {
                        bodies.Add(obj);
                    }
                }
                else
                {
                    if (SGeometry.Overlap(collider, obj.GetCollider()))
                    {
                        if (collisionMask.Contains(obj.GetCollisionLayer()))
                        {
                            bodies.Add(obj);
                        }
                    }
                }

            }
            return bodies;
        }
        public List<ICollidable> CastSpace(Vector2 pos, float r, string[] collisionMask)
        {
            List<ICollidable> bodies = new();
            CircleCollider collider = new(pos, r);
            List<ICollidable> objects = spatialHash.GetObjects(collider);
            foreach (ICollidable obj in objects)
            {
                if (collisionMask.Length <= 0)
                {
                    if (SGeometry.Overlap(collider, obj.GetCollider()))
                    {
                        bodies.Add(obj);
                    }
                }
                else
                {
                    if (SGeometry.Overlap(collider, obj.GetCollider()))
                    {
                        if (collisionMask.Contains(obj.GetCollisionLayer()))
                        {
                            bodies.Add(obj);
                        }
                    }
                }

            }
            return bodies;
        }
        public List<ICollidable> CastSpace(Vector2 pos, Vector2 size, Vector2 alignement, string[] collisionMask)
        {
            List<ICollidable> bodies = new();
            RectCollider collider = new(pos, size, alignement);
            List<ICollidable> objects = spatialHash.GetObjects(collider);
            foreach (ICollidable obj in objects)
            {
                if (collisionMask.Length <= 0)
                {
                    if (SGeometry.Overlap(collider, obj.GetCollider()))
                    {
                        bodies.Add(obj);
                    }
                }
                else
                {
                    if (SGeometry.Overlap(collider, obj.GetCollider()))
                    {
                        if (collisionMask.Contains(obj.GetCollisionLayer()))
                        {
                            bodies.Add(obj);
                        }
                    }
                }

            }
            return bodies;
        }
        public List<ICollidable> CastSpace(Vector2 pos, Vector2 dir, float length, string[] collisionMask)
        {
            List<ICollidable> bodies = new();
            SegmentCollider collider = new(pos, dir, length);
            List<ICollidable> objects = spatialHash.GetObjects(collider);
            foreach (ICollidable obj in objects)
            {
                if (collisionMask.Length <= 0)
                {
                    if (SGeometry.Overlap(collider, obj.GetCollider()))
                    {
                        bodies.Add(obj);
                    }
                }
                else
                {
                    if (SGeometry.Overlap(collider, obj.GetCollider()))
                    {
                        if (collisionMask.Contains(obj.GetCollisionLayer()))
                        {
                            bodies.Add(obj);
                        }
                    }
                }

            }
            return bodies;
        }
        public List<ICollidable> CastSpace(Vector2 start, Vector2 end, string[] collisionMask)
        {
            List<ICollidable> bodies = new();
            SegmentCollider collider = new(start, end);
            List<ICollidable> objects = spatialHash.GetObjects(collider);
            foreach (ICollidable obj in objects)
            {
                if (collisionMask.Length <= 0)
                {
                    if (SGeometry.Overlap(collider, obj.GetCollider()))
                    {
                        bodies.Add(obj);
                    }
                }
                else
                {
                    if (SGeometry.Overlap(collider, obj.GetCollider()))
                    {
                        if (collisionMask.Contains(obj.GetCollisionLayer()))
                        {
                            bodies.Add(obj);
                        }
                    }
                }

            }
            return bodies;
        }


        public void DebugDrawGrid(Color border, Color fill)
        {
            spatialHash.DebugDrawGrid(border, fill);
        }
    }


}
