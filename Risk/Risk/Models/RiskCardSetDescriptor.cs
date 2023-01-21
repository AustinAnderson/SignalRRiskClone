using System.Collections;

namespace Risk.Models
{
    public class RiskCardSetDescriptor: IEnumerable<int>
    {
        public RiskCardSetDescriptor(int cardId1, int cardId2, int cardId3)
        {
            CardId1 = cardId1;
            CardId2 = cardId2;
            CardId3 = cardId3;
        }
        public int CardId1 { get; }
        public int CardId2 { get; }
        public int CardId3 { get; }
        public int? TerritoryBonusId { get; set; }
        public static bool operator ==(RiskCardSetDescriptor lhs, RiskCardSetDescriptor rhs)
        {
            return lhs.CardId1 == rhs.CardId1 && lhs.CardId2 == rhs.CardId2 && lhs.CardId3 == rhs.CardId3 && 
                   lhs.TerritoryBonusId == rhs.TerritoryBonusId;
        }
        public static bool operator !=(RiskCardSetDescriptor lhs, RiskCardSetDescriptor rhs)
        {
            return !(lhs == rhs);
        }
        public override bool Equals(object? obj)
        {
            return obj is RiskCardSetDescriptor other && this == other;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(
                CardId1.GetHashCode(),
                CardId2.GetHashCode(),
                CardId3.GetHashCode(),
                TerritoryBonusId?.GetHashCode()
            );
        }

        public IEnumerator<int> GetEnumerator()
        {
            yield return CardId1;
            yield return CardId2;
            yield return CardId3;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public override string ToString() => $"({CardId1},{CardId2},{CardId3})";
    }
}
