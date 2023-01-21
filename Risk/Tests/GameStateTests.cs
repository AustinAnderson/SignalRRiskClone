using Microsoft.VisualStudio.TestTools.UnitTesting;
using Risk.Models;
using System.Linq;

namespace Tests
{
    [TestClass]
    public class GameStateTests
    {
        [TestMethod]
        public void RemovePlayerWhenRemoveAfter()
        {
            var game = new Game
            {
                Players = new() {
                    new(){ PlayerColor= PlayerEnum.Blue },
                    new(){ PlayerColor= PlayerEnum.Orange},
                    new(){ PlayerColor= PlayerEnum.Yellow},
                    new(){ PlayerColor= PlayerEnum.Green},
                    new(){ PlayerColor= PlayerEnum.Black}
                },
                PlayerTurnIndex = 2
            };
            var currentBeforeOp = game.CurrentPlayer.PlayerColor;
            game.RemovePlayer(PlayerEnum.Green);
            Assert.AreEqual(
                string.Join(",", new[]
                {
                    PlayerEnum.Blue,
                    PlayerEnum.Orange,
                    PlayerEnum.Yellow,
                    PlayerEnum.Black
                }),
                string.Join(",", game.Players.Select(p => p.PlayerColor)),
                "expected player list to be the same except with green removed"
            );
            Assert.AreEqual(
                currentBeforeOp, game.CurrentPlayer.PlayerColor, 
                "expected turn index to update to point to the same player"
            );
        }
        [TestMethod]
        public void RemovePlayerWhenRemoveBefore()
        {
            var game = new Game
            {
                Players = new() {
                    new(){ PlayerColor= PlayerEnum.Blue },
                    new(){ PlayerColor= PlayerEnum.Orange},
                    new(){ PlayerColor= PlayerEnum.Yellow},
                    new(){ PlayerColor= PlayerEnum.Green},
                    new(){ PlayerColor= PlayerEnum.Black}
                },
                PlayerTurnIndex = 2
            };
            var currentBeforeOp = game.CurrentPlayer.PlayerColor;
            game.RemovePlayer(PlayerEnum.Orange);
            Assert.AreEqual(
                string.Join(",", new[]
                {
                    PlayerEnum.Blue,
                    PlayerEnum.Yellow,
                    PlayerEnum.Green,
                    PlayerEnum.Black
                }),
                string.Join(",", game.Players.Select(p => p.PlayerColor)),
                "expected player list to be the same except with orange removed"
            );
            Assert.AreEqual(
                currentBeforeOp, game.CurrentPlayer.PlayerColor, 
                "expected turn index to update to point to the same player"
            );
        }
        //can never be eliminated on your turn because can only attack if leaving 1 territory behind and not using them in the attack

    }
}