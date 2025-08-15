using ShapeEngine.StaticLib;

namespace ShapeEngine.Input;

/// <summary>
/// Represents a gesture logic for an <see cref="InputAction"/>.
/// Handles tap, multi-tap, long press, and long release gestures,
/// tracking progress, state, and completion.
/// </summary>
public sealed class InputGesture
{
    /// <summary>
    /// Represents the possible states of an <see cref="InputGesture"/>.
    /// </summary>
    public enum State
    {
        /// <summary>
        /// The gesture is waiting to start.
        /// </summary>
        Waiting = 1,
        /// <summary>
        /// The gesture is currently in progress.
        /// </summary>
        InProgress = 2,
        /// <summary>
        /// The gesture has completed successfully.
        /// </summary>
        Completed = 4,
        /// <summary>
        /// The gesture has failed.
        /// </summary>
        Failed = 8
    }
    
    /// <summary>
    /// Represents the types of gesture for an <see cref="InputGesture"/>.
    /// </summary>
    public enum Type
    { 
        /// <summary>
        /// No gesture type.
        /// </summary>
        None = 1,
        /// <summary>
        /// Single tap gesture.
        /// </summary>
        Tap = 2,
        /// <summary>
        /// Multi-tap gesture.
        /// </summary>
        MultiTap = 4,
        /// <summary>
        /// Long press gesture.
        /// </summary>
        LongPress = 8,
        /// <summary>
        /// Long release gesture.
        /// </summary>
        LongRelease = 16
    }
    
    /// <summary>
    /// Represents the result of an <see cref="InputGesture"/> update.
    /// Contains the gesture type, current state, and normalized progress.
    /// </summary>
    public readonly struct Result
    {
        /// <summary>
        /// The type of gesture (e.g., Tap, MultiTap, LongPress, etc.).
        /// </summary>
        public readonly Type ActivationType;
    
        /// <summary>
        /// The current state of the gesture (Waiting, InProgress, Completed, Failed).
        /// </summary>
        public readonly State CurState;
    
        /// <summary>
        /// The normalized progress of the gesture, clamped between 0 and 1.
        /// </summary>
        public readonly float NormalizedProgress;
    
        /// <summary>
        /// Initializes a new instance of <see cref="Result"/> with default values.
        /// </summary>
        public Result()
        {
            ActivationType = Type.None;
            CurState = State.Waiting;
            NormalizedProgress = 0f;
        }
    
        /// <summary>
        /// Initializes a new instance of <see cref="Result"/> with specified values.
        /// </summary>
        /// <param name="activationType">The type of gesture.</param>
        /// <param name="curState">The current state of the gesture.</param>
        /// <param name="normalizedProgress">The normalized progress value.</param>
        internal Result(Type activationType, State curState, float normalizedProgress)
        {
            ActivationType = activationType;
            CurState = curState;
            NormalizedProgress = ShapeMath.Clamp(normalizedProgress, 0f, 1f);
        }

        /// <summary>
        /// Picks the most relevant <see cref="Result"/> between this instance and <paramref name="otherResult"/>.
        /// Prioritizes completed, in-progress, and waiting states in that order.
        /// Returns the result with the highest priority based on gesture type and state.
        /// </summary>
        /// <param name="otherResult">The other <see cref="Result"/> to compare.</param>
        /// <returns>The most relevant <see cref="Result"/>.</returns>
        public Result Pick(Result otherResult)
        {
            if (otherResult.ActivationType == Type.None) return this;
            if(ActivationType == Type.None) return otherResult;
            
            if (CurState == State.Completed) return this;
            if(otherResult.CurState == State.Completed) return otherResult;
            
            if (CurState == State.InProgress) return this;
            if(otherResult.CurState == State.InProgress) return otherResult;
            
            if(CurState == State.Waiting) return this;
            return otherResult.CurState == State.Waiting ? otherResult : this;
        }
    }
    
    
    /// <summary>
    /// The type of gesture (e.g., Tap, MultiTap, LongPress, etc.)
    /// </summary>
    public readonly Type ActivationType;

    /// <summary>
    /// The duration required for the gesture, in seconds.
    /// </summary>
    public readonly float Duration;

    /// <summary>
    /// The target count for multi-tap activations.
    /// </summary>
    public readonly int TargetCount;

    /// <summary>
    /// The current state of the gesture (Waiting, InProgress, Completed, Failed).
    /// </summary>
    public State CurState { get; private set; }

    /// <summary>
    /// The normalized progress of the gesture, clamped between 0 and 1.
    /// </summary>
    public float NormalizedProgress { get; private set; }

    /// <summary>
    /// Indicates whether the gesture should automatically reset after completion or failure.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>
    /// If true,
    /// the gesture will automatically reset to its initial state in the next Update call after being completed or failed.
    /// </item>
    /// <item>
    /// If false, <see cref="Reset"/> has to be called manually to reset the gesture state.
    /// Until the gesture is reset,
    /// it will continue to report the last state and progress values after completion or failure.
    /// </item>
    /// </list>
    /// </remarks>
    public readonly bool AutomaticReset;
    
    private int count;
    private float timer;
    
    /// <summary>
    /// Initializes a new instance of <see cref="InputGesture"/> with default values.
    /// </summary>
    /// <remarks>
    /// Private to restrict instantiation; use static factory methods instead.
    /// </remarks>
    private InputGesture()
    {
        ActivationType = Type.None;
        CurState = State.Waiting;
        Duration = 0;
        TargetCount = 0;
        NormalizedProgress = 0f;
        timer = 0f;
        count = 0;
        AutomaticReset = false;
    }
    
    /// <summary>
    /// Initializes a new instance of <see cref="InputGesture"/> with the specified gesture type, duration, and target count.
    /// </summary>
    /// <param name="type">The type of gesture (e.g., Tap, MultiTap, LongPress, etc.).</param>
    /// <param name="duration">The required duration for the gesture in seconds.</param>
    /// <param name="targetCount">The number of activations required for multi-tap gestures.</param>
    /// <param name="automaticReset">Indicates whether the gesture should automatically reset after completion or failure. See <see cref="AutomaticReset"/> for more information. </param>
    /// <remarks>
    /// Private to restrict instantiation; use static factory methods instead.
    /// </remarks>
    private InputGesture(Type type, float duration, int targetCount, bool automaticReset)
    {
        ActivationType = type;
        
        Duration = duration;
        
        TargetCount = targetCount;
        
        count = 0;
        timer = 0f;

        NormalizedProgress = 0f;
        
        CurState = State.Waiting;
        
        AutomaticReset = automaticReset;
    }

    internal Result Update(float dt, InputState cur)
    {
        if (ActivationType == Type.None) return new();

        if (CurState is State.Failed or State.Completed)
        {
            if(AutomaticReset) Reset();
            else return new(ActivationType, CurState, NormalizedProgress);
        }
        
        if (CurState is State.Waiting) //nothing active yet
        {
            if (cur.Pressed)
            {
                switch (ActivationType)
                {
                    case Type.Tap:
                        if (Duration > 0f)
                        {
                            CurState = State.InProgress;
                            timer = Duration;
                        }
                        else
                        {
                            CurState = State.Failed;
                        }

                        return new(ActivationType, CurState, 0f);
                    case Type.MultiTap:
                        if (Duration > 0f && TargetCount > 1)
                        {
                            CurState = State.InProgress;
                            timer = Duration;
                            count = 1; //start with the first tap
                            NormalizedProgress = count / (float)TargetCount;
                        }
                        else
                        {
                            CurState = State.Failed;
                        }
                        return new(ActivationType, CurState, 0f);
                    case Type.LongPress:
                    case Type.LongRelease:
                        if (Duration > 0f)
                        {
                            CurState = State.InProgress;
                            timer = Duration;
                        }
                        else
                        {
                            CurState = State.Failed;
                        }
                        return new(ActivationType, CurState, 0f);
                }
            }
        }
        else //in progress
        {
            if (timer > 0f)
            {
                timer -= dt;
                if (timer <= 0f)
                {
                    timer = 0f;
                    count = 0;

                    if (ActivationType == Type.LongRelease)
                    {
                        return new(ActivationType, State.InProgress, 1f);
                    }
                    
                    if (ActivationType == Type.LongPress)
                    {
                        CurState = State.Completed;
                        NormalizedProgress = 1f;
                        return new(ActivationType, CurState, NormalizedProgress);
                    }
                    
                    NormalizedProgress = 0f;
                    CurState = State.Failed;
                    return new(ActivationType, CurState, 0f);
                }
                
                if(ActivationType != Type.MultiTap) NormalizedProgress = 1f - (timer / Duration);
            }
            
            
            if (cur.Pressed)
            {
                if (ActivationType == Type.MultiTap)
                {
                    count++;
                    if (count >= TargetCount)
                    {
                        CurState = State.Completed;
                        NormalizedProgress = 1f;
                    }
                    else
                    {
                        NormalizedProgress = count / (float)TargetCount;
                    }

                    return new(ActivationType, CurState, NormalizedProgress);
                }
            }
            else if (cur.Released)
            {
                if (ActivationType == Type.Tap)
                {
                    CurState = State.Completed;
                    NormalizedProgress = 1f;
                    return new(ActivationType, CurState, NormalizedProgress);
                }
                
                if(ActivationType == Type.LongPress)
                {
                    CurState = State.Failed;
                    NormalizedProgress = 0f;
                    return new(ActivationType, CurState, NormalizedProgress);
                }
                
                if(ActivationType == Type.LongRelease)
                {
                    if (timer > 0f)
                    {
                        CurState = State.Failed;
                        NormalizedProgress = 0f;
                        return new(ActivationType, CurState, NormalizedProgress);
                    }
                    CurState = State.Completed;
                    NormalizedProgress = 1f;
                    return new(ActivationType, CurState, NormalizedProgress);
                }
            }
        }
        
        return new(ActivationType, CurState, NormalizedProgress);
    }

    /// <summary>
    /// Resets the gesture  state to its initial values.
    /// Sets <see cref="CurState"/> to Waiting, clears the timer and count, and resets <see cref="NormalizedProgress"/>.
    /// </summary>
    public void Reset()
    {
        CurState = State.Waiting;
        timer = 0f;
        count = 0;
        NormalizedProgress = 0f;
    }
    
    /// <summary>
    /// Returns an input gesture representing no special gesture.
    /// Used for actions that do not require any gesture logic.
    /// </summary>
    public static InputGesture None() => new();

    /// <summary>
    /// Returns a gesture for a long press.
    /// Triggered when the input is held down for longer than the specified duration.
    /// </summary>
    /// <param name="duration">The required hold duration in seconds.</param>
    /// <param name="automaticReset">Indicates whether the gesture should automatically reset after completion or failure. See <see cref="AutomaticReset"/> for more information. </param>
    public static InputGesture LongPress(float duration, bool automaticReset = true) => new(Type.LongPress, duration, 0, automaticReset);

    /// <summary>
    /// Returns a gesture for a long release.
    /// Triggered when the input is released after the specified duration.
    /// </summary>
    /// <param name="duration">The required release duration in seconds.</param>
    /// <param name="automaticReset">Indicates whether the gesture should automatically reset after completion or failure. See <see cref="AutomaticReset"/> for more information. </param>
    public static InputGesture LongRelease(float duration, bool automaticReset = true) => new(Type.LongRelease, duration, 0, automaticReset);

    /// <summary>
    /// Returns a gesture for a tap.
    /// Triggered when the input is pressed and released within the specified duration.
    /// </summary>
    /// <param name="duration">The maximum tap duration in seconds.</param>
    /// <param name="automaticReset">Indicates whether the gesture should automatically reset after completion or failure. See <see cref="AutomaticReset"/> for more information. </param>
    public static InputGesture Tap(float duration, bool automaticReset = true) => new(Type.Tap, duration, 0, automaticReset);

    /// <summary>
    /// Returns a gesture for a double tap.
    /// Triggered when the input is pressed and released twice within the specified duration.
    /// </summary>
    /// <param name="duration">The maximum double tap duration in seconds.</param>
    /// <param name="automaticReset">Indicates whether the gesture should automatically reset after completion or failure. See <see cref="AutomaticReset"/> for more information. </param>
    public static InputGesture DoubleTap(float duration, bool automaticReset = true) => new(Type.MultiTap, duration, 2, automaticReset);

    /// <summary>
    /// Returns a gesture for a multi tap.
    /// Triggered when the input is pressed and released the specified number of times within the duration.
    /// </summary>
    /// <param name="duration">The maximum multi tap duration in seconds.</param>
    /// <param name="targetCount">The number of taps required.</param>
    /// <param name="automaticReset">Indicates whether the gesture should automatically reset after completion or failure. See <see cref="AutomaticReset"/> for more information. </param>
    public static InputGesture MultiTap(float duration, int targetCount, bool automaticReset = true) => new(Type.MultiTap, duration, targetCount, automaticReset);
    
}