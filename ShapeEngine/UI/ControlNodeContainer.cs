using System.Numerics;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.UI;

public class ControlNodeContainer : ControlNode
{
    // public enum ContainerType
    // {
    //     None = 0,
    //     Horizontal = 1,
    //     Vertical = 2,
    //     Grid = 3
    // }

    public event Action<ControlNode, ControlNode>? OnFirstNodeSelected;
    public event Action<ControlNode, ControlNode>? OnLastNodeSelected;
    public event Action<ControlNode, ControlNode>? OnNodeSelected;


    private Grid grid = new();

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

    private int DisplayCount => grid.Count;
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

            int maxValue = DisplayCount > ChildCount ? 0 : ChildCount - DisplayCount;
            displayIndex = value > maxValue ? maxValue : value;

        }
    }

    private int navigationStep = 1;
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
    
    // private ContainerType type = ContainerType.None;
    // public ContainerType Type
    // {
    //     get => type;
    //     set
    //     {
    //         if (value == type) return;
    //         dirty = true;
    //         type = value;
    //     } 
    // }
    public Vector2 Gap { get; set; } = new();

    

    #region Private Members
    private int displayCount = -1;
    private int displayIndex = 0;
    // private int gridRows = 1;
    // private int gridColumns = 1;

    // private readonly List<ControlNode> displayedNodes = new();
    
    private bool dirty = false;
    
    private Vector2 curOffset = new();
    private Vector2 gapSize = new();
    private Vector2 startPos = new();
    private Vector2 elementSize = new();
    private Vector2 direction = new();
    private Vector2 alignement = new();
    #endregion
    
    #region Override

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
                    stretchFactorTotal += DisplayedChildren[i].ContainerStretch;
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
        // gapSize = new(hGapSize + elementWidth, vGapSize + elementHeight);
        elementSize = new(elementWidth, elementHeight);
        direction = Grid.Placement.ToVector2();
        // direction = new
        //     (
        //         Grid.IsGrid || Grid.IsHorizontal ? Grid.Placement.Horizontal : 0f,
        //         Grid.IsGrid || Grid.IsVertical ? Grid.Placement.Vertical : 0f
        //     );
        alignement = Grid.Placement.Invert().ToAlignement();
        curOffset = new(0f, 0f);

        // if (Grid.IsVertical)
        // {
        //     startPos = Rect.TopLeft;
        //     float stretchFactorTotal = 0f;
        //     int count = DisplayCount <= 0 ? DisplayedChildrenCount : DisplayCount;
        //     for (int i = 0; i < count; i++)
        //     {
        //         if (i < DisplayedChildrenCount)
        //         {
        //             stretchFactorTotal += DisplayedChildren[i].ContainerStretch;
        //         }
        //         else stretchFactorTotal += 1;
        //     }
        //     int gaps = count - 1;
        //
        //     float totalHeight = Rect.Height;
        //     gapSize = new(0f, totalHeight * Gap.Y);
        //     float elementHeight = (totalHeight - gaps * gapSize.Y) / stretchFactorTotal;
        //     elementSize = new(0f, elementHeight);
        //     curOffset = new(0f, 0f);
        // }
        //
        // if (Grid.IsHorizontal)
        // {
        //     startPos = Rect.TopLeft;
        //     var stretchFactorTotal = 0f;
        //     int count = DisplayCount <= 0 ? DisplayedChildrenCount : DisplayCount;
        //     for (var i = 0; i < count; i++)
        //     {
        //         if (i < DisplayedChildrenCount)
        //         {
        //             stretchFactorTotal += DisplayedChildren[i].ContainerStretch;
        //         }
        //         else stretchFactorTotal += 1;
        //     }
        //     int gaps = count - 1;
        //
        //     float totalWidth = Rect.Width;
        //     gapSize = new(totalWidth * Gap.X, 0f);
        //     float elementWidth = (totalWidth - gaps * gapSize.X) / stretchFactorTotal;
        //     elementSize = new(elementWidth, 0f);
        //     curOffset = new(0f, 0f);
        // }
        //
        // if (Grid.IsGrid)
        // {
        //     startPos = Rect.GetPoint(Grid.Placement.Invert().ToAlignement()); // Rect.TopLeft;
        //
        //     int hGaps = Grid.Cols - 1;
        //     float totalWidth = Rect.Width;
        //     float hGapSize = totalWidth * Gap.X;
        //     float elementWidth = (totalWidth - hGaps * hGapSize) / Grid.Cols;
        //
        //     int vGaps = Grid.Rows - 1;
        //     float totalHeight = Rect.Height;
        //     float vGapSize = totalHeight * Gap.Y;
        //     float elementHeight = (totalHeight - vGaps * vGapSize) / Grid.Rows;
        //
        //     gapSize = new(hGapSize + elementWidth, vGapSize + elementHeight);
        //     elementSize = new(elementWidth, elementHeight);
        //     
        //     
        // }


    }
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
                startPos + ((gapSize + elementSize) * coords.ToVector2() * direction), //  new Vector2(gapSize.X * coords.Col, 0f) + new Vector2(0f, gapSize.Y * coords.Row), 
                elementSize,
                alignement
            );

            return r;
        }
        else
        {
            var size = new Vector2
            (
                grid.IsVertical ? elementSize.X : elementSize.X * node.ContainerStretch,
                grid.IsHorizontal ? elementSize.Y : elementSize.Y * node.ContainerStretch
            );
            var clampedSize = new Vector2
            (
                node.MaxSize.X > 0 ? MathF.Min(size.X, MaxSize.X) : size.X,
                node.MaxSize.Y > 0 ? MathF.Min(size.Y, MaxSize.Y) : size.Y
            );
            var r = new Rect
            (
                startPos + curOffset, // gapSize * coords.ToVector2() * direction, //  new Vector2(gapSize.X * coords.Col, 0f) + new Vector2(0f, gapSize.Y * coords.Row), 
                clampedSize,
                alignement
            );

            curOffset += (gapSize + size) * direction;
            return r;
        }
        
        
        
        // if (Grid.IsVertical)
        // {
        //     float height = elementSize.Y * node.ContainerStretch;
        //     var size = new Vector2(Rect.Width, height);
        //     var maxSize = node.MaxSize;
        //     if (maxSize.X > 0f) size.X = MathF.Min(size.X, maxSize.X);
        //     if (maxSize.Y > 0f) size.Y = MathF.Min(size.Y, maxSize.Y);
        //     var r = new Rect(startPos + curOffset, size, new(0f));
        //     curOffset += new Vector2(0f, gapSize.Y + height);
        //     return r;
        // }
        //
        // if (Grid.IsHorizontal)
        // {
        //     float width = elementSize.X * node.ContainerStretch;
        //     Vector2 size = new(width, Rect.Height);
        //     var maxSize = node.MaxSize;
        //     if (maxSize.X > 0f) size.X = MathF.Min(size.X, maxSize.X);
        //     if (maxSize.Y > 0f) size.Y = MathF.Min(size.Y, maxSize.Y);
        //     var r = new Rect(startPos + curOffset, size, new(0f));
        //     curOffset += new Vector2(gapSize.X + width, 0f);
        //     return r;
        // }
        //
        // if (Grid.IsGrid)
        // {
        //     // int count = GridColumns * GridRows;
        //     // if (displayedNodes.Count < count) count = displayedNodes.Count;
        //     if (DisplayedChildren == null) return inputRect;
        //     if (!Grid.IsValid) return inputRect;
        //     int i = DisplayedChildren.IndexOf(node);
        //     if (i < 0) return inputRect;
        //     var coords = Grid.IndexToCoordinates(i); // ShapeMath.TransformIndexToCoordinates(i, GridRows, GridColumns, true);
        //     var r = new Rect
        //     (
        //         startPos + new Vector2(gapSize.X * coords.Col, 0f) + new Vector2(0f, gapSize.Y * coords.Row), 
        //         elementSize,
        //         new(0f)
        //     );
        //     return r;
        // }
        //
        // return inputRect;
    }
    protected override void ChildWasAdded(ControlNode newChild)
    {
        dirty = true;
        newChild.OnSelectedChanged += OnChildSelectionChanged;
        newChild.OnVisibleInHierarchyChanged += OnChildVisibleInHierarchyChanged;
        newChild.OnNavigated += OnChildNavigated;
        // newChild.OnVisibleChanged += OnChildVisibleChanged;
        // newChild.OnParentVisibleChanged += OnChildParentVisibleChanged;
    }
    protected override void ChildWasRemoved(ControlNode oldChild)
    {
        dirty = true;
        oldChild.OnSelectedChanged -= OnChildSelectionChanged;
        oldChild.OnVisibleInHierarchyChanged -= OnChildVisibleInHierarchyChanged;
        oldChild.OnNavigated -= OnChildNavigated;
        // oldChild.OnVisibleChanged -= OnChildVisibleChanged;
        // oldChild.OnParentVisibleChanged -= OnChildParentVisibleChanged;
        // oldChild.SetDisplayed(this, true);
        oldChild.Displayed = true;

    }

    #endregion
    
    #region Virtual
    protected virtual void OnChildNavigated(ControlNode child, Direction dir)
    {
        if (NavigationStep == 0) return;

        if (!Grid.IsValid) return;
        
        var signedPlacement = grid.Placement.Signed;
        // var factor = grid.IsVertical ? signedPlacement.Vertical : signedPlacement.Horizontal;
        dir *= signedPlacement;
        
        if (Grid.IsGrid)
        {
            if (!Grid.IsValid) return;
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
            
            
            if (IsFirst(child))
            {
                if(dir.IsLeft || dir.IsUp || dir.IsUpLeft || dir.IsUpRight)
                {
                    // var signedPlacement = grid.Placement.Signed;
                    // var factor = grid.IsVertical ? signedPlacement.Vertical : signedPlacement.Horizontal;
                    DisplayIndex -= NavigationStep;
                }
            }
            else if (IsLast(child))
            {
                if(dir.IsRight || dir.IsDown || dir.IsDownRight || dir.IsDownLeft)
                {
                    // var signedPlacement = grid.Placement.Signed;
                    // var factor = grid.IsVertical ? signedPlacement.Vertical : signedPlacement.Horizontal;
                    DisplayIndex += NavigationStep;
                }
            }
        }
    }

    protected virtual void FirstNodeWasSelected(ControlNode node) { }
    protected virtual void LastNodeWasSelected(ControlNode node) { }
    protected virtual void NodeWasSelected(ControlNode node) { }
    protected virtual bool IsFirst(ControlNode node)
    {
        if (DisplayedChildrenCount <= 0) return false;
        return DisplayedChildren[0] == node;
    }
    protected virtual bool IsLast(ControlNode node)
    {
        if (DisplayedChildrenCount <= 0) return false;
        return DisplayedChildren[^1] == node;
    }
    #endregion

    #region Public
    public void NextItem() => DisplayIndex += 1;
    public void PreviousItem() => DisplayIndex -= 1;

    public void NextPage()
    {
        if (DisplayCount <= 0) return;
        
        DisplayIndex += DisplayCount;
    }
    public void PrevPage()
    {
        if (DisplayCount <= 0) return;
        DisplayIndex -= DisplayCount;
    }
    public void MovePage(int pages)
    {
        if (pages == 0) return;
        if (DisplayCount <= 0) return;
        DisplayIndex += DisplayCount * pages;
    }

    public void FirstPage() => DisplayIndex = 0;
    public void LastPage() => DisplayIndex = ChildCount - DisplayCount;

    #endregion
    
    #region Private
    private void CompileDisplayedNodes()
    {
        dirty = false;
        DisplayedChildren ??= new();
        DisplayedChildren.Clear();

        var visibleChildren = GetChildren((node => node.Visible && node.ParentVisible));

        if (visibleChildren == null) return;
        
        if (DisplayCount > 0)
        {
            if (displayIndex + DisplayCount > visibleChildren.Count)
            {
                displayIndex = visibleChildren.Count - DisplayCount;
            }
        }
        
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
    // private void OnChildVisibleChanged(ControlNode child, bool visible)
    // {
    //     dirty = true;
    // }
    // private void OnChildParentVisibleChanged(ControlNode child, bool parentVisible)
    // {
    //     dirty = true;
    // }

    private void OnChildVisibleInHierarchyChanged(ControlNode node, bool value)
    {
        dirty = true;
    }
    private void OnChildSelectionChanged(ControlNode child, bool selected)
    {
        if(selected) ResolveOnNodeSelected(child);
    }
    private void ResolveOnFirstNodeSelected(ControlNode node)
    {
        FirstNodeWasSelected(node);
        OnFirstNodeSelected?.Invoke(this, node);

        
        // if (displayCount >= 0 && type is ContainerType.Horizontal or ContainerType.Vertical)
        // {
        //     DisplayIndex -= 1;
        // }
    }
    private void ResolveOnLastNodeSelected(ControlNode node)
    {
        LastNodeWasSelected(node);
        OnLastNodeSelected?.Invoke(this, node);

        // if (displayCount >= 0 && type is ContainerType.Horizontal or ContainerType.Vertical)
        // {
        //     DisplayIndex += 1;
        // }
    }
    private void ResolveOnNodeSelected(ControlNode node)
    {
        if(IsFirst(node)) ResolveOnFirstNodeSelected(node);
        else if(IsLast(node)) ResolveOnLastNodeSelected(node);
        
        NodeWasSelected(node);
        OnNodeSelected?.Invoke(this, node);
        
    }
    #endregion
}