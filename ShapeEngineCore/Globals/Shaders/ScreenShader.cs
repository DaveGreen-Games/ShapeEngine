using Raylib_CsLo;

namespace ShapeEngineCore.Globals.Shaders
{
    public class ScreenShader
    {
        private Shader shader;
        private bool enabled = true;
        private string name = "";
        private int order = 0;
        public ScreenShader(string path, string name, bool enabled = true, int order = 0)
        {
            this.name = name;
            shader = LoadShader("330", path);
            this.enabled = enabled;
            this.order = order;
        }

        public int GetOrder() { return order; }
        public void SetOrder(int newOrder) { order = newOrder; }
        public Shader GetShader() { return shader; }
        public bool IsEnabled() { return enabled; }
        public void Enable() { enabled = true; }
        public void Disable() { enabled = false; }
        public string GetName() { return name; }
        public void Unload() { UnloadShader(shader); }


    }

}
