using ShapeEngine.Color;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Screen;

/// <summary>
/// Handles a color flash effect over a specified duration, interpolating between two colors.
/// </summary>
/// <remarks>
/// This class is used to create a temporary color flash effect, such as for screen flashes or UI feedback, by interpolating from a start color to an end color over a set duration.
/// </remarks>
internal sealed class ShapeFlash
{
    /// <summary>
    /// The maximum duration of the flash effect in seconds.
    /// </summary>
    private float maxDuration = 0.0f;
    /// <summary>
    /// The current timer for the flash effect in seconds.
    /// </summary>
    private float flashTimer = 0.0f;
    /// <summary>
    /// The starting color of the flash.
    /// </summary>
    private ColorRgba startColorRgba;
    /// <summary>
    /// The ending color of the flash.
    /// </summary>
    private ColorRgba endColorRgba;
    /// <summary>
    /// The current color of the flash, interpolated between start and end.
    /// </summary>
    private ColorRgba curColorRgba;

    /// <summary>
    /// Initializes a new instance of the <see cref="ShapeFlash"/> class.
    /// </summary>
    /// <param name="duration">The duration of the flash effect in seconds.</param>
    /// <param name="start">The starting color of the flash.</param>
    /// <param name="end">The ending color of the flash.</param>
    public ShapeFlash(float duration, ColorRgba start, ColorRgba end)
    {

        maxDuration = duration;
        flashTimer = duration;
        startColorRgba = start;
        curColorRgba = start;
        endColorRgba = end;
    }

    /// <summary>
    /// Updates the flash effect timer and interpolates the color.
    /// </summary>
    /// <param name="dt">The time elapsed since the last update, in seconds.</param>
    public void Update(float dt)
    {
        if (flashTimer > 0.0f)
        {
            flashTimer -= dt;
            float f = 1.0f - flashTimer / maxDuration;
            curColorRgba = startColorRgba.Lerp(endColorRgba, f);
            if (flashTimer <= 0.0f)
            {
                flashTimer = 0.0f;
                curColorRgba = endColorRgba;
            }
        }
    }

    /// <summary>
    /// Determines whether the flash effect has finished.
    /// </summary>
    /// <returns>True if the flash has finished; otherwise, false.</returns>
    public bool IsFinished() { return flashTimer <= 0.0f; }

    /// <summary>
    /// Gets the current color of the flash effect.
    /// </summary>
    /// <returns>The current interpolated color.</returns>
    public ColorRgba GetColor() { return curColorRgba; }

}