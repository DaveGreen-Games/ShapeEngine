using System.Numerics;
using ShapeEngine.Geometry.TriangulationDef;
using ShapeEngine.ShapeClipper;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.PolylineDef;

public partial class Polyline
{
    #region Math

    /// <summary>
    /// Gets the centroid point along the polyline, based on its length.
    /// </summary>
    /// <returns>The centroid point located at the halfway mark along the polyline's length.</returns>
    /// <remarks>
    /// This method uses linear interpolation to find the midpoint along the polyline.
    /// </remarks>
    public Vector2 GetCentroidOnLine()
    {
        return GetPoint(0.5f);
        // if (Count <= 0) return new(0f);
        // else if (Count == 1) return this[0];
        // float halfLengthSq = LengthSquared * 0.5f;
        // var segments = GetEdges();
        // float curLengthSq = 0f; 
        // foreach (var seg in segments)
        // {
        //     float segLengthSq = seg.LengthSquared;
        //     curLengthSq += segLengthSq;
        //     if (curLengthSq >= halfLengthSq)
        //     {
        //         float dif = curLengthSq - halfLengthSq;
        //         return seg.Center + seg.Dir * MathF.Sqrt(dif);
        //     }
        // }
        // return new Vector2();
    }

    /// <summary>
    /// Gets a point along the polyline at a normalized position.
    /// </summary>
    /// <param name="f">A value between 0 and 1 representing the normalized position along the polyline's total length.</param>
    /// <returns>
    /// The interpolated <see cref="Vector2"/> at the specified normalized position.
    /// </returns>
    /// <remarks>
    /// If <paramref name="f"/> is 0, returns the first point; if 1, returns the last.
    /// For intermediate values, interpolates along the segments.
    /// </remarks>
    public Vector2 GetPoint(float f)
    {
        if (Count == 0) return new();
        if (Count == 1) return this[0];
        if (Count == 2) return this[0].Lerp(this[1], f);
        if (f <= 0f) return this[0];
        if (f >= 1f) return this[^1];

        var totalLengthSq = GetLengthSquared();
        var targetLengthSq = totalLengthSq * f;
        var curLengthSq = 0f;
        for (var i = 0; i < Count - 1; i++)
        {
            var start = this[i];
            var end = this[(i + 1) % Count];
            var lSq = (start - end).LengthSquared();
            if (lSq <= 0) continue;

            if (curLengthSq + lSq >= targetLengthSq)
            {
                var aF = curLengthSq / totalLengthSq;
                var bF = (curLengthSq + lSq) / totalLengthSq;
                var curF = ShapeMath.LerpInverseFloat(aF, bF, f);
                return start.Lerp(end, curF);
            }

            curLengthSq += lSq;
        }

        return new();
    }

    /// <summary>
    /// Calculates the total length of the polyline by summing the distances between consecutive points.
    /// </summary>
    /// <returns>The total length as a <see cref="float"/>. Returns 0 if fewer than 2 points.</returns>
    /// <remarks>
    /// The length is the sum of the Euclidean distances between each pair of consecutive points.
    /// </remarks>
    public float GetLength()
    {
        if (this.Count < 2) return 0f;
        var length = 0f;
        for (var i = 0; i < Count - 1; i++)
        {
            var w = this[i + 1] - this[i];
            length += w.Length();
        }

        return length;
    }

    /// <summary>
    /// Calculates the squared total length of the polyline.
    /// </summary>
    /// <returns>The squared length as a <see cref="float"/>. Returns 0 if fewer than 2 points.</returns>
    /// <remarks>
    /// Useful for performance when only relative lengths are needed, as it avoids the square root operation.
    /// </remarks>
    public float GetLengthSquared()
    {
        if (this.Count < 2) return 0f;
        var lengthSq = 0f;
        for (var i = 0; i < Count - 1; i++)
        {
            var w = this[i + 1] - this[i];
            lengthSq += w.LengthSquared();
        }

        return lengthSq;
    }

    #endregion

    #region Transform

    /// <summary>
    /// Sets the centroid of the polyline to a new position by translating all points.
    /// </summary>
    /// <param name="newPosition">The new position for the centroid.</param>
    /// <remarks>
    /// This method moves the entire polyline so that its mean centroid matches <paramref name="newPosition"/>.
    /// </remarks>
    public void SetPosition(Vector2 newPosition)
    {
        var delta = newPosition - GetCentroidMean();
        ChangePosition(delta);
    }

    /// <summary>
    /// Rotates the polyline around its centroid by a specified angle in radians.
    /// </summary>
    /// <param name="rotRad">The rotation angle in radians.</param>
    /// <remarks>
    /// The rotation is applied in-place to all points, using the mean centroid as the origin.
    /// </remarks>
    public void ChangeRotation(float rotRad)
    {
        if (Count < 2) return;
        var origin = GetCentroidMean();
        for (int i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            this[i] = origin + w.Rotate(rotRad);
        }
    }

    /// <summary>
    /// Sets the absolute rotation of the polyline so that the vector from the centroid to the first point matches the specified angle.
    /// </summary>
    /// <param name="angleRad">The target angle in radians.</param>
    /// <remarks>
    /// The polyline is rotated in-place to achieve the desired orientation.
    /// </remarks>
    public void SetRotation(float angleRad)
    {
        if (Count < 2) return;

        var origin = GetCentroidMean();
        var curAngle = (this[0] - origin).AngleRad();
        var rotRad = ShapeMath.GetShortestAngleRad(curAngle, angleRad);
        ChangeRotation(rotRad, origin);
    }

    /// <summary>
    /// Scales the polyline uniformly about its centroid.
    /// </summary>
    /// <param name="scale">The scale factor to apply to all points relative to the centroid.</param>
    /// <remarks>
    /// A scale of <c>1</c> leaves the polyline unchanged; values greater than <c>1</c> enlarge it,
    /// and values between <c>0 and 1</c> shrink it.
    /// </remarks>
    public void ScaleSize(float scale)
    {
        if (Count < 2) return;
        var origin = GetCentroidMean();
        for (int i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            this[i] = origin + w * scale;
        }
    }

    /// <summary>
    /// Changes the length of each vector from the centroid to each point by a specified amount.
    /// </summary>
    /// <param name="amount">The amount to change the length of each vector from the centroid.</param>
    /// <remarks>
    /// Positive values increase the size, negative values decrease it. The direction of each point from the centroid is preserved.
    /// </remarks>
    public void ChangeSize(float amount)
    {
        if (Count < 2) return;
        var origin = GetCentroidMean();
        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            this[i] = origin + w.ChangeLength(amount);
        }
    }

    /// <summary>
    /// Sets the distance from the centroid to each point to a specified value.
    /// </summary>
    /// <param name="size">The new distance from the centroid to each point.</param>
    /// <remarks>
    /// All points are set to be exactly <paramref name="size"/> units from the centroid, preserving their directions.
    /// </remarks>
    public void SetSize(float size)
    {
        if (Count < 2) return;
        var origin = GetCentroidMean();
        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            this[i] = origin + w.SetLength(size);
        }
    }

    #endregion
    
    #region Outline Triangulation
    /// <summary>
    /// Triangulates this polyline's stroked outline and writes the generated triangles into the provided <see cref="Triangulation"/>.
    /// </summary>
    /// <param name="result">The destination triangulation that receives the generated triangles.</param>
    /// <param name="thickness">The stroke thickness to triangulate.</param>
    /// <param name="miterLimit">The maximum miter length factor used for stroke joins.</param>
    /// <param name="beveled">Whether joins that exceed the miter limit should be beveled.</param>
    /// <param name="endType">The end-cap style to use for the open polyline.</param>
    /// <param name="useDelaunay">Whether to apply Delaunay refinement when creating the triangulation.</param>
    /// <remarks>
    /// This method triangulates only the stroked outline of the polyline. It does not modify the polyline itself.
    /// </remarks>
    public void TriangulateOutline(Triangulation result, float thickness, float miterLimit = 2f, bool beveled = false, ShapeClipperEndType endType = ShapeClipperEndType.Butt, bool useDelaunay = false)
    {
        ShapeClipper2D.CreatePolylineTriangulation(this, thickness, miterLimit, beveled, endType, useDelaunay, result);
    }
    
    /// <summary>
    /// Triangulates this polyline's stroked outline and writes the generated triangles into the provided <see cref="TriMesh"/>.
    /// </summary>
    /// <param name="result">The destination triangle mesh that receives the generated vertices and indices.</param>
    /// <param name="thickness">The stroke thickness to triangulate.</param>
    /// <param name="miterLimit">The maximum miter length factor used for stroke joins.</param>
    /// <param name="beveled">Whether joins that exceed the miter limit should be beveled.</param>
    /// <param name="endType">The end-cap style to use for the open polyline.</param>
    /// <param name="useDelaunay">Whether to apply Delaunay refinement when creating the triangulation.</param>
    /// <remarks>
    /// This method triangulates only the stroked outline of the polyline. It does not modify the polyline itself.
    /// </remarks>
    public void TriangulateOutline(TriMesh result, float thickness, float miterLimit = 2f, bool beveled = false, ShapeClipperEndType endType = ShapeClipperEndType.Butt, bool useDelaunay = false)
    {
        ShapeClipper2D.CreatePolylineTriangulation(this, thickness, miterLimit, beveled, endType, useDelaunay, result);
    }
    #endregion
    
    #region Outline Perimeter Triangulation
    
    /// <summary>
    /// Builds a new open <see cref="Polyline"/> representing the first portion of this polyline up to the specified traveled distance.
    /// </summary>
    /// <param name="perimeterToDraw">The distance to trace along the polyline. Values less than or equal to zero are ignored.</param>
    /// <param name="result">The destination polyline that will be cleared and populated with the traced points.</param>
    /// <remarks>
    /// The resulting polyline follows the original vertex order from the first point toward the last point and does not wrap.
    /// If the requested distance ends in the middle of a segment, an interpolated endpoint is added.
    /// </remarks>
    public void ToPolylinePerimeter(float perimeterToDraw, Polyline result)
    {
        ShapeClipper2D.ToPolylinePerimeter(this, perimeterToDraw, result);
        // if (perimeterToDraw <= 0f) return;
        //
        // if (Count <= 1) return;
        //
        // result.Clear();
        //
        // bool ccw = perimeterToDraw > 0;
        // float absPerimeterToDraw = MathF.Abs(perimeterToDraw);
        // float accumulatedPerimeter = 0f;
        // int currentIndex = ccw ? 0 : Count - 1;
        //
        //
        // // Create polyline based on perimeter.
        // // // Positive walks forward from 0.
        // // // Negative walks backward from Count - 1.
        // // // No wrapping: polyline stays open.
        // while (absPerimeterToDraw > accumulatedPerimeter)
        // {
        //     int nextIndex = currentIndex + (ccw ? 1 : -1);
        //     if (nextIndex < 0 || nextIndex >= Count) break; //safety
        //     
        //     var cur = this[currentIndex];
        //     var next = this[nextIndex];
        //     result.Add(cur);
        //     
        //     float segmentLength = (next - cur).Length();
        //
        //     if (accumulatedPerimeter + segmentLength >= absPerimeterToDraw)
        //     {
        //         float remainingPerimeter = absPerimeterToDraw - accumulatedPerimeter;
        //         float f = segmentLength <= 0f ? 0f : remainingPerimeter / segmentLength;
        //         var end = cur.Lerp(next, f);
        //         result.Add(end);
        //         break;
        //     }
        //
        //     accumulatedPerimeter += segmentLength;
        //     currentIndex = nextIndex;
        //
        //     // Reached the open end of the polyline; add the endpoint and stop.
        //     if ((ccw && currentIndex == Count - 1) || (!ccw && currentIndex == 0))
        //     {
        //         result.Add(this[currentIndex]);
        //         break;
        //     }
        // }
    }
    
    /// <summary>
    /// Triangulates the stroked outline of a partial section of this polyline, measured by traveled distance, and writes the generated triangles into the provided <see cref="Triangulation"/>.
    /// </summary>
    /// <param name="result">The destination triangulation that receives the generated triangles.</param>
    /// <param name="perimeterToDraw">The distance to trace along the polyline before triangulating the resulting stroked section.</param>
    /// <param name="thickness">The stroke thickness used to build the outline geometry.</param>
    /// <param name="miterLimit">The maximum miter length factor used for joins between consecutive segments.</param>
    /// <param name="beveled">Whether joins that exceed the miter limit should fall back to beveled corners.</param>
    /// <param name="capType">The cap style to use at the open ends of the generated partial stroke.</param>
    /// <param name="useDelaunay">Whether to apply Delaunay refinement when creating the triangulation.</param>
    /// <remarks>
    /// This method does not modify the polyline itself. It delegates to the clipping backend to derive the partial stroke and triangulate it.
    /// </remarks>
    public void TriangulateOutlinePerimeter(Triangulation result, float perimeterToDraw, float thickness, 
        float miterLimit = 2f, bool beveled = false, LineCapType capType = LineCapType.CappedExtended, bool useDelaunay = false)
    {
        ShapeClipper2D.CreatePolylineTriangulationPerimeter(this, perimeterToDraw, thickness, miterLimit, beveled, capType.ToShapeClipperEndType(), useDelaunay, result);
    }
    
    /// <summary>
    /// Triangulates the stroked outline of a partial section of this polyline, measured by traveled distance, and writes the generated geometry into the provided <see cref="TriMesh"/>.
    /// </summary>
    /// <param name="result">The destination triangle mesh that receives the generated vertices and indices.</param>
    /// <param name="perimeterToDraw">The distance to trace along the polyline before triangulating the resulting stroked section.</param>
    /// <param name="thickness">The stroke thickness used to build the outline geometry.</param>
    /// <param name="miterLimit">The maximum miter length factor used for joins between consecutive segments.</param>
    /// <param name="beveled">Whether joins that exceed the miter limit should fall back to beveled corners.</param>
    /// <param name="capType">The cap style to use at the open ends of the generated partial stroke.</param>
    /// <param name="useDelaunay">Whether to apply Delaunay refinement when creating the triangulation.</param>
    /// <remarks>
    /// This method does not modify the polyline itself. It delegates to the clipping backend to derive the partial stroke and triangulate it.
    /// </remarks>
    public void TriangulateOutlinePerimeter(TriMesh result, float perimeterToDraw, float thickness, 
        float miterLimit = 2f, bool beveled = false, LineCapType capType = LineCapType.CappedExtended, bool useDelaunay = false)
    {
        ShapeClipper2D.CreatePolylineTriangulationPerimeter(this, perimeterToDraw, thickness, miterLimit, beveled, capType.ToShapeClipperEndType(), useDelaunay, result);
    }
    #endregion

    #region Outline Percentage Triangulation
    /// <summary>
    /// Builds a new open <see cref="Polyline"/> representing the first portion of this polyline up to the specified fraction of its total length.
    /// </summary>
    /// <param name="f">The fraction of the total polyline length to include. Values less than or equal to zero are ignored, and values greater than or equal to one copy the full polyline.</param>
    /// <param name="result">The destination polyline that receives the traced points.</param>
    /// <remarks>
    /// This method converts the requested fraction into an absolute length along the polyline, then delegates to <see cref="ToPolylinePerimeter(float, Polyline)"/>.
    /// </remarks>
    public void ToPolylinePercentage(float f, Polyline result)
    {
        ShapeClipper2D.ToPolylinePercentage(this, f, result);
        // if (f <= 0) return;
        //
        // if (Count <= 1) return;
        //
        // if (f >= 1f)
        // {
        //     result.Clear();
        //     foreach (var p in this)
        //     {
        //         result.Add(p);
        //     }
        //     return;
        // }
        //
        // float totalPerimeter = 0f;
        //
        // for (var i = 0; i < Count - 1; i++)
        // {
        //     var start = this[i];
        //     var end = this[i + 1];
        //     float l = (end - start).Length();
        //     totalPerimeter += l;
        // }
        // ToPolylinePerimeter(totalPerimeter * f, result);
    }

    /// <summary>
    /// Triangulates the stroked outline of a partial section of this polyline, measured as a fraction of its total length, and writes the generated triangles into the provided <see cref="Triangulation"/>.
    /// </summary>
    /// <param name="result">The destination triangulation that receives the generated triangles.</param>
    /// <param name="f">The fraction of the total polyline length to include before triangulating the resulting stroked section.</param>
    /// <param name="thickness">The stroke thickness used to build the outline geometry.</param>
    /// <param name="miterLimit">The maximum miter length factor used for joins between consecutive segments.</param>
    /// <param name="beveled">Whether joins that exceed the miter limit should fall back to beveled corners.</param>
    /// <param name="capType">The cap style to use at the open ends of the generated partial stroke.</param>
    /// <param name="useDelaunay">Whether to apply Delaunay refinement when creating the triangulation.</param>
    /// <remarks>
    /// This method does not modify the polyline itself. It delegates to the clipping backend to derive the partial stroke and triangulate it.
    /// </remarks>
    public void TriangulateOutlinePercentage(Triangulation result, float f, float thickness, 
        float miterLimit = 2f, bool beveled = false, LineCapType capType = LineCapType.CappedExtended, bool useDelaunay = false)
    {
        ShapeClipper2D.CreatePolylineTriangulationPercentage(this, f, thickness, miterLimit, beveled, capType.ToShapeClipperEndType(), useDelaunay, result);
    }
   
    /// <summary>
    /// Triangulates the stroked outline of a partial section of this polyline, measured as a fraction of its total length, and writes the generated geometry into the provided <see cref="TriMesh"/>.
    /// </summary>
    /// <param name="result">The destination triangle mesh that receives the generated vertices and indices.</param>
    /// <param name="f">The fraction of the total polyline length to include before triangulating the resulting stroked section.</param>
    /// <param name="thickness">The stroke thickness used to build the outline geometry.</param>
    /// <param name="miterLimit">The maximum miter length factor used for joins between consecutive segments.</param>
    /// <param name="beveled">Whether joins that exceed the miter limit should fall back to beveled corners.</param>
    /// <param name="capType">The cap style to use at the open ends of the generated partial stroke.</param>
    /// <param name="useDelaunay">Whether to apply Delaunay refinement when creating the triangulation.</param>
    /// <remarks>
    /// This method does not modify the polyline itself. It delegates to the clipping backend to derive the partial stroke and triangulate it.
    /// </remarks>
    public void TriangulateOutlinePercentage(TriMesh result, float f, float thickness, 
        float miterLimit = 2f, bool beveled = false, LineCapType capType = LineCapType.CappedExtended, bool useDelaunay = false)
    {
        ShapeClipper2D.CreatePolylineTriangulationPercentage(this, f, thickness, miterLimit, beveled, capType.ToShapeClipperEndType(), useDelaunay, result);
    }
    #endregion
}