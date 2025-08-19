using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.PointsDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.PolygonDef;


public partial class Polygon
{
    #region Math
    /// <summary>
    /// Gets the projected shape points by translating each vertex by a vector.
    /// </summary>
    /// <param name="v">The vector to project along.</param>
    /// <returns>A <see cref="Points"/> collection of projected points, or null if the vector is zero.</returns>
    public Points? GetProjectedShapePoints(Vector2 v)
    {
        if (v.LengthSquared() <= 0f) return null;

        var points = new Points(Count);
        for (var i = 0; i < Count; i++)
        {
            points.Add(this[i]);
            points.Add(this[i] + v);
        }

        return points;
    }
    /// <summary>
    /// Projects the polygon along a vector and returns the convex hull of the result.
    /// </summary>
    /// <param name="v">The vector to project along.</param>
    /// <returns>A new <see cref="Polygon"/> representing the projected convex hull,
    /// or null if the vector is zero.</returns>
    public Polygon? ProjectShape(Vector2 v)
    {
        if (v.LengthSquared() <= 0f || Count < 3) return null;

        var points = new Points(Count * 2);
        for (var i = 0; i < Count; i++)
        {
            points.Add(this[i]);
            points.Add(this[i] + v);
        }

        return FindConvexHull(points);
    }
    /// <summary>
    /// Calculates the centroid (center of mass) of the polygon.
    /// </summary>
    /// <returns>The centroid as a <see cref="Vector2"/>.</returns>
    public Vector2 GetCentroid()
    {
        if (Count <= 0) return new();
        if (Count == 1) return this[0];
        if (Count == 2) return (this[0] + this[1]) / 2;
        if (Count == 3) return (this[0] + this[1] + this[2]) / 3;

        var centroid = new Vector2();
        var area = 0f;
        for (int i = Count - 1; i >= 0; i--)
        {
            var a = this[i];
            var index = ShapeMath.WrapIndex(Count, i - 1);
            var b = this[index];
            float cross = a.X * b.Y - b.X * a.Y; //clockwise 
            area += cross;
            centroid += (a + b) * cross;
        }

        area *= 0.5f;
        return centroid / (area * 6);

        //return GetCentroidMean();
        // Vector2 result = new();

        // for (int i = 0; i < Count; i++)
        // {
        // var a = this[i];
        // var b = this[(i + 1) % Count];
        //// float factor = a.X * b.Y - b.X * a.Y; //clockwise 
        // float factor = a.Y * b.X - a.X * b.Y; //counter clockwise
        // result.X += (a.X + b.X) * factor;
        // result.Y += (a.Y + b.Y) * factor;
        // }

        // return result * (1f / (GetArea() * 6f));
    }
    /// <summary>
    /// Calculates the perimeter (total edge length) of the polygon.
    /// </summary>
    /// <returns>The perimeter length.</returns>
    public float GetPerimeter()
    {
        if (this.Count < 3) return 0f;
        float length = 0f;
        for (int i = 0; i < Count; i++)
        {
            Vector2 w = this[(i + 1) % Count] - this[i];
            length += w.Length();
        }

        return length;
    }
    /// <summary>
    /// Calculates the squared perimeter of the polygon.
    /// </summary>
    /// <returns>The squared perimeter length.</returns>
    public float GetPerimeterSquared()
    {
        if (Count < 3) return 0f;
        var lengthSq = 0f;
        for (var i = 0; i < Count; i++)
        {
            var w = this[(i + 1) % Count] - this[i];
            lengthSq += w.LengthSquared();
        }

        return lengthSq;
    }
    /// <summary>
    /// Calculates the squared diameter (maximum distance from centroid to a vertex).
    /// </summary>
    /// <returns>The squared diameter.</returns>
    private float GetDiameterSquared()
    {
        if (Count <= 2) return 0;
        var center = GetCentroid();
        var maxDisSquared = -1f;
        for (int i = 0; i < Count; i++)
        {
            var p = this[0];
            var disSquared = (p - center).LengthSquared();
            if (maxDisSquared < 0 || disSquared > maxDisSquared)
            {
                maxDisSquared = disSquared;
            }
        }

        return maxDisSquared;
    }
    /// <summary>
    /// Calculates the diameter (maximum distance from centroid to a vertex).
    /// </summary>
    /// <returns>The diameter.</returns>
    private float GetDiameter()
    {
        if (Count <= 2) return 0;
        return MathF.Sqrt(GetDiameterSquared());
    }
    /// <summary>
    /// Calculates the signed area of the polygon.
    /// </summary>
    /// <returns>The area. Negative if the polygon is clockwise.</returns>
    public float GetArea()
    {
        if (Count < 3) return 0f;
        var area = 0f;
        for (int i = Count - 1; i >= 0; i--) //backwards to be clockwise
        {
            var a = this[i];
            var index = ShapeMath.WrapIndex(Count, i - 1);
            var b = this[index];
            float cross = a.X * b.Y - b.X * a.Y; //clockwise 
            area += cross;
        }

        return area / 2f;
    }
    /// <summary>
    /// Determines if the polygon's winding order is clockwise.
    /// </summary>
    /// <returns>True if clockwise; otherwise, false.</returns>
    public bool IsClockwise() => GetArea() < 0f;

    /// <summary>
    /// Determines if the polygon is convex.
    /// </summary>
    /// <returns>True if convex; otherwise, false.</returns>
    /// <remarks>
    /// <list type="bullet">
    /// <item>
    /// A <c>convex</c> polygon is a polygon where all interior angles are less than 180°,
    /// and every line segment between any two points inside the polygon lies entirely within the polygon.
    /// In other words, no vertices "point inward."
    /// </item>
    /// <item>
    /// A <c>concave</c> polygon is a polygon that has at least one interior angle greater than 180°,
    /// and at least one line segment between points inside the polygon passes outside the polygon.
    /// This means it has at least one "caved-in" or inward-pointing vertex.
    /// </item>
    /// </list>
    /// </remarks>
    public bool IsConvex()
    {
        int num = Count;
        if (num < 3) return false;
        
        bool? sign = null;
        for (var i = 0; i < num; i++)
        {
            int prev = (i + num - 1) % num;//wraps around to last index if i is 0
            int next = (i + 1) % num; //wraps around to 0 if i is last index
            var d0 = this[i] - this[prev];
            var d1 = this[next] - this[i];
            float cross = d0.Cross(d1);
        
            //Ignores collinear points and only sets the reference sign on the first valid cross product.
            if (cross == 0f) continue;
        
            //Checks if the current sign of the cross-product is the same as the last
            //If it is not, the polygon is self-intersecting
            bool currentSign = cross > 0f;
            if (sign == null)
                sign = currentSign;
            else if (sign != currentSign)
                return false;
        }
        
        //all cross-product signs were either collinear or the same.
        return true;
    }
    
    /// <summary>
    /// Determines if a polygon defined by a list of vertices is convex.
    /// </summary>
    /// <param name="vertices">The list of vertices representing the polygon.</param>
    /// <returns>True if the polygon is convex; otherwise, false.</returns>
    /// <remarks>
    /// <list type="bullet">
    /// <item><c>Convex</c>:
    /// A convex polygon is a polygon where all interior angles are less than 180°,
    /// and every line segment between any two points inside the polygon lies entirely within the polygon.
    /// This means none of its vertices "point inward," and the polygon does not have any indentations. </item>
    /// <item><c>Concave</c>:
    /// A concave polygon has at least one interior angle greater than 180°
    /// and at least one vertex that points inward (self-intersecting).</item>
    /// </list>
    /// </remarks>
    public static bool IsConvex(List<Vector2> vertices)
    {
        int num = vertices.Count;
        if (num < 3) return false;
        
        bool? sign = null;
        for (var i = 0; i < num; i++)
        {
            int prev = (i + num - 1) % num;//wraps around to last index if i is 0
            int next = (i + 1) % num; //wraps around to 0 if i is last index
            var d0 = vertices[i] - vertices[prev];
            var d1 = vertices[next] - vertices[i];
            float cross = d0.Cross(d1);
        
            //Ignores collinear points and only sets the reference sign on the first valid cross-product.
            if (cross == 0f) continue;
        
            //Checks if the current sign of the cross-product is the same as the last
            //If it is not, the polygon is self-intersecting (concave)
            bool currentSign = cross > 0f;
            if (sign == null)
                sign = currentSign;
            else if (sign != currentSign)
                return false;
        }
        
        //all cross-product signs were either collinear or the same.
        return true;
    }
   
    
    /// <summary>
    /// Converts the polygon to a <see cref="Points"/> collection.
    /// </summary>
    /// <returns>A <see cref="Points"/> collection of the polygon's vertices.</returns>
    public Points ToPoints()
    {
        return new(this);
    }
    /// <summary>
    /// Calculates the mean centroid (average of all vertices).
    /// </summary>
    /// <returns>The mean centroid as a <see cref="Vector2"/>.</returns>
    public Vector2 GetCentroidMean()
    {
        if (Count <= 0) return new(0f);
        if (Count == 1) return this[0];
        if (Count == 2) return (this[0] + this[1]) / 2;
        if (Count == 3) return (this[0] + this[1] + this[2]) / 3;
        var total = new Vector2(0f);
        foreach (var p in this)
        {
            total += p;
        }

        return total / Count;
    }
    /// <summary>
    /// Computes the length of this polygon's apothem. Only valid for regular polygons.
    /// </summary>
    /// <returns>The length of the apothem.</returns>
    /// <remarks>More info: http://en.wikipedia.org/wiki/Apothem</remarks>
    public float GetApothem() => (this.GetCentroid() - (this[0].Lerp(this[1], 0.5f))).Length();
    #endregion
    
    #region Transform
    /// <summary>
    /// Sets the position of the polygon's centroid.
    /// </summary>
    /// <param name="newPosition">The new centroid position.</param>
    public void SetPosition(Vector2 newPosition)
    {
        var centroid = GetCentroid();
        var delta = newPosition - centroid;
        ChangePosition(delta);
    }
    /// <summary>
    /// Rotates the polygon by a given angle in radians.
    /// </summary>
    /// <param name="rotRad">The rotation angle in radians.</param>
    public void ChangeRotation(float rotRad)
    {
        if (Count < 3) return;
        var origin = GetCentroid();
        for (int i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            this[i] = origin + w.Rotate(rotRad);
        }
    }
    /// <summary>
    /// Sets the absolute rotation of the polygon.
    /// </summary>
    /// <param name="angleRad">The target rotation angle in radians.</param>
    public void SetRotation(float angleRad)
    {
        if (Count < 3) return;

        var origin = GetCentroid();
        var curAngle = (this[0] - origin).AngleRad();
        var rotRad = ShapeMath.GetShortestAngleRad(curAngle, angleRad);
        ChangeRotation(rotRad, origin);
    }
    /// <summary>
    /// Scales the polygon uniformly about its centroid.
    /// </summary>
    /// <param name="scale">The scale factor.</param>
    public void ScaleSize(float scale)
    {
        if (Count < 3) return;
        var origin = GetCentroid();
        for (int i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            this[i] = origin + w * scale;
        }
    }
    /// <summary>
    /// Changes the size of the polygon by a given amount (relative to each vertex).
    /// </summary>
    /// <param name="amount">The amount to change the size.</param>
    public void ChangeSize(float amount)
    {
        if (Count < 3) return;
        var origin = GetCentroid();
        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            this[i] = origin + w.ChangeLength(amount);
        }
    }
    /// <summary>
    /// Sets the size (distance from centroid) of the polygon.
    /// </summary>
    /// <param name="size">The new size (distance from centroid to each vertex).</param>
    public void SetSize(float size)
    {
        if (Count < 3) return;
        var origin = GetCentroid();
        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            this[i] = origin + w.SetLength(size);
        }
    }
    /// <summary>
    /// Returns a copy of the polygon with its centroid set to a new position.
    /// </summary>
    /// <param name="newPosition">The new centroid position.</param>
    /// <returns>A new <see cref="Polygon"/> with the updated position, or null if invalid.</returns>
    public Polygon? SetPositionCopy(Vector2 newPosition)
    {
        if (Count < 3) return null;
        var centroid = GetCentroid();
        var delta = newPosition - centroid;
        return ChangePositionCopy(delta);
    }
    /// <summary>
    /// Returns a copy of the polygon translated by an offset.
    /// </summary>
    /// <param name="offset">The translation offset.</param>
    /// <returns>A new <see cref="Polygon"/> with the updated position, or null if invalid.</returns>
    public new Polygon? ChangePositionCopy(Vector2 offset)
    {
        if (Count < 3) return null;
        var newPolygon = new Polygon(this.Count);
        for (int i = 0; i < Count; i++)
        {
            newPolygon.Add(this[i] + offset);
        }

        return newPolygon;
    }
    /// <summary>
    /// Returns a copy of the polygon rotated by a given angle around an origin.
    /// </summary>
    /// <param name="rotRad">The rotation angle in radians.</param>
    /// <param name="origin">The origin to rotate around.</param>
    /// <returns>A new <see cref="Polygon"/> with the updated rotation, or null if invalid.</returns>
    public new Polygon? ChangeRotationCopy(float rotRad, Vector2 origin)
    {
        if (Count < 3) return null;
        var newPolygon = new Polygon(this.Count);
        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            newPolygon.Add(origin + w.Rotate(rotRad));
        }

        return newPolygon;
    }
    /// <summary>
    /// Returns a copy of the polygon rotated by a given angle around its centroid.
    /// </summary>
    /// <param name="rotRad">The rotation angle in radians.</param>
    /// <returns>A new <see cref="Polygon"/> with the updated rotation, or null if invalid.</returns>
    public Polygon? ChangeRotationCopy(float rotRad)
    {
        if (Count < 3) return null;
        return ChangeRotationCopy(rotRad, GetCentroid());
    }
    /// <summary>
    /// Returns a copy of the polygon with its absolute rotation set around an origin.
    /// </summary>
    /// <param name="angleRad">The target rotation angle in radians.</param>
    /// <param name="origin">The origin to rotate around.</param>
    /// <returns>A new <see cref="Polygon"/> with the updated rotation, or null if invalid.</returns>
    public new Polygon? SetRotationCopy(float angleRad, Vector2 origin)
    {
        if (Count < 3) return null;
        var curAngle = (this[0] - origin).AngleRad();
        var rotRad = ShapeMath.GetShortestAngleRad(curAngle, angleRad);
        return ChangeRotationCopy(rotRad, origin);
    }
    /// <summary>
    /// Returns a copy of the polygon with its absolute rotation set around its centroid.
    /// </summary>
    /// <param name="angleRad">The target rotation angle in radians.</param>
    /// <returns>A new <see cref="Polygon"/> with the updated rotation, or null if invalid.</returns>
    public Polygon? SetRotationCopy(float angleRad)
    {
        if (Count < 3) return null;

        var origin = GetCentroid();
        var curAngle = (this[0] - origin).AngleRad();
        var rotRad = ShapeMath.GetShortestAngleRad(curAngle, angleRad);
        return ChangeRotationCopy(rotRad, origin);
    }
    /// <summary>
    /// Returns a copy of the polygon scaled uniformly about its centroid.
    /// </summary>
    /// <param name="scale">The scale factor.</param>
    /// <returns>A new <see cref="Polygon"/> with the updated scale, or null if invalid.</returns>
    public Polygon? ScaleSizeCopy(float scale)
    {
        if (Count < 3) return null;
        return ScaleSizeCopy(scale, GetCentroid());
    }
    /// <summary>
    /// Returns a copy of the polygon scaled uniformly about a given origin.
    /// </summary>
    /// <param name="scale">The scale factor.</param>
    /// <param name="origin">The origin to scale about.</param>
    /// <returns>A new <see cref="Polygon"/> with the updated scale, or null if invalid.</returns>
    public new Polygon? ScaleSizeCopy(float scale, Vector2 origin)
    {
        if (Count < 3) return null;
        var newPolygon = new Polygon(this.Count);

        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            newPolygon.Add(origin + w * scale);
        }

        return newPolygon;
    }
    /// <summary>
    /// Returns a copy of the polygon scaled non-uniformly about a given origin.
    /// </summary>
    /// <param name="scale">The scale vector.</param>
    /// <param name="origin">The origin to scale about.</param>
    /// <returns>A new <see cref="Polygon"/> with the updated scale, or null if invalid.</returns>
    public new Polygon? ScaleSizeCopy(Vector2 scale, Vector2 origin)
    {
        if (Count < 3) return null;
        var newPolygon = new Polygon(this.Count);

        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            newPolygon.Add(origin + w * scale);
        }

        return newPolygon;
    }
    /// <summary>
    /// Returns a copy of the polygon with its size changed by a given amount about a given origin.
    /// </summary>
    /// <param name="amount">The amount to change the size.</param>
    /// <param name="origin">The origin to scale about.</param>
    /// <returns>A new <see cref="Polygon"/> with the updated size, or null if invalid.</returns>
    public new Polygon? ChangeSizeCopy(float amount, Vector2 origin)
    {
        if (Count < 3) return null;
        var newPolygon = new Polygon(this.Count);

        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            newPolygon.Add(origin + w.ChangeLength(amount));
        }

        return newPolygon;
    }
    /// <summary>
    /// Returns a copy of the polygon with its size changed by a given amount about its centroid.
    /// </summary>
    /// <param name="amount">The amount to change the size.</param>
    /// <returns>A new <see cref="Polygon"/> with the updated size, or null if invalid.</returns>
    public Polygon? ChangeSizeCopy(float amount)
    {
        if (Count < 3) return null;
        return ChangeSizeCopy(amount, GetCentroid());
    }
    /// <summary>
    /// Returns a copy of the polygon with its size set to a given value about a given origin.
    /// </summary>
    /// <param name="size">The new size (distance from origin to each vertex).</param>
    /// <param name="origin">The origin to scale about.</param>
    /// <returns>A new <see cref="Polygon"/> with the updated size, or null if invalid.</returns>
    public new Polygon? SetSizeCopy(float size, Vector2 origin)
    {
        if (Count < 3) return null;
        var newPolygon = new Polygon(this.Count);

        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            newPolygon.Add(origin + w.SetLength(size));
        }

        return newPolygon;
    }
    /// <summary>
    /// Returns a copy of the polygon with its size set to a given value about its centroid.
    /// </summary>
    /// <param name="size">The new size (distance from centroid to each vertex).</param>
    /// <returns>A new <see cref="Polygon"/> with the updated size, or null if invalid.</returns>
    public Polygon? SetSizeCopy(float size)
    {
        if (Count < 3) return null;
        return SetSizeCopy(size, GetCentroid());
    }
    /// <summary>
    /// Returns a copy of the polygon with a transform applied, using a given origin.
    /// </summary>
    /// <param name="transform">The transform to apply.</param>
    /// <param name="origin">The origin for rotation and scaling.</param>
    /// <returns>A new <see cref="Polygon"/> with the transform applied, or null if invalid.</returns>
    public new Polygon? SetTransformCopy(Transform2D transform, Vector2 origin)
    {
        if (Count < 3) return null;
        var newPolygon = SetPositionCopy(transform.Position);
        if (newPolygon == null) return null;
        newPolygon.SetRotation(transform.RotationRad, origin);
        newPolygon.SetSize(transform.ScaledSize.Length, origin);
        return newPolygon;
    }
    /// <summary>
    /// Returns a copy of the polygon with an offset transform applied, using a given origin.
    /// </summary>
    /// <param name="offset">The offset transform to apply.</param>
    /// <param name="origin">The origin for rotation and scaling.</param>
    /// <returns>A new <see cref="Polygon"/> with the offset applied, or null if invalid.</returns>
    public new Polygon? ApplyOffsetCopy(Transform2D offset, Vector2 origin)
    {
        if (Count < 3) return null;

        var newPolygon = ChangePositionCopy(offset.Position);
        if (newPolygon == null) return null;
        newPolygon.ChangeRotation(offset.RotationRad, origin);
        newPolygon.ChangeSize(offset.ScaledSize.Length, origin);
        return newPolygon;
    }

    #endregion
}