using Risk.Models;

namespace Risk.Connections
{
    public interface IBroadcaster
    {
        Task TerritoryChange(Guid gameId, int territoryId, PlayerEnum newOwner, int newArmyCount);
        Task PlayerTurnChange(Guid gameId, PlayerEnum newTurn);
        Task FinishClaimPhase(Guid gameId, PlayerEnum newTurn);
        Task RiskCardChange(Guid gameId, PlayerEnum player, int newRiskCardCount);
        Task Attack(Guid gameId, PlayerEnum player, int fromTerritoryId, PlayerEnum? owner, int toTerritoryId, int attackCount);
        Task Defend(Guid gameId, PlayerEnum player, int armyCount);
        Task PlayerEliminated(Guid gameId, PlayerEnum loserColor);
        Task GameWon(Guid gameId, PlayerEnum playerColor);
    }
}
