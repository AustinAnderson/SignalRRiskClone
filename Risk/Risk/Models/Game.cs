using Risk.Models.Exceptions;

namespace Risk.Models
{
    public class Game
    {
        public Guid Id { get; set; }
        public List<PlayerState> Players { get; set; }
        public int PlayerTurnIndex { get; set; }
        public bool GainedCardThisTurn { get; set; }
        public PlayerState CurrentPlayer => Players[PlayerTurnIndex];
        //TODO: definitely test this
        public void RemovePlayer(PlayerEnum loserColor)
        {
            bool currentAfterRemoved = false;
            for(int i=Players.Count-1; i>=0; i--)
            {
                if (i == PlayerTurnIndex)
                {
                    currentAfterRemoved = true;
                }
                if(Players[i].PlayerColor == loserColor)
                {
                    Players.RemoveAt(i);
                    break;
                }
            }
            if (currentAfterRemoved)
            {
                PlayerTurnIndex--;
            }
        }
        public void NextTurn()
        {
            PlayerTurnIndex++;
            if(PlayerTurnIndex >= Players.Count)
            {
                PlayerTurnIndex = 0;
            }
            GainedCardThisTurn = false;
        }
        public void AssertCorrectTurn(PlayerEnum player)
        {
            if(CurrentPlayer.PlayerColor != player)
            {
                throw new BadRequestException($"It's not {player} player's turn for game {Id}");
            }
        }
        public AttackState? AttackState { get; set; }
        public RiskBoard RiskBoard { get; set; }
        public RiskBank RiskBank { get; set; }

        
    }
}
