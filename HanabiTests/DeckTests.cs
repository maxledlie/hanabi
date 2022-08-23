using Hanabi;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace HanabiTests
{
    public class DeckTests
    {
        [Test]
        public void Deck_ContainsCorrectCards()
        {
            var deck = Deck.Random();

            var numberCounts = new Dictionary<int, int>();
            var colorCounts = new Dictionary<Color, int>();

            Card? currentCard = deck.DrawCard();
            while(currentCard != null)
            {
                if (!numberCounts.ContainsKey(currentCard.Number))
                    numberCounts[currentCard.Number] = 1;
                else
                    numberCounts[currentCard.Number]++;

                if (!colorCounts.ContainsKey(currentCard.Color))
                    colorCounts[currentCard.Color] = 1;
                else
                    colorCounts[currentCard.Color]++;

                currentCard = deck.DrawCard();
            }

            Assert.That(numberCounts[1], Is.EqualTo(15));
            Assert.That(numberCounts[2], Is.EqualTo(10));
            Assert.That(numberCounts[3], Is.EqualTo(10));
            Assert.That(numberCounts[4], Is.EqualTo(10));
            Assert.That(numberCounts[5], Is.EqualTo(5));

            foreach (Color color in Enum.GetValues(typeof(Color)))
                Assert.That(colorCounts[color], Is.EqualTo(10));
        }
    }
}