
namespace ShapeEngine.Core;

/// <summary>
/// Represents an empty scene with no additional logic.
/// </summary>
public sealed class SceneEmpty : Scene
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SceneEmpty"/> class.
    /// </summary>
    public SceneEmpty() { }

    /// <summary>
    /// Called when the scene is activated.
    /// </summary>
    /// <param name="oldScene">The previously active scene.</param>
    protected override void OnActivate(Scene oldScene)
    {
    }

    /// <summary>
    /// Called when the scene is deactivated.
    /// </summary>
    protected override void OnDeactivate()
    {
    }
}


