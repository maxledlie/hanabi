namespace Hanabi
{
    public class DiscardInfo : MoveInfo
    {
        public int PlayerIndex { get; set; }
        public Color CardColor { get; set; }
        public int CardNumber { get; set; }
    }
}
