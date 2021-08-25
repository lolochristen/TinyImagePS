namespace TinyImagePS.Models
{
    public class TinifyResponseOutput
    {
        public int Size { get; set; }
        public string Type { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public decimal Ratio { get; set; }
        public string Url { get; set; }

        public override string ToString()
        {
            return $"{Type} {Url} ({Width}x{Height}/{Ratio}, {Size})";
        }
    }
}