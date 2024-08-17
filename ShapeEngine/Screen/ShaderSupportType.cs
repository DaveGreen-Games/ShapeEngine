namespace ShapeEngine.Screen;

public enum ShaderSupportType
{
    /// <summary>
    /// No shaders supported. No screen shader buffer texture loaded.
    /// </summary>
    None = 1,
    
    /// <summary>
    /// A single shader is supported. Shader container can still hold
    /// any number of shaders but only first active shader will be used.
    /// </summary>
    Single = 2,
    
    /// <summary>
    /// Any number of screen shaders is supported.
    /// </summary>
    Multi = 4
}