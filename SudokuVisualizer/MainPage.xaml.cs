using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SudokuVisualizer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Sudoku puzzle;
        private Grid grid;

        private const int n = 9;

        public MainPage()
        {
            this.InitializeComponent();
            puzzle = new Sudoku(new int[n, n]);


            makeSudoku();
            makeGrid();
        }

        private void makeGrid()
        {
            grid = this.FindName("sudokuGrid") as Grid;

            if(grid == null)
                throw new Exception("Could not find root grid element");

            for (int k = 0; k < n * n; k++)
            {
                int i = k / n, j = k % n;
                TextBox tb = new TextBox();
                tb.Name = $"{i}{j}";
                tb.Text = puzzle[i, j] == 0 ? "" : $"{puzzle[i, j]}";
                tb.IsReadOnly = true;
                tb.FontSize = 40;
                tb.TextAlignment = TextAlignment.Center;
                tb.BorderThickness = new Thickness(1);
                grid.Children.Add(tb);
                Grid.SetRow(tb, i);
                Grid.SetColumn(tb, j);
            }
        }

        private void makeSudoku()
        {
            string values = "003020600900305001001806400008102900700000008006708200002609500800203009005010300";
            for (int i = 0; i < values.Length; i++)
                puzzle[i / n, i % n] = values[i] - '0';
        }

        private void solveButton_Click(object sender, RoutedEventArgs e)
        {
            puzzle.solveSudoku(this);
        }
    }
}
