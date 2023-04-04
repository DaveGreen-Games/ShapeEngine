using Raylib_CsLo;
using System.Numerics;
using ShapeLib;
using Microsoft.Win32;
using System.Drawing;

namespace ShapeUI.Container
{
    public class BoxDisplayContainer2 : BoxContainer2
    {
        protected int displayCount = 0;
        protected int curIndex = 0;
        protected int movementDir = 0;

        //public BoxDisplayContainer2(int displayCount, float gapRelative)
        //    : base(gapRelative)
        //{
        //    this.displayCount = displayCount;
        //    RegisterEvents();
        //}
        //public BoxDisplayContainer2(int displayCount, float gapRelative, List<UIElement> children)
        //    : base(gapRelative, children)
        //{
        //    this.displayCount = displayCount;
        //    RegisterEvents();
        //}
        //public BoxDisplayContainer2(int displayCount, float gapRelative, params UIElement[] children)
        //    : base(gapRelative, children)
        //{
        //    this.displayCount = displayCount;
        //    RegisterEvents();
        //}
        //public BoxDisplayContainer(int displayCount, float gapRelative, bool vContainer)
        //   : base(gapRelative, vContainer)
        //{
        //    this.displayCount = displayCount;
        //    RegisterEvents();
        //}
        //public BoxDisplayContainer(int displayCount, float gapRelative, bool vContainer, List<UIElement> children)
        //    : base(gapRelative, vContainer, children)
        //{
        //    this.displayCount = displayCount;
        //    RegisterEvents();
        //}
        //public BoxDisplayContainer(int displayCount, float gapRelative, bool vContainer, params UIElement[] children)
        //    : base(gapRelative, vContainer, children)
        //{
        //    this.displayCount = displayCount;
        //    RegisterEvents();
        //}
        protected void RegisterEvents()
        {
            UIHandler.OnDirectionInput += OnDirectionInput;
            UIHandler.OnSelectedItemUnregistered += OnSelectedItemUnregistered;
        }
        

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

    public class BoxContainer2 : UIContainer2
    {
        public float GapRelative { get; set; } = 0f;
        public bool VContainer { get; set; } = true;
        public Vector2 ElementMaxSizeRelative { get; set; } = new(0f);
        public BoxContainer2() { }
        public BoxContainer2(List<UIElement> children) : base(children) { }
        public BoxContainer2(params UIElement[] children) : base(children) { }

        protected override void UpdateRects(int startIndex, int endIndex)
        {
            if (VContainer)
            {
                AlignUIElementsVertical(GetRect(), Elements, startIndex, endIndex, GapRelative, ElementMaxSizeRelative.X, ElementMaxSizeRelative.Y);
            }
            else
            {
                AlignUIElementsHorizontal(GetRect(), Elements, startIndex, endIndex, GapRelative, ElementMaxSizeRelative.X, ElementMaxSizeRelative.Y);
            }
        }
    }

    public class UIContainer2 : UIElement
    {

        public List<UIElement> Elements { get; protected set; } = new();
        public string Title { get; set; } = "";


        public UIContainer2() { }
        public UIContainer2(List<UIElement> elements)
        {
            this.Elements = elements;
        }
        public UIContainer2(params UIElement[] elements)
        {
            this.Elements = elements.ToList();
        }
        public override void DrawElement()
        {
            DrawChildren(GetStartIndex(), GetEndIndex());
        }
        public override void Update(float dt, Vector2 mousePosUI)
        {
            UpdateRects(GetStartIndex(), GetEndIndex());
            UpdateChildren(dt, mousePosUI, GetStartIndex(), GetEndIndex());
        }
        
        public virtual int GetStartIndex() { return 0; }
        public virtual int GetEndIndex() { return Elements.Count; }
        public virtual List<UIElement> GetDisplayedItems() { return Elements; }
        public UIElement? GetChild(int index)
        {
            if (Elements == null || Elements.Count <= 0) return null;
            if (index < 0) index = 0;
            else if (index >= Elements.Count) index = Elements.Count - 1;

            return Elements[index];
        }
        public UIElement? GetDisplayedItem(int index)
        {
            var items = GetDisplayedItems();
            if (items == null || items.Count <= 0) return null;
            if (index < 0) index = 0;
            else if (index >= items.Count) index = items.Count - 1;
            return items[index];
        }


        protected virtual void DrawChildren(int startIndex, int endIndex)
        {
            for (int i = startIndex; i < endIndex; i++)
            {
                Elements[i].Draw();
            }

        }
        protected virtual void UpdateChildren(float dt, Vector2 mousePosUI, int startIndex, int endIndex)
        {
            for (int i = startIndex; i < endIndex; i++)
            {
                Elements[i].Update(dt, mousePosUI);
            }
        }
        protected virtual void UpdateRects(int startIndex, int endIndex)
        {
            for (int i = startIndex; i < endIndex; i++)
            {
                Elements[i].UpdateRect(GetPos(new(0.5f)), GetSize(), new(0.5f));
            }
        }
        
        public static void AlignUIElementsHorizontal(Rectangle rect, List<UIElement> elements, float gapRelative = 0f, float elementMaxSizeX = 1f, float elementMaxSizeY = 1f)
        {
            AlignUIElementsHorizontal(rect, elements, 0, elements.Count, gapRelative, elementMaxSizeX, elementMaxSizeY);
        }
        public static void AlignUIElementsHorizontal(Rectangle rect, List<UIElement> elements, int startIndex, int endIndex, float gapRelative = 0f, float elementMaxSizeX = 1f, float elementMaxSizeY = 1f)
        {
            Vector2 startPos = new(rect.x, rect.y);
            Vector2 maxElementSizeRel = new(elementMaxSizeX, elementMaxSizeY);
            float stretchFactorTotal = 0f;
            for (int i = startIndex; i < endIndex; i++)
            {
                stretchFactorTotal += elements[i].StretchFactor;
            }
            int count = endIndex - startIndex;
            int gaps = count - 1;

            float totalWidth = rect.width;
            float gapSize = totalWidth * gapRelative;
            float elementWidth = (totalWidth - gaps * gapSize) / stretchFactorTotal;
            Vector2 offset = new(0f, 0f);
            for (int i = startIndex; i < endIndex; i++)
            {
                var item = elements[i];
                float width = elementWidth * item.StretchFactor;
                Vector2 size = new(width, rect.height);
                Vector2 maxSize = maxElementSizeRel * new Vector2(rect.width, rect.height);
                if (maxSize.X > 0f) size.X = MathF.Min(size.X, maxSize.X);
                if (maxSize.Y > 0f) size.Y = MathF.Min(size.Y, maxSize.Y);
                item.UpdateRect(startPos + offset, size, new(0f));
                offset += new Vector2(gapSize + width, 0f);
            }

        }
        public static void AlignUIElementsVertical(Rectangle rect, List<UIElement> elements, float gapRelative = 0f, float elementMaxSizeX = 1f, float elementMaxSizeY = 1f)
        {
            AlignUIElementsVertical(rect, elements, 0, elements.Count, gapRelative, elementMaxSizeX, elementMaxSizeY);
        }
        public static void AlignUIElementsVertical(Rectangle rect, List<UIElement> elements, int startIndex, int endIndex, float gapRelative = 0f, float elementMaxSizeX = 1f, float elementMaxSizeY = 1f)
        {
            Vector2 startPos = new(rect.x, rect.y);
            Vector2 maxElementSizeRel = new(elementMaxSizeX, elementMaxSizeY);
            float stretchFactorTotal = 0f;
            for (int i = startIndex; i < endIndex; i++)
            {
                stretchFactorTotal += elements[i].StretchFactor;
            }
            int count = endIndex - startIndex;
            int gaps = count - 1;

            float totalHeight = rect.height;
            float gapSize = totalHeight * gapRelative;
            float elementHeight = (totalHeight - gaps * gapSize) / stretchFactorTotal;
            Vector2 offset = new(0f, 0f);
            for (int i = startIndex; i < endIndex; i++)
            {
                var item = elements[i];
                float height = elementHeight * item.StretchFactor;
                Vector2 size = new(rect.width, height);
                Vector2 maxSize = maxElementSizeRel * new Vector2(rect.width, rect.height);
                if (maxSize.X > 0f) size.X = MathF.Min(size.X, maxSize.X);
                if (maxSize.Y > 0f) size.Y = MathF.Min(size.Y, maxSize.Y);
                item.UpdateRect(startPos + offset, size, new(0f));
                offset += new Vector2(0, gapSize + size.Y);
            }

        }
        public static void AlignUIElementsGrid(Rectangle rect, List<UIElement> elements, int columns, int rows, float hGapRelative = 0f, float vGapRelative = 0f, bool leftToRight = true)
        {
            AlignUIElementsGrid(rect, elements, columns, rows, 0, elements.Count, hGapRelative, vGapRelative, leftToRight);
        }
        public static void AlignUIElementsGrid(Rectangle rect, List<UIElement> elements, int columns, int rows, int startIndex, int endIndex, float hGapRelative = 0f, float vGapRelative = 0f, bool leftToRight = true)
        {
            Vector2 startPos = new(rect.x, rect.y);

            int hGaps = columns - 1;
            float totalWidth = rect.width;
            float hGapSize = totalWidth * hGapRelative;
            float elementWidth = (totalWidth - hGaps * hGapSize) / columns;
            Vector2 hGap = new(hGapSize + elementWidth, 0);

            int vGaps = rows - 1;
            float totalHeight = rect.height;
            float vGapSize = totalHeight * vGapRelative;
            float elementHeight = (totalHeight - vGaps * vGapSize) / rows;
            Vector2 vGap = new(0, vGapSize + elementHeight);

            Vector2 elementSize = new(elementWidth, elementHeight);
            int displayedItems = endIndex - startIndex;

            for (int i = 0; i < displayedItems; i++)
            {
                var item = elements[i + startIndex];
                var coords = SUtils.TransformIndexToCoordinates(i, rows, columns, leftToRight);

                item.UpdateRect(startPos + hGap * coords.col + vGap * coords.row, elementSize, new(0f));
            }
        }


        /*
        public UIElement? GetFirstElement()
        {
            var displayedItems = GetDisplayedItems();
            if (displayedItems == null || displayedItems.Count <= 0) return null;
            return displayedItems[0];
        }
        public UIElement? GetLastElement()
        {
            var displayedItems = GetDisplayedItems();
            if (displayedItems == null || displayedItems.Count <= 0) return null;
            return displayedItems[displayedItems.Count - 1];
        }
        public bool IsFirstItemSelected()
        {
            var displayedItems = GetDisplayedItems();
            if (displayedItems == null || displayedItems.Count <= 0) return false;
            return IsElementSelected(displayedItems[0]);
        }
        public bool IsLastElementSelected()
        {
            var displayedItems = GetDisplayedItems();
            if (displayedItems == null || displayedItems.Count <= 0) return false;
            return IsElementSelected(displayedItems[displayedItems.Count - 1]);
        }
        public bool IsElementSelected(UIElement element)
        {
            return element.Selected;
        }
        */
        /*
        public bool SelectFirstElement()
        {
            var element = GetFirstElement();
            if (element != null)
            {
                var selectable = element as UIElementSelectable;
                if (selectable != null)
                {
                    UIHandler.SelectUIElement(selectable);
                    return true;
                }
                else
                {
                    var container = element as UIContainer;
                    if (container != null) return container.SelectFirstElement();
                }
            }
            return false;
        }
        public bool SelectLastElement()
        {
            var element = GetLastElement();
            if (element != null)
            {
                var selectable = element as UIElementSelectable;
                if (selectable != null)
                {
                    UIHandler.SelectUIElement(selectable);
                    return true;
                }
                else
                {
                    var container = element as UIContainer;
                    if (container != null) return container.SelectLastElement();
                }
            }
            return false;
        }
        */
        //public void AddElement(UIElement newElement) { Elements.Add(newElement); }
        //public void AddElements(List<UIElement> newElements) { this.Elements.AddRange(newElements); }
        //public void AddElements(params UIElement[] newElements) { this.Elements.AddRange(newElements); }
        //public void RemoveElement(UIElement element) { Elements.Remove(element); }
        //public void RemoveElements(List<UIElement> elementsToRemove) { foreach (var element in elementsToRemove) RemoveElement(element); }
        //public void RemoveElements(params UIElement[] elementsToRemove) { foreach (var element in elementsToRemove) RemoveElement(element); }
        //public void RemoveAll(Predicate<UIElement> match) { Elements.RemoveAll(match); }
        //public void ClearElements() { Elements.Clear(); }
        //public List<UIElement> FindChildren(Predicate<UIElement> match) { return Elements.FindAll(match); }
        //public bool HasChildren() { return Elements.Count > 0; }
        //public virtual bool Select(int index = -1)
        //{
        //    var items = GetDisplayedItems();
        //    if (items != null && items.Count > 0)
        //    {
        //        int i = 0;
        //        if (index < 0) i = 0;
        //        else if (index >= items.Count) i = items.Count - 1;
        //        else i = index;
        //        var element = items[i];
        //        if (element != null)
        //        {
        //            var selectable = element as UIElementSelectable;
        //            if (selectable != null)
        //            {
        //                UIHandler.SelectUIElement(selectable);
        //                return true;
        //            }
        //            else
        //            {
        //                var container = element as UIContainer;
        //                if (container != null) return container.Select(index);
        //            }
        //        }
        //    }
        //    return false;
        //}
        //public bool IsSelected(bool deep = false)
        //{
        //    var displayedItems = GetDisplayedItems();
        //    if (displayedItems == null || displayedItems.Count <= 0) return false;
        //    if (deep)
        //    {
        //        foreach (var child in displayedItems)
        //        {
        //            var selectable = child as UIElementSelectable;
        //            if (selectable != null)
        //            {
        //                if (selectable.IsSelected()) return true;
        //            }
        //            else
        //            {
        //                var container = child as UIContainer;
        //                if (container != null)
        //                {
        //                    bool selected = container.IsSelected();
        //                    if (selected) return true;
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        foreach (var item in displayedItems)
        //        {
        //            if (IsElementSelected(item)) return true;
        //        }
        //    }
        //
        //    return false;
        //}

        
        //public virtual void RegisterChildren()
        //{
        //    foreach (var child in GetDisplayedItems())
        //    {
        //        RegisterChild(child);
        //    }
        //}
        //
        //public virtual void UnregisterChildren()
        //{
        //    foreach (var child in GetDisplayedItems())
        //    {
        //        UnregisterChild(child);
        //    }
        //}
        //
        //protected void RegisterChild(UIElement child)
        //{
        //    if (child is UIContainer)
        //    {
        //        var container = (UIContainer)child;
        //        container.RegisterChildren();
        //    }
        //    else if (child is UIElementSelectable)
        //    {
        //        var selectable = (UIElementSelectable)child;
        //        UIHandler.RegisterUIElement(selectable);
        //    }
        //}
        //
        //protected void UnregisterChild(UIElement child)
        //{
        //    if (child is UIContainer)
        //    {
        //        var container = (UIContainer)child;
        //        container.UnregisterChildren();
        //    }
        //    else if (child is UIElementSelectable)
        //    {
        //        var selectable = (UIElementSelectable)child;
        //        UIHandler.UnregisterUIElement(selectable);
        //    }
        //}
        
        //protected virtual void DrawBackground(Rectangle rect, Color color)
        //{
        //    SDrawing.DrawRectangle(rect, color);
        //}
    }


    public class UIContainer : UIElement
    {
        protected List<UIElement> children = new();
        //protected Color bgColor = new(0, 0, 0, 0);
        protected string containerTitle = "";

        public UIContainer() { }
        public UIContainer(List<UIElement> children)
        {
            this.children = children;
        }
        public UIContainer(params UIElement[] children)
        {
            this.children = children.ToList();
        }


        public List<UIElement> FindChildren(Predicate<UIElement> match) { return children.FindAll(match); }

        public string GetContainerTitle() { return containerTitle; }
        public void SetContainerTitle(string newTitle) { containerTitle = newTitle; }
        //public void SetColors(Color newBgColor)
        //{
        //    bgColor = newBgColor;
        //}
        //public bool HasBackground() { return bgColor.a > 0; }

        public bool HasChildren() { return children != null && children.Count > 0; }

        public virtual List<UIElement> GetDisplayedItems() { return children; }

        /// <summary>
        /// Try to select the children at the designated index.
        /// Index of < 0 tries to select the first element, Index of >= child count tries to select the last element.
        /// If the item inherits from UIElementSelectable it is selected. If the item inherits from UIContainer 
        /// than Select() is called on the UIContainer etc.
        /// </summary>
        /// <param name="index">Item to try to select.</param>
        /// <returns></returns>
        //public virtual bool Select(int index = -1)
        //{
        //    var items = GetDisplayedItems();
        //    if (items != null && items.Count > 0)
        //    {
        //        int i = 0;
        //        if (index < 0) i = 0;
        //        else if (index >= items.Count) i = items.Count - 1;
        //        else i = index;
        //        var element = items[i];
        //        if (element != null)
        //        {
        //            var selectable = element as UIElementSelectable;
        //            if (selectable != null)
        //            {
        //                UIHandler.SelectUIElement(selectable);
        //                return true;
        //            }
        //            else
        //            {
        //                var container = element as UIContainer;
        //                if (container != null) return container.Select(index);
        //            }
        //        }
        //    }
        //    return false;
        //}

        public UIElement? GetChild(int index)
        {
            if (children == null || children.Count <= 0) return null;
            if (index < 0) index = 0;
            else if (index >= children.Count) index = children.Count - 1;

            return children[index];
        }
        public UIElement? GetDisplayedItem(int index)
        {
            var items = GetDisplayedItems();
            if (items == null || items.Count <= 0) return null;
            if (index < 0) index = 0;
            else if (index >= items.Count) index = items.Count - 1;
            return items[index];
        }
        /*
        public bool SelectFirstElement()
        {
            var element = GetFirstElement();
            if (element != null)
            {
                var selectable = element as UIElementSelectable;
                if (selectable != null)
                {
                    UIHandler.SelectUIElement(selectable);
                    return true;
                }
                else
                {
                    var container = element as UIContainer;
                    if (container != null) return container.SelectFirstElement();
                }
            }
            return false;
        }
        public bool SelectLastElement()
        {
            var element = GetLastElement();
            if (element != null)
            {
                var selectable = element as UIElementSelectable;
                if (selectable != null)
                {
                    UIHandler.SelectUIElement(selectable);
                    return true;
                }
                else
                {
                    var container = element as UIContainer;
                    if (container != null) return container.SelectLastElement();
                }
            }
            return false;
        }
        */

        public UIElement? GetFirstElement()
        {
            var displayedItems = GetDisplayedItems();
            if (displayedItems == null || displayedItems.Count <= 0) return null;
            return displayedItems[0];
        }
        public UIElement? GetLastElement()
        {
            var displayedItems = GetDisplayedItems();
            if (displayedItems == null || displayedItems.Count <= 0) return null;
            return displayedItems[displayedItems.Count - 1];
        }
        public bool IsFirstItemSelected()
        {
            var displayedItems = GetDisplayedItems();
            if (displayedItems == null || displayedItems.Count <= 0) return false;
            return IsElementSelected(displayedItems[0]);
        }
        public bool IsLastElementSelected()
        {
            var displayedItems = GetDisplayedItems();
            if (displayedItems == null || displayedItems.Count <= 0) return false;
            return IsElementSelected(displayedItems[displayedItems.Count - 1]);
        }
        public bool IsElementSelected(UIElement element)
        {
            return element.Selected;
            //var selectable = element as UIElementSelectable;
            //if (selectable != null) return selectable.IsSelected();
            //else return false;
        }
        //public bool IsSelected(bool deep = false)
        //{
        //    var displayedItems = GetDisplayedItems();
        //    if (displayedItems == null || displayedItems.Count <= 0) return false;
        //    if (deep)
        //    {
        //        foreach (var child in displayedItems)
        //        {
        //            var selectable = child as UIElementSelectable;
        //            if (selectable != null)
        //            {
        //                if (selectable.IsSelected()) return true;
        //            }
        //            else
        //            {
        //                var container = child as UIContainer;
        //                if (container != null)
        //                {
        //                    bool selected = container.IsSelected();
        //                    if (selected) return true;
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        foreach (var item in displayedItems)
        //        {
        //            if (IsElementSelected(item)) return true;
        //        }
        //    }
        //
        //    return false;
        //}

        public virtual void AddChild(UIElement newChild)
        {
            //RegisterChild(newChild);
            children.Add(newChild);
        }

        public virtual void AddChildren(List<UIElement> newChildren)
        {
            //foreach (var child in newChildren)
            //{
            //    RegisterChild(child);
            //}
            children.AddRange(newChildren);
        }
        public virtual void AddChildren(params UIElement[] newChildren)
        {
            //foreach (var child in newChildren)
            //{
            //    RegisterChild(child);
            //}
            children.AddRange(newChildren);
        }
        public virtual void RemoveChild(UIElement child)
        {
            //UnregisterChild(child);
            children.Remove(child);
        }

        public void RemoveChildren(List<UIElement> childrenToRemove)
        {
            foreach (var child in childrenToRemove)
            {
                RemoveChild(child);
            }
        }
        public void RemoveChildren(params UIElement[] childrenToRemove)
        {
            foreach (var child in childrenToRemove)
            {
                RemoveChild(child);
            }
        }
        public void RemoveAll(Predicate<UIElement> match)
        {
            //UnregisterChildren();
            children.RemoveAll(match);
            //RegisterChildren();
        }
        public void ClearChildren()
        {
            //UnregisterChildren();
            children.Clear();
        }

        public override void Draw()
        {
            //if (HasBackground()) DrawBackground(GetRect(new(0f)), bgColor);
            DrawChildren();
        }

        public override void Update(float dt, Vector2 mousePosUI)
        {
            foreach (var child in children)
            {
                child.UpdateRect(GetPos(new(0.5f)), GetSize(), new(0.5f));
                child.Update(dt, mousePosUI);
            }
        }

        //public virtual void RegisterChildren()
        //{
        //    foreach (var child in GetDisplayedItems())
        //    {
        //        RegisterChild(child);
        //    }
        //}
        //
        //public virtual void UnregisterChildren()
        //{
        //    foreach (var child in GetDisplayedItems())
        //    {
        //        UnregisterChild(child);
        //    }
        //}
        //
        //protected void RegisterChild(UIElement child)
        //{
        //    if (child is UIContainer)
        //    {
        //        var container = (UIContainer)child;
        //        container.RegisterChildren();
        //    }
        //    else if (child is UIElementSelectable)
        //    {
        //        var selectable = (UIElementSelectable)child;
        //        UIHandler.RegisterUIElement(selectable);
        //    }
        //}
        //
        //protected void UnregisterChild(UIElement child)
        //{
        //    if (child is UIContainer)
        //    {
        //        var container = (UIContainer)child;
        //        container.UnregisterChildren();
        //    }
        //    else if (child is UIElementSelectable)
        //    {
        //        var selectable = (UIElementSelectable)child;
        //        UIHandler.UnregisterUIElement(selectable);
        //    }
        //}
        protected virtual void DrawChildren(int startIndex = -1, int endIndex = -1)
        {
            if (startIndex < 0 || endIndex < 0)
            {
                foreach (var child in children)
                {
                    child.Draw();
                }
            }
            else
            {
                for (int i = startIndex; i < endIndex; i++)
                {
                    children[i].Draw();
                }
            }

        }
        protected virtual void UpdateChildren(float dt, Vector2 mousePosUI, int startIndex = -1, int endIndex = -1)
        {
            if (startIndex < 0 || endIndex < 0)
            {
                foreach (var child in children)
                {
                    child.Update(dt, mousePosUI);
                }
            }
            else
            {
                for (int i = startIndex; i < endIndex; i++)
                {
                    children[i].Update(dt, mousePosUI);
                }
            }
        }

        //protected virtual void DrawBackground(Rectangle rect, Color color)
        //{
        //    SDrawing.DrawRectangle(rect, color);
        //}
    }

}
