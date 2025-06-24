using System.Numerics;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Screen;

/// <summary>
/// Represents a shader used for rendering shapes, with support for enabling/disabling, ordering, and value setting.
/// </summary>
/// <remarks>
/// This class wraps a Raylib <see cref="Shader"/> and provides utility methods for setting shader values and managing shader state.
/// </remarks>
public class ShapeShader
{
    /// <summary>
    /// Gets the underlying Raylib shader.
    /// </summary>
    public Shader Shader { get; private set; }
    /// <summary>
    /// Gets or sets whether the shader is enabled for rendering.
    /// </summary>
    public bool Enabled { get; set; }
    /// <summary>
    /// Gets the unique identifier for this shader instance.
    /// </summary>
    public uint ID { get; private set; }
    /// <summary>
    /// Gets or sets the order in which this shader is applied relative to others.
    /// Lower values are applied first; higher values are applied later.
    /// </summary>
    public int Order { get; set; }
    /// <summary>
    /// Gets whether the shader is currently loaded.
    /// </summary>
    public bool Loaded { get; private set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ShapeShader"/> class with a new unique ID.
    /// </summary>
    /// <param name="shader">The Raylib shader to wrap.</param>
    /// <param name="enabled">Whether the shader is enabled.</param>
    /// <param name="order">
    /// The order in which the shader is applied.
    /// Lower values are applied first; higher values are applied later.
    /// </param>
    public ShapeShader(Shader shader, bool enabled = true, int order = 0)
    {
        this.Shader = shader;
        this.Enabled = enabled;
        this.ID = ShapeID.NextID;
        this.Order = order;
        this.Loaded = true;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="ShapeShader"/> class with a specified ID.
    /// </summary>
    /// <param name="shader">The Raylib shader to wrap.</param>
    /// <param name="id">The unique ID to assign to this shader.</param>
    /// <param name="enabled">Whether the shader is enabled.</param>
    /// <param name="order">
    /// The order in which the shader is applied.
    /// Lower values are applied first; higher values are applied later.
    /// </param>
    public ShapeShader(Shader shader, uint id, bool enabled = true, int order = 0)
    {
        this.Shader = shader;
        this.Enabled = enabled;
        this.ID = id;
        this.Order = order;
        this.Loaded = true;
    }
    /// <summary>
    /// Unloads the shader from memory.
    /// </summary>
    /// <returns>True if the shader was unloaded; otherwise, false.</returns>
    public bool Unload()
    {
        if (!Loaded) return false;
        Loaded = false;
        Raylib.UnloadShader(Shader);
        return true;
    }
    
    /// <summary>
    /// Sets a float value for a shader property.
    /// </summary>
    /// <param name="shader">The shader to set the value on.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="value">The float value to set.</param>
    public static void SetValueFloat(Shader shader, string propertyName, float value)
    {
        int valueLocation = Raylib.GetShaderLocation(shader, propertyName);
        Raylib.SetShaderValue(shader, valueLocation, value, ShaderUniformDataType.Float);
    }
    /// <summary>
    /// Sets a vector value for a shader property.
    /// </summary>
    /// <param name="shader">The shader to set the value on.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="values">The float array representing the vector value.</param>
    /// <param name="dataType">The data type of the vector.</param>
    public static void SetValueVec(Shader shader, string propertyName, float[] values, ShaderUniformDataType dataType)
    {
        int valueLocation = Raylib.GetShaderLocation(shader, propertyName);
        Raylib.SetShaderValue(shader, valueLocation, values, dataType);
    }
    /// <summary>
    /// Sets a Vector2 value for a shader property using two floats.
    /// </summary>
    /// <param name="shader">The shader to set the value on.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="v1">The X component.</param>
    /// <param name="v2">The Y component.</param>
    public static void SetValueVector2(Shader shader, string propertyName, float v1, float v2)
    {
        int valueLocation = Raylib.GetShaderLocation(shader, propertyName);
        Raylib.SetShaderValue(shader, valueLocation, new[] { v1, v2 }, ShaderUniformDataType.Vec2);
    }
    /// <summary>
    /// Sets a Vector3 value for a shader property using three floats.
    /// </summary>
    /// <param name="shader">The shader to set the value on.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="v1">The X component.</param>
    /// <param name="v2">The Y component.</param>
    /// <param name="v3">The Z component.</param>
    public static void SetValueVector3(Shader shader, string propertyName, float v1, float v2, float v3)
    {
        int valueLocation = Raylib.GetShaderLocation(shader, propertyName);
        Raylib.SetShaderValue(shader, valueLocation, new[] { v1, v2, v3 }, ShaderUniformDataType.Vec3);
    }
    /// <summary>
    /// Sets a Vector4 value for a shader property using four floats.
    /// </summary>
    /// <param name="shader">The shader to set the value on.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="v1">The X component.</param>
    /// <param name="v2">The Y component.</param>
    /// <param name="v3">The Z component.</param>
    /// <param name="v4">The W component.</param>
    public static void SetValueVector4(Shader shader, string propertyName, float v1, float v2, float v3, float v4)
    {
        int valueLocation = Raylib.GetShaderLocation(shader, propertyName);
        Raylib.SetShaderValue(shader, valueLocation, new[] { v1, v2, v3, v4 }, ShaderUniformDataType.Vec4);
    }
    /// <summary>
    /// Sets a Vector2 value for a shader property using a <see cref="Vector2"/>.
    /// </summary>
    /// <param name="shader">The shader to set the value on.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="vec">The <see cref="Vector2"/> value to set.</param>
    public static void SetValueVector2(Shader shader, string propertyName, Vector2 vec)
    {
        int valueLocation = Raylib.GetShaderLocation(shader, propertyName);
        Raylib.SetShaderValue(shader, valueLocation, new[] { vec.X, vec.Y}, ShaderUniformDataType.Vec2);
    }
    /// <summary>
    /// Sets a color value for a shader property using a <see cref="ColorRgba"/>.
    /// </summary>
    /// <param name="shader">The shader to set the value on.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="colorRgba">The color value to set.</param>
    public static void SetValueColor(Shader shader, string propertyName, ColorRgba colorRgba)
    {
        int valueLocation = Raylib.GetShaderLocation(shader, propertyName);
        Raylib.SetShaderValue(shader, valueLocation, new[] {colorRgba.R / 255f, colorRgba.G / 255f, colorRgba.B / 255f, colorRgba.A / 255f}, ShaderUniformDataType.Vec4);
    }
}