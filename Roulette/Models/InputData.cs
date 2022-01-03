using System;

namespace Roulette.Models
{
    /// <summary>
    /// Class for holding value[0, 36] of number and for calculating winning positions.
    /// </summary>
    public class InputData
    {
        /// <summary>
        /// Checks in constructor, if number given is out of range
        /// </summary>
        public string Error { get; private set; } = null;
        /// <summary>
        /// Specifies the Winning number.
        /// </summary>
        public int? InputValue { get; private set; } = null;
        public InputData(int number)
        {
            if (number < 0) Error = "Number must be greater or equal to 0";
            if (number > 37) Error = "Number can't be greater than 36";
            if (Error is null) InputValue = number;
        }
        /// <summary>
        /// Check if number given is even
        /// </summary>
        private bool NumberIsEven(int? number) => number.HasValue ? number % 2 == 0 : false;
        /// <summary>
        /// Checks if winningNumber is zero
        /// </summary>
        public bool IsZero() => InputValue.HasValue ? InputValue == 0 : false;
        /// <summary>
        /// Checks if winningNumber is even
        /// </summary>
        public bool IsEven() => NumberIsEven(InputValue);
        /// <summary>
        /// Checks if winning number is small
        /// </summary>
        public bool IsSmall() => InputValue.HasValue ? InputValue < 19 : false;
        /// <summary>
        /// Calculates DozenEnum for winning number and checks if it is not zero and has value, if not returns WithoutDozen
        /// </summary>
        public DozenEnum DozenNumber() => (InputValue.HasValue && InputValue != 0) ? (DozenEnum)(((InputValue - 1) / 12) + 1) : DozenEnum.WithoutDozen;
        /// <summary>
        /// Calculates ColumnNUmber for winning number and checks if winning number has value and is not zero, if false returns WithoutColumn
        /// </summary>
        public ColumnEnum ColumnNumber() => (InputValue.HasValue && InputValue != 0) ? (ColumnEnum)(InputValue % 3) : ColumnEnum.WithoutColumn;
        /// <summary>
        /// Returns true if winningNumber is black, else false
        /// </summary>
        public bool IsBlack()
        {
            if (InputValue == 10 || InputValue == 28) return true;      // Exceptions
            return NumberIsEven(InputValue % 9)                         // We go throw every nonary of 36 and check if value from nonary
                && InputValue % 9 != 0;                                 // is even AND if input value % 9 is not 0
        }
    }
}
