using System.Numerics;
using Raylib_CsLo;
using ShapeEngine.Lib;

namespace ShapeEngine.Screen;

public class ShapeShader
{
    public Shader Shader { get; private set; }
    public bool Enabled { get; set; }
    public uint ID { get; private set; }
    public int Order { get; set; }
    public bool Loaded { get; private set; }
    
    public ShapeShader(Shader shader, bool enabled = true, int order = 0)
    {
        this.Shader = shader;
        this.Enabled = enabled;
        this.ID = SID.NextID;
        this.Order = order;
        this.Loaded = true;
    }
    public ShapeShader(Shader shader, uint id, bool enabled = true, int order = 0)
    {
        this.Shader = shader;
        this.Enabled = enabled;
        this.ID = id;
        this.Order = order;
        this.Loaded = true;
    }
    public bool Unload()
    {
        if (!Loaded) return false;
        Loaded = false;
        UnloadShader(Shader);
        return true;
    }
    
    public static void SetValueFloat(Shader shader, string propertyName, float value)
    {
        int valueLocation = GetShaderLocation(shader, propertyName);
        SetShaderValue(shader, valueLocation, value, ShaderUniformDataType.SHADER_UNIFORM_FLOAT);
    }
    public static void SetValueVec(Shader shader, string propertyName, float[] values, ShaderUniformDataType dataType)
    {
        int valueLocation = GetShaderLocation(shader, propertyName);
        SetShaderValue(shader, valueLocation, values, dataType);
    }
    public static void SetValueVector2(Shader shader, string propertyName, float v1, float v2)
    {
        int valueLocation = GetShaderLocation(shader, propertyName);
        SetShaderValue(shader, valueLocation, new float[] { v1, v2 }, ShaderUniformDataType.SHADER_UNIFORM_VEC2);
    }
    public static void SetValueVector3(Shader shader, string propertyName, float v1, float v2, float v3)
    {
        int valueLocation = GetShaderLocation(shader, propertyName);
        SetShaderValue(shader, valueLocation, new float[] { v1, v2, v3 }, ShaderUniformDataType.SHADER_UNIFORM_VEC3);
    }
    public static void SetValueVector4(Shader shader, string propertyName, float v1, float v2, float v3, float v4)
    {
        int valueLocation = GetShaderLocation(shader, propertyName);
        SetShaderValue(shader, valueLocation, new float[] { v1, v2, v3, v4 }, ShaderUniformDataType.SHADER_UNIFORM_VEC4);
    }
    public static void SetValueVector2(Shader shader, string propertyName, Vector2 vec)
    {
        int valueLocation = GetShaderLocation(shader, propertyName);
        SetShaderValue(shader, valueLocation, new float[] { vec.X, vec.Y}, ShaderUniformDataType.SHADER_UNIFORM_VEC2);
    }
    public static void SetValueColor(Shader shader, string propertyName, Raylib_CsLo.Color color)
    {
        int valueLocation = GetShaderLocation(shader, propertyName);
        SetShaderValue(shader, valueLocation, new float[] {color.r / 255f, color.g / 255f, color.b / 255f, color.a / 255f}, ShaderUniformDataType.SHADER_UNIFORM_VEC4);
    }
}