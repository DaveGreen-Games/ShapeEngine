namespace ShapeEngine.Geometry.CollisionSystem.CollisionHandlerDef;

public partial class CollisionHandler
{
    #region Support Classes

    private class CollisionObjectRegister : ObjectRegister<CollisionObject>
    {
        private readonly CollisionHandler handler;

        public CollisionObjectRegister(int capacity, CollisionHandler handler) : base(capacity)
        {
            this.handler = handler;
        }

        protected override void ObjectAdded(CollisionObject obj)
        {
            obj.OnCollisionSystemEntered(handler);
        }

        protected override void ObjectRemoved(CollisionObject obj)
        {
            obj.OnCollisionSystemLeft(handler);
        }
    }

    #endregion
}