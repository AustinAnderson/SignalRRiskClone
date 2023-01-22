namespace Risk.Models
{
    public class RiskCard
    {
        public enum Type { Wild, Infantry, Calvary, Cannon }
        public RiskCard(int territoryId, Type cardType)
        {
            TerritoryId = territoryId;
            CardType = cardType;
        }

        public int TerritoryId { get; }
        public Type CardType { get;  }
    }
}
