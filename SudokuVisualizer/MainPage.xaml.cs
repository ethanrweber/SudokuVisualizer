using System;
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
        private Sudoku puzzle, puzzleAnswer;
        private Grid grid;

        private const int n = 9;

        /****************************************************
         * Page Initialization
         ****************************************************/

        public MainPage()
        {
            this.InitializeComponent();

            makeGrid();
            makeSudoku(SudokuValues.getRandomSudoku());
        }

        private void makeGrid()
        {
            grid = this.FindName("sudokuGrid") as Grid;

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    TextBox tb = new TextBox();
                    double bottomBorder = (i + 1) % 3 == 0 && i < 8 ? 5 : 1;
                    double rightBorder = (j + 1) % 3 == 0 && j < 8 ? 5 : 1;

                    // textbox properties
                    tb.Name = $"{i}{j}";
                    tb.FontSize = 40;
                    tb.TextAlignment = TextAlignment.Center;
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
            puzzle = new Sudoku(new int[n, n]);
            puzzle.Fill(values);

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    TextBox tb = (TextBox) grid.FindName($"{i}{j}");
                    tb.Text = puzzle[i, j] != 0 ? puzzle[i, j].ToString() : "";
                }
            }

            puzzleAnswer = new Sudoku(puzzle);
            puzzleAnswer.solveSudoku();
        }


        /****************************************************
         * TextBox Controls
         ****************************************************/

        // gets the row / column of a textbox in the main grid and returns them as a tuple
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
            args.Cancel = digit - '0' < 1 || digit - '0' > 9;
        }

        // update puzzle with textbox value
        private void textBox_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            (int i, int j) = getTextBoxIJ(sender);

            if (int.TryParse(sender.Text, out int value))
                puzzle[i, j] = value;
        }

        // move the cursor to a neighboring cell when pressing arrow keys or wasd
        private void textbox_MoveCursor(object sender, RoutedEventArgs e)
        {
            var keyEvent = e as KeyRoutedEventArgs;
            (int i, int j) = getTextBoxIJ((TextBox) sender);


            int dCol = 0, dRow = 0;
            switch (keyEvent?.Key)
            {
                case VirtualKey.W:
                case VirtualKey.Up:
                    dCol--;
                    break;
                case VirtualKey.S:
                case VirtualKey.Down:
                    dCol++;
                    break;
                case VirtualKey.D:
                case VirtualKey.Right:
                    dRow++;
                    break;
                case VirtualKey.A:
                case VirtualKey.Left:
                    dRow--;
                    break;
                default:
                    return;
            }

            i = (i + dCol + n) % n;
            j = (j + dRow + n) % n;

            TextBox nextTextBox = (TextBox) grid.FindName($"{i}{j}");
            if (nextTextBox != null)
            {
                nextTextBox.Focus(FocusState.Programmatic);
                nextTextBox.SelectAll();
            }
        }


        /****************************************************
         * Button Controls
         ****************************************************/

        // fill in all values of the puzzle with the answer
        private void solveButton_Click(object sender, RoutedEventArgs e)
        {
            for(int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    puzzle[i, j, this] = puzzleAnswer[i, j];
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
                    TextBox tb = (TextBox) grid.FindName($"{i}{j}");
                    tb.Background = puzzle[i, j] != 0 && puzzle[i, j] != puzzleAnswer[i, j]
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
                    puzzle[i, j, this] = puzzleAnswer[i, j];
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

    }
}
