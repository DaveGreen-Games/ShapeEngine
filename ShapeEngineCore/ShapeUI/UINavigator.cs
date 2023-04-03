
using Raylib_CsLo;
using System.Numerics;

namespace ShapeUI
{
    public class UINavigator
    {
        private HashSet<UIElement> register = new();

        public UIElement? StartElement { get; protected set; } = null;
        public UIElement? SelectedElement { get; protected set; } = null;
        public float InputInterval { get; set; } = 1f;
        public bool InputDisabled { get; set; } = false;
        public UINeighbors.NeighborDirection LastInputDirection { get; protected set; } = UINeighbors.NeighborDirection.NONE;

        private float dirInputTimer = -1f;


        public UINavigator(params UIElement[] elements)
        {
            RegisterElements(elements);
        }

        public void RegisterElements(params UIElement[] elements)
        {
            if (elements.Length > 0)
            {
                if (SelectedElement != null)
                {
                    SelectedElement.Deselect();
                    SelectedElement = null;
                }

                foreach (var element in register)
                {
                    element.Deselect();
                    element.WasSelected -= OnUIElementSelected;
                }
                register.Clear();

                foreach (var element in elements)
                {
                    element.Deselect();
                    element.WasSelected += OnUIElementSelected;
                }

                StartElement = elements[0];
                StartElement.Select();
                SelectedElement = StartElement;

                register = elements.ToHashSet();
            }
            else
            {
                if (SelectedElement != null)
                {
                    SelectedElement.Deselect();
                    SelectedElement = null;
                }
                StartElement = null;
                foreach (var element in register)
                {
                    element.Deselect();
                    element.WasSelected -= OnUIElementSelected;
                }
                register.Clear();
            }

        }
        public void Reset()
        {
            if (register.Count > 0)
            {
                foreach (var element in register) { element.Deselect(); }
                if (StartElement != null)
                {
                    StartElement.Select();
                    SelectedElement = StartElement;
                }
                else
                {

                    StartElement = register.First();
                    StartElement.Select();
                    SelectedElement = StartElement;
                }
            }
        }
        public void Close()
        {
            register.Clear();
            SelectedElement = null;
        }
        public void Update(float dt, UINeighbors.NeighborDirection inputDirection)
        {
            if (SelectedElement != null && !SelectedElement.Selected) SelectedElement.Selected = true;

            if (InputDisabled) return;

            if (SelectedElement == null)
            {
                if (StartElement == null) return;
                else
                {
                    SelectedElement = StartElement;
                    SelectedElement.Select();
                }
            }
            else
            {
                UIElement? newSelected = null;

                if (dirInputTimer > 0f)
                {
                    if (LastInputDirection == inputDirection)
                    {
                        dirInputTimer -= dt;
                        if (dirInputTimer <= 0f) dirInputTimer = 0f;
                    }
                    else
                    {
                        dirInputTimer = -1f;
                        LastInputDirection = UINeighbors.NeighborDirection.NONE;
                    }
                }
                else
                {
                    if (inputDirection == UINeighbors.NeighborDirection.TOP)// || (LastInputDirection == UINeighbors2.NeighborDirection.TOP && dirInputTimer == 0f))
                    {
                        LastInputDirection = UINeighbors.NeighborDirection.TOP;
                        newSelected = CheckDirection(SelectedElement, inputDirection);
                        if (InputInterval > 0f) dirInputTimer = InputInterval - dt;
                        //OnDirectionInput?.Invoke(lastDir, Selected, newSelected);
                    }
                    else if (inputDirection == UINeighbors.NeighborDirection.RIGHT)// || (LastInputDirection == UINeighbors2.NeighborDirection.RIGHT && dirInputTimer == 0f))
                    {
                        LastInputDirection = UINeighbors.NeighborDirection.RIGHT;
                        newSelected = CheckDirection(SelectedElement, inputDirection);
                        if (InputInterval > 0f) dirInputTimer = InputInterval - dt;
                        //OnDirectionInput?.Invoke(lastDir, Selected, newSelected);
                    }
                    else if (inputDirection == UINeighbors.NeighborDirection.BOTTOM)// || (LastInputDirection == UINeighbors2.NeighborDirection.BOTTOM && dirInputTimer == 0f))
                    {
                        LastInputDirection = UINeighbors.NeighborDirection.BOTTOM;
                        newSelected = CheckDirection(SelectedElement, inputDirection);
                        if (InputInterval > 0f) dirInputTimer = InputInterval - dt;
                        //OnDirectionInput?.Invoke(lastDir, Selected, newSelected);
                    }
                    else if (inputDirection == UINeighbors.NeighborDirection.LEFT)// || (LastInputDirection == UINeighbors2.NeighborDirection.LEFT && dirInputTimer == 0f))
                    {
                        LastInputDirection = UINeighbors.NeighborDirection.LEFT;
                        newSelected = CheckDirection(SelectedElement, inputDirection);
                        if (InputInterval > 0f) dirInputTimer = InputInterval - dt;
                        //OnDirectionInput?.Invoke(lastDir, Selected, newSelected);
                    }
                }


                if (newSelected != null)
                {
                    SelectedElement.Deselect();
                    SelectedElement = newSelected;
                    SelectedElement.Select();
                }
            }
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

        protected UIElement? CheckDirection(UIElement current, UINeighbors.NeighborDirection dir)
        {
            var neighbor = current.Neighbors.GetNeighbor(dir);
            if (neighbor != null)
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
            if (register == null || register.Count <= 0) return null;
            List<UIElement> neighbors = register.ToList().FindAll(e => e != current && !e.Disabled);
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
