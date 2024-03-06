using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.Structs;

public readonly struct Grid : IEquatable<Grid>
{
    public readonly struct Coordinates : IEquatable<Coordinates>
    {
        public readonly int Row;
        public readonly int Col;
        public bool IsValid => Row >= 0 && Col >= 0;

        public int Distance
        {
            get
            {
                int r = Row < 0 ? Row * -1 : Row;
                int c = Col < 0 ? Col * -1 : Col;
                
                return r + c;
            }
        }
    
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

        #region Operators
        public bool Equals(Coordinates other) => Row == other.Row && Col == other.Col;

        public override bool Equals(object? obj) => obj is Coordinates other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Row, Col);
        
        public static bool operator ==(Coordinates left, Coordinates right) => left.Equals(right);

        public static bool operator !=(Coordinates left, Coordinates right) => !left.Equals(right);
        
        public override string ToString()
        {
            return $"({Col},{Row})";
        }

        public static Coordinates operator +(Coordinates left, Coordinates right)
        {
            return 
                new
                (
                    left.Col + right.Col,
                    left.Row + right.Row
                );
        }
        public static Coordinates operator -(Coordinates left, Coordinates right)
        {
            return 
                new
                (
                    left.Col - right.Col,
                    left.Row - right.Row
                );
        }
        public static Coordinates operator *(Coordinates left, Coordinates right)
        {
            return 
                new
                (
                    left.Col * right.Col,
                    left.Row * right.Row
                );
        }
        public static Coordinates operator /(Coordinates left, Coordinates right)
        {
            return 
                new
                (
                    right.Col == 0 ? left.Col : left.Col / right.Col,
                    right.Row == 0 ? left.Row : left.Row / right.Row
                );
        }
        public static Coordinates operator +(Coordinates left, Direction right)
        {
            return 
                new
                (
                    left.Col + right.Horizontal,
                    left.Row + right.Vertical
                );
        }
        public static Coordinates operator -(Coordinates left, Direction right)
        {
            return 
                new
                (
                    left.Col - right.Horizontal,
                    left.Row - right.Vertical
                );
        }
        public static Coordinates operator *(Coordinates left, Direction right)
        {
            return 
                new
                (
                    left.Col * right.Horizontal,
                    left.Row * right.Vertical
                );
        }
        public static Coordinates operator /(Coordinates left, Direction right)
        {
            return 
                new
                (
                    right.Horizontal == 0 ? left.Col : left.Col / right.Horizontal,
                    right.Vertical == 0 ? left.Row : left.Row / right.Vertical
                );
        }
        public static Coordinates operator +(Coordinates left, int right)
        {
            return 
                new
                (
                    left.Col + right,
                    left.Row + right
                );
        }
        public static Coordinates operator -(Coordinates left, int right)
        {
            return 
                new
                (
                    left.Col - right,
                    left.Row - right
                );
        }
        public static Coordinates operator *(Coordinates left, int right)
        {
            return 
                new
                (
                    left.Col * right,
                    left.Row * right
                );
        }
        public static Coordinates operator /(Coordinates left, int right)
        {
            if (right == 0) return left;
            return 
                new
                (
                    left.Col / right,
                    left.Row / right
                );
        }
        
        #endregion
    }

    // public readonly Rect Rect;
    // public readonly Direction Direction;
    public readonly int Rows;
    public readonly int Cols;
    public readonly Direction Placement;
    public readonly bool IsTopToBottomFirst;
    
    public bool IsLeftToRightFirst => !IsTopToBottomFirst;
    public bool IsValid => Rows > 0 && Cols > 0;
    public bool IsHorizontal => Cols > 0 && Rows == 1;
    // public bool IsHorizontalInvalid => Cols > 0 && Rows == 0;
    public bool IsVertical => Rows > 0 && Cols == 1;
    // public bool IsVerticalInvalid => Rows > 0 && Cols == 0;
    public bool IsGrid => Cols > 1 && Rows > 1;
    public int Count => Rows < 0 || Cols < 0 ? -1 : Rows * Cols;
    
    
    public Grid()
    {
        this.Rows = 0;
        this.Cols = 0;
        this.Placement = Direction.Empty;
        this.IsTopToBottomFirst = false;
    }
    public Grid(int cols, int rows)
    {
        this.Cols = cols;
        this.Rows = rows;
        this.Placement = new
        (
            cols <= 1 ? 0 : 1,    
            rows <= 1 ? 0 : 1    
        );
        this.IsTopToBottomFirst = false;
    }
    public Grid(int cols, int rows, bool horizontalReversed = false, bool verticalReversed = false)
    {
        this.Cols = cols;
        this.Rows = rows;
        this.Placement = new
            (
                cols <= 1 ? 0 : horizontalReversed ? -1 : 1,    
                rows <= 1 ? 0 : verticalReversed ? -1 : 1    
            );
        this.IsTopToBottomFirst = false;
    }
    public Grid(int cols, int rows, bool horizontalReversed = false, bool verticalReversed = false, bool isTopToBottomFirst = false)
    {
        this.Cols = cols;
        this.Rows = rows;
        this.Placement = new
        (
            cols <= 1 ? 0 : horizontalReversed ? -1 : 1,    
            rows <= 1 ? 0 : verticalReversed ? -1 : 1    
        );
        this.IsTopToBottomFirst = isTopToBottomFirst;
    }


    public static Grid GetVerticalGrid(int rows, bool reversed) => new(1, rows, false, reversed, false);
    public static Grid GetHorizontalGrid(int cols, bool reversed) => new(cols, 1, reversed, false, false);
    
    
    
    public bool IsIndexInBounds(int index) => index >= 0 && index <= Count;
    public Size GetCellSize(Rect bounds) => IsValid ? new Size(bounds.Width / Cols, bounds.Height / Rows) : new();
    
    public int GetCellIndex(Vector2 pos, Rect bounds)
    {
        return CoordinatesToIndex(GetCellCoordinate(pos, bounds));
    }
    public int GetCellIndexUnclamped(Vector2 pos, Rect bounds)
    {
        var result = GetCellCoordinate(pos, bounds);
        if (!AreCoordinatesInside(result)) return -1;
        return CoordinatesToIndex(result);
    }
    public Coordinates GetCellCoordinate(Vector2 pos, Rect bounds)
    {
        var cellSize = GetCellSize(bounds);
        int xi = Math.Clamp((int)Math.Floor((pos.X - bounds.X) / cellSize.Width), 0, Cols - 1);
        int yi = Math.Clamp((int)Math.Floor((pos.Y - bounds.Y) / cellSize.Height), 0, Rows - 1);
        return new(xi, yi);
    }
    public Coordinates GetCellCoordinateUnclamped(Vector2 pos, Rect bounds)
    {
        var cellSize = GetCellSize(bounds);
        int xi = (int)Math.Floor((pos.X - bounds.X) / cellSize.Width);
        int yi = (int)Math.Floor((pos.Y - bounds.Y) / cellSize.Height);
        return new(xi, yi);
    }
    public Coordinates ClampCoordinates(Coordinates coordinates)
    {
        var col = coordinates.Col < 0 ? 0 : coordinates.Col > Cols ? Cols - 1 : coordinates.Col;
        var row = coordinates.Row < 0 ? 0 : coordinates.Row > Rows ? Rows - 1 : coordinates.Row;
        return new(col, row);
        
    }

    public Vector2 GetPosition(Rect bounds, Coordinates coordinates)
    {
        var cellSize = GetCellSize(bounds);
        var pos = bounds.GetPoint(Placement.Invert().ToAlignement());

        return pos + cellSize * coordinates.ToVector2() * Placement.ToVector2();
    }

    public Rect GetRect(Rect bounds, Coordinates coordinates)
    {
        var cellSize = GetCellSize(bounds);
        var alignement = Placement.Invert().ToAlignement();
        var pos = bounds.GetPoint(alignement);
        return new(pos, cellSize, alignement);
    }
    public bool AreCoordinatesInside(Coordinates coordinates)
    {
        if (coordinates.Col < 0 || coordinates.Col >= Cols) return false;
        if (coordinates.Row < 0 || coordinates.Row >= Rows) return false;
        return true;
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
        
        if (!IsTopToBottomFirst)
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
        
        if (IsLeftToRightFirst)
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

    }

    public int GetRects(Rect bounds, ref List<Rect> result)
    {
        if (!IsValid) return 0;
        
        int count = result.Count;

        if (IsLeftToRightFirst)
        {
            for (var row = 0; row <= Rows; row++)
            {
                for (var col = 0; col <= Cols; col++)
                {
                    var coordinates = new Coordinates(col, row);
                    result.Add(GetRect(bounds, coordinates));
                
                }
            }
        }
        else
        {
            
            for (var col = 0; col <= Cols; col++)
            {
                for (var row = 0; row <= Rows; row++)
                {
                    var coordinates = new Coordinates(col, row);
                    result.Add(GetRect(bounds, coordinates));
                
                }
            }
        }
        

        return result.Count - count;
    }

    #region Operators
    public bool Equals(Grid other) => Rows == other.Rows && Cols == other.Cols && Placement == other.Placement && IsTopToBottomFirst == other.IsTopToBottomFirst;

    public override bool Equals(object? obj) => obj is Grid other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Rows, Cols, Placement, IsTopToBottomFirst);
    
    public static bool operator ==(Grid left, Grid right) => left.Equals(right);

    public static bool operator !=(Grid left, Grid right) => !left.Equals(right);
    
    public override string ToString()
    {
        var leftToRightText = !IsTopToBottomFirst ? "L->R" : "T->B";
        return $"Cols: {Cols}, Rows: {Rows}, {leftToRightText})";
    }

    
    #endregion
    
}