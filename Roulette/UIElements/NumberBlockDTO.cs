namespace Roulette.UIElements
{
    /// <summary>
    /// Class for holding data for TextBlock and Rectangle elements for a certain number
    /// </summary>
    public class NumberBlockDTO
    {
        public int GridColumn { get; set; }
        public int GridRow { get; set; }
        public string BorderTag { get; set; }
        public string TextBlockName { get; set; }
        public string Background { get; set; }
        public string Foreground { get; set; }
        public string Text { get; set; }
    }
}
