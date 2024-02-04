using ShapeEngine.Color;
using ShapeEngine.Lib;

namespace ShapeEngine.Screen;

internal sealed class ShapeFlash
{
    private float maxDuration = 0.0f;
    private float flashTimer = 0.0f;
    private ShapeColor startColor;
    private ShapeColor endColor;
    private ShapeColor curColor;

    public ShapeFlash(float duration, ShapeColor start, ShapeColor end)
    {

        maxDuration = duration;
        flashTimer = duration;
        startColor = start;
        curColor = start;
        endColor = end;
    }

    public void Update(float dt)
    {
        if (flashTimer > 0.0f)
        {
            flashTimer -= dt;
            float f = 1.0f - flashTimer / maxDuration;
            curColor = startColor.Lerp(endColor, f);
            if (flashTimer <= 0.0f)
            {
                flashTimer = 0.0f;
                curColor = endColor;
            }
        }
    }
    public bool IsFinished() { return flashTimer <= 0.0f; }
    public ShapeColor GetColor() { return curColor; }

}