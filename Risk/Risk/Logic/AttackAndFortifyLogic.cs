using Risk.Connections;
using Risk.Models;
using Risk.Models.Exceptions;
using Risk.Persistence;

namespace Risk.Logic
{
    public class AttackAndFortifyLogic
    {
        private readonly IGameRepository gameRepository;
        private readonly IBroadcaster broadcaster;
        private readonly Random random;

        public AttackAndFortifyLogic(IGameRepository gameRepository, IBroadcaster broadcaster)
        {
            this.gameRepository = gameRepository;
            this.broadcaster = broadcaster;
            random = new Random((int)DateTime.Now.Ticks);
        }
        public async Task<int[]> GetAttackableTerritories(Guid gameId, PlayerEnum player, int territoryId)
        {
            var game = await gameRepository.GetGame(gameId);
            var from = game.RiskBoard[territoryId];
            if(from.ArmyState.Owner != player)
            {
                throw new BadRequestException($"{player} player doesn't own territory {territoryId} ({from.TerritoryName})");
            }
            return from.AvailableNeighbors.Where(x => game.RiskBoard[x].ArmyState.Owner != player).ToArray();
        }
        public async Task AttackTerritory(Guid gameId, PlayerEnum player, int fromTerritoryId, int toTerritoryId, int attackCount)
        {
            var game = await gameRepository.GetGame(gameId);
            game.AssertCorrectTurn(player);
            var from = game.RiskBoard[fromTerritoryId];
            var to = game.RiskBoard[toTerritoryId];
            if(from.ArmyState.Owner != player)
            {
                throw new BadRequestException($"{player} player doesn't own territory {fromTerritoryId} ({from.TerritoryName})");
            }
            if (!from.AvailableNeighbors.Contains(toTerritoryId))
            {
                throw new BadRequestException( 
                    $"can't reach {toTerritoryId} ({to.TerritoryName}) from {fromTerritoryId} ({from.TerritoryName})"
                );
            }
            if(attackCount < 1 || attackCount >= from.ArmyState.OccupyingArmyCount || attackCount > 3)
            {
                throw new BadRequestException($"{player} player cannot attack with {attackCount} armies from {fromTerritoryId} ({from.TerritoryName}): number must be between 1 and 3 inclusive, and 1 less than the number on the territory");
            }
            if(to.ArmyState.OccupyingArmyCount < 1)
            {
                if(to.ArmyState.Owner != null)
                {
                    throw new InvalidStateException($"error in program, owner is not null with no armies: {toTerritoryId}{{{to.ArmyState}}}", game);
                }
                throw new BadRequestException($"target territory {toTerritoryId} ({to.TerritoryName}) is unoccupied, {player} player should call conquer instead");
            }

            game.AttackState = new AttackState
            {
                AttackCount = attackCount,
                AttackingTerritoryId = fromTerritoryId,
                DefendingTerritoryId = toTerritoryId
            };
            await gameRepository.SaveGame(game);
            //player 1 decides to attack x from y with z armies
            //notify all players of attack, (all players animate dice of player 1)
            await broadcaster.Attack(gameId, player, fromTerritoryId, to.ArmyState.Owner, toTerritoryId, attackCount);
            //ask player that owns x how many to defend with (for this ex player 2)
            //check random who wins, cases:
            // player 1 won, remove player2's 
            //          (broad cast new armies on territories)
            // player 2 won, remove player1's
            //          (broad cast new armies on territories)
            // then if
            // 1. player 2 out of armies on toTerritoryId,
            //          (player 1 see that empty and call conquer with additionalArmies)
        }
        public async Task Defend(Guid gameId, PlayerEnum player, int armyCount)
        {
            var game = await gameRepository.GetGame(gameId);
            if(game.AttackState == null || game.RiskBoard[game.AttackState.DefendingTerritoryId].ArmyState.Owner != player)
            {
                throw new BadRequestException($"{player} player is not under attack (attackState={game.AttackState})");
            }
            if(armyCount != 1 && armyCount != 2)
            {
                throw new BadRequestException($"invalid army count {armyCount} for defense, must be 1 or 2");
            }
            var defendingTeritoryArmyCount = game.RiskBoard[game.AttackState.DefendingTerritoryId].ArmyState.OccupyingArmyCount;
            if(defendingTeritoryArmyCount < 1)
            {
                throw new InvalidStateException($"defending territory {game.AttackState.DefendingTerritoryId} has no occupying armies and shouldn't be owned or under attack", game);
            }
            if(defendingTeritoryArmyCount < armyCount)
            {
                throw new BadRequestException($"invalid army count {armyCount}, only have {defendingTeritoryArmyCount} to use");
            }
            var attackerColor= game.RiskBoard[game.AttackState.AttackingTerritoryId].ArmyState.Owner 
                                    ?? throw new InvalidStateException($"{game.AttackState.AttackingTerritoryId} is attacking in state but owner is null", game);
            if(attackerColor != game.CurrentPlayer.PlayerColor)
            {
                throw new InvalidStateException($"attacker color {attackerColor} doesn't match current player's {game.CurrentPlayer.PlayerColor}", game);
            }
            await broadcaster.Defend(gameId, player, armyCount);
            int attackMax = 0;
            for(int i= 0; i < game.AttackState.AttackCount; i++)
            {
                var roll = random.Next(6);
                if (roll > attackMax)
                {
                    attackMax = roll;
                }
            }
            int defMax = random.Next(6);
            if(armyCount == 2)
            {
                defMax = Math.Max(defMax, random.Next(6));
            }
            //tie goes to defender, assume attacker win first
            int changeAmount = armyCount;//so defender loses this amount
            var loserTerritoryId = game.AttackState.DefendingTerritoryId; //from here
            var loserColor = player;
            //attack state cleared if defender wins, or attacker wins but doesn't wipe out defender
            game.AttackState.NeedToConquer = (game.RiskBoard[game.AttackState.DefendingTerritoryId].ArmyState.OccupyingArmyCount - armyCount) == 0;
            if(defMax >= attackMax)
            {
                //actually defender win, so loser is attacker
                loserColor = game.RiskBoard[game.AttackState.AttackingTerritoryId].ArmyState.Owner
                                    ?? throw new InvalidStateException($"{game.AttackState.AttackingTerritoryId} is attacking in state but owner is null", game);
                //loses this amount
                changeAmount = game.AttackState.AttackCount;
                //from here
                loserTerritoryId = game.AttackState.AttackingTerritoryId;
                game.AttackState = null;
            }
            //handle attacker wins but doesn't wipe out defender
            else if (!game.AttackState.NeedToConquer)
            {
                game.AttackState = null;
            }
            game.RiskBoard[loserTerritoryId].ArmyState.OccupyingArmyCount -= changeAmount;
            await broadcaster.TerritoryChange(
                gameId,
                loserTerritoryId,
                loserColor,
                game.RiskBoard[loserTerritoryId].ArmyState.OccupyingArmyCount
            );
            //if need to conquer then player eliminated from territory, check if fully dead
            if(game.AttackState != null && game.AttackState.NeedToConquer)
            {
                if(!game.RiskBoard.Any(t=>t.ArmyState.Owner == loserColor))
                {
                    game.CurrentPlayer.Cards.AddRange(game.Players.First(p=>p.PlayerColor == loserColor).Cards);
                    game.RemovePlayer(loserColor);
                    await broadcaster.PlayerEliminated(gameId, loserColor);
                    await broadcaster.RiskCardChange(gameId, game.CurrentPlayer.PlayerColor, game.CurrentPlayer.Cards.Count);
                    if (game.Players.Count == 1)
                    {
                        await broadcaster.GameWon(gameId, game.CurrentPlayer.PlayerColor);
                    }
                }
            }
            await gameRepository.SaveGame(game);

        }
        public async Task ConquerTerritory(Guid gameId, PlayerEnum player, int additionalArmies)
        {
            var game = await gameRepository.GetGame(gameId);
            game.AssertCorrectTurn(player);
            if(game.AttackState == null || !game.AttackState.NeedToConquer)
            {
                throw new BadRequestException($"{player} player has not won any recent battles that need conquering (attackState={game.AttackState})");
            }
            int changeAmount = game.AttackState.AttackCount + additionalArmies;
            int fromTerritoryId = game.AttackState.AttackingTerritoryId;
            int toTerritoryId = game.AttackState.DefendingTerritoryId;
            if(changeAmount >= game.RiskBoard[fromTerritoryId].ArmyState.OccupyingArmyCount)
            {
                throw new BadRequestException(
                    $"{player} player cannot send in {additionalArmies}, with {game.AttackState.AttackCount} from attack, " +
                    $"would leave from territory {fromTerritoryId} " +
                    $"({game.RiskBoard[fromTerritoryId].TerritoryName}) with less than one occupying it" +
                    $"(currently occupied by {game.RiskBoard[fromTerritoryId].ArmyState.OccupyingArmyCount} armies)"
                );
            }
            game.RiskBoard[fromTerritoryId].ArmyState.OccupyingArmyCount -= changeAmount;
            game.RiskBoard[toTerritoryId].ArmyState.OccupyingArmyCount = changeAmount;
            game.RiskBoard[toTerritoryId].ArmyState.Owner = player;
            game.AttackState = null;
            if (!game.GainedCardThisTurn)
            {
                await broadcaster.RiskCardChange(gameId, player, game.CurrentPlayer.Cards.Count);
                game.CurrentPlayer.Cards.Add(game.RiskBank.DispenseCard());
                game.GainedCardThisTurn = true;
            }
            await gameRepository.SaveGame(game);
            await broadcaster.TerritoryChange(gameId, fromTerritoryId, player, game.RiskBoard[fromTerritoryId].ArmyState.OccupyingArmyCount);
            await broadcaster.TerritoryChange(gameId, toTerritoryId, player, game.RiskBoard[toTerritoryId].ArmyState.OccupyingArmyCount);
        }
        public async Task Fortify(Guid gameId, PlayerEnum player, int fromTerritoryId, int toTerritoryId)
        {
            var game = await gameRepository.GetGame(gameId);
            game.AssertCorrectTurn(player);
            RiskTerritory from;
            RiskTerritory to;
            try
            {
                from = game.RiskBoard[fromTerritoryId];
                to = game.RiskBoard[toTerritoryId];
            }
            catch (ArgumentException ex)
            {
                throw new BadRequestException(ex.Message, ex);
            }
            if (from.ArmyState.Owner != player)
            {
                throw new BadRequestException($"{player} player doesn't own from territory {from.GetNameRep()}");
            }
            if (to.ArmyState.Owner != player)
            {
                throw new BadRequestException($"{player} player doesn't own to territory {to.GetNameRep()}");
            }
            if (from.ArmyState.HasSentFortification)
            {
                throw new BadRequestException($"{player} player has already sent fortification from {from.GetNameRep()}");
            }
            if (from.ArmyState.OccupyingArmyCount < 2)
            {
                throw new BadRequestException($"{player} player must have at least 2 armies on territory {from.GetNameRep()}, (has {from.ArmyState.OccupyingArmyCount})");
            }
            from.ArmyState.OccupyingArmyCount--;
            from.ArmyState.HasSentFortification = true;
            to.ArmyState.OccupyingArmyCount++;
            await gameRepository.SaveGame(game);
            await broadcaster.TerritoryChange(gameId, fromTerritoryId, player, from.ArmyState.OccupyingArmyCount);
            await broadcaster.TerritoryChange(gameId, toTerritoryId, player, to.ArmyState.OccupyingArmyCount);
        }
        public async Task EndTurn(Guid gameId, PlayerEnum player)
        {
            var game = await gameRepository.GetGame(gameId);
            game.AssertCorrectTurn(player);
            game.NextTurn();
            foreach(var territory in game.RiskBoard)
            {
                territory.ArmyState.HasSentFortification = false;
            }
            await gameRepository.SaveGame(game);
            await broadcaster.PlayerTurnChange(gameId, game.CurrentPlayer.PlayerColor);
        }
    }
}
