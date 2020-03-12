using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Popups;
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
        private Sudoku puzzle, puzzleAnswer;
        private Grid grid;

        private const int n = 9;

        public MainPage()
        {
            this.InitializeComponent();
            puzzle = new Sudoku(new int[n, n]);


            makeSudoku();
            makeGrid();
        }

        private void makeSudoku()
        {
            string values = "003020600900305001001806400008102900700000008006708200002609500800203009005010300";
            for (int i = 0; i < values.Length; i++)
                puzzle[i / n, i % n] = values[i] - '0';

            puzzleAnswer = new Sudoku(puzzle);
            puzzleAnswer.solveSudoku();
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
                tb.FontSize = 40;
                tb.TextAlignment = TextAlignment.Center;
                tb.BorderThickness = new Thickness(1);
                tb.BeforeTextChanging += textBox_BeforeTextChanging;
                tb.TextChanging += textBox_TextChanging;

                grid.Children.Add(tb);

                Grid.SetRow(tb, i);
                Grid.SetColumn(tb, j);
            }
        }

        private (int, int) getTextBoxIJ(TextBox tb) => (tb.Name[0] - '0', tb.Name[1] - '0');

        // text changing: clear background
        // only allow a single digit 1-9
        private void textBox_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            sender.Background = null;

            if (args.NewText.Length == 0)
            {
                (int i, int j) = getTextBoxIJ(sender);
                puzzle[i, j] = 0;
                return;
            }

            char digit = args.NewText[0];
            args.Cancel = args.NewText.Length > 1 || digit - '0' < 1 || digit - '0' > 9;
        }

        // update puzzle with textbox value
        private void textBox_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            (int i, int j) = getTextBoxIJ(sender);

            if (int.TryParse(sender.Text, out int value))
                puzzle[i, j] = value;
        }

        private void solveButton_Click(object sender, RoutedEventArgs e)
        {
            for(int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    puzzle[i, j, this] = puzzleAnswer[i, j];
        }

        private async void isCorrectButton_Click(object sender, RoutedEventArgs e)
        {
            string message = puzzle.hasErrors(puzzleAnswer) 
                ? "Incorrect." 
                : puzzle.isAnswer() 
                    ? "Correct!" 
                    : "Correct so far!";

            var messageDialog = new MessageDialog(message);
            await messageDialog.ShowAsync();
        }

        private void ShowErrors_OnClick(object sender, RoutedEventArgs e)
        {
            for(int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                {
                    TextBox tb = (TextBox) grid.FindName($"{i}{j}");

                    tb.Background = puzzle[i, j] != 0 && puzzle[i, j] != puzzleAnswer[i, j]
                        ? new SolidColorBrush(Colors.Red)
                        : null;
                }
                    

        }
    }
}
