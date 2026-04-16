namespace ShapeEngine.ShapeClipper;

/// <summary>
/// Provides a simple reusable pool of <see cref="TriMesh"/> instances to reduce allocations during repeated triangulation work.
/// </summary>
/// <remarks>
/// Rented meshes are cleared before being returned to the caller, and returned meshes are cleared again before being stored in the pool.
/// This type is intended for manual reuse patterns and does not perform thread synchronization.
/// </remarks>
public class TriMeshPool
{
    private readonly Stack<TriMesh> stack;
    
    /// <summary>
    /// Initializes a new <see cref="TriMeshPool"/> with an optional number of preallocated meshes.
    /// </summary>
    /// <param name="startCapacity">The number of empty <see cref="TriMesh"/> instances to create and store in the pool initially. Values less than zero are treated as zero.</param>
    public TriMeshPool(int startCapacity)
    {
        if (startCapacity <= 0) startCapacity = 0;
        stack = new Stack<TriMesh>(startCapacity);
        for (int i = 0; i < startCapacity; i++)
        {
            stack.Push(new TriMesh());
        }
    }
    
    /// <summary>
    /// Retrieves a cleared <see cref="TriMesh"/> from the pool, or creates a new one if the pool is empty.
    /// </summary>
    /// <returns>A cleared <see cref="TriMesh"/> ready for use.</returns>
    public TriMesh RentMesh()
    {
        if (stack.Count > 0)
        {
            var m = stack.Pop();
            m.Clear();
            return m;
        }
        return new TriMesh();
    }

    /// <summary>
    /// Clears the specified <see cref="TriMesh"/> and returns it to the pool for reuse.
    /// </summary>
    /// <param name="mesh">The mesh to return to the pool.</param>
    /// <remarks>
    /// After calling this method, the caller should no longer use <paramref name="mesh"/> unless it is rented again later.
    /// </remarks>
    public void ReturnMesh(TriMesh mesh)
    {
        mesh.Clear();
        stack.Push(mesh);
    }
    
}