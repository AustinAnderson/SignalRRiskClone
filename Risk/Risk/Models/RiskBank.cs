namespace Risk.Models
{
    public class RiskBank
    {
        private List<RiskCard> cards = new List<RiskCard>();
        public RiskBank(List<RiskCard> cards)
        {
            this.cards = cards;
        }
        public RiskBank()
        {
            var horse = RiskCard.Type.Calvary;
            var man = RiskCard.Type.Infantry;
            var gun = RiskCard.Type.Cannon;
            cards = new List<RiskCard>
            {
#region deck composition
                new(34, horse, ContinentEnum.Asia),
                new(0, man, ContinentEnum.NorthAmerica),
                new(3, horse, ContinentEnum.NorthAmerica),
                new(12, man, ContinentEnum.SouthAmerica),
                new(11, gun, ContinentEnum.SouthAmerica),
                new(16, man, ContinentEnum.Africa),
                new(8, gun, ContinentEnum.NorthAmerica),
                new(40, man, ContinentEnum.Asia),
                new(15, man, ContinentEnum.Africa),
                new(29, gun, ContinentEnum.Australia),
                new(5, horse, ContinentEnum.NorthAmerica),
                new(7, gun, ContinentEnum.NorthAmerica),
                new(14, man, ContinentEnum.Africa),
                new(22, gun, ContinentEnum.Europe),
                new(2, horse, ContinentEnum.NorthAmerica),
                new(19, man, ContinentEnum.Europe),
                new(39, horse, ContinentEnum.Asia),
                new(26, gun, ContinentEnum.Australia),
                new(35, horse, ContinentEnum.Asia),
                new(37, gun, ContinentEnum.Asia),
                new(33, man, ContinentEnum.Asia),
                new(18, horse, ContinentEnum.Africa),
                new(38, man, ContinentEnum.Asia),
                new(36, man, ContinentEnum.Asia),
                new(27, man, ContinentEnum.Australia),
                new(13, horse, ContinentEnum.Africa),
                new(23, gun, ContinentEnum.Europe),
                new(1, gun, ContinentEnum.NorthAmerica),
                new(4, horse, ContinentEnum.NorthAmerica),
                new(10, man, ContinentEnum.Africa),
                new(21, horse, ContinentEnum.Europe),
                new(20, horse, ContinentEnum.Europe),
                new(31, horse, ContinentEnum.Asia),
                new(17, gun, ContinentEnum.Africa),
                new(41, man, ContinentEnum.Asia),
                new(24, gun, ContinentEnum.Europe),
                new(30, horse, ContinentEnum.Asia),
                new(9, man, ContinentEnum.Africa),
                new(28, gun, ContinentEnum.Australia),
                new(25, gun, ContinentEnum.Europe),
                new(6, gun, ContinentEnum.NorthAmerica),
                new(32, horse, ContinentEnum.Asia),
                new(-1,RiskCard.Type.Wild, ContinentEnum.Asia),
                new(-2,RiskCard.Type.Wild, ContinentEnum.Asia)
#endregion
            };
        }
        private Random random = new Random((int)DateTime.Now.Ticks);
        public int Payout { get; private set; } = 4;
        private int increaseFactor = 2;
        public int TradeInCards(RiskCardSet cardSet)
        {
            cards.Add(cardSet.Card1);
            cards.Add(cardSet.Card2);
            cards.Add(cardSet.Card3);
            int amount = Payout;
            Payout = Payout + increaseFactor;
            if (Payout == 12) increaseFactor = 3;
            if (Payout == 15) increaseFactor = 5;
            return amount;
        }
        public int GetAllocableArmies(RiskBoard board, PlayerEnum player)
        {
            var idToCountMap=board.Where(t=>t.ArmyState.Owner == player).ToDictionary(x=>x.TerritoryID, x=>x.ArmyState.OccupyingArmyCount);
            int count = Math.Max(idToCountMap.Keys.Count / 3, 3);
            var countsPerContinent = ContinentEnum.Continents.ToDictionary(x => x, x => 0);
            foreach(var territoryId in idToCountMap.Keys)
            {
                var territory = board[territoryId];
                countsPerContinent[territory.Continent]++;
                if(countsPerContinent[territory.Continent] == RiskBoard.ContinentCounts[territory.Continent])
                {
                    count += territory.Continent.Bonus;
                }
            }
            return count;
        }
        public RiskCard DispenseCard()
        {
            int ndx = random.Next(cards.Count);
            var card = cards[ndx];
            cards.RemoveAt(ndx);
            return card;
        }

    }
}
