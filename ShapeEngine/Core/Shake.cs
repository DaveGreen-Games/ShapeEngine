using ShapeEngine.Lib;

namespace ShapeEngine.Core
{
    public class Shake
    {
        private float timer = 0.0f;
        private float duration = 0.0f;
        private float[] values;
        private float[] factors;

        public float Smoothness { get; set; } = 0.0f;
        public float F { get; private set; } = 0.0f;

        /// <summary>
        /// Create a new shake class with the specified amount of float values that are affected. 
        /// Each value has an additional factor for the intensity that can be set in the Start() function.
        /// </summary>
        /// <param name="valueCount">How many float values to generate.</param>
        public Shake(int valueCount) 
        { 
            values = new float[valueCount]; 
            factors = new float[valueCount]; 
            for (int i = 0; i < valueCount; i++) { values[i] = 0f; factors[i] = 1.0f; }
        }


        public float Get(int index)
        {
            if(index < 0 || index >= values.Length) return 0.0f;
            else return values[index] * factors[index];
        }
        public bool IsActive() { return timer > 0.0f; }

        public void Start(float duration, float smoothness, params float[] newFactors)
        {
            timer = duration;
            this.duration = duration;
            this.Smoothness = smoothness;

            int count = newFactors.Length;
            if(count > factors.Length)  count = factors.Length;
            for (int i = 0; i < count; i++)
            {
                factors[i] = newFactors[i];
            }
            F = 0.0f;
        }
        public void Stop()
        {
            timer = 0f;
            F = 0.0f;
            ResetValues();
        }
        public void Update(float dt)
        {
            if (timer > 0.0f)
            {
                timer -= dt;
                if (timer <= 0.0f)
                {
                    timer = 0.0f;
                    ResetValues();
                    F = 0.0f;
                    return;
                }
                F = timer / duration;
                for (int i = 0; i < values.Length; i++)
                {
                    UpdateValue(i);
                }
            }
        }
        
        private void UpdateValue(int index) { values[index] = Lerp(SRNG.randF(-1.0f, 1.0f), values[index], Smoothness) * F; }
        private void ResetValues() { for (int i = 0; i < values.Length; i++) { values[i] = 0.0f; } }
    }


}
