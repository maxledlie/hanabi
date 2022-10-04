namespace Agents
{
    public class DiscreteProbabilityDistribution<T> where T : notnull
    {
        private T[] _items;
        private double[] _lowerLimits;
        private Random _random;
        public DiscreteProbabilityDistribution(Dictionary<T, double> probabilities, int? seed = null)
        {
            _items = probabilities.Keys.ToArray();
            _lowerLimits = new double[_items.Count()];

            for (int i = 1; i < _items.Count(); i++)
            {
                _lowerLimits[i] = _lowerLimits[i - 1] + probabilities[_items[i - 1]];
            }

            _random = seed is not null ? new Random(seed.Value) : new Random();
        }

        public T GetNext()
        {
            double rand = _random.NextDouble();
            for (int iValue = 0; iValue < _items.Length; iValue++)
            {
                double lowerLimit = _lowerLimits[iValue];
                double upperLimit = iValue == _lowerLimits.Length - 1 ? 1 : _lowerLimits[iValue + 1];
                if (rand >= lowerLimit && rand <= upperLimit)
                    return _items[iValue];
            }

            throw new Exception("Implementation error in DiscreteProbabilityDistribution");
        }
    }
}
