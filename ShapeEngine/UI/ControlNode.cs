using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.RectDef;

namespace ShapeEngine.UI;

/// <summary>
/// Represents an abstract base class for UI control nodes, providing core functionality for hierarchy, state, events, and input handling.
/// </summary>
public abstract class ControlNode
{
    #region Events

    /// <summary>
    /// Occurs when navigation happens to this node.
    /// Parameters: Invoker, Direction
    /// </summary>
    public event Action<ControlNode, Direction>? OnNavigated; 

    /// <summary>
    /// Occurs when the parent of this node changes.
    /// Parameters: Invoker, Old Parent, New Parent
    /// </summary>
    public event Action<ControlNode, ControlNode?, ControlNode?>? OnParentChanged;

    /// <summary>
    /// Occurs when a child is added to this node.
    /// Parameters: Invoker, New Child
    /// </summary>
    public event Action<ControlNode, ControlNode>? OnChildAdded;

    /// <summary>
    /// Occurs when a child is removed from this node.
    /// Parameters: Invoker, Old Child
    /// </summary>
    public event Action<ControlNode, ControlNode>? OnChildRemoved;

    /// <summary>
    /// Occurs when the displayed state of this node changes.
    /// Parameters: Invoker, Value
    /// </summary>
    public event Action<ControlNode, bool>? OnDisplayedChanged;

    /// <summary>
    /// Occurs when the active-in-hierarchy state changes.
    /// Parameters: Invoker, Value
    /// </summary>
    public event Action<ControlNode, bool>? OnActiveInHierarchyChanged;

    /// <summary>
    /// Occurs when the visible-in-hierarchy state changes.
    /// Parameters: Invoker, Value
    /// </summary>
    public event Action<ControlNode, bool>? OnVisibleInHierarchyChanged;

    /// <summary>
    /// Occurs when the visible state changes.
    /// Parameters: Invoker, Value
    /// </summary>
    public event Action<ControlNode,bool>? OnVisibleChanged;

    /// <summary>
    /// Occurs when the active state changes.
    /// Parameters: Invoker, Value
    /// </summary>
    public event Action<ControlNode,bool>? OnActiveChanged;

    /// <summary>
    /// Occurs when the parent active state changes.
    /// Parameters: Invoker, Value
    /// </summary>
    public event Action<ControlNode,bool>? OnParentActiveChanged;

    /// <summary>
    /// Occurs when the parent visible state changes.
    /// Parameters: Invoker, Value
    /// </summary>
    public event Action<ControlNode,bool>? OnParentVisibleChanged;

    /// <summary>
    /// Occurs when the mouse enters this node.
    /// Parameters: Invoker, Mouse Position
    /// </summary>
    public event Action<ControlNode,Vector2>? OnMouseEntered;

    /// <summary>
    /// Occurs when the mouse exits this node.
    /// Parameters: Invoker, Last Mouse Position Inside
    /// </summary>
    public event Action<ControlNode,Vector2>? OnMouseExited;

    /// <summary>
    /// Occurs when the selected state changes.
    /// Parameters: Invoker, Value
    /// </summary>
    public event Action<ControlNode, bool>? OnSelectedChanged;

    /// <summary>
    /// Occurs when the pressed state changes.
    /// Parameters: Invoker, Value
    /// </summary>
    public event Action<ControlNode, bool>? OnPressedChanged;

    /// <summary>
    /// Occurs when the navigable state changes.
    /// Parameters: Invoker, Value
    /// </summary>
    public event Action<ControlNode, bool>? OnNavigableChanged;

    /// <summary>
    /// Occurs when the mouse filter changes.
    /// Parameters: Invoker, Old Filter, New Filter
    /// </summary>
    public event Action<ControlNode, MouseFilter, MouseFilter>? OnMouseFilterChanged;

    /// <summary>
    /// Occurs when the selection filter changes.
    /// Parameters: Invoker, Old Filter, New Filter
    /// </summary>
    public event Action<ControlNode, SelectFilter, SelectFilter>? OnSelectionFilterChanged;

    /// <summary>
    /// Occurs when the input filter changes.
    /// Parameters: Invoker, Old Filter, New Filter
    /// </summary>
    public event Action<ControlNode, InputFilter, InputFilter>? OnInputFilterChanged;

    #endregion

    #region Private Members
    private ControlNode? parent;
    private readonly List<ControlNode> children = new();
    private SelectFilter selectionFilter = SelectFilter.None;
    private MouseFilter mouseFilter = MouseFilter.Ignore;
    private InputFilter inputFilter = InputFilter.None;
    private bool active = true;
    private bool visible = true;
    private bool parentActive = true;
    private bool parentVisible = true;
    private bool selected;
    
    private bool displayed = true;

    /// <summary>
    /// Gets or sets whether this node and its children are displayed.
    /// Changing this value will recursively update all children.
    /// Triggers <see cref="OnDisplayedChanged"/> and updates navigable and visible-in-hierarchy states.
    /// </summary>
    public bool Displayed
    {
        get => displayed;
        set
        {
            if (value == displayed) return;
            prevNavigable = Navigable;
            prevIsVisibleInHierarchy = IsVisibleInHierarchy;
            displayed = value;
            ResolveOnDisplayedChanged();
            foreach (var child in children)
            {
                child.Displayed = value;
            }
        }
    }
    
    
    private bool navigationSelected;
    private bool prevNavigable;
    private bool prevIsVisibleInHierarchy;
    private bool prevIsActiveInHierarchy;
    #endregion

    #region Public Members

    /// <summary>
    /// The anchor point used to position this node within its parent.
    /// </summary>
    public AnchorPoint Anchor = new(0f);

    /// <summary>
    /// Stretch determines the size of the rect based on the parent rect size. Values are relative and in range 0 - 1.
    /// If Stretch values are 0 then the size of the rect can be set manually without it being changed by the parent rect size.
    /// </summary>
    public Vector2 Stretch = new(1, 1);

    /// <summary>
    /// The stretch factor for the container, affecting how much space this node occupies relative to its siblings.
    /// </summary>
    public float ContainerStretch = 1f;

    /// <summary>
    /// The minimum size constraint for this node.
    /// </summary>
    public Size MinSize = new(0f);

    /// <summary>
    /// The maximum size constraint for this node.
    /// </summary>
    public Size MaxSize = new(0f);

    /// <summary>
    /// Margins applied to this node's rectangle.
    /// </summary>
    public Rect.Margins Margins = new();

    /// <summary>
    /// Gets or sets the selection filter for this node, determining how it can be selected.
    /// </summary>
    public SelectFilter SelectionFilter
    {
        get => selectionFilter;
        set
        {
            if (selectionFilter == value) return;
            prevNavigable = Navigable;
            var prev = selectionFilter;
            selectionFilter = value;
            ResolveOnSelectionFilterChanged(prev, selectionFilter);
        }
    }

    /// <summary>
    /// Gets or sets the mouse filter for this node, determining how it handles mouse input.
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

    /// <summary>
    /// Gets or sets the input filter for this node, determining how it handles input events.
    /// </summary>
    public InputFilter InputFilter
    {
        get => inputFilter;
        set
        {
            if (inputFilter == value) return;
            prevNavigable = Navigable;
            var prev = inputFilter;
            inputFilter = value;
            ResolveOnInputFilterChanged(prev, inputFilter);
            
        }
    }

    /// <summary>
    /// Optional list of children that are currently displayed. If null, all children are considered displayed.
    /// </summary>
    protected List<ControlNode>? DisplayedChildren = null;
    #endregion

    #region Getters & Setters
    
    /// <summary>
    /// Gets or sets whether this node is active.
    /// Changing this value triggers <see cref="ResolveActiveChanged"/> and may update children.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <item>
    ///     <description>parent active and change to active -> yes</description>
    ///   </item>
    ///   <item>
    ///     <description>parent active and change to inactive -> no</description>
    ///   </item>
    ///   <item>
    ///     <description>parent inactive and change to active -> yes</description>
    ///   </item>
    ///   <item>
    ///     <description>parent inactive and  change to inactive -> no</description>
    ///   </item>
    /// </list>
    /// </remarks>
    public bool Active
    {
        get => active;
        set
        {
            if (active == value) return;
            prevNavigable = Navigable;
            prevIsActiveInHierarchy = IsActiveInHierarchy;
            active = value;
            ResolveActiveChanged();
            if(parent == null || parentActive)
                ChangeChildrenActive(active);
        }
        
    }
    
    /// <summary>
    /// Gets or sets whether this node is visible.
    /// Changing this value triggers <see cref="ResolveVisibleChanged"/> and may update children.
    /// </summary>
    public bool Visible
    {
        get => visible;
        set
        {
            if (visible == value) return;
            prevNavigable = Navigable;
            prevIsVisibleInHierarchy = IsVisibleInHierarchy;
            visible = value;
            ResolveVisibleChanged();
            if(parent == null || parentVisible)
                ChangeChildrenVisible(visible);
            
        } 
    }
    
    /// <summary>
    /// Gets whether the parent node is visible.
    /// </summary>
    public bool ParentVisible => parentVisible;
    
    /// <summary>
    /// Gets whether the parent node is active.
    /// </summary>
    public bool ParentActive => parentActive;
    
    /// <summary>
    /// Gets whether this node and all its parents are visible and displayed.
    /// </summary>
    public bool IsVisibleInHierarchy => visible && parentVisible && displayed;
    
    /// <summary>
    /// Gets whether this node and all its parents are active.
    /// </summary>
    public bool IsActiveInHierarchy => active && parentActive;
    
    /// <summary>
    /// Gets the rectangle representing this node's bounds.
    /// Only settable on root nodes.
    /// Can be used if <see cref="Stretch"/> is set to zero.
    /// </summary>
    public Rect Rect { get; private set; }
    
    /// <summary>
    /// Gets whether the mouse is currently inside this node.
    /// </summary>
    public bool MouseInside { get; private set; }
    
    /// <summary>
    /// Gets the last mouse position inside this node.
    /// </summary>
    public Vector2 MouseInsidePosition { get; private set; }
    
    /// <summary>
    /// Gets whether this node is selected.
    /// Can only be set internally.
    /// </summary>
    public bool Selected
    {
        get => selected;
        private set
        {
            if (selected == value) return;
            selected = value;
            ResolveSelectedChanged();
        } 
    }
    
    /// <summary>
    /// Gets whether this node is currently pressed.
    /// </summary>
    public bool Pressed { get; private set; }
    
    /// <summary>
    /// Gets the parent node, or null if this is a root node.
    /// Can only be set internally.
    /// </summary>
    public ControlNode? Parent
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
    /// Gets whether this node has a parent.
    /// </summary>
    public bool HasParent => parent != null;
    
    /// <summary>
    /// Gets whether this node has any children.
    /// </summary>
    public bool HasChildren => children.Count > 0;
    
    /// <summary>
    /// Gets the number of displayed children.
    /// </summary>
    public int DisplayedChildrenCount => DisplayedChildren?.Count ?? 0;
    
    /// <summary>
    /// Gets whether this node has any displayed children.
    /// </summary>
    public bool HasDisplayedChildren => DisplayedChildrenCount > 0;
    
    /// <summary>
    /// Gets an enumerable of all child nodes.
    /// </summary>
    public IEnumerable<ControlNode> GetChildrenEnumerable => children;
    
    // public List<ControlNode> GetChildren(Predicate<ControlNode> match) => children.FindAll(match);
    
    /// <summary>
    /// Gets whether this node is navigable (active, visible, and has appropriate filters).
    /// </summary>
    public bool Navigable => 
        IsActiveInHierarchy && IsVisibleInHierarchy &&
        SelectionFilter is SelectFilter.All or SelectFilter.Navigation && 
        InputFilter is InputFilter.All or InputFilter.MouseNever;
    
    #endregion

    #region Children

    /// <summary>
    /// Returns the index of the specified child in the children list.
    /// </summary>
    public int GetChildIndex(ControlNode child) => children.IndexOf(child);

    /// <summary>
    /// Gets the previous child of the specified child in the children list, wrapping around if at the start.
    /// Returns null if the child is not found or the list is empty.
    /// </summary>
    public ControlNode? GetPreviousChild(ControlNode child)
    {
        if (children.Count <= 0) return null;
        var index = children.IndexOf(child);
        if (index < 0) return null;
        if (index == 0)
        {
            return children[^1];
        }
        return children[index - 1];
    }

    /// <summary>
    /// Gets the next child of the specified child in the children list, wrapping around if at the end.
    /// Returns null if the child is not found or the list is empty.
    /// </summary>
    public ControlNode? GetNextChild(ControlNode child)
    {
        if (children.Count <= 0) return null;
        var index = children.IndexOf(child);
        if (index < 0) return null;
        if (index >= children.Count - 1)
        {
            return children[0];
        }
        return children[index + 1];
    }

    /// <summary>
    /// Adds a child node to this node. Handles parent reassignment and updates hierarchy state.
    /// Returns true if the child was added, false if already present.
    /// </summary>
    public bool AddChild(ControlNode child)
    {
        if (child.Parent != null)
        {
            if (child.Parent == this) return false;
            child.Parent.RemoveChild(child);
        }
        
        children.Add(child);
        ResolveChildAdded(child);
        child.Parent = this;

        //a child can only be added if it has no parent -> if it has a parent, RemoveChild is called first
        //therefore a child that is added always has:
            //parentActive & parentVisible set to true (because it has no parent)
            //all the children are set to the active & visible value instead of parentActive & parentVisible value (again because it has no parent)
        bool checkActive = parent == null ? active : parentActive;
        bool checkVisible = parent == null ? visible : parentVisible;
        
        if (!checkActive)
        {
            child.prevIsActiveInHierarchy = child.IsActiveInHierarchy;
            child.prevNavigable = child.Navigable;
            child.parentActive = checkActive;
            child.ResolveParentActiveChanged();
            if(child.active != checkActive) child.ChangeChildrenActive(checkActive);
            
        }
        
        if (!checkVisible)
        {
            child.prevIsVisibleInHierarchy = child.IsVisibleInHierarchy;
            child.prevNavigable = child.Navigable;
            child.parentVisible = checkVisible;
            child.ResolveParentVisibleChanged();
            if(child.visible != checkVisible) child.ChangeChildrenVisible(checkVisible);
            
        }
        
        return true;
    }

    /// <summary>
    /// Removes the specified child node from this node. Updates hierarchy state accordingly.
    /// Returns true if the child was removed, false otherwise.
    /// </summary>
    public bool RemoveChild(ControlNode child)
    {
        if (child.Parent == null || child.Parent != this) return false;
        if (!children.Remove(child)) return false;
        
        ResolveChildRemoved(child);
        child.Parent = null;

        if (child.parentActive == false)
        {
            child.prevNavigable = child.Navigable;
            child.prevIsActiveInHierarchy = child.IsActiveInHierarchy;
            child.parentActive = true;
            child.ResolveParentActiveChanged();

            if (child.active) child.ChangeChildrenActive(child.active);
        }

        if (child.parentVisible == false)
        {
            child.prevNavigable = child.Navigable;
            child.prevIsVisibleInHierarchy = child.IsVisibleInHierarchy;
            child.parentVisible = true;
            child.ResolveParentVisibleChanged();
            
            if(child.visible) child.ChangeChildrenVisible(child.visible);
        }
        
        return true;

    }

    /// <summary>
    /// Removes the child node at the specified index from this node.
    /// </summary>
    private void RemoveChild(int index)
    {
        var child = children[index];
        children.RemoveAt(index);
        ResolveChildRemoved(child);
        child.Parent = null;
        child.parentActive = true; //default value if parent is null
        child.parentVisible = true; //default value if parent is null
    }

    /// <summary>
    /// Removes all child nodes from this node.
    /// </summary>
    public void ClearChildren()
    {
        for (int i = children.Count - 1; i >= 0; i--)
        {
            RemoveChild(i);
        }
    }

    /// <summary>
    /// Gets the child node at the specified index, or null if out of range.
    /// </summary>
    public ControlNode? GetChild(int index) => children.Count <= index ? null : children[index];

    /// <summary>
    /// Returns a copy of the children list.
    /// </summary>
    public List<ControlNode>? GetChildrenCopy() => children.Count <= 0 ? null :  children.ToList();

    /// <summary>
    /// Returns a list of children matching the given predicate.
    /// </summary>
    public List<ControlNode>? GetChildren(Predicate<ControlNode> match) => children.Count <= 0 ? null : children.FindAll(match);

    /// <summary>
    /// Recursively adds all children (and their descendants) to the provided result set.
    /// Returns the number of nodes added.
    /// </summary>
    public int GetAllChildren(ref HashSet<ControlNode> result)
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
    /// Recursively adds all children (and their descendants) matching the predicate to the result set.
    /// Returns the number of nodes added.
    /// </summary>
    public int GetAllChildren(Predicate<ControlNode> match, ref HashSet<ControlNode> result)
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

    /// <summary>
    /// Recursively adds all navigable children (and their descendants) to the provided set.
    /// Returns the number of nodes added.
    /// </summary>
    public int GetAllNavigableChildren(ref HashSet<ControlNode> navigable)
    {
        if (children.Count <= 0) return 0;
        if (!IsVisibleInHierarchy || !IsActiveInHierarchy) return 0;
        
        var count = navigable.Count;
        foreach (var child in children)
        {
            if(child.Navigable) navigable.Add(child);
            child.GetAllNavigableChildren(ref navigable);
        }
        return navigable.Count - count;
    }

    /// <summary>
    /// Recursively adds all children (and their descendants) that are visible in the hierarchy to the provided set.
    /// Returns the number of nodes added.
    /// </summary>
    public int GetAllVisibleInHierarchyChildren(ref HashSet<ControlNode> visibleChildren)
    {
        if (children.Count <= 0) return 0;
        if (!IsVisibleInHierarchy || !IsActiveInHierarchy) return 0;
        
        var count = visibleChildren.Count;
        foreach (var child in children)
        {
            if(child.IsVisibleInHierarchy) visibleChildren.Add(child);
            child.GetAllVisibleInHierarchyChildren(ref visibleChildren);
        }
        return visibleChildren.Count - count;
    }

    /// <summary>
    /// Recursively adds all children (and their descendants) that are active in the hierarchy to the provided set.
    /// Returns the number of nodes added.
    /// </summary>
    public int GetAllActiveInHierarchyChildren(ref HashSet<ControlNode> activeChildren)
    {
        if (children.Count <= 0) return 0;
        if (!IsVisibleInHierarchy || !IsActiveInHierarchy) return 0;
        
        var count = activeChildren.Count;
        foreach (var child in children)
        {
            if(child.IsVisibleInHierarchy) activeChildren.Add(child);
            child.GetAllActiveInHierarchyChildren(ref activeChildren);
        }
        return activeChildren.Count - count;
    }

    /// <summary>
    /// Recursively updates the parentVisible state for all children and triggers visibility change resolution.
    /// </summary>
    private void ChangeChildrenVisible(bool value)
    {
        foreach (var child in children)
        {
            child.prevNavigable = child.Navigable;
            child.prevIsVisibleInHierarchy = child.IsVisibleInHierarchy;
            child.parentVisible = value;
            child.ResolveParentVisibleChanged();
            // if(value || child.visible != value)
            if(child.visible) child.ChangeChildrenVisible(value);
        }
    }

    /// <summary>
    /// Recursively updates the parentActive state for all children and triggers active state change resolution.
    /// </summary>
    private void ChangeChildrenActive(bool value)
    {
        foreach (var child in children)
        {
            child.prevNavigable = child.Navigable;
            child.prevIsActiveInHierarchy = child.IsActiveInHierarchy;
            child.parentActive = value;
            child.ResolveParentActiveChanged();
            // if(value || child.active != value)
            if(child.active) child.ChangeChildrenActive(value);
        }
        
        //value     active      inactive    active      inactive
        //child     active      inactive    inactive    active
        //          yes         no          no          yes
    }

    #endregion

    #region Select & Deselect
    
    /// <summary>
    /// Selects this node if the selection filter allows it.
    /// Returns true if selection was successful.
    /// </summary>
    public bool Select()
    {
        if (SelectionFilter == SelectFilter.None) return false;
        Selected = true;
        return true;
    }
    
    /// <summary>
    /// Deselects this node if the selection filter allows it.
    /// Returns true if deselection was successful.
    /// </summary>
    public bool Deselect()
    {
        if (SelectionFilter == SelectFilter.None) return false;
        Selected = false;
        navigationSelected = false;
        return true;
    }
    
    /// <summary>
    /// Selects this node via navigation if allowed by selection and input filters.
    /// Returns true if navigation selection was successful.
    /// </summary>
    public bool NavigationSelect()
    {
        if (SelectionFilter is SelectFilter.Mouse or SelectFilter.None) return false;
        if (InputFilter is InputFilter.None or InputFilter.MouseOnly) return false;
        Selected = true;
        navigationSelected = true;
        return true;
    }
    
    /// <summary>
    /// Deselects this node via navigation if allowed by selection and input filters.
    /// Returns true if navigation deselection was successful.
    /// </summary>
    public bool NavigationDeselect()
    {
        if (SelectionFilter is SelectFilter.Mouse or SelectFilter.None) return false;
        if (InputFilter is InputFilter.None or InputFilter.MouseOnly) return false;
        Selected = false;
        navigationSelected = false;
        return true;
    }
    
    /// <summary>
    /// Selects this node via mouse if allowed by the selection filter.
    /// </summary>
    private void MouseSelect()
    {
        if (SelectionFilter is SelectFilter.Navigation or SelectFilter.None) return;
        Selected = true;
    }
    
    /// <summary>
    /// Deselects this node via mouse if allowed by the selection filter and not navigation selected.
    /// </summary>
    private void MouseDeselect()
    {
        if (SelectionFilter is SelectFilter.Navigation or SelectFilter.None) return;
        if (navigationSelected) return;
        Selected = false;
    }
    
    #endregion
    
    #region Update & Draw

    /// <summary>
    /// Sets the rectangle for this node, applying margins.
    /// </summary>
    /// <param name="newRect">The new rectangle to set.</param>
    public void SetRect(Rect newRect)
    {
        Rect = newRect.ApplyMargins(Margins);
    }

    /// <summary>
    /// Updates the rectangle for this node based on a source rectangle, anchor, stretch, min/max size, and margins.
    /// </summary>
    /// <param name="sourceRect">The source rectangle to base the update on.</param>
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
    /// Updates this node and its children if this is a root node.
    /// </summary>
    /// <param name="dt">Delta time since last update.</param>
    /// <param name="mousePos">Current mouse position.</param>
    public void Update(float dt, Vector2 mousePos)
    {
        if (parent != null) return;
        InternalUpdate(dt, mousePos, true);
    }

    /// <summary>
    /// Draws this node and its children if this is a root node.
    /// </summary>
    public void Draw()
    {
        if (parent != null) return;
        InternalDraw();
    }

    /// <summary>
    /// Sets the rectangle for a child node. Can be overridden for custom layout logic.
    /// </summary>
    /// <param name="child">The child node.</param>
    /// <param name="inputRect">The input rectangle.</param>
    /// <returns>The rectangle to assign to the child.</returns>
    protected virtual Rect SetChildRect(ControlNode child, Rect inputRect) => inputRect;

    /// <summary>
    /// Updates all displayed children of this node.
    /// </summary>
    /// <param name="dt">Delta time since last update.</param>
    /// <param name="mousePos">Current mouse position.</param>
    /// <param name="mousePosValid">Whether the mouse position is valid for input.</param>
    private void UpdateChildren(float dt, Vector2 mousePos, bool mousePosValid)
    {
        var iterator = DisplayedChildren ?? children;
        foreach (var child in iterator)
        {
            if(!child.displayed) continue;
            child.UpdateRect(SetChildRect(child, Rect));
            child.InternalUpdate(dt, mousePos, mousePosValid);
            OnChildUpdated(child);
        }
    }

    /// <summary>
    /// Draws all displayed children of this node.
    /// </summary>
    private void DrawChildren()
    {
        var iterator = DisplayedChildren ?? children;
        foreach (var child in iterator)
        {
            if(!child.displayed) continue;
            child.InternalDraw();
            OnChildDrawn(child);
        }
    }

    /// <summary>
    /// Internal update logic for this node, including mouse and input handling, and updating children.
    /// </summary>
    /// <param name="dt">Delta time since last update.</param>
    /// <param name="mousePos">Current mouse position.</param>
    /// <param name="mousePosValid">Whether the mouse position is valid for input.</param>
    private void InternalUpdate(float dt, Vector2 mousePos, bool mousePosValid)
    {
        //if it is not visible it should also not be updated!
        if (!IsVisibleInHierarchy) return;

        if (IsActiveInHierarchy)
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
                        if (SelectionFilter is SelectFilter.All or SelectFilter.Mouse)
                        {
                            MouseSelect();
                        }
                    }
                    else
                    {
                        ResolveMouseExited(MouseInsidePosition);
                        if (SelectionFilter is SelectFilter.All or SelectFilter.Mouse)
                        {
                            MouseDeselect();
                        }
                    }
                }

                if (isMouseInside) MouseInsidePosition = mousePos;
            
            }

            if (InputFilter != InputFilter.None)
            {
                var pressed = false;
                if (InputFilter == InputFilter.MouseOnly)
                {
                    pressed = MouseInside && GetMousePressedState();
                } 
                else if (InputFilter == InputFilter.MouseNever)
                {
                    pressed = GetPressedState();
                }
                else if (InputFilter == InputFilter.All)
                {
                    pressed = (MouseInside && GetMousePressedState()) || GetPressedState();
                }

                if (Pressed != pressed)
                {
                    Pressed = pressed;
                    ResolvePressedChanged();
                }
            
            }
        }
        
        OnUpdate(dt, mousePos, mousePosValid);
        UpdateChildren(dt, mousePos, mousePosValid);
    }

    /// <summary>
    /// Internal draw logic for this node and its children.
    /// </summary>
    private void InternalDraw()
    {
        if (!IsVisibleInHierarchy) return;
        
        OnDraw();
        DrawChildren();
    }
    #endregion

    #region Input
    /// <summary>
    /// Return if the key for the pressed state is down.
    /// Override this method to handle custom input logic.
    /// </summary>
    /// <returns>Returns false per default.</returns>
    protected virtual bool GetPressedState() => false;
    
    /// <summary>
    /// Return if the mouse button for the pressed state is down (only is called when mouse is inside).
    /// Override this method to handle custom input logic.
    /// </summary>
    /// <returns>Returns false per default.</returns>   
    protected virtual bool GetMousePressedState() => false;

    /// <summary>
    /// Return the direction to move to another element.
    /// Override this method to handle custom navigation direction.
    /// </summary>
    /// <returns>Returns empty direction per default. (0,0)</returns>
    public virtual Direction GetNavigationDirection() => new();

    /// <summary>
    /// Called when this node is navigated to in a given direction.
    /// Invokes the <see cref="OnNavigated"/> event after handling navigation logic.
    /// </summary>
    /// <param name="dir">The direction from which navigation occurred.</param>
    public void NavigatedTo(Direction dir)
    {
        HasNavigated(dir);
        OnNavigated?.Invoke(this, dir);
    }
    
    /// <summary>
    /// Handles custom logic when this node is navigated to.
    /// Can be overridden in derived classes.
    /// </summary>
    /// <param name="dir">The direction from which navigation occurred.</param>
    protected virtual void HasNavigated(Direction dir)
    {
        
    }
    #endregion

    #region Public

    /// <summary>
    /// Calculates the Euclidean distance from this node to another node.
    /// Returns -1 if the other node is the same as this node.
    /// </summary>
    /// <code>return (other.Rect.TopLeft - Rect.TopLeft).Length();</code>
    /// <param name="other">The other <see cref="ControlNode"/> to measure distance to.</param>
    /// <returns>The distance as a float, or -1 if the nodes are the same.</returns>
    public float GetDistanceTo(ControlNode other)
    {
        if (other == this) return -1f;
        return (other.Rect.TopLeft - Rect.TopLeft).Length();
    }

    /// <summary>
    /// Calculates the squared Euclidean distance from this node to another node.
    /// Returns -1 if the other node is the same as this node.
    /// </summary>
    /// <code>return (other.Rect.TopLeft - Rect.TopLeft).LengthSquared();</code>
    /// <param name="other">The other <see cref="ControlNode"/> to measure distance to.</param>
    /// <returns>The squared distance as a float, or -1 if the nodes are the same.</returns>
    public float GetDistanceSquaredTo(ControlNode other)
    {
        if (other == this) return -1f;
        return (other.Rect.TopLeft - Rect.TopLeft).LengthSquared();
    }

    /// <summary>
    /// Gets the navigation origin point for this node based on the given direction.
    /// </summary>
    /// <param name="dir">The navigation <see cref="Direction"/>.</param>
    /// <returns>The origin <see cref="Vector2"/> for navigation.</returns>
    public Vector2 GetNavigationOrigin(Direction dir) => Rect.GetPoint(dir.Invert().ToAlignement());

    #endregion
    
    #region Virtual

    /// <summary>
    /// Called during the update cycle of this node.
    /// Override to implement custom update logic.
    /// </summary>
    /// <param name="dt">Delta time since last update.</param>
    /// <param name="mousePos">Current mouse position.</param>
    /// <param name="mousePosValid">Whether the mouse position is valid for input.</param>
    protected virtual void OnUpdate(float dt, Vector2 mousePos, bool mousePosValid) { }

    /// <summary>
    /// Called after a child node has been updated.
    /// Override to implement custom logic after a child update.
    /// </summary>
    /// <param name="child">The child node that was updated.</param>
    protected virtual void OnChildUpdated(ControlNode child) { }

    /// <summary>
    /// Called during the draw cycle of this node.
    /// Override to implement custom drawing logic.
    /// </summary>
    protected virtual void OnDraw() { } 

    /// <summary>
    /// Called after a child node has been drawn.
    /// Override to implement custom logic after a child draw.
    /// </summary>
    /// <param name="child">The child node that was drawn.</param>
    protected virtual void OnChildDrawn(ControlNode child) { }

    /// <summary>
    /// Called when the active state of this node changes.
    /// </summary>
    /// <param name="value">The new active state.</param>
    protected virtual void ActiveWasChanged(bool value) { }

    /// <summary>
    /// Called when the visible state of this node changes.
    /// </summary>
    /// <param name="value">The new visible state.</param>
    protected virtual void VisibleWasChanged(bool value) { }

    /// <summary>
    /// Called when the parent active state changes.
    /// </summary>
    /// <param name="value">The new parent active state.</param>
    protected virtual void ParentActiveWasChanged(bool value) { }

    /// <summary>
    /// Called when the parent visible state changes.
    /// </summary>
    /// <param name="value">The new parent visible state.</param>
    protected virtual void ParentVisibleWasChanged(bool value) { }

    /// <summary>
    /// Called when the parent of this node changes.
    /// </summary>
    /// <param name="oldParent">The previous parent node.</param>
    /// <param name="newParent">The new parent node.</param>
    protected virtual void ParentWasChanged(ControlNode? oldParent, ControlNode? newParent) { }

    /// <summary>
    /// Called when a child node is added.
    /// </summary>
    /// <param name="newChild">The child node that was added.</param>
    protected virtual void ChildWasAdded(ControlNode newChild) { }

    /// <summary>
    /// Called when a child node is removed.
    /// </summary>
    /// <param name="oldChild">The child node that was removed.</param>
    protected virtual void ChildWasRemoved(ControlNode oldChild) { }

    /// <summary>
    /// Called when the mouse enters this node.
    /// </summary>
    /// <param name="mousePos">The mouse position at entry.</param>
    protected virtual void MouseHasEntered(Vector2 mousePos) { }

    /// <summary>
    /// Called when the mouse exits this node.
    /// </summary>
    /// <param name="mousePos">The last mouse position inside the node.</param>
    protected virtual void MouseHasExited(Vector2 mousePos) { }

    /// <summary>
    /// Called when the selected state changes.
    /// </summary>
    /// <param name="value">The new selected state.</param>
    protected virtual void SelectedWasChanged(bool value) { }

    /// <summary>
    /// Called when the pressed state changes.
    /// </summary>
    /// <param name="value">The new pressed state.</param>
    protected virtual void PressedWasChanged(bool value) { }

    /// <summary>
    /// Called when the mouse filter changes.
    /// </summary>
    /// <param name="old">The previous mouse filter.</param>
    /// <param name="cur">The current mouse filter.</param>
    protected virtual void MouseFilterWasChanged(MouseFilter old, MouseFilter cur) { }

    /// <summary>
    /// Called when the selection filter changes.
    /// </summary>
    /// <param name="old">The previous selection filter.</param>
    /// <param name="cur">The current selection filter.</param>
    protected virtual void SelectionFilterWasChanged(SelectFilter old, SelectFilter cur) { }

    /// <summary>
    /// Called when the input filter changes.
    /// </summary>
    /// <param name="old">The previous input filter.</param>
    /// <param name="cur">The current input filter.</param>
    protected virtual void InputFilterWasChanged(InputFilter old, InputFilter cur) { }

    /// <summary>
    /// Called when the navigable state changes.
    /// </summary>
    /// <param name="value">The new navigable state.</param>
    protected virtual void NavigableWasChanged(bool value) { }

    /// <summary>
    /// Called when the displayed state changes.
    /// </summary>
    /// <param name="value">The new displayed state.</param>
    protected virtual void DisplayedWasChanged(bool value) { }

    /// <summary>
    /// Called when the active-in-hierarchy state changes.
    /// </summary>
    /// <param name="value">The new active-in-hierarchy state.</param>
    protected virtual void ActiveInHierarchyChanged(bool value) { }

    /// <summary>
    /// Called when the visible-in-hierarchy state changes.
    /// </summary>
    /// <param name="value">The new visible-in-hierarchy state.</param>
    protected virtual void VisibleInHierarchyChanged(bool value) { }

    #endregion

    #region Private
    private void ResolveOnActiveInHierarchyChanged()
    {
        if (prevIsActiveInHierarchy == IsActiveInHierarchy) return;
        ActiveInHierarchyChanged(IsActiveInHierarchy);
        OnActiveInHierarchyChanged?.Invoke(this, IsActiveInHierarchy);
    }

    private void ResolveOnVisibleInHierarchyChanged()
    {
        if (prevIsVisibleInHierarchy == IsVisibleInHierarchy) return;
        VisibleInHierarchyChanged(IsVisibleInHierarchy);
        OnVisibleInHierarchyChanged?.Invoke(this, IsVisibleInHierarchy);
    }
    private void ResolveOnDisplayedChanged()
    {
        DisplayedWasChanged(displayed);
        OnDisplayedChanged?.Invoke(this, displayed);
        ResolveOnNavigableChanged();
        ResolveOnVisibleInHierarchyChanged();
    }
    private void ResolveActiveChanged()
    {
        ActiveWasChanged(active);
        OnActiveChanged?.Invoke(this, active);
        ResolveOnNavigableChanged();
        ResolveOnActiveInHierarchyChanged();
    }
    private void ResolveVisibleChanged()
    {
        VisibleWasChanged(visible);
        OnVisibleChanged?.Invoke(this, visible);
        ResolveOnNavigableChanged();
        ResolveOnVisibleInHierarchyChanged();
    }
    private void ResolveParentVisibleChanged()
    {
        ParentVisibleWasChanged(parentVisible);
        OnParentVisibleChanged?.Invoke(this, parentVisible);
        ResolveOnNavigableChanged();
        ResolveOnVisibleInHierarchyChanged();
    }
    private void ResolveParentActiveChanged()
    {
        ParentActiveWasChanged(parentActive);
        OnParentActiveChanged?.Invoke(this, parentActive);
        ResolveOnNavigableChanged();
        ResolveOnActiveInHierarchyChanged();
    }
    private void ResolveParentChanged(ControlNode? oldParent, ControlNode? newParent)
    {
        ParentWasChanged(oldParent, newParent);
        OnParentChanged?.Invoke(this, oldParent, newParent);
        // ResolveOnNavigableChanged(Navigable);
    }
    private void ResolveChildAdded(ControlNode newChild)
    {
        ChildWasAdded(newChild);
        OnChildAdded?.Invoke(this, newChild);
    }
    private void ResolveChildRemoved(ControlNode oldChild)
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
    private void ResolveSelectedChanged()
    {
        // Console.WriteLine($"Selected Changed to {selected} in {this.Anchor}");
        SelectedWasChanged(selected);
        OnSelectedChanged?.Invoke(this, selected);
    }
    private void ResolvePressedChanged()
    {
        PressedWasChanged(Pressed);
        OnPressedChanged?.Invoke(this, Pressed);
    }
    private void ResolveOnMouseFilterChanged(MouseFilter old, MouseFilter cur)
    {
        MouseFilterWasChanged(old, cur);
        OnMouseFilterChanged?.Invoke(this, old, cur);
    }
    private void ResolveOnSelectionFilterChanged(SelectFilter old, SelectFilter cur)
    {
        SelectionFilterWasChanged(old, cur);
        OnSelectionFilterChanged?.Invoke(this, old, cur);
        ResolveOnNavigableChanged();
    }
    private void ResolveOnInputFilterChanged(InputFilter old, InputFilter cur)
    {
        InputFilterWasChanged(old, cur);
        OnInputFilterChanged?.Invoke(this, old, cur);
        ResolveOnNavigableChanged();
    }
    private void ResolveOnNavigableChanged()
    {
        if (prevNavigable == Navigable) return;
        NavigableWasChanged(Navigable);
        OnNavigableChanged?.Invoke(this, Navigable);
    }
    #endregion
    
}