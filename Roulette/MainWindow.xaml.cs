using Roulette.Controllers;
using Roulette.Models;
using Roulette.UIElements;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Roulette
{
    /// <summary>
    /// GUI(Graphical User Interface) Layer
    /// Interaction logic for MainWindow.xaml
    /// Takes input from controller and colors winning positions on the board and creates an animation of Popup element for 10 seconds.
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int ZERO = 0;
        private const int POPUP_DELAY = 10;                 //Delay for popup element in seconds
        private const string STRING_EMPTY = "";

        private readonly string soundPath = System.IO.Path.GetFullPath("../../Source/audio/sound.wav").ToString();
        private readonly Brush WINNING_FOREGROUND_BRUSH = Brushes.Black;
        private readonly Brush WINNING_BACKGROUND_BRUSH = Brushes.Beige;
        private readonly Brush BLACK_COLOR = Brushes.Black;
        private readonly Brush RED_COLOR = Brushes.Red;
        private readonly Brush WHITE_COLOR = Brushes.White;
        private readonly Brush GREEN_COLOR = Brushes.Green;

        private List<InputData> _previousWinningNumberList;
        private DispatcherTimer _popupTimer;
        private MediaPlayer _mediaPlayer;
        private RouletteController _controller;
        private Brush _previousBorderColor;
        private Brush _previousTextColor;
        private InputData _winningNumber;

        /// <summary>
        /// Getter for returning previous InputData, if count of the Previous numbers list is bigger than 1, else null
        /// </summary>
        public InputData PreviousNumber => _previousWinningNumberList.Count > 1
            ? _previousWinningNumberList[_previousWinningNumberList.Count - 2]
            : null; 

        public MainWindow()
        {
            _previousWinningNumberList = new List<InputData>();
            _previousBorderColor = null;
            _previousTextColor = null;
            InitializeComponent();

            _mediaPlayer = new MediaPlayer();
            _mediaPlayer.Open(new Uri(soundPath));

            _popupTimer = new DispatcherTimer();
            _popupTimer.Interval = TimeSpan.FromSeconds(POPUP_DELAY);

            _controller = new RouletteController();
            _controller.dataGotten += WaitAndShowWinningPosition;
            _controller.ListenPort();
        }

        /// <summary>
        /// Waiting for a winning number, which is not null, showing all it's winning positions and adding it to queue with previous winning numbers 
        /// </summary>
        public void WaitAndShowWinningPosition(InputData winningNumber)
        {
            _winningNumber = winningNumber;
            if (_winningNumber != null)
            {
                _previousWinningNumberList.Add(_winningNumber);
                if (_previousWinningNumberList.Count == 6) _previousWinningNumberList.RemoveAt(ZERO);
                Dispatcher.Invoke(ShowWinningPositions);
            }
        }

        /// <summary>
        /// Shows all winning positions :
        ///     Winning Number
        ///     Is Even or Odd
        ///     Is 0 to 18 or 19 to 36
        ///     Is black or red
        ///     Which dozen does it belong to
        ///     Which column does it belong to
        /// </summary>
        public void ShowWinningPositions()
        {
            UpdatePreviousWinningNumbers();
            ChangePopupContent();

            if (_previousTextColor != null)
            {
                //Return Previous Colours
                ChangeWinningNumberColors(_previousBorderColor, _previousTextColor, PreviousNumber);
                ChangeWinnningConditionsColors(BLACK_COLOR, WHITE_COLOR, PreviousNumber);
            }
            //Change new winning positions' colors
            if(!_winningNumber.IsZero())
            {
                ChangeWinningNumberColors(WINNING_BACKGROUND_BRUSH, WINNING_FOREGROUND_BRUSH, _winningNumber);
                ChangeWinnningConditionsColors(WINNING_BACKGROUND_BRUSH, WINNING_FOREGROUND_BRUSH, _winningNumber);
            }
            else
            {
                ChangeZeroColors();
            }
            FadeIn();
        }

        private void UpdatePreviousWinningNumbers()
        {
            for (int i = ZERO; i < _previousWinningNumberList.Count; i++)
            {
                TextBlock textBlock = (TextBlock)FindName($"previousWinningTextBlock_{_previousWinningNumberList.Count - i}");
                Ellipse ellipse = (Ellipse)FindName($"previousWinningEllipse_{_previousWinningNumberList.Count - i}");
                bool isZero = _previousWinningNumberList[i].IsZero();
                bool isBlack = _previousWinningNumberList[i].IsBlack();

                textBlock.Text = $"{_previousWinningNumberList[i].InputValue}";
                textBlock.Foreground = (isZero || !isBlack) ? BLACK_COLOR : WHITE_COLOR;
                ellipse.Fill = isZero ? GREEN_COLOR : (isBlack ? BLACK_COLOR : RED_COLOR);
            }
        }

        /// <summary>
        /// Changing the content of Popup element. If number is zero, there is changed only content of winningNumberLabel, other labels are empty 
        /// </summary>
        private void ChangePopupContent()
        {
            winningNumberLabel.Content = _winningNumber.InputValue;
            if (_winningNumber.IsZero())
            {
                isEvenLabel.Content = STRING_EMPTY;
                isBlackLabel.Content = STRING_EMPTY;
                dozenNumberLabel.Content = STRING_EMPTY;
                columnNumberLabel.Content = STRING_EMPTY;
                isSmallLabel.Content = STRING_EMPTY;
                return;
            }
            isEvenLabel.Content = _winningNumber.IsEven() ? "Even" : "Odd";
            isBlackLabel.Content = _winningNumber.IsBlack() ? "Black" : "Red";
            dozenNumberLabel.Content = _winningNumber.DozenNumber() != DozenEnum.WithoutDozen 
                ? $"{(_winningNumber.DozenNumber()).ToString()} Dozen" 
                : STRING_EMPTY;
            columnNumberLabel.Content = _winningNumber.ColumnNumber() != ColumnEnum.WithoutColumn 
                ? $"Column {_winningNumber.ColumnNumber().ToString()}" 
                : STRING_EMPTY;
            isSmallLabel.Content = _winningNumber.IsSmall() ? "1 to 18" : "19 to 36";
        }

        /// <summary>
        /// Changes colors of zero number. 
        /// </summary>
        private void ChangeZeroColors()
        {
            zeroPolygon.Fill = WINNING_BACKGROUND_BRUSH;
            text_0.Foreground = WINNING_FOREGROUND_BRUSH;

            _previousBorderColor = Brushes.LightGreen;
            _previousTextColor = Brushes.Black;
            if (_previousWinningNumberList[_previousWinningNumberList.Count - 1] is null) return;

            ChangeWinnningConditionsColors(BLACK_COLOR, WHITE_COLOR, PreviousNumber);
        }

        /// <summary>
        /// Changes colors of previous or current number's Border and TextBlock
        /// </summary>
        private void ChangeWinningNumberColors(Brush backgroundColor, Brush foregroundColor, InputData data)
        {
            if (!data.IsZero())
            {
                string borderTag = $"Border_{data?.InputValue}";
                string textBlockTag = $"text_{data?.InputValue}";

                Border numberBorder;
                TextBlock numberTextBlock;
                (numberBorder, numberTextBlock) = FindNumberBlockByTag(borderTag);
                _previousBorderColor = numberBorder.Background;
                _previousTextColor = numberTextBlock.Foreground;

                ColorBorder(numberBorder, backgroundColor);
                ColorTextBlock(numberTextBlock, foregroundColor);
            }
            else
            {
                zeroPolygon.Fill = GREEN_COLOR;
                text_0.Foreground = BLACK_COLOR;
            }
        }

        /// <summary>
        /// Changes colors of current or previous winning conditional positions  
        /// </summary>
        private void ChangeWinnningConditionsColors(Brush backgroundColor, Brush foregroundColor, InputData number)
        {
            if (number is null) return;
            if (number.IsZero()) return;

            //Check if is Even
            ColorBorder(number.IsEven() ? isEven : isOdd, backgroundColor);
            ColorTextBlock(number.IsEven() ? isEvenText : isOddText, foregroundColor);

            //Check if is Zero

            //Check if is less than 19
            ColorBorder(number.IsSmall() ? is1To18 : is19To36, backgroundColor);
            ColorTextBlock(number.IsSmall() ? is1To18Text : is19To36Text, foregroundColor);

            //Check if is Black
            ColorBorder(number.IsBlack() ? isBlack : isRed, number.IsBlack() ? backgroundColor : RED_COLOR);

            var columnBorder = (Border)FindName($"Column{number.ColumnNumber()}");
            var columnTextBlock = (TextBlock)FindName($"Column{number.ColumnNumber()}Text");

            //Check ColumnNumber
            ColorBorder(columnBorder, backgroundColor);
            ColorTextBlock(columnTextBlock, foregroundColor);

            var dozenBorder = (Border)FindName($"is{(int)number.DozenNumber()}Dozen");
            var dozenTextBlock = (TextBlock)FindName($"is{(int)number.DozenNumber()}DozenText");

            //Check the dozen number
            ColorBorder(dozenBorder, backgroundColor);
            ColorTextBlock(dozenTextBlock, foregroundColor);
        }

        /// <summary>
        /// Returns Number block (Border + TextBlock) by the number's tag
        /// </summary>
        public (Border, TextBlock) FindNumberBlockByTag(string tag)
        {
            foreach (var item in ItemControlNumbers.Items)
            {
                Border border = FindItemControl(ItemControlNumbers, "numberBorder", item) as Border;
                if (border.Tag.ToString() == tag) return (border, (TextBlock)border.Child);
            }
            return (new Border(), new TextBlock());
        }

        /// <summary>
        /// Returns content from container(ContentPresenter)
        /// </summary>
        private object FindItemControl(ItemsControl itemsControl, string controlName, object item)
        {
            ContentPresenter container = itemsControl.ItemContainerGenerator.ContainerFromItem(item) as ContentPresenter;
            container.ApplyTemplate();
            return container.ContentTemplate.FindName(controlName, container);
        }

        /// <summary>
        /// Colors Border's Foreground into Brush
        /// </summary>
        private void ColorBorder(Border border, Brush brush) => border.Background = brush;

        /// <summary>
        /// Colors TextBlock's Fill into Brush
        /// </summary>
        private void ColorTextBlock(TextBlock text, Brush brush) => text.Foreground = brush;

        /// <summary>
        /// Method for Popup window to fade in with a list of all winning positions for 10 seconds(starts the _timer)
        /// </summary>
        private void FadeIn()
        {
            winningResultsPopup.IsOpen = true;
            _mediaPlayer.Play();
            _popupTimer.Start();
            _popupTimer.Tick += new EventHandler(FadeOut);
        }

        /// <summary>
        /// Method for Popup window to fade out and stop the _timer
        /// </summary>
        public void FadeOut(object sender, EventArgs e)
        {
            winningResultsPopup.IsOpen = false;
            _popupTimer.Stop();
            _mediaPlayer.Stop();
        }

        /// <summary>
        /// Loading generated Number blocks -> Border + TextBlock
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ItemControlNumbers.ItemsSource = NumbersListGenerator.GenerateNumbers();
        }
    }
}
