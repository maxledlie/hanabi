using Hanabi;

namespace Agents
{
    public class RuleBasedAgent
    {
        public int PlayerIndex { get; private set; }

        public RuleBasedAgent(int playerIndex)
        {
            PlayerIndex = playerIndex;
        }

        public void TakeTurn(Game game)
        {
            Console.WriteLine();
            Console.WriteLine("------------------------------");
            Console.WriteLine($"Player {PlayerIndex}'s turn");
            Console.WriteLine("------------------------------");

            Console.WriteLine("I don't know what to do! I'll just discard my left-most card.");
            game.Discard(0);
        }
    }
}