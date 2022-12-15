
using System.Numerics;
using Raylib_CsLo;

namespace ShapeLib.ShapeCollision
{
    public class CollisionHandler
    {
        protected bool advanced = false;
        protected List<ICollidable> collidables = new();
        protected List<ICollidable> tempHolding = new();
        protected List<ICollidable> tempRemoving = new();
        protected SpatialHash spatialHash;

        protected List<OverlapInfo> overlapInfos = new();
        protected List<CastInfo> castInfos = new();

        //advanced section
        protected List<ICollidable> disabled = new();
        protected List<ICollidable> staticCollidables = new();


        //advanced automatically removes disabled shapes and readds them when they are enabled again
        public CollisionHandler(float x, float y, float w, float h, int rows, int cols, bool advanced = false)
        {
            spatialHash = new(x, y, w, h, rows, cols);
            this.advanced = advanced;
        }
        public void UpdateArea(Rectangle newArea)
        {
            int rows = spatialHash.GetRows();
            int cols = spatialHash.GetCols();
            spatialHash.Close();
            spatialHash = new(newArea.x, newArea.y, newArea.width, newArea.height, rows, cols);
            foreach (var c in staticCollidables)
            {
                spatialHash.Add(c, false);
            }
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
            castInfos.Clear();
        }
        public void Close()
        {
            Clear();
            spatialHash.Close();
        }
        public virtual void Update(float dt)
        {
            spatialHash.ClearDynamic();

            if (advanced)
            {
                for (int i = staticCollidables.Count - 1; i >= 0; i--)
                {
                    var collider = staticCollidables[i];
                    if (!collider.GetCollider().IsEnabled())
                    {
                        disabled.Add(collider);
                        spatialHash.Remove(collider, false);
                        staticCollidables.RemoveAt(i);
                    }
                }
            }

            for (int i = collidables.Count - 1; i >= 0; i--)
            {
                var collider = collidables[i];
                if (!collider.GetCollider().IsEnabled())
                {
                    if (advanced)
                    {
                        disabled.Add(collider);
                        collidables.RemoveAt(i);
                    }
                    continue;
                }
                if (collider.GetColliderType() == ColliderType.DYNAMIC)
                {
                    spatialHash.Add(collider, true);
                }
            }

            for (int i = 0; i < collidables.Count; i++)
            {
                ICollidable collider = collidables[i];
                if (!collider.GetCollider().IsEnabled()) continue;
                string[] collisionMask = collider.GetCollisionMask();

                //if (collider.GetColliderType() == ColliderType.STATIC || collider.GetColliderClass() == ColliderClass.AREA) continue;

                List<ICollidable> others = spatialHash.GetObjects(collider);
                foreach (ICollidable other in others)
                {
                    string otherLayer = other.GetCollisionLayer();
                    if (collisionMask.Length > 0)
                    {
                        //if (otherLayer == "") continue; //do i need that?
                        if (!collisionMask.Contains(otherLayer)) continue;
                    }//else collide with everything

                    if (collider.GetColliderClass() == ColliderClass.COLLIDER)
                    {
                        if (other.GetColliderClass() == ColliderClass.COLLIDER)
                        {
                            CastInfo info = SGeometry.CheckCast(collider, other, dt);
                            if (info.collided || info.overlapping)
                            {
                                castInfos.Add(info);
                            }
                        }
                        //else//area is being checked by collider
                        //{
                        //    bool overlap = Overlap.Check(collider, other);
                        //    if (overlap)
                        //    {
                        //
                        //        overlapInfos.Add(new(true, collider, other));
                        //    }
                        //}
                    }
                    else//area checks other things
                    {
                        bool overlap = Overlap.Check(collider, other);
                        if (overlap)
                        {
                            overlapInfos.Add(new(true, other, collider, other.GetCollider().Vel, collider.GetCollider().Vel));
                        }
                    }

                }

            }
        }
        public virtual void Resolve()
        {
            //collidables.AddRange(tempHolding);
            foreach (var collider in tempHolding)
            {
                if (collider.GetColliderType() == ColliderType.STATIC)
                {
                    if (collider.GetColliderClass() == ColliderClass.AREA)
                    {
                        if (advanced)
                        {
                            if (!collider.GetCollider().IsEnabled())
                            {
                                disabled.Add(collider);
                            }
                            else
                            {
                                collidables.Add(collider);
                            }
                        }
                        else
                        {
                            collidables.Add(collider);
                        }
                    }
                    else
                    {
                        if (advanced)
                        {
                            if (!collider.GetCollider().IsEnabled())
                            {
                                disabled.Add(collider);
                            }
                            else
                            {
                                spatialHash.Add(collider, false);
                                staticCollidables.Add(collider);
                            }
                        }
                        else
                        {
                            spatialHash.Add(collider, false);
                        }
                    }
                }
                else
                {
                    if (advanced)
                    {
                        if (!collider.GetCollider().IsEnabled())
                        {
                            disabled.Add(collider);
                        }
                        else
                        {
                            collidables.Add(collider);
                        }
                    }
                    else
                    {
                        collidables.Add(collider);
                    }
                }
            }
            tempHolding.Clear();

            foreach (var collider in tempRemoving)
            {
                if (collider.GetColliderType() == ColliderType.STATIC)
                {
                    if (advanced)
                    {
                        if (!collider.GetCollider().IsEnabled())
                        {
                            disabled.Remove(collider);
                        }
                        else
                        {
                            spatialHash.Remove(collider, false);
                            staticCollidables.Remove(collider);
                        }
                    }
                    else
                    {
                        spatialHash.Remove(collider, false);
                    }
                }
                else
                {
                    if (advanced)
                    {
                        if (!collider.GetCollider().IsEnabled())
                        {
                            disabled.Remove(collider);
                        }
                        else
                        {
                            collidables.Remove(collider);
                        }
                    }
                    else
                    {
                        collidables.Remove(collider);
                    }
                }
            }
            tempRemoving.Clear();

            foreach (CastInfo info in castInfos)
            {
                if (info.self == null) continue;
                info.self.Collide(info);
            }
            castInfos.Clear();

            foreach (OverlapInfo info in overlapInfos)
            {
                if (info.other == null || info.self == null) continue;
                info.other.Overlap(info);
                info.self.Overlap(info);
            }
            overlapInfos.Clear();
            if (advanced)
            {
                for (int i = disabled.Count - 1; i >= 0; i--)
                {
                    var collider = disabled[i];
                    if (collider.GetCollider().IsEnabled())
                    {
                        if (collider.GetColliderType() == ColliderType.STATIC)
                        {
                            spatialHash.Add(collider, false);
                            staticCollidables.Add(collider);
                        }
                        else
                        {
                            collidables.Add(collider);
                        }
                        disabled.RemoveAt(i);
                    }
                }
            }
        }


        public List<ICollidable> CastSpace(ICollidable caster)
        {
            List<ICollidable> bodies = new();
            List<ICollidable> objects = spatialHash.GetObjects(caster);
            foreach (ICollidable obj in objects)
            {
                if (caster.GetCollisionMask().Length <= 0)
                {
                    if (Overlap.Check(caster.GetCollider(), obj.GetCollider()))
                    {
                        bodies.Add(obj);
                    }
                }
                else
                {
                    if (Overlap.Check(caster.GetCollider(), obj.GetCollider()))
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
                    if (Overlap.Check(collider, obj.GetCollider()))
                    {
                        bodies.Add(obj);
                    }
                }
                else
                {
                    if (Overlap.Check(collider, obj.GetCollider()))
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
            RectCollider collider = new(new(rect.x, rect.y), new(rect.width, rect.height));
            List<ICollidable> objects = spatialHash.GetObjects(collider);
            foreach (ICollidable obj in objects)
            {
                if (collisionMask.Length <= 0)
                {
                    if (Overlap.Check(collider, obj.GetCollider()))
                    {
                        bodies.Add(obj);
                    }
                }
                else
                {
                    if (Overlap.Check(collider, obj.GetCollider()))
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
                    if (Overlap.Check(collider, obj.GetCollider()))
                    {
                        bodies.Add(obj);
                    }
                }
                else
                {
                    if (Overlap.Check(collider, obj.GetCollider()))
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
        public List<ICollidable> CastSpace(Vector2 pos, Vector2 size, string[] collisionMask)
        {
            List<ICollidable> bodies = new();
            RectCollider collider = new(pos, size);
            List<ICollidable> objects = spatialHash.GetObjects(collider);
            foreach (ICollidable obj in objects)
            {
                if (collisionMask.Length <= 0)
                {
                    if (Overlap.Check(collider, obj.GetCollider()))
                    {
                        bodies.Add(obj);
                    }
                }
                else
                {
                    if (Overlap.Check(collider, obj.GetCollider()))
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
                    if (Overlap.Check(collider, obj.GetCollider()))
                    {
                        bodies.Add(obj);
                    }
                }
                else
                {
                    if (Overlap.Check(collider, obj.GetCollider()))
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


        public CastInfo RayCastSpace(Vector2 pos, Vector2 dir, float length, string[] collisionMask)
        {
            throw new NotImplementedException();
        }
        public CastInfo RayCastSpace(SegmentCollider ray, string[] collisionMask)
        {
            throw new NotImplementedException();
        }

        public void DebugDrawGrid(Color border, Color fill)
        {
            spatialHash.DebugDrawGrid(border, fill);
        }
    }


}
