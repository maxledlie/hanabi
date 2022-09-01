using Hanabi;

namespace Agents
{
    public class ProbabilityDistribution
    {
        Dictionary<(Color, int), int> _numLeftByType = new Dictionary<(Color, int), int>();

        public ProbabilityDistribution(Dictionary<(Color, int), int> numLeftByType)
        {
            _numLeftByType = numLeftByType;
        }

        public double Get(Card card) => Get(card.Color, card.Number);

        public double Get(Color color, int number)
        {
            var key = (color, number);
            return _numLeftByType.ContainsKey(key) ? (double)_numLeftByType[key] / NumLeftTotal() : 0;
        }

        public void ColorIs(Color color)
        {
            foreach (Color otherColor in Enum.GetValues<Color>())
            {
                if (otherColor == color)
                    continue;

                for (int i = 1; i < 6; i++)
                {
                    _numLeftByType[(otherColor, i)] = 0;
                }
            }
        }

        public void ColorIsNot(Color color)
        {
            for (int i = 1; i < 6; i++)
            {
                _numLeftByType[(color, i)] = 0;
            }
        }

        public void NumberIs(int number)
        {
            foreach (Color color in Enum.GetValues<Color>())
            {
                for (int otherNumber = 0; otherNumber < 5; otherNumber++)
                {
                    if (otherNumber == number)
                        continue;

                    _numLeftByType[(color, otherNumber)] = 0;
                }
            }
        }

        public void NumberIsNot(int number)
        {
            foreach (Color color in Enum.GetValues<Color>())
            {
                _numLeftByType[(color, number)] = 0;
            }
        }

        public void RemoveInstance(Color color, int number)
        {
            if (_numLeftByType[(color, number)] > 0)
                _numLeftByType[(color, number)]--;
        }

        int NumLeftTotal() => _numLeftByType.Values.Sum();
    }
}
