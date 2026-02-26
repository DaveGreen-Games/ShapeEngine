using System.Numerics;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core.GameDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Core;

public class TriangleBatch
{
    #region Pooling Logic
    private static readonly Stack<TriangleBatch> Pool = new();

    /// <summary>
    /// Retrieves a cleared, ready-to-use TriangleBatch from the pool or creates a new one.
    /// </summary>
    public static TriangleBatch Get(ColorRgba color, int initialCapacity = 256)
    {
        TriangleBatch batch;
        if (Pool.Count > 0)
        {
            batch = Pool.Pop();
            batch.color = color;
            // batch.vertices.Clear(); // Ensure it's empty
            
            // Optional: Ensure capacity if specifically requested, 
            // though usually we just let the list grow and keep the large buffer.
            if (batch.vertices.Capacity < initialCapacity * 3)
            {
                batch.vertices.Capacity = initialCapacity * 3;
            }
        }
        else
        {
            batch = new TriangleBatch(color, initialCapacity);
        }

        batch.InUse = true;
        return batch;
    }

    /// <summary>
    /// Returns the batch to the pool for reuse. 
    /// DO NOT use the batch reference after calling this.
    /// </summary>
    public static void Return(TriangleBatch batch)
    {
        if (!batch.InUse)
        {
            Game.Instance.Logger.LogWarning("Returning TriangleBatch to the pool is not possible! The TriangleBatch is already in the pool.");
            return;
        }

        batch.InUse = false;
        batch.Clear(); // Clear data before storing
        Pool.Push(batch);
    }

    #endregion
    
    #region Instance Logic
    
    #region Members
    private readonly List<Vector2> vertices;
    private ColorRgba color;
    public bool InUse { get; private set; }
    #endregion
    
    #region Constructor
    private TriangleBatch(ColorRgba color, int initialCapacity)
    {
        this.color = color;
        this.vertices = new List<Vector2>(initialCapacity * 3);
        this.InUse = false;
    }
    #endregion

    #region Methods
    public void SetColor(ColorRgba newColor)
    {
        if (!InUse)
        {
            Game.Instance.Logger.LogWarning("TriangleBatch color cannot be changed! The TriangleBatch is not in use. Call TriangleBatch.Get() to retrieve a batch from the pool before setting the color.");
            return;
        }
        this.color = newColor;
    }

    public void AddTriangle(Triangle triangle)
    {
        if (!InUse)
        {
            Game.Instance.Logger.LogWarning("TriangleBatch cannot add triangle! The TriangleBatch is not in use. Call TriangleBatch.Get() to retrieve a batch from the pool before adding triangles.");
            return;
        }
        vertices.Add(triangle.A);
        vertices.Add(triangle.B);
        vertices.Add(triangle.C);
    }

    public void AddTriangle(Vector2 a, Vector2 b, Vector2 c)
    {
        if (!InUse)
        {
            Game.Instance.Logger.LogWarning("TriangleBatch cannot add triangle! The TriangleBatch is not in use. Call TriangleBatch.Get() to retrieve a batch from the pool before adding triangles.");
            return;
        }
        vertices.Add(a);
        vertices.Add(b);
        vertices.Add(c);
    }

    public void Draw()
    {
        if (!InUse)
        {
            Game.Instance.Logger.LogWarning("TriangleBatch cannot be drawn! The TriangleBatch is not in use. Call TriangleBatch.Get() to retrieve a batch from the pool before drawing.");
            return;
        }
        
        if (vertices.Count == 0) return;

        Rlgl.Begin(DrawMode.Triangles);
        Rlgl.Color4f(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);

        var count = vertices.Count;
        for (int i = 0; i < count; i++)
        {
            var v = vertices[i];
            Rlgl.Vertex2f(v.X, v.Y);
        }

        Rlgl.End();
    }

    public void Clear()
    {
        vertices.Clear();
    }
    #endregion
    
    #endregion
}

public static class TriangleBatcher
{
    public static void StartBatch(ColorRgba color)
    {
        Rlgl.Begin(DrawMode.Triangles);
        SetColor(color);
    }

    public static void EndBatch()
    {
        Rlgl.End();
    }

    public static void SetColor(ColorRgba color)
    {
        Rlgl.Color4f(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f); 
    }
    
    public static void AddTriangle(Vector2 a, Vector2 b, Vector2 c)
    {
        Rlgl.Vertex2f(a.X, a.Y);
        Rlgl.Vertex2f(b.X, b.Y);
        Rlgl.Vertex2f(c.X, c.Y);
    }
    public static void AddTriangle(Vector2 a, Vector2 b, Vector2 c, ColorRgba color)
    {
        SetColor(color);
        Rlgl.Vertex2f(a.X, a.Y);
        Rlgl.Vertex2f(b.X, b.Y);
        Rlgl.Vertex2f(c.X, c.Y);
    }
    
    public static void AddTriangle(Triangle triangle)
    {
        Rlgl.Vertex2f(triangle.A.X, triangle.A.Y);
        Rlgl.Vertex2f(triangle.B.X, triangle.B.Y);
        Rlgl.Vertex2f(triangle.C.X, triangle.C.Y);
    }
    public static void AddTriangle(Triangle triangle, ColorRgba color)
    {
        SetColor(color);
        Rlgl.Vertex2f(triangle.A.X, triangle.A.Y);
        Rlgl.Vertex2f(triangle.B.X, triangle.B.Y);
        Rlgl.Vertex2f(triangle.C.X, triangle.C.Y);
    }

    public static void DrawTriangle(Triangle triangle, ColorRgba color)
    {
        Rlgl.Begin(DrawMode.Triangles);
        Rlgl.Color4f(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f); 
        Rlgl.Vertex2f(triangle.A.X, triangle.A.Y);
        Rlgl.Vertex2f(triangle.B.X, triangle.B.Y);
        Rlgl.Vertex2f(triangle.C.X, triangle.C.Y);
        Rlgl.End();
    }
    public static void DrawTriangle(Vector2 a, Vector2 b, Vector2 c, ColorRgba color)
    {
        Rlgl.Begin(DrawMode.Triangles);
        Rlgl.Color4f(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f); 
        Rlgl.Vertex2f(a.X, a.Y);
        Rlgl.Vertex2f(b.X, b.Y);
        Rlgl.Vertex2f(c.X, c.Y);
        Rlgl.End();
    }

    public static void DrawTriangles(IEnumerable<Triangle> triangles, ColorRgba color)
    {
        Rlgl.Begin(DrawMode.Triangles);
        Rlgl.Color4f(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
        
        foreach (var triangle in triangles)
        {
            Rlgl.Vertex2f(triangle.A.X, triangle.A.Y);
            Rlgl.Vertex2f(triangle.B.X, triangle.B.Y);
            Rlgl.Vertex2f(triangle.C.X, triangle.C.Y);
        }

        Rlgl.End();
    }
    public static void DrawTriangles(Vector2[] trianglePoints, ColorRgba color)
    {
        Rlgl.Begin(DrawMode.Triangles);
        Rlgl.Color4f(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
        
        foreach (var v in trianglePoints)
        {
            Rlgl.Vertex2f(v.X, v.Y);
        }

        Rlgl.End();
    }
}