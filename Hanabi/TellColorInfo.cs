namespace Hanabi
{
    public class TellColorInfo : MoveInfo
    {
        public int PlayerIndex { get; set; }
        public int RecipientIndex { get; set; }
        public Color Color { get; set; }
        public List<int> HandPositions { get; set; } = new List<int>();
    }
}
