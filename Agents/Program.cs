using Hanabi;
using CommandLine;

namespace Agents
{
    public class Options
    {
        [Option('s', "seed", Required = false, HelpText = "You can provide a random seed for reproducible RNG")]
        public int? Seed { get; set; } = null;
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(Run)
                .WithNotParsed(HandleParseError);
        }

        public static void Run(Options options)
        {
            var randomizer = new Randomizer(options.Seed);
            Console.WriteLine($"Random seed: {randomizer.Seed}");

            int numPlayers = 4;

            var agents = new List<BayesianAgent>();
            var game = new Game(numPlayers, Deck.Random(randomizer));
            for (int i = 0; i < numPlayers; i++)
            {
                var view = new GameView(i, game);
                var agent = new BayesianAgent(i, view);
                game.RegisterAgent(i, agent);
                agents.Add(agent);
            }

            while (!game.IsOver)
            {
                agents[game.CurrentPlayer].TakeTurn(randomizer);
            }

            Console.WriteLine($"Game over! Final score: {game.Score()}");
        }

        public static void HandleParseError(IEnumerable<Error> errors)
        {
            foreach (var err in errors)
            {
                Console.WriteLine(err);
            }
        }
    }
}
