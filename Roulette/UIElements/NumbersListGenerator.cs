using System.Collections.Generic;

namespace Roulette.UIElements
{
    /// <summary>
    /// Class for generating a list of NumberBlocks
    /// </summary>
    public static class NumbersListGenerator
    {

        /// <summary>
        /// Returns a list of NumberBlocks DTOs from 0 to 36 in a specific order, to place them one after another one in grid
        /// </summary>
        public static List<NumberBlockDTO> GenerateNumbers()
        {
            List<NumberBlockDTO> list = new List<NumberBlockDTO>();
            for (int i = 3; i > 0; i--)                                          //Going through rows
            {
                for (int number = i; number < (3 * 12 - 2 + i); number += 3)     //Going through columns
                {
                    list.Add(new NumberBlockDTO()
                    {
                        GridColumn = (number - 1) / 3,
                        GridRow = number % 3,
                        Text = number.ToString(),
                        BorderTag = $"Border_{number}",
                        Background = NumberIsBlack(number) ? "Black" : "Red",       //If Number is Black, it's background color is black
                        Foreground = NumberIsBlack(number) ? "White" : "Black"      //If Number is black, it's text is white
                    });
                }
            }
            return list;
        }

        /// <summary>
        /// Returns true if number is black, if not -> false
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        private static bool NumberIsBlack(int number)
        {
            if (number == 10 || number == 28) return true;
            return (number % 9) % 2 == 0
                && number % 9 != 0;
        }
    }
}
