using System.Numerics;

namespace ShapeEngine.StaticLib;

/// <summary>
/// Provides 2D Perlin noise generation utilities.
/// Produces smooth, continuous noise values (approximately in the range [-1, 1]) for given world coordinates
/// scaled by a lattice cell size (gridSize). Maintains a static cache of pseudo-random gradient vectors
/// and exposes an integer Seed to change the generated gradients. Changing Seed clears the gradient cache.
/// Note: the internal gradient cache is not synchronized; use external synchronization for concurrent access.
/// </summary>
public static class PerlinNoise2D
{
    private static readonly Dictionary<(int, int), Vector2> gradients = [];
    private static int _seed = 0;


    /// <summary>
    /// Gets or sets the integer seed used to generate pseudo-random gradient vectors.
    /// Setting a new value clears the cached gradient table so subsequent noise samples use gradients derived from the new seed.
    /// </summary>
    /// <value>The seed value. Default is 0.</value>
    public static int Seed
    {
        get { return _seed; }
        set
        {
            if(_seed == value) return;
            _seed = value;
            
            gradients.Clear();
        }
    }
    

    /// <summary>
    /// Generates a pseudo-random unit gradient vector for a given integer grid coordinate (x, y).
    /// </summary>
    private static Vector2 Gradient(int x, int y)
    {
        var key = (x, y);
        if(gradients.TryGetValue(key, out var existingGradient)) return existingGradient;

        System.Random rand = new(_seed + x * 4967 + y * 3253);

        float angle = rand.NextSingle() * MathF.PI * 2f;
        Vector2 g = new(MathF.Cos(angle), MathF.Sin(angle));

        gradients[key] = g;
        return g;
    }

    /// <summary>
    /// Performs standard linear interpolation between two values a and b based on the weight t.
    /// </summary>
    /// <param name="a">The Start value.</param>
    /// <param name="b">The end value.</param>
    /// <param name="t">The interpolation factor, typically in the range [0, 1].</param>
    /// <returns>The interpolated value between a and b based on t.</returns>
    private static float Lerp(float a, float b, float t)
    {
        return a + t * (b - a);
    }

    /// <summary>
    /// Ken Perlin's smoother step function: 6t^5 - 15t^4 + 10t^3.
    /// This ensures the derivatives are zero at t=0 and t=1, producing smoother transitions.
    /// </summary>
    /// <param name="t">The input value to be smoothed, typically in the range [0, 1].</param>
    private static float Fade(float t)
    {
        return t * t * t * (t * (t * 6f - 15f) + 10f);
    }

    /// <summary>
    /// Generates a 2D Perlin Noise value for a given coordinate (x, y) at a specified grid scale.
    /// The noise is smooth, continuous, and typically normalized to the range [-1.0, 1.0].
    /// </summary>
    /// <param name="x">The world/sample X coordinate. Interpreted in the same units as gridSize.</param>
    /// <param name="y">The world/sample Y coordinate. Interpreted in the same units as gridSize.</param>
    /// <param name="gridSize">The size of each lattice cell. Larger values produce lower-frequency (smoother) noise; smaller values produce higher-frequency detail.</param>
    /// <returns>Perlin noise value at (x,y), approximately in the range [-1, 1].</returns>
    public static float Noise(float x, float y, float gridSize)
    {
        int x0 = (int)MathF.Floor(x / gridSize);
        int y0 = (int)MathF.Floor(y / gridSize);
        int x1 = x0 + 1;
        int y1 = y0 + 1;

        float dx = x / gridSize - x0;
        float dy = y / gridSize - y0;

        Vector2 g00 = Gradient(x0, y0);
        Vector2 g10 = Gradient(x1, y0);
        Vector2 g01 = Gradient(x0, y1);
        Vector2 g11 = Gradient(x1, y1);

        Vector2 d00 = new(dx, dy);
        Vector2 d10 = new(dx - 1, dy);
        Vector2 d01 = new(dx, dy - 1);
        Vector2 d11 = new(dx - 1, dy - 1);

        float n00 = Vector2.Dot(g00, d00);
        float n10 = Vector2.Dot(g10, d10);
        float n01 = Vector2.Dot(g01, d01);
        float n11 = Vector2.Dot(g11, d11);

        float u = Fade(dx);
        float v = Fade(dy);

        float nx0 = Lerp(n00, n10, u);
        float nx1 = Lerp(n01, n11, u);

        float nxy = Lerp(nx0, nx1, v);

        return nxy;
    }
}
