namespace Hanabi
{
    public class Card
    {
        public Color Color { get; }
        public int Number { get; }

        public Card(Color color, int number)
        {
            Color = color;
            Number = number;
        }

        public override string ToString()
        {
            string colorString = Enum.GetName(typeof(Color), Color) ?? "";
            return colorString + " " + Number.ToString();
        }

        public bool Equals(Card other)
        {
            return Color == other.Color && Number == other.Number;
        }
    }
}
