namespace Hanabi
{
    public class Player
    {
        public List<Card> Hand { get; private set; }

        public Player(List<Card> hand)
        {
            Hand = hand;
        }
    }
}
