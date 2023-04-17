using Raylib_CsLo;
using ShapePersistent;


namespace ShapeShaders
{
    public class ScreenShader
    {
        private Shader shader;
        private bool enabled = true;
        private uint id;
        private int order = 0;
        public ScreenShader(string fileName, uint id, bool enabled = true, int order = 0)
        {
            this.id = id;
            shader = Raylib.LoadShader(null, fileName); // ResourceManager.LoadFragmentShader(fileName); // LoadShader("330", fileName);
            this.enabled = enabled;
            this.order = order;
        }
        public ScreenShader(Shader shader, uint id, bool enabled = true, int order = 0)
        {
            this.id = id;
            this.shader = shader;// = ResourceManager.LoadFragmentShader(fileName); // LoadShader("330", fileName);
            this.enabled = enabled;
            this.order = order;
        }
        public int GetOrder() { return order; }
        public void SetOrder(int newOrder) { order = newOrder; }
        public Shader GetShader() { return shader; }
        public bool IsEnabled() { return enabled; }
        public void Enable() { enabled = true; }
        public void Disable() { enabled = false; }
        public uint GetID() { return id; }
        public void Unload() { UnloadShader(shader); }


    }

}
