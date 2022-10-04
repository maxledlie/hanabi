using Agents;
using Hanabi;
using HanabiTests;
using NUnit.Framework;
using System;
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
            List<OptionTracker> probs = agent.HandProbabilities;

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

        [Test]
        public void HandProbabilities_AfterTell_ReflectsNewKnowledge()
        {
            var game = new Game(3, GameTests.TestDeck());
            var view = new GameView(0, game);
            var agent = new BayesianAgent(0, view);

            game.RegisterAgent(0, agent);

            game.TellColor(1, Color.White); // Our agent tells player 1 something, just to progress the game
            game.TellColor(0, Color.Green); // Player 1 tells our agent about their green cards. This should update their knowledge.

            var probs = agent.HandProbabilities;

            // We now know that our zeroth card is NOT green. Since we can see one green in our teammates hands,
            // this removes 9 possibilites, leaving us with 31
            Assert.That(probs[0].Get(Color.Green, 1), Is.Zero);
            Assert.That(probs[0].Get(Color.Red, 1), Is.EqualTo(2.0 / 31));

            // We now know that our first card IS green.
            Assert.That(probs[1].Get(Color.Red, 1), Is.Zero);
            Assert.That(probs[1].Get(Color.Green, 1), Is.EqualTo(2.0 / 9));
            Assert.That(probs[1].Get(Color.Green, 2), Is.EqualTo(2.0 / 9));
            Assert.That(probs[1].Get(Color.Green, 3), Is.EqualTo(2.0 / 9));
            Assert.That(probs[1].Get(Color.Green, 4), Is.EqualTo(2.0 / 9));
            Assert.That(probs[1].Get(Color.Green, 5), Is.EqualTo(1.0 / 9));
        }

        [Test]
        public void HandProbabilities_AfterOwnDiscard_ReflectsNewKnowledge()
        {
            var game = new Game(3, GameTests.TestDeck());
            var view = new GameView(0, game);
            var agent = new BayesianAgent(0, view);

            game.RegisterAgent(0, agent);

            game.Discard(0);

            var probs = agent.HandProbabilities;

            // Our agent has discarded a card and seen that it was a Red 1.
            // The probability of other cards in their hand being Red 1 should have decreased,
            // and the probability of them being any other card slightly increased.
            for (int i = 0; i < 5; i++)
            {
                Assert.That(probs[i].Get(Color.Red, 1), Is.EqualTo(1.0 / 39));
                Assert.That(probs[i].Get(Color.Red, 2), Is.EqualTo(2.0 / 39));
                Assert.That(probs[i].Get(Color.Red, 3), Is.EqualTo(2.0 / 39));
                Assert.That(probs[i].Get(Color.Red, 4), Is.EqualTo(1.0 / 39));
                Assert.That(probs[i].Get(Color.Red, 5), Is.Zero);
            }
        }

        [Test]
        public void HandProbabilities_AfterTeammateDiscard_ReflectsNewKnowledge()
        {
            var game = new Game(3, GameTests.TestDeck());
            var view = new GameView(0, game);
            var agent = new BayesianAgent(0, view);

            game.RegisterAgent(0, agent);

            game.TellNumber(1, 4); // Tell Player 1 about their number 4 just to skip our turn
            game.Discard(2); // Player 1 discards their 4, and it is replaced with the White 1 from the top of the deck.

            var probs = agent.HandProbabilities;

            // Our agent should recognise that Player 1 now has a White 1. Their probability of any card in their hand being
            // a White 1 should be reduced, and the probability of them being any other card slightly increased.
            for (int i = 0; i < 5; i++)
            {
                Assert.That(probs[i].Get(Color.White, 1), Is.EqualTo(2.0 / 39));
                Assert.That(probs[i].Get(Color.White, 2), Is.EqualTo(1.0 / 39));
                Assert.That(probs[i].Get(Color.White, 3), Is.EqualTo(2.0 / 39));
                Assert.That(probs[i].Get(Color.White, 4), Is.EqualTo(1.0 / 39));
                Assert.That(probs[i].Get(Color.White, 5), Is.Zero);

                // They should remember that they saw a Red 4, even though it's now in the discard pile.
                Assert.That(probs[i].Get(Color.Red, 4), Is.EqualTo(1.0 / 39)); 
            }
        }

        [Test]
        public void HandProbabilities_AfterOwnPlay_ReflectsNewKnowledge()
        {
            var game = new Game(3, GameTests.TestDeck());
            var view = new GameView(0, game);
            var agent = new BayesianAgent(0, view);

            game.RegisterAgent(0, agent);

            game.PlayCard(0);

            var probs = agent.HandProbabilities;

            // Our agent has played a card and seen that it was a Red 1.
            // The probability of other cards in their hand being Red 1 should have decreased,
            // and the probability of them being any other card slightly increased.
            for (int i = 0; i < 5; i++)
            {
                Assert.That(probs[i].Get(Color.Red, 1), Is.EqualTo(1.0 / 39));
                Assert.That(probs[i].Get(Color.Red, 2), Is.EqualTo(2.0 / 39));
                Assert.That(probs[i].Get(Color.Red, 3), Is.EqualTo(2.0 / 39));
                Assert.That(probs[i].Get(Color.Red, 4), Is.EqualTo(1.0 / 39));
                Assert.That(probs[i].Get(Color.Red, 5), Is.Zero);
            }
        }

        [Test]
        public void HandProbabilities_AfterTeammatePlay_ReflectsNewKnowledge()
        {
            var game = new Game(3, GameTests.TestDeck());
            var view = new GameView(0, game);
            var agent = new BayesianAgent(0, view);

            game.RegisterAgent(0, agent);

            game.TellNumber(1, 4); // Tell Player 1 about their number 4 just to skip our turn
            game.PlayCard(2); // Player 1 plays their 4 (losing a life), and it is replaced with the White 1 from the top of the deck.

            var probs = agent.HandProbabilities;

            // Our agent should recognise that Player 1 now has a White 1. Their probability of any card in their hand being
            // a White 1 should be reduced, and the probability of them being any other card slightly increased.
            for (int i = 0; i < 5; i++)
            {
                Assert.That(probs[i].Get(Color.White, 1), Is.EqualTo(2.0 / 39));
                Assert.That(probs[i].Get(Color.White, 2), Is.EqualTo(1.0 / 39));
                Assert.That(probs[i].Get(Color.White, 3), Is.EqualTo(2.0 / 39));
                Assert.That(probs[i].Get(Color.White, 4), Is.EqualTo(1.0 / 39));
                Assert.That(probs[i].Get(Color.White, 5), Is.Zero);

                // They should remember that they saw a Red 4, even though it's now in the discard pile.
                Assert.That(probs[i].Get(Color.Red, 4), Is.EqualTo(1.0 / 39));
            }
        }
    }
}