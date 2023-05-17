using ShapeCore;
using System.Numerics;

namespace ShapeLib
{
    public static class SSegment
    {
        public static List<Segment> Split(this Segment s, float f)
        {
            Vector2 p = GetPoint(s, f);
            return new() { new(s.start, p), new(p, s.end) };
        }
        public static Vector2 GetPoint(this Segment s, float f) { return s.start.Lerp(s.end, f); }
        public static Segment Rotate(this Segment s, float pivot, float rad)
        {
            Vector2 p = GetPoint(s, pivot);
            Vector2 s = s.start - p;
            Vector2 e = s.end - p;
            return new Segment(p + s.Rotate(rad), p + e.Rotate(rad));


            //float len = l.Length;
            //Vector2 d = l.Dir;
            //
            //float startLength = len * pivot;
            //float endLength = len * (1f - pivot);
            //
            //Vector2 p = l.start + d * startLength;
            //Vector2 newStart = p - (d * startLength).Rotate(rad);
            //Vector2 newEnd = p + (d * endLength).Rotate(rad);
            //return new Line(newStart, newEnd);
        }
        public static Segment Scale(this Segment s, float scale) { return new(s.start * scale, s.end * scale); }
        public static Segment Scale(this Segment s, Vector2 scale) { return new(s.start * scale, s.end * scale); }
        public static Segment Scale(this Segment s, float startScale, float endScale) { return new(s.start * startScale, s.end * endScale); }
        public static Segment ScaleF(this Segment s, float scale, float f)
        {
            Vector2 p = GetPoint(s, f);
            Vector2 s = s.start - p;
            Vector2 e = s.end - p;
            return new Segment(p + s * scale, p + e * scale);

            //float len = l.Length;
            //Vector2 d = l.Dir;
            //
            //float startLength = len * f;
            //float endLength = len * (1f - f);
            //
            //Vector2 p = l.start + d * startLength;
            //Vector2 newStart = p - (d * startLength * scale);
            //Vector2 newEnd = p + (d * endLength * scale);
            //return new Line(newStart, newEnd);
        }
        public static Segment ScaleF(this Segment s, Vector2 scale, float f)
        {
            Vector2 p = GetPoint(s, f);
            Vector2 s = s.start - p;
            Vector2 e = s.end - p;
            return new Segment(p + s * scale, p + e * scale);
        }
        public static Segment Move(this Segment s, Vector2 offset, float f) { return new(s.start + (offset * (1f - f)), s.end + (offset * (f))); }
        public static Segment Move(this Segment s, Vector2 offset) { return new(s.start + offset, s.end + offset); }
        public static Segment Move(this Segment s, float x, float y) { return Move(s, new Vector2(x, y)); }
       
    }
}
