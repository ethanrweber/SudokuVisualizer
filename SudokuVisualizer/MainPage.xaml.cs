using System;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

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
                    tb.Name = $"{i}{j}";
                    tb.FontSize = 40;
                    tb.TextAlignment = TextAlignment.Center;

                    double bottomBorder = (i + 1) % 3 == 0 && i < 8 ? 5 : 1;
                    double rightBorder = (j + 1) % 3 == 0 && j < 8 ? 5 : 1;
                    tb.BorderThickness = new Thickness(1, 1, rightBorder, bottomBorder);

                    tb.BeforeTextChanging += textBox_BeforeTextChanging;
                    tb.TextChanging += textBox_TextChanging;

                    grid.Children.Add(tb);

                    Grid.SetRow(tb, i);
                    Grid.SetColumn(tb, j);
                }
            }
        }

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
