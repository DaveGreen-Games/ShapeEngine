using System.Numerics;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core.GameDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Core;

/// <summary>
/// A class for efficient triangle drawing using Raylib's RLGL, supporting both persistent batches and immediate-mode drawing.
/// <para>
/// This class serves two main purposes:
/// </para>
/// <list type="bullet">
/// <item>
/// <b>Pooled Instances:</b> obtain an instance via <see cref="Get(ColorRgba, int)"/> to manage a persistent list of vertices.
/// This is ideal for static geometry that doesn't change often. The batch can be drawn repeatedly with <see cref="Draw"/>.
/// Remember to return it to the pool with <see cref="Return(TriangleBatch)"/> when finished.
/// </item>
/// <item>
/// <b>Immediate Mode (Static):</b> use static methods like <see cref="StartImmediateBatch(ColorRgba)"/>, <see cref="AddImmediateVertex(float, float)"/>,
/// and <see cref="EndImmediateBatch"/> for dynamic, per-frame triangle drawing without managing a specific object instance.
/// </item>
/// </list>
/// <para>
/// Both approaches utilize <see cref="DrawMode.Triangles"/>, meaning every 3 consecutive vertices form a single triangle.
/// </para>
/// </summary>
public class TriangleBatch
{
    #region Pooling Logic
    /// <summary>
    /// A stack used to pool <see cref="TriangleBatch"/> instances for reuse.
    /// </summary>
    private static readonly Stack<TriangleBatch> Pool = new();

    /// <summary>
    /// Retrieves a cleared, ready-to-use TriangleBatch from the pool or creates a new one.
    /// </summary>
    /// <param name="color">The color to be used for drawing the triangles in this batch.</param>
    /// <param name="initialCapacity">The initial capacity of the vertex list (number of triangles * 3).</param>
    /// <returns>A TriangleBatch instance ready for use.</returns>
    public static TriangleBatch Get(ColorRgba color, int initialCapacity = 64)
    {
        TriangleBatch batch;
        
        if (Pool.Count > 0)
        {
            batch = Pool.Pop();
            batch.color = color;
            
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
    /// <param name="batch">The batch to return to the pool.</param>
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
    /// <summary>
    /// The list of vertices currently in the batch.
    /// </summary>
    private readonly List<Vector2> vertices;
    /// <summary>
    /// The color to be used when drawing the batch.
    /// </summary>
    private ColorRgba color;
    
    /// <summary>
    /// Gets a value indicating whether this batch is currently in use.
    /// </summary>
    public bool InUse { get; private set; }
    #endregion
    
    #region Constructor
    /// <summary>
    /// Initializes a new instance of the <see cref="TriangleBatch"/> class.
    /// </summary>
    /// <param name="color">The initial color of the batch.</param>
    /// <param name="initialCapacity">The initial capacity of the vertex list (number of triangles).</param>
    private TriangleBatch(ColorRgba color, int initialCapacity)
    {
        this.color = color;
        this.vertices = new List<Vector2>(initialCapacity * 3);
        this.InUse = false;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Sets the color for the entire batch. This is mostly useful for changing color between frames or over time
    /// without needing to create a new batch, but it can also be used to change the color of the batch before drawing if needed.
    /// </summary>
    /// <param name="newColor">The new color to apply.</param>
    /// <remarks>
    /// Can only be called if the batch is in use.
    /// </remarks>
    public void SetColor(ColorRgba newColor)
    {
        if (!InUse)
        {
            Game.Instance.Logger.LogWarning("TriangleBatch color cannot be changed! The TriangleBatch is not in use. Call TriangleBatch.Get() to retrieve a batch from the pool before setting the color.");
            return;
        }
        this.color = newColor;
    }
    
    /// <summary>
    /// Adds a triangle to the batch.
    /// </summary>
    /// <param name="triangle">The triangle to add.</param>
    /// <remarks>
    /// Can only be called if the batch is in use.
    /// </remarks>
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

    /// <summary>
    /// Adds a triangle defined by three vertices to the batch.
    /// </summary>
    /// <param name="a">The first vertex of the triangle.</param>
    /// <param name="b">The second vertex of the triangle.</param>
    /// <param name="c">The third vertex of the triangle.</param>
    /// <remarks>
    /// Can only be called if the batch is in use.
    /// </remarks>
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

    /// <summary>
    /// Draws all triangles currently in the batch using the batch's color.
    /// </summary>
    /// <remarks>
    /// Uses <see cref="Rlgl"/> (Raylib OpenGL wrapper) to draw vertices in <see cref="DrawMode.Triangles"/> mode.
    /// Can only be called if the batch is in use.
    /// </remarks>
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

    /// <summary>
    /// Clears all vertices from the batch.
    /// </summary>
    public void Clear()
    {
        vertices.Clear();
    }
    
    /// <summary>
    /// Clears all vertices from the batch and sets a new color.
    /// </summary>
    /// <param name="newColor">The new color to use for the batch.</param>
    public void Clear(ColorRgba newColor)
    {
        this.color = newColor;
        vertices.Clear();
    }
    #endregion
    
    #endregion
    
    #region Immediate Mode Logic
    private static bool batchActive;
    private static ColorRgba currentColor;
    
    #region Start/End Batch Logic
    /// <summary>
    /// Starts a new triangle batch drawing operation.
    /// </summary>
    /// <param name="color">The color to be applied to subsequent vertices.</param>
    /// <remarks>
    /// Calls <see cref="Rlgl.Begin(DrawMode)"/> with <see cref="DrawMode.Triangles"/>.
    /// Must be paired with <see cref="EndImmediateBatch"/>.
    /// </remarks>
    public static void StartImmediateBatch(ColorRgba color)
    {
        if (batchActive)
        {
            Game.Instance.Logger.LogWarning("Cannot start triangle batch! A batch is already active. Call EndBatch() before starting a new batch.");
            return;
        }
        
        batchActive = true;
        
        currentColor = color;
        
        Rlgl.Begin(DrawMode.Triangles);
        Rlgl.Color4f(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f); 
    }

    /// <summary>
    /// Ends the current triangle batch drawing operation.
    /// </summary>
    /// <remarks>
    /// Calls <see cref="Rlgl.End"/>.
    /// </remarks>
    public static void EndImmediateBatch()
    {
        if (!batchActive)
        {
            Game.Instance.Logger.LogWarning("Cannot end triangle batch! No batch is currently active. Call StartBatch() before calling EndBatch().");
            return;
        }
        
        batchActive = false;
        
        Rlgl.End();
    }
    #endregion
    
    #region Set Color
    /// <summary>
    /// Sets the current drawing color in RLGL.
    /// This affects all subsequent vertices until the color is changed again or the batch ends.
    /// This method can be used to change colors between vertices within the same batch.
    /// </summary>
    /// <param name="color">The color to set.</param>
    /// <remarks>
    /// Batch must be active before using this function. Use <see cref="StartImmediateBatch(ColorRgba)"/> to start a batch before setting the color.
    /// </remarks>
    public static void SetImmediateColor(ColorRgba color)
    {
        if (!batchActive)
        {
            Game.Instance.Logger.LogWarning("Cannot set triangle batch color! No batch is currently active. Call StartBatch() before setting the color.");
            return;
        }
        
        currentColor = color;
        
        Rlgl.Color4f(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f); 
    }
    #endregion
    
    #region Add Vertices

    /// <summary>
    /// Adds a single vertex to the current batch using the current color.
    /// TriangleBatcher uses DrawMode.Triangles, so every 3 consecutive vertices added will form a triangle when <see cref="EndImmediateBatch"/> is called!
    /// </summary>
    /// <param name="x">The x-coordinate of the vertex.</param>
    /// <param name="y">The y-coordinate of the vertex.</param>
    /// <remarks>
    /// Batch must be active before using this function. Use <see cref="StartImmediateBatch(ColorRgba)"/> to start a batch.
    /// </remarks>
    public static void AddImmediateVertex(float x, float y)
    {
        if (!batchActive)
        {
            Game.Instance.Logger.LogWarning("Cannot add vertex to batch! No batch is currently active. Call StartBatch() before adding vertices.");
            return;
        }
        
        Rlgl.Vertex2f(x, y);
    }

    /// <summary>
    /// Sets the color and adds a single vertex to the current batch.
    /// TriangleBatcher uses DrawMode.Triangles, so every 3 consecutive vertices added will form a triangle when <see cref="EndImmediateBatch"/> is called!
    /// </summary>
    /// <param name="x">The x-coordinate of the vertex.</param>
    /// <param name="y">The y-coordinate of the vertex.</param>
    /// <param name="color">The color to apply for this vertex and subsequent vertices.</param>
    /// <remarks>
    /// Batch must be active before using this function. Use <see cref="StartImmediateBatch(ColorRgba)"/> to start a batch.
    /// Resets the color back to the previous color set by <see cref="SetImmediateColor"/> or <see cref="StartImmediateBatch"/> after adding the vertex, so this will not affect subsequent vertices.
    /// </remarks>
    public static void AddImmediateVertex(float x, float y, ColorRgba color)
    {
        if (!batchActive)
        {
            Game.Instance.Logger.LogWarning("Cannot add vertex to batch! No batch is currently active. Call StartBatch() before adding vertices.");
            return;
        }
        
        Rlgl.Color4f(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f); 
        
        Rlgl.Vertex2f(x, y);
        
        Rlgl.Color4f(currentColor.R / 255f, currentColor.G / 255f, currentColor.B / 255f, currentColor.A / 255f); 
    }
    
    /// <summary>
    /// Adds a single vertex to the current batch using the current color.
    /// TriangleBatcher uses DrawMode.Triangles, so every 3 consecutive vertices added will form a triangle when <see cref="EndImmediateBatch"/> is called!
    /// </summary>
    /// <param name="vertex">The position of the vertex.</param>
    /// <remarks>
    /// Batch must be active before using this function. Use <see cref="StartImmediateBatch(ColorRgba)"/> to start a batch.
    /// </remarks>
    public static void AddImmediateVertex(Vector2 vertex)
    {
        if (!batchActive)
        {
            Game.Instance.Logger.LogWarning("Cannot add vertex to batch! No batch is currently active. Call StartBatch() before adding vertices.");
            return;
        }
        
        Rlgl.Vertex2f(vertex.X, vertex.Y);
    }
    
    /// <summary>
    /// Sets the color and adds a single vertex to the current batch.
    /// The color used will affect all subsequent vertices until changed again or the batch ends.
    /// TriangleBatcher uses DrawMode.Triangles, so every 3 consecutive vertices added will form a triangle when <see cref="EndImmediateBatch"/> is called!
    /// </summary>
    /// <param name="vertex">The position of the vertex.</param>
    /// <param name="color">The color to apply for this vertex and subsequent vertices.</param>
    /// <remarks>
    /// Batch must be active before using this function. Use <see cref="StartImmediateBatch(ColorRgba)"/> to start a batch.
    /// Resets the color back to the previous color set by <see cref="SetImmediateColor"/> or <see cref="StartImmediateBatch"/> after adding the vertex, so this will not affect subsequent vertices.
    /// </remarks>
    public static void AddImmediateVertex(Vector2 vertex, ColorRgba color)
    {
        if (!batchActive)
        {
            Game.Instance.Logger.LogWarning("Cannot add vertex to batch! No batch is currently active. Call StartBatch() before adding vertices.");
            return;
        }
        
        Rlgl.Color4f(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f); 
        
        Rlgl.Vertex2f(vertex.X, vertex.Y);
        
        Rlgl.Color4f(currentColor.R / 255f, currentColor.G / 255f, currentColor.B / 255f, currentColor.A / 255f); 
    }

    /// <summary>
    /// Adds a collection of vertices to the current batch using the current color.
    /// TriangleBatcher uses DrawMode.Triangles, so every 3 consecutive vertices added will form a triangle when <see cref="EndImmediateBatch"/> is called!
    /// </summary>
    /// <param name="vertices">The collection of vertices to add.</param>
    /// <remarks>
    /// Batch must be active before using this function. Use <see cref="StartImmediateBatch(ColorRgba)"/> to start a batch.
    /// </remarks>
    public static void AddImmediateVertices(IEnumerable<Vector2> vertices)
    {
        if (!batchActive)
        {
            Game.Instance.Logger.LogWarning("Cannot add vertices to batch! No batch is currently active. Call StartBatch() before adding vertices.");
            return;
        }
        
        foreach (var vertex in vertices)
        {
            Rlgl.Vertex2f(vertex.X, vertex.Y);
        }
    }
    
    /// <summary>
    /// Sets the color and adds a collection of vertices to the current batch.
    /// The color used will affect all subsequent vertices until changed again or the batch ends.
    /// TriangleBatcher uses DrawMode.Triangles, so every 3 consecutive vertices added will form a triangle when <see cref="EndImmediateBatch"/> is called!
    /// </summary>
    /// <param name="vertices">The collection of vertices to add.</param>
    /// <param name="color">The color to apply for these vertices and subsequent vertices.</param>
    /// <remarks>
    /// Batch must be active before using this function. Use <see cref="StartImmediateBatch(ColorRgba)"/> to start a batch.
    /// Resets the color back to the previous color set by <see cref="SetImmediateColor"/> or <see cref="StartImmediateBatch"/> after adding the vertices, so this will not affect subsequent vertices.
    /// </remarks>
    public static void AddImmediateVertices(IEnumerable<Vector2> vertices, ColorRgba color)
    {
        if (!batchActive)
        {
            Game.Instance.Logger.LogWarning("Cannot add vertices to batch! No batch is currently active. Call StartBatch() before adding vertices.");
            return;
        }
        
        Rlgl.Color4f(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f); 
        
        foreach (var vertex in vertices)
        {
            Rlgl.Vertex2f(vertex.X, vertex.Y);
        }
        
        Rlgl.Color4f(currentColor.R / 255f, currentColor.G / 255f, currentColor.B / 255f, currentColor.A / 255f); 
    }
    #endregion
    
    #region Add Triangles
    /// <summary>
    /// Adds a triangle's vertices to the current batch.
    /// </summary>
    /// <param name="a">The first vertex.</param>
    /// <param name="b">The second vertex.</param>
    /// <param name="c">The third vertex.</param>
    /// <remarks>
    /// Batch must be active before using this function. Use <see cref="StartImmediateBatch(ColorRgba)"/> to start a batch before setting the color.
    /// </remarks>
    public static void AddImmediateTriangle(Vector2 a, Vector2 b, Vector2 c)
    {
        if (!batchActive)
        {
            Game.Instance.Logger.LogWarning("Cannot add triangle to batch! No batch is currently active. Call StartBatch() before adding triangles.");
            return;
        }
        
        Rlgl.Vertex2f(a.X, a.Y);
        Rlgl.Vertex2f(b.X, b.Y);
        Rlgl.Vertex2f(c.X, c.Y);
    }
    
    /// <summary>
    /// Sets the color and adds a triangle's vertices to the current batch.
    /// The color used will affect all subsequent vertices until changed again or the batch ends.
    /// </summary>
    /// <param name="a">The first vertex.</param>
    /// <param name="b">The second vertex.</param>
    /// <param name="c">The third vertex.</param>
    /// <param name="color">The color to use for this triangle.</param>
    /// <remarks>
    /// Batch must be active before using this function. Use <see cref="StartImmediateBatch(ColorRgba)"/> to start a batch before setting the color.
    /// Resets the color back to the previous color set by <see cref="SetImmediateColor"/> or <see cref="StartImmediateBatch"/> after adding the triangle vertices, so this will not affect subsequent vertices.
    /// </remarks>
    public static void AddImmediateTriangle(Vector2 a, Vector2 b, Vector2 c, ColorRgba color)
    {
        if (!batchActive)
        {
            Game.Instance.Logger.LogWarning("Cannot add triangle to batch! No batch is currently active. Call StartBatch() before adding triangles.");
            return;
        }
        
        Rlgl.Color4f(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f); 
        
        Rlgl.Vertex2f(a.X, a.Y);
        Rlgl.Vertex2f(b.X, b.Y);
        Rlgl.Vertex2f(c.X, c.Y);
        
        Rlgl.Color4f(currentColor.R / 255f, currentColor.G / 255f, currentColor.B / 255f, currentColor.A / 255f); 
    }
    
    /// <summary>
    /// Adds a triangle's vertices to the current batch.
    /// </summary>
    /// <param name="triangle">The triangle to add.</param>
    /// <remarks>
    /// Batch must be active before using this function. Use <see cref="StartImmediateBatch(ColorRgba)"/> to start a batch before setting the color.
    /// </remarks>
    public static void AddImmediateTriangle(Triangle triangle)
    {
        if (!batchActive)
        {
            Game.Instance.Logger.LogWarning("Cannot add triangle to batch! No batch is currently active. Call StartBatch() before adding triangles.");
            return;
        }
        
        Rlgl.Vertex2f(triangle.A.X, triangle.A.Y);
        Rlgl.Vertex2f(triangle.B.X, triangle.B.Y);
        Rlgl.Vertex2f(triangle.C.X, triangle.C.Y);
    }
    
    /// <summary>
    /// Sets the color and adds a triangle's vertices to the current batch.
    /// The color used will affect all subsequent vertices until changed again or the batch ends.
    /// </summary>
    /// <param name="triangle">The triangle to add.</param>
    /// <param name="color">The color to use for this triangle.</param>
    /// <remarks>
    /// Batch must be active before using this function. Use <see cref="StartImmediateBatch(ColorRgba)"/> to start a batch before setting the color.
    /// Resets the color back to the previous color set by <see cref="SetImmediateColor"/> or <see cref="StartImmediateBatch"/> after adding the triangle, so this will not affect subsequent vertices.
    /// </remarks>
    public static void AddImmediateTriangle(Triangle triangle, ColorRgba color)
    {
        if (!batchActive)
        {
            Game.Instance.Logger.LogWarning("Cannot add triangle to batch! No batch is currently active. Call StartBatch() before adding triangles.");
            return;
        }
        
        Rlgl.Color4f(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f); 
        
        Rlgl.Vertex2f(triangle.A.X, triangle.A.Y);
        Rlgl.Vertex2f(triangle.B.X, triangle.B.Y);
        Rlgl.Vertex2f(triangle.C.X, triangle.C.Y);
        
        Rlgl.Color4f(currentColor.R / 255f, currentColor.G / 255f, currentColor.B / 255f, currentColor.A / 255f); 
    }

    /// <summary>
    /// Adds a collection of triangles to the current batch.
    /// </summary>
    /// <param name="triangles">The collection of triangles to add.</param>
    /// <remarks>
    /// Batch must be active before using this function. Use <see cref="StartImmediateBatch(ColorRgba)"/> to start a batch before adding triangles.
    /// </remarks>
    public static void AddImmediateTriangles(IEnumerable<Triangle> triangles)
    {
        if (!batchActive)
        {
            Game.Instance.Logger.LogWarning("Cannot add triangles to batch! No batch is currently active. Call StartBatch() before adding triangles.");
            return;
        }
        
        foreach (var triangle in triangles)
        {
            Rlgl.Vertex2f(triangle.A.X, triangle.A.Y);
            Rlgl.Vertex2f(triangle.B.X, triangle.B.Y);
            Rlgl.Vertex2f(triangle.C.X, triangle.C.Y);
        }
    }
    
    /// <summary>
    /// Sets the color and adds a collection of triangles to the current batch.
    /// The color used will affect all subsequent vertices until changed again or the batch ends.
    /// </summary>
    /// <param name="triangles">The collection of triangles to add.</param>
    /// <param name="color">The color to use for these triangles.</param>
    /// <remarks>
    /// Batch must be active before using this function. Use <see cref="StartImmediateBatch(ColorRgba)"/> to start a batch before adding triangles.
    /// Resets the color back to the previous color set by <see cref="SetImmediateColor"/> or <see cref="StartImmediateBatch"/> after adding the triangles, so this will not affect subsequent vertices.
    /// </remarks>
    public static void AddImmediateTriangles(IEnumerable<Triangle> triangles, ColorRgba color)
    {
        if (!batchActive)
        {
            Game.Instance.Logger.LogWarning("Cannot add triangles to batch! No batch is currently active. Call StartBatch() before adding triangles.");
            return;
        }
        
        Rlgl.Color4f(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f); 
        
        foreach (var triangle in triangles)
        {
            Rlgl.Vertex2f(triangle.A.X, triangle.A.Y);
            Rlgl.Vertex2f(triangle.B.X, triangle.B.Y);
            Rlgl.Vertex2f(triangle.C.X, triangle.C.Y);
        }
        
        Rlgl.Color4f(currentColor.R / 255f, currentColor.G / 255f, currentColor.B / 255f, currentColor.A / 255f); 
    }
    #endregion

    #region Immediate Mode Drawing
    /// <summary>
    /// Draws a single triangle immediately.
    /// </summary>
    /// <param name="triangle">The triangle to draw.</param>
    /// <param name="color">The color of the triangle.</param>
    /// <remarks>
    /// Batch must NOT be active when calling this function. This is for immediate-mode style drawing of individual triangles.
    /// Use <see cref="EndImmediateBatch"/> to end any active batch before calling this function.
    /// </remarks>
    public static void DrawImmediateTriangle(Triangle triangle, ColorRgba color)
    {
        if (batchActive)
        {
            Game.Instance.Logger.LogWarning("Cannot draw triangle immediately! A batch is currently active. Call EndBatch() before drawing individual triangles.");
            return;
        }
        
        Rlgl.Begin(DrawMode.Triangles);
        
        Rlgl.Color4f(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f); 
        
        Rlgl.Vertex2f(triangle.A.X, triangle.A.Y);
        Rlgl.Vertex2f(triangle.B.X, triangle.B.Y);
        Rlgl.Vertex2f(triangle.C.X, triangle.C.Y);
        
        Rlgl.End();
    }
    
    /// <summary>
    /// Draws a single triangle defined by three vertices immediately.
    /// </summary>
    /// <param name="a">The first vertex.</param>
    /// <param name="b">The second vertex.</param>
    /// <param name="c">The third vertex.</param>
    /// <param name="color">The color of the triangle.</param>
    /// <remarks>
    /// Batch must NOT be active when calling this function. This is for immediate-mode style drawing of individual triangles.
    /// Use <see cref="EndImmediateBatch"/> to end any active batch before calling this function.
    /// </remarks>
    public static void DrawImmediateTriangle(Vector2 a, Vector2 b, Vector2 c, ColorRgba color)
    {
        if (batchActive)
        {
            Game.Instance.Logger.LogWarning("Cannot draw triangle immediately! A batch is currently active. Call EndBatch() before drawing individual triangles.");
            return;
        }
        
        Rlgl.Begin(DrawMode.Triangles);
        
        Rlgl.Color4f(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f); 
        
        Rlgl.Vertex2f(a.X, a.Y);
        Rlgl.Vertex2f(b.X, b.Y);
        Rlgl.Vertex2f(c.X, c.Y);
        
        Rlgl.End();
    }

    /// <summary>
    /// Draws a collection of triangles immediately.
    /// </summary>
    /// <param name="triangles">The collection of triangles to draw.</param>
    /// <param name="color">The color to use for all triangles.</param>
    /// <remarks>
    /// Batch must NOT be active when calling this function. This is for immediate-mode style drawing of individual triangles.
    /// Use <see cref="EndImmediateBatch"/> to end any active batch before calling this function.
    /// </remarks>
    public static void DrawImmediateTriangles(IEnumerable<Triangle> triangles, ColorRgba color)
    {
        if (batchActive)
        {
            Game.Instance.Logger.LogWarning("Cannot draw triangles immediately! A batch is currently active. Call EndBatch() before drawing individual triangles.");
            return;
        }
        
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
    
    /// <summary>
    /// Draws a series of triangles defined by a flat array of vertices immediately.
    /// </summary>
    /// <param name="trianglePoints">An array of vertices where every 3 vertices define a triangle.</param>
    /// <param name="color">The color to use for all triangles.</param>
    /// <remarks>
    /// Batch must NOT be active when calling this function. This is for immediate-mode style drawing of individual triangles.
    /// Use <see cref="EndImmediateBatch"/> to end any active batch before calling this function.
    /// </remarks>
    public static void DrawImmediateTriangles(Vector2[] trianglePoints, ColorRgba color)
    {
        if (batchActive)
        {
            Game.Instance.Logger.LogWarning("Cannot draw triangles immediately! A batch is currently active. Call EndBatch() before drawing individual triangles.");
            return;
        }

        if (trianglePoints.Length <= 0) return;
        
        Rlgl.Begin(DrawMode.Triangles);
        
        Rlgl.Color4f(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
        
        foreach (var v in trianglePoints)
        {
            Rlgl.Vertex2f(v.X, v.Y);
        }

        Rlgl.End();
    }
    #endregion
    
    #endregion
}