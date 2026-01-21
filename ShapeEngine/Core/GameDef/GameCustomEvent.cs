using System.Numerics;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Core.GameDef;

public partial class Game
{
    /// <summary>
    /// Represents an event that triggers at a specified interval, allowing actions to be executed
    /// at regular time steps or every frame.
    /// </summary>
    /// <remarks>
    /// ScheduledGameEvents are sorted first by their <c>Order</c> property, then by a unique internal id. 
    /// If two ScheduledGameEvents have the same order, the one created earlier (with a lower id) is processed first.
    /// </remarks>
    public abstract class CustomEvent : IComparable<CustomEvent>
    {
        // Ensures ScheduledGameEvents with the same Order are uniquely sorted and not treated as duplicates in the SortedSet.
        private static uint idCounter;
        private readonly uint id = idCounter++;
        
        /// <summary>
        /// Determines the execution order of the CustomEvent.
        /// Lower values are executed before higher values.
        /// </summary>
        public readonly int Order;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomEvent"/> class with the default order (0).
        /// </summary>
        protected CustomEvent()
        {
            Order = 0;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomEvent"/> class with a specified order.
        /// </summary>
        /// <param name="order">The execution order for this event. Lower values are executed first.</param>
        protected CustomEvent(int order)
        {
            Order = order;
        }

        /// <summary>
        /// Called before input handling for the current frame or tick.
        /// Override to perform input-related logic that should run prior to the game's <c>HandleInput</c> method.
        /// </summary>
        /// <param name="time">The current game time.</param>
        /// <param name="mousePosGame">Mouse position in game world coordinates.</param>
        /// <param name="mousePosGameUi">Mouse position transformed to the in-game UI coordinate space.</param>
        /// <param name="mousePosUi">Mouse position in general UI/screen coordinates.</param>
        /// <remarks>Default implementation performs no action.</remarks>
        protected virtual void PreHandleInput(GameTime time, Vector2 mousePosGame, Vector2 mousePosGameUi, Vector2 mousePosUi) { }

        /// <summary>
        /// Called after input handling for the current frame or tick.
        /// Override to perform input-related logic that should run after the game's <c>HandleInput</c> method.
        /// </summary>
        /// <param name="time">The current game time.</param>
        /// <param name="mousePosGame">Mouse position in game world coordinates.</param>
        /// <param name="mousePosGameUi">Mouse position transformed to the in-game UI coordinate space.</param>
        /// <param name="mousePosUi">Mouse position in general UI/screen coordinates.</param>
        /// <remarks>Default implementation performs no action.</remarks>
        protected virtual void PostHandleInput(GameTime time, Vector2 mousePosGame, Vector2 mousePosGameUi, Vector2 mousePosUi) { }

        /// <summary>
        /// Called before the update step. Override to execute logic before updates.
        /// </summary>
        /// <param name="time">The current game time.</param>
        /// <param name="gameScreenInfo">Screen info for the game view.</param>
        /// <param name="gameUiScreenInfo">Screen info for the game UI.</param>
        /// <param name="uiScreenInfo">Screen info for the general UI.</param>
        protected virtual void PreUpdate(GameTime time, ScreenInfo gameScreenInfo, ScreenInfo gameUiScreenInfo, ScreenInfo uiScreenInfo) { }
        
        /// <summary>
        /// Called after the update step. Override to execute logic after updates.
        /// </summary>
        /// <param name="time">The current game time.</param>
        /// <param name="gameScreenInfo">Screen info for the game view.</param>
        /// <param name="gameUiScreenInfo">Screen info for the game UI.</param>
        /// <param name="uiScreenInfo">Screen info for the general UI.</param>
        protected virtual void PostUpdate(GameTime time, ScreenInfo gameScreenInfo, ScreenInfo gameUiScreenInfo, ScreenInfo uiScreenInfo) { }
        
        /// <summary>
        /// Called before drawing the game. Override to execute logic before the game is drawn.
        /// </summary>
        /// <param name="info">Screen info for the game view.</param>
        protected virtual void PreDrawGame(ScreenInfo info) { }
        
        /// <summary>
        /// Called after drawing the game. Override to execute logic after the game is drawn.
        /// </summary>
        /// <param name="info">Screen info for the game view.</param>
        protected virtual void PostDrawGame(ScreenInfo info) { }
        
        /// <summary>
        /// Called before drawing the game UI. Override to execute logic before the game UI is drawn.
        /// </summary>
        /// <param name="info">Screen info for the game UI.</param>
        protected virtual void PreDrawGameUi(ScreenInfo info) { }
        
        /// <summary>
        /// Called after drawing the game UI. Override to execute logic after the game UI is drawn.
        /// </summary>
        /// <param name="info">Screen info for the game UI.</param>
        protected virtual void PostDrawGameUi(ScreenInfo info) { }
        
        /// <summary>
        /// Called before drawing the general UI. Override to execute logic before the UI is drawn.
        /// </summary>
        /// <param name="info">Screen info for the general UI.</param>
        protected virtual void PreDrawUi(ScreenInfo info) { }
        
        /// <summary>
        /// Called after drawing the general UI. Override to execute logic after the UI is drawn.
        /// </summary>
        /// <param name="info">Screen info for the general UI.</param>
        protected virtual void PostDrawUi(ScreenInfo info) { }
        
        

        internal void TriggerOnHandleInput(bool pre, GameTime time, Vector2 mousePosGame, Vector2 mousePosGameUi, Vector2 mousePosUi)
        {
            if(pre) PreHandleInput(time, mousePosGame, mousePosGameUi, mousePosUi);
            else PostHandleInput(time, mousePosGame, mousePosGameUi, mousePosUi);
        }
        internal void TriggerOnUpdate(bool pre, GameTime time, ScreenInfo gameScreenInfo, ScreenInfo gameUiScreenInfo, ScreenInfo uiScreenInfo)
        {
            if(pre) PreUpdate(time, gameScreenInfo, gameUiScreenInfo, uiScreenInfo);
            else PostUpdate(time, gameScreenInfo, gameUiScreenInfo, uiScreenInfo);
        }
        internal void TriggerOnDrawGame(bool pre, ScreenInfo info)
        {
            if(pre) PreDrawGame(info);
            else PostDrawGame(info);
        }
        internal void TriggerOnDrawGameUi(bool pre, ScreenInfo info)
        {
            if(pre) PreDrawGameUi(info);
            else PostDrawGameUi(info);
        }
        internal void TriggerOnDrawUi(bool pre, ScreenInfo info)
        {
            if(pre) PreDrawUi(info);
            else PostDrawUi(info);
        }

        /// <summary>
        /// Compares this <see cref="CustomEvent"/> to another for sorting in a <see cref="SortedSet{T}"/>.
        /// Events are first compared by <c>Order</c>, then by unique <c>id</c> to ensure consistent ordering.
        /// </summary>
        /// <param name="other">The other <see cref="CustomEvent"/> to compare with.</param>
        /// <returns>
        /// Less than zero if this instance precedes <paramref name="other"/> in the sort order,
        /// zero if they are equal, or greater than zero if it follows <paramref name="other"/>.
        /// </returns>
        /// <remarks>
        /// Each CustomEvent instance receives a unique id, ensuring that no two different ScheduledGameEvents are ever considered equal.
        /// As a result, this method only returns 0 when comparing an instance to itself.
        /// </remarks>
        public int CompareTo(CustomEvent? other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (other is null) return 1;
            int orderComparison = Order.CompareTo(other.Order);
            if (orderComparison != 0) return orderComparison;
            return id.CompareTo(other.id);
        }
    }
    
    /// <summary>
    /// Stores all active <see cref="CustomEvent"/> instances managed by the game.
    /// </summary>
    /// <remarks>
    /// ScheduledGameEvents are sorted by order first and by unique id second.
    /// </remarks>
    private readonly SortedSet<CustomEvent> customEvents = [];

    /// <summary>
    /// Adds a new <see cref="CustomEvent"/> to the managed set.
    /// </summary>
    /// <param name="customEvent">
    /// The <see cref="CustomEvent"/> instance to add.
    /// </param>
    /// <returns>True if the event was added; otherwise, false.</returns>
    /// <remarks>Duplicates are not allowed!</remarks>
    public bool AddCustomEvent(CustomEvent customEvent) => customEvents.Add(customEvent);

    /// <summary>
    /// Removes the specified <see cref="CustomEvent"/> from the list.
    /// </summary>
    /// <param name="customEvent">The CustomEvent to remove.</param>
    /// <returns>True if the event was removed; otherwise, false.</returns>
    public bool RemoveCustomEvent(CustomEvent customEvent) => customEvents.Remove(customEvent);

    /// <summary>
    /// Checks if the specified <see cref="CustomEvent"/> is present in the list.
    /// </summary>
    /// <param name="customEvent">The CustomEvent to check for.</param>
    /// <returns>True if the event exists; otherwise, false.</returns>
    public bool HasCustomEvent(CustomEvent customEvent) => customEvents.Contains(customEvent);

    /// <summary>
    /// Removes all <see cref="CustomEvent"/> instances from the list.
    /// </summary>
    public void ClearCustomEvents() => customEvents.Clear();
    
    private void TriggerCustomEventsOnHandleInput(bool pre, GameTime time, Vector2 mousePosGame, Vector2 mousePosGameUi, Vector2 mousePosUi)
    {
        foreach (var intervalEvent in customEvents)
        {
            intervalEvent.TriggerOnHandleInput(pre, time, mousePosGame, mousePosGameUi, mousePosUi);
        }
    }
    private void TriggerCustomEventsOnUpdate(bool pre, GameTime time, ScreenInfo gameScreenInfo, ScreenInfo gameUiScreenInfo, ScreenInfo uiScreenInfo)
    {
        foreach (var intervalEvent in customEvents)
        {
            intervalEvent.TriggerOnUpdate(pre, time, gameScreenInfo, gameUiScreenInfo, uiScreenInfo);
        }
    }
    private void TriggerCustomEventsOnDrawGame(bool pre, ScreenInfo info)
    {
        foreach (var intervalEvent in customEvents)
        {
            intervalEvent.TriggerOnDrawGame(pre, info);
        }
    }
    private void TriggerCustomEventsOnDrawGameUi(bool pre, ScreenInfo info)
    {
        foreach (var intervalEvent in customEvents)
        {
            intervalEvent.TriggerOnDrawGameUi(pre, info);
        }
    }
    private void TriggerCustomEventsOnDrawUi(bool pre, ScreenInfo info)
    {
        foreach (var intervalEvent in customEvents)
        {
            intervalEvent.TriggerOnDrawUi(pre, info);
        }
    }

}