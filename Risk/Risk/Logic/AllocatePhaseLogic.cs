using Risk.Connections;
using Risk.Models;
using Risk.Models.Api;
using Risk.Models.Exceptions;
using Risk.Persistence;

namespace Risk.Logic
{
    public class AllocatePhaseLogic
    {
        private readonly IGameRepository gameRepository;
        private readonly IBroadcaster broadcaster;
        public AllocatePhaseLogic(IGameRepository gameRepository, IBroadcaster broadcaster)
        {
            this.gameRepository = gameRepository;
            this.broadcaster = broadcaster;
        }
        public async Task<TurnStartArmyInfo> StartTurn(Guid gameId, PlayerEnum player)
        {
            var game = await gameRepository.GetGame(gameId);
            game.AssertCorrectTurn(player);
            var turnStartArmyInfo = new TurnStartArmyInfo
            {
                NewArmies = game.RiskBank.GetAllocableArmies(game.RiskBoard, game.CurrentPlayer.PlayerColor),
                AvailableSets = game.CurrentPlayer.GetAvailableSets(game.RiskBoard),
                MustUseOneSet = game.CurrentPlayer.Cards.Count >= 5
            };
            game.CurrentPlayer.AllocableArmies = turnStartArmyInfo.NewArmies;
            await gameRepository.SaveGame(game);
            return turnStartArmyInfo;
        }
        public async Task<int> TradeInCards(Guid gameId, PlayerEnum player, List<RiskCardSetDescriptor> setList)
        {
            var game = await gameRepository.GetGame(gameId);
            game.AssertCorrectTurn(player);
            var availableSets = game.CurrentPlayer.GetAvailableSets(game.RiskBoard);
            if(availableSets.Count > setList.Count)
            {
                throw new BadRequestException($"{player} player cannot trade in {setList} sets, only {availableSets.Count} available");
            }
            foreach(var set in setList)
            {
                if (!availableSets.Contains(set))
                {
                    throw new BadRequestException($"{player} player cannot trade in set {set}, one or more cards are not owned");
                }
            }
            int additionalArmies = 0;
            int? bonusTerritory=null;
            foreach(var setDescription in setList)
            {
                var set=game.CurrentPlayer.TakeSet(setDescription);
                bonusTerritory = set.TerritoryBonusId;
                additionalArmies+=game.RiskBank.TradeInCards(set);
            }
            if (bonusTerritory != null)
            {
                game.RiskBoard[bonusTerritory.Value].ArmyState.OccupyingArmyCount += 2;
                var newCount = game.RiskBoard[bonusTerritory.Value].ArmyState.OccupyingArmyCount;
                await broadcaster.TerritoryChange(gameId, bonusTerritory.Value, player, newCount);
            }
            await broadcaster.RiskCardChange(gameId, player, game.CurrentPlayer.Cards.Count);
            game.CurrentPlayer.AllocableArmies += additionalArmies;
            await gameRepository.SaveGame(game);
            return additionalArmies;
        }
    }
}
