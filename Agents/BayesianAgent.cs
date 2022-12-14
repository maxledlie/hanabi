using Hanabi;

namespace Agents
{
    public class BayesianAgent : IAgent
    {
        GameView _view;
        const int _numMonteCarloSamples = 100;
        const int _evaluationDepth = 0;

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

        public int UnwinnablePenalty { get; set; } = 20;

        /// <summary>
        /// Represents the agent's current knowledge of the probability distributions of the cards
        /// in its own hand.
        /// </summary>
        public List<OptionTracker> HandOptionTrackers { get; private set; } = new List<OptionTracker>();

        public OptionTracker DeckOptionTracker { get; private set; }

        public BayesianAgent(int playerIndex, GameView gameAdapter)
        {
            PlayerIndex = playerIndex;
            _view = gameAdapter;

            OptionTracker initialOptions = InitialOptions();
            HandOptionTrackers = Enumerable.Range(0, _view.CardsPerPlayer).Select(i => initialOptions.Clone()).ToList();
            DeckOptionTracker = initialOptions.Clone();
        }

        public double Evaluate(Game game, int depth)
        {
            if (depth == 0)
                return EvaluateDepthZero(game);

            return 0;
        }

        public double EvaluateDepthZero(Game game)
        {
            double draftScore = game.Score() + LivesFactor(game.NumLives) + TokensFactor(game.NumTokens);
            return game.IsWinnable() ? draftScore : draftScore - UnwinnablePenalty;
        }

        public void TakeTurn(Randomizer randomizer)
        {
            IEnumerable<string> availableMoves = _view.AvailableMoves();

            double bestExpectedScore = double.NegativeInfinity;
            string? bestMove = null;
            foreach (string move in availableMoves)
            {
                // Generate a random hand from the individual probability distributions
                IList<HiddenState> possibleHiddenStates = DrawHiddenStates(HandOptionTrackers, DeckOptionTracker, _numMonteCarloSamples, randomizer);

                double totalScore = possibleHiddenStates.Sum(hiddenState =>
                {
                    var hand = hiddenState.Hand.Select(c => new Card(c.Item1, c.Item2));

                    Card? nextCard = hiddenState.NextCard == null
                        ? null
                        : new Card(hiddenState.NextCard.Value.Item1, hiddenState.NextCard.Value.Item2);
                    Game resultingState = _view.TestMove(move, hand, nextCard);
                    return Evaluate(resultingState, _evaluationDepth);
                });

                double expectedScore = totalScore / _numMonteCarloSamples;
                if (expectedScore > bestExpectedScore)
                {
                    bestExpectedScore = expectedScore;
                    bestMove = move;
                }
            }

            if (bestMove == null)
                throw new Exception("Failed to select a best move");

            Console.WriteLine($"Player {PlayerIndex}: {bestMove} (expected score {bestExpectedScore})");
            _view.MakeMove(bestMove);
        }

        /// <summary>
        /// Given the possible cards left in each hand position, randomly generates a hand from this distribution.
        /// The distributions for each hand position are not independent: drawing a card from e.g. position 1 removes
        /// it as an option from all subsequent hand positions.
        /// </summary>
        private IList<HiddenState> DrawHiddenStates(IList<OptionTracker> handOptionTrackers, OptionTracker deckOptionTracker, int numDraws, Randomizer randomizer)
        {
            var hiddenStates = new List<HiddenState>();
            for (int i = 0; i < numDraws; i++)
            {
                var drawnHand = new List<(Color, int)>();
                for (int iPos = 0; iPos < handOptionTrackers.Count(); iPos++)
                {
                    var opts = handOptionTrackers[iPos].Clone();
                    foreach ((Color color, int number) in drawnHand)
                        opts.RemoveInstance(color, number);

                    Dictionary<(Color, int), double> probabilities = opts.GetProbabilities();
                    var dist = new DiscreteProbabilityDistribution<(Color, int)>(probabilities, randomizer);
                    drawnHand.Add(dist.GetNext());
                }

                var deckOpts = deckOptionTracker.Clone();
                foreach ((Color color, int number) in drawnHand)
                    deckOpts.RemoveInstance(color, number);

                if (!deckOpts.HasOptions())
                {

                } else
                {
                    Dictionary<(Color, int), double> deckProbabilities = deckOpts.GetProbabilities();
                    var deckDist = new DiscreteProbabilityDistribution<(Color, int)>(deckProbabilities, randomizer);
                    var nextCard = deckDist.GetNext();

                    hiddenStates.Add(new HiddenState
                    {
                        Hand = drawnHand,
                        NextCard = nextCard
                    });
                }
            }

            return hiddenStates;
        }

        private OptionTracker InitialOptions()
        {
            var cardCounts = new Dictionary<(Color, int), int>();
            foreach (Color color in Enum.GetValues(typeof(Color)))
                for (int i = 1; i < 6; i++)
                    cardCounts[(color, i)] = 0;

            for (int iHand = 0; iHand < _view.OtherHands.Count; iHand++)
            {
                List<Card> hand = _view.OtherHands[iHand];
                for (int iCard = 0; iCard < _view.CardsPerPlayer; iCard++)
                {
                    Card card = hand[iCard];
                    var key = (card.Color, card.Number);
                    cardCounts[key]++;
                }
            }

            return new OptionTracker(cardCounts.ToDictionary(
                pair => pair.Key,
                pair => Deck.NumInstances(pair.Key.Item2) - pair.Value));
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

            for (int i = 0; i < _view.CardsPerPlayer; i++)
            {
                if (info.HandPositions.Contains(i))
                {
                    HandOptionTrackers[i].NumberIs(info.Number);
                } else
                {
                    HandOptionTrackers[i].NumberIsNot(info.Number);
                }
            }
        }

        void RespondToTellColor(TellColorInfo info)
        {
            if (info.RecipientIndex != this.PlayerIndex)
                return;

            for (int i = 0; i < _view.CardsPerPlayer; i++)
            {
                if (info.HandPositions.Contains(i))
                {
                    HandOptionTrackers[i].ColorIs(info.Color);
                }
                else
                {
                    HandOptionTrackers[i].ColorIsNot(info.Color);
                }
            }
        }

        void RespondToDiscard(DiscardInfo info)
        {
            if (info.PlayerIndex == this.PlayerIndex)
            {
                // The top card on the discard pile will be the one I just discarded.
                Card discarded = _view.DiscardPile.Last();
                ShiftTrackers(info.HandPosition);

                DeckOptionTracker.RemoveInstance(discarded.Color, discarded.Number);
                foreach (var tracker in HandOptionTrackers)
                    tracker.RemoveInstance(discarded.Color, discarded.Number);
            } else
            {
                int otherPlayerIndex = info.PlayerIndex > this.PlayerIndex ? info.PlayerIndex - 1 : info.PlayerIndex;

                // Make a note of the discarding player's replacement card if they got one
                if (_view.OtherHands[otherPlayerIndex].Count == _view.CardsPerPlayer)
                {
                    Card replacementCard = _view.OtherHands[otherPlayerIndex].Last();
                    DeckOptionTracker.RemoveInstance(replacementCard.Color, replacementCard.Number);
                    foreach (var tracker in HandOptionTrackers)
                        tracker.RemoveInstance(replacementCard.Color, replacementCard.Number);
                }
            }
        }

        void RespondToPlay(PlayCardInfo info)
        {
            if (info.PlayerIndex == this.PlayerIndex)
            {
                ShiftTrackers(info.HandPosition);

                DeckOptionTracker.RemoveInstance(info.CardColor, info.CardNumber);
                foreach (var tracker in HandOptionTrackers)
                    tracker.RemoveInstance(info.CardColor, info.CardNumber);
            } else
            {
                int otherPlayerIndex = info.PlayerIndex > this.PlayerIndex ? info.PlayerIndex - 1 : info.PlayerIndex;

                // Make a note of the playing player's replacement card if they got one
                if (_view.OtherHands[otherPlayerIndex].Count == _view.CardsPerPlayer)
                {
                    Card replacementCard = _view.OtherHands[otherPlayerIndex].Last();
                    DeckOptionTracker.RemoveInstance(replacementCard.Color, replacementCard.Number);
                    foreach (var tracker in HandOptionTrackers)
                        tracker.RemoveInstance(replacementCard.Color, replacementCard.Number);
                }
            }
        }

        /// <summary>
        /// When a card is removed from the hand by discard or play, the option trackers must be shifted to account for this.
        /// Each card to the right of the discarded position is shifted left, and the rightmost tracker contains the updated
        /// knowledge about the deck
        /// </summary>
        void ShiftTrackers(int removedIndex)
        {
            HandOptionTrackers.RemoveAt(removedIndex);

            if (DeckOptionTracker.HasOptions())
                HandOptionTrackers.Add(DeckOptionTracker.Clone());
        }
    }
}
