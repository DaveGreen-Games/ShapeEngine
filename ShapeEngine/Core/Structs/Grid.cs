using System.Numerics;
using ShapeEngine.Core.Shapes;

namespace ShapeEngine.Core.Structs;

public readonly struct Grid : IEquatable<Grid>
{
    public readonly struct Coordinates : IEquatable<Coordinates>
    {
        public readonly int Row;
        public readonly int Col;
        public bool IsValid => Row >= 0 && Col >= 0;

    
        public Coordinates()
        {
            this.Row = -1;
            this.Col = -1;
        }
        public Coordinates(int col, int row)
        {
            this.Row = row;
            this.Col = col;
        }

        public Vector2 ToVector2() => new Vector2(Col, Row);
        public bool Equals(Coordinates other) => Row == other.Row && Col == other.Col;

        public override bool Equals(object? obj) => obj is Coordinates other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Row, Col);
        
        public static bool operator ==(Coordinates left, Coordinates right) => left.Equals(right);

        public static bool operator !=(Coordinates left, Coordinates right) => !left.Equals(right);
        
        public override string ToString()
        {
            return $"({Col},{Row})";
        }
    }
    
    public readonly int Rows;
    public readonly int Cols;
    public readonly bool LeftToRight;
    
    public bool IsValid => Rows > 0 && Cols > 0;
    public int Count => Rows * Cols;
    
    
    public Grid()
    {
        this.Rows = 0;
        this.Cols = 0;
        this.LeftToRight = true;
    }
    public Grid(int rows, int cols)
    {
        this.Rows = rows;
        this.Cols = cols;
        this.LeftToRight = true;
    }
    public Grid(int rows, int cols, bool leftToRight)
    {
        this.Rows = rows;
        this.Cols = cols;
        this.LeftToRight = leftToRight;
    }

    public bool IsIndexInBounds(int index) => index >= 0 && index <= Count;
    public Vector2 GetCellSize(Rect bounds) => IsValid ? new Vector2(bounds.Width / Cols, bounds.Height / Rows) : new();
    
    public int GetCellIndex(Vector2 pos, Rect bounds)
    {
        return CoordinatesToIndex(GetCellCoordinate(pos, bounds));
    }
    public Coordinates GetCellCoordinate(Vector2 pos, Rect bounds)
    {
        var cellSize = GetCellSize(bounds);
        int xi = Math.Clamp((int)Math.Floor((pos.X - bounds.X) / cellSize.X), 0, Cols - 1);
        int yi = Math.Clamp((int)Math.Floor((pos.Y - bounds.Y) / cellSize.Y), 0, Rows - 1);
        return new(xi, yi);
    }
    

    public int GetCellIndices(Rect rect, Rect bounds, ref HashSet<int> indices)
    {
        var topLeft = GetCellCoordinate(rect.TopLeft, bounds);
        var bottomRight = GetCellCoordinate(rect.BottomRight, bounds);

        int count = indices.Count;
        for (int j = topLeft.Row; j <= bottomRight.Row; j++)
        {
            for (int i = topLeft.Col; i <= bottomRight.Col; i++)
            {
                int id = CoordinatesToIndex(new(i, j));
                indices.Add(id);
            }
        }

        return indices.Count - count;
    }
    
    public Coordinates IndexToCoordinates(int index)
    {
        if (!IsValid) return new();
        
        if (LeftToRight)
        {
            int row = index / Cols;
            int col = index % Cols;
            return new(col, row);
        }
        else
        {
            int col = index / Rows;
            int row = index % Rows;
            return new(col, row);
        }
            
    }
    
    public int CoordinatesToIndex(Coordinates coordinates)
    {
        if (!IsValid || !coordinates.IsValid) return -1;
        
        if (LeftToRight)
        {
            return coordinates.Row * Cols + coordinates.Col;
        }
        else
        {
            return coordinates.Col * Rows + coordinates.Row;
        }
    }

    public Direction GetDirection(Coordinates coordinates)
    {
        if (!coordinates.IsValid) return new();


        var hor = coordinates.Col == 0 ? -1 : coordinates.Col >= Cols - 1 ? 1 : 0;
        var ver = coordinates.Row == 0 ? -1 : coordinates.Row >= Rows - 1 ? 1 : 0;
        return new(hor, ver);

        // if (coordinates.Row == 0 && coordinates.Col == 0) return new(-1, -1);//topleft
        // if (coordinates.Row == 0 && coordinates.Col > 0 && coordinates.Col < Cols) return new(0, -1);//top
        // if (coordinates.Row == 0 && coordinates.Col >= Cols) return new(1, -1);//topRight

        // if (coordinates.Row > 0 && coordinates.Row < Rows && coordinates.Col >= Cols) return new(1, 0);//right
        // if (coordinates.Row >= Rows && coordinates.Col >= Cols) return new(1, 1); //bottom right
        // if (coordinates.Row >= Rows && coordinates.Col > 0 && coordinates.Col < Cols) return new(0, 1); //bottom
        // if (coordinates.Row >= Rows && coordinates.Col == 0) return new(0, 1); //bottomLeft

    }

    public bool Equals(Grid other) => Rows == other.Rows && Cols == other.Cols && LeftToRight == other.LeftToRight;

    public override bool Equals(object? obj) => obj is Grid other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Rows, Cols, LeftToRight);
    
    public static bool operator ==(Grid left, Grid right) => left.Equals(right);

    public static bool operator !=(Grid left, Grid right) => !left.Equals(right);
    
    public override string ToString()
    {
        var leftToRightText = LeftToRight ? "L->R" : "T->B";
        return $"Cols: {Cols}, Rows: {Rows}, {leftToRightText})";
    }
}