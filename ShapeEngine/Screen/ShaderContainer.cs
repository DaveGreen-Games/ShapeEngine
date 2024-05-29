namespace ShapeEngine.Screen;

public class ShaderContainer
{
    private Dictionary<uint, ShapeShader> shaders = new();
    
    public uint Add(ShapeShader shader)
    {
        if (shaders.ContainsKey(shader.ID)) shaders[shader.ID] = shader;
        else shaders.Add(shader.ID, shader);
        return shader.ID;
    }

    public ShapeShader? Get(uint id) => HasShader(id) ? shaders[id] : null;
    public bool Remove(uint id) => shaders.Remove(id);
    public bool Remove(ShapeShader shader) => shaders.Remove(shader.ID);
    public bool HasShaders() => shaders.Count > 0;
    public bool HasShader(uint id) => shaders.ContainsKey(id);
    public bool HasShader(ShapeShader shader) => shaders.ContainsKey(shader.ID);

    public void Close()
    {
        foreach (var shader in shaders.Values)
        {
            shader.Unload();
        }
    }
    
    public List<ShapeShader> GetActiveShaders()
    {
        var shadersToApply = shaders.Values.ToList().FindAll(s => s.Enabled);
        shadersToApply.Sort(delegate (ShapeShader a, ShapeShader b)
        {
            if (a.Order < b.Order) return -1;
            else if (a.Order > b.Order) return 1;
            else return 0;
        });
        return shadersToApply;
    }

    public List<ShapeShader> GetAllShaders() => shaders.Values.ToList();
    public List<uint> GetAllIDs() => shaders.Keys.ToList();
}