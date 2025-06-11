namespace ShapeEngine.Screen;

/// <summary>
/// Container for managing a collection of <see cref="ShapeShader"/> objects.
/// </summary>
/// <remarks>
/// Provides methods to add, remove, retrieve, and query shaders, as well as to get all active shaders in sorted order.
/// </remarks>
public class ShaderContainer
{
    private readonly Dictionary<uint, ShapeShader> shaders = new();

    /// <summary>
    /// Determines if there are any enabled shaders in the container.
    /// </summary>
    /// <returns>True if at least one shader is enabled; otherwise, false.</returns>
    public bool HasActiveShaders()
    {
        if (shaders.Count <= 0) return false;
        
        foreach (var shader in shaders.Values)
        {
            if (shader.Enabled) return true;
        }

        return false;
    }

    /// <summary>
    /// Adds a shader to the container or updates an existing one with the same ID.
    /// </summary>
    /// <param name="shader">The shader to add or update.</param>
    /// <returns>The ID of the added or updated shader.</returns>
    public uint Add(ShapeShader shader)
    {
        shaders[shader.ID] = shader;
        return shader.ID;
    }

    /// <summary>
    /// Retrieves a shader by its ID.
    /// </summary>
    /// <param name="id">The ID of the shader to retrieve.</param>
    /// <returns>The shader with the specified ID if found; otherwise, null.</returns>
    public ShapeShader? Get(uint id) => HasShader(id) ? shaders[id] : null;

    /// <summary>
    /// Removes a shader from the container by its ID.
    /// </summary>
    /// <param name="id">The ID of the shader to remove.</param>
    /// <returns>True if the shader was successfully removed; otherwise, false.</returns>
    public bool Remove(uint id) => shaders.Remove(id);

    /// <summary>
    /// Removes a shader from the container.
    /// </summary>
    /// <param name="shader">The shader to remove.</param>
    /// <returns>True if the shader was successfully removed; otherwise, false.</returns>
    public bool Remove(ShapeShader shader) => shaders.Remove(shader.ID);

    /// <summary>
    /// Determines if the container has any shaders.
    /// </summary>
    /// <returns>True if the container has at least one shader; otherwise, false.</returns>
    public bool HasShaders() => shaders.Count > 0;

    /// <summary>
    /// Determines if the container has a shader with the specified ID.
    /// </summary>
    /// <param name="id">The ID to check.</param>
    /// <returns>True if a shader with the specified ID exists; otherwise, false.</returns>
    public bool HasShader(uint id) => shaders.ContainsKey(id);

    /// <summary>
    /// Determines if the container has the specified shader.
    /// </summary>
    /// <param name="shader">The shader to check.</param>
    /// <returns>True if the shader exists in the container; otherwise, false.</returns>
    public bool HasShader(ShapeShader shader) => shaders.ContainsKey(shader.ID);
    
    /// <summary>
    /// Gets all enabled shaders sorted in ascending (lowest first) order by their <see cref="ShapeShader.Order"/> property.
    /// </summary>
    /// <returns>A sorted list of all enabled shaders.</returns>
    public List<ShapeShader> GetActiveShaders()
    {
        var shadersToApply = shaders.Values.ToList().FindAll(s => s.Enabled);
        shadersToApply.Sort(delegate (ShapeShader a, ShapeShader b)
        {
            if (a.Order < b.Order) return -1;
            if (a.Order > b.Order) return 1;
            return 0;
        });
        return shadersToApply;
    }

    /// <summary>
    /// Gets all shaders in the container.
    /// </summary>
    /// <returns>A list of all shaders.</returns>
    public List<ShapeShader> GetAllShaders() => shaders.Values.ToList();

    /// <summary>
    /// Gets all shader IDs in the container.
    /// </summary>
    /// <returns>A list of all shader IDs.</returns>
    public List<uint> GetAllIDs() => shaders.Keys.ToList();
}