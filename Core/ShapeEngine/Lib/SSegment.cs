using ShapeEngine.Core;
using System.Numerics;

namespace ShapeEngine.Lib
{
    public static class SSegment
    {
        /*
        public static List<Segment> Split(this Segment seg, float f)
        {
            Vector2 p = GetPoint(seg, f);
            return new() { new(seg.Start, p), new(p, seg.End) };
        }
        public static Vector2 GetPoint(this Segment seg, float f) { return seg.Start.Lerp(seg.End, f); }
        public static Segment Rotate(this Segment seg, float pivot, float rad)
        {
            Vector2 p = GetPoint(seg, pivot);
            Vector2 s = seg.Start - p;
            Vector2 e = seg.End - p;
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
        public static Segment Scale(this Segment seg, float scale) { return new(seg.Start * scale, seg.End * scale); }
        public static Segment Scale(this Segment seg, Vector2 scale) { return new(seg.Start * scale, seg.End * scale); }
        public static Segment Scale(this Segment seg, float startScale, float endScale) { return new(seg.Start * startScale, seg.End * endScale); }
        public static Segment ScaleF(this Segment seg, float scale, float f)
        {
            Vector2 p = GetPoint(seg, f);
            Vector2 s = seg.Start - p;
            Vector2 e = seg.End - p;
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
        public static Segment ScaleF(this Segment seg, Vector2 scale, float f)
        {
            Vector2 p = GetPoint(seg, f);
            Vector2 s = seg.Start - p;
            Vector2 e = seg.End - p;
            return new Segment(p + s * scale, p + e * scale);
        }
        public static Segment Move(this Segment s, Vector2 offset, float f) { return new(s.Start + (offset * (1f - f)), s.End + (offset * (f))); }
        public static Segment Move(this Segment s, Vector2 offset) { return new(s.Start + offset, s.End + offset); }
        public static Segment Move(this Segment s, float x, float y) { return Move(s, new Vector2(x, y)); }


        public static Points Inflate(this Segment s, float thickness, float alignement = 0.5f)
        {
            float w = thickness;
            Vector2 dir = s.Dir;
            Vector2 left = dir.GetPerpendicularLeft();
            Vector2 right = dir.GetPerpendicularRight();
            Vector2 a = s.Start + left * w * alignement;
            Vector2 b = s.Start + right * w * (1 - alignement);
            Vector2 c = s.End + right * w * (1 - alignement);
            Vector2 d = s.End + left * w * alignement;

            return new() { a, b, c, d };
        }
    */
    
    }
}
