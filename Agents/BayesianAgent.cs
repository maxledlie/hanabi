using Hanabi;

namespace Agents
{
    public class BayesianAgent
    {
        GameView _view;

        public int PlayerIndex { get; private set; }

        /// <summary>
        /// Given the number of lives remaining in a game, returns the value to add to a depth-zero evaluation
        /// of the game.
        /// </summary>
        public Func<int, double> LivesFactor { get; set; } = n => n;

        /// <summary>
        /// Given the number of tokens remaining in a game, returns the value to add to a depth-zero evaluation
        /// of the game.
        /// </summary>
        public Func<int, double> TokensFactor { get; set; } = n => n;

        /// <summary>
        /// Represents the agent's current knowledge of the probability distributions of the cards
        /// in its own hand.
        /// </summary>
        public List<ProbabilityDistribution> HandProbabilities { get; } = new List<ProbabilityDistribution>();

        public BayesianAgent(int playerIndex, GameView gameAdapter)
        {
            PlayerIndex = playerIndex;
            _view = gameAdapter;

            HandProbabilities = InitialProbabilities();
        }

        public double Evaluate(Game game, int depth)
        {
            if (depth == 0)
                return EvaluateDepthZero(game);

            return 0;
        }

        public double EvaluateDepthZero(Game game)
        {
            if (!game.IsWinnable())
                return double.NegativeInfinity;

            return game.Score() + LivesFactor(game.NumLives) + TokensFactor(game.NumTokens);
        }

        public void TakeTurn()
        {
            var availableMoves = _view.AvailableMoves();

            foreach (var move in availableMoves)
            {
                Game gameAfterMove = _view.TestMove(move);
            }
        }

        private List<ProbabilityDistribution> InitialProbabilities()
        {
            int numUnknownCards = 40;

            var cardCounts = new Dictionary<(Color, int), int>();
            foreach (Color color in Enum.GetValues(typeof(Color)))
                for (int i = 1; i < 6; i++)
                    cardCounts[(color, i)] = 0;

            for (int iPlayer = 0; iPlayer < _view.Players.Count; iPlayer++)
            {
                if (iPlayer == PlayerIndex)
                    continue;

                Player otherPlayer = _view.Players[iPlayer];
                for (int iCard = 0; iCard < 5; iCard++)
                {
                    Card card = otherPlayer.Hand[iCard];
                    var key = (card.Color, card.Number);
                    cardCounts[key]++;
                }
            }

            return Enumerable.Range(0, 5).Select(i => new ProbabilityDistribution(
                cardCounts.ToDictionary(
                    pair => pair.Key,
                    pair => (double)(Deck.NumInstances(pair.Key.Item2) - pair.Value) / numUnknownCards))).ToList();
            
        }
    }
}
