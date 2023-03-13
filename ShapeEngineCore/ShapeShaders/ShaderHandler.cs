using Raylib_CsLo;
using ShapeScreen;
using System.Numerics;

namespace ShapeShaders
{

    public class ShaderHandler
    {
        private Dictionary<int, ScreenShader> screenShaders = new();
        private Dictionary<int, Shader> shaders = new();
        private bool enabled = true;
        
        public void Close()
        {
            //foreach (ScreenBuffer screenBuffer in screenBuffers)
            //{
            //    screenBuffer.Unload();
            //}
            foreach (Shader shader in shaders.Values)
            {
                UnloadShader(shader);
            }
            foreach (ScreenShader screenShader in screenShaders.Values)
            {
                screenShader.Unload();
            }
            screenShaders.Clear();
            shaders.Clear();
            //screenBuffers = new ScreenBuffer[0];
        }

        public List<ScreenShader> GetCurActiveShaders()
        {
            if (!enabled) return new() { };

            List<ScreenShader> shadersToApply = screenShaders.Values.ToList().FindAll(s => s.IsEnabled());
            shadersToApply.Sort(delegate (ScreenShader a, ScreenShader b)
            {
                if (a.GetOrder() < b.GetOrder()) return -1;
                else if (a.GetOrder() > b.GetOrder()) return 1;
                else return 0;
            });
            return shadersToApply;
        }

        public bool HasScreenShader(int id) { return screenShaders.ContainsKey(id); }
        public void AddScreenShader(int id, string fileName, bool enabled = true, int order = 0)
        {
            if (fileName == "") return;
            if (screenShaders.ContainsKey(id)) return;
            screenShaders[id] = new ScreenShader(fileName, id, enabled);
        }
        public void AddScreenShader(int name, Shader shader, bool enabled = true, int order = 0)
        {
            if (screenShaders.ContainsKey(name)) return;
            screenShaders[name] = new ScreenShader(shader, name, enabled);
        }
        public void RemoveScreenShader(int id)
        {
            //if (!screenShaders.ContainsKey(name)) return;
            screenShaders.Remove(id);
        }
        public ScreenShader? GetScreenShader(int id)
        {
            if (!screenShaders.ContainsKey(id)) return null;
            return screenShaders[id];
        }

        public void AddShader(int id, string path)
        {
            if (path == "") return;
            if (shaders.ContainsKey(id)) return;
            shaders[id] = LoadShader("330", path);
        }
        public void RemoveShader(int id)
        {
            shaders.Remove(id);
        }
        public Shader? GetShader(int id)
        {
            if (!shaders.ContainsKey(id)) return null;
            return shaders[id];
        }

        public bool IsEnabled() { return enabled; }
        public bool Enable()
        {
            enabled = true;
            return enabled;
        }
        public bool Disable()
        {
            enabled = false;
            return enabled;
        }
        public void SetEnabled(bool value) { enabled = value; }
        public bool ToggleEnabled()
        {
            if (enabled) Disable();
            else Enable();
            return enabled;
        }
        public bool IsScreenShaderEnabled(int id)
        {
            if (!screenShaders.ContainsKey(id)) return false;
            return screenShaders[id].IsEnabled();
        }
        public void EnableScreenShader(int id)
        {
            if (!screenShaders.ContainsKey(id)) return;
            screenShaders[id].Enable();
        }
        public void DisableScreenShader(int id)
        {
            if (!screenShaders.ContainsKey(id)) return;
            screenShaders[id].Disable();
        }
        public void SetScreenShaderValueFloat(int id, string propertyName, float value)
        {
            if (!screenShaders.ContainsKey(id)) return;
            Shader shader = screenShaders[id].GetShader();
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, value, ShaderUniformDataType.SHADER_UNIFORM_FLOAT);
        }
        public void SetScreenShaderValueVec(int id, string propertyName, float[] values, ShaderUniformDataType dataType)
        {
            if (!screenShaders.ContainsKey(id)) return;
            Shader shader = screenShaders[id].GetShader();
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, values, dataType);
        }
        public void SetScreenShaderValueVec(int id, string propertyName, float v1, float v2)
        {
            if (!screenShaders.ContainsKey(id)) return;
            Shader shader = screenShaders[id].GetShader();
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, new float[] { v1, v2 }, ShaderUniformDataType.SHADER_UNIFORM_VEC2);
        }
        public void SetScreenShaderValueVec(int id, string propertyName, float v1, float v2, float v3)
        {
            if (!screenShaders.ContainsKey(id)) return;
            Shader shader = screenShaders[id].GetShader();
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, new float[] { v1, v2, v3 }, ShaderUniformDataType.SHADER_UNIFORM_VEC3);
        }
        public void SetScreenShaderValueVec(int id, string propertyName, float v1, float v2, float v3, float v4)
        {
            if (!screenShaders.ContainsKey(id)) return;
            Shader shader = screenShaders[id].GetShader();
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, new float[] { v1, v2, v3, v4 }, ShaderUniformDataType.SHADER_UNIFORM_VEC4);
        }
        public void SetScreenShaderValueVec(int id, string propertyName, Vector2 vec)
        {
            if (!screenShaders.ContainsKey(id)) return;
            Shader shader = screenShaders[id].GetShader();
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, new float[] { vec.X, vec.Y}, ShaderUniformDataType.SHADER_UNIFORM_VEC2);
        }
        public void SetScreenShaderValueVec(int id, string propertyName, Color color)
        {
            if (!screenShaders.ContainsKey(id)) return;
            Shader shader = screenShaders[id].GetShader();
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, new float[] {color.r / 255f, color.g / 255f, color.b / 255f, color.a / 255f}, ShaderUniformDataType.SHADER_UNIFORM_VEC4);
        }
        public void SetShaderValueFloat(int id, string propertyName, float value)
        {
            if (!shaders.ContainsKey(id)) return;
            Shader shader = shaders[id];
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, value, ShaderUniformDataType.SHADER_UNIFORM_FLOAT);
        }
        public void SetShaderValueVec(int id, string propertyName, float[] values, ShaderUniformDataType dataType)
        {
            if (!shaders.ContainsKey(id)) return;
            Shader shader = shaders[id];
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, values, dataType);
        }

        public void SetShaderValueVec(int id, string propertyName, float v1, float v2)
        {
            if (!shaders.ContainsKey(id)) return;
            Shader shader = shaders[id];
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, new float[] { v1, v2 }, ShaderUniformDataType.SHADER_UNIFORM_VEC2);
        }
        public void SetShaderValueVec(int id, string propertyName, float v1, float v2, float v3)
        {
            if (!shaders.ContainsKey(id)) return;
            Shader shader = shaders[id];
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, new float[] { v1, v2, v3 }, ShaderUniformDataType.SHADER_UNIFORM_VEC3);
        }
        public void SetShaderValueVec(int id, string propertyName, float v1, float v2, float v3, float v4)
        {
            if (!shaders.ContainsKey(id)) return;
            Shader shader = shaders[id];
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, new float[] { v1, v2, v3, v4 }, ShaderUniformDataType.SHADER_UNIFORM_VEC4);
        }
        public void SetShaderValueVec(int id, string propertyName, Vector2 vec)
        {
            if (!shaders.ContainsKey(id)) return;
            Shader shader = shaders[id];
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, new float[] { vec.X, vec.Y }, ShaderUniformDataType.SHADER_UNIFORM_VEC2);
        }
        public void SetShaderValueVec(int id, string propertyName, Color color)
        {
            if (!shaders.ContainsKey(id)) return;
            Shader shader = shaders[id];
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, new float[] { color.r / 255f, color.g / 255f, color.b / 255f, color.a / 255f }, ShaderUniformDataType.SHADER_UNIFORM_VEC4);
        }
    }
}
