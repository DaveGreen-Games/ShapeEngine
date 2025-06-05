using ShapeEngine.StaticLib;
using ShapeEngine.Random;

namespace ShapeEngine.Core
{
    /// <summary>
    /// Represents a shake effect that generates and manages multiple float values,
    /// each modifiable by an intensity factor, for use in effects such as screen shake.
    /// </summary>
    public class Shake
    {
        /// <summary>
        /// Timer for the shake effect, counts down to zero.
        /// </summary>
        private float timer;

        /// <summary>
        /// Total duration of the shake effect.
        /// </summary>
        private float duration;
    
        /// <summary>
        /// Array of current shake values.
        /// </summary>
        private float[] values;
    
        /// <summary>
        /// Array of intensity factors for each shake value.
        /// </summary>
        private float[] factors;
    
        /// <summary>
        /// Controls how smoothly the shake values interpolate between random values.
        /// </summary>
        public float Smoothness { get; set; }
    
        /// <summary>
        /// Current normalized progress of the shake (0 = finished, 1 = just started).
        /// </summary>
        public float F { get; private set; }
    
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
    
        /// <summary>
        /// Gets the current shake value at the specified index, multiplied by its factor.
        /// </summary>
        /// <param name="index">Index of the value to retrieve.</param>
        /// <returns>The shake value at the given index, or 0.0f if out of range.</returns>
        public float Get(int index)
        {
            if(index < 0 || index >= values.Length) return 0.0f;
            else return values[index] * factors[index];
        }
    
        /// <summary>
        /// Checks if the shake effect is currently active.
        /// </summary>
        /// <returns>True if the shake is active, otherwise false.</returns>
        public bool IsActive() { return timer > 0.0f; }
    
        /// <summary>
        /// Starts the shake effect with the given duration, smoothness, and optional intensity factors.
        /// </summary>
        /// <param name="shakeDuration">Duration of the shake effect.</param>
        /// <param name="smoothness">Smoothness of the shake interpolation.</param>
        /// <param name="newFactors">Optional intensity factors for each value.</param>
        public void Start(float shakeDuration, float smoothness, params float[] newFactors)
        {
            timer = shakeDuration;
            this.duration = shakeDuration;
            this.Smoothness = smoothness;
    
            int count = newFactors.Length;
            if(count > factors.Length)  count = factors.Length;
            for (int i = 0; i < count; i++)
            {
                factors[i] = newFactors[i];
            }
            F = 0.0f;
        }
    
        /// <summary>
        /// Stops the shake effect and resets all values.
        /// </summary>
        public void Stop()
        {
            timer = 0f;
            F = 0.0f;
            ResetValues();
        }
    
        /// <summary>
        /// Updates the shake effect, progressing the timer and updating values.
        /// </summary>
        /// <param name="dt">Delta time since last update.</param>
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
        
        /// <summary>
        /// Updates the shake value at the specified index using randomization and smoothness.
        /// </summary>
        /// <param name="index">Index of the value to update.</param>
        private void UpdateValue(int index) { values[index] = ShapeMath.LerpFloat(Rng.Instance.RandF(-1.0f, 1.0f), values[index], Smoothness) * F; }
    
        /// <summary>
        /// Resets all shake values to zero.
        /// </summary>
        private void ResetValues() { for (int i = 0; i < values.Length; i++) { values[i] = 0.0f; } }
    }
    
}
