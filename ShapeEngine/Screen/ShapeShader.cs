using System.Numerics;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.StaticLib;

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
        this.ID = ShapeID.NextID;
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
        Raylib.UnloadShader(Shader);
        return true;
    }
    
    public static void SetValueFloat(Shader shader, string propertyName, float value)
    {
        int valueLocation = Raylib.GetShaderLocation(shader, propertyName);
        Raylib.SetShaderValue(shader, valueLocation, value, ShaderUniformDataType.Float);
    }
    public static void SetValueVec(Shader shader, string propertyName, float[] values, ShaderUniformDataType dataType)
    {
        int valueLocation = Raylib.GetShaderLocation(shader, propertyName);
        Raylib.SetShaderValue(shader, valueLocation, values, dataType);
    }
    public static void SetValueVector2(Shader shader, string propertyName, float v1, float v2)
    {
        int valueLocation = Raylib.GetShaderLocation(shader, propertyName);
        Raylib.SetShaderValue(shader, valueLocation, new float[] { v1, v2 }, ShaderUniformDataType.Vec2);
    }
    public static void SetValueVector3(Shader shader, string propertyName, float v1, float v2, float v3)
    {
        int valueLocation = Raylib.GetShaderLocation(shader, propertyName);
        Raylib.SetShaderValue(shader, valueLocation, new float[] { v1, v2, v3 }, ShaderUniformDataType.Vec3);
    }
    public static void SetValueVector4(Shader shader, string propertyName, float v1, float v2, float v3, float v4)
    {
        int valueLocation = Raylib.GetShaderLocation(shader, propertyName);
        Raylib.SetShaderValue(shader, valueLocation, new float[] { v1, v2, v3, v4 }, ShaderUniformDataType.Vec4);
    }
    public static void SetValueVector2(Shader shader, string propertyName, Vector2 vec)
    {
        int valueLocation = Raylib.GetShaderLocation(shader, propertyName);
        Raylib.SetShaderValue(shader, valueLocation, new float[] { vec.X, vec.Y}, ShaderUniformDataType.Vec2);
    }
    public static void SetValueColor(Shader shader, string propertyName, ColorRgba colorRgba)
    {
        int valueLocation = Raylib.GetShaderLocation(shader, propertyName);
        Raylib.SetShaderValue(shader, valueLocation, new float[] {colorRgba.R / 255f, colorRgba.G / 255f, colorRgba.B / 255f, colorRgba.A / 255f}, ShaderUniformDataType.Vec4);
    }
}