using Hanabi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agents
{
    public class GameRunner
    {
        private Game _game;
        private IList<BayesianAgent> _agents;
        private Randomizer _randomizer;

        public GameRunner(Game game, IList<BayesianAgent> agents, Randomizer randomizer)
        {
            _game = game;
            _agents = agents;
            _randomizer = randomizer;
        }

        public void Run()
        {
            while (!_game.IsOver)
            {
                ReadCommand();
                _agents[_game.CurrentPlayer].TakeTurn(_randomizer);
            }

            Console.WriteLine($"Game over! Final score: {_game.Score()}");
        }

        private void ReadCommand()
        {
            while (true)
            {
                Console.Write("> ");
                var input = Console.ReadLine();
                if (string.IsNullOrEmpty(input))
                    return;

                if (input.StartsWith("options"))
                {
                    string[] tokens = input.Split();
                    int playerIndex = int.Parse(tokens[1]);
                    int optionsIndex = int.Parse(tokens[2]);

                    var agent = _agents[playerIndex];
                    var tracker = optionsIndex >= agent.HandOptionTrackers.Count ? agent.DeckOptionTracker : agent.HandOptionTrackers[optionsIndex];

                    Console.WriteLine(tracker.ToString());
                }
            }

        }
    }
}
