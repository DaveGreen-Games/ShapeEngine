using ShapeEngine.Core;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Screen;

/// <summary>
/// Implements a camera follower that tracks multiple targets,
/// adjusting the camera area to include all targets and optionally centering on a specific target.
/// Supports configurable margins and lerp speeds for smooth camera movement and resizing.
/// </summary>
/// <remarks>
/// Use this class to follow multiple objects in a scene, ensuring all are visible within the camera view.
/// Margins and lerp speeds allow for smooth transitions and dynamic area adjustments.
/// </remarks>
public class CameraFollowerMulti : ICameraFollower
{
    /// <summary>
    /// The target to center the camera on, if set.
    /// </summary>
    public ICameraFollowTarget? CenterTarget = null;
    /// <summary>
    /// The list of all camera follow targets.
    /// </summary>
    private readonly List<ICameraFollowTarget> targets = new();
    /// <summary>
    /// The previous camera rectangle, used for lerping.
    /// </summary>
    private Rect prevCameraRect;
    /// <summary>
    /// The margin (in pixels) to keep around all targets.
    /// </summary>
    public float TargetMargin = 250f;
    /// <summary>
    /// The minimum size of the camera area.
    /// </summary>
    public Size MinSize = new(100, 100);
    /// <summary>
    /// The lerp speed for camera position transitions (pixels per second).
    /// </summary>
    public float LerpSpeedPosition = 0f;
    /// <summary>
    /// The lerp speed for camera size transitions (pixels per second).
    /// </summary>
    public float LerpSpeedSize = 0f;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="CameraFollowerMulti"/> class.
    /// </summary>
    public CameraFollowerMulti()
    {
    
    }

    /// <summary>
    /// Resets the camera follower by clearing all current targets.
    /// </summary>
    public void Reset()
    {
        ClearTargets();
    }

    /// <summary>
    /// Draws a debug rectangle showing the previous camera area and its center.
    /// </summary>
    public void DrawDebugRect()
    {
        prevCameraRect.DrawLines(6f, new(System.Drawing.Color.IndianRed));
        prevCameraRect.Center.Draw(4f, new(System.Drawing.Color.LimeGreen));
        
    }
    /// <summary>
    /// Updates the camera rectangle to include all targets, applying margins and optional lerping.
    /// </summary>
    /// <param name="dt">Delta time in seconds.</param>
    /// <param name="cameraRect">The current camera rectangle.</param>
    /// <returns>The updated camera rectangle.</returns>
    public Rect Update(float dt, Rect cameraRect)
    {
        if (targets.Count <= 0) return cameraRect;
        // var cameraArea = camera.Area;
        
        var newCameraRect = new Rect(targets[0].GetCameraFollowPosition(), MinSize, new(0.5f));
        
        for (var i = 1; i < targets.Count; i++)
        {
            var target = targets[i];
            var pos = target.GetCameraFollowPosition();
            newCameraRect = newCameraRect.Enlarge(pos);
        }

        if (CenterTarget != null)
        {
            var pos = CenterTarget.GetCameraFollowPosition();
            float left = newCameraRect.Left;
            float right = newCameraRect.Right;
            float top = newCameraRect.Top;
            float bottom = newCameraRect.Bottom;

            float leftDif = MathF.Abs( left - pos.X );
            float rightDif = MathF.Abs( right - pos.X);
            float topDif = MathF.Abs( top - pos.Y);
            float bottomDif = MathF.Abs( bottom - pos.Y);

            float width = MathF.Max(leftDif, rightDif) * 2;
            float height = MathF.Max(topDif, bottomDif) * 2;

            newCameraRect = new(pos, new Size(width, height), new (0.5f, 0.5f));
        }

        newCameraRect = newCameraRect.ApplyMarginsAbsolute(-TargetMargin, -TargetMargin, -TargetMargin, -TargetMargin);
        if (LerpSpeedPosition <= 0f && LerpSpeedSize <= 0f)
        {
            prevCameraRect = newCameraRect;
            return newCameraRect;
        }
        // var finalPosition = prevCameraRect.Center.LerpTowards(newCameraRect.Center, 0.25f, dt );
        // var finalSize = prevCameraRect.Size.LerpTowards(newCameraRect.Size, 0.25f, dt);
        
        var finalPosition = LerpSpeedPosition > 0 ? prevCameraRect.Center.MoveTowards(newCameraRect.Center, LerpSpeedPosition * dt) : newCameraRect.Center;
        var finalSize = LerpSpeedSize > 0 ? prevCameraRect.Size.MoveTowards(newCameraRect.Size, LerpSpeedSize * dt) : newCameraRect.Size;
        prevCameraRect = new(finalPosition, finalSize, new(0.5f));
        return prevCameraRect;
    }


    /// <summary>
    /// Called when the follower is attached to a camera.
    /// </summary>
    public void OnCameraAttached()
    {
        
    }

    /// <summary>
    /// Called when the follower is detached from a camera.
    /// </summary>
    public void OnCameraDetached()
    {
        
    }
    
    /// <summary>
    /// Adds a new target to the follower.
    /// </summary>
    /// <param name="newTarget">The target to add.</param>
    /// <returns>True if the target was added; false if it was already present.</returns>
    public bool AddTarget(ICameraFollowTarget newTarget)
    {
        if (targets.Contains(newTarget)) return false;
        targets.Add(newTarget);
        return true;
    }

    /// <summary>
    /// Removes a target from the follower.
    /// </summary>
    /// <param name="target">The target to remove.</param>
    /// <returns>True if the target was removed; otherwise, false.</returns>
    public bool RemoveTarget(ICameraFollowTarget target)
    {
        return targets.Remove(target);
    }

    /// <summary>
    /// Removes all targets from the follower.
    /// </summary>
    public void ClearTargets()
    {
        targets.Clear();
    }
}