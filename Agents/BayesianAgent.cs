using Hanabi;

namespace Agents
{
    public class BayesianAgent : IAgent
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
        public List<OptionTracker> HandProbabilities { get; } = new List<OptionTracker>();

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

        private List<OptionTracker> InitialProbabilities()
        {
            var cardCounts = new Dictionary<(Color, int), int>();
            foreach (Color color in Enum.GetValues(typeof(Color)))
                for (int i = 1; i < 6; i++)
                    cardCounts[(color, i)] = 0;

            for (int iHand = 0; iHand < _view.OtherHands.Count; iHand++)
            {
                List<Card> hand = _view.OtherHands[iHand];
                for (int iCard = 0; iCard < 5; iCard++)
                {
                    Card card = hand[iCard];
                    var key = (card.Color, card.Number);
                    cardCounts[key]++;
                }
            }

            return Enumerable.Range(0, 5).Select(i => new OptionTracker(
                cardCounts.ToDictionary(
                    pair => pair.Key,
                    pair => Deck.NumInstances(pair.Key.Item2) - pair.Value))).ToList();
        }

        public void RespondToMove(string move)
        {
            string[] parts = move.Split(':', StringSplitOptions.RemoveEmptyEntries);
            string playerName = parts[0];
            int playerIndex = int.Parse(playerName.Split()[1]);
            string moveDescription = parts[1];
            string handPositions = parts.Length > 2 ? parts[2] : "";

            string[] moveTokens = moveDescription.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);

            switch (moveTokens[0])
            {
                case "tell":
                    RespondToTell(moveTokens, handPositions);
                    return;
                case "discard":
                    RespondToDiscard(playerIndex, moveTokens);
                    return;
                case "play":
                    RespondToPlay(moveTokens);
                    return;
            }
        }

        void RespondToTell(string[] moveTokens, string handPositionsStr)
        {
            int recipientIndex = int.Parse(moveTokens[2]);
            if (recipientIndex != this.PlayerIndex)
                return;

            var handPositions = handPositionsStr
                .Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.Parse(s));

            if (moveTokens[4] == "color")
            {
                Color color = Enum.Parse<Color>(moveTokens[5], ignoreCase: true);
                for (int i = 0; i < 5; i++)
                {
                    if (handPositions.Contains(i))
                    {
                        HandProbabilities[i].ColorIs(color);
                    } else
                    {
                        HandProbabilities[i].ColorIsNot(color);
                    }
                }

            } else if (moveTokens[4] == "number")
            {
                int number = int.Parse(moveTokens[5]);
                for (int i = 0; i < 5; i++)
                {
                    if (handPositions.Contains(i))
                    {
                        HandProbabilities[i].NumberIs(number);
                    } else
                    {
                        HandProbabilities[i].NumberIsNot(number);
                    }
                }
            }
        }

        void RespondToDiscard(int playerIndex, string[] moveTokens)
        {
            if (playerIndex == this.PlayerIndex)
            {
                // The top card on the discard pile will be the one I just discarded.
                for (int i = 0; i < 5; i++)
                {
                    Card discarded = _view.DiscardPile.Last();
                    HandProbabilities[i].RemoveInstance(discarded.Color, discarded.Number);
                }
            }
        }

        void RespondToPlay(string[] moveTokens)
        {

        }
    }
}
