using Agents;
using Hanabi;
using HanabiTests;
using NUnit.Framework;

namespace AgentTests
{
    public class BayesianAgentTests
    {
        [Test]
        public void Evaluate_DepthZero()
        {
            var game = new Game(3, GameTests.TestDeck());
            var agent = new BayesianAgent(0)
            {
                LivesFactor = (int n) => n,
                TokensFactor = (int n) => 0.5 * n
            };

            Assert.That(agent.Evaluate(game, 0), Is.EqualTo(7));
            game.TellNumber(2, 1); // Player 0 tells player 2 about their two 1s
            Assert.That(agent.Evaluate(game, 0), Is.EqualTo(6.5));
            game.TellColor(0, Color.Blue);
            Assert.That(agent.Evaluate(game, 0), Is.EqualTo(6));
            game.PlayCard(1); // Player 2 plays their [Red 1]
            Assert.That(agent.Evaluate(game, 0), Is.EqualTo(7));
            game.PlayCard(0); // Player 0 plays another [Red 1]. It has nowhere to go so they lose a life
            Assert.That(agent.Evaluate(game, 0), Is.EqualTo(6));
            game.Discard(0); // Player 1 discards their [Yellow 5]. Our agent can see that the game is unwinnable.
            Assert.That(agent.Evaluate(game, 0), Is.LessThan(0));
        }
    }
}