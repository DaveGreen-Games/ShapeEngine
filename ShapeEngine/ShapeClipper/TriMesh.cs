using System.Numerics;
using Clipper2Lib;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.Geometry.TriangulationDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.ShapeClipper;

/// <summary>
/// Represents a mutable triangle mesh stored as a flat vertex buffer.
/// </summary>
/// <remarks>
/// Every 3 consecutive vertices form one triangle. This type is optimized for pooled/buffer-oriented workflows,
/// while still providing helpers for copying, transforming, hashing, drawing, and conversion to <see cref="Triangulation"/>.
/// </remarks>
public sealed class TriMesh : IEquatable<TriMesh>
{
    #region Constants
    
    #endregion
    
    #region Properties

    /// <summary>
    /// Gets a read-only view of the triangle vertex buffer.
    /// </summary>
    /// <remarks>Every 3 consecutive vertices form one triangle.</remarks>
    private readonly List<Vector2> triangleVertices;
    
    /// <summary>
    /// Gets a read-only view of the mesh vertices.
    /// </summary>
    /// <remarks>Every 3 consecutive vertices represent one triangle in the mesh.</remarks>
    public IReadOnlyList<Vector2> TriangleVertices => triangleVertices;
    

    /// <summary>
    /// Gets the decimal precision used by this mesh for quantized hashing and equality.
    /// </summary>
    public int DecimalPlaces { get; }

    #endregion
    
    #region Constructors
    /// <summary>
    /// Initializes a new empty mesh using the current default clipper decimal precision.
    /// </summary>
    public TriMesh() : this(0, ClipperImmediate2D.DecimalPlaces) { }
    
    /// <summary>
    /// Initializes a new empty mesh with the specified initial vertex capacity.
    /// </summary>
    /// <param name="capacity">The initial number of vertices the internal buffer can store without resizing.</param>
    public TriMesh(int capacity) : this(capacity, ClipperImmediate2D.DecimalPlaces) { }

    /// <summary>
    /// Initializes a new empty mesh with the specified initial capacity and hash/equality precision.
    /// </summary>
    /// <param name="capacity">The initial number of vertices the internal buffer can store without resizing.</param>
    /// <param name="decimalPlaces">The decimal precision used for quantized hashing and equality comparisons.</param>
    public TriMesh(int capacity, int decimalPlaces)
    {
        triangleVertices = new List<Vector2>(capacity);
        DecimalPlaces = Math.Max(0, decimalPlaces);
    }
    #endregion
    
    #region Public Functions
    
    /// <summary>
    /// Removes all vertices and triangles from the mesh.
    /// </summary>
    public void Clear() => triangleVertices.Clear();

    /// <summary>
    /// Removes any trailing vertices that do not form a complete triangle.
    /// </summary>
    /// <remarks>
    /// After this call, <see cref="TriangleVertices"/> will contain a number of vertices that is a multiple of 3.
    /// Existing complete triangles are preserved in their original order.
    /// </remarks>
    public void Clean()
    {
        int validVertexCount = GetValidVertexCount();
        if (validVertexCount == triangleVertices.Count) return;

        triangleVertices.RemoveRange(validVertexCount, triangleVertices.Count - validVertexCount);
    }

    /// <summary>
    /// Gets the number of vertices that currently form complete triangles.
    /// </summary>
    /// <returns>The largest vertex count less than or equal to the current count that is divisible by 3.</returns>
    public int GetValidVertexCount()
    {
        return triangleVertices.Count - (triangleVertices.Count % 3);
    }
    
    /// <summary>
    /// Ensures the internal vertex buffer can hold at least the specified number of vertices without resizing.
    /// </summary>
    /// <param name="capacity">The minimum desired vertex capacity.</param>
    public void EnsureCapacity(int capacity)
    {
        triangleVertices.EnsureCapacity(capacity);
    }

    /// <summary>
    /// Adds a triangle defined by three vertices to the mesh.
    /// </summary>
    /// <param name="a">The first triangle vertex.</param>
    /// <param name="b">The second triangle vertex.</param>
    /// <param name="c">The third triangle vertex.</param>
    public void AddTriangle(Vector2 a, Vector2 b, Vector2 c)
    {
        triangleVertices.Add(a);
        triangleVertices.Add(b);
        triangleVertices.Add(c);
    }
   
    /// <summary>
    /// Adds the vertices of the specified triangle to the mesh.
    /// </summary>
    /// <param name="triangle">The triangle to append.</param>
    public void AddTriangle(Triangle triangle)
    {
        triangleVertices.Add(triangle.A);
        triangleVertices.Add(triangle.B);
        triangleVertices.Add(triangle.C);
    }

    /// <summary>
    /// Gets the triangle at the specified triangle index.
    /// </summary>
    /// <param name="index">The zero-based triangle index.</param>
    /// <returns>The triangle at the requested index, or a default triangle if the index is out of range.</returns>
    public Triangle GetTriangleAt(int index)
    {
        if (index < 0 || triangleVertices.Count < 3 || index * 3 >= triangleVertices.Count) return new Triangle();
        
        int startIndex = index * 3;
        return new Triangle(
            triangleVertices[startIndex],
            triangleVertices[startIndex + 1],
            triangleVertices[startIndex + 2]
        );
    }
    
    /// <summary>
    /// Draws all triangles in the mesh using the specified color.
    /// </summary>
    /// <param name="color">The color used to draw each triangle.</param>
    public void Draw(ColorRgba color)
    {
        var t = triangleVertices;
        int vertexCount = triangleVertices.Count;
        var rayColor = color.ToRayColor();
        for (int i = 0; i < vertexCount; i += 3)
        {
            Vector2 a = t[i];
            Vector2 b = t[i + 1];
            Vector2 c = t[i + 2];

            Raylib.DrawTriangle(a, b, c, rayColor);
        }
    }
    
    /// <summary>
    /// Triangulates the specified integer-coordinate paths and writes the resulting triangles into this mesh.
    /// </summary>
    /// <param name="subject">The paths to triangulate.</param>
    /// <param name="useDelaunay">Whether to apply Delaunay refinement when triangulating.</param>
    /// <returns><c>true</c> if triangulation succeeded and produced at least one triangle; otherwise, <c>false</c>.</returns>
    public bool TriangulatePaths64ToMesh(Paths64 subject, bool useDelaunay)
    {
        Clear();

        TriangulateResult res = Clipper.Triangulate(subject, out Paths64 tris, useDelaunay);
        if (res != TriangulateResult.success || tris.Count == 0) return false;

        FillMesh(tris);
        return true;
    }

    /// <summary>
    /// Converts this mesh into a triangulation.
    /// </summary>
    /// <param name="dst">The destination triangulation that will be cleared and filled with this mesh's triangles.</param>
    public void ToTriangulation(Triangulation dst)
    {
        if (dst == null) throw new ArgumentNullException(nameof(dst));

        dst.Clear();
        int vertexCount = triangleVertices.Count;
        for (int i = 0; i < vertexCount; i += 3)
        {
            Vector2 a = triangleVertices[i];
            Vector2 b = triangleVertices[i + 1];
            Vector2 c = triangleVertices[i + 2];
            Triangle t = new(a, b, c);
            dst.Add(t);
        }
    }

    /// <summary>
    /// Creates a copy of this mesh.
    /// </summary>
    /// <returns>A new mesh containing the same triangle vertices in the same order.</returns>
    public TriMesh Copy()
    {
        TriMesh result = new(triangleVertices.Count, DecimalPlaces);
        CopyTo(result);
        return result;
    }
    
    /// <summary>
    /// Copies this mesh into <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The destination mesh.</param>
    public void CopyTo(TriMesh result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));
        if (ReferenceEquals(result, this)) return;

        CopyTransformed(result, static p => p);
    }
    
    #endregion
    
    #region Hash
    /// <summary>
    /// Creates a stable 64-bit hash key for the current mesh by hashing the triangle vertex data in order.
    /// </summary>
    /// <param name="decimalPlaces">
    /// The number of decimal places used to quantize vertex coordinates before hashing.
    /// Pass a negative value to use this mesh's <see cref="DecimalPlaces"/>.
    /// </param>
    /// <returns>A 64-bit hash key suitable for cache keys and change detection.</returns>
    public ulong GetHashKey(int decimalPlaces = -1)
    {
        if (decimalPlaces < 0) decimalPlaces = DecimalPlaces;

        var triangles = triangleVertices;
        int trianglePointCount = triangles.Count;
        Fnv1aHashQuantizer hashQuantizer = new(decimalPlaces);
        ulong hash = hashQuantizer.StartHash(trianglePointCount);
        for (int i = 0; i < trianglePointCount; i++)
        {
            hash = hashQuantizer.Add(hash, triangles[i]);
        }

        return hash;
    }

    /// <summary>
    /// Creates a fixed-width hexadecimal string representation of the current mesh hash key.
    /// </summary>
    /// <param name="decimalPlaces">
    /// The number of decimal places used to quantize vertex coordinates before hashing.
    /// Pass a negative value to use this mesh's <see cref="DecimalPlaces"/>.
    /// </param>
    /// <returns>A 16-character uppercase hexadecimal hash key string.</returns>
    public string GetHashKeyHex(int decimalPlaces = -1) => GetHashKey(decimalPlaces).ToString("X16");

    /// <summary>
    /// Creates a string representation of the current mesh hash key.
    /// </summary>
    /// <param name="decimalPlaces">
    /// The number of decimal places used to quantize vertex coordinates before hashing.
    /// Pass a negative value to use this mesh's <see cref="DecimalPlaces"/>.
    /// </param>
    /// <returns>A stable hexadecimal hash key string.</returns>
    public string GetHashKeyString(int decimalPlaces = -1) => GetHashKeyHex(decimalPlaces);

    /// <summary>
    /// Returns a 32-bit hash code derived from the stable 64-bit mesh hash key.
    /// </summary>
    /// <returns>A 32-bit hash code for the current mesh.</returns>
    public override int GetHashCode()
    {
        ulong hashKey = GetHashKey();
        return unchecked((int)(hashKey ^ (hashKey >> 32)));
    }

    /// <summary>
    /// Returns a string that describes this mesh's triangle count, vertex count, and stable hash key.
    /// </summary>
    /// <returns>A string in the form <c>TriMesh[Triangles: ..., Vertices: ..., Hash: ...]</c>.</returns>
    public override string ToString()
    {
        int vertexCount = triangleVertices.Count;
        int triangleCount = vertexCount / 3;
        return $"TriMesh[Triangles: {triangleCount}, Vertices: {vertexCount}, Hash: {GetHashKeyHex()}]";
    }
    #endregion
    
    #region Operators
    /// <summary>
    /// Implicitly converts a <see cref="TriMesh"/> into a new <see cref="Triangulation"/>.
    /// </summary>
    /// <param name="mesh">The mesh to convert.</param>
    /// <returns>
    /// A new triangulation containing the mesh triangles in the same order,
    /// or <c>null</c> when <paramref name="mesh"/> is <c>null</c>.
    /// </returns>
    public static implicit operator Triangulation?(TriMesh? mesh)
    {
        if (mesh == null) return null;

        Triangulation triangulation = new(mesh.triangleVertices.Count / 3);
        mesh.ToTriangulation(triangulation);
        return triangulation;
    }
    
    /// <summary>
    /// Adds a vector to all vertices of a mesh.
    /// </summary>
    /// <param name="left">The mesh to translate.</param>
    /// <param name="right">The vector to add to each vertex.</param>
    /// <returns>A new mesh with all vertices translated by the specified vector.</returns>
    public static TriMesh operator +(TriMesh left, Vector2 right)
    {
        return left.OffsetCopy(right);
    }

    /// <summary>
    /// Subtracts a vector from all vertices of a mesh.
    /// </summary>
    /// <param name="left">The mesh to translate.</param>
    /// <param name="right">The vector to subtract from each vertex.</param>
    /// <returns>A new mesh with all vertices translated by the negative of the specified vector.</returns>
    public static TriMesh operator -(TriMesh left, Vector2 right)
    {
        return left.OffsetCopy(-right);
    }

    /// <summary>
    /// Adds a scalar value to all components of all vertices of a mesh.
    /// </summary>
    /// <param name="left">The mesh to translate.</param>
    /// <param name="right">The scalar value to add to each vertex component.</param>
    /// <returns>A new mesh with all vertex components increased by the specified value.</returns>
    public static TriMesh operator +(TriMesh left, float right)
    {
        return left.OffsetCopy(new Vector2(right));
    }

    /// <summary>
    /// Subtracts a scalar value from all components of all vertices of a mesh.
    /// </summary>
    /// <param name="left">The mesh to translate.</param>
    /// <param name="right">The scalar value to subtract from each vertex component.</param>
    /// <returns>A new mesh with all vertex components decreased by the specified value.</returns>
    public static TriMesh operator -(TriMesh left, float right)
    {
        return left.OffsetCopy(new Vector2(-right));
    }

    /// <summary>
    /// Multiplies all vertices of a mesh by a scalar value.
    /// </summary>
    /// <param name="left">The mesh to scale.</param>
    /// <param name="right">The scalar value to multiply each vertex by.</param>
    /// <returns>A new mesh with all vertices uniformly scaled by the specified factor.</returns>
    public static TriMesh operator *(TriMesh left, float right)
    {
        return left.ScaleCopy(right);
    }

    /// <summary>
    /// Multiplies all vertices of a mesh by a scalar value.
    /// </summary>
    /// <param name="left">The scalar value to multiply each vertex by.</param>
    /// <param name="right">The mesh to scale.</param>
    /// <returns>A new mesh with all vertices uniformly scaled by the specified factor.</returns>
    public static TriMesh operator *(float left, TriMesh right)
    {
        return right * left;
    }

    /// <summary>
    /// Divides all vertices of a mesh by a scalar value.
    /// </summary>
    /// <param name="left">The mesh to scale.</param>
    /// <param name="right">The scalar value to divide each vertex by.</param>
    /// <returns>A new mesh with all vertices uniformly scaled by the inverse of the specified factor.</returns>
    public static TriMesh operator /(TriMesh left, float right)
    {
        if (right == 0f) throw new DivideByZeroException("TriMesh scalar division requires a non-zero divisor.");
        return left.ScaleCopy(1f / right);
    }

    /// <summary>
    /// Multiplies all vertices of a mesh by a vector.
    /// </summary>
    /// <param name="left">The mesh to scale.</param>
    /// <param name="right">The vector to multiply each vertex by.</param>
    /// <returns>A new mesh with all vertices scaled by the specified vector components.</returns>
    public static TriMesh operator *(TriMesh left, Vector2 right)
    {
        return left.ScaleCopy(right);
    }

    /// <summary>
    /// Divides all vertices of a mesh by a vector.
    /// </summary>
    /// <param name="left">The mesh to scale.</param>
    /// <param name="right">The vector to divide each vertex by.</param>
    /// <returns>A new mesh with all vertices scaled by the inverse of the specified vector components.</returns>
    public static TriMesh operator /(TriMesh left, Vector2 right)
    {
        if (right.X == 0f || right.Y == 0f) throw new DivideByZeroException("TriMesh vector division requires non-zero X and Y divisors.");

        TriMesh result = new(left.triangleVertices.Count, left.DecimalPlaces);
        left.CopyTransformed(result, p => p / right);
        return result;
    }
    
    #endregion
    
    #region Transform
    /// <summary>
    /// Offsets all vertices of this mesh in place by the specified vector.
    /// </summary>
    /// <param name="offset">The translation applied to each vertex.</param>
    public void Offset(Vector2 offset)
    {
        var triangles = triangleVertices;
        int vertexCount = triangles.Count;
        for (int i = 0; i < vertexCount; i++)
        {
            triangles[i] += offset;
        }
    }

    /// <summary>
    /// Uniformly scales all vertices of this mesh in place by the specified factor.
    /// </summary>
    /// <param name="scale">The scale factor applied to each vertex.</param>
    public void Scale(float scale)
    {
        var triangles = triangleVertices;
        int vertexCount = triangles.Count;
        for (int i = 0; i < vertexCount; i++)
        {
            triangles[i] *= scale;
        }
    }

    /// <summary>
    /// Non-uniformly scales all vertices of this mesh in place by the specified vector.
    /// </summary>
    /// <param name="scale">The component-wise scale factor applied to each vertex.</param>
    public void Scale(Vector2 scale)
    {
        var triangles = triangleVertices;
        int vertexCount = triangles.Count;
        for (int i = 0; i < vertexCount; i++)
        {
            triangles[i] *= scale;
        }
    }

    /// <summary>
    /// Rotates all vertices of this mesh in place by the specified angle around the given pivot.
    /// </summary>
    /// <param name="amountRad">The rotation amount in radians.</param>
    /// <param name="pivot">The pivot point about which to rotate.</param>
    public void RotateBy(float amountRad, Vector2 pivot)
    {
        if (amountRad == 0f) return;

        var triangles = triangleVertices;
        int vertexCount = triangles.Count;
        for (int i = 0; i < vertexCount; i++)
        {
            triangles[i] = RotatePoint(triangles[i], amountRad, pivot);
        }
    }

    /// <summary>
    /// Sets the absolute rotation of this mesh around the given pivot.
    /// </summary>
    /// <param name="angleRad">The target rotation angle in radians.</param>
    /// <param name="pivot">The pivot point about which to rotate.</param>
    /// <remarks>
    /// The current orientation is determined by the first non-zero vertex direction from the pivot.
    /// If all vertices coincide with the pivot, the mesh is left unchanged.
    /// </remarks>
    public void RotateTo(float angleRad, Vector2 pivot)
    {
        if (!TryGetReferenceAngle(pivot, out float currentAngleRad)) return;

        float amountRad = ShapeMath.GetShortestAngleRad(currentAngleRad, angleRad);
        RotateBy(amountRad, pivot);
    }
    
    /// <summary>
    /// Copies this mesh into <paramref name="result"/>, offsetting each vertex by the specified vector.
    /// </summary>
    /// <param name="result">The destination mesh.</param>
    /// <param name="offset">The translation applied to each copied vertex.</param>
    public void OffsetCopy(TriMesh result, Vector2 offset)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));
        if (ReferenceEquals(result, this))
        {
            Offset(offset);
            return;
        }

        CopyTransformed(result, p => p + offset);
    }

    /// <summary>
    /// Creates a new mesh whose vertices are offset by the specified vector.
    /// </summary>
    /// <param name="offset">The translation applied to each copied vertex.</param>
    /// <returns>A new offset copy of this mesh.</returns>
    public TriMesh OffsetCopy(Vector2 offset)
    {
        TriMesh result = new(triangleVertices.Count, DecimalPlaces);
        OffsetCopy(result, offset);
        return result;
    }

    /// <summary>
    /// Copies this mesh into <paramref name="result"/>, scaling each vertex by the specified factor.
    /// </summary>
    /// <param name="result">The destination mesh.</param>
    /// <param name="scale">The scale factor applied to each copied vertex.</param>
    public void ScaleCopy(TriMesh result, float scale)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));
        if (ReferenceEquals(result, this))
        {
            Scale(scale);
            return;
        }

        CopyTransformed(result, p => p * scale);
    }

    /// <summary>
    /// Copies this mesh into <paramref name="result"/>, scaling each vertex by the specified vector.
    /// </summary>
    /// <param name="result">The destination mesh.</param>
    /// <param name="scale">The component-wise scale factor applied to each copied vertex.</param>
    public void ScaleCopy(TriMesh result, Vector2 scale)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));
        if (ReferenceEquals(result, this))
        {
            Scale(scale);
            return;
        }

        CopyTransformed(result, p => p * scale);
    }

    /// <summary>
    /// Copies this mesh into <paramref name="result"/>, rotating each vertex by the specified angle around the given pivot.
    /// </summary>
    /// <param name="result">The destination mesh.</param>
    /// <param name="amountRad">The rotation amount in radians.</param>
    /// <param name="pivot">The pivot point about which to rotate.</param>
    public void RotateByCopy(TriMesh result, float amountRad, Vector2 pivot)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));
        if (ReferenceEquals(result, this))
        {
            RotateBy(amountRad, pivot);
            return;
        }

        CopyTransformed(result, p => RotatePoint(p, amountRad, pivot));
    }

    /// <summary>
    /// Creates a new mesh whose vertices are rotated by the specified angle around the given pivot.
    /// </summary>
    /// <param name="amountRad">The rotation amount in radians.</param>
    /// <param name="pivot">The pivot point about which to rotate.</param>
    /// <returns>A new rotated copy of this mesh.</returns>
    public TriMesh RotateByCopy(float amountRad, Vector2 pivot)
    {
        TriMesh result = new(triangleVertices.Count, DecimalPlaces);
        RotateByCopy(result, amountRad, pivot);
        return result;
    }

    /// <summary>
    /// Copies this mesh into <paramref name="result"/>, setting its absolute rotation around the given pivot.
    /// </summary>
    /// <param name="result">The destination mesh.</param>
    /// <param name="angleRad">The target rotation angle in radians.</param>
    /// <param name="pivot">The pivot point about which to rotate.</param>
    public void RotateToCopy(TriMesh result, float angleRad, Vector2 pivot)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));
        if (ReferenceEquals(result, this))
        {
            RotateTo(angleRad, pivot);
            return;
        }

        if (!TryGetReferenceAngle(pivot, out float currentAngleRad))
        {
            CopyTo(result);
            return;
        }

        float amountRad = ShapeMath.GetShortestAngleRad(currentAngleRad, angleRad);
        RotateByCopy(result, amountRad, pivot);
    }

    /// <summary>
    /// Creates a new mesh whose vertices are rotated to the specified absolute angle around the given pivot.
    /// </summary>
    /// <param name="angleRad">The target rotation angle in radians.</param>
    /// <param name="pivot">The pivot point about which to rotate.</param>
    /// <returns>A new rotated copy of this mesh.</returns>
    public TriMesh RotateToCopy(float angleRad, Vector2 pivot)
    {
        TriMesh result = new(triangleVertices.Count, DecimalPlaces);
        RotateToCopy(result, angleRad, pivot);
        return result;
    }

    /// <summary>
    /// Creates a new mesh whose vertices are uniformly scaled by the specified factor.
    /// </summary>
    /// <param name="scale">The scale factor applied to each copied vertex.</param>
    /// <returns>A new scaled copy of this mesh.</returns>
    public TriMesh ScaleCopy(float scale)
    {
        TriMesh result = new(triangleVertices.Count, DecimalPlaces);
        ScaleCopy(result, scale);
        return result;
    }

    /// <summary>
    /// Creates a new mesh whose vertices are non-uniformly scaled by the specified vector.
    /// </summary>
    /// <param name="scale">The component-wise scale factor applied to each copied vertex.</param>
    /// <returns>A new scaled copy of this mesh.</returns>
    public TriMesh ScaleCopy(Vector2 scale)
    {
        TriMesh result = new(triangleVertices.Count, DecimalPlaces);
        ScaleCopy(result, scale);
        return result;
    }
    
    #endregion
    
    #region Equality Functions
    
    /// <summary>
    /// Determines whether this mesh is equal to another mesh.
    /// </summary>
    /// <param name="other">The mesh to compare with the current mesh.</param>
    /// <returns><c>true</c> if both meshes contain the same vertices in the same order within quantized precision; otherwise, <c>false</c>.</returns>
    /// <remarks>The comparison uses the coarser precision of the two meshes.</remarks>
    public bool Equals(TriMesh? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        var triangles = triangleVertices;
        var otherTriangles = other.triangleVertices;
        int trianglePointCount = triangles.Count;
        if (trianglePointCount != otherTriangles.Count) return false;

        DecimalQuantizer quantizer = new(Math.Max(DecimalPlaces, other.DecimalPlaces));
        for (int i = 0; i < trianglePointCount; i++)
        {
            var a = triangles[i];
            var b = otherTriangles[i];
            if (!quantizer.QuantizedEquals(a, b)) return false;
        }

        return true;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current mesh.
    /// </summary>
    /// <param name="obj">The object to compare with the current mesh.</param>
    /// <returns><c>true</c> if <paramref name="obj"/> is a <see cref="TriMesh"/> equal to this instance; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is TriMesh other && Equals(other);
    }

    /// <summary>
    /// Determines whether two meshes are equal.
    /// </summary>
    /// <param name="left">The first mesh to compare.</param>
    /// <param name="right">The second mesh to compare.</param>
    /// <returns><c>true</c> if both meshes are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(TriMesh? left, TriMesh? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two meshes are not equal.
    /// </summary>
    /// <param name="left">The first mesh to compare.</param>
    /// <param name="right">The second mesh to compare.</param>
    /// <returns><c>true</c> if the meshes are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(TriMesh? left, TriMesh? right)
    {
        return !(left == right);
    }

    #endregion
    
    #region Private Functions

    private void FillMesh(Paths64 tris)
    {
        // Each Path64 in tris is a triangle with 3 points (CCW).
        int triCount = tris.Count;
        var triangles = triangleVertices;
        triangles.Capacity = Math.Max(triangles.Capacity, triCount * 3);
        
        for (int i = 0; i < triCount; i++)
        {
            var tri = tris[i];
            if (tri.Count < 3) continue;
        
            // Convert int coords back to world coords
            // because y is never flipped - return triangles are in cw order and flipping 0 with 1 turns them into ccw order!
            Vector2 a = ClipperImmediate2D.ToVec2(tri[1]);
            Vector2 b = ClipperImmediate2D.ToVec2(tri[0]);
            Vector2 c = ClipperImmediate2D.ToVec2(tri[2]);
        
            // enforce CCW (defensive)
            if (Cross(b - a, c - a) > 0f)
            {
                var tmp = b; b = c; c = tmp;
            }
        
            triangles.Add(a);
            triangles.Add(b);
            triangles.Add(c);
        }
    }
    
    private static float Cross(in Vector2 a, in Vector2 b) => a.X * b.Y - a.Y * b.X;
    
    private void CopyTransformed(TriMesh result, Func<Vector2, Vector2> transformer)
    {
        var triangles = triangleVertices;
        int vertexCount = triangles.Count;
        result.Clear();
        result.triangleVertices.EnsureCapacity(vertexCount);
        for (int i = 0; i < vertexCount; i++)
        {
            result.triangleVertices.Add(transformer(triangles[i]));
        }
    }

    private bool TryGetReferenceAngle(Vector2 pivot, out float angleRad)
    {
        var triangles = triangleVertices;
        int vertexCount = triangles.Count;
        for (int i = 0; i < vertexCount; i++)
        {
            Vector2 dir = triangles[i] - pivot;
            if (dir.LengthSquared() <= 0f) continue;

            angleRad = dir.AngleRad();
            return true;
        }

        angleRad = 0f;
        return false;
    }

    private static Vector2 RotatePoint(Vector2 point, float amountRad, Vector2 pivot)
    {
        return pivot + (point - pivot).Rotate(amountRad);
    }
    
    #endregion
}