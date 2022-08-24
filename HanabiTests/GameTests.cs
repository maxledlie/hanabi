using Hanabi;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace HanabiTests
{
    public class GameTests
    {
        public static Deck TestDeck() => new Deck(new List<Card>
        {
            // Player 0 hand
            new Card(Color.Red, 1),
            new Card(Color.Green, 3),
            new Card(Color.Blue, 1),
            new Card(Color.Green, 3),
            new Card(Color.Green, 2),

            // Player 1 hand
            new Card(Color.Yellow, 5),
            new Card(Color.Yellow, 2),
            new Card(Color.Red, 4),
            new Card(Color.White, 2),
            new Card(Color.White, 5),

            // Player 3 hand (or first card if 2 player)
            new Card(Color.Red, 5),
            new Card(Color.Red, 1),
            new Card(Color.Blue, 2),
            new Card(Color.Green, 1),
            new Card(Color.White, 4),

            // First card
            new Card(Color.White, 1),
            new Card(Color.Yellow, 2),
            new Card(Color.Blue, 4),
            new Card(Color.Green, 1),
            new Card(Color.White, 2),
            new Card(Color.Blue, 3),
            new Card(Color.Yellow, 1),
            new Card(Color.Yellow, 3),
            new Card(Color.White, 1),
            new Card(Color.Green, 5),
            new Card(Color.Red, 2),
            new Card(Color.Yellow, 4),
            new Card(Color.Blue, 1),
            new Card(Color.Green, 4),
            new Card(Color.White, 3),
            new Card(Color.Blue, 5),
            new Card(Color.Blue, 3),
            new Card(Color.Green, 1),
            new Card(Color.Yellow, 3),
            new Card(Color.Green, 4),
            new Card(Color.Blue, 4),
            new Card(Color.Red, 3),
            new Card(Color.Green, 2),
            new Card(Color.Yellow, 1),
            new Card(Color.Yellow, 1),
            new Card(Color.Yellow, 4),
            new Card(Color.Red, 1),
            new Card(Color.White, 4),
            new Card(Color.Blue, 1),
            new Card(Color.White, 3),
            new Card(Color.Red, 4),
            new Card(Color.Red, 2),
            new Card(Color.White, 1),
            new Card(Color.Blue, 2),
            new Card(Color.Red, 3)
        });

        [Test]
        public void Game_SetUpCorrectly()
        {
            var game = new Game(3, Deck.Random());

            Assert.That(game.NumLives, Is.EqualTo(3));
            Assert.That(game.NumTokens, Is.EqualTo(8));
            Assert.That(game.CurrentPlayer, Is.EqualTo(0));
            Assert.That(game.Players.Count, Is.EqualTo(3));
        }

        [TestCase(2, 5)]
        [TestCase(3, 5)]
        [TestCase(4, 4)]
        [TestCase(5, 4)]
        public void Game_PlayersReceiveCorrectNumberOfCards(int numPlayers, int numCards)
        {
            var game = new Game(numPlayers, Deck.Random());

            for (int i = 0; i < numPlayers; i++)
                Assert.That(game.Players[i].Hand.Count, Is.EqualTo(numCards));
        }

        [Test]
        public void Game_FirstCardsAreDealtToPlayers()
        {
            var deck = Deck.Random();
            var game = new Game(3, deck);

            Assert.That(game.Players[0].Hand.Count, Is.EqualTo(5));
            Assert.That(game.Players[1].Hand.Count, Is.EqualTo(5));
            Assert.That(game.Players[2].Hand.Count, Is.EqualTo(5));
            Assert.That(deck.NumCardsRemaining, Is.EqualTo(35));
        }

        [Test]
        public void MakingMoves_IncrementsCurrentPlayer()
        {
            var game = new Game(3, TestDeck());

            Assert.That(game.CurrentPlayer, Is.EqualTo(0));
            game.TellColor(1, Color.Yellow);
            Assert.That(game.CurrentPlayer, Is.EqualTo(1));
            game.TellNumber(0, 1);
            Assert.That(game.CurrentPlayer, Is.EqualTo(2));
            game.Discard(0);
            Assert.That(game.CurrentPlayer, Is.EqualTo(0));
            game.PlayCard(0);
            Assert.That(game.CurrentPlayer, Is.EqualTo(1));
        }

        [Test]
        public void TellColor_RemovesToken()
        {
            var game = new Game(2, TestDeck());

            game.TellColor(1, Color.Red);
            Assert.That(game.NumTokens, Is.EqualTo(7));
        }

        [Test]
        public void TellColor_TargetPlayerHasNoCardsOfThatColor_BreaksRule()
        {
            var game = new Game(2, TestDeck());

            Assert.Throws(typeof(RuleViolationException), () =>
            {
                game.TellColor(1, Color.Blue);
            });
        }

        [Test]
        public void TellColor_TargetPlayerIsCurrentPlayer_BreaksRule()
        {
            var game = new Game(2, TestDeck());

            Assert.Throws(typeof(RuleViolationException), () =>
            {
                game.TellColor(0, Color.Red);
            });
        }

        [Test]
        public void TellColor_NoTokens_BreaksRule()
        {
            var game = new Game(2, TestDeck());
            game.NumTokens = 0;

            Assert.Throws(typeof(RuleViolationException), () =>
            {
                game.TellColor(1, Color.Red);
            });
        }

        [Test]
        public void TellNumber_RemovesToken()
        {
            var game = new Game(2, TestDeck());

            game.TellNumber(1, 5);
            Assert.That(game.NumTokens, Is.EqualTo(7));
        }

        [Test]
        public void TellNumber_TargetPlayerHasNoCardsOfThatNumber_BreaksRule()
        {
            var game = new Game(2, TestDeck());

            Assert.Throws(typeof(RuleViolationException), () =>
            {
                game.TellNumber(1, 1);
            });
        }

        [Test]
        public void TellNumber_TargetPlayerIsCurrentPlayer_BreaksRule()
        {
            var game = new Game(2, TestDeck());

            Assert.Throws(typeof(RuleViolationException), () =>
            {
                game.TellNumber(0, 5);
            });
        }

        [Test]
        public void TellNumber_NoTokens_BreaksRule()
        {
            var game = new Game(2, TestDeck());
            game.NumTokens = 0;

            Assert.Throws(typeof(RuleViolationException), () =>
            {
                game.TellNumber(1, 5);
            });
        }

        [Test]
        public void Discard_MaximumTokens_DoesNotAddToken()
        {
            var game = new Game(2, TestDeck());

            game.Discard(0);
            Assert.That(game.NumTokens, Is.EqualTo(8));
        }

        [Test]
        public void Discard_LessThanMaximumTokens_AddsToken()
        {
            var game = new Game(2, TestDeck());

            game.TellNumber(1, 5);
            game.TellNumber(0, 1);
            Assert.That(game.NumTokens, Is.EqualTo(6));

            game.Discard(0);
            Assert.That(game.NumTokens, Is.EqualTo(7));
        }

        [Test]
        public void Discard_PlayerReceivesNextCardInDeck()
        {
            var deck = TestDeck();
            var game = new Game(2, deck);
            int numCardsRemainingBeforeDiscard = deck.NumCardsRemaining;

            game.Discard(0);

            Assert.That(game.Players[0].Hand[4].Equals(new Card(Color.Red, 5)));
            Assert.That(deck.NumCardsRemaining, Is.EqualTo(numCardsRemainingBeforeDiscard - 1));
        }

        [Test]
        public void Discard_CardAddedToDiscardPile()
        {
            var game = new Game(2, TestDeck());

            game.Discard(0);
            game.Discard(0);

            Assert.That(game.DiscardPile.Count, Is.EqualTo(2));
            Assert.That(game.DiscardPile[0].Equals(new Card(Color.Red, 1)));
            Assert.That(game.DiscardPile[1].Equals(new Card(Color.Yellow, 5)));
        }

        [Test]
        public void PlayCard_PlayerReceivesNextCardInDeck()
        {
            var deck = TestDeck();
            var game = new Game(2, deck);
            int numCardsRemainingBeforeDiscard = deck.NumCardsRemaining;

            game.PlayCard(0);

            Assert.That(game.Players[0].Hand[4].Equals(new Card(Color.Red, 5)));
            Assert.That(deck.NumCardsRemaining, Is.EqualTo(numCardsRemainingBeforeDiscard - 1));
        }

        [Test]
        public void PlayCard_CorrectPlay_UpdatesStack()
        {
            var game = new Game(2, TestDeck());

            Assert.That(game.Stacks[Color.Red], Is.EqualTo(0));
            game.PlayCard(0); // Player 0 plays [Red 1]
            Assert.That(game.Stacks[Color.Red], Is.EqualTo(1));
        }

        [Test]
        public void PlayCard_IncorrectPlay_LosesALife()
        {
            var game = new Game(2, TestDeck(), numStartingLives: 3);
            game.PlayCard(1); // Player 0 plays [Green 3]

            Assert.That(game.Stacks[Color.Green], Is.EqualTo(0));
            Assert.That(game.NumLives, Is.EqualTo(2));
        }

        [Test]
        public void PlayCard_IncorrectPlayOnLastLife_EndsGame()
        {
            var game = new Game(2, TestDeck(), numStartingLives: 1);
            game.PlayCard(1); // Player 0 plays [Green 3]

            Assert.That(game.IsOver);
        }

        [Test]
        public void PlayCard_IncorrectPlay_CardAddedToDiscardPile()
        {
            var game = new Game(2, TestDeck());
            game.PlayCard(1);

            Assert.That(game.DiscardPile.Count, Is.EqualTo(1));
            Assert.That(game.DiscardPile[0].Equals(new Card(Color.Green, 3)));
        }

        [TestCase(7, 8, Description="Playing a 5 with fewer than max tokens will restore one")]
        [TestCase(8, 8, Description="A token is not restored if already have max")]
        public void PlayCard_StackCompleted_GainsAToken(int numTokensBefore, int numTokensAfter)
        {
            // The deck is set up so that each player repeatedly playing their first card
            // will complete the red stack.
            var deck = new Deck(new List<Card> 
            {
                // Player 0 hand
                new Card(Color.Red, 1),
                new Card(Color.Red, 3),
                new Card(Color.Red, 5),
                new Card(Color.White, 2),
                new Card(Color.White, 2),

                // Player 1 hand
                new Card(Color.Red, 2),
                new Card(Color.Red, 4),
                new Card(Color.White, 1),
                new Card(Color.White, 1),
                new Card(Color.White, 1),

                new Card(Color.White, 3),
                new Card(Color.White, 3),
                new Card(Color.White, 4),
                new Card(Color.White, 4),
                new Card(Color.White, 5),
                new Card(Color.Green, 1)
            });

            var game = new Game(2, deck);
            game.NumTokens = numTokensBefore;
            for (int i = 0; i < 5; i++)
            {
                game.PlayCard(0);
            }

            Assert.That(game.NumTokens, Is.EqualTo(numTokensAfter));
        }

        [Test]
        public void PlayCard_AllStacksCompleted_EndsGame()
        {
            var stacks = new Dictionary<Color, int>();
            stacks[Color.Red] = 5;
            stacks[Color.Green] = 5;
            stacks[Color.Blue] = 5;
            stacks[Color.White] = 5;
            stacks[Color.Yellow] = 4;

            var game = new Game(2, TestDeck())
            {
                Stacks = stacks
            };

            game.Discard(0); // Player 0 discards
            game.PlayCard(0); // Player 1 plays the yellow 5

            Assert.That(game.IsOver);
        }

        [Test]
        public void GameEndsWhenEachPlayerHasHadOneMoreTurnAsfterLastCardWasDrawn()
        {
            var deck = new Deck(new List<Card>
            {
                // Player 0 hand
                new Card(Color.Red, 1),
                new Card(Color.Red, 1),
                new Card(Color.Red, 1),
                new Card(Color.Red, 2),
                new Card(Color.Red, 2),

                // Player 1 hand
                new Card(Color.Red, 3),
                new Card(Color.Red, 3),
                new Card(Color.Red, 4),
                new Card(Color.Red, 4),
                new Card(Color.Red, 5),

                // Player 3 hand
                new Card(Color.Blue, 1),
                new Card(Color.Blue, 1),
                new Card(Color.Blue, 1),
                new Card(Color.Blue, 2),
                new Card(Color.Blue, 2),

                // One remaining card in deck
                new Card(Color.White, 5)
            });

            var game = new Game(3, deck);

            Assert.That(game.IsOver, Is.False);
            game.Discard(0); // Player 0 discards and draws the last card
            Assert.That(game.IsOver, Is.False);
            game.Discard(0); // Player 1 gets their last go
            Assert.That(game.IsOver, Is.False);
            game.Discard(0); // Player 2 gets their last go
            Assert.That(game.IsOver, Is.False);
            game.Discard(0); // Player 0 gets their last go
            Assert.That(game.IsOver);
        }
    }
}
