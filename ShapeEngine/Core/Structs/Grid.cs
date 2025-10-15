using System.Numerics;
using ShapeEngine.Geometry.RectDef;

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
        var alignment = Placement.Invert().ToAlignement();
        var pos = bounds.GetPoint(alignment);
        return new(pos, cellSize, alignment);
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
        if (!IsValid) return new(-1, -1);
        
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
        if (!IsValid || !coordinates.IsPositive) return -1;
        
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
        if (!coordinates.IsPositive) return new();


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