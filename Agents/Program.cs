using Hanabi;

namespace Agents
{
    public class Program
    {
        public static void Main()
        {
            int numPlayers = 4;

            var agents = new List<BayesianAgent>();
            var game = new Game(numPlayers, Deck.Random());
            for (int i = 0; i < numPlayers; i++)
            {
                var view = new GameView(i, game);
                var agent = new BayesianAgent(i, view);
                game.RegisterAgent(i, agent);
                agents.Add(agent);
            }

            while (!game.IsOver)
            {
                agents[game.CurrentPlayer].TakeTurn();
            }

            Console.WriteLine($"Game over! Final score: {game.Score()}");
        }
    }
}
