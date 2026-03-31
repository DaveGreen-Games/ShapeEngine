namespace ShapeEngine.ShapeClipper;

public class TriMeshPool
{
    private readonly Stack<TriMesh> stack;
    
    public TriMeshPool(int startCapacity)
    {
        if (startCapacity <= 0) startCapacity = 0;
        stack = new Stack<TriMesh>(startCapacity);
        for (int i = 0; i < startCapacity; i++)
        {
            stack.Push(new TriMesh());
        }
    }
    
    public TriMesh RentMesh()
    {
        if (stack.Count > 0)
        {
            var m = stack.Pop();
            m.Clear();
            return m;
        }
        return new TriMesh();
    }

    public void ReturnMesh(TriMesh mesh)
    {
        mesh.Clear();
        stack.Push(mesh);
    }
    
}