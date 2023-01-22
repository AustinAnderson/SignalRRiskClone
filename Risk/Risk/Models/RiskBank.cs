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
                new(34, horse),
                new(0, man),
                new(3, horse),
                new(12, man),
                new(11, gun),
                new(16, man),
                new(8, gun),
                new(40, man),
                new(15, man),
                new(29, gun),
                new(5, horse),
                new(7, gun),
                new(14, man),
                new(22, gun),
                new(2, horse),
                new(19, man),
                new(39, horse),
                new(26, gun),
                new(35, horse),
                new(37, gun),
                new(33, man),
                new(18, horse),
                new(38, man),
                new(36, man),
                new(27, man),
                new(13, horse),
                new(23, gun),
                new(1, gun),
                new(4, horse),
                new(10, man),
                new(21, horse),
                new(20, horse),
                new(31, horse),
                new(17, gun),
                new(41, man),
                new(24, gun),
                new(30, horse),
                new(9, man),
                new(28, gun),
                new(25, gun),
                new(6, gun),
                new(32, horse),
                new(-1,RiskCard.Type.Wild),
                new(-2,RiskCard.Type.Wild)
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
