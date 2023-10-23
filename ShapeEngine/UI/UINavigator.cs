using System.Numerics;
using Raylib_CsLo;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Input;
using ShapeEngine.Lib;
using ShapeEngine.Screen;

namespace ShapeEngine.UI;

public abstract class SceneElement
{
    private SceneElement? parent = null;
    private HashSet<SceneElement> children = new();

    private bool visible = true;
    public bool Visible
    {
        get => visible;
        set
        {
            if (value == visible) return;
            visible = value;
            OnVisibledChanged(value);
        }
    }

    private bool active = true;
    public bool Active
    {
        get => active;
        set
        {
            if (active == value) return;
            active = value;
            OnActiveChanged(value);
        }
    }

    public bool SetParent(SceneElement? newParent)
    {
        if (newParent == parent) return false;
        
        var prevParent = parent;
        parent = newParent;
        OnParentChanged(prevParent, newParent);
        return true;
    }
    public bool AddChild(SceneElement child)
    {
        if (!children.Add(child)) return false;
        SetParent(this);
        return true;
    }
    public bool RemoveChild(SceneElement child)
    {
        if (!children.Remove(child)) return false;
        SetParent(null);
        return true;
    }
    public bool TransferChildren(SceneElement from, SceneElement to)
    {
        if (from == to) return false;
        foreach (var child in from.children)
        {
            to.AddChild(child);
        }
        from.children = new();

        return true;
    }
    internal void Activate(SceneElement old)
    {
        OnActivated(old);
        foreach (var child in children)
        {
            child.Activate(old);
        }
    }
    internal void Deactivate()
    {
        OnDeactivated();
        foreach (var child in children)
        {
            child.Deactivate();
        }
    }
    internal void Update(float dt, float deltaSow, ScreenInfo game, ScreenInfo ui)
    {
        if (!Active) return;
        if (!ProcessUpdate(dt, deltaSow, game, ui)) return;
        foreach (var child in children)
        {
            child.Update(dt, deltaSow, game, ui);
        }
    }
    internal void DrawGame(ScreenInfo game)
    {
        if (!Visible) return;
        if (!ProcessDrawGame(game)) return;
        foreach (var child in children)
        {
            child.DrawGame(game);
        }
    }
    internal void DrawGameUI(ScreenInfo ui)
    {
        if (!Visible) return;
        if (!ProcessDrawGameUI(ui)) return;
        foreach (var child in children)
        {
            child.DrawGameUI(ui);
        }
    }
    internal void DrawUI(ScreenInfo ui)
    {
        if (!Visible) return;
        if (!ProcessDrawUI(ui)) return;
        foreach (var child in children)
        {
            child.DrawUI(ui);
        }
    }
    internal void WindowSizeChanged(DimensionConversionFactors conversionFactors)
    {
        OnWindowSizeChanged(conversionFactors);
        foreach (var child in children)
        {
            child.WindowSizeChanged(conversionFactors);
        }
    }
    internal void WindowPositionChanged(Vector2 oldPos, Vector2 newPos)
    {
        OnWindowPositionChanged(oldPos, newPos);
        foreach (var child in children)
        {
            child.WindowPositionChanged(oldPos, newPos);
        }
    }
    internal void MonitorChanged(MonitorInfo newMonitor)
    {
        OnMonitorChanged(newMonitor);
        foreach (var child in children)
        {
            child.MonitorChanged(newMonitor);
        }
    }
    internal void GamepadConnected(Gamepad gamepad)
    {
        OnGamepadConnected(gamepad);
        foreach (var child in children)
        {
            child.GamepadConnected(gamepad);
        }
    }
    internal void GamepadDisconnected(Gamepad gamepad)
    {
        OnGamepadDisconnected(gamepad);
        foreach (var child in children)
        {
            child.GamepadDisconnected(gamepad);
        }
    }
    internal void InputDeviceChanged(InputDevice prevDevice, InputDevice curDevice)
    {
        OnInputDeviceChanged(prevDevice, curDevice);
        foreach (var child in children)
        {
            child.InputDeviceChanged(prevDevice, curDevice);
        }
    }
    internal void CursorEnteredScreen()
    {
        OnCursorEnteredScreen();
        foreach (var child in children)
        {
            child.CursorEnteredScreen();
        }
    }
    internal void CursorLeftScreen()
    {
        OnCursorLeftScreen();
        foreach (var child in children)
        {
            child.CursorLeftScreen();
        }
    }
    internal void CursorHiddenChanged(bool hidden)
    {
        OnCursorHiddenChanged(hidden);
        foreach (var child in children)
        {
            child.CursorHiddenChanged(hidden);
        }
    }
    internal void CursorLockChanged(bool locked)
    {
        OnCursorLockChanged(locked);
        foreach (var child in children)
        {
            child.CursorLockChanged(locked);
        }
    }
    internal void WindowFocusChanged(bool focused)
    {
        OnWindowFocusChanged(focused);
        foreach (var child in children)
        {
            child.WindowFocusChanged(focused);
        }
    }
    internal void WindowFullscreenChanged(bool fullscreen)
    {
        OnWindowFullscreenChanged(fullscreen);
        foreach (var child in children)
        {
            child.WindowFullscreenChanged(fullscreen);
        }
    }
    internal void WindowMaximizeChanged(bool maximized)
    {
        OnWindowMaximizeChanged(maximized);
        foreach (var child in children)
        {
            child.WindowMaximizeChanged(maximized);
        }
    }
    internal void Close()
    {
        OnClosed();
        foreach (var child in children)
        {
            child.Close();
        }
    }

    protected virtual void OnActivated(SceneElement old) { }
    protected virtual void OnDeactivated() { }
    protected virtual void OnClosed() { }
    protected virtual bool ProcessUpdate(float dt, float deltaSlow, ScreenInfo game, ScreenInfo ui) { return true; }
    protected virtual bool ProcessDrawGame(ScreenInfo game) { return true; }
    protected virtual bool ProcessDrawGameUI(ScreenInfo ui) { return true; }
    protected virtual bool ProcessDrawUI(ScreenInfo ui) { return true; }
    protected virtual void OnChildAdded(SceneElement child) { }
    protected virtual void OnChildRemoved(SceneElement child) { }
    protected virtual void OnParentChanged(SceneElement? prevParent, SceneElement? newParent) { }
    protected virtual void OnVisibledChanged(bool value) { }
    protected virtual void OnActiveChanged(bool value) { }
    
    protected virtual void OnWindowSizeChanged(DimensionConversionFactors conversionFactors){}
    protected virtual void OnWindowPositionChanged(Vector2 oldPos, Vector2 newPos){}
    protected virtual void OnMonitorChanged(MonitorInfo newMonitor){}
    protected virtual void OnGamepadConnected(Gamepad gamepad){}
    protected virtual void OnGamepadDisconnected(Gamepad gamepad){}
    protected virtual void OnInputDeviceChanged(InputDevice prevDevice, InputDevice curDevice){}
    protected virtual void OnCursorEnteredScreen() { }
    protected virtual void OnCursorLeftScreen() { }
    protected virtual void OnCursorHiddenChanged(bool hidden) { }
    protected virtual void OnCursorLockChanged(bool locked) { }
    protected virtual void OnWindowFocusChanged(bool focused) { }
    protected virtual void OnWindowFullscreenChanged(bool fullscreen) { }
    protected virtual void OnWindowMaximizeChanged(bool maximized) { }
}

public abstract class CanvasElement
{
    private CanvasElement? parent = null;
    private HashSet<CanvasElement> children = new();

    private Rect rect = new();
    public Rect Rect
    {
        get => rect;
        set => rect = value;
    }
    
    private bool visible = true;
    public bool Visible
    {
        get => visible;
        set
        {
            if (value == visible) return;
            visible = value;
            OnVisibleChanged(value);
        }
    }
    
    private bool active = true;
    public bool Active
    {
        get => active;
        set
        {
            if (active == value) return;
            active = value;
            OnActiveChanged(value);
        }
    }
    
    private bool selectable = true;
    public bool Selectable
    {
        get => selectable;
        set
        {
            if (selectable == value) return;
            selectable = value;
            OnSelectableChanged(value);
        }
    }

    public bool MouseInside { get; private set; } = false;
    
    public bool HasParent() => parent != null;
    public bool HasParent(CanvasElement find)
    {
        if (parent == null) return false;
        List<CanvasElement> parents = new();
        CanvasElement curParent = parent;
        while (true)
        {
            if (curParent == find) return true;
            parents.Add(curParent);
            var nextParent = curParent.parent;
            if (nextParent != null && !parents.Contains(nextParent))
            {
                curParent = nextParent;
            }
            else return false;
        }
    }
    public CanvasElement? GetParent() => parent;
    public List<CanvasElement> GetParents()
    {
        if (parent == null) return new();
        
        List<CanvasElement> parents = new();
        CanvasElement curParent = parent;
        while (true)
        {
            parents.Add(curParent);
            var nextParent = curParent.parent;
            if (nextParent != null && !parents.Contains(nextParent))
            {
                curParent = nextParent;
            }
            else return parents;
        }
    }
    public void FindParent(Func<CanvasElement, bool> result)
    {
        if (parent == null) return;
        List<CanvasElement> parents = new();
        CanvasElement curParent = parent;
        while (true)
        {
            if (result(curParent)) return;
            parents.Add(curParent);
            var nextParent = curParent.parent;
            if (nextParent != null && !parents.Contains(nextParent))
            {
                curParent = nextParent;
            }
            else return;
        }
    }
    
    public bool HasChildren() => children.Count > 0;
    public HashSet<CanvasElement> GetChildren()
    {
        var result = new HashSet<CanvasElement>();

        foreach (var child in children)
        {
            result.Add(child);
        }
        
        return result;
    }
    public HashSet<CanvasElement> GetAllChildren()
    {
        if (!HasChildren()) return new();
        var result = new HashSet<CanvasElement>();

        foreach (var child in children)
        {
            result.Add(child);
            if (child.HasChildren())
            {
                var subChildren = child.GetAllChildren();
                foreach (var subChild in subChildren)
                {
                    result.Add(subChild);
                }
            }
        }
        
        return result;
    }
    public bool FindChild(Func<CanvasElement, bool> result)
    {
        if (!HasChildren()) return false;

        foreach (var child in children)
        {
            if (result(child)) return true;
            if (child.HasChildren())
            {
                if (child.FindChild(result)) return true;
            }
        }
        
        return false;
    }
    public bool HasChild(CanvasElement find)
    {
        if (!HasChildren()) return false;

        if (children.Contains(find)) return true;
        
        foreach (var child in children)
        {
            if (child.HasChild(find)) return true;
        }
        
        return false;
    }
    
    public bool SetParent(CanvasElement? newParent)
    {
        if (newParent == parent) return false;
        
        var prevParent = parent;
        parent = newParent;
        OnParentChanged(prevParent, newParent);
        return true;
    }
    public bool AddChild(CanvasElement child)
    {
        if (!children.Add(child)) return false;
        child.SetParent(this);
        child.Added();
        return true;
    }
    public bool RemoveChild(CanvasElement child)
    {
        if (!children.Remove(child)) return false;
        child.SetParent(null);
        child.Removed();
        return true;
    }
    public bool TransferChildren(CanvasElement from, CanvasElement to)
    {
        if (from == to) return false;
        foreach (var child in from.children)
        {
            to.AddChild(child);
        }
        from.children = new();

        return true;
    }
    internal void Added()
    {
        OnAdded();
        foreach (var child in children)
        {
            child.Added();
        }
    }
    internal void Removed()
    {
        OnRemoved();
        foreach (var child in children)
        {
            child.Removed();
        }
    }

    internal void UpdateRects()
    {
        OnUpdateRects();
        foreach (var child in children)
        {
            UpdateRects();
        }
    }
    internal void Update(float dt, Vector2 mousePos)
    {
        if (!Active) return;
        
        var prevMouseInside = MouseInside;
        MouseInside = rect.ContainsPoint(mousePos);
        
        if(MouseInside && !prevMouseInside) CursorEntered(mousePos);
        else if(!MouseInside && prevMouseInside) CursorLeft(mousePos);
        
        if (!ProcessUpdate(dt, mousePos)) return;
        foreach (var child in children)
        {
            child.Update(dt, mousePos);
        }
    }
    internal void Draw()
    {
        if (!Visible) return;
        if (!ProcessDraw()) return;
        foreach (var child in children)
        {
            child.Draw();
        }
    }
    internal void CursorEntered(Vector2 pos)
    {
        OnCursorEntered(pos);
        foreach (var child in children)
        {
            child.CursorEntered(pos);
        }
    }
    internal void CursorLeft(Vector2 pos)
    {
        OnCursorLeft(pos);
        foreach (var child in children)
        {
            child.CursorLeft(pos);
        }
    }
    internal void Close()
    {
        OnClosed();
        foreach (var child in children)
        {
            child.Close();
        }
    }

    protected virtual void OnUpdateRects() { }
    protected virtual void OnAdded() { }
    protected virtual void OnRemoved() { }
    protected virtual void OnClosed() { }
    protected virtual bool ProcessUpdate(float dt, Vector2 mousePos) { return true; }
    protected virtual bool ProcessDraw() { return true; }
    protected virtual void OnChildAdded(CanvasElement child) { }
    protected virtual void OnChildRemoved(CanvasElement child) { }
    protected virtual void OnParentChanged(CanvasElement? prevParent, CanvasElement? newParent) { }
    protected virtual void OnVisibleChanged(bool value) { }
    protected virtual void OnActiveChanged(bool value) { }
    protected virtual void OnSelectableChanged(bool value) { }
    protected virtual void OnSelected() { }
    protected virtual void OnDeselected() { }
    protected virtual void OnCursorEntered(Vector2 pos) { }
    protected virtual void OnCursorLeft(Vector2 pos) { }
}

// public class RectContainer
// {
//     protected Rect rect = new();
//
//     public Rect Rect
//     {
//         get => rect;
//         set
//         {
//             var oldRect = rect;
//             rect = value;
//             OnRectUpdated(oldRect, rect);
//         }
//     }
//     
//     private RectContainer? parent = null;
//     private Dictionary<string, RectContainer> children = new();
//
//     public RectContainer() { }
//
//     public RectContainer(params (RectContainer container, string name)[] items)
//     {
//         foreach (var item in items)
//         {
//             AddChild(item.container, item.name);
//         }
//     }
//     public void AddChild(RectContainer container, string name)
//     {
//         if (children.ContainsKey(name))
//         {
//             children[name].parent = null;
//             children[name] = container;
//             container.parent = this;
//         }
//         else
//         {
//             children.Add(name, container);
//             container.parent = this;
//         }
//     }
//     public RectContainer? RemoveChild(string name)
//     {
//         if (children.ContainsKey(name))
//         {
//             var child = children[name];
//             children.Remove(name);
//             child.parent = null;
//             return child;
//         }
//
//         return null;
//     }
//     public RectContainer? GetChild(string name)
//     {
//         if (!children.ContainsKey(name)) return null;
//         return children[name];
//     }
//     
//     protected virtual void OnRectUpdated(Rect oldRect, Rect newRect) { }
//     
// }
//
// public class RectContainer2
// {
//     private Rect rect = new();
//
//     public Rect Rect
//     {
//         get => rect;
//         set
//         {
//             rect = value;
//             OnRectUpdated();
//         }
//     }
//     
//     protected virtual void OnRectUpdated() { }
// }


//UICanvas -> UIElement -> UIContainer -> BoxContainer/GridContainer


/*
//ui elements cycle
// 1. set rect (container)
// 2. update (navigator?)
// 3. update state (navigator)
// 4. draw (navigator?)


//just a rect 
//update & draw functions
//select /deselect functions
//hidden & selectable properties
//a way for handling input for pressed (like the old system?)
public class UIElement2
{
    public event Action<UIElement2>? WasSelected;

    public Rect Rect
    {
        get => Margins.Apply(rect);
        set => rect = value;
    }
    public bool Selectable 
    {
        get { return selectable; }
        set 
        {
            selectable = value;
            if (!selectable && Selected)
            {
                Selected = false;
                SelectedChanged(false);
            }
            SelectableChanged(selectable);
        }
    }
    public bool Hidden
    {
        get { return hidden; }
        set
        {
            hidden = value;
            if (hidden && Selected)
            {
                Selected = false;
                SelectedChanged(false);
            }
            HiddenChanged(hidden);
        }
    }

    //public selfSelectable
    //internal parentSelectable
    //public selectable => selfSelectable && parentSelectable
    
    //public selfHidden
    //internal parentHidden
    //public hidden => selfHidden || parentHidden
    
    public UINeighbors Neighbors { get; private set; } = new();
    public UIMargins Margins { get; set; } = new();
    public float StretchFactor { get; set; } = 1f;
    public bool Selected { get; private set; } = false;
    public bool Pressed { get; private set; } = false;
    public bool MouseInside { get; private set;} = false;
    
    
    private bool selectable = true;
    private bool hidden = false;
    private Rect rect = new();

    public void ClearSelection()
    {
        Selected = false; 
        Pressed = false;
    }
        
    public void Select()
    {
        if (Selected) return;
        Selected = true;
        SelectedChanged(true);
        WasSelected?.Invoke(this);
    }
    public void Deselect()
    {
        if (!Selected) return;
        Selected = false;
        SelectedChanged(false);
    }
    
    public void Press()
    {
        if(Pressed) return;
        Pressed = true;
        //WasPressed?.Invoke(this);
        PressedChanged(true);
    }
    public void Release()
    {
        if(!Pressed) return;
        Pressed = false;
        PressedChanged(false);
        //Released = true;
    }
    
    public bool IsPointInside(Vector2 uiPos) => Rect.ContainsPoint(uiPos);

    //i dont know yet if that will move into the ui navigator
    public void UpdateState(Vector2 mousePos, bool mouseDeselects)
    {
        if (Selectable && !Hidden)
        {
            bool prevMouseInside = MouseInside;
            MouseInside = IsPointInside(mousePos);
            if (Selected)
            {
                if(!MouseInside && prevMouseInside && mouseDeselects) Deselect();
            }
            else
            {
                if (MouseInside && !prevMouseInside) Select();
            }
            
            bool prevPressed = Pressed;
            bool pressed;
            if (CheckShortcutPressed()) pressed = true;
            else if (Selected && CheckPressed()) pressed = true;
            else if (MouseInside && CheckMousePressed()) pressed = true;
            else pressed = false;
    
    
            if (pressed && !prevPressed) Press();
            else if (!pressed && prevPressed) Release();
        }
    }
    public virtual void Draw() { }
    public virtual void Update(float dt, Vector2 mousePos) { }

    protected virtual bool CheckPressed() { return false; }
    protected virtual bool CheckMousePressed() { return false; }
    protected virtual bool CheckShortcutPressed() { return false; }
    protected virtual void PressedChanged(bool pressed) { }
    protected virtual void SelectedChanged(bool selected) { }
    protected virtual void HiddenChanged(bool newValue) { }
    protected virtual void SelectableChanged(bool newValue) { }
}

//ui containers only take care of updating rects
//display count & pages
//change between pages
//no display count means all non hidden elements are factored in for rect calculation/update

//how to deal with ui container hidden & selectable?
// if a container is hidden all its children should be hidden as well -> so that they are not visible & so that the
//ui navigator does not use them
//the same applies to selectable
//... those states need to be applied through the entire hierarchy of ui elements -> but how to safe the original state
//if an ui element is hidden an than the parent is hidden as well, as soon as the parent is no longer hidden the ui element
//should still be hidden because it was hidden before...
public abstract class UIContainer2 : UIElement2
{
    // public event Action<UIElement2>? NewElementSelected;
    // public event Action<UIElement2>? FirstElementSelected;
    // public event Action<UIElement2>? LastElementSelected;

    public string Title { get; set; } = "";
    public int DisplayCount { get; set; } = -1;
    public List<UIElement2> Elements { get; protected set; } = new();

    private List<UIElement2> visibleElements = new();

    private List<UIElement2> displayedElements = new();
    //public List<UIElement2> VisibleElements { get; protected set; } = new();
    //public List<UIElement2> DisplayedElements { get; protected set; } = new();
    
    protected int curDisplayIndex = 0;


    public override void Update(float dt, Vector2 mousePos)
    {
        if (!Hidden)
        {
            UpdateDisplayedElements();
            UpdateRects();
        }
    }


    // public void RegisterElements(IEnumerable<UIElement> newElements)
    // {
    //     curDisplayIndex = 0;
    //     var uiElements = newElements.ToList();
    //     if (uiElements.Any())
    //     {
    //         foreach (var element in Elements)
    //         {
    //             element.ClearSelection();
    //             element.WasSelected -= OnUIElementSelected;
    //         }
    //         Elements.Clear();
    //
    //         foreach (var element in uiElements)
    //         {
    //             element.ClearSelection();
    //             element.WasSelected += OnUIElementSelected;
    //         }
    //
    //         Elements = uiElements;//.ToList();
    //         UpdateDisplayedElements();
    //
    //         
    //         if (SelectedElement != null)
    //         {
    //             SelectedElement.ClearSelection();
    //             SelectedElement = null;
    //         }
    //     }
    //     else
    //     {
    //         lastSelectedIndex = -1;
    //         if (SelectedElement != null)
    //         {
    //             SelectedElement.ClearSelection();
    //             SelectedElement = null;
    //         }
    //         
    //         foreach (var element in Elements)
    //         {
    //             element.ClearSelection();
    //             element.WasSelected -= OnUIElementSelected;
    //         }
    //         Elements.Clear();
    //         VisibleElements.Clear();
    //         DisplayedElements.Clear();
    //     }
    //
    // }
    // public void RegisterElements(params UIElement[] newElements)
    // {
    //     RegisterElements(newElements);
    //     // curDisplayIndex = 0;
    //     // if (newElements.Length > 0)
    //     // {
    //     //     foreach (var element in Elements)
    //     //     {
    //     //         element.ClearSelection();
    //     //         element.WasSelected -= OnUIElementSelected;
    //     //     }
    //     //     Elements.Clear();
    //     //
    //     //     foreach (var element in newElements)
    //     //     {
    //     //         element.ClearSelection();
    //     //         element.WasSelected += OnUIElementSelected;
    //     //     }
    //     //
    //     //     Elements = newElements.ToList();
    //     //     UpdateDisplayedElements();
    //     //
    //     //     
    //     //     if (SelectedElement != null)
    //     //     {
    //     //         SelectedElement.ClearSelection();
    //     //         SelectedElement = null;
    //     //     }
    //     // }
    //     // else
    //     // {
    //     //     lastSelectedIndex = -1;
    //     //     if (SelectedElement != null)
    //     //     {
    //     //         SelectedElement.ClearSelection();
    //     //         SelectedElement = null;
    //     //     }
    //     //     
    //     //     foreach (var element in Elements)
    //     //     {
    //     //         element.ClearSelection();
    //     //         element.WasSelected -= OnUIElementSelected;
    //     //     }
    //     //     Elements.Clear();
    //     //     VisibleElements.Clear();
    //     //     DisplayedElements.Clear();
    //     // }
    //
    // }

    public void Reset()
    {
        curDisplayIndex = 0;
        // if (Elements.Count > 0)
        // {
        //     if (SelectedElement != null)
        //     {
        //         SelectedElement.ClearSelection();
        //         SelectedElement = null;
        //     }
        //     lastSelectedIndex = 0;
        // }
    }
    public void Close()
    {
        Elements.Clear();
        visibleElements.Clear();
        // VisibleElements.Clear();
        // DisplayedElements.Clear();
        // SelectedElement = null;
    }

    

    // public List<UIElement2> GetDisplayedAvailableElements()
    // {
    //     return DisplayedElements.FindAll(e => !e.Selectable);
    // }

    public bool MoveNext()
    {
        if (DisplayCount <= 0) return false;
        return SetDisplayStartIndex(curDisplayIndex + 1);
        
    }
    public bool MovePrevious()
    {
        if (DisplayCount <= 0) return false;
        return SetDisplayStartIndex(curDisplayIndex - 1);
    }
    public bool MoveNextPage()
    {
        if(DisplayCount <= 0) return false;
        return SetDisplayStartIndex(curDisplayIndex + DisplayCount);
    }
    public bool MovePreviousPage()
    {
        if (DisplayCount <= 0) return false;
        return SetDisplayStartIndex(curDisplayIndex - DisplayCount);
    }
    public bool MoveToElement(UIElement2 element)
    {
        int index = Elements.IndexOf(element);
        if (index < 0) return false;
        if (index >= curDisplayIndex && index <= GetDisplayEndIndex()) return false;
        return SetDisplayStartIndex(index);
    }
    public bool SetDisplayStartIndex(int newIndex)
    {
        if (DisplayCount <= 0) return false;
        if(newIndex > VisibleElements.Count - DisplayCount) newIndex = VisibleElements.Count - DisplayCount;
        if(newIndex < 0) newIndex = 0;

        if(newIndex != curDisplayIndex)
        {
            int dif = newIndex - curDisplayIndex;
            curDisplayIndex = newIndex;
            UpdateDisplayedElements();
            if (SelectedElement != null)
            {
                var displayedElements = GetDisplayedAvailableElements();
                if (displayedElements.Count > 0 && !displayedElements.Contains(SelectedElement))
                {
                    SelectedElement.Deselect();
                    SelectedElement = null;
                    if(dif > 0)
                    {
                        lastSelectedIndex = 0;
                        SelectedElement = displayedElements[0];
                        SelectedElement.Select();
                    }
                    else
                    {
                        lastSelectedIndex = displayedElements.Count - 1;
                        SelectedElement = displayedElements[lastSelectedIndex];
                        SelectedElement.Select();
                    }
                }
            }
            return true;
        }
        return false;
    }
    protected int GetDisplayStartIndex() { return curDisplayIndex; }
    protected int GetDisplayCount()
    {
        if (DisplayCount <= 0) return Elements.Count;
        else return DisplayCount;
    }
    protected int GetDisplayEndIndex() 
    {
        int endIndex = GetDisplayStartIndex() + GetDisplayCount() - 1;
        if (endIndex >= visibleElements.Count) endIndex = visibleElements.Count - 1;
        return endIndex;
    }


    protected abstract void UpdateRects();
    
    private void UpdateDisplayedElements()
    {
        int prevCount = visibleElements.Count;
        visibleElements = Elements.FindAll(e => !e.Hidden);

        int count = GetDisplayEndIndex() - GetDisplayStartIndex();
        displayedElements = visibleElements.GetRange(GetDisplayStartIndex(), count + 1);

        if (prevCount != visibleElements.Count)
        {
            if (curDisplayIndex > visibleElements.Count - DisplayCount) curDisplayIndex = visibleElements.Count - DisplayCount;
            if (curDisplayIndex < 0) curDisplayIndex = 0;
        }
    }
    


    public static void AlignUIElementsHorizontal(Rect rect, List<UIElement2> elements, int displayCount = -1, float gapRelative = 0f, float elementMaxSizeX = 1f, float elementMaxSizeY = 1f)
    {
        Vector2 startPos = new(rect.X, rect.Y);
        Vector2 maxElementSizeRel = new(elementMaxSizeX, elementMaxSizeY);
        float stretchFactorTotal = 0f;
        int count = displayCount <= 0 ? elements.Count : displayCount;
        for (int i = 0; i < count; i++)
        {
            if (i < elements.Count)
            {
                stretchFactorTotal += elements[i].StretchFactor;
            }
            else stretchFactorTotal += 1;
        }
        int gaps = count - 1;

        float totalWidth = rect.Width;
        float gapSize = totalWidth * gapRelative;
        float elementWidth = (totalWidth - gaps * gapSize) / stretchFactorTotal;
        Vector2 offset = new(0f, 0f);
        foreach (var element in elements)
        {
            float width = elementWidth * element.StretchFactor;
            Vector2 size = new(width, rect.Height);
            Vector2 maxSize = maxElementSizeRel * new Vector2(rect.Width, rect.Height);
            if (maxSize.X > 0f) size.X = MathF.Min(size.X, maxSize.X);
            if (maxSize.Y > 0f) size.Y = MathF.Min(size.Y, maxSize.Y);
            element.Rect = new(startPos + offset, size, new(0f));
            offset += new Vector2(gapSize + width, 0f);
        }

    }
    public static void AlignUIElementsVertical(Rect rect, List<UIElement2> elements, int displayCount = -1, float gapRelative = 0f, float elementMaxSizeX = 1f, float elementMaxSizeY = 1f)
    {
        Vector2 startPos = new(rect.X, rect.Y);
        Vector2 maxElementSizeRel = new(elementMaxSizeX, elementMaxSizeY);
        float stretchFactorTotal = 0f;
        int count = displayCount <= 0 ? elements.Count : displayCount;
        for (int i = 0; i < count; i++)
        {
            if (i < elements.Count)
            {
                stretchFactorTotal += elements[i].StretchFactor;
            }
            else stretchFactorTotal += 1;
        }
        int gaps = count - 1;

        float totalHeight = rect.Height;
        float gapSize = totalHeight * gapRelative;
        float elementHeight = (totalHeight - gaps * gapSize) / stretchFactorTotal;
        Vector2 offset = new(0f, 0f);
        foreach (var element in elements)
        {
            float height = elementHeight * element.StretchFactor;
            Vector2 size = new(rect.Width, height);
            Vector2 maxSize = maxElementSizeRel * new Vector2(rect.Width, rect.Height);
            if (maxSize.X > 0f) size.X = MathF.Min(size.X, maxSize.X);
            if (maxSize.Y > 0f) size.Y = MathF.Min(size.Y, maxSize.Y);
            element.Rect =  new(startPos + offset, size, new(0f));
            offset += new Vector2(0f, gapSize + height);
        }

    }
    public static void AlignUIElementsGrid(Rect rect, List<UIElement2> elements, int columns, int rows, float hGapRelative = 0f, float vGapRelative = 0f, bool leftToRight = true)
    {
        Vector2 startPos = new(rect.X, rect.Y);

        int hGaps = columns - 1;
        float totalWidth = rect.Width;
        float hGapSize = totalWidth * hGapRelative;
        float elementWidth = (totalWidth - hGaps * hGapSize) / columns;
        Vector2 hGap = new(hGapSize + elementWidth, 0);

        int vGaps = rows - 1;
        float totalHeight = rect.Height;
        float vGapSize = totalHeight * vGapRelative;
        float elementHeight = (totalHeight - vGaps * vGapSize) / rows;
        Vector2 vGap = new(0, vGapSize + elementHeight);

        Vector2 elementSize = new(elementWidth, elementHeight);
        int count = columns * rows;
        if (elements.Count < count) count = elements.Count;
        for (int i = 0; i < count; i++)
        {
            var item = elements[i];
            var coords = ShapeUtils.TransformIndexToCoordinates(i, rows, columns, leftToRight);

            item.Rect = new(startPos + hGap * coords.col + vGap * coords.row, elementSize, new(0f));
        }
    }
    
}

//displays item either vertical or horizontal
public class BoxContainer2 : UIContainer2
{
    protected override void UpdateRects()
    {
        throw new NotImplementedException();
    }
}

//displays items in a grid (row * columns = display count)
public class GridContainer2 : UIContainer2
{
    protected override void UpdateRects()
    {
        throw new NotImplementedException();
    }
}

//takes care of all the ui navigation between ui elements
//the selection of ui elements via mouse cursor or input
//updating & drawing
//elements can be added/removed anytime
//every frame at the start a list of active items is generated (not hidden & selectable)
public class UINavigator
{
    public float InputInterval { get; set; } = 0.1f;
    public UINeighbors.NeighborDirection LastInputDirection { get; protected set; } = UINeighbors.NeighborDirection.NONE;
    public UINeighbors.NeighborDirection CurInputDirection { get; protected set; } = UINeighbors.NeighborDirection.NONE;
    public bool WrapVertical { get; set; } = true;
    public bool WrapHorizontal { get; set; } = true;
    public UIElement? SelectedElement { get; protected set; } = null;
    private List<UIElement> elements = new();
    private List<UIElement> activeElements = new();
    private List<UIElement> drawElements = new();
    
    private float dirInputTimer = -1f;


    public void Update(float dt, Vector2 mousePos)
    {
        activeElements.Clear();
        drawElements.Clear();
        foreach (var e in elements)
        {
            if (!e.Hidden)
            {
                drawElements.Add(e);
                if(!e.SelectionLocked) activeElements.Add(e);
            }
            
            e.Update(dt, mousePos);
            
            if (e == SelectedElement)
            {
                if (!e.Selected)
                {
                    SelectedElement = FindClosest(e);
                }
            }
            else
            {
                if (e.Selected)
                {
                    OnNewElementSelected(e);
                }
            }
        }
    }

    public void Draw()
    {
        foreach (var e in drawElements)
        {
            e.Draw();
        }
    }
    
    public UIElement? SelectLeft()
    {
        return null;
    }
    public UIElement? SelectRight()
    {
        return null;
    }
    public UIElement? SelectTop()
    {
        return null;
    }
    public UIElement? SelectBottom()
    {
        return null;
    }
    public UIElement? SelectNext()
    {
        return null;
    }
    public UIElement? SelectPrev()
    {
        return null;
    }

    protected virtual void OnNewElementSelected(UIElement element)
    {
        if(SelectedElement != null) SelectedElement.Deselect();
        SelectedElement = element;
    }

    private UIElement? FindClosest(UIElement element)
    {
        return null;
    }
    // private List<UIElement> Filter() => Elements.FindAll((e) => e is { Hidden: false, SelectionLocked: false });
}
*/



/*

   using System.ComponentModel.Design.Serialization;
   using ShapeEngine.Core.Shapes;
   using System.Numerics;
   using ShapeEngine.Lib;
   
   namespace ShapeEngine.UI
   {
   public abstract class UIElement
   {
   public event Action<UIElement>? WasSelected;
   
   
   public Rect Rect
   {
   get => Margins.Apply(rect);
   set => rect = value;
   }
   public bool SelectionLocked 
   {
   get { return selectionLocked; }
   set 
   {
   selectionLocked = value;
   //Released = false;
   if (selectionLocked && Selected)
   {
   Selected = false;
   SelectedChanged(false);
   }
   SelectionLockedChanged(selectionLocked);
   }
   }
   public bool Hidden
   {
   get { return hidden; }
   set
   {
   hidden = value;
   //Released = false;
   if (hidden && Selected)
   {
   Selected = false;
   SelectedChanged(false);
   }
   HiddenChanged(hidden);
   }
   }
   
   public UINeighbors Neighbors { get; private set; } = new();
   public UIMargins Margins { get; set; } = new();
   public float StretchFactor { get; set; } = 1f;
   public bool Selected { get; private set; } = false;
   public bool Pressed { get; private set; } = false;
   public bool MouseInside { get; private set;} = false;
   public float MouseTolerance { get; set; } = 5f;
   public bool MouseDeselects { get; set; } = false;
   
   
   private bool selectionLocked = true;
   private bool hidden = false;
   private Vector2 prevMousePos = new();
   private Rect rect = new();
   
   public void ClearSelection()
   {
   Selected = false; 
   Pressed = false;
   }
   
   public void Select()
   {
   if (Selected) return;
   Selected = true;
   SelectedChanged(true);
   WasSelected?.Invoke(this);
   }
   public void Deselect()
   {
   if (!Selected) return;
   Selected = false;
   SelectedChanged(false);
   }
   
   public void Press()
   {
   if(Pressed) return;
   Pressed = true;
   //WasPressed?.Invoke(this);
   PressedChanged(true);
   }
   public void Release()
   {
   if(!Pressed) return;
   Pressed = false;
   PressedChanged(false);
   //Released = true;
   }
   
   
   
   
   public bool IsPointInside(Vector2 uiPos) => Rect.ContainsPoint(uiPos);
   
   private void Check(Vector2 mousePos)
   {
   //Released = false;
   if (!SelectionLocked && !Hidden)
   {
   MouseInside = IsPointInside(mousePos);
   bool prevMouseInside = IsPointInside(prevMousePos);
   float disSq = (prevMousePos - mousePos).LengthSquared();
   if (Selected)
   {
   if(!MouseInside && prevMouseInside && MouseDeselects) Deselect();
   }
   else
   {
   if (MouseInside && !prevMouseInside && (MouseTolerance <= 0f || disSq > MouseTolerance * MouseTolerance ) ) Select();
   }
   
   // if ((MouseInside && !prevMouseInside) || (!MouseInside && prevMouseInside) || disSq > MouseTolerance * MouseTolerance)
   // {
   //     if (MouseInside && !Selected)
   //     {
   //         Select();
   //     }
   //     else if (!MouseInside && Selected && MouseDeselects)
   //     {
   //         Deselect();
   //     }
   // }
   
   bool prevPressed = Pressed;
   bool pressed;
   if (CheckShortcutPressed()) pressed = true;
   else if (Selected && CheckPressed()) pressed = true;
   else if (MouseInside && CheckMousePressed()) pressed = true;
   else pressed = false;
   
   
   if (pressed && !prevPressed) Press();
   else if (!pressed && prevPressed) Release();
   }
   }
   
   public void Draw() => DrawElement();
   
   public void Update(float dt, Vector2 mousePos)
   {
   UpdateElement(dt, mousePos);
   Check(mousePos);
   prevMousePos = mousePos;
   }
   
   protected virtual void UpdateElement(float dt, Vector2 mousePos) { }
   protected virtual void DrawElement() { }
   protected virtual bool CheckPressed() { return false; }
   protected virtual bool CheckMousePressed() { return false; }
   protected virtual bool CheckShortcutPressed() { return false; }
   protected virtual void PressedChanged(bool pressed) { }
   protected virtual void SelectedChanged(bool selected) { }
   protected virtual void HiddenChanged(bool newValue) { }
   protected virtual void SelectionLockedChanged(bool newValue) { }
   }
   
   }
   
   
   /*
   public class UIElement
   {
   protected Rectangle rect;
   
   protected float stretchFactor = 1f; //used for containers
   public UIMargins Margins { get; set; } = new();
   
   
   //public void SetMargins(float left = 0, float right = 0, float top = 0, float bottom = 0)
   //{
   //    Margins = new(top, right, bottom, left);
   //}
   //public UIMargins GetMargins() { return Margins; }
   public void SetStretchFactor(float newFactor) { stretchFactor = newFactor; }
   public float GetStretchFactor() { return stretchFactor; }
   public Rectangle GetRect(Vector2 alignement) 
   { 
   if(alignement.X == 0f && alignement.Y == 0f) return Margins.Apply(rect);
   else
   {
   Vector2 topLeft = new Vector2(rect.X, rect.Y);
   Vector2 size = GetSize();
   Vector2 offset = size * alignement;
   return Margins.Apply(new
   (
   topLeft.X + offset.X,
   topLeft.Y + offset.Y,
   size.X,
   size.Y
   ));
   }
   }
   
   
   public virtual void UpdateRect(Vector2 pos, Vector2 size, Vector2 alignement)
   {
   //Vector2 align = UIHandler.GetAlignementVector(alignement);
   Vector2 offset = size * alignement;
   rect = new(pos.X - offset.X, pos.Y - offset.Y, size.X, size.Y);
   }
   public virtual void UpdateRect(Rectangle rect, Vector2 alignement) 
   { 
   UpdateRect(new Vector2(rect.x, rect.y), new Vector2(rect.width, rect.height), alignement);
   }
   public virtual float GetWidth() { return GetRect(new(0f)).width; }
   public virtual float GetHeight() { return GetRect(new(0f)).height; }
   public virtual Vector2 GetPos(Vector2 alignement) 
   {
   Rectangle rect = GetRect(new(0f));
   Vector2 topLeft = new Vector2(rect.X, rect.Y);
   Vector2 offset = GetSize() * alignement;
   return topLeft + offset;
   }
   public virtual Vector2 GetSize() 
   {
   Rectangle rect = GetRect(new(0f));
   return new(rect.width, rect.height); 
   }
   
   public bool IsPointInside(Vector2 uiPos)
   {
   return CheckCollisionPointRec(uiPos, GetRect(new(0f))); // GetScaledRect());
   }
   public virtual void Update(float dt, Vector2 mousePosUI)
   {
   
   }
   public virtual void Draw(Vector2 uiSize, Vector2 stretchFactor)
   {
   
   }
   
   }
   * /
   
   /*
   public class UIElementSelectable2 : UIElement
   {
   protected bool hovered = false;
   protected bool selected = false;
   protected bool disabled = false;
   
   protected bool pressed = false;
   protected bool clicked = false;
   
   protected bool mousePressed = false;
   protected bool mouseClicked = false;
   
   protected UINeighbors neighbors = new();
   protected int shortcut = -1;
   
   
   public bool Clicked() { return clicked || mouseClicked; }
   public bool Pressed() { return pressed || mousePressed; }
   protected virtual bool CheckPressed()
   {
   return selected && InputHandler.IsDown(UIHandler.playerSlot, UIHandler.InputSelect);
   }
   protected virtual bool CheckClicked()
   {
   return selected && InputHandler.IsReleased(UIHandler.playerSlot, UIHandler.InputSelect);
   }
   protected virtual bool CheckMousePressed()
   {
   return hovered && InputHandler.IsDown(UIHandler.playerSlot, UIHandler.InputSelectMouse);
   }
   protected virtual bool CheckMouseClicked()
   {
   return hovered && InputHandler.IsReleased(UIHandler.playerSlot, UIHandler.InputSelectMouse);
   }
   protected virtual bool IsShortcutDown()
   {
   if (shortcut == -1) return false;
   return InputHandler.IsDown(UIHandler.playerSlot, shortcut);
   }
   protected virtual bool IsShortcutReleased()
   {
   if (shortcut == -1) return false;
   return InputHandler.IsReleased(UIHandler.playerSlot, shortcut);
   }
   
   public virtual bool IsAutomaticDetectionDirectionEnabled(UINeighbors.NeighborDirection dir) { return true; }
   
   public void AddShortcut(int shortCutID)
   {
   shortcut = shortCutID;
   }
   public void RemoveShortcut()
   {
   shortcut = -1;
   }
   public void SetNeighbor(UIElementSelectable neighbor, UINeighbors.NeighborDirection dir) { neighbors.SetNeighbor(neighbor, dir); }
   public bool IsSelected() { return selected; }
   public void Disable() { disabled = true; }
   public void Enable() { disabled = false; }
   public bool IsDisabled() { return disabled; }
   public void Select()
   {
   if (selected) return;
   selected = true;
   SelectedChanged(true);
   //AudioHandler.PlaySFX("button hover");
   }
   public void Deselect()
   {
   if (!selected) return;
   selected = false;
   SelectedChanged(false);
   }
   
   public override void Update(float dt, Vector2 mousePosUI)
   {
   clicked = false;
   mouseClicked = false;
   if (disabled) return;
   
   bool shortcutPressed = IsShortcutDown();
   bool shortcutReleased = IsShortcutReleased();
   
   var prevPressed = pressed;
   var prevMousePressed = mousePressed;
   
   if (shortcutPressed || shortcutReleased)
   {
   clicked = shortcutReleased;
   pressed = shortcutPressed;
   }
   else
   {
   var prevHovered = hovered;
   hovered = IsPointInside(mousePosUI);
   if (hovered && !prevHovered)
   {
   HoveredChanged(true);
   PlayHoveredSound();
   }
   else if (!hovered && prevHovered) { HoveredChanged(false); }
   
   if (hovered)
   {
   if (mousePressed) mouseClicked = CheckMouseClicked();
   mousePressed = CheckMousePressed();
   }
   else
   {
   mousePressed = false;
   }
   
   if (selected)
   {
   if (pressed) clicked = CheckClicked();
   pressed = CheckPressed();
   }
   else
   {
   pressed = false;
   }
   }
   
   bool pressedChanged = pressed && !prevPressed;
   bool mousePressedChanged = mousePressed && !prevMousePressed;
   
   if (pressedChanged || mousePressedChanged)
   {
   if (pressedChanged) PressedChanged(true);
   if (mousePressedChanged) MousePressedChanged(true);
   
   PlayClickedSound();
   }
   else
   {
   if (prevPressed) PressedChanged(false);
   if (prevMousePressed) MousePressedChanged(false);
   
   }
   
   if (clicked) WasClicked();
   if (mouseClicked) WasMouseClicked();
   }
   
   
   public virtual void PlayHoveredSound()
   {
   //AudioHandler.PlaySFX("button hover");
   }
   public virtual void PlayClickedSound()
   {
   //AudioHandler.PlaySFX("button click");
   }
   public virtual void WasMouseClicked() { }
   public virtual void MousePressedChanged(bool pressed) { }
   public virtual void HoveredChanged(bool hovered) { }
   
   public virtual void WasClicked() { }
   public virtual void PressedChanged(bool pressed) { }
   public virtual void SelectedChanged(bool selected) { }
   
   
   
   public UIElementSelectable? CheckDirection(UINeighbors.NeighborDirection dir, List<UIElementSelectable> register)
   {
   var neighbor = GoToNeighbor(dir);
   if (neighbor != null) return neighbor;
   else if (IsAutomaticDetectionDirectionEnabled(dir))
   {
   var closest = FindNeighbor(dir, register);
   if (closest != null)
   {
   Deselect();
   closest.Select();
   return closest;
   }
   }
   return null;
   }
   private UIElementSelectable? FindNeighbor(UINeighbors.NeighborDirection dir, List<UIElementSelectable> register)
   {
   UIElementSelectable current = this;
   if (register == null || register.Count <= 0) return null;
   List<UIElementSelectable> neighbors = register.FindAll(e => e != current && !e.IsDisabled());// && e.IsAutomaticDetectionDirectionEnabled(dir));
   if (neighbors.Count <= 0) return null;
   if (neighbors.Count == 1)
   {
   if (current.CheckNeighborDistance(neighbors[0], dir) < float.PositiveInfinity) return neighbors[0];
   else return null;
   }
   int closestIndex = -1;
   float closestDistance = float.PositiveInfinity;
   for (int i = 0; i < neighbors.Count; i++)
   {
   float dis = current.CheckNeighborDistance(neighbors[i], dir);
   if (dis < closestDistance)
   {
   closestDistance = dis;
   closestIndex = i;
   }
   }
   
   if (closestIndex < 0 || closestIndex >= neighbors.Count) return null;
   return neighbors[closestIndex];
   }
   private Vector2 GetDirectionPosition(UINeighbors.NeighborDirection dir)
   {
   Rectangle self = GetRect(new(0f));
   switch (dir)
   {
   case UINeighbors.NeighborDirection.TOP:
   return new(self.X + self.width / 2, self.Y + self.height);//bottom
   case UINeighbors.NeighborDirection.RIGHT:
   return new(self.X, self.Y + self.height / 2); //left
   case UINeighbors.NeighborDirection.BOTTOM:
   return new(self.X + self.width / 2, self.Y);//top
   case UINeighbors.NeighborDirection.LEFT:
   return new(self.X + self.width, self.Y + self.height / 2);//right
   default: return new(self.X + self.width / 2, self.Y + self.height / 2); //center
   }
   }
   private float CheckNeighborDistance(UIElementSelectable neighbor, UINeighbors.NeighborDirection dir)
   {
   if (neighbor == null) return float.PositiveInfinity;
   Vector2 pos = GetDirectionPosition(dir);
   Vector2 otherPos = neighbor.GetDirectionPosition(dir);
   switch (dir)
   {
   case UINeighbors.NeighborDirection.TOP:
   if (pos.Y - otherPos.Y > 0)//neighbor is really on top
   {
   return (otherPos - pos).LengthSquared();
   }
   return float.PositiveInfinity;
   case UINeighbors.NeighborDirection.RIGHT:
   if (otherPos.X - pos.X > 0)
   {
   return (otherPos - pos).LengthSquared();
   }
   return float.PositiveInfinity;
   case UINeighbors.NeighborDirection.BOTTOM:
   if (otherPos.Y - pos.Y > 0)
   {
   return (otherPos - pos).LengthSquared();
   }
   return float.PositiveInfinity;
   case UINeighbors.NeighborDirection.LEFT:
   if (pos.X - otherPos.X > 0)
   {
   return (otherPos - pos).LengthSquared();
   }
   return float.PositiveInfinity;
   default:
   return float.PositiveInfinity;
   }
   
   }
   private UIElementSelectable? GoToNeighbor(UINeighbors.NeighborDirection dir) { return neighbors.SelectNeighbor(dir, this); }
   
   
   }
   * /
*/