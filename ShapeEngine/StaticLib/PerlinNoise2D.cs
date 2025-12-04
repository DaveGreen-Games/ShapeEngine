using Clipper2Lib;
using ShapeEngine.Core;
using ShapeEngine.Geometry.PointsDef;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace ShapeEngine.StaticLib
{
    public static class PerlinNoise2D
    {
        private static Dictionary<(int, int), Vector2> gradients = new Dictionary<(int, int), Vector2>();
        private static int _seed = 0;
        public static int seed
        {
            get { return _seed; }
            set
            {
                _seed = value;
                gradients.Clear();
            }
        }


        private static Vector2 Gradient(int x, int y)
        {
            var key = (x, y);
            if (gradients.ContainsKey(key))
                return gradients[key];

            System.Random rand = new System.Random(seed + x * 4967 + y * 3253);

            float angle = (float)(rand.NextDouble() * ShapeMath.PI * 2);
            Vector2 g = new Vector2(MathF.Cos(angle), MathF.Sin(angle));

            gradients[key] = g;
            return g;
        }

        private static float Lerp(float a, float b, float t)
        {
            return a + t * (b - a);
        }

        private static float Fade(float t)
        {
            return t * t * t * (t * (t * 6 - 15) + 10);
        }

        /// <summary>
        /// Perlin Noise is a gradient-based noise function used to generate smooth, natural-looking patterns.
        ///It produces values that vary continuously across space, avoiding sharp transitions and pure randomness.
        /// Because nearby points generate similar values, the output forms soft, organic structures.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="gridSize"></param>
        /// <returns></returns>
        public static float Noise(float x, float y, float gridSize)
        {
            int x0 = (int)Math.Floor(x / gridSize);
            int y0 = (int)Math.Floor(y / gridSize);
            int x1 = x0 + 1;
            int y1 = y0 + 1;

            float dx = x / gridSize - x0;
            float dy = y / gridSize - y0;

            Vector2 g00 = Gradient(x0, y0);
            Vector2 g10 = Gradient(x1, y0);
            Vector2 g01 = Gradient(x0, y1);
            Vector2 g11 = Gradient(x1, y1);

            Vector2 d00 = new Vector2(dx, dy);
            Vector2 d10 = new Vector2(dx - 1, dy);
            Vector2 d01 = new Vector2(dx, dy - 1);
            Vector2 d11 = new Vector2(dx - 1, dy - 1);

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
}
