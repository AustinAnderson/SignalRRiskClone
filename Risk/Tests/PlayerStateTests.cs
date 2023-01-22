using Microsoft.VisualStudio.TestTools.UnitTesting;
using Risk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass]
    public class TestGetAvailableSets
    {
        private void AssertSetDescriptorMatch(int[] expected, int? expectedBonusId, RiskCardSetDescriptor descriptor, string message="")
        {
            Assert.AreEqual(expectedBonusId, descriptor.TerritoryBonusId, "expected bonus id");
            Assert.AreEqual(
                string.Join(",", expected.OrderBy(x => x)),
                string.Join(",", descriptor.OrderBy(x => x)), 
                "card set; "+message
            );
        }
        [TestMethod]
        public void ThreeSame()
        {
            var board = new RiskBoard();
            PlayerState state = new PlayerState();
            state.Cards = new List<RiskCard>
            {
                new(1, RiskCard.Type.Infantry),
                new(2, RiskCard.Type.Infantry),
                new(3, RiskCard.Type.Infantry),
            };
            var res = state.GetAvailableSets(board);
            Assert.AreEqual(1, res?.Count, "set count");
#pragma warning disable CS8602 // Dereference of a possibly null reference, checked by assert
            AssertSetDescriptorMatch(new[] { 1, 2, 3 }, null, res[0]);
#pragma warning restore CS8602 
        }
        [TestMethod]
        public void ThreeDifferent()
        {
            var board = new RiskBoard();
            PlayerState state = new PlayerState();
            state.Cards = new List<RiskCard>
            {
                new(1, RiskCard.Type.Infantry),
                new(2, RiskCard.Type.Cannon),
                new(3, RiskCard.Type.Calvary),
            };
            var res = state.GetAvailableSets(board);
            Assert.AreEqual(1, res?.Count, "set count");
#pragma warning disable CS8602 // Dereference of a possibly null reference, checked by assert
            AssertSetDescriptorMatch(new[] { 1, 2, 3 }, null, res[0]);
#pragma warning restore CS8602 
        }
        [TestMethod]
        public void ThreeNoneAvailable()
        {
            var board = new RiskBoard();
            PlayerState state = new PlayerState();
            state.Cards = new List<RiskCard>
            {
                new(1, RiskCard.Type.Calvary),
                new(2, RiskCard.Type.Cannon),
                new(3, RiskCard.Type.Calvary),
            };
            var res = state.GetAvailableSets(board);
            Assert.AreEqual(0, res?.Count, "set count");
        }
        [TestMethod]
        public void ThreeSameOneWild()
        {
            var board = new RiskBoard();
            PlayerState state = new PlayerState();
            state.Cards = new List<RiskCard>
            {
                new(1, RiskCard.Type.Infantry),
                new(-1, RiskCard.Type.Wild),
                new(3, RiskCard.Type.Infantry),
            };
            var res = state.GetAvailableSets(board);
            Assert.AreEqual(1, res?.Count, "set count");
#pragma warning disable CS8602 // Dereference of a possibly null reference, checked by assert
            AssertSetDescriptorMatch(new[] { 1, -1, 3 }, null, res[0]);
#pragma warning restore CS8602 
        }
        [TestMethod]
        public void ThreeDifferentOneWild()
        {
            for (int i = 0; i < 3; i++)
            {
                var board = new RiskBoard();
                PlayerState state = new PlayerState();
                state.Cards = new List<RiskCard>
                {
                    new(1, RiskCard.Type.Infantry),
                    new(2, RiskCard.Type.Cannon),
                    new(3, RiskCard.Type.Calvary),
                };
                string missing = state.Cards[i].CardType.ToString();
                var expected = new[] { 1, 2, 3 };
                expected[i] = -1;
                state.Cards[i]=new RiskCard(-1, RiskCard.Type.Wild);
                var res = state.GetAvailableSets(board);
                Assert.AreEqual(1, res?.Count, "set count");
#pragma warning disable CS8602 // Dereference of a possibly null reference, checked by assert
                AssertSetDescriptorMatch(expected, null, res[0], "wild replacing "+missing);
#pragma warning restore CS8602
            }
        }

    }
}
