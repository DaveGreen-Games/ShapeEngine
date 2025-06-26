using System.Numerics;

namespace ShapeEngine.Core;

/// <summary>
/// Represents a target that can be followed by a camera in the ShapeEngine.
/// </summary>
/// <remarks>
/// Implement this interface for any object that should be tracked by a camera system.
/// </remarks>
public interface ICameraFollowTarget
{
    /// <summary>
    /// Called when the camera starts following this target.
    /// </summary>
    /// <remarks>
    /// Use this to perform any setup or state changes needed when following begins.
    /// </remarks>
    public void FollowStarted();

    /// <summary>
    /// Called when the camera stops following this target.
    /// </summary>
    /// <remarks>
    /// Use this to perform any cleanup or state changes needed when following ends.
    /// </remarks>
    public void FollowEnded();

    /// <summary>
    /// Gets the position that the camera should follow.
    /// </summary>
    /// <returns>
    /// The <see cref="Vector2"/> position for the camera to track.
    /// </returns>
    public Vector2 GetCameraFollowPosition();
}