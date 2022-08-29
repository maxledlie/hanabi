using Hanabi;

namespace Agents
{
    public class ProbabilityDistribution
    {
        Dictionary<(Color, int), double> _vals = new Dictionary<(Color, int), double>();

        public ProbabilityDistribution(Dictionary<(Color, int), double> vals)
        {
            _vals = vals;
        }

        public double Get(Card card)
        {
            var key = (card.Color, card.Number);
            return _vals.ContainsKey(key) ? _vals[key] : 0;
        }
    }
}
