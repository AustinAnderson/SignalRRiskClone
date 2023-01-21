namespace Risk.Models.Api
{
    public class TurnStartArmyInfo
    {
        public int NewArmies { get; set; }
        public List<RiskCardSetDescriptor> AvailableSets { get; set; }
        public bool MustUseOneSet { get; set; }
    }
}
