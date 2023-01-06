using System.Numerics;
using Raylib_CsLo;

namespace ShapeUI
{
    public class Button : UIElementSelectable
    {
        protected Alignement animationAlignement = Alignement.CENTER;
        protected Vector2 offset = new();
        protected Vector2 sizeOffset = new(1f, 1f);
        protected EaseHandler easeHandler = new();

        protected UISelectionColors stateColors = new();

        public void SetAnimationAlignment(Alignement newAlignement) { animationAlignement = newAlignement; }

        public void SetStateColors(UISelectionColors newColors) { this.stateColors = newColors; }

        public override bool IsAutomaticDetectionDirectionEnabled(UINeighbors.NeighborDirection dir)
        {
            switch (dir)
            {
                case UINeighbors.NeighborDirection.TOP: return true;
                case UINeighbors.NeighborDirection.RIGHT: return true;
                case UINeighbors.NeighborDirection.BOTTOM: return true;
                case UINeighbors.NeighborDirection.LEFT: return true;
                default: return true;
            }
        }
        public override void Update(float dt, Vector2 mousePosUI)
        {
            base.Update(dt, mousePosUI);
            easeHandler.Update(dt);
            if (easeHandler.HasChain("offset"))
            {
                var result = easeHandler.GetVector2("offset");
                offset = result;
            }
            else { offset.X = 0f; offset.Y = 0f; }

            if (easeHandler.HasChain("sizeUp"))
            {
                var result = easeHandler.GetVector2("sizeUp");
                sizeOffset = result;
            }
        }
        public override void Draw(Vector2 uisSize, Vector2 stretchFactor)
        {
            Color color = stateColors.baseColor;
            if (disabled) color = stateColors.disabledColor;
            else if (pressed || mousePressed) color = stateColors.pressedColor;
            else if (hovered) color = stateColors.hoveredColor;


            DrawButton(color);
            //Rectangle rect = GetRect(Alignement.TOPLEFT);
            //Rectangle offsetRect = new Rectangle(rect.X + offset.X, rect.Y + offset.Y, rect.width * sizeOffset.X, rect.height * sizeOffset.Y);
            //DrawRectangleRec(offsetRect, color);
            //if (selected) DrawRectangleV(new Vector2(offsetRect.X, offsetRect.y + offsetRect.height * 0.9f), new Vector2(offsetRect.width, offsetRect.height * 0.1f), stateColors.selectedColor);
        }

        protected virtual void DrawButton(Color color)
        {
            Vector2 pos = GetPos(animationAlignement);
            Vector2 size = GetSize();
            Rectangle animationRect = Utils.ConstructRectangle(pos + offset, size * sizeOffset, animationAlignement);

            DrawRectangleRec(animationRect, color);
            if (selected) DrawRectangleV(new Vector2(animationRect.X, animationRect.y + animationRect.height * 0.9f), new Vector2(animationRect.width, animationRect.height * 0.1f), stateColors.selectedColor);
        }

        protected virtual void PlayAnimation(string name)
        {
            switch (name)
            {
                case "pressed":
                    if (easeHandler.HasChain("sizeUp")) return;
                    easeHandler.AddChain(
                        "sizeUp",
                        new Vector2(1, 1),
                        new EaseOrder(0.15f, new Vector2(0.15f, 0.15f), EasingType.QUAD_IN),
                        new EaseOrder(0.05f, new Vector2(-0.15f, -0.15f), EasingType.QUAD_OUT)
                        );
                    break;

                case "hovered":
                    if (easeHandler.HasChain("offset")) return;
                    easeHandler.AddChain(
                        "offset",
                        new Vector2(0, 0),
                        new EaseOrder(0.1f, new Vector2(50, 0), EasingType.QUAD_OUT),
                        new EaseOrder(0.2f, new Vector2(-75, 0), EasingType.BACK_IN),
                        new EaseOrder(0.1f, new Vector2(25, 0), EasingType.LINEAR_IN)
                        );
                    break;

                case "selected":
                    if (easeHandler.HasChain("offset")) return;
                    easeHandler.AddChain(
                        "offset",
                        new Vector2(0, 0),
                        new EaseOrder(0.1f, new Vector2(-50, 0), EasingType.QUAD_OUT),
                        new EaseOrder(0.2f, new Vector2(75, 0), EasingType.BACK_IN),
                        new EaseOrder(0.1f, new Vector2(-25, 0), EasingType.LINEAR_IN)
                        );
                    break;
            }
        }
        public override void PressedChanged(bool pressed)
        {
            if (pressed)
            {
                PlayAnimation("pressed");
            }
            else
            {
                sizeOffset.X = 1f;
                sizeOffset.Y = 1f;
            }
        }
        public override void MousePressedChanged(bool pressed)
        {
            if (pressed)
            {
                PlayAnimation("pressed");
            }
            else
            {
                sizeOffset.X = 1f;
                sizeOffset.Y = 1f;
            }
        }
        public override void HoveredChanged(bool hovered)
        {
            if (!hovered) return;
            PlayAnimation("hovered");
        }
        public override void SelectedChanged(bool selected)
        {
            if (!selected) return;
            PlayAnimation("selected");
        }
    
    }

}
