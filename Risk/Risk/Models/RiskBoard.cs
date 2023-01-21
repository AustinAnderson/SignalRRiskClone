using System.Collections;
using System.Collections.Generic;

namespace Risk.Models
{
    public class RiskBoard: IEnumerable<RiskTerritory>
    {
        /// <summary>
        /// indexed by territory id
        /// </summary>
        private List<RiskTerritory> territories;
        public RiskTerritory this[int i]
        {
            get { 
                AssertTerritoryExists(i);
                return territories[i]; 
            }
            set { 
                AssertTerritoryExists(i);
                territories[i] = value; 
            }
        }
        private void AssertTerritoryExists(int territoryId)
        {
            if(territoryId < 0 || territoryId >= territories.Count)
            {
                throw new ArgumentException($"no such territory {territoryId}");
            }
        }

        public static IReadOnlyDictionary<ContinentEnum, int> ContinentCounts => continentCounts;
        private static Dictionary<ContinentEnum, int> continentCounts = new RiskBoard().territories.GroupBy(t => t.Continent)
            .ToDictionary(x => x.Key, x => x.Count());

        public RiskBoard()
        {
            territories = new List<RiskTerritory>
            {
                new(0, "Alaska", ContinentEnum.NorthAmerica, new[]{ 1, 3, 33}),
                new(1, "Northwest Territory", ContinentEnum.NorthAmerica, new[]{ 0, 2, 3, 4}),
                new(2, "Greenland", ContinentEnum.NorthAmerica, new[] { 1, 4, 5, 19}),
                new(3, "Alberta", ContinentEnum.NorthAmerica, new[]{ 0, 1, 4, 6}),
                new(4, "Ontario", ContinentEnum.NorthAmerica, new[]{ 1, 2, 3, 5, 6, 7}),
                new(5, "Quebec", ContinentEnum.NorthAmerica, new[]{ 2, 4, 7}),
                new(6, "Western United States", ContinentEnum.NorthAmerica, new[]{ 3, 4, 7, 8}),
                new(7, "Eastern United States", ContinentEnum.NorthAmerica, new[]{ 4, 5, 6, 8}),
                new(8, "Central America", ContinentEnum.NorthAmerica, new[]{ 6, 7, 9}),

                new(9, "Venezuela", ContinentEnum.SouthAmerica, new[]{ 8, 10, 11}),
                new(10, "Peru", ContinentEnum.SouthAmerica, new[]{ 9, 11, 12}),
                new(11, "Brazil", ContinentEnum.SouthAmerica, new[]{ 9, 10, 12, 13}),
                new(12, "Argentina", ContinentEnum.SouthAmerica, new[]{ 10, 11}),

                new(13, "North Africa", ContinentEnum.Africa, new[]{ 11, 25, 24, 14, 15, 16}),
                new(14, "Egypt", ContinentEnum.Africa, new[]{ 13, 24, 15, 38}),
                new(15, "East Africa", ContinentEnum.Africa, new[]{ 14, 38, 13, 16, 17, 18}),
                new(16, "Congo", ContinentEnum.Africa, new[]{ 13, 15, 17}),
                new(17, "South Africa", ContinentEnum.Africa, new[]{ 16, 15, 18}),
                new(18, "Madagascar", ContinentEnum.Africa, new[]{ 17, 15}),

                new(19, "Iceland", ContinentEnum.Europe, new[]{ 2, 22, 20}),
                new(20, "Scandinavia", ContinentEnum.Europe, new[]{ 22, 23, 21}),
                new(21, "Slavic", ContinentEnum.Europe, new[]{ 20, 23, 24, 38, 34, 30}),
                new(22, "Great Britian", ContinentEnum.Europe, new[]{ 19, 20, 23, 25}),
                new(23, "Northern Europe", ContinentEnum.Europe, new[]{ 22, 20, 21, 24, 25}),
                new(24, "Southern Europe", ContinentEnum.Europe, new[]{ 25, 23, 21, 38, 14}),
                new(25, "Western Europe", ContinentEnum.Europe, new[]{ 22, 23, 24, 13}),

                new(26, "Indonesia", ContinentEnum.Australia, new[]{ 41, 27, 28}),
                new(27, "New Guinea", ContinentEnum.Australia, new[]{ 26, 29}),
                new(28, "Western Australia", ContinentEnum.Australia, new[]{ 26, 29}),
                new(29, "Eastern Australia", ContinentEnum.Australia, new[]{ 28, 27}),

                new(30, "Ural", ContinentEnum.Asia, new[]{ 21, 34, 31, 40}),
                new(31, "Siberia", ContinentEnum.Asia, new[]{ 30, 40, 36, 35, 32}),
                new(32, "Yakutsk", ContinentEnum.Asia, new[]{ 31, 35, 33}),
                new(33, "Kamchatka", ContinentEnum.Asia, new[]{ 32, 35, 36, 37, 0}),
                new(34, "Afghanistan", ContinentEnum.Asia, new[]{ 21, 38, 39, 40, 30}),
                new(35, "Irkutsk", ContinentEnum.Asia, new[]{ 31, 36, 33, 32}),
                new(36, "Mongolia", ContinentEnum.Asia, new[]{ 31, 40, 35, 33, 37}),
                new(37, "Japan", ContinentEnum.Asia, new[]{ 36, 33}),
                new(38, "Middle East", ContinentEnum.Asia, new[]{ 39, 34, 21, 24, 14, 15}),
                new(39, "India", ContinentEnum.Asia, new[]{ 38, 34, 40, 41}),
                new(40, "China", ContinentEnum.Asia, new[]{ 41, 39, 34, 30, 31, 36}),
                new(41, "Southeast Asia", ContinentEnum.Asia, new[]{ 39, 40, 26})
            };
        }
        public RiskBoard(Dictionary<int,RiskTerritoryState> state) : this()
        {
            var providedIds = string.Join(",", state.Keys.Select(x => "" + x).OrderBy(x => x));
            if(providedIds != string.Join(",", territories.Select(x => "" + x.TerritoryID).OrderBy(x => x)) )
            {
                throw new ArgumentException($"Invalid state rep, ids do not match map, got [{providedIds}]");
            }
            foreach(var kvp in state)
            {
                this[kvp.Key].ArmyState.OccupyingArmyCount = kvp.Value.OccupyingArmyCount;
                this[kvp.Key].ArmyState.Owner = kvp.Value.Owner;
            }
        }
        public void ClaimTerritory(PlayerState player, int territoryId)
        {
            AssertTerritoryExists(territoryId);
            if(player.AllocableArmies <= 0)
            {
                throw new ArgumentException($"{player.PlayerColor} player has no armies to claim territories with");
            }
            if(territories[territoryId].ArmyState.Owner != null &&
               territories[territoryId].ArmyState.Owner != player.PlayerColor)
            {
                throw new ArgumentException($"{player.PlayerColor} player cannot claim territory owned by {player.PlayerColor} player");
            }
            territories[territoryId].ArmyState.Owner = player.PlayerColor;
            territories[territoryId].ArmyState.OccupyingArmyCount++;
            player.AllocableArmies--;
        }
        public void AddArmies(PlayerState player, int territoryId, int armyCount)
        {
            AssertTerritoryExists(territoryId);
            if(territories[territoryId].ArmyState.Owner!= player.PlayerColor)
            {
                throw new ArgumentException($"{player.PlayerColor} player doesn't own territory {territoryId} ({territories[territoryId].TerritoryName})");
            }
            if(armyCount > player.AllocableArmies)
            {
                throw new ArgumentException($"{player.PlayerColor} player doesn't have {armyCount} armies to allocate ({player.AllocableArmies} available)");
            }
            player.AllocableArmies -= armyCount;
            territories[territoryId].ArmyState.OccupyingArmyCount = armyCount;
        }

        public IEnumerator<RiskTerritory> GetEnumerator() => territories.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
