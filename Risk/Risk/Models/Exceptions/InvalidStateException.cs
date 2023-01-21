namespace Risk.Models.Exceptions
{
    /// <summary>
    /// thrown when the components of the state retrieved for a game are misaligned
    /// </summary>
    public class InvalidStateException:Exception
    {
        public InvalidStateException(string message, Game game) : base(message) 
        { 
            GameState = game;
        }
        public InvalidStateException(string message, Exception inner, Game game) : base(message, inner) 
        { 
            GameState = game;
        }
        public Game GameState { get; }
    }
}
