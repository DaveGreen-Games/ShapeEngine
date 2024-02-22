using System.Numerics;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

//Stopwatch functionality
//using System.Diagnostics;
//public readonly Stopwatch watch = new();
//watch.Restart();
//watch.Stop();
//var ms = watch.Elapsed.TotalMilliseconds;
//Console.WriteLine(ms);

namespace ShapeEngine.Core.Collision
{
    /*
    public class ScreenTextures : Dictionary<uint, ScreenTexture>
    {
        public ActiveScreenTextures GetActive(ScreenTextureMask screenTextureMask)
        {
            if (screenTextureMask.Count <= 0)
                return new(this.Values.Where((st) => st.Active));
            else
                return new(this.Values.Where((st) => st.Active && screenTextureMask.Contains(st.ID)));
        }
        public ActiveScreenTextures GetActive()
        {
            return new(this.Values.Where((st) => st.Active));
        }
        public List<ScreenTexture> GetAll() { return this.Values.ToList(); }

    }
    public class ActiveScreenTextures : List<ScreenTexture>
    {
        public ActiveScreenTextures(IEnumerable<ScreenTexture> textures)
        {
            this.AddRange(textures);
        }
        public ActiveScreenTextures(params ScreenTexture[] textures)
        {
            this.AddRange(textures);
        }
        public ActiveScreenTextures SortDrawOrder()
        {
            this.Sort(delegate (ScreenTexture x, ScreenTexture y)
            {
                //if (x == null || y == null) return 0;

                if (x.DrawOrder < y.DrawOrder) return -1;
                else if (x.DrawOrder > y.DrawOrder) return 1;
                else return 0;
            });
            return this;
        }
    }
    public class ScreenTextureMask : HashSet<uint>
    {
        public ScreenTextureMask() { }
        public ScreenTextureMask(params uint[] mask)
        {
            foreach (var id in mask)
            {
                this.Add(id);
            }
        }
        public ScreenTextureMask(IEnumerable<uint> mask)
        {
            foreach (var id in mask)
            {
                this.Add(id);
            }
        }
        public ScreenTextureMask(HashSet<uint> mask)
        {
            foreach (var id in mask)
            {
                this.Add(id);
            }
        }
    }
    */


    public class CollisionPoints : ShapeList<CollisionPoint>
    {
        public CollisionPoints()
        {
            
        }
        public CollisionPoints(params CollisionPoint[] points) { AddRange(points); }
        public CollisionPoints(IEnumerable<CollisionPoint> points) { AddRange(points); }


        public bool Valid => Count > 0;

        public void FlipNormals(Vector2 referencePoint)
        {
            for (var i = 0; i < Count; i++)
            {
                var p = this[i];
                var dir = referencePoint - p.Point;
                if (dir.IsFacingTheOppositeDirection(p.Normal))
                    this[i] = this[i].FlipNormal();
            }
        }
        
        
        public ClosestPoint GetClosestPoint(Vector2 p)
        {
            if (Count <= 0) return new();

            float minDisSquared = float.PositiveInfinity;
            CollisionPoint closestPoint = new();

            for (var i = 0; i < Count; i++)
            {
                var point = this[i];

                float disSquared = (point.Point - p).LengthSquared();
                if (disSquared > minDisSquared) continue;
                minDisSquared = disSquared;
                closestPoint = point;
            }
            return new(closestPoint, minDisSquared);
        }

        public override int GetHashCode() { return Game.GetHashCode(this); }
        public bool Equals(CollisionPoints? other)
        {
            if (other == null) return false;
            if (Count != other.Count) return false;
            for (var i = 0; i < Count; i++)
            {
                if (this[i].Equals(other[i])) return false;
            }
            return true;
        }


        public Points GetUniquePoints()
        {
            var uniqueVertices = new HashSet<Vector2>();
            for (var i = 0; i < Count; i++)
            {
                uniqueVertices.Add(this[i].Point);
            }
            return new(uniqueVertices);
        }
        public CollisionPoints GetUniqueCollisionPoints()
        {
            var unique = new HashSet<CollisionPoint>();
            for (var i = 0; i < Count; i++)
            {
                unique.Add(this[i]);
            }
            return new(unique);
        }

        public void SortClosest(Vector2 refPoint)
        {
            this.Sort
                (
                    comparison: (a, b) =>
                    {
                        float la = (refPoint - a.Point).LengthSquared();
                        float lb = (refPoint - b.Point).LengthSquared();

                        if (la > lb) return 1;
                        else if (MathF.Abs(x: la - lb) < 0.01f) return 0;
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