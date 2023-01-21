namespace Risk.Models
{
    public class AttackState
    {
        public int AttackCount { get; set; }
        public int AttackingTerritoryId { get; set; }
        public int DefendingTerritoryId { get; set; }
        public bool NeedToConquer { get; set; }
        public override string ToString() 
            => $"{AttackingTerritoryId} => {DefendingTerritoryId} with {AttackCount}{(NeedToConquer?", (needs to conquer)":"")}";
    }
}
