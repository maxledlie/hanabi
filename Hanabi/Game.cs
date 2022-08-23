namespace Hanabi
{
    public class Game
    {
        public static int MAX_TOKENS = 8;

        int _numPlayers;
        bool _isLastRound;
        int _playerWhoDrewLastCard = -1;

        public int NumLives { get; private set; }
        public int NumTokens { get; set; } = MAX_TOKENS;
        public int CurrentPlayer { get; private set; } = 0;
        public List<Player> Players { get; }
        public Deck Deck { get; }
        public bool IsOver { get; private set; }
        public Dictionary<Color, int> Stacks { get; set; }

        public Game(int numPlayers, Deck deck, int numStartingLives = 3)
        {
            Deck = deck;
            _numPlayers = numPlayers;
            NumLives = numStartingLives;
            Players = new List<Player>();

            int cardsPerPlayer = numPlayers > 3 ? 4 : 5;

            for (int i = 0; i < numPlayers; i++)
            {
                var hand = new List<Card>();
                for (int j = 0; j < cardsPerPlayer; j++)
                {
                    Card? card = deck.DrawCard();
                    if (card != null)
                        hand.Add(card);
                }

                Players.Add(new Player(hand));
            }

            // Initialise stacks
            Stacks = new Dictionary<Color, int>();
            foreach (var color in Enum.GetValues(typeof(Color)))
            {
                Stacks[(Color) color] = 0;
            }
        }

        public void Discard(int positionInHand)
        {
            if (NumTokens < MAX_TOKENS)
                NumTokens++;

            Players[CurrentPlayer].Hand.RemoveAt(positionInHand);

            Card? nextCard = Deck.DrawCard();

            if (nextCard != null)
                Players[CurrentPlayer].Hand.Add(nextCard);

            EndTurn();
        }

        public void TellColor(int player, Color color)
        {
            if (NumTokens <= 0)
                throw new RuleViolationException("At least one token is required to tell anything");

            if (player == CurrentPlayer)
                throw new RuleViolationException("You cannot tell yourself anything");

            Player recipient = Players[player];

            if (!recipient.Hand.Any(card => card.Color == color))
                throw new RuleViolationException("You can only tell a player about a color if they are " +
                    "holding at least one card of that color");

            NumTokens--;
            EndTurn();
        }

        public void TellNumber(int player, int number)
        {
            if (NumTokens <= 0)
                throw new RuleViolationException("At least one token is required to tell anything");

            if (player == CurrentPlayer)
                throw new RuleViolationException("You cannot tell yourself anything");

            Player recipient = Players[player];

            if (!recipient.Hand.Any(card => card.Number == number))
                throw new RuleViolationException("You can only tell a player about a number if they are " +
                    "holding at least one card of that number");

            NumTokens--;
            EndTurn();
        }

        public void PlayCard(int positionInHand)
        {
            Card playedCard = Players[CurrentPlayer].Hand[positionInHand];
            
            if (Stacks[playedCard.Color] == playedCard.Number - 1)
            {
                // Card can be played: add it to the stack
                Stacks[playedCard.Color] = playedCard.Number;

                if (playedCard.Number == 5)
                {
                    if (NumTokens < MAX_TOKENS)
                        NumTokens++;

                    IsOver = Stacks.Values.All(x => x == 5);
                }

                if (playedCard.Number == 5 && NumTokens < MAX_TOKENS)
                    NumTokens++;
            } else
            {
                // Card cannot be played: discard it and lose a life
                NumLives--;
            }

            Players[CurrentPlayer].Hand.RemoveAt(positionInHand);

            Card? nextCard = Deck.DrawCard();

            if (nextCard != null)
                Players[CurrentPlayer].Hand.Add(nextCard);

            EndTurn();
        }

        void EndTurn()
        {
            if (!_isLastRound && Deck.NumCardsRemaining == 0)
            {
                _isLastRound = true;
                _playerWhoDrewLastCard = CurrentPlayer;
            }
            else if (_isLastRound && CurrentPlayer == _playerWhoDrewLastCard)
            {
                IsOver = true;
            }

            CurrentPlayer = (CurrentPlayer + 1) % _numPlayers;
        }
    }
}
