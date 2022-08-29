using Agents;
using Hanabi;
using HanabiTests;
using NUnit.Framework;
using System.Collections.Generic;

namespace AgentTests
{
    public class BayesianAgentTests
    {
        [Test]
        public void Evaluate_DepthZero()
        {
            var game = new Game(3, GameTests.TestDeck());
            var view = new GameView(0, game);
            var agent = new BayesianAgent(0, view)
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

        [Test]
        public void HandProbabilities_GameStart_IsBasedOnlyOnTeammatesHands()
        {
            var game = new Game(3, GameTests.TestDeck());
            var view = new GameView(0, game);
            var agent = new BayesianAgent(0, view);

            // There are 50 cards in total, so by default, each card has a (numInstances / 50) chance of
            // being any specific (color, number) pair. However, at the start of the game we can already
            // adjust our probabilities based on our teammates hands.
            List<ProbabilityDistribution> probs = agent.HandProbabilities;

            for (int i = 0; i < 5; i++)
            {
                Assert.That(probs[i].Get(new Card(Color.Red, 1)), Is.EqualTo(2.0 / 40));
                Assert.That(probs[i].Get(new Card(Color.Red, 2)), Is.EqualTo(2.0 / 40));
                Assert.That(probs[i].Get(new Card(Color.Red, 3)), Is.EqualTo(2.0 / 40));
                Assert.That(probs[i].Get(new Card(Color.Red, 4)), Is.EqualTo(1.0 / 40));
                Assert.That(probs[i].Get(new Card(Color.Red, 5)), Is.Zero);
                Assert.That(probs[i].Get(new Card(Color.White, 1)), Is.EqualTo(3.0 / 40));
                Assert.That(probs[i].Get(new Card(Color.White, 2)), Is.EqualTo(1.0 / 40));
                Assert.That(probs[i].Get(new Card(Color.White, 3)), Is.EqualTo(2.0 / 40));
                Assert.That(probs[i].Get(new Card(Color.White, 4)), Is.EqualTo(1.0 / 40));
                Assert.That(probs[i].Get(new Card(Color.White, 5)), Is.Zero);
            }
        }
    }
}