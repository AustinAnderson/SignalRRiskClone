using System.Collections;

namespace Risk.Models
{
    public class RiskCardSet : IEnumerable<RiskCard>
    {
        public RiskCardSet(RiskCard[] cards)
        {
            Card1 = cards[0];
            Card2 = cards[1];
            Card3 = cards[2];
        }
        public RiskCardSet(RiskCard card1, RiskCard card2, RiskCard card3)
        {
            Card1 = card1;
            Card2 = card2;
            Card3 = card3;
        }
        public RiskCard Card1 { get; }
        public RiskCard Card2 { get; }
        public RiskCard Card3 { get; }
        public int? TerritoryBonusId { get; set; }

        public IEnumerator<RiskCard> GetEnumerator()
        {
            yield return Card1;
            yield return Card2;
            yield return Card3;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
