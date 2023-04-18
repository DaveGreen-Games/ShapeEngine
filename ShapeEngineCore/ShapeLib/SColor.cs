using Raylib_CsLo;

namespace ShapeLib
{
    public static class SColor
    {
        public static Raylib_CsLo.Color Lerp(this Raylib_CsLo.Color a, Raylib_CsLo.Color b, float f)
        {
            return new Raylib_CsLo.Color(
                (int)RayMath.Lerp(a.r, b.r, f),
                (int)RayMath.Lerp(a.g, b.g, f),
                (int)RayMath.Lerp(a.b, b.b, f),
                (int)RayMath.Lerp(a.a, b.a, f));
        }

        public static Raylib_CsLo.Color Add(this Raylib_CsLo.Color a, Raylib_CsLo.Color b)
        {
            return new
                (
                a.r + b.r,
                a.g + b.g,
                a.b + b.b,
                a.a + b.a
                );
        }
        public static Raylib_CsLo.Color Subtract(this Raylib_CsLo.Color a, Raylib_CsLo.Color b)
        {
            return new
                (
                a.r - b.r,
                a.g - b.g,
                a.b - b.b,
                a.a - b.a
                );
        }
        public static Raylib_CsLo.Color Multiply(this Raylib_CsLo.Color a, Raylib_CsLo.Color b)
        {
            return new
                (
                a.r * b.r,
                a.g * b.g,
                a.b * b.b,
                a.a * b.a
                );
        }
        public static Raylib_CsLo.Color ChangeAlpha(this Raylib_CsLo.Color c, byte a)
        {
            c.a = a;
            return c;
        }
        public static Raylib_CsLo.Color ChangeAlpha(this Raylib_CsLo.Color c, int change)
        {
            int newAlpha = c.a - change;
            c.a = (byte)newAlpha;
            return c;
        }
        public static Raylib_CsLo.Color ChangeRed(this Raylib_CsLo.Color c, byte r)
        {
            c.r = r;
            return c;
        }
        public static Raylib_CsLo.Color ChangeRed(this Raylib_CsLo.Color c, int change)
        {
            int newRed = c.r - change;
            c.r = (byte)newRed;
            return c;
        }
        public static Raylib_CsLo.Color ChangeGreen(this Raylib_CsLo.Color c, byte g)
        {
            c.g = g;
            return c;
        }
        public static Raylib_CsLo.Color ChangeGreen(this Raylib_CsLo.Color c, int change)
        {
            int newGreen = c.g - change;
            c.g = (byte)newGreen;
            return c;
        }
        public static Raylib_CsLo.Color ChangeBlue(Raylib_CsLo.Color c, byte b)
        {
            c.b = b;
            return c;
        }
        public static Raylib_CsLo.Color ChangeBlue(this Raylib_CsLo.Color c, int change)
        {
            int newBlue = c.b - change;
            c.b = (byte)newBlue;
            return c;
        }

        /// <summary>
        /// Creates color with corrected brightness.
        /// </summary>
        /// <param name="color">Color to correct.</param>
        /// <param name="correctionFactor">The brightness correction factor. Must be between -1 and 1. 
        /// Negative values produce darker colors.</param>
        /// <returns>
        /// Corrected <see cref="Raylib_CsLo.Color"/> structure.
        /// </returns>
        public static Raylib_CsLo.Color ChangeBrightness(this Raylib_CsLo.Color color, float correctionFactor)
        {
            float red = color.r;
            float green = color.g;
            float blue = color.b;

            if (correctionFactor < 0)
            {
                correctionFactor = 1 + correctionFactor;
                red *= correctionFactor;
                green *= correctionFactor;
                blue *= correctionFactor;
            }
            else
            {
                red = (255 - red) * correctionFactor + red;
                green = (255 - green) * correctionFactor + green;
                blue = (255 - blue) * correctionFactor + blue;
            }

            return new((byte)red, (byte)green, (byte)blue, color.a);
        }
        public static Raylib_CsLo.Color ChangeHUE(this Raylib_CsLo.Color color, int amount)
        {
            var hvs = ColorToHSV(color);
            return ColorFromHSV((hvs.hue + amount) % 360, hvs.saturation, hvs.value);
        }
        public static (float hue, float saturation, float value) ColorToHSV(this Raylib_CsLo.Color color)
        {
            int max = Math.Max(color.r, Math.Max(color.g, color.g));
            int min = Math.Min(color.r, Math.Min(color.g, color.b));

            var systemColor = System.Drawing.Color.FromArgb(color.a, color.r, color.g, color.b);

            float hue = systemColor.GetHue();
            float saturation = (max == 0) ? 0 : 1f - (1f * min / max);
            float value = max / 255f;

            return new(hue, saturation, value);
        }
        public static Raylib_CsLo.Color ColorFromHSV(float hue, float saturation, float value)
        {
            int hi = Convert.ToInt32(MathF.Floor(hue / 60)) % 6;
            float f = hue / 60 - MathF.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return new(v, t, p, 255);
            else if (hi == 1)
                return new(q, v, p, 255);
            else if (hi == 2)
                return new(p, v, t, 255);
            else if (hi == 3)
                return new(p, q, v, 255);
            else if (hi == 4)
                return new(t, p, v, 255);
            else
                return new(v, p, q, 255);
        }

    }
}
