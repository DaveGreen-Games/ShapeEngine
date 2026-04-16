using System.Numerics;
using Clipper2Lib;
using ShapeEngine.Geometry;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.RectDef;

namespace ShapeEngine.ShapeClipper;
/// <summary>
/// Provides Clipper/world-space conversion helpers for 2D paths, polygons, rectangles, points, and related enums.
/// </summary>
/// <remarks>
/// These helpers centralize conversion between ShapeEngine geometry types and Clipper2 types using the
/// precision configured on <see cref="ShapeClipper2D"/>.
/// Most methods write into caller-supplied destination collections and clear or resize them as needed.
/// </remarks>
public static class ShapeClipperConversion2D
{
    #region Conversion Single To Single
    
    /// <summary>
    /// Converts a list of world-space vertices to a Clipper <see cref="Path64"/>.
    /// </summary>
    /// <param name="src">The source vertices.</param>
    /// <param name="dst">The destination Clipper path.</param>
    public static void ToPath64(this IReadOnlyList<Vector2> src, Path64 dst)
    {
        dst.Clear();
        dst.EnsureCapacity(src.Count);

        for (int i = 0; i < src.Count; i++)
        {
            dst.Add(src[i].ToPoint64());
        }
    }
  
    /// <summary>
    /// Converts a Clipper <see cref="Path64"/> to a list of world-space vertices.
    /// </summary>
    /// <param name="src">The source Clipper path.</param>
    /// <param name="dst">The destination vertex list.</param>
    public static void ToVector2List(this Path64 src, List<Vector2> dst)
    {
        dst.Clear();
        dst.EnsureCapacity(src.Count);
        
        for (int i = 0; i < src.Count; i++)
        {
            dst.Add(src[i].ToVec2());
        }
    }
    
    #endregion
    
    #region Conversion Multi to Multi
    
    /// <summary>
    /// Converts nested world-space vertex lists to a <see cref="Paths64"/> collection.
    /// </summary>
    /// <param name="src">The source polygons.</param>
    /// <param name="dst">The destination Clipper path collection.</param>
    public static void ToPaths64(this IReadOnlyList<IReadOnlyList<Vector2>> src, Paths64 dst) 
    {
        for (int i = 0; i < src.Count; i++)
        {
            if(dst.Count <= i)
            {
                var dstItem = new Path64();
                dst.Add(dstItem);
                
                var srcItem = src[i];
                if(srcItem.Count <= 0) continue;
                
                srcItem.ToPath64(dstItem);
            }
            else
            {
                var srcItem = src[i];
                if(srcItem.Count <= 0) continue;
                var dstItem = dst[i];
                srcItem.ToPath64(dstItem);
            }
        }
        if (dst.Count > src.Count)
        {
            for (int i = dst.Count - 1; i >= src.Count; i--)
            {
                dst.RemoveAt(i);
            }
        }
    }
    
    /// <summary>
    /// Converts a <see cref="Paths64"/> collection to nested world-space vertex lists.
    /// </summary>
    /// <param name="src">The source Clipper paths.</param>
    /// <param name="dst">The destination nested vertex list.</param>
    public static void ToVector2Lists(this Paths64 src, List<List<Vector2>> dst)
    {
        for (int i = 0; i < src.Count; i++)
        {
            if(dst.Count <= i)
            {
                var dstItem = new List<Vector2>();
                dst.Add(dstItem);
                
                var srcItem = src[i];
                if(srcItem.Count <= 0) continue;
                
                srcItem.ToVector2List(dstItem);
            }
            else
            {
                var srcItem = src[i];
                if(srcItem.Count <= 0) continue;
                var dstItem = dst[i];
                srcItem.ToVector2List(dstItem);
            }
        }
        if (dst.Count > src.Count)
        {
            for (int i = dst.Count - 1; i >= src.Count; i--)
            {
                dst.RemoveAt(i);
            }
        }
    }
    
    /// <summary>
    /// Converts a <see cref="Paths64"/> collection to a <see cref="Polygons"/> collection.
    /// </summary>
    /// <param name="src">The source Clipper paths.</param>
    /// <param name="dst">The destination polygon collection.</param>
    public static void ToPolygons(this Paths64 src, Polygons dst)
    {
        for (int i = 0; i < src.Count; i++)
        {
            if(dst.Count <= i)
            {
                var dstItem = new Polygon();
                dst.Add(dstItem);
                
                var srcItem = src[i];
                if(srcItem.Count <= 0) continue;
                
                srcItem.ToVector2List(dstItem);
            }
            else
            {
                var srcItem = src[i];
                if(srcItem.Count <= 0) continue;
                var dstItem = dst[i];
                srcItem.ToVector2List(dstItem);
            }
        }
        if (dst.Count > src.Count)
        {
            for (int i = dst.Count - 1; i >= src.Count; i--)
            {
                dst.RemoveAt(i);
            }
        }
    }
    
    /// <summary>
    /// Converts a <see cref="Paths64"/> collection to a <see cref="Polygons"/> collection, optionally skipping hole paths.
    /// </summary>
    /// <param name="src">The source Clipper paths.</param>
    /// <param name="dst">The destination polygon collection.</param>
    /// <param name="removeHoles">Whether hole paths should be ignored during conversion.</param>
    public static void ToPolygons(this Paths64 src, Polygons dst, bool removeHoles)
    {
        for (int i = 0; i < src.Count; i++)
        {
            var srcItem = src[i];
            if(srcItem.Count <= 0) continue;
            if(removeHoles && srcItem.IsHole()) continue;
            if(dst.Count <= i)
            {
                var dstItem = new Polygon();
                dst.Add(dstItem);
                srcItem.ToVector2List(dstItem);
            }
            else
            {
                var dstItem = dst[i];
                srcItem.ToVector2List(dstItem);
            }
        }
        if (dst.Count > src.Count)
        {
            for (int i = dst.Count - 1; i >= src.Count; i--)
            {
                dst.RemoveAt(i);
            }
        }
    }
    
    #endregion
    
    #region Conversion Single to Multi
    /// <summary>
    /// Converts a single world-space polygon to a single-entry <see cref="Paths64"/> collection.
    /// </summary>
    /// <param name="src">The source vertices.</param>
    /// <param name="dst">The destination path collection.</param>
    public static void ToPaths64(this IReadOnlyList<Vector2> src, Paths64 dst)
    {
        if (dst.Count <= 0)
        {
            var dstItem = new Path64();
            src.ToPath64(dstItem);
            dst.Add(dstItem);
        }
        else
        {
            var dstItem = dst[0];
            src.ToPath64(dstItem);
            if (dst.Count > 1)
            {
                dst.Clear();
                dst.Add(dstItem);
            }
        }
    }
  
    /// <summary>
    /// Converts a single Clipper <see cref="Path64"/> to a nested world-space vertex list containing one polygon.
    /// </summary>
    /// <param name="src">The source Clipper path.</param>
    /// <param name="dst">The destination nested vertex list.</param>
    public static void ToVector2Lists(this Path64 src, List<List<Vector2>> dst)
    {
        if (dst.Count <= 0)
        {
            var dstItem = new List<Vector2>();
            src.ToVector2List(dstItem);
            dst.Add(dstItem);
        }
        else
        {
            var dstItem = dst[0];
            src.ToVector2List(dstItem);
            if (dst.Count > 1)
            {
                dst.Clear();
                dst.Add(dstItem);
            }
        }
    }
    
    /// <summary>
    /// Converts a single Clipper <see cref="Path64"/> to a <see cref="Polygons"/> collection containing one polygon.
    /// </summary>
    /// <param name="src">The source Clipper path.</param>
    /// <param name="dst">The destination polygon collection.</param>
    public static void ToPolygons(this Path64 src, Polygons dst)
    {
        if (dst.Count <= 0)
        {
            var dstItem = new Polygon();
            src.ToVector2List(dstItem);
            dst.Add(dstItem);
        }
        else
        {
            var dstItem = dst[0];
            src.ToVector2List(dstItem);
            if (dst.Count > 1)
            {
                dst.Clear();
                dst.Add(dstItem);
            }
        }
    }
    
    #endregion

    #region Conversion Rect
    
    /// <summary>
    /// Converts a world-space <see cref="Rect"/> to a Clipper <see cref="Rect64"/>.
    /// </summary>
    /// <param name="r">The rectangle to convert.</param>
    /// <returns>The converted Clipper rectangle.</returns>
    public static Rect64 ToRect64(this Rect r)
    {
        // long left   = (long)Math.Round(r.X * Scale);
        // long top    = (long)Math.Round(r.Y * Scale);
        // long right  = (long)Math.Round((r.X + r.Width) * Scale);
        // long bottom = (long)Math.Round((r.Y + r.Height) * Scale);
        // return new Rect64(left, top, right, bottom);
        
        long left   = (long)Math.Round(r.X * ShapeClipper2D.Scale);
        long bottom    = (long)Math.Round(-r.Y * ShapeClipper2D.Scale);
        long right  = (long)Math.Round((r.X + r.Width) * ShapeClipper2D.Scale);
        long top = (long)Math.Round((-r.Y - r.Height) * ShapeClipper2D.Scale);
        return new Rect64(left, top, right, bottom);
    }
   
    /// <summary>
    /// Converts a Clipper <see cref="Rect64"/> to a world-space <see cref="Rect"/>.
    /// </summary>
    /// <param name="r">The rectangle to convert.</param>
    /// <returns>The converted world-space rectangle.</returns>
    public static Rect ToRect(this Rect64 r)
    {
        // float x = (float)(r.left * InvScale);
        // float y = (float)(r.top * InvScale);
        // float w = (float)((r.right - r.left) * InvScale);
        // float h = (float)((r.bottom - r.top) * InvScale);
        //
        // return new Rect(x, y, w, h);
        
        float x = (float)(r.left * ShapeClipper2D.InvScale);
        float y = (float)((-r.top - r.Height) * ShapeClipper2D.InvScale);
        float w = (float)(r.Width * ShapeClipper2D.InvScale);
        float h = (float)(r.Height * ShapeClipper2D.InvScale);

        return new Rect(x, y, w, h);
    }
    
    #endregion

    #region Point 64
    
    /// <summary>
    /// Converts a world-space <see cref="Vector2"/> to a Clipper <see cref="Point64"/>.
    /// </summary>
    /// <param name="v">The point to convert.</param>
    /// <returns>The converted Clipper point.</returns>
    public static Point64 ToPoint64(this Vector2 v)
    {
        long x = (long)Math.Round(v.X * ShapeClipper2D.Scale);
        long y = (long)Math.Round(-v.Y * ShapeClipper2D.Scale);
        return new Point64(x,y);
    }
    
    /// <summary>
    /// Converts a Clipper <see cref="Point64"/> to a world-space <see cref="Vector2"/>.
    /// </summary>
    /// <param name="p">The point to convert.</param>
    /// <returns>The converted world-space point.</returns>
    public static Vector2 ToVec2(this Point64 p)
    {
        return new Vector2((float)(p.X * ShapeClipper2D.InvScale), (float)(-p.Y * ShapeClipper2D.InvScale));
    }
    
    #endregion
    
    #region Enum Conversion
    
    /// <summary>
    /// Converts a <see cref="ShapeClipperFillRule"/> to the Clipper <see cref="FillRule"/> enum.
    /// </summary>
    /// <param name="fillRule">The ShapeClipper fill rule to convert.</param>
    /// <returns>The equivalent <see cref="FillRule"/> value.</returns>
    public static FillRule ToClipperFillRule(this ShapeClipperFillRule fillRule)
    {
        return (FillRule)fillRule;
    }
    
    /// <summary>
    /// Converts a <see cref="ShapeClipperJoinType"/> to the Clipper <see cref="JoinType"/> enum.
    /// </summary>
    /// <param name="joinType">The ShapeClipper join type to convert.</param>
    /// <returns>The equivalent <see cref="JoinType"/> value.</returns>
    public static JoinType ToClipperJoinType(this ShapeClipperJoinType joinType)
    {
        return (JoinType)joinType;
    }
    
    /// <summary>
    /// Converts a <see cref="ShapeClipperEndType"/> to the Clipper <see cref="EndType"/> enum.
    /// </summary>
    /// <param name="endType">The ShapeClipper end type to convert.</param>
    /// <returns>The equivalent <see cref="EndType"/> value.</returns>
    public static EndType ToClipperEndType(this ShapeClipperEndType endType)
    {
        return (EndType)endType;
    }

    /// <summary>
    /// Converts a <see cref="ShapeClipperClipType"/> to the Clipper <see cref="ClipType"/> enum.
    /// </summary>
    /// <param name="clipType">The ShapeClipper clip type to convert.</param>
    /// <returns>The equivalent <see cref="ClipType"/> value.</returns>
    public static ClipType ToClipperClipType(this ShapeClipperClipType clipType)
    {
        return (ClipType)clipType;
    }
    
    /// <summary>
    /// Converts a Clipper <see cref="FillRule"/> to the local <see cref="ShapeClipperFillRule"/> enum.
    /// </summary>
    /// <param name="fillRule">The Clipper fill rule to convert.</param>
    /// <returns>The equivalent <see cref="ShapeClipperFillRule"/> value.</returns>
    public static ShapeClipperFillRule ToShapeClipperFillRule(this FillRule fillRule)
    {
        return (ShapeClipperFillRule)fillRule;
    }
    
    /// <summary>
    /// Converts a Clipper <see cref="JoinType"/> to the local <see cref="ShapeClipperJoinType"/> enum.
    /// </summary>
    /// <param name="joinType">The Clipper join type to convert.</param>
    /// <returns>The equivalent <see cref="ShapeClipperJoinType"/> value.</returns>
    public static ShapeClipperJoinType ToShapeClipperJoinType(this JoinType joinType)
    {
        return (ShapeClipperJoinType)joinType;
    }
    
    /// <summary>
    /// Converts a Clipper <see cref="EndType"/> to the local <see cref="ShapeClipperEndType"/> enum.
    /// </summary>
    /// <param name="endType">The Clipper end type to convert.</param>
    /// <returns>The equivalent <see cref="ShapeClipperEndType"/> value.</returns>
    public static ShapeClipperEndType ToShapeClipperEndType(this EndType endType)
    {
        return (ShapeClipperEndType)endType;
    }
    
    /// <summary>
    /// Converts a Clipper <see cref="ClipType"/> to the local <see cref="ShapeClipperClipType"/> enum.
    /// </summary>
    /// <param name="clipType">The Clipper clip type to convert.</param>
    /// <returns>The equivalent <see cref="ShapeClipperClipType"/> value.</returns>
    public static ShapeClipperClipType ToShapeClipperClipType(this ClipType clipType)
    {
        return (ShapeClipperClipType)clipType;
    }

    /// <summary>
    /// Converts a <see cref="LineCapType"/> to the closest matching <see cref="ShapeClipperEndType"/>.
    /// </summary>
    /// <param name="capType">The line cap type to convert.</param>
    /// <returns>The closest matching <see cref="ShapeClipperEndType"/> value.</returns>
    /// <remarks>
    /// Both <see cref="LineCapType.Capped"/> and <see cref="LineCapType.CappedExtended"/> map to <see cref="ShapeClipperEndType.Round"/>.
    /// </remarks>
    public static ShapeClipperEndType ToShapeClipperEndType(this LineCapType capType)
    {
        if (capType is LineCapType.None) return ShapeClipperEndType.Butt;
        if (capType is LineCapType.Extended) return ShapeClipperEndType.Square;
        if (capType is LineCapType.Capped) return ShapeClipperEndType.Round;
        if (capType is LineCapType.CappedExtended) return ShapeClipperEndType.Round;
        else return ShapeClipperEndType.Butt;
    }
    
    /// <summary>
    /// Converts a <see cref="ShapeClipperEndType"/> to the closest matching <see cref="LineCapType"/>.
    /// </summary>
    /// <param name="endType">The ShapeClipper end type to convert.</param>
    /// <returns>The closest matching <see cref="LineCapType"/> value.</returns>
    /// <remarks>
    /// Polygon, joined, and butt end types all map to <see cref="LineCapType.None"/>.
    /// </remarks>
    public static LineCapType ToLineCapType(this ShapeClipperEndType endType)
    {
        if (endType is ShapeClipperEndType.Polygon) return LineCapType.None;
        if (endType is ShapeClipperEndType.Joined) return LineCapType.None;
        if (endType is ShapeClipperEndType.Butt) return LineCapType.None;
        if (endType is ShapeClipperEndType.Square) return LineCapType.Extended;
        if (endType is ShapeClipperEndType.Round) return LineCapType.CappedExtended;
        else return LineCapType.None;
    }
    
    #endregion
}