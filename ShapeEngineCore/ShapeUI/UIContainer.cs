using Raylib_CsLo;
using System.Numerics;
using ShapeLib;

namespace ShapeUI
{
    public class BoxContainer : UIContainer
    {
        public float GapRelative { get; set; } = 0f;
        public bool VContainer { get; set; } = true;
        public Vector2 ElementMaxSizeRelative { get; set; } = new(1f);
        public BoxContainer() { }
        public BoxContainer(params UIElement[] children) : base(children) { }

        
        protected override void UpdateRects()
        {
            if (VContainer)
            {
                AlignUIElementsVertical(GetRect(), DisplayedElements, DisplayCount, GapRelative, ElementMaxSizeRelative.X, ElementMaxSizeRelative.Y);
            }
            else
            {
                AlignUIElementsHorizontal(GetRect(), DisplayedElements, DisplayCount, GapRelative, ElementMaxSizeRelative.X, ElementMaxSizeRelative.Y);
            }
        }
    }
    public class GridContainer : UIContainer
    {
        public Vector2 GapRelative { get; set; } = new(0f);
        public bool LeftToRight { get; set; } = true;
        public int Rows { get; protected set; }
        public int Columns { get; protected set; }
        
        public GridContainer(int rows, int columns) 
        {
            this.Rows = rows;
            this.Columns = columns;
            this.DisplayCount = rows * columns;
        }
        public GridContainer(int rows, int columns, params UIElement[] children) : base(children)
        {
            this.Rows = rows;
            this.Columns = columns;
            this.DisplayCount = rows * columns;
        }


        protected override void UpdateRects()
        {
            AlignUIElementsGrid(GetRect(), DisplayedElements, Columns, Rows, GapRelative.X, GapRelative.Y, LeftToRight);
        }
    }

    //figure out a way to remove the start element -> if selected item disappears select closest one -> on reset select first on -> on start select first one
    public class UIContainer : UIElement
    {
        public event Action<UIElement>? NewElementSelected;
        public event Action<UIElement>? FirstElementSelected;
        public event Action<UIElement>? LastElementSelected;


        public string Title { get; set; } = "";
        public int DisplayCount { get; set; } = -1;
        public bool NavigationDisabled { get; protected set; } = true;
        public float InputInterval { get; set; } = 0.1f;
        public float MouseTolerance { get; set; } = 5f;
        public UINeighbors.NeighborDirection LastInputDirection { get; protected set; } = UINeighbors.NeighborDirection.NONE;
        public UINeighbors.NeighborDirection CurInputDirection { get; protected set; } = UINeighbors.NeighborDirection.NONE;
        //public UIElement? NavigationStartElement { get; protected set; } = null;
        //protected UIElement? lastSelectedElement = null;
        protected int lastSelectedIndex = -1;
        public UIElement? SelectedElement { get; protected set; } = null;
        public List<UIElement> Elements { get; protected set; } = new();
        public List<UIElement> VisibleElements { get; protected set; } = new();
        public List<UIElement> DisplayedElements { get; protected set; } = new();
        
        protected int curDisplayIndex = 0;
        protected Vector2 prevMousePos = new(0f);
        
        private float dirInputTimer = -1f;

        public UIContainer() { DisabledSelection = false; }
        public UIContainer(params UIElement[] elements)
        {
            DisabledSelection = false;
            RegisterElements(elements);
        }


        //public bool IsElementReleased(int index)
        //{
        //    if (index >= 0 && index < Elements.Count) return Elements[index].Released;
        //    return false;
        //}

        public override void Update(float dt, Vector2 mousePosUI)
        {
            if (!hidden)
            {
                UpdateDisplayedElements();
                UpdateNavigation(dt);
                UpdateRects();
                UpdateChildren(dt, mousePosUI);
            }
        }
        public override void Draw()
        {
           if(!Hidden) DrawChildren();
        }

        public void RegisterElements(params UIElement[] newElements)
        {
            curDisplayIndex = 0;
            if (newElements.Length > 0)
            {
                foreach (var element in Elements)
                {
                    element.Selected = false;
                    element.WasSelected -= OnUIElementSelected;
                    //element.InsideContainer = false;
                }
                Elements.Clear();

                foreach (var element in newElements)
                {
                    element.Selected = false;
                    element.WasSelected += OnUIElementSelected;
                    //element.InsideContainer = true;
                }

                Elements = newElements.ToList();
                UpdateDisplayedElements();

                
                if (SelectedElement != null)
                {
                    SelectedElement.Selected = false;
                    SelectedElement = null;
                    //if (SelectedElement.DisabledSelection || !GetDisplayedAvailableElements().Contains(SelectedElement))
                    //{
                    //    SelectedElement.Selected = false;
                    //    SelectedElement = null;
                    //    lastSelectedElement = null;
                    //}
                }

                //if (!NavigationDisabled)
                //{
                //    if(SelectedElement == null)
                //    {
                //        var e = GetNavigationStartElement();
                //        if(e != null)
                //        {
                //            SelectedElement = e;
                //            lastSelectedElement = e;
                //            SelectedElement.Select();
                //        }
                //    }
                //}
                //NavigationStartElement = GetNavigationStartElement();
            }
            else
            {
                lastSelectedIndex = -1;
                if (SelectedElement != null)
                {
                    SelectedElement.Selected = false;
                    SelectedElement = null;
                    //lastSelectedElement = null;
                }
                
                //NavigationStartElement = null;
                foreach (var element in Elements)
                {
                    element.Selected = false;
                    element.WasSelected -= OnUIElementSelected;
                    //element.InsideContainer = false;
                }
                Elements.Clear();
                VisibleElements.Clear();
                DisplayedElements.Clear();
            }

        }

        public void Reset()
        {
            curDisplayIndex = 0;
            if (Elements.Count > 0)
            {
                //foreach (var element in elements) { element.Deselect(); }
                
                if (SelectedElement != null)
                {
                    SelectedElement.Selected = false;
                    SelectedElement = null;
                }
                lastSelectedIndex = 0;
                //if (!NavigationDisabled)
                //{
                //    if (SelectedElement == null)
                //    {
                //        var e = GetNavigationStartElement();
                //        if (e != null)
                //        {
                //            SelectedElement = e;
                //            lastSelectedElement = e;
                //            SelectedElement.Select();
                //        }
                //    }
                //}
                //NavigationStartElement = GetNavigationStartElement();
                //
                //if (NavigationStartElement != null)
                //{
                //    SelectedElement = NavigationStartElement;
                //    NavigationStartElement.Select();
                //    NewElementSelected?.Invoke(NavigationStartElement);
                //}
            }
        }
        public void Close()
        {
            Elements.Clear();
            VisibleElements.Clear();
            DisplayedElements.Clear();
            SelectedElement = null;
            //NavigationStartElement = null;
        }

        
        protected override void SelectedChanged(bool selected)
        {
            if (selected) StartNavigation();
            else StopNavigation();
        }
        private void StartNavigation()
        {
            if (NavigationDisabled)
            {
                NavigationDisabled = false;
                UpdateSelectedElement();
                //if (lastSelectedElement != null && GetDisplayedAvailableElements().Contains(lastSelectedElement))
                //{
                //    SelectedElement = lastSelectedElement;
                //    SelectedElement.Select();
                //}
                //else
                //{
                //    var e = GetNavigationStartElement();
                //    if (e != null)
                //    {
                //        SelectedElement = e;
                //        lastSelectedElement = e;
                //        SelectedElement.Select();
                //    }
                //}

                //NavigationStartElement = GetNavigationStartElement();
                //if (SelectedElement == null && NavigationStartElement != null)
                //{
                //    SelectedElement = NavigationStartElement;
                //    NavigationStartElement.Select();
                //    NewElementSelected?.Invoke(NavigationStartElement);
                //}
            }
        }
        private void StopNavigation()
        {
            if (!NavigationDisabled)
            {
                NavigationDisabled = true;
                if (SelectedElement != null)
                {
                    //SelectedElement.Deselect();
                    SelectedElement.Selected = false;
                    SelectedElement = null;
                }
            }
        }
        public bool Navigate(UINeighbors.NeighborDirection inputDirection)
        {
            if (NavigationDisabled || DisabledSelection || Elements.Count <= 0 || VisibleElements.Count <= 0 || DisplayedElements.Count <= 0 || DisplayCount == 1) return false;

            if (inputDirection == UINeighbors.NeighborDirection.NONE)
            {
                LastInputDirection = UINeighbors.NeighborDirection.NONE;
                CurInputDirection = UINeighbors.NeighborDirection.NONE;
                dirInputTimer = -1;
                return false;
            }

            LastInputDirection = CurInputDirection;
            CurInputDirection = inputDirection;

            if (dirInputTimer > 0f)
            {
                if (LastInputDirection != CurInputDirection)
                {
                    dirInputTimer = -1;
                }
                else return false;
            }

            if (SelectedElement == null) return false;

            int newIndex = -1;
            List<UIElement> neighbors = DisplayedElements.FindAll(e => e != SelectedElement && !e.DisabledSelection);
            if (inputDirection == UINeighbors.NeighborDirection.TOP)
            {
                newIndex = CheckDirection(neighbors, SelectedElement, inputDirection);
                if (InputInterval > 0f) dirInputTimer = InputInterval;
            }
            else if (inputDirection == UINeighbors.NeighborDirection.RIGHT)
            {
                newIndex = CheckDirection(neighbors, SelectedElement, inputDirection);
                if (InputInterval > 0f) dirInputTimer = InputInterval;
            }
            else if (inputDirection == UINeighbors.NeighborDirection.BOTTOM)
            {
                newIndex = CheckDirection(neighbors, SelectedElement, inputDirection);
                if (InputInterval > 0f) dirInputTimer = InputInterval;
            }
            else if (inputDirection == UINeighbors.NeighborDirection.LEFT)
            {
                newIndex = CheckDirection(neighbors, SelectedElement, inputDirection);
                if (InputInterval > 0f) dirInputTimer = InputInterval;
            }

            if(newIndex >= 0)
            {
                UIElement newSelected = neighbors[newIndex];
                if(newSelected != SelectedElement)
                {
                    SelectedElement.Deselect();
                    SelectedElement = newSelected;
                    SelectedElement.Select();
                    lastSelectedIndex = newIndex;
                    
                    var availableElements = GetDisplayedAvailableElements();
                    if (newSelected == availableElements[0]) FirstElementSelected?.Invoke(newSelected);
                    else if (newSelected == availableElements[availableElements.Count - 1]) LastElementSelected?.Invoke(newSelected);
                    
                    return true;
                }
            }
            return false;
        }
        
        /*
        public UIElement? SelectNextElement()
        {
            if (NavigationDisabled || DisabledSelection) return null;
            if (NavigationSelectedElement == null) return null;
            var available = GetDisplayedAvailableElements();
            if (available.Count <= 0) return null;
            int index = available.IndexOf(NavigationSelectedElement);
            index += 1;
            if (index >= available.Count) index = 0;
            UIElement next = available[index];
            if (next != NavigationSelectedElement)
            {
                NavigationSelectedElement.Deselect();
                NavigationSelectedElement = next;
                NavigationSelectedElement.Select();
                NewElementSelected?.Invoke(next);
                return next;
            }
            else return NavigationSelectedElement;
        }
        public UIElement? SelectPreviousElement()
        {
            if (NavigationDisabled || DisabledSelection) return null;
            if (NavigationSelectedElement == null) return null;
            var available = GetDisplayedAvailableElements();
            if (available.Count <= 0) return null;
            int index = available.IndexOf(NavigationSelectedElement);
            index -= 1;
            if (index < 0) index = available.Count - 1;
            UIElement prev = available[index];
            if (prev != NavigationSelectedElement)
            {
                NavigationSelectedElement.Deselect();
                NavigationSelectedElement = prev;
                NavigationSelectedElement.Select();
                NewElementSelected?.Invoke(prev);
                return prev;
            }
            else return NavigationSelectedElement;
        }
        */
        public List<UIElement> GetDisplayedAvailableElements()
        {
            return DisplayedElements.FindAll(e => !e.DisabledSelection);
        }

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
        public bool MoveToElement(UIElement element)
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
                        //lastSelectedElement = null;
                        if(dif > 0)
                        {
                            lastSelectedIndex = 0;
                            SelectedElement = displayedElements[0];
                            //lastSelectedElement = SelectedElement;
                            SelectedElement.Select();
                        }
                        else
                        {
                            lastSelectedIndex = displayedElements.Count - 1;
                            SelectedElement = displayedElements[lastSelectedIndex];
                            //lastSelectedElement = SelectedElement;
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
            if (endIndex >= VisibleElements.Count) endIndex = VisibleElements.Count - 1;
            return endIndex;

            //int index = GetDisplayStartIndex() + GetDisplayCount() - 1;
            //if (index >= elements.Count) index = elements.Count - 1;
            //return index;
        }
        
       
        protected virtual void UpdateRects()
        {
            ///for (int i = startIndex; i < endIndex; i++)
            ///{
            ///    Elements[i].UpdateRect(GetPos(new(0.5f)), GetSize(), new(0.5f));
            ///}
        }
        protected void DrawChildren()
        {
            foreach (var element in DisplayedElements)
            {
                element.Draw();
            }
            //for (int i = GetDisplayStartIndex(); i <= GetDisplayEndIndex(); i++)
            //{
            //    elements[i].Draw();
            //}

        }
        protected void UpdateChildren(float dt, Vector2 mousePosUI)
        {
            if (!DisabledSelection)
            {
                foreach (var element in DisplayedElements)
                {
                    element.Check(prevMousePos, mousePosUI, NavigationDisabled, MouseTolerance);
                    element.Update(dt, mousePosUI);
                }
            }
            prevMousePos = mousePosUI;
        }
        protected void UpdateDisplayedElements()
        {
            int prevCount = VisibleElements.Count;
            VisibleElements = Elements.FindAll(e => !e.Hidden);

            int count = GetDisplayEndIndex() - GetDisplayStartIndex();
            DisplayedElements = VisibleElements.GetRange(GetDisplayStartIndex(), count + 1);

            if (prevCount != VisibleElements.Count)
            {
                if (curDisplayIndex > VisibleElements.Count - DisplayCount) curDisplayIndex = VisibleElements.Count - DisplayCount;
                if (curDisplayIndex < 0) curDisplayIndex = 0;
            }
        }
        
        //protected UIElement? FindClosestNeighbor(UIElement current)
        //{
        //    List<UIElement> neighbors = GetDisplayedAvailableElements();
        //    if (!neighbors.Contains(current))
        //    {
        //        float closestDisSq = float.PositiveInfinity;
        //        UIElement? closest = null;
        //        foreach (var neighbor in neighbors)
        //        {
        //            float disSq = (current.GetPos(new(0.5f)) - neighbor.GetPos(new(0.5f))).LengthSquared();
        //            if(disSq < closestDisSq)
        //            {
        //                closestDisSq = disSq;
        //                closest = neighbor;
        //            }
        //        }
        //        return closest;
        //    }
        //    return current;
        //}
        //protected int FindClosestNeighbor(List<UIElement> available, int lastSelectedIndex)
        //{
        //    if (available.Count <= 0) return -1;
        //    if (available.Count == 1) return 0;
        //    if (available.Count == 2) return 
        //    if(lastSelectedIndex > 0)
        //    {
        //
        //    }
        //    else
        //    {
        //
        //    }
        //    return 0;
        //}
        
        protected int CheckDirection(List<UIElement> neighbors, UIElement current, UINeighbors.NeighborDirection dir)
        {
            var neighbor = current.Neighbors.GetNeighbor(dir);
            if (neighbor != null && !neighbor.DisabledSelection && !neighbor.Hidden && neighbors.Contains(neighbor))
            {
                return neighbors.IndexOf(neighbor);
            }
            else return FindNeighbor(neighbors, current, dir);
        }
        protected int FindNeighbor(List<UIElement> neighbors, UIElement current, UINeighbors.NeighborDirection dir)
        {
            
            if (neighbors.Count <= 0) return -1;
            if (neighbors.Count == 1) return 0;
            //{
            //    if (CheckNeighborDistance(current, neighbors[0], dir) < float.PositiveInfinity) return neighbors[0];
            //    else return null;
            //}
            int closestIndex = -1;
            float closestDistance = float.PositiveInfinity;
            for (int i = 0; i < neighbors.Count; i++)
            {
                float dis = CheckNeighborDistance(current, neighbors[i], dir);
                if (dis < closestDistance)
                {
                    closestDistance = dis;
                    closestIndex = i;
                }
            }

            if (closestIndex < 0 || closestIndex >= neighbors.Count) return -1;
            return closestIndex;
        }
        protected float CheckNeighborDistance(UIElement current, UIElement neighbor, UINeighbors.NeighborDirection dir)
        {
            if (neighbor == null) return float.PositiveInfinity;
            Vector2 pos = GetDirectionPosition(current, dir);
            Vector2 otherPos = GetDirectionPosition(neighbor, dir);
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
        protected Vector2 GetDirectionPosition(UIElement element, UINeighbors.NeighborDirection dir)
        {
            Rectangle self = element.GetRect(new(0f));
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
        protected void UpdateNavigation(float dt)
        {
            //if (!Selected || InputDisabled || Disabled || !selectable) return;
            if (NavigationDisabled || DisabledSelection) return;
            CleanSelectedElement();
            UpdateSelectedElement();
            //var newStartElement = GetNavigationDefaultElement();
            //if (newStartElement != NavigationStartElement) NavigationStartElement = newStartElement;
            //
            //if (SelectedElement != null)
            //{
            //    if (!SelectedElement.Selected)
            //    {
            //        if (SelectedElement.DisabledSelection || SelectedElement.Hidden)
            //        {
            //
            //            if (NavigationStartElement != null)
            //            {
            //                SelectedElement = NavigationStartElement;
            //                SelectedElement.Select();
            //                NewElementSelected?.Invoke(SelectedElement);
            //            }
            //        }
            //        else SelectedElement.Selected = true;
            //    }
            //}
            //else
            //{
            //    if (NavigationStartElement != null)
            //    {
            //        SelectedElement = NavigationStartElement;
            //        SelectedElement.Select();
            //        NewElementSelected?.Invoke(SelectedElement);
            //    }
            //}


            if (dirInputTimer > 0f)
            {
                dirInputTimer -= dt;
                if (dirInputTimer <= 0f) dirInputTimer = 0f;
            }
        }
        protected void CleanSelectedElement()
        {
            if (SelectedElement != null)
            {
                if (SelectedElement.DisabledSelection || SelectedElement.Hidden)
                {
                    SelectedElement = null;
                    //lastSelectedElement = null;
                }
            }
        }
        protected void UpdateSelectedElement()
        {
            if (!NavigationDisabled)
            {
                if (SelectedElement == null)
                {
                    var available = GetDisplayedAvailableElements();
                    if(available.Count > 0)
                    {
                        if (lastSelectedIndex >= 0 && lastSelectedIndex < available.Count)
                        {
                            SelectedElement = available[lastSelectedIndex];
                            SelectedElement.Select();
                        }
                        else
                        {
                            if (lastSelectedIndex >= available.Count)
                            {
                                lastSelectedIndex = available.Count - 1;
                                SelectedElement = available[lastSelectedIndex];
                                SelectedElement.Select();
                            }
                            else
                            {
                                lastSelectedIndex = 0;
                                SelectedElement = available[0];
                                SelectedElement.Select();
                            }
                        }
                    }
                }
            }
        }

        //protected UIElement? GetNavigationDefaultElement()
        //{
        //    var available = GetDisplayedAvailableElements();
        //    if (available.Count > 0) return available[0];
        //    return null;
        //}
        private void OnUIElementSelected(UIElement element)
        {
            if (element != SelectedElement)
            {
                var available = GetDisplayedAvailableElements();
                int index = available.IndexOf(element);
                if (index >= 0)
                {
                    if (SelectedElement != null) SelectedElement.Deselect();
                    lastSelectedIndex = index;
                    SelectedElement = element;
                    SelectedElement.Select();
                }

            }
        }


        public static void AlignUIElementsHorizontal(Rectangle rect, List<UIElement> elements, int displayCount = -1, float gapRelative = 0f, float elementMaxSizeX = 1f, float elementMaxSizeY = 1f)
        {
            Vector2 startPos = new(rect.x, rect.y);
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

            float totalWidth = rect.width;
            float gapSize = totalWidth * gapRelative;
            float elementWidth = (totalWidth - gaps * gapSize) / stretchFactorTotal;
            Vector2 offset = new(0f, 0f);
            foreach (var element in elements)
            {
                float width = elementWidth * element.StretchFactor;
                Vector2 size = new(width, rect.height);
                Vector2 maxSize = maxElementSizeRel * new Vector2(rect.width, rect.height);
                if (maxSize.X > 0f) size.X = MathF.Min(size.X, maxSize.X);
                if (maxSize.Y > 0f) size.Y = MathF.Min(size.Y, maxSize.Y);
                element.UpdateRect(startPos + offset, size, new(0f));
                offset += new Vector2(gapSize + width, 0f);
            }

        }
        public static void AlignUIElementsVertical(Rectangle rect, List<UIElement> elements, int displayCount = -1, float gapRelative = 0f, float elementMaxSizeX = 1f, float elementMaxSizeY = 1f)
        {
            Vector2 startPos = new(rect.x, rect.y);
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

            float totalHeight = rect.height;
            float gapSize = totalHeight * gapRelative;
            float elementHeight = (totalHeight - gaps * gapSize) / stretchFactorTotal;
            Vector2 offset = new(0f, 0f);
            foreach (var element in elements)
            {
                float height = elementHeight * element.StretchFactor;
                Vector2 size = new(rect.width, height);
                Vector2 maxSize = maxElementSizeRel * new Vector2(rect.width, rect.height);
                if (maxSize.X > 0f) size.X = MathF.Min(size.X, maxSize.X);
                if (maxSize.Y > 0f) size.Y = MathF.Min(size.Y, maxSize.Y);
                element.UpdateRect(startPos + offset, size, new(0f));
                offset += new Vector2(0f, gapSize + height);
            }

        }
        public static void AlignUIElementsGrid(Rectangle rect, List<UIElement> elements, int columns, int rows, float hGapRelative = 0f, float vGapRelative = 0f, bool leftToRight = true)
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
            int count = columns * rows;
            if (elements.Count < count) count = elements.Count;
            for (int i = 0; i < count; i++)
            {
                var item = elements[i];
                var coords = SUtils.TransformIndexToCoordinates(i, rows, columns, leftToRight);

                item.UpdateRect(startPos + hGap * coords.col + vGap * coords.row, elementSize, new(0f));
            }
        }
    }
}
