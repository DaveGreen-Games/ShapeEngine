using System.Drawing;
using System.Numerics;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Core.CollisionSystem;

public class IntersectSpaceResult : List<IntersectSpaceRegister>
{
    #region Members
    
    public readonly Vector2 Origin;
    public IntersectSpaceRegister? First => Count <= 0 ? null : this[0];
    public IntersectSpaceRegister? Last => Count <= 0 ? null : this[Count - 1];
    
    #endregion
    
    #region Constructors
    public IntersectSpaceResult(Vector2 origin, int capacity) : base(capacity)
    {
        Origin = origin;
    }

    #endregion
    
    #region Public Functions
    
    public bool AddRegister(IntersectSpaceRegister reg)
    {
        if (reg.Count <= 0) return false;
        Add(reg);
        return true;
    }
    
    #endregion
    
    #region Sorting
    public bool SortClosestFirst() => SortClosestFirst(Origin);
    public bool SortFurthestFirst() => SortFurthestFirst(Origin);

    public bool SortClosestFirst(Vector2 referencePoint)
    {
        if(Count <= 0) return false;
        if (Count == 1)
        {
            this[0].SortClosestFirst(referencePoint);
            return true;
        }
        foreach (var reg in this)
        {
            reg.SortClosestFirst(referencePoint);
        }  
        this.Sort
        (
            comparison: (a, b) =>
            {
                if (a.Count <= 0) return 1;
                if (b.Count <= 0) return -1;
                
                var aEntry = a.First;
                if(aEntry == null) return 1;
                
                var bEntry = b.First;
                if(bEntry == null) return -1;
                
                float la = (referencePoint - aEntry.First.Point).LengthSquared();
                float lb = (referencePoint - bEntry.First.Point).LengthSquared();

                if (la > lb) return 1;
                if (MathF.Abs(x: la - lb) < 0.01f) return 0;
                return -1;
            }
        );
        return true;
    }
    public bool SortFurthestFirst(Vector2 referencePoint)
    {
        if(Count <= 0) return false;
        if (Count == 1)
        {
            this[0].SortFurthestFirst(referencePoint);
            return true;
        }
        foreach (var reg in this)
        {
            reg.SortFurthestFirst(referencePoint);
        }
        this.Sort
        (
            comparison: (a, b) =>
            {
                if (a.Count <= 0) return -1;
                if (b.Count <= 0) return 1;
                
                var aEntry = a.First;
                if(aEntry == null) return -1;
                
                var bEntry = b.First;
                if(bEntry == null) return 1;
                
                float la = (referencePoint - aEntry.First.Point).LengthSquared();
                float lb = (referencePoint - bEntry.First.Point).LengthSquared();

                if (la > lb) return 1;
                if (MathF.Abs(x: la - lb) < 0.01f) return 0;
                return -1;
            }
        );
        return true;
    }
    #endregion
    
    #region Validation
    //TODO: validate functions for Result, Register, and Entry
    
    #endregion
    
    #region Closest/Furthest Entry
    
    public IntersectSpaceEntry? GetClosestEntry(out float closestDistanceSquared)
    {
        closestDistanceSquared = -1;
        if (Count <= 0) return null;
        return GetClosestEntry(Origin, out closestDistanceSquared);
    }

    public IntersectSpaceEntry? GetClosestEntry(Vector2 referencePoint, out float closestDistanceSquared)
    {
        closestDistanceSquared = -1f;
        if (Count <= 0) return null;
        if (Count == 1)
        {
            var reg = this[0];
            return reg.GetClosestEntry(referencePoint, out closestDistanceSquared);
        }
        IntersectSpaceEntry? closestEntry = null;
        foreach (var reg in this)
        {
            var entry = reg.GetClosestEntry(referencePoint, out var distanceSquared);
            if (closestEntry == null)
            {
                closestEntry = entry;
                closestDistanceSquared = distanceSquared;
            }
            else if (distanceSquared < closestDistanceSquared)
            {
                closestEntry = entry;
                closestDistanceSquared = distanceSquared;
            }
            
        }
        return closestEntry;
    }
    public IntersectSpaceEntry? GetFurthestEntry(out float furthestDistanceSquared)
    {
        furthestDistanceSquared = -1f;
        if (Count <= 0) return null;
        return GetFurthestEntry(Origin, out furthestDistanceSquared);
    }
    public IntersectSpaceEntry? GetFurthestEntry(Vector2 referencePoint, out float furthestDistanceSquared)
    {
        furthestDistanceSquared = -1f;
        if(Count <= 0) return null;
        if (Count == 1)
        {
            var reg = this[0];
            return reg.GetFurthestEntry(referencePoint, out furthestDistanceSquared);
        }
        IntersectSpaceEntry? furthestEntry = null;
        foreach (var reg in this)
        {
            var entry = reg.GetFurthestEntry(referencePoint, out var distanceSquared);
            if (furthestEntry == null)
            {
                furthestEntry = entry;
                furthestDistanceSquared = distanceSquared;
            }
            else if (distanceSquared > furthestDistanceSquared)
            {
                furthestEntry = entry;
                furthestDistanceSquared = distanceSquared;
            }
            
        }
        return furthestEntry;
    }

    #endregion
    
    #region Closest/Furthest Collider
    
    public IntersectSpaceEntry? GetClosestEntryCollider(out float closestDistanceSquared)
    {
        closestDistanceSquared = -1;
        if (Count <= 0) return null;
        return GetClosestEntryCollider(Origin, out closestDistanceSquared);
    }
    public IntersectSpaceEntry? GetClosestEntryCollider(Vector2 referencePoint, out float closestDistanceSquared)
    {
        closestDistanceSquared = -1f;
        if(Count <= 0) return null;
        if (Count == 1)
        {
            var reg = this[0];
            return reg.GetClosestEntryCollider(referencePoint, out closestDistanceSquared);
        }
        IntersectSpaceEntry? closestEntry = null;
        foreach (var reg in this)
        {
            var entry = reg.GetClosestEntryCollider(referencePoint, out var distanceSquared);
            if (closestEntry == null)
            {
                closestEntry = entry;
                closestDistanceSquared = distanceSquared;
            }
            else if (distanceSquared < closestDistanceSquared)
            {
                closestEntry = entry;
                closestDistanceSquared = distanceSquared;
            }
            
        }
        return closestEntry;
    }
    public IntersectSpaceEntry? GetFurthestEntryCollider(out float furthestDistanceSquared)
    {
        furthestDistanceSquared = -1f;
        if (Count <= 0) return null;
        return GetFurthestEntryCollider(Origin, out furthestDistanceSquared);
    }
    public IntersectSpaceEntry? GetFurthestEntryCollider(Vector2 referencePoint, out float furthestDistanceSquared)
    {
        furthestDistanceSquared = -1f;
        if(Count <= 0) return null;
        if (Count == 1)
        {
            var reg = this[0];
            return reg.GetFurthestEntryCollider(referencePoint, out furthestDistanceSquared);
        }
        IntersectSpaceEntry? furthestEntry = null;
        foreach (var reg in this)
        {
            var entry = reg.GetFurthestEntryCollider(referencePoint, out var distanceSquared);
            if (furthestEntry == null)
            {
                furthestEntry = entry;
                furthestDistanceSquared = distanceSquared;
            }
            else if (distanceSquared > furthestDistanceSquared)
            {
                furthestEntry = entry;
                furthestDistanceSquared = distanceSquared;
            }
            
        }
        return furthestEntry;
    }

    #endregion
    
    #region Closest/Furthest Register
    
    public IntersectSpaceRegister? GetClosestRegister(out float closestDistanceSquared)
    {
        closestDistanceSquared = -1;
        if(Count <= 0) return null;
        return GetClosestRegister(Origin, out closestDistanceSquared);
    }
    public IntersectSpaceRegister? GetClosestRegister(Vector2 referencePoint, out float closestDistanceSquared)
    {
        closestDistanceSquared = -1;
        if(Count <= 0) return null;
        if (Count == 1)
        {
            var reg = this[0];
            closestDistanceSquared = (referencePoint - reg.OtherCollisionObject.Transform.Position).LengthSquared();
            return reg;
        }
        
        var closestReg = this[0];
        closestDistanceSquared = (referencePoint - closestReg.OtherCollisionObject.Transform.Position).LengthSquared();
        for (int i = 1; i < Count; i++)
        {
            var reg = this[i];
            var distanceSquared = (referencePoint - reg.OtherCollisionObject.Transform.Position).LengthSquared();
            if (distanceSquared < closestDistanceSquared)
            {
                closestDistanceSquared = distanceSquared;
                closestReg = reg;
            }
        }
        return closestReg;
    }
    public IntersectSpaceRegister? GetFurthestRegister(out float furthestDistanceSquared)
    {
        furthestDistanceSquared = -1;
        if(Count <= 0) return null;
        return GetFurthestRegister(Origin, out furthestDistanceSquared);
    }
    public IntersectSpaceRegister? GetFurthestRegister(Vector2 referencePoint, out float furthestDistanceSquared)
    {
        furthestDistanceSquared = -1;
        if(Count <= 0) return null;
        if (Count == 1)
        {
            var reg = this[0];
            furthestDistanceSquared = (referencePoint - reg.OtherCollisionObject.Transform.Position).LengthSquared();
            return this[0];
        }
        
        var furthestReg = this[0];
        furthestDistanceSquared = (referencePoint - furthestReg.OtherCollisionObject.Transform.Position).LengthSquared();
        for (int i = 1; i < Count; i++)
        {
            var reg = this[i];
            var distanceSquared = (referencePoint - reg.OtherCollisionObject.Transform.Position).LengthSquared();
            if (distanceSquared > furthestDistanceSquared)
            {
                furthestDistanceSquared = distanceSquared;
                furthestReg = reg;
            }
        }
        return furthestReg;
    }

    #endregion
    
    #region Closest/Furthest Collision Points
    
    public CollisionPoint GetClosestCollisionPoint(out float closestDistanceSquared)
    {
        closestDistanceSquared = -1;
        if (Count <= 0) return new();
        return GetClosestCollisionPoint(Origin, out closestDistanceSquared);
    }
    public CollisionPoint GetClosestCollisionPoint(Vector2 position, out float closestDistanceSquared)
    {
        closestDistanceSquared = -1;
        if (Count <= 0) return new();
        if (Count == 1)
        {
            var point = this[0].GetClosestCollisionPoint(position, out closestDistanceSquared);
            return point;
        }
        var closestPoint = this[0].GetClosestCollisionPoint(position, out closestDistanceSquared);
        for (int i = 1; i < Count; i++)
        {
            var point = this[i].GetClosestCollisionPoint(position, out var distanceSquared);
            if (distanceSquared < closestDistanceSquared)
            {
                closestDistanceSquared = distanceSquared;
                closestPoint = point;
            }
        }

        return closestPoint;
    }
    public CollisionPoint GetFurthestCollisionPoint(out float furthestDistanceSquared)
    {
        furthestDistanceSquared = -1;
        if (Count <= 0) return new();
        return GetFurthestCollisionPoint(Origin, out furthestDistanceSquared);
    }
    public CollisionPoint GetFurthestCollisionPoint(Vector2 position, out float furthestDistanceSquared)
    {
        furthestDistanceSquared = -1;
        if (Count <= 0) return new();
        if (Count == 1)
        {
            var point = this[0].GetFurthestCollisionPoint(position, out furthestDistanceSquared);
            return point;
        }
        var furthestPoint = this[0].GetClosestCollisionPoint(position, out furthestDistanceSquared);
        for (int i = 1; i < Count; i++)
        {
            var point = this[i].GetFurthestCollisionPoint(position, out var distanceSquared);
            if (distanceSquared < furthestDistanceSquared)
            {
                furthestDistanceSquared = distanceSquared;
                furthestPoint = point;
            }
        }

        return furthestPoint;
    }
   
    #endregion
    
}
