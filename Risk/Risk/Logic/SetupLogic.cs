using Risk.Connections;
using Risk.Models;
using Risk.Models.Api;
using Risk.Models.Exceptions;
using Risk.Persistence;

namespace Risk.Logic
{
    public class SetupLogic
    {
        private readonly IGameRepository gameRepository;
        private readonly IBroadcaster broadcaster;
        private readonly Random random;
        public SetupLogic(IGameRepository gameRepository, IBroadcaster broadcaster)
        {
            this.gameRepository = gameRepository;
            this.broadcaster = broadcaster;
            random = new Random((int)DateTime.Now.Ticks);
        }
        public async Task<Game> StartGame(PlayerEnum[] players)
        {
            if(players.Distinct().Count() != players.Length)
            {
                throw new BadRequestException("Players must use distinct colors");
            }
            int armiesPerPlayer = players.Length switch
            {
                1 => throw new BadRequestException("Cannot play with only one person"),
                2 => 40,
                3 => 35,
                4 => 30,
                5 => 25,
                6 => 20,
                _ => throw new NotImplementedException($"Unknown how many armies for a game with {players.Length} players")
            };
            var game = new Game
            {
                Players = players.Select(x => new PlayerState
                {
                    AllocableArmies = armiesPerPlayer,
                    PlayerColor = x
                }).ToList(),
                PlayerTurnIndex = random.Next(0,players.Length),
                Id = Guid.NewGuid(),
                RiskBank = new RiskBank(),
                RiskBoard = new RiskBoard()
            };
            await gameRepository.SaveGame(game);
            return game;//or mapper.ToApiModel(game)
        }
        public async Task PlaceInitialArmy(Guid gameId, PlayerEnum player, int territoryId)
        {
            var game = await gameRepository.GetGame(gameId);
            game.AssertCorrectTurn(player);
            game.RiskBoard.ClaimTerritory(game.CurrentPlayer, territoryId);
            await broadcaster.TerritoryChange(
                gameId, territoryId, player, 
                game.RiskBoard[territoryId].ArmyState.OccupyingArmyCount
            );
            game.NextTurn();
            var newTurn=game.CurrentPlayer.PlayerColor;
            if (game.Players.All(x => x.AllocableArmies == 0))
            {
                await broadcaster.FinishClaimPhase(gameId, newTurn);
            }
            else 
            {
                await broadcaster.PlayerTurnChange(gameId, newTurn);
            }
            await gameRepository.SaveGame(game);
        }
        
        public async Task DistributeArmies(Guid gameId, PlayerEnum player, Dictionary<int, int> additionsByTerritoryId)
        {
            var game = await gameRepository.GetGame(gameId);
            game.AssertCorrectTurn(player);
            if(game.CurrentPlayer.AllocableArmies != additionsByTerritoryId.Values.Sum())
            {
                throw new BadRequestException($"{player} player must distribute exactly {game.CurrentPlayer.AllocableArmies} armies");
            }
            foreach(var distribution in additionsByTerritoryId)
            {
                game.RiskBoard.AddArmies(game.CurrentPlayer, distribution.Key, distribution.Value);
                await broadcaster.TerritoryChange(gameId, distribution.Key, player,
                    game.RiskBoard[distribution.Key].ArmyState.OccupyingArmyCount
                );
            }
        }
        

    }
}
