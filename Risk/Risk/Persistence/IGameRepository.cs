using Risk.Models;

namespace Risk.Persistence
{
    public interface IGameRepository
    {
        public Task<Game> GetGame(Guid GameId);
        public Task SaveGame(Game game);
    }
}
