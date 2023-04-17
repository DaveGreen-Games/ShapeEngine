using Raylib_CsLo;
using ShapeScreen;
using System.Numerics;

namespace ShapeShaders
{

    public class ShaderHandler
    {
        private Dictionary<uint, ScreenShader> screenShaders = new();
        private Dictionary<uint, Shader> shaders = new();
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

        public bool HasScreenShader(uint id) { return screenShaders.ContainsKey(id); }
        public void AddScreenShader(uint id, string fileName, bool enabled = true, int order = 0)
        {
            if (fileName == "") return;
            if (screenShaders.ContainsKey(id)) return;
            screenShaders[id] = new ScreenShader(fileName, id, enabled);
        }
        public void AddScreenShader(uint id, Shader shader, bool enabled = true, int order = 0)
        {
            if (screenShaders.ContainsKey(id)) return;
            screenShaders[id] = new ScreenShader(shader, id, enabled);
        }
        public void RemoveScreenShader(uint id)
        {
            //if (!screenShaders.ContainsKey(name)) return;
            screenShaders.Remove(id);
        }
        public ScreenShader? GetScreenShader(uint id)
        {
            if (!screenShaders.ContainsKey(id)) return null;
            return screenShaders[id];
        }

        public void AddShader(uint id, string path)
        {
            if (path == "") return;
            if (shaders.ContainsKey(id)) return;
            shaders[id] = LoadShader("330", path);
        }
        public void RemoveShader(uint id)
        {
            shaders.Remove(id);
        }
        public Shader? GetShader(uint id)
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
        public bool IsScreenShaderEnabled(uint id)
        {
            if (!screenShaders.ContainsKey(id)) return false;
            return screenShaders[id].IsEnabled();
        }
        public void EnableScreenShader(uint id)
        {
            if (!screenShaders.ContainsKey(id)) return;
            screenShaders[id].Enable();
        }
        public void DisableScreenShader(uint id)
        {
            if (!screenShaders.ContainsKey(id)) return;
            screenShaders[id].Disable();
        }
        public void SetScreenShaderValueFloat(uint id, string propertyName, float value)
        {
            if (!screenShaders.ContainsKey(id)) return;
            Shader shader = screenShaders[id].GetShader();
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, value, ShaderUniformDataType.SHADER_UNIFORM_FLOAT);
        }
        public void SetScreenShaderValueVec(uint id, string propertyName, float[] values, ShaderUniformDataType dataType)
        {
            if (!screenShaders.ContainsKey(id)) return;
            Shader shader = screenShaders[id].GetShader();
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, values, dataType);
        }
        public void SetScreenShaderValueVec(uint id, string propertyName, float v1, float v2)
        {
            if (!screenShaders.ContainsKey(id)) return;
            Shader shader = screenShaders[id].GetShader();
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, new float[] { v1, v2 }, ShaderUniformDataType.SHADER_UNIFORM_VEC2);
        }
        public void SetScreenShaderValueVec(uint id, string propertyName, float v1, float v2, float v3)
        {
            if (!screenShaders.ContainsKey(id)) return;
            Shader shader = screenShaders[id].GetShader();
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, new float[] { v1, v2, v3 }, ShaderUniformDataType.SHADER_UNIFORM_VEC3);
        }
        public void SetScreenShaderValueVec(uint id, string propertyName, float v1, float v2, float v3, float v4)
        {
            if (!screenShaders.ContainsKey(id)) return;
            Shader shader = screenShaders[id].GetShader();
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, new float[] { v1, v2, v3, v4 }, ShaderUniformDataType.SHADER_UNIFORM_VEC4);
        }
        public void SetScreenShaderValueVec(uint id, string propertyName, Vector2 vec)
        {
            if (!screenShaders.ContainsKey(id)) return;
            Shader shader = screenShaders[id].GetShader();
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, new float[] { vec.X, vec.Y}, ShaderUniformDataType.SHADER_UNIFORM_VEC2);
        }
        public void SetScreenShaderValueVec(uint id, string propertyName, Color color)
        {
            if (!screenShaders.ContainsKey(id)) return;
            Shader shader = screenShaders[id].GetShader();
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, new float[] {color.r / 255f, color.g / 255f, color.b / 255f, color.a / 255f}, ShaderUniformDataType.SHADER_UNIFORM_VEC4);
        }
        public void SetShaderValueFloat(uint id, string propertyName, float value)
        {
            if (!shaders.ContainsKey(id)) return;
            Shader shader = shaders[id];
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, value, ShaderUniformDataType.SHADER_UNIFORM_FLOAT);
        }
        public void SetShaderValueVec(uint id, string propertyName, float[] values, ShaderUniformDataType dataType)
        {
            if (!shaders.ContainsKey(id)) return;
            Shader shader = shaders[id];
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, values, dataType);
        }

        public void SetShaderValueVec(uint id, string propertyName, float v1, float v2)
        {
            if (!shaders.ContainsKey(id)) return;
            Shader shader = shaders[id];
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, new float[] { v1, v2 }, ShaderUniformDataType.SHADER_UNIFORM_VEC2);
        }
        public void SetShaderValueVec(uint id, string propertyName, float v1, float v2, float v3)
        {
            if (!shaders.ContainsKey(id)) return;
            Shader shader = shaders[id];
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, new float[] { v1, v2, v3 }, ShaderUniformDataType.SHADER_UNIFORM_VEC3);
        }
        public void SetShaderValueVec(uint id, string propertyName, float v1, float v2, float v3, float v4)
        {
            if (!shaders.ContainsKey(id)) return;
            Shader shader = shaders[id];
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, new float[] { v1, v2, v3, v4 }, ShaderUniformDataType.SHADER_UNIFORM_VEC4);
        }
        public void SetShaderValueVec(uint id, string propertyName, Vector2 vec)
        {
            if (!shaders.ContainsKey(id)) return;
            Shader shader = shaders[id];
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, new float[] { vec.X, vec.Y }, ShaderUniformDataType.SHADER_UNIFORM_VEC2);
        }
        public void SetShaderValueVec(uint id, string propertyName, Color color)
        {
            if (!shaders.ContainsKey(id)) return;
            Shader shader = shaders[id];
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, new float[] { color.r / 255f, color.g / 255f, color.b / 255f, color.a / 255f }, ShaderUniformDataType.SHADER_UNIFORM_VEC4);
        }
    }
}
