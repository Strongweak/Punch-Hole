using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmptySearch
{
    private static int ROW, COL;
    private List<List<(int, int)>> islands;

    public EmptySearch(int row, int col)
    {
        ROW = row;
        COL = col;
        islands = new List<List<(int, int)>>();
    }
    public List<List<(int, int)>> GetIslands()
    {
        return islands;
    }
    // No of rows
    // and columns

    // A function to check if
    // a given cell (row, col)
    // can be included in DFS
    public static bool isSafe(int[, ] M, int row, int col,
        bool[, ] visited)
    {
        // row number is in range,
        // column number is in range
        // and value is 1 and not
        // yet visited
        return (row >= 0) && (row < ROW) && (col >= 0)
               && (col < COL)
               && (M[row, col] == 1 && !visited[row, col]);
    }

    // A utility function to do
    // DFS for a 2D boolean matrix.
    // It only considers the 8
    // neighbors as adjacent vertices
    public static void DFS(int[, ] M, int row, int col,
        bool[, ] visited, List<(int, int)> currentIsland)
    {
        // These arrays are used to
        // get row and column numbers
        // of 8 neighbors of a given cell
        int[] rowNbr
            = new int[] { -1, -1, -1, 0, 0, 1, 1, 1 };
        int[] colNbr
            = new int[] { -1, 0, 1, -1, 1, -1, 0, 1 };

        // Mark this cell
        // as visited
        visited[row, col] = true;
        currentIsland.Add((row, col));

        // Recur for all
        // connected neighbours
        for (int k = 0; k < 8; ++k)
            if (isSafe(M, row + rowNbr[k], col + colNbr[k],
                    visited))
                DFS(M, row + rowNbr[k], col + colNbr[k],
                    visited,currentIsland);
    }
    // The main function that
    // returns count of islands
    // in a given boolean 2D matrix
    public int countIslands(int[, ] M)
    {
        // Make a bool array to
        // mark visited cells.
        // Initially all cells
        // are unvisited
        bool[, ] visited = new bool[ROW, COL];

        // Initialize count as 0 and
        // traverse through the all
        // cells of given matrix
        int count = 0;
        for (int i = 0; i < ROW; ++i)
        for (int j = 0; j < COL; ++j)
            if (M[i, j] == 1 && !visited[i, j]) {
                // If a cell with value 1 is not
                // visited yet, then new island
                // found, Visit all cells in this
                // island and increment island count
                List<(int, int)> currentIsland = new List<(int, int)>();
                DFS(M, i, j, visited,currentIsland);
                islands.Add(currentIsland);
                ++count;
            }
        return count;
    }
    public bool CanShapeFit(bool[,] shape, List<(int, int)> island)
    {
        int shapeRows = shape.GetLength(0);
        int shapeCols = shape.GetLength(1);

        foreach ((int startX, int startY) in island)
        {
            bool fits = true;
            for (int i = 0; i < shapeRows; i++)
            {
                for (int j = 0; j < shapeCols; j++)
                {
                    if (shape[i, j] && !island.Contains((startX + i, startY + j)))
                    {
                        fits = false;
                        break;
                    }
                }
                if (!fits) break;
            }
            if (fits) return true;
        }
        return false;
    }
}
