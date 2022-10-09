namespace Hanabi
{
    public class Randomizer
    {
        private Random _random;

        public int Seed { get; }

        public Randomizer(int? seed = null)
        {
            Seed = seed ?? Guid.NewGuid().GetHashCode();
            _random = new Random(Seed);
        }

        public double NextDouble() => _random.NextDouble();
        public int Next() => _random.Next();
        public int Next(int min, int max) => _random.Next(min, max);
    }
}
