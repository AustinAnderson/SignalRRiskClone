namespace Risk.Models
{
    //functions as a node in the risk graph with teritories as verticies and connections that can be attacked as edges
    public class RiskTerritoryState
    {
        public PlayerEnum? Owner { get; set; }
        public int OccupyingArmyCount { get; set; }
        public bool HasSentFortification { get; set; }
        public override string ToString() => $"{Owner} ({OccupyingArmyCount}, {HasSentFortification})";
    }
    public class RiskTerritory
    {
        public int TerritoryID { get; }
        public string TerritoryName { get; }
        public ContinentEnum Continent { get; }
        public int[] AvailableNeighbors;
        public RiskTerritoryState ArmyState { get; }
        public string GetNameRep() => $"{TerritoryID} ({TerritoryName})";

        public RiskTerritory(int territoryID, string territoryName, ContinentEnum continent, int[] availableNeighbors)
        {
            Continent = continent;
            TerritoryID = territoryID;
            TerritoryName = territoryName;
            AvailableNeighbors = availableNeighbors;
            ArmyState = new RiskTerritoryState();
        }
    }
}
