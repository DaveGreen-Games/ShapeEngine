using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.StaticLib.Drawing;

namespace ShapeEngine.UI;

/// <summary>
/// Represents a rectangular node in the UI hierarchy, supporting anchoring, stretching, margins, and parent/child relationships.
/// </summary>
/// <remarks>
/// RectNode is a base class for UI layout and interaction, supporting mouse filtering, hierarchical parenting, and flexible sizing.
/// Each <see cref="RectNode"/> can have one <see cref="RectNode"/> parent and any number of <see cref="RectNode"/> children.
/// </remarks>
public class RectNode
{
    #region Events

    /// <summary>
    /// Occurs when the parent of this node changes.
    /// </summary>
    /// <remarks>
    /// Parameters:
    /// <list type="bullet">
    /// <item><description>Invoker: The node whose parent changed.</description></item>
    /// <item><description>Old Parent: The previous parent node, or null if there was none.</description></item>
    /// <item><description>New Parent: The new parent node, or null if detached.</description></item>
    /// </list>
    /// </remarks>
    public event Action<RectNode, RectNode?, RectNode?>? OnParentChanged;
    
    /// <summary>
    /// Occurs when a new child is added to this node.
    /// </summary>
    /// <remarks>
    /// Parameters:
    /// <list type="bullet">
    /// <item><description>Invoker: The node to which the child was added.</description></item>
    /// <item><description>New Child: The child node that was added.</description></item>
    /// </list>
    /// </remarks>
    public event Action<RectNode, RectNode>? OnChildAdded;
    
    /// <summary>
    /// Occurs when a child is removed from this node.
    /// </summary>
    /// <remarks>
    /// Parameters:
    /// <list type="bullet">
    /// <item><description>Invoker: The node from which the child was removed.</description></item>
    /// <item><description>Old Child: The child node that was removed.</description></item>
    /// </list>
    /// </remarks>
    public event Action<RectNode, RectNode>? OnChildRemoved;
    
    /// <summary>
    /// Occurs when the mouse enters this node's area.
    /// </summary>
    /// <remarks>
    /// Parameters:
    /// <list type="bullet">
    /// <item><description>Invoker: The node the mouse entered.</description></item>
    /// <item><description>Mouse Pos: The position of the mouse when entering.</description></item>
    /// </list>
    /// </remarks>
    public event Action<RectNode,Vector2>? OnMouseEntered;
    
    /// <summary>
    /// Occurs when the mouse exits this node's area.
    /// </summary>
    /// <remarks>
    /// Parameters:
    /// <list type="bullet">
    /// <item><description>Invoker: The node the mouse exited.</description></item>
    /// <item><description>Last Mouse Pos Inside: The last position of the mouse inside the node.</description></item>
    /// </list>
    /// </remarks>
    public event Action<RectNode,Vector2>? OnMouseExited;
    
    /// <summary>
    /// Occurs when the mouse filter for this node changes.
    /// </summary>
    /// <remarks>
    /// Parameters:
    /// <list type="bullet">
    /// <item><description>Invoker: The node whose mouse filter changed.</description></item>
    /// <item><description>Old Filter: The previous mouse filter value.</description></item>
    /// <item><description>New Filter: The new mouse filter value.</description></item>
    /// </list>
    /// </remarks>
    public event Action<RectNode, MouseFilter, MouseFilter>? OnMouseFilterChanged;
    
    
    #endregion

    #region Constructor
    /// <summary>
    /// Initializes a new instance of the <see cref="RectNode"/> class.
    /// </summary>
    public RectNode() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RectNode"/> class with a name.
    /// </summary>
    /// <param name="name">The name of the node.</param>
    public RectNode(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RectNode"/> class with anchor and stretch.
    /// </summary>
    /// <param name="anchor">The anchor point.</param>
    /// <param name="stretch">The stretch vector.</param>
    public RectNode(AnchorPoint anchor, Vector2 stretch)
    {
        Anchor = anchor;
        Stretch = stretch;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RectNode"/> class with anchor, stretch, and name.
    /// </summary>
    /// <param name="anchor">The anchor point.</param>
    /// <param name="stretch">The stretch vector.</param>
    /// <param name="name">The name of the node.</param>
    public RectNode(AnchorPoint anchor, Vector2 stretch, string name)
    {
        Anchor = anchor;
        Stretch = stretch;
        Name = name;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RectNode"/> class with anchor, stretch, and mouse filter.
    /// </summary>
    /// <param name="anchor">The anchor point.</param>
    /// <param name="stretch">The stretch vector.</param>
    /// <param name="mouseFilter">The mouse filter mode.</param>
    public RectNode(AnchorPoint anchor, Vector2 stretch, MouseFilter mouseFilter)
    {
        Anchor = anchor;
        Stretch = stretch;
        this.mouseFilter = mouseFilter;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RectNode"/> class with anchor, stretch, margins, and mouse filter.
    /// </summary>
    /// <param name="anchor">The anchor point.</param>
    /// <param name="stretch">The stretch vector.</param>
    /// <param name="margins">The margins.</param>
    /// <param name="mouseFilter">The mouse filter mode.</param>
    public RectNode(AnchorPoint anchor, Vector2 stretch, Rect.Margins margins, MouseFilter mouseFilter)
    {
        Anchor = anchor;
        Stretch = stretch;
        this.mouseFilter = mouseFilter;
        Margins = margins;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RectNode"/> class with anchor, stretch, margins, mouse filter, and name.
    /// </summary>
    /// <param name="anchor">The anchor point.</param>
    /// <param name="stretch">The stretch vector.</param>
    /// <param name="margins">The margins.</param>
    /// <param name="mouseFilter">The mouse filter mode.</param>
    /// <param name="name">The name of the node.</param>
    public RectNode(AnchorPoint anchor, Vector2 stretch, Rect.Margins margins, MouseFilter mouseFilter, string name)
    {
        Anchor = anchor;
        Stretch = stretch;
        this.mouseFilter = mouseFilter;
        Margins = margins;
        Name = name;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RectNode"/> class with anchor, stretch, mouse filter, and name.
    /// </summary>
    /// <param name="anchor">The anchor point.</param>
    /// <param name="stretch">The stretch vector.</param>
    /// <param name="mouseFilter">The mouse filter mode.</param>
    /// <param name="name">The name of the node.</param>
    public RectNode(AnchorPoint anchor, Vector2 stretch, MouseFilter mouseFilter, string name)
    {
        Anchor = anchor;
        Stretch = stretch;
        this.mouseFilter = mouseFilter;
        Name = name;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RectNode"/> class with anchor, stretch, and margins.
    /// </summary>
    /// <param name="anchor">The anchor point.</param>
    /// <param name="stretch">The stretch vector.</param>
    /// <param name="margins">The margins.</param>
    public RectNode(AnchorPoint anchor, Vector2 stretch, Rect.Margins margins)
    {
        Anchor = anchor;
        Stretch = stretch;
        Margins = margins;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RectNode"/> class with anchor, stretch, margins, and name.
    /// </summary>
    /// <param name="anchor">The anchor point.</param>
    /// <param name="stretch">The stretch vector.</param>
    /// <param name="margins">The margins.</param>
    /// <param name="name">The name of the node.</param>
    public RectNode(AnchorPoint anchor, Vector2 stretch, Rect.Margins margins, string name)
    {
        Anchor = anchor;
        Stretch = stretch;
        Margins = margins;
        Name = name;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RectNode"/> class with anchor, stretch, margins, min/max size, mouse filter, and name.
    /// </summary>
    /// <param name="anchor">The anchor point.</param>
    /// <param name="stretch">The stretch vector.</param>
    /// <param name="margins">The margins.</param>
    /// <param name="minSize">The minimum size.</param>
    /// <param name="maxSize">The maximum size.</param>
    /// <param name="mouseFilter">The mouse filter mode.</param>
    /// <param name="name">The name of the node.</param>
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
    private RectNode? parent;
    private readonly List<RectNode> children = new();
    private MouseFilter mouseFilter = MouseFilter.Ignore;
    #endregion

    #region Public Members
    
    /// <summary>
    /// The anchor point for this node's rectangle.
    /// </summary>
    public AnchorPoint Anchor = new(0f);

    /// <summary>
    /// Stretch determines the size of the rect based on the parent rect size. Values are relative and in range 0 - 1.
    /// If Stretch values are 0 then the size of the rect can be set manually without it being changed by the parent rect size.
    /// </summary>
    public Vector2 Stretch = new(1, 1);

    /// <summary>
    /// The minimum size constraint for this node.
    /// </summary>
    public Size MinSize = new(0f);

    /// <summary>
    /// The maximum size constraint for this node.
    /// </summary>
    public Size MaxSize = new(0f);

    /// <summary>
    /// The margins applied to this node's rectangle.
    /// </summary>
    public Rect.Margins Margins = new();

    /// <summary>
    /// The name of this node.
    /// </summary>
    public string Name = string.Empty;

    /// <summary>
    /// Gets or sets the mouse filter mode for this node.
    /// </summary>
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

    /// <summary>
    /// Gets the rectangle representing this node's position and size.
    /// </summary>
    public Rect Rect { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the mouse is currently inside this node.
    /// </summary>
    public bool MouseInside { get; private set; }

    /// <summary>
    /// Gets the last known mouse position inside this node.
    /// </summary>
    public Vector2 MouseInsidePosition { get; private set; }

    /// <summary>
    /// Gets the parent node of this node, or null if it has no parent.
    /// </summary>
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

    /// <summary>
    /// Gets the number of child nodes.
    /// </summary>
    public int ChildCount => children.Count;

    /// <summary>
    /// Gets a value indicating whether this node has a parent.
    /// </summary>
    public bool HasParent => parent != null;

    /// <summary>
    /// Gets a value indicating whether this node has any children.
    /// </summary>
    public bool HasChildren => children.Count > 0;

    /// <summary>
    /// Gets an enumerable of this node's children.
    /// </summary>
    public IEnumerable<RectNode> GetChildrenEnumerable => children;
    #endregion

    #region Children

    /// <summary>
    /// Adds a child node to this node.
    /// </summary>
    /// <param name="child">The child node to add.</param>
    /// <returns>True if the child was added; otherwise, false.</returns>
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
    /// <summary>
    /// Removes a child node from this node.
    /// </summary>
    /// <param name="child">The child node to remove.</param>
    /// <returns>True if the child was removed; otherwise, false.</returns>
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
    
    /// <summary>
    /// Removes all children from this node.
    /// </summary>
    public void ClearChildren()
    {
        for (int i = children.Count - 1; i >= 0; i--)
        {
            RemoveChild(i);
        }
    }

    /// <summary>
    /// Gets the child node at the specified index.
    /// </summary>
    /// <param name="index">The index of the child.</param>
    /// <returns>The child node, or null if the index is out of range.</returns>
    public RectNode? GetChild(int index) => children.Count <= index ? null : children[index];

    /// <summary>
    /// Gets a copy of the list of child nodes.
    /// </summary>
    /// <returns>A new list containing all child nodes.</returns>
    public List<RectNode>? GetChildrenCopy() => children.Count <= 0 ? null : children.ToList();

    /// <summary>
    /// Gets a list of child nodes that match the specified predicate.
    /// </summary>
    /// <param name="match">The predicate to match.</param>
    /// <returns>A list of matching child nodes.</returns>
    public List<RectNode>? GetChildren(Predicate<RectNode> match) => children.Count <= 0 ? null : children.FindAll(match);

    /// <summary>
    /// Recursively gets all descendant nodes and adds them to the provided set.
    /// </summary>
    /// <param name="result">A set to which all descendants will be added.</param>
    /// <returns>The number of nodes added.</returns>
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

    /// <summary>
    /// Recursively gets all descendant nodes matching the predicate and adds them to the provided set.
    /// </summary>
    /// <param name="match">The predicate to match.</param>
    /// <param name="result">A set to which all matching descendants will be added.</param>
    /// <returns>The number of nodes added.</returns>
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
    /// <summary>
    /// Gets a child node by a path string, using the specified separator.
    /// </summary>
    /// <example>
    /// "Ui/Player/BottomLeft/Health" would return the child node with the name "Health".
    /// This would use "/" as seperator.
    /// "Ui Player BottomLeft Health" would use the default " " (space) as the separator.
    /// </example>
    /// <param name="path">The path string.</param>
    /// <param name="separator">The separator character.</param>
    /// <returns>The child node, or null if not found.</returns>
    public RectNode? GetChild(string path, char separator = ' ')
    {
        if (path.Length <= 0) return null;
        if (path == Name) return this;
        var names = path.Split(separator);
        return GetChild(names);
    }

    /// <summary>
    /// Gets a child node by a sequence of names.
    /// </summary>
    /// <param name="path">The sequence of names.</param>
    /// <returns>The child node, or null if not found.</returns>
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

    /// <summary>
    /// Gets the rectangle of a child node by a sequence of names.
    /// </summary>
    /// <param name="path">The sequence of names.</param>
    /// <returns>The rectangle of the child node, or this node's rectangle if not found.</returns>
    public Rect GetRect(params string[] path)
    {
        var child = GetChild(path);
        if (child != null) return child.Rect;

        return Rect;
    }

    /// <summary>
    /// Gets the rectangle of a child node by a path string and separator.
    /// </summary>
    /// <example>
    /// "Ui/Player/BottomLeft/Health" would return the child rect with the name "Health".
    /// This would use "/" as seperator.
    /// "Ui Player BottomLeft Health" would use the default " " (space) as the separator.
    /// </example>
    /// <param name="path">The path string.</param>
    /// <param name="seperator">The separator character.</param>
    /// <returns>The rectangle of the child node, or this node's rectangle if not found.</returns>
    public Rect GetRect(string path, char seperator = ' ')
    {
        var child = GetChild(path, seperator);
        if (child != null) return child.Rect;

        return Rect;
    }
    #endregion
    
    #region Update & Draw

    /// <summary>
    /// Sets this node's rectangle, applying margins.
    /// </summary>
    /// <param name="newRect">The new rectangle.</param>
    public void SetRect(Rect newRect)
    {
        Rect = newRect.ApplyMargins(Margins);
    }

    /// <summary>
    /// Updates this node's rectangle based on a source rectangle and constraints.
    /// </summary>
    /// <param name="sourceRect">The source rectangle.</param>
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

    /// <summary>
    /// Updates this node and its children, starting from the root.
    /// </summary>
    /// <remarks>Can only be called on the root node! If this <see cref="RectNode"/> has a <see cref="Parent"/> then the function will return.</remarks>
    /// <param name="dt">The delta time.</param>
    /// <param name="mousePos">The mouse position.</param>
    public void Update(float dt, Vector2 mousePos)
    {
        if (parent != null) return;
        InternalUpdate(dt, mousePos, true);
    }

    /// <summary>
    /// Draws this node and its children, starting from the root.
    /// </summary>
    /// <remarks>Can only be called on the root node! If this <see cref="RectNode"/> has a <see cref="Parent"/> then the function will return.</remarks>
    public void Draw()
    {
        if (parent != null) return;
        InternalDraw();
    }

    /// <summary>
    /// Draws debug lines for this node and its children.
    /// </summary>
    /// <param name="color">The color of the debug lines.</param>
    /// <param name="lineThickness">The thickness of the lines.</param>
    public void DebugDraw(ColorRgba color, float lineThickness)
    {
        Rect.DrawLines(lineThickness, color);
        foreach (var child in children)
        {
            child.DebugDraw(color, lineThickness);
        }
    }

    /// <summary>
    /// Sets the rectangle for a child node. Can be overridden for custom layouts.
    /// </summary>
    /// <param name="child">The child node.</param>
    /// <param name="inputRect">The input rectangle.</param>
    /// <returns>The rectangle to assign to the child.</returns>
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

    /// <summary>
    /// Calculates the distance between the top-left corners of this node's rectangle and another node's rectangle.
    /// </summary>
    /// <param name="other">The other node.</param>
    /// <returns>The distance, or -1 if the same node.</returns>
    public float GetDistanceTo(RectNode other)
    {
        if (other == this) return -1f;
        return (other.Rect.TopLeft - Rect.TopLeft).Length();
    }

    /// <summary>
    /// Calculates the squared distance between the top-left corners of this node's rectangle and another node's rectangle.
    /// </summary>
    /// <param name="other">The other node.</param>
    /// <returns>The squared distance, or -1 if the same node.</returns>
    public float GetDistanceSquaredTo(RectNode other)
    {
        if (other == this) return -1f;
        return (other.Rect.TopLeft - Rect.TopLeft).LengthSquared();
    }

    #endregion
    
    #region Virtual
    /// <summary>
    /// Called when this node is updated.
    /// </summary>
    /// <remarks>Override this method for custom logic.</remarks>
    /// <param name="dt">The delta time.</param>
    /// <param name="mousePos">The mouse position.</param>
    /// <param name="mousePosValid">Whether the mouse position is valid.</param>
    protected virtual void OnUpdate(float dt, Vector2 mousePos, bool mousePosValid) { }

    /// <summary>
    /// Called when a child node is updated.
    /// </summary>
    /// <remarks>Override this method for custom logic.</remarks>
    /// <param name="child">The child node.</param>
    protected virtual void OnChildUpdated(RectNode child) { }

    /// <summary>
    /// Called when this node is drawn.
    /// </summary>
    /// <remarks>Override this method for custom logic.</remarks>
    protected virtual void OnDraw() { } 

    /// <summary>
    /// Called when a child node is drawn.
    /// </summary>
    /// <remarks>Override this method for custom logic.</remarks>
    /// <param name="child">The child node.</param>
    protected virtual void OnChildDrawn(RectNode child) { }

    /// <summary>
    /// Called when the parent of this node changes.
    /// </summary>
    /// <remarks>Override this method for custom logic.</remarks>
    /// <param name="oldParent">The previous parent node.</param>
    /// <param name="newParent">The new parent node.</param>
    protected virtual void ParentWasChanged(RectNode? oldParent, RectNode? newParent) { }

    /// <summary>
    /// Called when a child node is added.
    /// </summary>
    /// <remarks>Override this method for custom logic.</remarks>
    /// <param name="newChild">The new child node.</param>
    protected virtual void ChildWasAdded(RectNode newChild) { }

    /// <summary>
    /// Called when a child node is removed.
    /// </summary>
    /// <param name="oldChild">The removed child node.</param>
    protected virtual void ChildWasRemoved(RectNode oldChild) { }

    /// <summary>
    /// Called when the mouse enters this node.
    /// </summary>
    /// <param name="mousePos">The mouse position.</param>
    protected virtual void MouseHasEntered(Vector2 mousePos) { }

    /// <summary>
    /// Called when the mouse exits this node.
    /// </summary>
    /// <remarks>Override this method for custom logic.</remarks>
    /// <param name="mousePos">The mouse position.</param>
    protected virtual void MouseHasExited(Vector2 mousePos) { }

    /// <summary>
    /// Called when the mouse filter changes.
    /// </summary>
    /// <remarks>Override this method for custom logic.</remarks>
    /// <param name="old">The old mouse filter.</param>
    /// <param name="cur">The new mouse filter.</param>
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