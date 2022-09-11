namespace Hanabi
{
    public class PlayCardInfo : MoveInfo
    {
        public int PlayerIndex { get; set; }
        public Color CardColor { get; set; }
        public int CardNumber { get; set; }
        public bool Successful { get; set; }
    }
}
