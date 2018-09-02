using System;
using System.Threading.Tasks;
using CrossExchange.Controller;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using Moq;

namespace CrossExchange.Tests
{
    public class ShareControllerTests
    {
        private readonly Mock<IShareRepository> _shareRepositoryMock = new Mock<IShareRepository>();
        private readonly Mock<ITradeRepository> _tradeRepositoryMock = new Mock<ITradeRepository>();
        private readonly Mock<IPortfolioRepository> _portfolioRepositoryMock= new Mock<IPortfolioRepository>();
        private readonly ShareController _shareController;
        private readonly TradeController _tradeController;

        public ShareControllerTests()
        {
            _shareController = new ShareController(_shareRepositoryMock.Object);
            _tradeController = new TradeController(_shareRepositoryMock.Object,_tradeRepositoryMock.Object, _portfolioRepositoryMock.Object);
        }

        [Test]
        public async Task Post_ShouldInsertHourlySharePrice()
        {
            var hourRate = new HourlyShareRate
            {
                Symbol = "CBI",
                Rate = 330.0M,
                TimeStamp = new DateTime(2018, 08, 17, 5, 0, 0)
            };

            // Arrange

            // Act
            var result = await _shareController.Post(hourRate);

            // Assert
            Assert.NotNull(result);

            var createdResult = result as CreatedResult;
            Assert.NotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
        }

        [Test]
        public async Task Post()
        {
            var tradeModel = new TradeModel
            {
                Action = "BUY",
                NoOfShares = 10,
                PortfolioId = 1,                
                Symbol = "REL"
            };

            // Arrange

            // Act
            var result = await _tradeController.Post(tradeModel);

            // Assert
            Assert.NotNull(result);
        }

    }
}
