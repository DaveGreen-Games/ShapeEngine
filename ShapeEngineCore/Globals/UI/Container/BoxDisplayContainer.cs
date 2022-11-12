using System.Numerics;

namespace ShapeEngineCore.Globals.UI.Container
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
            UpdateRects(GetRect(), curIndex, GetEndIndex());
            UpdateChildren(dt, mousePosUI, curIndex, GetEndIndex());
        }
        public override void Draw(Vector2 uiSize, Vector2 stretchFactor)
        {
            if (HasBackground()) DrawBackground(GetRect(), bgColor);

            DrawChildren(uiSize, stretchFactor, curIndex, GetEndIndex());
        }

        /*
        protected override void UpdateRects(Rectangle rect, int startIndex, int endIndex)
        {
            Vector2 startPos = new(rect.x, rect.y);

            int count = displayCount;// endIndex - startIndex;
            int gaps = count - 1;
        
            if (vContainer)
            {
                float totalHeight = rect.height;
                float gapSize = totalHeight * gapRelative;
                float baseHeight = (totalHeight - gaps * gapSize) / count;
                float accumulatedHeight = 0f;
                float stretchFactorTotal = 0f;
                for (int i = startIndex; i < endIndex; i++)
                {
                    float factor = children[i].GetStretchFactor();
                    stretchFactorTotal += factor;
                    accumulatedHeight += baseHeight * factor;
                }
                accumulatedHeight += gapSize * gaps;
                float dif = accumulatedHeight - totalHeight;
                float correction = 0f;
                if(dif > 0) correction = dif / stretchFactorTotal;

                //float elementHeight = (totalHeight - gaps * gapSize) / stretchFactorTotal;
                
                Vector2 offset = new(0f, 0f);
                for (int i = startIndex; i < endIndex; i++)
                {
                    var item = children[i];
                    float stretchFactor = item.GetStretchFactor();
                    float elementHeight = baseHeight * stretchFactor - correction * stretchFactor; 
                    //float height = elementHeight * item.GetStretchFactor();
                    Vector2 size = new(rect.width, elementHeight);
                    item.UpdateRect(startPos + offset, size, Alignement.TOPLEFT);
                    offset += new Vector2(0, gapSize + elementHeight);
                }
            }
            else
            {
                float totalWidth = rect.width;
                float gapSize = totalWidth * gapRelative;
                float baseWidth = (totalWidth - gaps * gapSize) / count;
                float accumulatedWidth = 0f;
                float stretchFactorTotal = 0f;
                for (int i = startIndex; i < endIndex; i++)
                {
                    float factor = children[i].GetStretchFactor();
                    stretchFactorTotal += factor;
                    accumulatedWidth += baseWidth * factor;
                }

                float dif = accumulatedWidth - totalWidth;
                float correction = 0f;
                if (dif > 0) correction = dif / stretchFactorTotal;
                //float elementWidth = (totalWidth - gaps * gapSize) / stretchFactorTotal;
                Vector2 offset = new(0f, 0f);
                for (int i = startIndex; i < endIndex; i++)
                {
                    var item = children[i];
                    //float width = elementWidth * item.GetStretchFactor();
                    float stretchFactor = item.GetStretchFactor();
                    float elementWidth = baseWidth * stretchFactor - correction * stretchFactor;
                    Vector2 size = new(elementWidth, rect.height);
                    item.UpdateRect(startPos + offset, size, Alignement.TOPLEFT);
                    offset += new Vector2(gapSize + elementWidth, 0f);
                }
            }
        
        }
        */


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
