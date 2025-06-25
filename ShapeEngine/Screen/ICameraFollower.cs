using ShapeEngine.Geometry.Rect;

namespace ShapeEngine.Screen;

/// <summary>
/// Defines methods for objects that follow or interact with a camera in the scene.
/// Can update the camera's position and area based on a target.
/// </summary>
/// <remarks>
/// Implement this interface for objects that need to control or follow a camera, such as player or object tracking systems.
/// </remarks>
public interface ICameraFollower
{
    /// <summary>
    /// Resets the follower to its initial state.
    /// </summary>
    void Reset();

    /// <summary>
    /// Updates the follower's state based on the elapsed time and current camera rectangle.
    /// </summary>
    /// <param name="dt">The elapsed time since the last update, in seconds.</param>
    /// <param name="cameraRect">The current rectangle representing the camera's view.</param>
    /// <returns>The updated rectangle for the follower.</returns>
    Rect Update(float dt, Rect cameraRect);

    /// <summary>
    /// Called when the follower is attached to a camera.
    /// </summary>
    void OnCameraAttached();

    /// <summary>
    /// Called when the follower is detached from a camera.
    /// </summary>
    void OnCameraDetached();
}
