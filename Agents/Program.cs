using Hanabi;

namespace Agents
{
    public class Program
    {
        public static void Main()
        {
            int numPlayers = 4;
            var game = new Game(numPlayers, Deck.Random());

            var agents = new List<BayesianAgent>();
            for (int i = 0; i < numPlayers; i++)
            {
                var adapter = new GameView(i, game);
                agents.Add(new BayesianAgent(i, adapter));
            }

            while (!game.IsOver)
            {
                agents[game.CurrentPlayer].TakeTurn();
            }

            Console.WriteLine($"Game over! Final score: {game.Score()}");
        }
    }
}
