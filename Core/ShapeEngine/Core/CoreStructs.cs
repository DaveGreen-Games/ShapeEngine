using ShapeEngine.Lib;
using System.Numerics;

namespace ShapeEngine.Core
{
    public struct CollisionInformation
    {
        public List<Collision> Collisions;
        public CollisionSurface CollisionSurface;
        public CollisionInformation(List<Collision> collisions, bool computesIntersections)
        {
            this.Collisions = collisions;
            if (!computesIntersections) this.CollisionSurface = new();
            else
            {
                Vector2 avgPoint = new();
                Vector2 avgNormal = new();
                int count = 0;
                foreach (var col in collisions)
                {
                    if (col.Intersection.Valid)
                    {
                        count++;
                        var surface = col.Intersection.CollisionSurface;
                        avgPoint += surface.Point;
                        avgNormal += surface.Normal;
                    }
                }

                if (count > 0)
                {
                    avgPoint /= count;
                    avgNormal = avgNormal.Normalize();
                    this.CollisionSurface = new(avgPoint, avgNormal);
                }
                else
                {
                    this.CollisionSurface = new();
                }
            }
        }

        //public bool ContainsCollidable(TCollidable other)
        //{
        //    foreach (var c in Collisions)
        //    {
        //        if (c.Other == other) return true;
        //    }
        //    return false;
        //}
        public List<Collision> FilterCollisions(Predicate<Collision> match)
        {
            List<Collision> filtered = new();
            foreach (var c in Collisions)
            {
                if (match(c)) filtered.Add(c);
            }
            return filtered;
        }
        public List<ICollidable> FilterObjects(Predicate<ICollidable> match)
        {
            HashSet<ICollidable> filtered = new();
            foreach (var c in Collisions)
            {
                if (match(c.Other)) filtered.Add(c.Other);
            }
            return filtered.ToList();
        }
        public List<ICollidable> GetAllObjects()
        {
            HashSet<ICollidable> others = new();
            foreach (var c in Collisions)
            {
                others.Add(c.Other);

            }
            return others.ToList();
        }
        public List<Collision> GetFirstContactCollisions()
        {
            return FilterCollisions((c) => c.FirstContact);
        }
        public List<ICollidable> GetFirstContactObjects()
        {
            var filtered = GetFirstContactCollisions();
            HashSet<ICollidable> others = new();
            foreach (var c in filtered)
            {
                others.Add(c.Other);
            }
            return others.ToList();
        }
    }
    public struct Collision
    {
        public bool FirstContact;
        public ICollidable Self;
        public ICollidable Other;
        public Vector2 SelfVel;
        public Vector2 OtherVel;
        public Intersection Intersection;

        public Collision(ICollidable self, ICollidable other, bool firstContact)
        {
            this.Self = self;
            this.Other = other;
            this.SelfVel = self.GetCollider().Vel;
            this.OtherVel = other.GetCollider().Vel;
            this.Intersection = new();
            this.FirstContact = firstContact;
        }
        public Collision(ICollidable self, ICollidable other, bool firstContact, CollisionPoints collisionPoints)
        {
            this.Self = self;
            this.Other = other;
            this.SelfVel = self.GetCollider().Vel;
            this.OtherVel = other.GetCollider().Vel;
            this.Intersection = new(collisionPoints, SelfVel, self.GetCollider().Pos);
            this.FirstContact = firstContact;
        }

    }
    public struct Intersection
    {
        public bool Valid;
        public CollisionSurface CollisionSurface;
        public CollisionPoints ColPoints;

        public Intersection() { this.Valid = false; this.CollisionSurface = new(); this.ColPoints = new(); }
        public Intersection(CollisionPoints points, Vector2 vel, Vector2 refPoint)
        {
            if (points.Count <= 0)
            {
                this.Valid = false;
                this.CollisionSurface = new();
                this.ColPoints = new();
            }
            else
            {
                Vector2 avgPoint = new();
                Vector2 avgNormal = new();
                int count = 0;
                foreach (var p in points)
                {
                    if (DiscardNormal(p.Normal, vel)) continue;
                    if (DiscardNormal(p, refPoint)) continue;

                    count++;
                    avgPoint += p.Point;
                    avgNormal += p.Normal;
                }
                if (count > 0)
                {
                    this.Valid = true;
                    this.ColPoints = points;
                    avgPoint /= count;
                    avgNormal = avgNormal.Normalize();
                    this.CollisionSurface = new(avgPoint, avgNormal);
                }
                else
                {
                    this.Valid = false;
                    this.ColPoints = points;
                    this.CollisionSurface = new();
                }
            }
        }
        public Intersection(CollisionPoints points)
        {
            if (points.Count <= 0)
            {
                this.Valid = false;
                this.CollisionSurface = new();
                this.ColPoints = new();
            }
            else
            {
                this.Valid = true;
                this.ColPoints = points;

                Vector2 avgPoint = new();
                Vector2 avgNormal = new();
                foreach (var p in points)
                {
                    avgPoint += p.Point;
                    avgNormal += p.Normal;
                }
                if (points.Count > 0)
                {
                    avgPoint /= points.Count;
                    avgNormal = avgNormal.Normalize();
                    this.CollisionSurface = new(avgPoint, avgNormal);
                }
                else this.CollisionSurface = new();
            }
        }


        private static bool DiscardNormal(Vector2 n, Vector2 vel)
        {
            return n.IsFacingTheSameDirection(vel);
        }
        private static bool DiscardNormal(CollisionPoint p, Vector2 refPoint)
        {
            Vector2 dir = p.Point - refPoint;
            return p.Normal.IsFacingTheSameDirection(dir);
        }

        //public void FlipNormals(Vector2 refPoint)
        //{
        //    if (points.Count <= 0) return;
        //
        //    List<(Vector2 p, Vector2 n)> newPoints = new();
        //    foreach (var p in points)
        //    {
        //        Vector2 dir = refPoint - p.p;
        //        if (dir.IsFacingTheOppositeDirection(p.n)) newPoints.Add((p.p, p.n.Flip()));
        //        else newPoints.Add(p);
        //    }
        //    this.points = newPoints;
        //    this.n = points[0].n;
        //}
        //public Intersection CheckVelocityNew(Vector2 vel)
        //{
        //    List<(Vector2 p, Vector2 n)> newPoints = new();
        //    
        //    for (int i = points.Count - 1; i >= 0; i--)
        //    {
        //        var intersection = points[i];
        //        if (intersection.n.IsFacingTheSameDirection(vel)) continue;
        //        newPoints.Add(intersection);
        //    }
        //    return new(newPoints);
        //}

    }
    public struct CollisionSurface
    {
        public Vector2 Point;
        public Vector2 Normal;
        public bool Valid;

        public CollisionSurface() { Point = new(); Normal = new(); Valid = false; }
        public CollisionSurface(Vector2 point, Vector2 normal)
        {
            this.Point = point;
            this.Normal = normal;
            this.Valid = true;
        }

    }
    public struct CollisionPoint
    {
        public Vector2 Point;
        public Vector2 Normal;

        public CollisionPoint() { Point = new(); Normal = new(); }
        public CollisionPoint(Vector2 p, Vector2 n) { Point = p; Normal = n; }

        public CollisionPoint FlipNormal()
        {
            return new(Point, Normal.Flip());
        }
        public CollisionPoint FlipNormal(Vector2 referencePoint)
        {
            Vector2 dir = referencePoint - Point;
            if (dir.IsFacingTheOppositeDirection(Normal)) return FlipNormal();

            return this;
        }
    }
    
    public class QueryInfos : List<QueryInfo>
    {
        public void SortClosest(Vector2 origin)
        {
            if (Count > 1)
            {
                Sort
                (
                    (a, b) =>
                    {
                        if (!a.points.valid) return 1;
                        else if (!b.points.valid) return -1;
                        
                        float la = (origin - a.points.closest.Point).LengthSquared();
                        float lb = (origin - b.points.closest.Point).LengthSquared();
            
                        if (la > lb) return 1;
                        else if (la == lb) return 0;
                        else return -1;
                    }
                );
            }
        }
    }
    public struct QueryInfo
    {
        public Vector2 origin;
        public ICollidable collidable;
        public QueryPoints points;

        public QueryInfo(ICollidable collidable, Vector2 origin)
        {
            this.collidable = collidable;
            this.origin = origin;
            this.points = new();
        }
        public QueryInfo(ICollidable collidable, Vector2 origin, CollisionPoints points)
        {
            this.collidable = collidable;
            this.origin = origin;
            this.points = new(points, origin);
        }
    }
    public struct QueryPoints
    {
        public bool valid;
        public CollisionPoints points;
        public CollisionPoint closest;

        public QueryPoints()
        {
            this.valid = false;
            this.points = new();
            this.closest = new();
        }
        public QueryPoints(CollisionPoints points, Vector2 origin)
        {
            if(points.Count <= 0)
            {
                this.valid = false;
                this.points = new();
                this.closest = new();
            }
            else
            {
                this.valid = true;
                points.SortClosest(origin);
                this.points = points;
                this.closest = points[0];
            }
        }
    }
    
    
    public class CollisionPoints : List<CollisionPoint>
    {
        public bool Valid { get { return Count > 0; } }
        public void FlipNormals(Vector2 referencePoint)
        {
            for (int i = 0; i < Count; i++)
            {
                var p = this[i];
                Vector2 dir = referencePoint - p.Point;
                if (dir.IsFacingTheOppositeDirection(p.Normal))
                    this[i] = this[i].FlipNormal();
            }
        }
        
        
        public CollisionPoints Copy()
        {
            CollisionPoints copy = new();
            foreach (var item in this)
            {
                copy.Add(item);
            }
            return copy;
        }
        
        public void SortClosest(Vector2 refPoint)
        {
            this.Sort
                (
                    (a, b) =>
                    {
                        float la = (refPoint - a.Point).LengthSquared();
                        float lb = (refPoint - b.Point).LengthSquared();

                        if (la > lb) return 1;
                        else if (la == lb) return 0;
                        else return -1;
                    }
                );
        }

    }



}

    //public struct QueryInfo
    //{
    //    public ICollidable collidable;
    //    public Intersection intersection;
    //    public QueryInfo(ICollidable collidable)
    //    {
    //        this.collidable = collidable;
    //        this.intersection = new();
    //    }
    //    public QueryInfo(ICollidable collidable, CollisionPoints points)
    //    {
    //        this.collidable = collidable;
    //        this.intersection = new(points);
    //    }
    //
    //}