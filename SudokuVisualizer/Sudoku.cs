using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;

namespace SudokuVisualizer
{
    public class Sudoku
    {
        private const int n = 9;
        public int[,] Grid { get; set; }

        public Sudoku(int[,] grid)
        {
            if (grid.GetLength(0) != n || grid.GetLength(1) != n)
                throw new Exception("grid must be 9x9");

            Grid = grid;
        }

        public Sudoku(Sudoku puzzle)
        {
            Grid = (int[,]) puzzle.Grid.Clone();
        }

        public int this[int i, int j, MainPage page=null]
        {
            get => Grid[i, j];
            set
            {
                Grid[i, j] = value;
                TextBox tb = (TextBox) page?.FindName($"{i}{j}");
                if (tb != null)
                {
                    tb.Text = Grid[i, j].ToString();
                }
            } 
        }

    }

    public static class SudokuExtensions
    {
        private const int n = 9;

        public static bool Fill(this Sudoku puzzle, string sudokuString)
        {
            if (sudokuString.Length != 81 || sudokuString.Any(c => !char.IsDigit(c)))
                return false;

            for (int i = 0; i < n*n; i++)
                puzzle[i / n, i % n] = sudokuString[i] - '0';
            return true;
        }

        public static bool isAnswer(this Sudoku s)
        {
            for (int i = 0; i < n; i++)
            {
                // build row and column;
                int[] row = new int[n], col = new int[n];
                for (int j = 0; j < n; j++)
                {
                    row[j] = s.Grid[i, j];
                    col[j] = s.Grid[j, i];
                }

                // verify rows and columns
                if (!verifyRowCol(row) || !verifyRowCol(col))
                    return false;
            }

            // generate boxes
            int[,] box1 = new int[3, 3];
            int[,] box2 = new int[3, 3];
            int[,] box3 = new int[3, 3];
            int[,] box4 = new int[3, 3];
            int[,] box5 = new int[3, 3];
            int[,] box6 = new int[3, 3];
            int[,] box7 = new int[3, 3];
            int[,] box8 = new int[3, 3];
            int[,] box9 = new int[3, 3];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    box1[i, j] = s[i, j];
                    box4[i, j] = s[i + 3, j];
                    box7[i, j] = s[i + 6, j];
                }

                for (int j = 3; j < 6; j++)
                {
                    box2[i, j - 3] = s[i, j];
                    box5[i, j - 3] = s[i + 3, j];
                    box8[i, j - 3] = s[i + 6, j];
                }

                for (int j = 6; j < 9; j++)
                {
                    box3[i, j - 6] = s[i, j];
                    box6[i, j - 6] = s[i + 3, j];
                    box9[i, j - 6] = s[i + 6, j];
                }
            }

            // verify boxes
            List<int[,]> boxes = new List<int[,]>
                { box1, box2, box3, box4, box5, box6, box7, box8, box9 };

            return boxes.All(verifyBox);
        }

        public static bool hasErrors(this Sudoku board, Sudoku answerReference = null)
        {
            if (answerReference == null)
            {
                answerReference = new Sudoku(board);
                answerReference.solveSudoku();
            }

            bool hasErrors = false;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                    if (board[i, j] != 0 && board[i, j] != answerReference[i, j])
                    {
                        hasErrors = true;
                        break;
                    }

                if (hasErrors)
                    break;
            }

            return hasErrors;
        }

        public static bool solveSudoku(this Sudoku board, MainPage page=null)
        {
            int row = -1, col = -1;
            bool isEmpty = true;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (board[i, j] == 0)
                    {
                        row = i;
                        col = j;
                        isEmpty = false;
                        break;
                    }
                }

                if (!isEmpty)
                    break;
            }

            if (isEmpty)
                return true;

            for (int num = 1; num <= n; num++)
            {
                if (isSafe(board, row, col, num))
                {
                    board[row, col, page] = num;
                    if (solveSudoku(board, page))
                        return true;
                    board[row, col] = 0;
                }
            }

            return false;
        }

        private static bool isSafe(Sudoku board, int row, int col, int num)
        {
            for (int d = 0; d < n; d++)
                if (board[row, d] == num)
                    return false;
            for (int r = 0; r < n; r++)
                if (board[r, col] == num)
                    return false;
            int sqrt = 3;
            int boxRowStart = row - row % sqrt;
            int boxColStart = col - col % sqrt;

            for (int r = boxRowStart; r < boxRowStart + sqrt; r++)
                for (int d = boxColStart; d < boxColStart + sqrt; d++)
                    if (board[r, d] == num)
                        return false;
            return true;
        }

        private static HashSet<int> answerArr = new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        private static bool verifyRowCol(int[] r) => r.ToHashSet().SetEquals(answerArr);

        private static bool verifyBox(int[,] box)
        {
            HashSet<int> vals = new HashSet<int>();
            foreach (int i in box)
                vals.Add(i);
            return vals.SetEquals(answerArr);
        }


    }
}
