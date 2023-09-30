using ShapeEngine.Lib;

namespace ShapeEngine.Screen;

internal sealed class ShapeFlash
{
    private float maxDuration = 0.0f;
    private float flashTimer = 0.0f;
    private Raylib_CsLo.Color startColor = new(0, 0, 0, 0);
    private Raylib_CsLo.Color endColor = new(0, 0, 0, 0);
    private Raylib_CsLo.Color curColor = new(0, 0, 0, 0);

    public ShapeFlash(float duration, Raylib_CsLo.Color start, Raylib_CsLo.Color end)
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
            curColor = startColor.Lerp(endColor, f); // SColor.LerpColor(startColor, endColor, f);
            if (flashTimer <= 0.0f)
            {
                flashTimer = 0.0f;
                curColor = endColor;
            }
        }
    }
    public bool IsFinished() { return flashTimer <= 0.0f; }
    public Raylib_CsLo.Color GetColor() { return curColor; }

}