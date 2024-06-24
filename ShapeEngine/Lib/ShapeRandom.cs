using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Random;

namespace ShapeEngine.Lib
{
    public static class ShapeRandom
    {
        //private static Random rand = new Random();
        private static RandomNumberGenerator rand = new RandomNumberGenerator();

        public static void SetSeed(int seed) { rand.SetSeed(seed); }

        public static bool Chance(float value) { return rand.Chance(value); }
        
        #region RandomItem
        public static T? PickRandomItem<T>(params WeightedItem<T>[] items)
        {
           return rand.PickRandomItem(items);
        }
        public static List<T> PickRandomItems<T>(int amount, params WeightedItem<T>[] items)
        {
            return rand.PickRandomItems(amount, items);
        }
        public static T? PickRandomItem<T>(params (T item, int weight)[] items)
        {
            return rand.PickRandomItem(items);
        }
        public static List<T> PickRandomItems<T>(int amount, params (T item, int weight)[] items)
        {
            return rand.PickRandomItems<T>(amount, items);
        }
        public static string PickRandomItem(params (string id, int weight)[] items)
        {
            return rand.PickRandomItem(items);
        }
        public static List<string> PickRandomItems(int amount, params (string id, int weight)[] items)
        {
            return rand.PickRandomItems(amount, items);
        }
        
        #endregion

        #region Angle
        public static float RandAngleRad() { return rand.RandAngleRad(); }
        public static float RandAngleDeg() { return rand.RandAngleDeg(); }
        
        public static float RandDirF() { return rand.RandDirF(); }
        public static int RandDirI() { return rand.RandDirI(); }
        #endregion
        
        #region Float
        public static float RandF() { return rand.RandF(); }
        public static float RandF(float max) { return rand.RandF(max); }
        public static float RandF(float min, float max) { return rand.RandF(min, max); }
        #endregion
        
        #region Int
        public static int RandI() { return rand.RandI(); }
        public static int RandI(int max) { return rand.RandI(max); }
        public static int RandI(int min, int max) { return rand.RandI(min, max); }
        #endregion
        
        #region Vec2
        public static Vector2 RandVec2() { return rand.RandVec2(); }
        public static Vector2 RandVec2(float max) { return rand.RandVec2(max); }
        public static Vector2 RandVec2(float min, float max) { return rand.RandVec2(min, max); }
        public static Vector2 RandPoint(Vector2 start, Vector2 end) { return rand.RandPoint(start, end); }
        public static Vector2 RandPoint(Vector2 origin) { return rand.RandPoint(origin); }
        public static Vector2 RandPoint(Vector2 origin, float max) { return rand.RandPoint(origin, max); }
        public static Vector2 RandPoint(Vector2 origin, float min, float max) { return rand.RandPoint(origin, min, max); }
        #endregion
        
        #region Size

        public static Size RandSize() => rand.RandSize();
        public static Size RandSize(float max) => rand.RandSize(max);
        public static Size RandSize(float min, float max) => rand.RandSize(min, max);
        public static Size RandSize(Size max) => rand.RandSize(max);
        public static Size RandSize(Size min, Size max) => rand.RandSize(min, max);
        #endregion
        
        #region Color
        public static ColorRgba RandColorAlpha(ColorRgba colorRgba) { return rand.RandColorAlpha(colorRgba); }
        public static ColorRgba RandColorAlpha(ColorRgba colorRgba, int max) { return rand.RandColorAlpha(colorRgba, max); }
        public static ColorRgba RandColorAlpha(ColorRgba colorRgba, int min, int max) { return rand.RandColorAlpha(colorRgba, min, max); }
        
        public static ColorRgba RandColorRed(ColorRgba colorRgba) { return rand.RandColorRed(colorRgba); }
        public static ColorRgba RandColorRed(ColorRgba colorRgba, int max) { return rand.RandColorRed(colorRgba, max); }
        public static ColorRgba RandColorRed(ColorRgba colorRgba, int min, int max) { return rand.RandColorRed(colorRgba, min, max); }
        
        public static ColorRgba RandColorGreen(ColorRgba colorRgba) { return rand.RandColorGreen(colorRgba); }
        public static ColorRgba RandColorGreen(ColorRgba colorRgba, int max) { return rand.RandColorGreen(colorRgba, max); }
        public static ColorRgba RandColorGreen(ColorRgba colorRgba, int min, int max) { return rand.RandColorGreen(colorRgba, min, max); }
        
        public static ColorRgba RandColorBlue(ColorRgba colorRgba) { return rand.RandColorBlue(colorRgba); }
        public static ColorRgba RandColorBlue(ColorRgba colorRgba, int max) { return rand.RandColorBlue(colorRgba, max); }
        public static ColorRgba RandColorBlue(ColorRgba colorRgba, int min, int max) { return rand.RandColorBlue(colorRgba, min, max); }
        
        public static ColorRgba RandColor() { return rand.RandColor(); }
        public static ColorRgba RandColor(int alpha) { return rand.RandColor(alpha); }
        public static ColorRgba RandColor(int min, int max, int alpha = -1) { return rand.RandColor(min, max, alpha); }
        #endregion

        #region Collection
        public static T? RandCollection<T>(List<T> list, bool pop = false) { return rand.RandCollection<T>(list, pop); }
        public static List<T> RandCollection<T>(List<T> source, int amount, bool pop = false) { return rand.RandCollection<T>(source, amount, pop); }
        public static T? RandCollection<T>(T[] array) { return rand.RandCollection(array); }
        #endregion
    }
}