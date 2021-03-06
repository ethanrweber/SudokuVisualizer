﻿using System;
using System.Collections;
using System.Collections.Generic;
using Windows.System;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace SudokuVisualizer
{
    public sealed partial class MainPage : Page
    {

#region global variables

        private Sudoku puzzle, puzzleAnswer;
        private Grid grid;
        private Stack<(int, int, int)> userInputHistory, undoHistory;

        private const int n = 9;
        private const int blankCellNum = 0;

#endregion

#region page initialization and controls

        public MainPage()
        {
            this.InitializeComponent();

            makeGrid();
            makeSudoku(SudokuValues.getRandomSudoku());
        }

        // initialize the grid on the main page with a 9x9 grid of textboxes and set their properties/events
        private void makeGrid()
        {
            grid = this.FindName("sudokuGrid") as Grid;

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    TextBox tb = new TextBox();

                    // textbox properties
                    tb.Name = $"{i}{j}";
                    tb.FontSize = 40;
                    tb.TextAlignment = TextAlignment.Center;
                    double bottomBorder = (i + 1) % 3 == 0 && i < 8 ? 5 : 1,
                           rightBorder  = (j + 1) % 3 == 0 && j < 8 ? 5 : 1;
                    tb.BorderThickness = new Thickness(1, 1, rightBorder, bottomBorder);
                    tb.MaxLength = 1;

                    // textbox events
                    tb.BeforeTextChanging += textBox_BeforeTextChanging;
                    tb.TextChanging += textBox_TextChanging;
                    tb.KeyDown += textbox_MoveCursor;

                    grid.Children.Add(tb);

                    Grid.SetRow(tb, i);
                    Grid.SetColumn(tb, j);
                }
            }
        }

        // given a string of 81 digits, fill in a sudoku puzzle and display it on screen
        // solve it and save the answer in the background to use as a reference
        private void makeSudoku(string values)
        {
            userInputHistory = new Stack<(int, int, int)>();
            undoHistory = new Stack<(int, int, int)>();

            puzzle = new Sudoku(new int[n, n]);

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    int value = values[i * n + j] - '0';
                    updateSudokuText(i, j, value);
                }
            }

            // setting text above triggers 'BeforeTextChanging' method
            userInputHistory.Clear();
            
            puzzleAnswer = new Sudoku(puzzle);
            puzzleAnswer.solveSudoku();
        }

        private void updateSudokuText(int i, int j, int newValue)
        {
            if (i < 0 || i > n || j < 0 || j > n || newValue < 0 || newValue > n)
                throw new Exception("Invalid value");

            puzzle[i, j] = newValue;

            TextBox tb = getTextBoxFromIJ(i, j);
            tb.Text = newValue > 0 ? newValue.ToString() : "";
        }

#endregion

#region textbox controls

        // gets the row / column of a textbox in the main grid and returns them as a tuple
        private (int, int) getIJFromTextBox(TextBox tb) => (tb.Name[0] - '0', tb.Name[1] - '0');
        private TextBox getTextBoxFromIJ(int i, int j) => (TextBox) grid.FindName($"{i}{j}");

        // text changing: clear background
        // only allow a single digit 1-9
        private void textBox_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            sender.Background = null;

            (int i, int j) = getIJFromTextBox(sender);

            if (args.NewText.Length == 0)
            {
                userInputHistory.Push((i, j, blankCellNum));
                puzzle[i, j] = blankCellNum;
            }
            else
            {
                int digit = args.NewText[0] - '0';
                args.Cancel = digit < 1 || digit > n;
            }
        }

        // update puzzle with textbox value
        private void textBox_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            (int i, int j) = getIJFromTextBox(sender);

            if (int.TryParse(sender.Text, out int newValue))
            {
                userInputHistory.Push((i, j, puzzle[i, j]));
                puzzle[i, j] = newValue;
            }
        }

        // move the cursor to a neighboring cell when pressing arrow keys or wasd
        private void textbox_MoveCursor(object sender, RoutedEventArgs e)
        {
            var keyEvent = e as KeyRoutedEventArgs;
            (int i, int j) = getIJFromTextBox((TextBox) sender);

            int dRow = 0, dCol = 0;
            switch (keyEvent?.Key)
            {
                case VirtualKey.W:
                case VirtualKey.Up:
                    dRow--;
                    break;
                case VirtualKey.S:
                case VirtualKey.Down:
                    dRow++;
                    break;
                case VirtualKey.D:
                case VirtualKey.Right:
                    dCol++;
                    break;
                case VirtualKey.A:
                case VirtualKey.Left:
                    dCol--;
                    break;
                default:
                    return;
            }

            // mod operation allows wrapping around screen
            i = (i + dRow + n) % n;
            j = (j + dCol + n) % n;

            TextBox nextTextBox = (TextBox) grid.FindName($"{i}{j}");
            if (nextTextBox != null)
            {
                nextTextBox.Focus(FocusState.Programmatic);
                nextTextBox.SelectAll();
            }
        }

#endregion

#region button controls

        // fill in all values of the puzzle with the answer
        private void solveButton_Click(object sender, RoutedEventArgs e)
        {
            for(int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    updateSudokuText(i, j, puzzleAnswer[i, j]);
        }

        // validate the puzzle
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

        // highlight all incorrect cells red on screen
        private void ShowErrors_OnClick(object sender, RoutedEventArgs e)
        {
            for(int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                {
                    TextBox tb = getTextBoxFromIJ(i, j);
                    tb.Background = puzzle[i, j] != blankCellNum && puzzle[i, j] != puzzleAnswer[i, j]
                        ? new SolidColorBrush(Colors.Red)
                        : null;
                }
        }

        // fill in a random, unfilled cell with its correct value
        private void hintButton_Click(object sender, RoutedEventArgs e)
        {
            bool filled = false;
            while (!filled && !puzzle.isAnswer())
            {
                var rand = new Random();
                int i = rand.Next(9), j = rand.Next(9);
                if (puzzle[i, j] != puzzleAnswer[i, j])
                {
                    updateSudokuText(i, j, puzzleAnswer[i, j]);
                    filled = true;
                }
            }
        }

        // replace the current puzzle with a new one from the stored values
        // prompts the user to confirm before replacing puzzle
        private async void newPuzzleButton_Click(object sender, RoutedEventArgs e)
        {
            var messageDialog = new MessageDialog("Are you sure? This will overwrite your current progress", "Create New Puzzle");

            messageDialog.Commands.Add(new UICommand("Yes") { Id = 0 });
            messageDialog.Commands.Add(new UICommand("No") { Id = 1 });

            var result = await messageDialog.ShowAsync();

            if((int) result.Id == 0)
                makeSudoku(SudokuValues.getRandomSudoku());
        }

        private void UndoButton_OnClickButton_Click(object sender, RoutedEventArgs e)
        {
            if(userInputHistory.TryPop(out (int, int, int) result))
            {
                (int i, int j, int previousValue) = result;

                // add current value to 'undo' stack
                int currentValue = puzzle[i, j];
                undoHistory.Push((i, j, currentValue));

                // replace current value with previous value
                updateSudokuText(i, j, previousValue);
            }
        }

        private void RedoButton_OnClickButton_Click(object sender, RoutedEventArgs e)
        {
            if (undoHistory.TryPop(out (int, int, int) result))
            {
                (int i, int j, int previouslyReplacedValue) = result;

                // textbox 'BeforeTextChanging' event already pushes value to user history stack

                // replace current value with previous value
                updateSudokuText(i, j, previouslyReplacedValue);
            }
        }

#endregion

    }
}
