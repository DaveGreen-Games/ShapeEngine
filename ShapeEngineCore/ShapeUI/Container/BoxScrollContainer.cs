using Raylib_CsLo;
using ShapeInput;
using ShapeLib;
using System.Numerics;

namespace ShapeUI.Container
{
    public class BoxScrollContainer : BoxDisplayContainer
    {
        bool isMouseInside = false;
        bool isScrolling = false;
        float scrollMovement = 0f;
        float scrollSpeed = 1f;
        float scrollThreshold = 0.05f;

        float scrollBarSize = 0.08f;
        bool reversedScrollBarLocation = true;
        bool isInsideScrollBar = false;
        bool isInsideScrollBarHandle = false;
        bool isScrollBarHandleDragging = false;
        Rectangle scrollBarRect;
        Rectangle scrollBarHandleRect;
        Color scrollBarColor = ORANGE;
        Color scrollHandleColor = WHITE;
        Color scrollHandleHoveredColor = GREEN;
        Vector2 handleDragOffset = new();

        public int gamepladSlot = 0;

        public BoxScrollContainer(int displayCount, float gapRelative)
            : base(displayCount, gapRelative)
        {
        }
        public BoxScrollContainer(int displayCount, float gapRelative, List<UIElement> children)
            : base(displayCount, gapRelative, children)
        {
        }
        public BoxScrollContainer(int displayCount, float gapRelative, params UIElement[] children)
            : base(displayCount, gapRelative, children)
        {
        }
        public BoxScrollContainer(int displayCount, float gapRelative, bool vContainer)
            : base(displayCount, gapRelative, vContainer)
        {
        }
        public BoxScrollContainer(int displayCount, float gapRelative, bool vContainer, List<UIElement> children)
            : base(displayCount, gapRelative, vContainer, children)
        {
        }
        public BoxScrollContainer(int displayCount, float gapRelative, bool vContainer, params UIElement[] children)
            : base(displayCount, gapRelative, vContainer, children)
        {
        }
        protected override void OnDirectionInput(UINeighbors.NeighborDirection dir, UIElement? selected, UIElement? nextSelected)
        {
            if (nextSelected == null) return;

            if (!isScrollBarHandleDragging && !isScrolling)
            {
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
        }

        public float GetScrollSpeed() { return scrollSpeed; }

        public void SetScrollSpeed(float newScrollSpeed) { scrollSpeed = newScrollSpeed; }
        public void SetScrollBarColors(Color scrollBar, Color scrollBarHandle, Color scrollBarHandleHovered)
        {
            scrollBarColor = scrollBar;
            scrollHandleColor = scrollBarHandle;
            scrollHandleHoveredColor = scrollBarHandleHovered;
        }

        public bool IsScrollingEnabled() { return scrollSpeed > 0f; }
        public bool IsScrollBarEnabled() { return scrollBarSize > 0f && scrollHandleColor.a > 0 && children.Count > 0 && children.Count > displayCount; }


        public override void Update(float dt, Vector2 mousePosUI)
        {
            //scrollBarMovement = 0f;
            movementDir = 0;
            isInsideScrollBar = false;
            isInsideScrollBarHandle = false;
            isScrolling = false;
            if (InputHandler.IsKeyboardMouse())
            {
                if (InputHandler.IsKeyboardOnlyMode())
                {
                    isMouseInside = false;
                    //MoveThroughElements();
                    UpdateRects(GetRect(new(0f)), curIndex, GetEndIndex());
                }
                else
                {
                    isMouseInside = IsPointInside(mousePosUI);

                    if (isMouseInside)
                    {
                        if (IsScrollingEnabled())
                        {
                            Vector2 mouseWheel = InputHandler.GetMouseWheelMovementV(true);
                            float scrollMove = vContainer ? mouseWheel.Y : mouseWheel.X;
                            Scroll(scrollMove, dt);
                        }
                    }

                    if (IsScrollBarEnabled())
                    {
                        CalculateScrollBarRects(mousePosUI);
                    }

                    if (InputHandler.IsReleased(-1, InputHandler.UI_SelectMouse))
                    {
                        EndScrollBarHandleDrag();
                    }

                    if (isInsideScrollBar && !isScrolling)
                    {
                        if (InputHandler.IsPressed(-1, InputHandler.UI_SelectMouse))
                        {
                            CheckScrollBarClick(mousePosUI);
                        }
                    }

                    if (isScrollBarHandleDragging)
                    {
                        DragScrollBarHandle(mousePosUI);
                    }

                    //if (!isScrollBarHandleDragging && !isScrolling)
                    //{
                    //    MoveThroughElements();
                    //}

                    //adjust rect to scroll bar
                    if (isScrollBarHandleDragging || (isMouseInside && IsScrollBarEnabled()))
                    {
                        var rect = GetRect(new(0f));

                        if (vContainer)
                        {
                            float width = rect.width * scrollBarSize;
                            if (reversedScrollBarLocation) rect.width -= width;
                            else
                            {
                                rect.x += width;
                                rect.width -= width;
                            }
                        }
                        else
                        {
                            float height = rect.height * scrollBarSize;
                            if (reversedScrollBarLocation) rect.height -= height;
                            else
                            {
                                rect.y += height;
                                rect.height -= height;
                            }
                        }

                        UpdateRects(rect, curIndex, GetEndIndex());
                    }
                    else UpdateRects(GetRect(new(0f)), curIndex, GetEndIndex());
                }
            }
            else if (InputHandler.IsGamepad())
            {

                //gamepad scrolling
                if (IsScrollingEnabled() && IsSelected())
                {
                    Vector2 axis = InputHandler.GetGamepadAxisRight(gamepladSlot, 0.25f, true);
                    float scrollMove = vContainer ? axis.Y : axis.X;
                    Scroll(scrollMove, dt);
                }

                UpdateRects(GetRect(new(0f)), curIndex, GetEndIndex());
            }

            for (int i = curIndex; i < GetEndIndex(); i++)
            {
                children[i].Update(dt, mousePosUI);
            }
        }
        public override void Draw(Vector2 uiSize, Vector2 stretchFactor)
        {
            if (HasBackground()) DrawBackground(GetRect(new(0f)), bgColor);

            DrawScrollBar();

            for (int i = curIndex; i < GetEndIndex(); i++)
            {
                children[i].Draw(uiSize, stretchFactor);
            }
        }
        protected virtual void DrawScrollBar()
        {
            if (isScrollBarHandleDragging || (isMouseInside && IsScrollBarEnabled()))
            {
                //Vector2 slotSize = GetSlotSize();
                //if (vContainer)
                //{
                //    //draw background
                //    Vector2 start;
                //    //float slotHeight = scrollBarRect.height / children.Count;
                //    //float slotWidth = scrollBarRect.width;
                //    Vector2 size = new(slotSize.X, slotSize.Y * 0.9f);
                //
                //    if (reversedScrollBarLocation)//left
                //    {
                //        start = GetPos(Alignement.TOPRIGHT) - new Vector2(slotSize.X, 0f);
                //    }
                //    else
                //    {
                //        start = GetPos(Alignement.TOPLEFT);
                //
                //    }
                //    for (int i = 0; i < children.Count; i++)
                //    {
                //        Drawing.DrawRectangle(Utils.ConstructRectangle(start + new Vector2(0, slotSize.Y * i), size, Alignement.TOPLEFT), scrollBarColor);
                //    }
                //}
                //else
                //{
                //    //draw background
                //    Vector2 start;
                //    //float slotHeight = scrollBarRect.height;
                //    //float slotWidth = scrollBarRect.width / children.Count;
                //    Vector2 size = new(slotSize.X * 0.9f, slotSize.Y);
                //
                //    if (reversedScrollBarLocation)//bottom
                //    {
                //        start = GetPos(Alignement.BOTTOMLEFT) - new Vector2(0, slotSize.Y);
                //    }
                //    else
                //    {
                //        start = GetPos(Alignement.TOPLEFT);
                //
                //    }
                //    for (int i = 0; i < children.Count; i++)
                //    {
                //        Drawing.DrawRectangle(Utils.ConstructRectangle(start + new Vector2(slotSize.X * i, 0f), size, Alignement.TOPLEFT), scrollBarColor);
                //    }
                //}

                SDrawing.DrawRectangle(scrollBarRect, scrollBarColor);

                if (isInsideScrollBarHandle || isScrollBarHandleDragging)
                {
                    SDrawing.DrawRectangle(scrollBarHandleRect, scrollHandleHoveredColor);
                }
                else
                {
                    SDrawing.DrawRectangle(scrollBarHandleRect, scrollHandleColor);
                }
            }
        }

        private void Scroll(float movement, float dt)
        {
            isScrolling = MathF.Abs(movement) > 0f;
            if (movement > 0 && scrollMovement < 0) scrollMovement = 0f;
            else if (movement < 0 && scrollMovement > 0) scrollMovement = 0f;
            scrollMovement += movement * dt * scrollSpeed;
            if (scrollMovement > scrollThreshold)
            {
                ChangeCurIndex(1);
                scrollMovement = 0f;
            }
            else if (scrollMovement < -scrollThreshold)
            {
                ChangeCurIndex(-1);
                scrollMovement = 0f;
            }
        }

        protected Vector2 GetSlotSize()
        {
            if (vContainer)
            {
                float slotWidth = scrollBarRect.width;
                float slotHeight = scrollBarRect.height / children.Count;
                return new(slotWidth, slotHeight);
            }
            else
            {
                float slotWidth = scrollBarRect.width / children.Count;
                float slotHeight = scrollBarRect.height;
                return new(slotWidth, slotHeight);
            }
        }
        protected int CalculateIndex(float value, bool ceil = true)
        {
            Vector2 slotSize = GetSlotSize();
            if (vContainer)
            {
                float index = (value - GetPos(new(0f)).Y) / slotSize.Y;
                if (ceil) return (int)MathF.Ceiling(index);
                else return (int)MathF.Floor(index);
            }
            else
            {
                float index = (value - GetPos(new(0f)).X) / slotSize.X;
                if (ceil) return (int)MathF.Ceiling(index);
                else return (int)MathF.Floor(index);
            }
        }

        private void DragScrollBarHandle(Vector2 mousePosUI)
        {
            if (vContainer)
            {
                //float prevY = scrollBarHandleRect.y;
                //float slotHeight = scrollBarRect.height / children.Count;
                float y = Clamp(mousePosUI.Y + handleDragOffset.Y, scrollBarRect.y, scrollBarRect.y + scrollBarRect.height - scrollBarHandleRect.height);
                scrollBarHandleRect.y = y;
                //SetIndex((int)((y - GetPos(Alignement.TOPLEFT).Y) / slotHeight));
                SetIndex(CalculateIndex(y));
            }
            else
            {
                //float prevX = scrollBarHandleRect.x;
                //float slotWidth = scrollBarRect.width / children.Count;
                float x = Clamp(mousePosUI.X + handleDragOffset.X, scrollBarRect.X, scrollBarRect.X + scrollBarRect.width - scrollBarHandleRect.width);
                scrollBarHandleRect.x = x;
                //SetIndex((int)((x - GetPos(Alignement.TOPLEFT).X) / slotWidth));
                SetIndex(CalculateIndex(x));
            }
        }
        private void EndScrollBarHandleDrag()
        {
            handleDragOffset = new(0f);
            isScrollBarHandleDragging = false;
        }
        private void StartScrollBarHandleDrag(Vector2 mousePosUI)
        {
            handleDragOffset = new Vector2(scrollBarHandleRect.X, scrollBarHandleRect.Y) - mousePosUI;
            isScrollBarHandleDragging = true;
        }
        private void CheckScrollBarClick(Vector2 mousePosUI)
        {
            if (isInsideScrollBarHandle)
            {
                StartScrollBarHandleDrag(mousePosUI);
            }
            else
            {
                SetScrollBarHandlePosition(mousePosUI);
            }
        }
        private void SetScrollBarHandlePosition(Vector2 pos)
        {
            if (vContainer)
            {
                if (pos.Y < scrollBarHandleRect.Y)
                {
                    float y = Clamp(pos.Y, scrollBarRect.y, scrollBarRect.y + scrollBarRect.height - scrollBarHandleRect.height);
                    scrollBarHandleRect.y = y;
                    SetIndex(CalculateIndex(y, false));
                }
                else if (pos.Y > scrollBarHandleRect.Y + scrollBarHandleRect.height)
                {
                    float y = Clamp(pos.Y - scrollBarHandleRect.height, scrollBarRect.y, scrollBarRect.y + scrollBarRect.height - scrollBarHandleRect.height);
                    scrollBarHandleRect.y = y;
                    SetIndex(CalculateIndex(y, true));
                }
            }
            else
            {
                if (pos.X < scrollBarHandleRect.x)
                {
                    float x = Clamp(pos.X, scrollBarRect.x, scrollBarRect.x + scrollBarRect.width - scrollBarHandleRect.width);
                    scrollBarHandleRect.x = x;
                    SetIndex(CalculateIndex(x, false));
                }
                else if (pos.X > scrollBarHandleRect.x + scrollBarHandleRect.width)
                {
                    float x = Clamp(pos.X - scrollBarHandleRect.width, scrollBarRect.x, scrollBarRect.x + scrollBarRect.width - scrollBarHandleRect.width);
                    scrollBarHandleRect.x = x;
                    SetIndex(CalculateIndex(x, true));
                }
            }

            //if (vContainer)
            //{
            //    //float prevY = scrollBarHandleRect.y;
            //    float slotHeight = scrollBarRect.height / children.Count;
            //    float y = Clamp(pos.Y, scrollBarRect.y, scrollBarRect.y + scrollBarRect.height - scrollBarHandleRect.height);
            //    scrollBarHandleRect.y = y;
            //    SetIndex((int)((y - GetPos(Alignement.TOPLEFT).Y) / slotHeight));
            //}
            //else
            //{
            //    //float prevX = scrollBarHandleRect.x;
            //    float slotWidth = scrollBarRect.width / children.Count;
            //    float x = Clamp(pos.X, scrollBarRect.x, scrollBarRect.x + scrollBarRect.width - scrollBarHandleRect.width);
            //    scrollBarHandleRect.x = x;
            //    SetIndex((int)((x - GetPos(Alignement.TOPLEFT).X) / slotWidth));
            //}

        }
        private void CalculateScrollBarRects(Vector2 mousePosUI)
        {
            var rect = GetRect(new(0f));
            Vector2 slotSize = GetSlotSize();
            if (vContainer)
            {
                Vector2 size = new Vector2(rect.width * scrollBarSize, rect.height);

                //float slotHeight = rect.height / children.Count;
                Vector2 handleSize = new(size.X, slotSize.Y * displayCount);
                Vector2 handlePosOffset = new(0, slotSize.Y * curIndex);
                if (reversedScrollBarLocation)
                {
                    Vector2 alignement = new(1, 0);
                    scrollBarRect = SRect.ConstructRect(GetPos(alignement), size, alignement);
                    scrollBarHandleRect = SRect.ConstructRect(GetPos(alignement) + handlePosOffset, handleSize, alignement);
                }
                else
                {
                    Vector2 alignement = new(0);
                    scrollBarRect = SRect.ConstructRect(GetPos(alignement), size, alignement);
                    scrollBarHandleRect = SRect.ConstructRect(GetPos(alignement) + handlePosOffset, handleSize, alignement);
                }
            }
            else
            {
                Vector2 size = new Vector2(rect.width, rect.height * scrollBarSize);

                //float slotWidth = rect.width / children.Count;
                Vector2 handleSize = new(slotSize.X * displayCount, size.Y);
                Vector2 handlePosOffset = new(slotSize.X * curIndex, 0);
                if (reversedScrollBarLocation)
                {
                    Vector2 alignement = new(0, 1);
                    scrollBarRect = SRect.ConstructRect(GetPos(alignement), size, alignement);
                    scrollBarHandleRect = SRect.ConstructRect(GetPos(alignement) + handlePosOffset, handleSize, alignement);
                }
                else
                {
                    Vector2 alignement = new(0);
                    scrollBarRect = SRect.ConstructRect(GetPos(alignement), size, alignement);
                    scrollBarHandleRect = SRect.ConstructRect(GetPos(alignement) + handlePosOffset, handleSize, alignement);
                }
            }

            if (isMouseInside)
            {
                isInsideScrollBar = Raylib.CheckCollisionPointRec(mousePosUI, scrollBarRect);
                if (isInsideScrollBar)
                {
                    isInsideScrollBarHandle = Raylib.CheckCollisionPointRec(mousePosUI, scrollBarHandleRect);
                }
            }
        }
    }

}
