
using Raylib_CsLo;
using System.Numerics;

namespace ShapeUI
{
    public class UINavigator
    {
        public event Action<UIElement>? NewElementSelected;

        private List<UIElement> elements = new();

        public UIElement? StartElement { get; protected set; } = null;
        public UIElement? SelectedElement { get; protected set; } = null;
        public bool InputDisabled { get; protected set; } = true;
        public float InputInterval { get; set; } = 0.3f;
        public UINeighbors.NeighborDirection LastInputDirection { get; protected set; } = UINeighbors.NeighborDirection.NONE;
        public UINeighbors.NeighborDirection CurInputDirection { get; protected set; } = UINeighbors.NeighborDirection.NONE;

        private float dirInputTimer = -1f;


        public UINavigator(params UIElement[] newElements)
        {
            RegisterElements(newElements);
        }


        public void RegisterElements(params UIElement[] newElements)
        {
            if (newElements.Length > 0)
            {
                foreach (var element in elements)
                {
                    element.Selected = false;
                    element.WasSelected -= OnUIElementSelected;
                }
                elements.Clear();

                foreach (var element in newElements)
                {
                    element.Selected = false;
                    element.WasSelected += OnUIElementSelected;
                }
                
                elements = newElements.ToList();

                if (SelectedElement != null)
                {
                    if (SelectedElement.Disabled || !SelectedElement.Selectable || !newElements.Contains(SelectedElement))
                    {
                        SelectedElement.Selected = false;
                        SelectedElement = null;
                    }
                }

                StartElement = GetStartElement();
            }
            else
            {
                if (SelectedElement != null)
                {
                    SelectedElement.Selected = false;
                    SelectedElement = null;
                }
                StartElement = null;
                foreach (var element in elements)
                {
                    element.Selected = false;
                    element.WasSelected -= OnUIElementSelected;
                }
                elements.Clear();
            }

        }
        
        public void Reset()
        {
            if (elements.Count > 0)
            {
                //foreach (var element in elements) { element.Deselect(); }

                if(SelectedElement != null)
                {
                    SelectedElement.Selected = false;
                    SelectedElement = null;
                }
                
                StartElement = GetStartElement();

                if (StartElement != null)
                {
                    SelectedElement = StartElement;
                    StartElement.Select();
                    NewElementSelected?.Invoke(StartElement);
                }
            }
        }
        public void Close()
        {
            elements.Clear();
            SelectedElement = null;
            StartElement = null;
        }
        
        public void StartNavigation()
        {
            if (InputDisabled)
            {
                InputDisabled = false;
                
                StartElement = GetStartElement();
                if(SelectedElement == null && StartElement != null)
                {
                    SelectedElement = StartElement;
                    StartElement.Select();
                    NewElementSelected?.Invoke(StartElement);
                }
            }
        }
        public void StopNavigation()
        {
            if (!InputDisabled)
            {
                InputDisabled = true;
                if(SelectedElement != null)
                {
                    SelectedElement.Selected = false;
                    SelectedElement = null;
                }
            }
        }

        public UIElement? SelectNextElement()
        {
            if (SelectedElement == null) return null;
            var available = GetAvailableElements();
            if(available.Count <= 0) return null; 
            int index = available.IndexOf(SelectedElement);
            index += 1;
            if (index >= available.Count) index = 0;
            UIElement next = available[index];
            if (next != SelectedElement)
            {
                SelectedElement.Deselect();
                SelectedElement = next;
                SelectedElement.Select();
                NewElementSelected?.Invoke(StartElement);
                return next;
            }
            else return SelectedElement;
        }
        public UIElement? SelectPreviousElement() 
        {
            if (SelectedElement == null) return null;
            var available = GetAvailableElements();
            if (available.Count <= 0) return null;
            int index = available.IndexOf(SelectedElement);
            index -= 1;
            if (index < 0) index = available.Count - 1;
            UIElement next = available[index];
            if (next != SelectedElement)
            {
                SelectedElement.Deselect();
                SelectedElement = next;
                SelectedElement.Select();
                NewElementSelected?.Invoke(StartElement);
                return next;
            }
            else return SelectedElement;
        }
        public void Navigate(UINeighbors.NeighborDirection inputDirection)
        {
            if (InputDisabled) return;

            if (inputDirection == UINeighbors.NeighborDirection.NONE)
            {
                LastInputDirection = UINeighbors.NeighborDirection.NONE;
                CurInputDirection = UINeighbors.NeighborDirection.NONE;
                dirInputTimer = -1;
                return;
            }
            
            LastInputDirection = CurInputDirection;
            CurInputDirection = inputDirection;
            
            if (dirInputTimer > 0f)
            {
                if (LastInputDirection != CurInputDirection)
                {
                    dirInputTimer = -1;
                }
                else return;
            }
            
            if (SelectedElement == null) return;
            
            UIElement? newSelected = null;

            if (inputDirection == UINeighbors.NeighborDirection.TOP)
            {
                newSelected = CheckDirection(SelectedElement, inputDirection);
                if (InputInterval > 0f) dirInputTimer = InputInterval;
            }
            else if (inputDirection == UINeighbors.NeighborDirection.RIGHT)
            {
                newSelected = CheckDirection(SelectedElement, inputDirection);
                if (InputInterval > 0f) dirInputTimer = InputInterval;
            }
            else if (inputDirection == UINeighbors.NeighborDirection.BOTTOM)
            {
                newSelected = CheckDirection(SelectedElement, inputDirection);
                if (InputInterval > 0f) dirInputTimer = InputInterval;
            }
            else if (inputDirection == UINeighbors.NeighborDirection.LEFT)
            {
                newSelected = CheckDirection(SelectedElement, inputDirection);
                if (InputInterval > 0f) dirInputTimer = InputInterval;
            }
            
            if (newSelected != null)
            {
                SelectedElement.Deselect();
                SelectedElement = newSelected;
                SelectedElement.Select();
                NewElementSelected?.Invoke(StartElement);
            }
        }
        public void Update(float dt)
        {
            if (InputDisabled) return;

            var newStartElement = GetStartElement();
            if (newStartElement != StartElement) StartElement = newStartElement;

            if(SelectedElement != null)
            {
                if (!SelectedElement.Selected)
                {
                    if(SelectedElement.Disabled || !SelectedElement.Selectable)
                    {
                        if(StartElement != null)
                        {
                            SelectedElement = StartElement;
                            SelectedElement.Select();
                            NewElementSelected?.Invoke(SelectedElement);
                        }
                    }
                    else SelectedElement.Selected = true;
                }
            }
            else
            {
                if(StartElement != null)
                {
                    SelectedElement = StartElement;
                    SelectedElement.Select();
                    NewElementSelected?.Invoke(SelectedElement);
                }
            }


            if (dirInputTimer > 0f)
            {
                dirInputTimer -= dt;
                if (dirInputTimer <= 0f) dirInputTimer = 0f;
            }
        }

        protected UIElement? GetStartElement()
        {
            var available = GetAvailableElements();
            if(available.Count > 0) return available[0];
            return null;
        }
        private void OnUIElementSelected(UIElement element)
        {
            if (element == SelectedElement) return; //valid selection from the navigator
            else
            {
                if (SelectedElement != null) SelectedElement.Deselect();
                SelectedElement = element;
            }
        }
        
        protected List<UIElement> GetAvailableElements()
        {
            return elements.ToList().FindAll(e => !e.Disabled && e.Selectable);
        }
        protected UIElement? CheckDirection(UIElement current, UINeighbors.NeighborDirection dir)
        {
            var neighbor = current.Neighbors.GetNeighbor(dir);
            if (neighbor != null && !neighbor.Disabled && neighbor.Selectable)
            {
                //current.Deselect();
                //neighbor.Select();
                return neighbor;
            }
            else
            {
                var closest = FindNeighbor(current, dir);
                if (closest != null)
                {
                    //current.Deselect();
                    //closest.Select();
                    return closest;
                }
            }
            return null;
        }
        protected UIElement? FindNeighbor(UIElement current, UINeighbors.NeighborDirection dir)
        {
            if (elements == null || elements.Count <= 0) return null;
            List<UIElement> neighbors = elements.ToList().FindAll(e => e != current && !e.Disabled && e.Selectable);
            if (neighbors.Count <= 0) return null;
            if (neighbors.Count == 1)
            {
                if (CheckNeighborDistance(current, neighbors[0], dir) < float.PositiveInfinity) return neighbors[0];
                else return null;
            }
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

            if (closestIndex < 0 || closestIndex >= neighbors.Count) return null;
            return neighbors[closestIndex];
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

        //public event Action<UINeighbors2.NeighborDirection, UIElement, UIElement> SelectedChanged;

        //public delegate void DirectionInput(UINeighbors.NeighborDirection dir, UIElement? selected, UIElement? nextSelected);
        //public static event DirectionInput? OnDirectionInput;
        //public static event Action<UIElement>? OnSelectedItemUnregistered;
        //public UINavigator() { }
        //public void ClearRegister() { register.Clear(); }
        //public bool RegisterUIElement(UIElement2 element)
        //{
        //    return register.Add(element);
        //}
        //public bool UnregisterUIElement(UIElement2 element)
        //{
        //    bool removed = register.Remove(element);
        //    if (selected != null && selected == element)
        //    {
        //        DeselectUIElement();
        //        ///OnSelectedItemUnregistered?.Invoke(element);
        //    }
        //    else if (element.IsSelected())
        //    {
        //        element.Deselect();
        //        //OnSelectedItemUnregistered?.Invoke(element);
        //    }
        //    return removed;
        //}

        //public bool DeselectUIElement()
        //{
        //    if (Selected != null)
        //    {
        //        Selected.Deselect();
        //        Selected = null;
        //        return true;
        //    }
        //    return false;
        //}
        //public bool SelectUIElement(UIElement2 element)
        //{
        //    if (element != null && element.IsDisabled()) return false;
        //    if (Selected != null) Selected.Deselect();
        //    Selected = element;
        //    if (Selected != null) Selected.Select();
        //    return true;
        //}
    }

}
