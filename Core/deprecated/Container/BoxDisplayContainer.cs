/*
using System.Numerics;

namespace ShapeUI.Container
{
    public class BoxDisplayContainer : BoxContainer
    {
        protected int displayCount = 0;
        protected int curIndex = 0;
        protected int movementDir = 0;

        public BoxDisplayContainer(int displayCount, float gapRelative)
            : base(gapRelative)
        {
            this.displayCount = displayCount;
            RegisterEvents();
        }
        public BoxDisplayContainer(int displayCount, float gapRelative, List<UIElement> children)
            : base(gapRelative, children)
        {
            this.displayCount = displayCount;
            RegisterEvents();
        }
        public BoxDisplayContainer(int displayCount, float gapRelative, params UIElement[] children)
            : base(gapRelative, children)
        {
            this.displayCount = displayCount;
            RegisterEvents();
        }
        public BoxDisplayContainer(int displayCount, float gapRelative, bool vContainer)
           : base(gapRelative, vContainer)
        {
            this.displayCount = displayCount;
            RegisterEvents();
        }
        public BoxDisplayContainer(int displayCount, float gapRelative, bool vContainer, List<UIElement> children)
            : base(gapRelative, vContainer, children)
        {
            this.displayCount = displayCount;
            RegisterEvents();
        }
        public BoxDisplayContainer(int displayCount, float gapRelative, bool vContainer, params UIElement[] children)
            : base(gapRelative, vContainer, children)
        {
            this.displayCount = displayCount;
            RegisterEvents();
        }
        protected void RegisterEvents()
        {
            UIHandler.OnDirectionInput += OnDirectionInput;
            UIHandler.OnSelectedItemUnregistered += OnSelectedItemUnregistered;
        }
        //~VDisplayContainer()//Deconstructor
        //{
        //    UIHandler.OnDirectionInput -= OnDirectionInput;
        //    UIHandler.OnSelectedItemUnregistered -= OnSelectedItemUnregistered;
        //}

        public virtual UIElement NextElement(UIElement selected)
        {
            int index = children.IndexOf(selected);
            if (index >= children.Count - 1) return selected;
            
            index++;
            movementDir = 1;

            UIElement next = children[index];
            if (next == GetLastElement()) ChangeCurIndex(1);
            
            return next;
        }
        public virtual UIElement PrevElement(UIElement selected)
        {
            int index = children.IndexOf(selected);
            if (index <= 0) return selected;

            index--;
            movementDir = -1;

            UIElement next = children[index];
            if (next == GetFirstElement()) ChangeCurIndex(-1);

            return next;
        }

        protected virtual void OnDirectionInput(UINeighbors.NeighborDirection dir, UIElement? selected, UIElement? nextSelected)
        {
            if (nextSelected == null) return;
            if (vContainer)
            {
                if (dir == UINeighbors.NeighborDirection.TOP)
                {
                    if (nextSelected == GetFirstElement())
                    {
                        ChangeCurIndex(-1);
                    }
                }
                else if (dir == UINeighbors.NeighborDirection.BOTTOM)
                {
                    if (nextSelected == GetLastElement())
                    {
                        ChangeCurIndex(1);
                    }
                }
            }
            else
            {
                if (dir == UINeighbors.NeighborDirection.LEFT)
                {
                    if (nextSelected == GetFirstElement())
                    {
                        ChangeCurIndex(-1);
                    }
                }
                else if (dir == UINeighbors.NeighborDirection.RIGHT)
                {
                    if (nextSelected == GetLastElement())
                    {
                        ChangeCurIndex(1);
                    }
                }
            }

        }
        protected virtual void OnSelectedItemUnregistered(UIElement element)
        {
            if (movementDir > 0)
            {
                var selectable = GetFirstElement() as UIElementSelectable;
                if (selectable != null) UIHandler.SelectUIElement(selectable);
            }
            else if (movementDir < 0)
            {
                var selectable = GetLastElement() as UIElementSelectable;
                if (selectable != null) UIHandler.SelectUIElement(selectable);
            }
        }
        public override void AddChild(UIElement newChild)
        {
            if (displayCount > children.Count)
            {
                RegisterChild(newChild);
            }
            children.Add(newChild);
        }
        public override void AddChildren(List<UIElement> newChildren)
        {
            foreach (var child in newChildren)
            {
                AddChild(child);
            }
        }
        public override void AddChildren(params UIElement[] newChildren)
        {
            foreach (var child in newChildren)
            {
                AddChild(child);
            }
        }

        public void SetDisplayCount(int newDisplayCount)
        {
            int dif = newDisplayCount - displayCount;
            if (dif > 0) // register new children
            {
                int start = GetEndIndex();
                int end = start + dif;
                for (int i = start; i < end; i++)
                {
                    if (children.Count <= i) break;
                    RegisterChild(children[i]);
                }

            }
            else if (dif < 0) //unregister some children
            {
                int start = curIndex + newDisplayCount; // GetEndIndex();
                int end = GetEndIndex();
                for (int i = start; i < end; i++)
                {
                    if (children.Count <= i) break;
                    var child = children[i];
                    if (IsElementSelected(child))
                    {
                        UIHandler.DeselectUIElement();
                        int prev = start - 1;
                        if (prev >= 0)
                        {
                            var selectable = children[prev] as UIElementSelectable;
                            if (selectable != null) UIHandler.SelectUIElement(selectable);
                        }
                    }
                    UnregisterChild(children[i]);
                }

            }
            displayCount = newDisplayCount;
        }

        public void SetIndex(int newIndex)
        {
            //UnregisterChildren();
            int prevCurIndex = curIndex;
            int prevLastIndex = GetEndIndex();

            int maxIndex = children.Count - displayCount;
            if (maxIndex < 0) maxIndex = 0;

            if (newIndex < 0) curIndex = 0;
            else if (newIndex > maxIndex) curIndex = maxIndex;
            else curIndex = newIndex;

            movementDir = curIndex - prevCurIndex;

            if ((int)MathF.Abs(movementDir) >= displayCount)
            {
                for (int i = prevCurIndex; i < prevLastIndex; i++)
                {
                    var selectable = children[i] as UIElementSelectable;
                    if (selectable != null) UnregisterChild(selectable);
                }
                RegisterChildren();
            }
            else
            {
                if (movementDir > 0)
                {
                    for (int i = prevCurIndex; i < curIndex; i++)
                    {
                        var selectable = children[i] as UIElementSelectable;
                        if (selectable != null) UnregisterChild(selectable);
                    }
                    for (int i = prevLastIndex; i < GetEndIndex(); i++)
                    {
                        var selectable = children[i] as UIElementSelectable;
                        if (selectable != null) RegisterChild(selectable);
                    }
                }
                else if (movementDir < 0)
                {
                    for (int i = GetEndIndex(); i < prevLastIndex; i++)
                    {
                        var selectable = children[i] as UIElementSelectable;
                        if (selectable != null) UnregisterChild(selectable);
                    }
                    for (int i = curIndex; i < prevCurIndex; i++)
                    {
                        var selectable = children[i] as UIElementSelectable;
                        if (selectable != null) RegisterChild(selectable);
                    }
                }
            }
        }
        public void SetIndex(UIElement child)
        {
            if (!children.Contains(child)) return;
            SetIndex(children.IndexOf(child));
        }
        public int GetCurIndex() { return curIndex; }
        public int GetDisplayCount() { return displayCount; }
        public void ChangeCurIndex(int amount)
        {
            SetIndex(curIndex + amount);
        }
        public int GetEndIndex()
        {
            int index = curIndex + displayCount;
            int count = children.Count;
            if (index > count) index = count;
            return index;
        }

        public override List<UIElement> GetDisplayedItems()
        {
            if (children.Count <= 0) return new();
            int start = curIndex;
            if (start >= children.Count) start = children.Count - 1;
            int end = curIndex + displayCount;
            if (end >= children.Count) end = children.Count;
            return children.GetRange(start, end - start);
        }

        public override void Update(float dt, Vector2 mousePosUI)
        {
            movementDir = 0;
            UpdateRects(GetRect(new(0f)), curIndex, GetEndIndex());
            UpdateChildren(dt, mousePosUI, curIndex, GetEndIndex());
        }
        public override void Draw(Vector2 uiSize, Vector2 stretchFactor)
        {
            if (HasBackground()) DrawBackground(GetRect(new(0f)), bgColor);

            DrawChildren(uiSize, stretchFactor, curIndex, GetEndIndex());
        }

        public override void RegisterChildren()
        {
            for (int i = curIndex; i < GetEndIndex(); i++)
            {
                var child = children[i];
                RegisterChild(child);
            }
        }

        public override void UnregisterChildren()
        {
            for (int i = curIndex; i < GetEndIndex(); i++)
            {
                var child = children[i];
                UnregisterChild(child);
            }
        }
    }

}
*/