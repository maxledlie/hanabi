using Agents;
using Hanabi;
using HanabiTests;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace AgentTests
{
    internal class GameViewTests
    {
        [Test]
        public void AvailableMoves_StartingPosition()
        {
            var game = new Game(2, GameTests.TestDeck());
            var adapter = new GameView(0, game);

            List<string> availableMoves = adapter.AvailableMoves().ToList();

            Assert.That(availableMoves.Count, Is.EqualTo(16));
            Assert.That(availableMoves.Contains("discard 0"));
            Assert.That(availableMoves.Contains("discard 1"));
            Assert.That(availableMoves.Contains("discard 2"));
            Assert.That(availableMoves.Contains("discard 3"));
            Assert.That(availableMoves.Contains("discard 4"));
            Assert.That(availableMoves.Contains("play 0"));
            Assert.That(availableMoves.Contains("play 1"));
            Assert.That(availableMoves.Contains("play 2"));
            Assert.That(availableMoves.Contains("play 3"));
            Assert.That(availableMoves.Contains("play 4"));
            Assert.That(availableMoves.Contains("tell player 1 about number 2"));
            Assert.That(availableMoves.Contains("tell player 1 about number 4"));
            Assert.That(availableMoves.Contains("tell player 1 about number 5"));
            Assert.That(availableMoves.Contains("tell player 1 about color white"));
            Assert.That(availableMoves.Contains("tell player 1 about color red"));
            Assert.That(availableMoves.Contains("tell player 1 about color yellow"));
        }

        [Test]
        public void AvailableMoves_NoTokens()
        {
            var game = new Game(2, GameTests.TestDeck()) { NumTokens = 0 };
            var view = new GameView(0, game);

            List<string> availableMoves = view.AvailableMoves().ToList();

            Assert.That(availableMoves.Count, Is.EqualTo(10));
            Assert.That(availableMoves.Contains("discard 0"));
            Assert.That(availableMoves.Contains("discard 1"));
            Assert.That(availableMoves.Contains("discard 2"));
            Assert.That(availableMoves.Contains("discard 3"));
            Assert.That(availableMoves.Contains("discard 4"));
            Assert.That(availableMoves.Contains("play 0"));
            Assert.That(availableMoves.Contains("play 1"));
            Assert.That(availableMoves.Contains("play 2"));
            Assert.That(availableMoves.Contains("play 3"));
            Assert.That(availableMoves.Contains("play 4"));
        }
    }
}
