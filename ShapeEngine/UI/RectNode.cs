using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.StaticLib;
using ShapeEngine.StaticLib.Drawing;

namespace ShapeEngine.UI;

public class RectNode
{
    #region Events

    /// <summary>
    /// Parameters: Invoker, Old Parent, New Parent
    /// </summary>
    public event Action<RectNode, RectNode?, RectNode?>? OnParentChanged;
    /// <summary>
    /// Parameters: Invoker, New Child
    /// </summary>
    public event Action<RectNode, RectNode>? OnChildAdded;
    /// <summary>
    /// Parameters: Invoker, Old Child
    /// </summary>
    public event Action<RectNode, RectNode>? OnChildRemoved;

    
    /// <summary>
    /// Parameters: Invoker, Mouse Pos
    /// </summary>
    public event Action<RectNode,Vector2>? OnMouseEntered;
    /// <summary>
    /// Parameters: Invoker, Last Mouse Pos Inside
    /// </summary>
    public event Action<RectNode,Vector2>? OnMouseExited;
    
    /// <summary>
    /// Parameters: Invoker, Old Filter, New Filter
    /// </summary>
    public event Action<RectNode, MouseFilter, MouseFilter>? OnMouseFilterChanged;
    
    
    #endregion

    #region Constructor
    public RectNode() { }
    public RectNode(string name)
    {
        Name = name;
    }
    public RectNode(AnchorPoint anchor, Vector2 stretch)
    {
        Anchor = anchor;
        Stretch = stretch;
    }
    public RectNode(AnchorPoint anchor, Vector2 stretch, string name)
    {
        Anchor = anchor;
        Stretch = stretch;
        Name = name;
    }
    public RectNode(AnchorPoint anchor, Vector2 stretch, MouseFilter mouseFilter)
    {
        Anchor = anchor;
        Stretch = stretch;
        this.mouseFilter = mouseFilter;
    }
    public RectNode(AnchorPoint anchor, Vector2 stretch, Rect.Margins margins, MouseFilter mouseFilter)
    {
        Anchor = anchor;
        Stretch = stretch;
        this.mouseFilter = mouseFilter;
        Margins = margins;
    }
    public RectNode(AnchorPoint anchor, Vector2 stretch, Rect.Margins margins, MouseFilter mouseFilter, string name)
    {
        Anchor = anchor;
        Stretch = stretch;
        this.mouseFilter = mouseFilter;
        Margins = margins;
        Name = name;
    }
    public RectNode(AnchorPoint anchor, Vector2 stretch, MouseFilter mouseFilter, string name)
    {
        Anchor = anchor;
        Stretch = stretch;
        this.mouseFilter = mouseFilter;
        Name = name;
    }
    public RectNode(AnchorPoint anchor, Vector2 stretch, Rect.Margins margins)
    {
        Anchor = anchor;
        Stretch = stretch;
        Margins = margins;
    }
    public RectNode(AnchorPoint anchor, Vector2 stretch, Rect.Margins margins, string name)
    {
        Anchor = anchor;
        Stretch = stretch;
        Margins = margins;
        Name = name;
    }
    public RectNode(AnchorPoint anchor, Vector2 stretch, Rect.Margins margins, Size minSize, Size maxSize, MouseFilter mouseFilter, string name)
    {
        Anchor = anchor;
        Stretch = stretch;
        Margins = margins;
        Name = name;
        MinSize = minSize;
        MaxSize = maxSize;
        this.mouseFilter = mouseFilter;

    }
    #endregion
    
    #region Private Members
    private RectNode? parent = null;
    private readonly List<RectNode> children = new();
    private MouseFilter mouseFilter = MouseFilter.Ignore;
    #endregion

    #region Public Members
    
    public AnchorPoint Anchor = new(0f);
    /// <summary>
    /// Stretch determines the size of the rect based on the parent rect size. Values are relative and in range 0 - 1.
    /// If Stretch values are 0 than the size of the rect can be set manually without it being changed by the parent rect size.
    /// </summary>
    public Vector2 Stretch = new(1, 1);

    public Size MinSize = new(0f);
    public Size MaxSize = new(0f);
    public Rect.Margins Margins = new();
    public string Name = string.Empty;
    public MouseFilter MouseFilter
    {
        get => mouseFilter;
        set
        {
            if (mouseFilter == value) return;
            var prev = mouseFilter;
            mouseFilter = value;
            ResolveOnMouseFilterChanged(prev, mouseFilter);
        }
    }

    #endregion

    #region Getters & Setters
    public Rect Rect { get; private set; } = new();
    public bool MouseInside { get; private set; } = false;
    public Vector2 MouseInsidePosition { get; private set; }
    public RectNode? Parent
    {
        get => parent;
        private set
        {
            if (parent == value) return;
            var oldParent = parent;
            parent = value;
            ResolveParentChanged(oldParent, parent);
        }
        
    }
    public int ChildCount => children.Count;
    public bool HasParent => parent != null;
    public bool HasChildren => children.Count > 0;
    public IEnumerable<RectNode> GetChildrenEnumerable => children;
    #endregion

    #region Children

    public bool AddChild(RectNode child)
    {
        if (child.Parent != null)
        {
            if (child.Parent == this) return false;
            child.Parent.RemoveChild(child);
        }
        
        children.Add(child);
        ResolveChildAdded(child);
        child.Parent = this;
        return true;
    }
    public bool RemoveChild(RectNode child)
    {
        if (child.Parent == null || child.Parent != this) return false;
        if (!children.Remove(child)) return false;
        
        ResolveChildRemoved(child);
        child.Parent = null;

        return true;

    }
    private void RemoveChild(int index)
    {
        var child = children[index];
        children.RemoveAt(index);
        ResolveChildRemoved(child);
        child.Parent = null;
    }
    
    public void ClearChildren()
    {
        for (int i = children.Count - 1; i >= 0; i--)
        {
            RemoveChild(i);
        }
    }

    public RectNode? GetChild(int index) => children.Count <= index ? null : children[index];
    public List<RectNode>? GetChildrenCopy() => children.ToList();
    public List<RectNode>? GetChildren(Predicate<RectNode> match) => children.FindAll(match);
    public int GetAllChildren(ref HashSet<RectNode> result)
    {
        if (children.Count <= 0) return 0;
        
        var count = result.Count;
        foreach (var child in children)
        {
            result.Add(child);
            child.GetAllChildren(ref result);
        }

        return result.Count - count;
    }
    public int GetAllChildren(Predicate<RectNode> match, ref HashSet<RectNode> result)
    {
        if (children.Count <= 0) return 0;
        
        var count = result.Count;
        var matched = children.FindAll(match);
        foreach (var child in matched)
        {
            result.Add(child);
            child.GetAllChildren(match, ref result);
        }

        return result.Count - count;
    }

    private RectNode? FindChild(string name)
    {
        if (children.Count <= 0) return null;
        foreach (var child in children)
        {
            if (child.Name != string.Empty && child.Name == name) return child;
        }
        return null;
    }
    public RectNode? GetChild(string path, char separator = ' ')
    {
        if (path.Length <= 0) return null;
        if (path == Name) return this;
        var names = path.Split(separator);
        return GetChild(names);
    }
    public RectNode? GetChild(params string[] path)
    {
        if (path.Length <= 0) return null;
        
        var curChild = this;
        for (var i = 0; i < path.Length; i++)
        {
            string name = path[i];
            if (name == "") return curChild;
            var next = curChild.FindChild(name);
            if (next != null) curChild = next;
        }

        return curChild;
    }

    public Rect GetRect(params string[] path)
    {
        var child = GetChild(path);
        if (child != null) return child.Rect;

        return Rect;
    }
    public Rect GetRect(string path, char seperator = ' ')
    {
        var child = GetChild(path, seperator);
        if (child != null) return child.Rect;

        return Rect;
    }
    #endregion
    
    #region Update & Draw

    public void SetRect(Rect newRect)
    {
        Rect = newRect.ApplyMargins(Margins);
    }
    public void UpdateRect(Rect sourceRect)
    {
        var p = sourceRect.GetPoint(Anchor);
        var size = Stretch.LengthSquared() == 0f ? Rect.Size : sourceRect.Size * Stretch;

        if (MinSize.Area > 0)
        {
            size = size.Max(MinSize);
        }

        if (MaxSize.Area > 0)
        {
            size = size.Min(MaxSize);
        }
        
        Rect = new Rect(p, size, Anchor).ApplyMargins(Margins);
    }
    public void Update(float dt, Vector2 mousePos)
    {
        if (parent != null) return;
        InternalUpdate(dt, mousePos, true);
    }
    public void Draw()
    {
        if (parent != null) return;
        InternalDraw();
    }
    public void DebugDraw(ColorRgba color, float lineThickness)
    {
        Rect.DrawLines(lineThickness, color);
        foreach (var child in children)
        {
            child.DebugDraw(color, lineThickness);
        }
    }

    protected virtual Rect SetChildRect(RectNode child, Rect inputRect) => inputRect;
    private void UpdateChildren(float dt, Vector2 mousePos, bool mousePosValid)
    {
        foreach (var child in children)
        {
            child.UpdateRect(SetChildRect(child, Rect));
            child.InternalUpdate(dt, mousePos, mousePosValid);
            OnChildUpdated(child);
        }
    }

    private void DrawChildren()
    {
        foreach (var child in children)
        {
            child.InternalDraw();
            OnChildDrawn(child);
        }
    }

    private void InternalUpdate(float dt, Vector2 mousePos, bool mousePosValid)
    {
        if (MouseFilter != MouseFilter.Ignore)
        {
            bool isMouseInside = mousePosValid && Rect.ContainsPoint(mousePos);
            if (MouseFilter == MouseFilter.Stop) mousePosValid = false;
                
            if (MouseInside != isMouseInside)
            {
                MouseInside = isMouseInside;
                if (isMouseInside)
                {
                    ResolveMouseEntered(mousePos);
                }
                else
                {
                    ResolveMouseExited(MouseInsidePosition);
                }
            }

            if (isMouseInside) MouseInsidePosition = mousePos;
            
        }
        
        OnUpdate(dt, mousePos, mousePosValid);
        UpdateChildren(dt, mousePos, mousePosValid);
    }

    private void InternalDraw()
    {
        OnDraw();
        DrawChildren();
    }
    #endregion

    #region Public

    public float GetDistanceTo(RectNode other)
    {
        if (other == this) return -1f;
        return (other.Rect.TopLeft - Rect.TopLeft).Length();
    }
    public float GetDistanceSquaredTo(RectNode other)
    {
        if (other == this) return -1f;
        return (other.Rect.TopLeft - Rect.TopLeft).LengthSquared();
    }

    #endregion
    
    #region Virtual
    protected virtual void OnUpdate(float dt, Vector2 mousePos, bool mousePosValid) { }
    protected virtual void OnChildUpdated(RectNode child) { }
    protected virtual void OnDraw() { } 
    protected virtual void OnChildDrawn(RectNode child) { }
    protected virtual void ParentWasChanged(RectNode? oldParent, RectNode? newParent) { }
    protected virtual void ChildWasAdded(RectNode newChild) { }
    protected virtual void ChildWasRemoved(RectNode oldChild) { }
    protected virtual void MouseHasEntered(Vector2 mousePos) { }
    protected virtual void MouseHasExited(Vector2 mousePos) { }
    protected virtual void MouseFilterWasChanged(MouseFilter old, MouseFilter cur) { }
    #endregion

    #region Private
    private void ResolveParentChanged(RectNode? oldParent, RectNode? newParent)
    {
        ParentWasChanged(oldParent, newParent);
        OnParentChanged?.Invoke(this, oldParent, newParent);
        // ResolveOnNavigableChanged(Navigable);
    }
    private void ResolveChildAdded(RectNode newChild)
    {
        ChildWasAdded(newChild);
        OnChildAdded?.Invoke(this, newChild);
    }
    private void ResolveChildRemoved(RectNode oldChild)
    {
        ChildWasRemoved(oldChild);
        OnChildRemoved?.Invoke(this, oldChild);
    }
    private void ResolveMouseEntered(Vector2 mousePos)
    {
        MouseHasEntered(mousePos);
        OnMouseEntered?.Invoke(this, mousePos);
    }
    private void ResolveMouseExited(Vector2 mousePos)
    {
        MouseHasExited(mousePos);
        OnMouseExited?.Invoke(this, mousePos);
    }
    private void ResolveOnMouseFilterChanged(MouseFilter old, MouseFilter cur)
    {
        MouseFilterWasChanged(old, cur);
        OnMouseFilterChanged?.Invoke(this, old, cur);
    }
    #endregion
    
}