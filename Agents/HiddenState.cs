using Hanabi;

namespace Agents
{
    public record HiddenState
    {
        public IList<(Color, int)> Hand { get; init; } = new List<(Color, int)>();
        public (Color, int)? NextCard { get; init; }
    }
}
