using Hanabi;

namespace Agents
{
    public class Program
    {
        public static void Main()
        {
            int numPlayers = 4;

            var agents = new List<BayesianAgent>();
            for (int i = 0; i < numPlayers; i++)
                agents.Add(new BayesianAgent(i));

            var game = new Game(numPlayers, Deck.Random());

            while (!game.IsOver)
            {
                agents[game.CurrentPlayer].TakeTurn(game);
            }

            Console.WriteLine($"Game over! Final score: {game.Score()}");
        }
    }
}
