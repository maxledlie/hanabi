namespace Hanabi
{
    public class TellNumberInfo : MoveInfo
    {
        public int PlayerIndex { get; set; }
        public int RecipientIndex { get; set; }
        public int Number { get; set; }
        public List<int> HandPositions { get; set; } = new List<int>();
    }
}
