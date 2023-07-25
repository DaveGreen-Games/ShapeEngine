using System.Numerics;

namespace ShapeEngine.Core
{
    public abstract class Scene : IScene
    {
        public bool CallUpdate { get; set; } = true;
        public bool CallHandleInput { get; set; } = true;
        public bool CallDraw { get; set; } = true;

        public virtual void Close() { }

        public virtual void Activate(IScene oldScene) { }
        public virtual void Deactivate() { }
        //public virtual void HandleInput(float dt, Vector2 mousePosGame, Vector2 mousePosUI) { }
        public virtual void Update(float dt, Vector2 mousePosGame, Vector2 mousePosUI) { }
        public virtual void Draw(Vector2 gameSIze, Vector2 mousePosGame) { }
        public virtual void DrawUI(Vector2 uiSize, Vector2 mousePosUI) { }
        public virtual void DrawToScreen(Vector2 screenSize, Vector2 mousePos) { }
        public virtual Area? GetCurArea() { return null; }
    }
    public sealed class SceneEmpty : Scene
    {
        public SceneEmpty() { }
    }

    /*
    public class Scene
    {


        protected bool paused = false;
        protected bool inputDisabled = false;
        protected bool hidden = false;

        public Scene() { }


        public virtual Area? GetCurArea() { return null; }


        public virtual void Activate(Scene? oldScene) { }
        public virtual void Deactivate(Scene? newScene) { if (newScene != null) GAMELOOP.SwitchScene(this, newScene); }

        public virtual void Start() { }
        public virtual void Close() { }
        

        public virtual void HandleInput() { }
        public virtual void Update(float dt) { }
        public virtual void Draw() { }
        /// <summary>
        /// Used for drawing UI elements like buttons, progress bars and text.
        /// </summary>
        /// <param name="uiSize"> The actual size of the UI texture. Can be used to make position and size of ui elements relative.</param>
        /// <param name="stretchFactor">Should be used for ui elements like buttons where the position and size is set a start but 
        /// the button should still be relative to the current screen size. Just multiply your raw position and size with the stretch factor.
        /// ONLY needed if texture mode is not fixed! </param>
        public virtual void DrawUI(Vector2 uiSize) { }
        //public virtual void HandleInput(float dt) { }




        public bool IsInputDisabled() { return inputDisabled; }
        public bool IsHidden() { return hidden; }
        public bool IsPaused() { return paused; }


        public void DisableInput()
        {
            if(inputDisabled) return;
            inputDisabled = true;
        }
        public void EnableInput() 
        { 
            if (!inputDisabled) return;
            inputDisabled = false; 
        }
        public bool ToggleInputDisabled()
        {
            inputDisabled= !inputDisabled;
            return inputDisabled;
        }

        public void Hide()
        {
            if(hidden) return;
            hidden = true;
        }
        public void UnHide()
        {
            if(!hidden) return;
            hidden = false;
        }
        public bool ToggleHide()
        {
            hidden = !hidden;
            return hidden;
        }


        public void Pause()
        {
            if (paused) return;
            paused = true;
        }
        public void UnPause()
        {
            if (!paused) return;
            paused = false;
        }
        public bool TogglePause() 
        { 
            paused = !paused; 
            return paused; 
        }
        
    }
    */
}
