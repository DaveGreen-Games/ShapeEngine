using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.UI;

/// <summary>
/// Represents a container for <see cref="ControlNode"/> elements, managing their layout, navigation, and display logic.
/// </summary>
/// <remarks>
/// Supports grid and linear layouts, navigation, and paging. Handles child visibility and selection events.
/// </remarks>
public class ControlNodeContainer : ControlNode
{
    /// <summary>
    /// Occurs when the first node in the container is selected.
    /// <list type="bullet">
    /// <item><description><c>ControlNode</c>: The container instance.</description></item>
    /// <item><description><c>ControlNode</c>: The selected node.</description></item>
    /// </list>
    /// </summary>
    public event Action<ControlNode, ControlNode>? OnFirstNodeSelected;
    /// <summary>
    /// Occurs when the last node in the container is selected.
    /// <list type="bullet">
    /// <item><description><c>ControlNode</c>: The container instance.</description></item>
    /// <item><description><c>ControlNode</c>: The selected node.</description></item>
    /// </list>
    /// </summary>
    public event Action<ControlNode, ControlNode>? OnLastNodeSelected;
    /// <summary>
    /// Occurs when any node in the container is selected.
    /// <list type="bullet">
    /// <item><description><c>ControlNode</c>: The container instance.</description></item>
    /// <item><description><c>ControlNode</c>: The selected node.</description></item>
    /// </list>
    /// </summary>
    public event Action<ControlNode, ControlNode>? OnNodeSelected;

    private Grid grid = new();

    /// <summary>
    /// Gets or sets the <see cref="Grid"/> layout for the container.
    /// </summary>
    public Grid Grid
    {
        get => grid;
        set
        {
            if (grid == value) return;
            grid = value;
            if (grid.Count < 0)
            {
                displayIndex = 0;
            }
            dirty = true;
        }
    }

    /// <summary>
    /// If true, the display index of the container is clamped to <c>childCount - displayCount</c>.
    /// If false, the display index is incremented/decremented by displayCount as long as 0 or childCount is not reached.
    /// </summary>
    public bool AlwaysKeepFilled = true;
    /// <summary>
    /// Gets the number of elements to display based on the grid configuration.
    /// </summary>
    private int DisplayCount => grid.Count;
    /// <summary>
    /// Gets or sets the index of the first displayed child node.
    /// </summary>
    public int DisplayIndex
    {
        get => displayIndex;
        set
        {
            if (value == displayIndex) return;
            dirty = true;
            
            if (value < 0)
            {
                displayIndex = 0;
                return;
            }

            if (AlwaysKeepFilled)
            {
                int maxValue = DisplayCount > ChildCount ? 0 : ChildCount - DisplayCount;
                displayIndex = value > maxValue ? maxValue : value;
                
            }
            else
            {
                if (value > ChildCount) return;
                displayIndex = value;
            }
            

        }
    }

    private int navigationStep = 1;
    /// <summary>
    /// Gets or sets the navigation step size for moving between nodes.
    /// </summary>
    public int NavigationStep
    {
        get => navigationStep;
        set
        {
            if (value == navigationStep) return;
            if (value < 0) return;
            navigationStep = value;
        }
    }
    /// <summary>
    /// Gets or sets the gap between elements in the container as a <see cref="Vector2"/>.
    /// </summary>
    public Vector2 Gap { get; set; } = new();

    #region Private Members
    private int displayIndex = 0;
    
    private bool dirty = false;
    
    private Vector2 curOffset = new();
    private Size gapSize = new();
    private Vector2 startPos = new();
    private Size elementSize = new();
    private Vector2 direction = new();
    private AnchorPoint alignement = new();
    #endregion
    
    #region Override

    /// <summary>
    /// Updates the container and recalculates layout if necessary.
    /// </summary>
    /// <param name="dt">Delta time since last update.</param>
    /// <param name="mousePos">Current mouse position.</param>
    /// <param name="mousePosValid">Indicates if the mouse position is valid.</param>
    protected override void OnUpdate(float dt, Vector2 mousePos, bool mousePosValid)
    {
        if (!Grid.IsValid) return;
        if(dirty) CompileDisplayedNodes();

        var stretchFactorTotal = 0f;
        if (!grid.IsGrid)
        {
            int count = DisplayCount <= 0 ? DisplayedChildrenCount : DisplayCount;
            for (var i = 0; i < count; i++)
            {
                if (i < DisplayedChildrenCount)
                {
                    stretchFactorTotal += DisplayedChildren != null ? DisplayedChildren[i].ContainerStretch : 0;
                }
                else stretchFactorTotal += 1;
            }

        }
        
        startPos = Rect.GetPoint(Grid.Placement.Invert().ToAlignement()); // Rect.TopLeft;

        float horizontalDivider = Grid.IsGrid ? Grid.Cols : Grid.IsHorizontal ? stretchFactorTotal : 1f;
        float verticalDivider = Grid.IsGrid ? Grid.Rows : Grid.IsVertical ? stretchFactorTotal : 1f;
        
        int hGaps = Grid.Cols - 1;
        float totalWidth = Rect.Width;
        float hGapSize = totalWidth * Gap.X;
        float elementWidth = (totalWidth - hGaps * hGapSize) / horizontalDivider;

        int vGaps = Grid.Rows - 1;
        float totalHeight = Rect.Height;
        float vGapSize = totalHeight * Gap.Y;
        float elementHeight = (totalHeight - vGaps * vGapSize) / verticalDivider;

        gapSize = new(hGapSize, vGapSize);
        elementSize = new(elementWidth, elementHeight);
        direction = Grid.Placement.ToVector2();
        alignement = Grid.Placement.Invert().ToAlignement();
        curOffset = new(0f, 0f);


    }
    /// <summary>
    /// Sets the rectangle for a child node based on the current layout.
    /// </summary>
    /// <param name="node">The child node.</param>
    /// <param name="inputRect">The input rectangle.</param>
    /// <returns>The calculated rectangle for the child node.</returns>
    protected override Rect SetChildRect(ControlNode node, Rect inputRect)
    {
        if (!Grid.IsValid) return inputRect;
        if (DisplayedChildren == null) return inputRect;
        
        if (grid.IsGrid)
        {
            int i = DisplayedChildren.IndexOf(node);
            if (i < 0) return inputRect;
            var coords = Grid.IndexToCoordinates(i);
            var r = new Rect
            (
                startPos + ((gapSize + elementSize) * coords.ToVector2() * direction),
                elementSize,
                alignement
            );

            return r;
        }
        else
        {
            var size = new Size
            (
                grid.IsVertical ? elementSize.Width : elementSize.Width * node.ContainerStretch,
                grid.IsHorizontal ? elementSize.Height : elementSize.Height * node.ContainerStretch
            );
            var clampedSize = new Size
            (
                node.MaxSize.Width > 0 ? MathF.Min(size.Width, MaxSize.Width) : size.Width,
                node.MaxSize.Height > 0 ? MathF.Min(size.Height, MaxSize.Height) : size.Height
            );
            var r = new Rect
            (
                startPos + curOffset, 
                clampedSize,
                alignement
            );

            curOffset += (gapSize + size) * direction;
            return r;
        }
    }
    /// <summary>
    /// Handles logic when a child node is added to the container.
    /// </summary>
    /// <param name="newChild">The newly added child node.</param>
    protected override void ChildWasAdded(ControlNode newChild)
    {
        dirty = true;
        newChild.OnSelectedChanged += OnChildSelectionChanged;
        newChild.OnVisibleInHierarchyChanged += OnChildVisibleInHierarchyChanged;
        newChild.OnNavigated += OnChildNavigated;
    }
    /// <summary>
    /// Handles logic when a child node is removed from the container.
    /// </summary>
    /// <param name="oldChild">The removed child node.</param>
    protected override void ChildWasRemoved(ControlNode oldChild)
    {
        dirty = true;
        oldChild.OnSelectedChanged -= OnChildSelectionChanged;
        oldChild.OnVisibleInHierarchyChanged -= OnChildVisibleInHierarchyChanged;
        oldChild.OnNavigated -= OnChildNavigated;
        oldChild.Displayed = true;

    }

    #endregion
    
    #region Virtual
    /// <summary>
    /// Handles navigation input for a child node.
    /// </summary>
    /// <param name="child">The child node that was navigated from.</param>
    /// <param name="dir">The direction of navigation.</param>
    protected virtual void OnChildNavigated(ControlNode child, Direction dir)
    {
        if (NavigationStep == 0) return;

        if (!Grid.IsValid) return;

        var signedPlacement = grid.Placement.Signed;
        dir *= signedPlacement;
        
        if (Grid.IsGrid)
        {
            if (!Grid.IsValid) return;
            if (DisplayedChildren == null) return;
            var index = DisplayedChildren.IndexOf(child);
            var coords = Grid.IndexToCoordinates(index);
            var coordsDir = Grid.GetDirection(coords);
            if ((coordsDir.Vertical != 0 && coordsDir.Vertical == dir.Vertical) || (coordsDir.Horizontal != 0 && coordsDir.Horizontal == dir.Horizontal))
            {
                //left to right setup
                if (Grid.IsLeftToRightFirst)
                {
                    if (dir.IsLeft) DisplayIndex -= 1;
                    else if (dir.IsUp) DisplayIndex -= Grid.Cols;
                    else if (dir.IsUpLeft) DisplayIndex -= (Grid.Cols + 1);
                
                    if (dir.IsRight) DisplayIndex += 1;
                    else if (dir.IsDown) DisplayIndex += Grid.Cols;
                    else if (dir.IsDownRight) DisplayIndex += (Grid.Cols + 1);
                }
                else
                {
                    if (dir.IsLeft) DisplayIndex -= Grid.Rows; 
                    else if (dir.IsUp) DisplayIndex -= 1;
                    else if (dir.IsUpLeft) DisplayIndex -= (Grid.Rows + 1);
                
                    if (dir.IsRight) DisplayIndex += Grid.Rows;
                    else if (dir.IsDown) DisplayIndex += 1; 
                    else if (dir.IsDownRight) DisplayIndex += (Grid.Rows + 1);
                }
            }
        }
        else
        {
            if (IsFirstDisplayed(child))
            {
                if(dir.IsLeft || dir.IsUp || dir.IsUpLeft || dir.IsUpRight)
                {
                    DisplayIndex -= NavigationStep;
                }
            }
            else if (IsLastDisplayed(child))
            {
                if(dir.IsRight || dir.IsDown || dir.IsDownRight || dir.IsDownLeft)
                {
                    if (displayIndex + DisplayCount < ChildCount)
                    {
                        DisplayIndex += NavigationStep;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Called when the first node is selected. Override to provide custom logic.
    /// </summary>
    /// <param name="node">The selected node.</param>
    protected virtual void FirstNodeWasSelected(ControlNode node) { }
    /// <summary>
    /// Called when the last node is selected. Override to provide custom logic.
    /// </summary>
    /// <param name="node">The selected node.</param>
    protected virtual void LastNodeWasSelected(ControlNode node) { }
    /// <summary>
    /// Called when any node is selected. Override to provide custom logic.
    /// </summary>
    /// <param name="node">The selected node.</param>
    protected virtual void NodeWasSelected(ControlNode node) { }
    /// <summary>
    /// Determines if the specified node is the first displayed node.
    /// </summary>
    /// <param name="node">The node to check.</param>
    /// <returns>True if the node is the first displayed; otherwise, false.</returns>
    protected virtual bool IsFirstDisplayed(ControlNode node)
    {
        if (DisplayedChildren == null || DisplayedChildren.Count <= 0) return false;
        for (int i = 0; i < DisplayedChildren.Count; i++)
        {
            var child = DisplayedChildren[i];
            if (!child.Active || !child.ParentActive) continue;

            return child == node;
        }

        return false;
    }
    /// <summary>
    /// Determines if the specified node is the last displayed node.
    /// </summary>
    /// <param name="node">The node to check.</param>
    /// <returns>True if the node is the last displayed; otherwise, false.</returns>
    protected virtual bool IsLastDisplayed(ControlNode node)
    {
        if (DisplayedChildren == null || DisplayedChildren.Count <= 0) return false;
        for (int i = DisplayedChildren.Count - 1; i >= 0; i--)
        {
            var child = DisplayedChildren[i];
            if (!child.Active || !child.ParentActive) continue;

            return child == node;
        }

        return false;
    }
    #endregion

    #region Public
    /// <summary>
    /// Moves to the next item in the container.
    /// </summary>
    public void NextItem() => DisplayIndex += 1;
    /// <summary>
    /// Moves to the previous item in the container.
    /// </summary>
    public void PreviousItem() => DisplayIndex -= 1;

    /// <summary>
    /// Gets the maximum number of pages based on the display count and child count.
    /// </summary>
    public int MaxPages => DisplayCount <= 0 ? 1 : (int)MathF.Ceiling((float)ChildCount / (float)DisplayCount);
    /// <summary>
    /// Gets the current page number.
    /// </summary>
    public int CurPage => DisplayCount <= 0 ? 1 : ((DisplayIndex + DisplayCount - 1) / DisplayCount) + 1;
    /// <summary>
    /// Moves to the next page.
    /// </summary>
    /// <param name="wrapAround">If true, wraps to the first page when the end is reached.</param>
    public void NextPage(bool wrapAround = false)
    {
        if (DisplayCount <= 0) return;
        
        if(wrapAround && CurPage >= MaxPages) FirstPage();
        
        else DisplayIndex += DisplayCount;
    }
    /// <summary>
    /// Moves to the previous page.
    /// </summary>
    /// <param name="wrapAround">If true, wraps to the last page when the beginning is reached.</param>
    public void PrevPage(bool wrapAround = false)
    {
        if (DisplayCount <= 0) return;
        
        if(wrapAround && CurPage <= 1) LastPage();
        else DisplayIndex -= DisplayCount;
    }
    /// <summary>
    /// Moves the display by a specified number of pages.
    /// </summary>
    /// <param name="pages">The number of pages to move. Positive for forward, negative for backward.</param>
    public void MovePage(int pages)
    {
        if (pages == 0) return;
        if (DisplayCount <= 0) return;
        DisplayIndex += DisplayCount * pages;
    }

    /// <summary>
    /// Moves to the first page.
    /// </summary>
    public void FirstPage() => DisplayIndex = 0;
    /// <summary>
    /// Moves to the last page.
    /// </summary>
    public void LastPage() => DisplayIndex = ChildCount - DisplayCount;

    #endregion
    
    #region Private
    /// <summary>
    /// Compiles the list of currently displayed child nodes based on visibility and display index.
    /// </summary>
    private void CompileDisplayedNodes()
    {
        dirty = false;
        DisplayedChildren ??= new();
        DisplayedChildren.Clear();

        var visibleChildren = GetChildren((node => node.Visible && node.ParentVisible));

        if (visibleChildren == null) return;
        
        for (var i = 0; i < visibleChildren.Count; i++)
        {
            var child = visibleChildren[i];
            if (DisplayCount == 0) child.Displayed = false;
            else if (DisplayCount > 0)
            {
                if (i < DisplayIndex) child.Displayed = false;
                else if (i >= DisplayIndex + DisplayCount) child.Displayed = false;
                else
                {
                    DisplayedChildren.Add(child);
                    child.Displayed = true;
                }

            }
            else
            {
                child.Displayed = true;
                DisplayedChildren.Add(child);
            }
        }
    }

    /// <summary>
    /// Handles changes in a child's visibility within the hierarchy.
    /// </summary>
    /// <param name="node">The child node.</param>
    /// <param name="value">The new visibility value.</param>
    private void OnChildVisibleInHierarchyChanged(ControlNode node, bool value)
    {
        dirty = true;
    }
    /// <summary>
    /// Handles changes in a child's selection state.
    /// </summary>
    /// <param name="child">The child node.</param>
    /// <param name="selected">Whether the child is selected.</param>
    private void OnChildSelectionChanged(ControlNode child, bool selected)
    {
        if(selected) ResolveOnNodeSelected(child);
    }
    /// <summary>
    /// Resolves the event when the first node is selected.
    /// </summary>
    /// <param name="node">The selected node.</param>
    private void ResolveOnFirstNodeSelected(ControlNode node)
    {
        FirstNodeWasSelected(node);
        OnFirstNodeSelected?.Invoke(this, node);
    }
    /// <summary>
    /// Resolves the event when the last node is selected.
    /// </summary>
    /// <param name="node">The selected node.</param>
    private void ResolveOnLastNodeSelected(ControlNode node)
    {
        LastNodeWasSelected(node);
        OnLastNodeSelected?.Invoke(this, node);
    }
    /// <summary>
    /// Resolves the event when any node is selected.
    /// </summary>
    /// <param name="node">The selected node.</param>
    private void ResolveOnNodeSelected(ControlNode node)
    {
        if(IsFirstDisplayed(node)) ResolveOnFirstNodeSelected(node);
        else if(IsLastDisplayed(node)) ResolveOnLastNodeSelected(node);
        
        NodeWasSelected(node);
        OnNodeSelected?.Invoke(this, node);
        
    }
    #endregion
}