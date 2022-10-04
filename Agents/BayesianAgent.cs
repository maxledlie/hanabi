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

        public void RespondToMove(MoveInfo moveInfo)
        {
            switch (moveInfo)
            {
                case TellColorInfo tellColorInfo:
                    RespondToTellColor(tellColorInfo);
                    return;
                case TellNumberInfo tellNumberInfo:
                    RespondToTellNumber(tellNumberInfo);
                    return;
                case DiscardInfo discardInfo:
                    RespondToDiscard(discardInfo);
                    return;
                case PlayCardInfo playInfo:
                    RespondToPlay(playInfo);
                    return;
            }
        }

        void RespondToTellNumber(TellNumberInfo info)
        {
            if (info.RecipientIndex != this.PlayerIndex)
                return;

            for (int i = 0; i < 5; i++)
            {
                if (info.HandPositions.Contains(i))
                {
                    HandProbabilities[i].NumberIs(info.Number);
                } else
                {
                    HandProbabilities[i].NumberIsNot(info.Number);
                }
            }
        }

        void RespondToTellColor(TellColorInfo info)
        {
            if (info.RecipientIndex != this.PlayerIndex)
                return;

            for (int i = 0; i < 5; i++)
            {
                if (info.HandPositions.Contains(i))
                {
                    HandProbabilities[i].ColorIs(info.Color);
                }
                else
                {
                    HandProbabilities[i].ColorIsNot(info.Color);
                }
            }
        }

        void RespondToDiscard(DiscardInfo info)
        {
            if (info.PlayerIndex == this.PlayerIndex)
            {
                // The top card on the discard pile will be the one I just discarded.
                Card discarded = _view.DiscardPile.Last();
                for (int i = 0; i < 5; i++)
                {
                    HandProbabilities[i].RemoveInstance(discarded.Color, discarded.Number);
                }
            } else
            {
                int otherPlayerIndex = info.PlayerIndex > this.PlayerIndex ? info.PlayerIndex - 1 : info.PlayerIndex;

                // Make a note of the discarding player's replacement card if they got one
                if (_view.OtherHands[otherPlayerIndex].Count == 5)
                {
                    Card replacementCard = _view.OtherHands[otherPlayerIndex][4];
                    for (int i = 0; i < 5; i++)
                    {
                        HandProbabilities[i].RemoveInstance(replacementCard.Color, replacementCard.Number);
                    }
                }
            }
        }

        void RespondToPlay(PlayCardInfo info)
        {
            if (info.PlayerIndex == this.PlayerIndex)
            {
                for (int i = 0; i < 5; i++)
                {
                    HandProbabilities[i].RemoveInstance(info.CardColor, info.CardNumber);
                }
            } else
            {
                int otherPlayerIndex = info.PlayerIndex > this.PlayerIndex ? info.PlayerIndex - 1 : info.PlayerIndex;

                // Make a note of the playing player's replacement card if they got one
                if (_view.OtherHands[otherPlayerIndex].Count == 5)
                {
                    Card replacementCard = _view.OtherHands[otherPlayerIndex][4];
                    for (int i = 0; i < 5; i++)
                    {
                        HandProbabilities[i].RemoveInstance(replacementCard.Color, replacementCard.Number);
                    }
                }
            }
        }
    }
}
