using Hanabi;

namespace Agents
{
    public class GameView
    {
        int _playerIndex;
        Game _game;

        public GameView(int playerIndex, Game game)
        {
            _playerIndex = playerIndex;
            _game = game;
        }

        string GetColorName(Color color) => Enum.GetName(typeof(Color), color)?.ToLower() ?? "";

        public IEnumerable<string> AvailableMoves()
        {
            List<Card> hand = _game.PlayerHands[_game.CurrentPlayer];

            IEnumerable<string> playOptions = hand.Select((_, i) => $"discard {i}");
            IEnumerable<string> discardOptions = hand.Select((_, i) => $"play {i}");

            if (_game.NumTokens <= 0)
                return playOptions.Concat(discardOptions);

            IEnumerable<string> tellOptions = _game.PlayerHands.SelectMany((otherHand, iPlayer) =>
            {
                if (iPlayer == _game.CurrentPlayer)
                    return new List<string>();

                var colorOptions = otherHand.Select(card => card.Color)
                    .Distinct()
                    .Select(color => $"tell player {iPlayer} about color {GetColorName(color)}");

                var numberOptions = otherHand.Select(card => card.Number)
                    .Distinct()
                    .Select(number => $"tell player {iPlayer} about number {number}");

                return colorOptions.Concat(numberOptions);
            });

            return playOptions.Concat(discardOptions).Concat(tellOptions);
        }

        public Game TestMove(string move, IEnumerable<(Color, int)> hand)
        {
            Game gameClone = _game.Clone();

            string[] tokens = move.Split();

            if (tokens[0] == "tell")
                return ApplyTell(gameClone, tokens);

            switch (tokens[0])
            {
                case "play":
                    gameClone.PlayCard(int.Parse(tokens[1]));
                    break;
                case "discard":
                    gameClone.Discard(int.Parse(tokens[1]));
                    break;
            }

            return gameClone;
        }

        Game ApplyTell(Game gameClone, string[] moveTokens)
        {
            int playerIndex = int.Parse(moveTokens[2]);

            if (moveTokens[4] == "color")
            {
                Color color = Enum.Parse<Color>(moveTokens[5], ignoreCase: true);
                gameClone.TellColor(playerIndex, color);
                return gameClone;
            } else
            {
                int number = int.Parse(moveTokens[5]);
                gameClone.TellNumber(playerIndex, number);
                return gameClone;
            }
        }

        public int NumPlayers => _game.NumPlayers;
        public int NumTokens => _game.NumTokens;
        public int NumLives => _game.NumLives;
        public List<Card> DiscardPile => _game.DiscardPile;
        public MoveInfo LastMoveInfo => _game.LastMoveInfo;

        public List<List<Card>> OtherHands
        {
            get
            {
                var hands = _game.PlayerHands.ToList();
                hands.RemoveAt(_playerIndex);
                return hands;
            }
        }
    }
}
