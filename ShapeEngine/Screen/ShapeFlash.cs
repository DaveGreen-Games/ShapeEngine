using ShapeEngine.Color;
using ShapeEngine.Lib;

namespace ShapeEngine.Screen;

internal sealed class ShapeFlash
{
    private float maxDuration = 0.0f;
    private float flashTimer = 0.0f;
    private ColorRgba startColorRgba;
    private ColorRgba endColorRgba;
    private ColorRgba curColorRgba;

    public ShapeFlash(float duration, ColorRgba start, ColorRgba end)
    {

        maxDuration = duration;
        flashTimer = duration;
        startColorRgba = start;
        curColorRgba = start;
        endColorRgba = end;
    }

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
    public bool IsFinished() { return flashTimer <= 0.0f; }
    public ColorRgba GetColor() { return curColorRgba; }

}