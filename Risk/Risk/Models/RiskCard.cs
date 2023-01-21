namespace Risk.Models
{
    public class RiskCard
    {
        public enum Type { Wild, Infantry, Calvary, Cannon }
        public RiskCard(int territoryId, Type cardType, ContinentEnum continent)
        {
            TerritoryId = territoryId;
            CardType = cardType;
            Continent = continent;
        }

        public int TerritoryId { get; }
        public Type CardType { get;  }
        public ContinentEnum Continent { get; }
    }
}
