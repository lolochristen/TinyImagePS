namespace TinyImagePS.Models
{
    public class TinifyResponseInput
    {
        public int Size { get; set; }
        public string Type { get; set; }

        public override string ToString()
        {
            return $"{Type} ({Size})";
        }
    }
}