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
            Player player = _game.Players[_game.CurrentPlayer];

            IEnumerable<string> playOptions = player.Hand.Select((_, i) => $"discard {i}");
            IEnumerable<string> discardOptions = player.Hand.Select((_, i) => $"play {i}");

            if (_game.NumTokens <= 0)
                return playOptions.Concat(discardOptions);

            IEnumerable<string> tellOptions = _game.Players.SelectMany((otherPlayer, iPlayer) =>
            {
                if (iPlayer == _game.CurrentPlayer)
                    return new List<string>();

                var colorOptions = otherPlayer.Hand.Select(card => card.Color)
                    .Distinct()
                    .Select(color => $"tell player {iPlayer} about color {GetColorName(color)}");

                var numberOptions = otherPlayer.Hand.Select(card => card.Number)
                    .Distinct()
                    .Select(number => $"tell player {iPlayer} about number {number}");

                return colorOptions.Concat(numberOptions);
            });

            return playOptions.Concat(discardOptions).Concat(tellOptions);
        }

        public Game TestMove(string move)
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
                Color color = (Color)Enum.Parse(typeof(Color), moveTokens[5]);
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
        public List<Player> Players => _game.Players;
    }
}
