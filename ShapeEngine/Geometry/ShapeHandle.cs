using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Geometry;

//TODO: Decide for one or the other implementation!

//TODO: DOCS!
public sealed class ShapeHandle
{
    #region Getters
    public readonly ShapeType ShapeType;

    public Segment Segment => segment??  throw new InvalidOperationException("ShapeHandle does not contain a Segment.");
    public Line Line => line?? throw new InvalidOperationException("ShapeHandle does not contain a Line.");
    public Ray Ray => ray?? throw new InvalidOperationException("ShapeHandle does not contain a Ray.");
    public Circle Circle => circle??  throw new InvalidOperationException("ShapeHandle does not contain a Circle.");
    public Triangle Triangle => triangle?? throw new InvalidOperationException("ShapeHandle does not contain a Triangle.");
    public Rect Rect => rect?? throw new InvalidOperationException("ShapeHandle does not contain a Rect.");
    public Quad Quad => quad??  throw new InvalidOperationException("ShapeHandle does not contain a Quad.");
    public Polygon Polygon => polygon?? throw new InvalidOperationException("ShapeHandle does not contain a Polygon.");
    public Polyline Polyline => polyline?? throw new InvalidOperationException("ShapeHandle does not contain a Polyline.");
    #endregion

    #region Members
    private readonly Segment? segment;
    private readonly Line? line;
    private readonly Ray? ray;
    private readonly Circle? circle;
    private readonly Triangle? triangle;
    private readonly Rect? rect;
    private readonly Quad? quad;
    private readonly Polygon? polygon;
    private readonly Polyline? polyline;
    #endregion

    #region Constructors
    private ShapeHandle(Segment s)
    {
        ShapeType = ShapeType.Segment;
        segment = s;
        line = null;
        ray = null;
        circle = null;
        triangle = null;
        rect = null;
        quad = null;
        polygon = null;
        polyline = null;
    }
    private ShapeHandle(Line l)
    {
        ShapeType = ShapeType.Line;
        segment = null;
        line = l;
        ray = null;
        circle = null;
        triangle = null;
        rect = null;
        quad = null;
        polygon = null;
        polyline = null;
    }
    public ShapeHandle(Ray r)
    {
        ShapeType = ShapeType.Ray;
        segment = null;
        line = null;
        ray = r;
        circle = null;
        triangle = null;
        rect = null;
        quad = null;
        polygon = null;
        polyline = null;
    }
    public ShapeHandle(Circle c)
    {
        ShapeType = ShapeType.Circle;
        segment = null;
        line = null;
        ray = null;
        circle = c;
        triangle = null;
        rect = null;
        quad = null;
        polygon = null;
        polyline = null;
    }
    private ShapeHandle(Triangle t)
    {
        ShapeType = ShapeType.Triangle;
        segment = null;
        line = null;
        ray = null;
        circle = null;
        triangle = t;
        rect = null;
        quad = null;
        polygon = null;
        polyline = null;
    }
    private ShapeHandle(Rect r)
    {
        ShapeType = ShapeType.Rect;
        segment = null;
        line = null;
        ray = null;
        circle = null;
        triangle = null;
        rect = r;
        quad = null;
        polygon = null;
        polyline = null;
    }
    private ShapeHandle(Quad q)
    {
        ShapeType = ShapeType.Quad;
        segment = null;
        line = null;
        ray = null;
        circle = null;
        triangle = null;
        rect = null;
        quad = q;
        polygon = null;
        polyline = null;
    }
    private ShapeHandle(Polygon p)
    {
        ShapeType = ShapeType.Poly;
        segment = null;
        line = null;
        ray = null;
        circle = null;
        triangle = null;
        rect = null;
        quad = null;
        polygon = p;
        polyline = null;
    }
    private ShapeHandle(Polyline pl)
    {
        ShapeType = ShapeType.PolyLine;
        segment = null;
        line = null;
        ray = null;
        circle = null;
        triangle = null;
        rect = null;
        quad = null;
        polygon = null;
        polyline = pl;
    }
    #endregion

    #region Static Factory
    public static ShapeHandle Create(Segment s) => new(s);
    public static ShapeHandle Create(Line l) => new(l);
    public static ShapeHandle Create(Ray r) => new(r);
    public static ShapeHandle Create(Circle c) => new(c);
    public static ShapeHandle Create(Triangle t) => new(t);
    public static ShapeHandle Create(Rect r) => new(r);
    public static ShapeHandle Create(Quad q) => new(q);
    public static ShapeHandle Create(Polygon p) => new(p);
    public static ShapeHandle Create(Polyline pl) => new(pl);
    #endregion

    #region TryGet helpers
    public bool TryGetSegment(out Segment value)
    {
        if (segment != null && ShapeType == ShapeType.Segment)
        {
            value = (Segment)segment;
            return true;
        }
        value = default!;
        return false;
    }

    public bool TryGetLine(out Line value)
    {
        if (line != null && ShapeType == ShapeType.Line)
        {
            value = (Line)line;
            return true;
        }
        value = default!;
        return false;
    }

    public bool TryGetRay(out Ray value)
    {
        if (ray != null && ShapeType == ShapeType.Ray)
        {
            value = (Ray)ray;
            return true;
        }
        value = default!;
        return false;
    }

    public bool TryGetCircle(out Circle value)
    {
        if (circle != null && ShapeType == ShapeType.Circle)
        {
            value = (Circle)circle;
            return true;
        }
        value = default!;
        return false;
    }

    public bool TryGetTriangle(out Triangle value)
    {
        if (triangle != null && ShapeType == ShapeType.Triangle)
        {
            value = (Triangle)triangle;
            return true;
        }
        value = default!;
        return false;
    }

    public bool TryGetRect(out Rect value)
    {
        if (rect != null && ShapeType == ShapeType.Rect)
        {
            value = (Rect)rect;
            return true;
        }
        value = default!;
        return false;
    }

    public bool TryGetQuad(out Quad value)
    {
        if (quad != null && ShapeType == ShapeType.Quad)
        {
            value = (Quad)quad;
            return true;
        }
        value = default!;
        return false;
    }

    public bool TryGetPolygon(out Polygon value)
    {
        if (polygon != null && ShapeType == ShapeType.Poly)
        {
            value = polygon;
            return true;
        }
        value = null!;
        return false;
    }

    public bool TryGetPolyline(out Polyline value)
    {
        if (polyline != null && ShapeType == ShapeType.PolyLine)
        {
            value = polyline;
            return true;
        }
        value = null!;
        return false;
    }

    public bool TryGet<T>(out T value)
    {
        if (segment is T s) { value = s; return true; }
        if (line is T l) { value = l; return true; }
        if (ray is T r) { value = r; return true; }
        if (circle is T c) { value = c; return true; }
        if (triangle is T t) { value = t; return true; }
        if (rect is T rc) { value = rc; return true; }
        if (quad is T q) { value = q; return true; }
        if (polygon is T p) { value = p; return true; }
        if (polyline is T pl) { value = pl; return true; }

        value = default!;
        return false;
    }
    #endregion
}

//TODO: DOCS & Rename!
public sealed class ShapeHandle2
{
    #region Members
    public readonly ShapeType ShapeType;
    private readonly object? shape;
    #endregion
    
    #region Constructor
    private ShapeHandle2(object shape, ShapeType type)
    {
        this.shape = shape ?? throw new ArgumentNullException(nameof(shape));
        ShapeType = type;
    }
    #endregion
    
    #region Factory Methods
    public static ShapeHandle2 Create(Segment s) => new(s, ShapeType.Segment);
    public static ShapeHandle2 Create(Line l) => new(l, ShapeType.Line);
    public static ShapeHandle2 Create(Ray r) => new(r, ShapeType.Ray);
    public static ShapeHandle2 Create(Circle c) => new(c, ShapeType.Circle);
    public static ShapeHandle2 Create(Triangle t) => new(t, ShapeType.Triangle);
    public static ShapeHandle2 Create(Rect r) => new(r, ShapeType.Rect);
    public static ShapeHandle2 Create(Quad q) => new(q, ShapeType.Quad);
    public static ShapeHandle2 Create(Polygon p) => new(p, ShapeType.Poly);
    public static ShapeHandle2 Create(Polyline pl) => new(pl, ShapeType.PolyLine);
    #endregion
    
    #region Getters
    public Segment Segment => shape is Segment s ? s : throw new InvalidOperationException("Not a Segment");
    public Line Line => shape is Line l ? l : throw new InvalidOperationException("Not a Line");
    public Ray Ray => shape is Ray r ? r : throw new InvalidOperationException("Not a Ray");
    public Circle Circle => shape is Circle c ? c : throw new InvalidOperationException("Not a Circle");
    public Triangle Triangle => shape is Triangle t ? t : throw new InvalidOperationException("Not a Triangle");
    public Rect Rect => shape is Rect r ? r : throw new InvalidOperationException("Not a Rect");
    public Quad Quad => shape is Quad q ? q : throw new InvalidOperationException("Not a Quad");
    public Polygon Polygon => shape is Polygon p ? p : throw new InvalidOperationException("Not a Polygon");
    public Polyline Polyline => shape is Polyline pl ? pl : throw new InvalidOperationException("Not a Polyline");
    #endregion
    
    public bool TryGet<T>(out T value)
    {
        if (shape is T v)
        {
            value = v;
            return true;
        }
        value = default!;
        return false;
    }
}