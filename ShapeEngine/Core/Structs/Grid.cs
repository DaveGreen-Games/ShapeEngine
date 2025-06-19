using System.Numerics;
using ShapeEngine.Core.Shapes;

namespace ShapeEngine.Core.Structs;

/// <summary>
/// Represents a 2D grid structure with customizable orientation, placement, and cell access utilities.
/// </summary>
/// <remarks>
/// Provides methods for coordinate/index conversion, cell size calculation, and grid navigation.
/// </remarks>
public readonly struct Grid : IEquatable<Grid>
{
    /// <summary>
    /// Represents a coordinate (row, column) within a <see cref="Grid"/>.
    /// </summary>
    /// <remarks>
    /// Provides arithmetic and comparison operators, as well as conversion to <see cref="Vector2"/>.
    /// </remarks>
    public readonly struct Coordinates : IEquatable<Coordinates>
    {
        /// <summary>
        /// The row index of the coordinate.
        /// </summary>
        public readonly int Row;
        /// <summary>
        /// The column index of the coordinate.
        /// </summary>
        public readonly int Col;
        /// <summary>
        /// Gets whether the coordinate is valid (both row and column are non-negative).
        /// </summary>
        public bool IsValid => Row >= 0 && Col >= 0;

        /// <summary>
        /// Gets the product of the absolute values of row and column.
        /// </summary>
        public int Count
        {
            get
            {
                int r = Row < 0 ? Row * -1 : Row;
                int c = Col < 0 ? Col * -1 : Col;
                
                return r * c;
            }
        }
        /// <summary>
        /// Gets the sum of the absolute values of row and column.
        /// </summary>
        public int Distance
        {
            get
            {
                int r = Row < 0 ? Row * -1 : Row;
                int c = Col < 0 ? Col * -1 : Col;
                
                return r + c;
            }
        }

        /// <summary>
        /// Initializes a coordinate with invalid values (-1, -1).
        /// </summary>
        public Coordinates()
        {
            this.Row = -1;
            this.Col = -1;
        }
        /// <summary>
        /// Initializes a coordinate with the specified column and row.
        /// </summary>
        /// <param name="col">The column index.</param>
        /// <param name="row">The row index.</param>
        public Coordinates(int col, int row)
        {
            this.Row = row;
            this.Col = col;
        }

        /// <summary>
        /// Converts this coordinate to a <see cref="Vector2"/>.
        /// </summary>
        /// <returns>A <see cref="Vector2"/> with (Col, Row).</returns>
        public Vector2 ToVector2() => new Vector2(Col, Row);

        #region Operators
        /// <summary>
        /// Determines whether this coordinate is equal to another coordinate.
        /// </summary>
        /// <param name="other">The coordinate to compare with.</param>
        /// <returns>True if the coordinates are equal; otherwise, false.</returns>
        public bool Equals(Coordinates other) => Row == other.Row && Col == other.Col;

        /// <summary>
        /// Determines whether this coordinate is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with.</param>
        /// <returns>True if the object is a <see cref="Coordinates"/> and is equal; otherwise, false.</returns>
        public override bool Equals(object? obj) => obj is Coordinates other && Equals(other);

        /// <summary>
        /// Returns a hash code for this coordinate.
        /// </summary>
        /// <returns>A hash code for the current coordinate.</returns>
        public override int GetHashCode() => HashCode.Combine(Row, Col);
        
        /// <summary>
        /// Determines whether two coordinates are equal.
        /// </summary>
        /// <param name="left">The first coordinate to compare.</param>
        /// <param name="right">The second coordinate to compare.</param>
        /// <returns>True if the coordinates are equal; otherwise, false.</returns>
        public static bool operator ==(Coordinates left, Coordinates right) => left.Equals(right);

        /// <summary>
        /// Determines whether two coordinates are not equal.
        /// </summary>
        /// <param name="left">The first coordinate to compare.</param>
        /// <param name="right">The second coordinate to compare.</param>
        /// <returns>True if the coordinates are not equal; otherwise, false.</returns>
        public static bool operator !=(Coordinates left, Coordinates right) => !left.Equals(right);
        
        /// <summary>
        /// Returns a string representation of the coordinate.
        /// </summary>
        /// <returns>A string describing the coordinate's column and row.</returns>
        public override string ToString()
        {
            return $"({Col},{Row})";
        }

        /// <summary>
        /// Adds two coordinates together.
        /// </summary>
        /// <param name="left">The first coordinate.</param>
        /// <param name="right">The second coordinate.</param>
        /// <returns>The sum of the two coordinates.</returns>
        public static Coordinates operator +(Coordinates left, Coordinates right)
        {
            return 
                new
                (
                    left.Col + right.Col,
                    left.Row + right.Row
                );
        }
        /// <summary>
        /// Subtracts one coordinate from another.
        /// </summary>
        /// <param name="left">The coordinate to subtract from.</param>
        /// <param name="right">The coordinate to subtract.</param>
        /// <returns>The difference of the two coordinates.</returns>
        public static Coordinates operator -(Coordinates left, Coordinates right)
        {
            return 
                new
                (
                    left.Col - right.Col,
                    left.Row - right.Row
                );
        }
        /// <summary>
        /// Multiplies two coordinates component-wise.
        /// </summary>
        /// <param name="left">The first coordinate.</param>
        /// <param name="right">The second coordinate.</param>
        /// <returns>The product of the two coordinates.</returns>
        public static Coordinates operator *(Coordinates left, Coordinates right)
        {
            return 
                new
                (
                    left.Col * right.Col,
                    left.Row * right.Row
                );
        }
        /// <summary>
        /// Divides one coordinate by another component-wise.
        /// </summary>
        /// <param name="left">The coordinate to divide.</param>
        /// <param name="right">The coordinate to divide by.</param>
        /// <returns>The quotient of the two coordinates. If a component of <paramref name="right"/> is zero,
        /// the corresponding component of <paramref name="left"/> is returned unchanged.</returns>
        public static Coordinates operator /(Coordinates left, Coordinates right)
        {
            return 
                new
                (
                    right.Col == 0 ? left.Col : left.Col / right.Col,
                    right.Row == 0 ? left.Row : left.Row / right.Row
                );
        }
        /// <summary>
        /// Adds a direction to a coordinate.
        /// </summary>
        /// <param name="left">The coordinate.</param>
        /// <param name="right">The direction to add.</param>
        /// <returns>The resulting coordinate.</returns>
        public static Coordinates operator +(Coordinates left, Direction right)
        {
            return 
                new
                (
                    left.Col + right.Horizontal,
                    left.Row + right.Vertical
                );
        }
        /// <summary>
        /// Subtracts a direction from a coordinate.
        /// </summary>
        /// <param name="left">The coordinate.</param>
        /// <param name="right">The direction to subtract.</param>
        /// <returns>The resulting coordinate.</returns>
        public static Coordinates operator -(Coordinates left, Direction right)
        {
            return 
                new
                (
                    left.Col - right.Horizontal,
                    left.Row - right.Vertical
                );
        }
        /// <summary>
        /// Multiplies a coordinate by a direction component-wise.
        /// </summary>
        /// <param name="left">The coordinate.</param>
        /// <param name="right">The direction to multiply by.</param>
        /// <returns>The resulting coordinate.</returns>
        public static Coordinates operator *(Coordinates left, Direction right)
        {
            return 
                new
                (
                    left.Col * right.Horizontal,
                    left.Row * right.Vertical
                );
        }
        /// <summary>
        /// Divides a coordinate by a direction component-wise.
        /// </summary>
        /// <param name="left">The coordinate.</param>
        /// <param name="right">The direction to divide by.</param>
        /// <returns>The resulting coordinate. If a component of <paramref name="right"/> is zero, the corresponding component of <paramref name="left"/> is returned unchanged.</returns>
        public static Coordinates operator /(Coordinates left, Direction right)
        {
            return 
                new
                (
                    right.Horizontal == 0 ? left.Col : left.Col / right.Horizontal,
                    right.Vertical == 0 ? left.Row : left.Row / right.Vertical
                );
        }
        /// <summary>
        /// Adds an integer to both components of a coordinate.
        /// </summary>
        /// <param name="left">The coordinate.</param>
        /// <param name="right">The integer to add.</param>
        /// <returns>The resulting coordinate.</returns>
        public static Coordinates operator +(Coordinates left, int right)
        {
            return 
                new
                (
                    left.Col + right,
                    left.Row + right
                );
        }
        /// <summary>
        /// Subtracts an integer from both components of a coordinate.
        /// </summary>
        /// <param name="left">The coordinate.</param>
        /// <param name="right">The integer to subtract.</param>
        /// <returns>The resulting coordinate.</returns>
        public static Coordinates operator -(Coordinates left, int right)
        {
            return 
                new
                (
                    left.Col - right,
                    left.Row - right
                );
        }
        /// <summary>
        /// Multiplies both components of a coordinate by an integer.
        /// </summary>
        /// <param name="left">The coordinate.</param>
        /// <param name="right">The integer to multiply by.</param>
        /// <returns>The resulting coordinate.</returns>
        public static Coordinates operator *(Coordinates left, int right)
        {
            return 
                new
                (
                    left.Col * right,
                    left.Row * right
                );
        }
        /// <summary>
        /// Divides both components of a coordinate by an integer.
        /// </summary>
        /// <param name="left">The coordinate.</param>
        /// <param name="right">The integer to divide by.</param>
        /// <returns>The resulting coordinate. If <paramref name="right"/> is zero, the original coordinate is returned.</returns>
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

    /// <summary>
    /// The number of rows in the grid.
    /// </summary>
    public readonly int Rows;
    /// <summary>
    /// The number of columns in the grid.
    /// </summary>
    public readonly int Cols;
    /// <summary>
    /// The direction in which the grid is placed.
    /// </summary>
    public readonly Direction Placement;
    /// <summary>
    /// Indicates if the grid is oriented top-to-bottom first.
    /// </summary>
    public readonly bool IsTopToBottomFirst;
    
    /// <summary>
    /// Get the direction for what is considered the next item.
    /// Standard Vertical Grid with 1 column that is top to bottom first would return new Direction(0, 1)
    /// </summary>
    /// <returns></returns>
    public Direction GetNextDirection()
    {
        if (!IsValid) return new();

        if (IsGrid)
        {
            return new(0, Placement.Vertical);
        }

        return Placement;
    }
    /// <summary>
    /// Get the direction for what is considered the previous item.
    /// Standard Vertical Grid with 1 column that is top to bottom first would return new Direction(0, -1)
    /// </summary>
    /// <returns></returns>
    public Direction GetPreviousDirection()
    {
        if (!IsValid) return new();
        
        if (IsGrid)
        {
            return new(0, -Placement.Vertical);
        }

        return new(-Placement.Horizontal, -Placement.Vertical);//reversed for previous
    }
    
    /// <summary>
    /// Indicates if the grid is oriented left-to-right first.
    /// </summary>
    public bool IsLeftToRightFirst => !IsTopToBottomFirst;
    /// <summary>
    /// Gets whether the grid is valid (both rows and columns are positive).
    /// </summary>
    public bool IsValid => Rows > 0 && Cols > 0;
    /// <summary>
    /// Gets whether the grid is horizontal (one row with positive columns).
    /// </summary>
    public bool IsHorizontal => Cols > 0 && Rows == 1;
    /// <summary>
    /// Gets whether the grid is vertical (one column with positive rows).
    /// </summary>
    public bool IsVertical => Rows > 0 && Cols == 1;
    /// <summary>
    /// Gets whether the grid is a true grid (positive rows and columns).
    /// </summary>
    public bool IsGrid => Cols > 1 && Rows > 1;
    /// <summary>
    /// Gets the total number of cells in the grid, or -1 if invalid.
    /// </summary>
    public int Count => Rows < 0 || Cols < 0 ? -1 : Rows * Cols;
    
    
    /// <summary>
    /// Initializes an empty grid (0x0).
    /// </summary>
    public Grid()
    {
        this.Rows = 0;
        this.Cols = 0;
        this.Placement = Direction.Empty;
        this.IsTopToBottomFirst = false;
    }
    /// <summary>
    /// Initializes a grid with the specified number of columns and rows.
    /// </summary>
    /// <param name="cols">The number of columns.</param>
    /// <param name="rows">The number of rows.</param>
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
    /// <summary>
    /// Initializes a grid with the specified number of columns and rows,
    /// with optional reversal of horizontal and vertical orientations.
    /// </summary>
    /// <param name="cols">The number of columns.</param>
    /// <param name="rows">The number of rows.</param>
    /// <param name="horizontalReversed">Whether to reverse the horizontal orientation.</param>
    /// <param name="verticalReversed">Whether to reverse the vertical orientation.</param>
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
    /// <summary>
    /// Initializes a grid with the specified number of columns and rows,
    /// with optional reversal of horizontal and vertical orientations,
    /// and specification of top-to-bottom or left-to-right first placement.
    /// </summary>
    /// <param name="cols">The number of columns.</param>
    /// <param name="rows">The number of rows.</param>
    /// <param name="horizontalReversed">Whether to reverse the horizontal orientation.</param>
    /// <param name="verticalReversed">Whether to reverse the vertical orientation.</param>
    /// <param name="isTopToBottomFirst">Whether the grid is top-to-bottom first.</param>
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


    /// <summary>
    /// Creates a standard vertical grid with the specified number of rows and reversal option.
    /// </summary>
    /// <param name="rows">The number of rows.</param>
    /// <param name="reversed">Whether to reverse the vertical orientation.</param>
    /// <returns>A vertical <see cref="Grid"/>.</returns>
    public static Grid GetVerticalGrid(int rows, bool reversed) => new(1, rows, false, reversed, false);
    /// <summary>
    /// Creates a standard horizontal grid with the specified number of columns and reversal option.
    /// </summary>
    /// <param name="cols">The number of columns.</param>
    /// <param name="reversed">Whether to reverse the horizontal orientation.</param>
    /// <returns>A horizontal <see cref="Grid"/>.</returns>
    public static Grid GetHorizontalGrid(int cols, bool reversed) => new(cols, 1, reversed, false, false);
    
    
    
    /// <summary>
    /// Determines if the given index is within the bounds of the grid.
    /// </summary>
    /// <param name="index">The index to check.</param>
    /// <returns>True if the index is within bounds, otherwise false.</returns>
    public bool IsIndexInBounds(int index) => index >= 0 && index <= Count;
    /// <summary>
    /// Calculates the size of each cell in the grid based on the given bounds.
    /// </summary>
    /// <param name="bounds">The bounds within which the grid is defined.</param>
    /// <returns>The size of each cell in the grid.</returns>
    public Size GetCellSize(Rect bounds) => IsValid ? new Size(bounds.Width / Cols, bounds.Height / Rows) : new();
    
    /// <summary>
    /// Gets the index of the cell at the given position within the specified bounds.
    /// </summary>
    /// <param name="pos">The position within the grid.</param>
    /// <param name="bounds">The bounds of the grid.</param>
    /// <returns>The index of the cell.</returns>
    public int GetCellIndex(Vector2 pos, Rect bounds)
    {
        return CoordinatesToIndex(GetCellCoordinate(pos, bounds));
    }
    /// <summary>
    /// Gets the index of the cell at the given position within the specified bounds, unclamped.
    /// </summary>
    /// <param name="pos">The position within the grid.</param>
    /// <param name="bounds">The bounds of the grid.</param>
    /// <returns>The index of the cell, or -1 if out of bounds.</returns>
    public int GetCellIndexUnclamped(Vector2 pos, Rect bounds)
    {
        var result = GetCellCoordinate(pos, bounds);
        if (!AreCoordinatesInside(result)) return -1;
        return CoordinatesToIndex(result);
    }
    /// <summary>
    /// Gets the grid coordinates of the cell at the given position within the specified bounds.
    /// </summary>
    /// <param name="pos">The position within the grid.</param>
    /// <param name="bounds">The bounds of the grid.</param>
    /// <returns>The coordinates of the cell.</returns>
    public Coordinates GetCellCoordinate(Vector2 pos, Rect bounds)
    {
        var cellSize = GetCellSize(bounds);
        int xi = Math.Clamp((int)Math.Floor((pos.X - bounds.X) / cellSize.Width), 0, Cols - 1);
        int yi = Math.Clamp((int)Math.Floor((pos.Y - bounds.Y) / cellSize.Height), 0, Rows - 1);
        return new(xi, yi);
    }
    /// <summary>
    /// Gets the grid coordinates of the cell at the given position within the specified bounds, unclamped.
    /// </summary>
    /// <param name="pos">The position within the grid.</param>
    /// <param name="bounds">The bounds of the grid.</param>
    /// <returns>The coordinates of the cell.</returns>
    public Coordinates GetCellCoordinateUnclamped(Vector2 pos, Rect bounds)
    {
        var cellSize = GetCellSize(bounds);
        int xi = (int)Math.Floor((pos.X - bounds.X) / cellSize.Width);
        int yi = (int)Math.Floor((pos.Y - bounds.Y) / cellSize.Height);
        return new(xi, yi);
    }
    /// <summary>
    /// Clamps the given coordinates to be within the bounds of the grid.
    /// </summary>
    /// <param name="coordinates">The coordinates to clamp.</param>
    /// <returns>The clamped coordinates.</returns>
    public Coordinates ClampCoordinates(Coordinates coordinates)
    {
        var col = coordinates.Col < 0 ? 0 : coordinates.Col > Cols ? Cols - 1 : coordinates.Col;
        var row = coordinates.Row < 0 ? 0 : coordinates.Row > Rows ? Rows - 1 : coordinates.Row;
        return new(col, row);
        
    }

    /// <summary>
    /// Gets the position of the cell with the given coordinates within the specified bounds.
    /// </summary>
    /// <param name="bounds">The bounds of the grid.</param>
    /// <param name="coordinates">The coordinates of the cell.</param>
    /// <returns>The position of the cell.</returns>
    public Vector2 GetPosition(Rect bounds, Coordinates coordinates)
    {
        var cellSize = GetCellSize(bounds);
        var pos = bounds.GetPoint(Placement.Invert().ToAlignement());

        return pos + cellSize * coordinates.ToVector2() * Placement.ToVector2();
    }

    /// <summary>
    /// Gets the rectangle representing the bounds of the cell with the given coordinates within the specified bounds.
    /// </summary>
    /// <param name="bounds">The bounds of the grid.</param>
    /// <param name="coordinates">The coordinates of the cell.</param>
    /// <returns>The rectangle representing the cell's bounds.</returns>
    public Rect GetRect(Rect bounds, Coordinates coordinates)
    {
        var cellSize = GetCellSize(bounds);
        var alignement = Placement.Invert().ToAlignement();
        var pos = bounds.GetPoint(alignement);
        return new(pos, cellSize, alignement);
    }
    /// <summary>
    /// Determines if the given coordinates are inside the bounds of the grid.
    /// </summary>
    /// <param name="coordinates">The coordinates to check.</param>
    /// <returns>True if the coordinates are inside, otherwise false.</returns>
    public bool AreCoordinatesInside(Coordinates coordinates)
    {
        if (coordinates.Col < 0 || coordinates.Col >= Cols) return false;
        if (coordinates.Row < 0 || coordinates.Row >= Rows) return false;
        return true;
    }

    /// <summary>
    /// Gets the cell indices that intersect with the given rectangle within the specified bounds.
    /// </summary>
    /// <param name="rect">The rectangle to check for intersections.</param>
    /// <param name="bounds">The bounds of the grid.</param>
    /// <param name="indices">A set to which the intersecting indices will be added.</param>
    /// <returns>The number of new indices added.</returns>
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
    
    /// <summary>
    /// Converts the given index to grid coordinates.
    /// </summary>
    /// <param name="index">The index to convert.</param>
    /// <returns>The coordinates corresponding to the index.</returns>
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
    
    /// <summary>
    /// Converts the given coordinates to an index.
    /// </summary>
    /// <param name="coordinates">The coordinates to convert.</param>
    /// <returns>The index corresponding to the coordinates.</returns>
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

    /// <summary>
    /// Gets the direction of movement (horizontal, vertical) based on the given coordinates within the grid.
    /// </summary>
    /// <param name="coordinates">The coordinates to check.</param>
    /// <returns>A <see cref="Direction"/> indicating the movement direction.</returns>
    public Direction GetDirection(Coordinates coordinates)
    {
        if (!coordinates.IsValid) return new();


        var hor = coordinates.Col == 0 ? -1 : coordinates.Col >= Cols - 1 ? 1 : 0;
        var ver = coordinates.Row == 0 ? -1 : coordinates.Row >= Rows - 1 ? 1 : 0;
        return new(hor, ver);

    }

    /// <summary>
    /// Fills the given list with rectangles representing each cell in the grid.
    /// </summary>
    /// <param name="bounds">The bounds of the grid.</param>
    /// <param name="result">The list to fill with cell rectangles.</param>
    /// <returns>The number of rectangles added to the list.</returns>
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
    /// <summary>
    /// Determines whether this grid is equal to another grid.
    /// </summary>
    /// <param name="other">The grid to compare with.</param>
    /// <returns>True if the grids are equal; otherwise, false.</returns>
    public bool Equals(Grid other) => Rows == other.Rows && Cols == other.Cols && Placement == other.Placement && IsTopToBottomFirst == other.IsTopToBottomFirst;

    /// <summary>
    /// Determines whether this grid is equal to another object.
    /// </summary>
    /// <param name="obj">The object to compare with.</param>
    /// <returns>True if the object is a <see cref="Grid"/> and is equal; otherwise, false.</returns>
    public override bool Equals(object? obj) => obj is Grid other && Equals(other);

    /// <summary>
    /// Returns a hash code for this grid.
    /// </summary>
    /// <returns>A hash code for the current grid.</returns>
    public override int GetHashCode() => HashCode.Combine(Rows, Cols, Placement, IsTopToBottomFirst);
    
    /// <summary>
    /// Determines whether two grids are equal.
    /// </summary>
    /// <param name="left">The first grid to compare.</param>
    /// <param name="right">The second grid to compare.</param>
    /// <returns>True if the grids are equal; otherwise, false.</returns>
    public static bool operator ==(Grid left, Grid right) => left.Equals(right);

    /// <summary>
    /// Determines whether two grids are not equal.
    /// </summary>
    /// <param name="left">The first grid to compare.</param>
    /// <param name="right">The second grid to compare.</param>
    /// <returns>True if the grids are not equal; otherwise, false.</returns>
    public static bool operator !=(Grid left, Grid right) => !left.Equals(right);
    
    /// <summary>
    /// Returns a string representation of the grid.
    /// </summary>
    /// <returns>A string describing the grid's columns, rows, and orientation.</returns>
    public override string ToString()
    {
        var leftToRightText = !IsTopToBottomFirst ? "L->R" : "T->B";
        return $"Cols: {Cols}, Rows: {Rows}, {leftToRightText})";
    }
    
    #endregion
    
}