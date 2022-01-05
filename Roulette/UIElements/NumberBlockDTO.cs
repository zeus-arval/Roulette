using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roulette.UIElements
{
    /// <summary>
    /// Class for holding data for TextBlock and Rectangle elements for a certain number
    /// </summary>
    public class NumberBlockDTO
    {
        public int GridColumn { get; set; }
        public int GridRow { get; set; }
        public string RectangleName { get; set; }
        public string TextBlockName { get; set; }
        public string Fill { get; set; }
        public string Foreground { get; set; }
        public string Text { get; set; }
    }
}
