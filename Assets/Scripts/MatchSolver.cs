using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class MatchSolver : MonoBehaviour
{
    private const int Columns = 9;
    private const string InputPath = "input.txt";
    private const string OutputPath = "output.txt";

    void Start()
    {
        string digits = File.ReadAllText(InputPath).Trim();
        int rows = Mathf.CeilToInt(digits.Length / (float)Columns);

        // Convert input to 2D board
        int[,] board = new int[rows, Columns];
        List<Vector2Int> fives = new();

        for (int i = 0; i < digits.Length; i++)
        {
            int r = i / Columns;
            int c = i % Columns;
            board[r, c] = digits[i] - '0';

            if (board[r, c] == 5)
                fives.Add(new Vector2Int(r, c));
        }

        // Find valid 5-5 matches
        List<(Vector2Int, Vector2Int)> validPairs = new();

        for (int i = 0; i < fives.Count; i++)
        {
            for (int j = i + 1; j < fives.Count; j++)
            {
                if (IsValidMatch(board, fives[i], fives[j]))
                    validPairs.Add((fives[i], fives[j]));
            }
        }

        // Generate all combinations of disjoint pairs
        var bestSolutions = new List<List<(Vector2Int, Vector2Int)>>();
        int targetCount = (fives.Count / 2) * 2;

        void DFS(List<(Vector2Int, Vector2Int)> current, HashSet<Vector2Int> used, int index)
        {
            if (current.Count * 2 >= targetCount)
            {
                bestSolutions.Add(new List<(Vector2Int, Vector2Int)>(current));
                return;
            }

            for (int i = index; i < validPairs.Count; i++)
            {
                var (a, b) = validPairs[i];
                if (used.Contains(a) || used.Contains(b)) continue;

                used.Add(a); used.Add(b);
                current.Add((a, b));

                DFS(current, used, i + 1);

                current.RemoveAt(current.Count - 1);
                used.Remove(a); used.Remove(b);
            }
        }

        DFS(new(), new(), 0);

        // Sort by shortest length and take top 10
        var top10 = bestSolutions
            .OrderBy(s => s.Count)
            .Take(10)
            .ToList();

        List<string> lines = new();
        foreach (var solution in top10)
        {
            string line = string.Join("|", solution.Select(pair =>
                $"{pair.Item1.x},{pair.Item1.y},{pair.Item2.x},{pair.Item2.y}"));
            lines.Add(line);
        }

        File.WriteAllLines(OutputPath, lines);
        Debug.Log("✅ Matching complete. Results written to output.txt");
    }

    bool IsValidMatch(int[,] board, Vector2Int a, Vector2Int b)
    {
        int dr = Math.Sign(b.x - a.x);
        int dc = Math.Sign(b.y - a.y);

        if (dr != 0 && dc != 0 && Mathf.Abs(b.x - a.x) != Mathf.Abs(b.y - a.y))
            return false; // must be straight or perfect diagonal

        int steps = Math.Max(Mathf.Abs(b.x - a.x), Mathf.Abs(b.y - a.y));
        for (int i = 1; i < steps; i++)
        {
            int r = a.x + i * dr;
            int c = a.y + i * dc;
            if (board[r, c] != 0) return false;
        }

        return true;
    }
}
