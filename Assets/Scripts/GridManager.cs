using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public class GridManager : MonoBehaviour
{
    public GameObject tilePrefab;
    public int rows = 11;
    public int columns = 9;
    public float spacing = 1.1f; // spacing between tiles
    
    public int stage = 1; // Default stage to test
    
    public NumTileController selectedTile;
    
    private NumTileController[] _tiles;
    
    public UnityEvent tileMatched;
    public UnityEvent rowCleared;
    public UnityEvent tileSelected;
    public UnityEvent helperUsed;
    
    public UnityEvent gameWin;
    public UnityEvent gameOver;

    public int helper = 6;
    public Text helperText;
    void Start()
    {
        _tiles = new NumTileController[rows * columns];
        GenerateGrid();
        
        // TestStage(stage);
    }

    int GetIndex(int x, int y) => y * columns + x;
    (int x, int y) GetXY(int index) => (index % columns, index / columns);

    public void SelectTile(NumTileController tile)
    {
        if (tile == null) return;
        HighlightAllValidPairs();
        int tileIndex = System.Array.IndexOf(_tiles, tile);
        var (x1, y1) = GetXY(tileIndex);
        
        if (tileIndex == -1)
        {
            Debug.LogWarning("Tile not found in _tiles array.");
            return;
        }
        // Check if the selected tile is already amatched
        if (tile.IsMatched())
        {
            Debug.Log("Tile is already matched.");
            return;
        }

        // Check if the selected tile is empty
        if (tile.IsEmpty())
        {
            Debug.Log("Selected tile is empty.");
            return;
        }
        
        if (tile == selectedTile)
        {
            // If the same tile is clicked again, deselect it
            selectedTile.SetHighlight(false);
            selectedTile = null;
            return;
        }
        
        if (selectedTile != null)
        {

            int selectedTileIndex = System.Array.IndexOf(_tiles, selectedTile);
            var (x2, y2) = GetXY(selectedTileIndex);
            Debug.Log($"Selected tile: {tile.name} at ({x1}, {y1}), Previous selected tile: {selectedTile?.name} at ({x2}, {y2})");
            if (IsValidMatch(x1, y1, x2, y2))
            {
                // If a valid match is found, clear the matched tiles
                MatchedTile(tile, selectedTile);
                ClearAllMatchedRows();
                tileMatched.Invoke();
                if (AllTilesEmpty())
                {
                    Debug.Log("All tiles are empty. Game Over!");
                    // Handle game over logic here, e.g., show a message or reset the game
                    gameWin.Invoke();
                    return;
                }

                if (FindAllValidPairs().Count == 0 && helper <= 0) // && helper == 0
                {
                    Debug.Log("No valid pairs left. Game Over!");
                    // Handle game over logic here, e.g., show a message or reset the game
                    gameOver.Invoke();
                }
            }
            selectedTile.SetHighlight(false);
            selectedTile = null;
            return;
        }
        selectedTile = tile;
        // If a tile is selected, highlight it
        selectedTile.SetHighlight(true);
        tileSelected.Invoke();
    }
    
    /*
     * Generates a grid of _tiles based on the specified number of rows and columns.
     * 
     */
    void GenerateGrid()
    {
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                Vector2 position = new Vector2(x * spacing, y * -spacing); // Top-down layout
                GameObject tileObj = Instantiate(tilePrefab, position, Quaternion.identity, transform);
                NumTileController tile = tileObj.GetComponent<NumTileController>();
                tile.Clear();
                int index = GetIndex(x, y);
                _tiles[index] = tile;
                tile.name = $"Tile_{y}_{x}";
            }
        }
        float width = columns * spacing;
        float height = rows * spacing;
        // Center the grid in the scene
        transform.position = new Vector3(-width / 2 + spacing / 2, height / 2 - spacing / 2, 0);
    }
    
    /**
     * Applies a predefined stage to the board by setting the values of each tile.
     * * @param List int: A list of integers representing the values to set on the tiles.
     * 
     */
    void ApplyStageToBoard(List<int> values)
    {
        int i = 0;
        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                _tiles[GetIndex(x, y)].SetValue(values[i++]);
            }
        }
    }
    
    public void TestStage(int stage)
    {
        helper = 6;

        helperText.text = helper.ToString();
        selectedTile = null;
        foreach (var tile in _tiles) tile.SetHighlight(false);
        List<int> values = GenerateStage(stage);
        ApplyStageToBoard(values);
    }
    
    /**
     * Generates a stage with a specific number of pairs based on the stage number.
     * 
     * @param stage The stage number (1, 2, or 3).
     * @return A list of integers representing the values to set on the tiles.
     */
    public List<int> GenerateStage(int stage)
    {
        int pairCount = stage switch { 1 => 3, 2 => 2, _ => 1 };
        int totalTiles = 3 * columns;
        List<int> distribution = new List<int> { 4, 3, 4, 4, 3, 4, 4, 3, 4 }; // 1-9

        List<int> fullValues = new();
        for (int i = 0; i < 9; i++)
        for (int j = 0; j < distribution[i]; j++)
            fullValues.Add(i + 1);
        
        List<int> working = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        System.Random rng = new();
        
        for (int i = 0; i < totalTiles - 9; i++)
        {
            int temp = fullValues[rng.Next(fullValues.Count)];
            working.Add(temp);
            fullValues.Remove(temp);
        }
        
        working = working.OrderBy(x => rng.Next()).ToList();
        // Initial shuffle
        for (int i = working.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (working[i], working[j]) = (working[j], working[i]);
        }

        // Apply to board
        ApplyStageToBoard(working);

        int attempt = 0;
        while (attempt < 20000)
        {
            attempt++;
            var validPairs = FindAllValidPairs();

            if (validPairs.Count == pairCount)
                break;

            // If we have more than enough valid pairs, replace one of them with a random value
            if (validPairs.Count > pairCount)
            {
                var (a, b) = validPairs[rng.Next(validPairs.Count)];

                int replacement;
                do
                {
                    replacement = fullValues[rng.Next(fullValues.Count)];
                } while (replacement == working[a] || replacement == working[b]);
                
                fullValues.Add(working[a]); 
                
                working[a] = replacement;

                fullValues.Remove(replacement);
            }
            else
            {
                // Not enough valid pairs: swap two random
                int i1 = rng.Next(totalTiles);
                int i2 = rng.Next(totalTiles);
                (working[i1], working[i2]) = (working[i2], working[i1]);
            }

            ApplyStageToBoard(working);
        }

        Debug.Log($"Generated stage in {attempt} refinement steps.");
        return working;
    }

    
    /**
     * Finds all valid pairs of tiles that can be matched based on the game rules.
     * 
     * @return A list of tuples containing the indices of valid pairs.
     */
    public List<(int indexA, int indexB)> FindAllValidPairs()
    {
        var pairs = new List<(int, int)>();
        HashSet<int> used = new();


        for (int i = 0; i < _tiles.Length; i++)
        {
            if (used.Contains(i)) continue;
            var (x1, y1) = GetXY(i);
            for (int j = i + 1; j < _tiles.Length; j++)
            {
                if (used.Contains(j)) continue;
                var (x2, y2) = GetXY(j);
                if (IsValidMatch(x1, y1, x2, y2))
                {
                    pairs.Add((i, j)); 
                    used.Add(i);
                    used.Add(j);
                    break; // Only one match per tile
                }
            }
        }

        return pairs;
    }

    /**
     * Clears a specific row by shifting down the rows above it and clearing the bottom row.
     * 
     * @param rowToClear The index of the row to clear.
     */
    public void ClearRow(int rowToClear)
    {
        int highestRow = GetHighestFilledRow();
        // Clear the row
        // for (int x = 0; x < columns; x++)
        // {
        //     int index = GetIndex(x, rowToClear);
        //     _tiles[index].Clear();
        // }
        
        // Shift down rows above
        for (int y = rowToClear; y < highestRow; y++)
        {

            for (int x = 0; x < columns; x++)
            {
                int fromIndex = GetIndex(x, y + 1);
                int toIndex = GetIndex(x, y);
                _tiles[toIndex].SetValue(_tiles[fromIndex].GetValue());
                if (_tiles[fromIndex].IsMatched())
                {
                    _tiles[toIndex].MarkMatched();
                }
            }
        }

        // Clear bottom row
        for (int x = 0; x < columns; x++)
        {
            _tiles[GetIndex(x, highestRow)].Clear();
        }
        rowCleared.Invoke();
    }
    
    /**
     * Gets the highest filled row in the grid.
     * 
     * @return The index of the highest filled row, or -1 if no rows are filled.
     */
    int GetHighestFilledRow()
    {
        for (int i = 0; i < columns * rows; i++)
        {
            if (_tiles[i].IsEmpty())
                return i / columns; // Return the row index
        }
        
        return -1;
    }

    /**
     * Replicates numbers down to fill empty tiles with existing values.
     * 
     * This method collects all non-empty tile values and fills empty tiles
     * from the top down with these values, maintaining the order of appearance.
     */
    public void ReplicateNumbersDown()
    {
        if (helper <= 0)
        {
            Debug.LogWarning("Helper is zero, cannot replicate numbers down.");
            return;
        }
        List<int> existingValues = new List<int>();

        // Collect values
        foreach (var tile in _tiles)
        {
            int val = tile.GetValue();
            if (val > 0 && !tile.IsMatched())
            {
                existingValues.Add(val);
            }
        }

        // Fill empty _tiles with copied values
        int index = 0;
        for (int i = 0; i < _tiles.Length; i++)
        {
            if (_tiles[i].IsEmpty() && index < existingValues.Count)
            {
                _tiles[i].SetValue(existingValues[index]);
                index++;
            }
        }
        helper--;
        helperText.text = helper.ToString();
        helperUsed.Invoke();
    }

    /**
     * Checks if a specific row is completely matched.
     * 
     * @param y The index of the row to check.
     * @return True if all tiles in the row are matched, false otherwise.
     */
    public bool IsRowMatched(int y)
    {
        bool hasNonEmptyTile = false;

        for (int x = 0; x < columns; x++)
        {
            var tile = _tiles[GetIndex(x, y)];
            if (!tile.IsEmpty())
            {
                hasNonEmptyTile = true;
            }
            if (!tile.IsMatched() && !tile.IsEmpty()) // || tile.IsEmpty()
                return false;
        }
        return hasNonEmptyTile;
    }

    /**
     * Clears all matched rows in the grid.
     * 
     * This method iterates through each row and clears it if it is completely matched.
     */
    public void ClearAllMatchedRows()
    {
        for (int y = 0; y < rows; y++)
        {
            if (IsRowMatched(y))
            {
                Debug.Log($"Clearing row {y} as it is matched.");
                ClearRow(y);
                
                y--;
            }
        }
    }
    
    bool IsValidMatch(int x1, int y1, int x2, int y2)
    {

        NumTileController tile1 = _tiles[GetIndex(x1, y1)];
        NumTileController tile2 = _tiles[GetIndex(x2, y2)];
        
        int val1 = tile1.GetValue();
        int val2 = tile2.GetValue();

        if (tile1.IsMatched() || tile2.IsMatched()) return false;
        
        if (val1 == 0 || val2 == 0) return false;
        if (!(val1 == val2 || val1 + val2 == 10)) return false;

        int dx = x2 - x1;
        int dy = y2 - y1;

        // Direction must be horizontal, vertical, or diagonal
        // if (!(dx == 0 || dy == 0 || Mathf.Abs(dx) == Mathf.Abs(dy)))
        //     return false;
        if (!(dx == 0 || dy == 0 || Mathf.Abs(dx) == Mathf.Abs(dy)))
        {
            // Reorder coordinates to ensure x1 <= x2 and y1 <= y2
            if (y1 > y2)
            {
                (x1, x2) = (x2, x1);
                (y1, y2) = (y2, y1);
            }
            // Check for a corner case where the match is diagonal but not straight
            for (int i = GetIndex(x1 + 1, y1); i < GetIndex(x2, y2); i++)
            {
                if (!_tiles[i].IsMatched())
                    return false;
            }
        }
        else
        {
            int stepX = dx == 0 ? 0 : dx / Mathf.Abs(dx);
            int stepY = dy == 0 ? 0 : dy / Mathf.Abs(dy);
            int steps = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy));

            // Check all _tiles between (x1, y1) and (x2, y2)
            for (int i = 1; i < steps; i++)
            {
                int ix = x1 + i * stepX;
                int iy = y1 + i * stepY;

                if (!_tiles[GetIndex(ix, iy)].IsMatched())
                    return false;
            }
        }
        // Normalize direction
        
        return true;
    }
    
    public void HighlightAllValidPairs()
    {
        var validPairs = FindAllValidPairs();
        foreach (var (indexA, indexB) in validPairs)
        {
            _tiles[indexA].SetHighlight(true);
            _tiles[indexB].SetHighlight(true);
        }
    }

    public void MatchedTile(NumTileController tile1, NumTileController tile2)
    {
        tile1.MarkMatched();
        tile2.MarkMatched();
    }
    
    public bool AllTilesEmpty()
    {
        return _tiles.All(tile => tile.IsEmpty());
    }

}