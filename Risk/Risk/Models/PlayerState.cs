namespace Risk.Models
{
    public class PlayerState
    {
        public PlayerEnum PlayerColor { get; set; }
        public List<RiskCard> Cards = new List<RiskCard>();
        public int AllocableArmies { get; set; }



        private class WorkingRiskCardSet
        {
            public WorkingRiskCardSet(RiskCard ref1, RiskCard ref2, RiskCard ref3)
            {
                RiskCardRefs=new List<RiskCard> { ref1, ref2, ref3 };
            }
            public List<RiskCard> RiskCardRefs;
            public int? TerritoryBonusId;
        }
        public List<RiskCardSetDescriptor> GetAvailableSets(RiskBoard board)
        {
            var byType = Enum.GetValues<RiskCard.Type>().ToDictionary(x => x, x => new List<RiskCard>());
            foreach(var card in Cards)
            {
                byType[card.CardType].Add(card);
            }

            var sets = new List<WorkingRiskCardSet>();
            var wilds=new List<RiskCard>();
            if (byType.ContainsKey(RiskCard.Type.Wild))
            {
                wilds = byType[RiskCard.Type.Wild];
                byType.Remove(RiskCard.Type.Wild);
            }
            //now byType and wilds have all cards in Cards list
            //make max sets ignoring wilds

            //h horse m man, g gun
            //h 1 1 1
            //m 1 1
            //g 1 1 1
            //can't have 3 of hmg, would be equal to 3h 3m 3g
            //so after analysing all combinations of 4 options 000 100 110 111 ^ 3,
            //always go with max hmg (1 or 2 of those) unless one row has 1 card and 
            //the others have 3 or more, in which case don't group the hmg group

            //so pull out hmg groups from dict, then make remaining 3 of a kind groups
            int hmgCount = 0;
            if(byType.Any(x=>x.Value.Count==1) && byType.All(x=>x.Value.Count >=1) && !IgnoreHmgGroup(byType))
            {
                hmgCount = 1;
            }
            else if(byType.Any(x=>x.Value.Count==2) && byType.All(x => x.Value.Count >= 2))
            {
                hmgCount = 2;
            }
            for(int i = 0; i < hmgCount; i++)
            {
                sets.Add(new(
                    byType[RiskCard.Type.Calvary][0],
                    byType[RiskCard.Type.Infantry][0],
                    byType[RiskCard.Type.Cannon][0]
                ));
                byType[RiskCard.Type.Calvary].RemoveAt(0);
                byType[RiskCard.Type.Infantry].RemoveAt(0);
                byType[RiskCard.Type.Cannon].RemoveAt(0);
            }
            //with hmg groups pulled out, make sets
            foreach(var kvp in byType)
            {
                foreach(var group in kvp.Value.Chunk(3).ToArray())
                {
                    if (group.Length == 3)
                    {
                        sets.Add(new (group[0],group[1],group[2]));
                        kvp.Value.Remove(group[0]);
                        kvp.Value.Remove(group[1]);
                        kvp.Value.Remove(group[2]);
                    }
                }
            }
            //after making without wilds, make furthur sets if possible with wilds and unused not wilds
            if(wilds.Count == 2 && byType.Sum(x=>x.Value.Count) == 1)
            {
                //if only one left over card, and two wilds, can use that
                var kvp=byType.First(x => x.Value.Count == 1);
                sets.Add(new(wilds[0], wilds[1], kvp.Value[0]));
                wilds.Clear();
                kvp.Value.RemoveAt(0);
            }
            //not sure about this but idea is need to keep wilds, so need to track left over
            foreach(var wild in wilds)
            {
                AddSetsForOneWildCard(byType, sets, wild);
            }
            //check if any set has a card that matches owned, if so put it at front for each that do
            foreach(var set in sets)
            {
                foreach(var card in set.RiskCardRefs)
                {
                    if (card.TerritoryId>=0 && board[card.TerritoryId].ArmyState.Owner==PlayerColor)
                    {
                        set.TerritoryBonusId = card.TerritoryId;
                        break;
                    }
                }
            }
            sets = sets.OrderBy(x => x.TerritoryBonusId ?? int.MaxValue).ToList();

            //if not check left over cards to see if any are the same type and can be swapped for one in a set that matches territory
                //TODO:
            if(!sets.Any(x=>x.TerritoryBonusId != null))
            {
                var availableSwaps = new Queue<RiskCard>(byType.SelectMany(kvp => kvp.Value)
                                           .Where(x => board[x.TerritoryId].ArmyState.Owner == PlayerColor));
                foreach(var set in sets)
                {
                    if (availableSwaps.Count == 0) break;
                    for(int i=set.RiskCardRefs.Count - 1; i >= 0; i--)
                    {
                        if(set.RiskCardRefs[i].CardType == availableSwaps.Peek().CardType)
                        {
                            set.RiskCardRefs[i]=availableSwaps.Dequeue();
                            break;
                        }
                    }
                }
            }
            //convert to list of descriptor since no new risk cards have been created or removed from the player's hand
            return sets.Select(x => 
                new RiskCardSetDescriptor(x.RiskCardRefs[0].TerritoryId, x.RiskCardRefs[1].TerritoryId, x.RiskCardRefs[2].TerritoryId) 
                { 
                    TerritoryBonusId = x.TerritoryBonusId
                }
            ).ToList();
        }
        //return false if the wild can't complete any set
        private bool AddSetsForOneWildCard(Dictionary<RiskCard.Type,List<RiskCard>> grouped, List<WorkingRiskCardSet> sets, RiskCard wild)
        {
            //by prioritizing hmg we can handle the case where two wilds completes * 2 2
            //find hmg and return immediately or
            for(RiskCard.Type t1=RiskCard.Type.Infantry; t1 <= RiskCard.Type.Cannon; t1++)
            {
                for(RiskCard.Type t2=RiskCard.Type.Infantry; t2 <= RiskCard.Type.Cannon; t2++)
                {
                    if (t1 == t2) continue;
                    if (grouped[t1].Count > 0 && grouped[t2].Count > 0)
                    {
                        sets.Add(new(grouped[t1][0], grouped[t2][0], wild));
                        grouped[t1].RemoveAt(0);
                        grouped[t2].RemoveAt(0);
                        return true;
                    }
                }
            }
            //else find 2 in a row
            foreach(var group in grouped)
            {
                //can't be greater than 2 or would be already grouped
                //can't be less than 2 or wouldn't make a group
                if(group.Value.Count == 2)
                {
                    sets.Add(new(group.Value[0], group.Value[1], wild));
                    group.Value.Clear();
                    return true;
                }
            }
            return false;
        }
        private bool IgnoreHmgGroup(Dictionary<RiskCard.Type,List<RiskCard>> grouped)
        {
            //h 1 1 1
            //m 1 1 1 
            //g 1
            return (grouped[RiskCard.Type.Infantry].Count == 1 &&
                    grouped[RiskCard.Type.Cannon].Count == 3 &&
                    grouped[RiskCard.Type.Calvary].Count == 3) ||

                   (grouped[RiskCard.Type.Infantry].Count == 3 &&
                    grouped[RiskCard.Type.Cannon].Count == 1 &&
                    grouped[RiskCard.Type.Calvary].Count == 3) ||

                   (grouped[RiskCard.Type.Infantry].Count == 3 &&
                    grouped[RiskCard.Type.Cannon].Count == 3 &&
                    grouped[RiskCard.Type.Calvary].Count == 1);
        }
        public RiskCardSet TakeSet(RiskCardSetDescriptor set)
        {
            RiskCard[] retrieved=new RiskCard[3];
            int i = 0;
            foreach (int cardId in set)
            {
                retrieved[i]= Cards.FirstOrDefault(x => x.TerritoryId == set.CardId1) 
                ?? throw new ArgumentException(
                    $"Invalid descriptor {set}, {PlayerColor} player doesn't have a card with id {set.CardId1}"
                   );
                i++;
            }
            foreach(var card in retrieved)
            {
                Cards.Remove(card);
            }
            return new RiskCardSet(retrieved);
        }
    }
}
