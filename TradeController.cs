﻿using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace CrossExchange.Controller
{
    [Route("api/Trade")]
    public class TradeController : ControllerBase
    {
        private IShareRepository _shareRepository { get; set; }
        private ITradeRepository _tradeRepository { get; set; }
        private IPortfolioRepository _portfolioRepository { get; set; }

        public TradeController(IShareRepository shareRepository, ITradeRepository tradeRepository, IPortfolioRepository portfolioRepository)
        {
            _shareRepository = shareRepository;
            _tradeRepository = tradeRepository;
            _portfolioRepository = portfolioRepository;
        }

        [HttpGet("{portfolioid}")]
        public async Task<IActionResult> GetAllTradings([FromRoute]int portFolioid)
        {
            var trade = _tradeRepository.Query().Where(x => x.PortfolioId.Equals(portFolioid));
            return Ok(trade);
        }

        /*************************************************************************************************************************************
        For a given portfolio, with all the registered shares you need to do a trade which could be either a BUY or SELL trade. For a particular trade keep following conditions in mind:
		BUY:
        a) The rate at which the shares will be bought will be the latest price in the database.
		b) The share specified should be a registered one otherwise it should be considered a bad request.
		c) The Portfolio of the user should also be registered otherwise it should be considered a bad request.

        SELL:
        a) The share should be there in the portfolio of the customer.
		b) The Portfolio of the user should be registered otherwise it should be considered a bad request.
		c) The rate at which the shares will be sold will be the latest price in the database.
        d) The number of shares should be sufficient so that it can be sold.
        Hint: You need to group the total shares bought and sold of a particular share and see the difference to figure out if there are sufficient quantities available for SELL.

        *************************************************************************************************************************************/

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]TradeModel model)
        {
            var rate = _tradeRepository.Query().Where(x => x.Symbol == model.Symbol)
                    .Select(x => x.Price).LastOrDefault();

            var portfolio = _portfolioRepository.Query().Where(x => x.Id == model.PortfolioId).FirstOrDefault();
            if (portfolio == null)
            {
                return BadRequest();
            }

            var share = _shareRepository.Query().Where(x => x.Symbol == model.Symbol);
            if (share == null)
            {
                return BadRequest();
            }

            if (model.Action == "BUY")
            {
                var e = new Trade
                {
                    Action = "BUY",
                    NoOfShares = model.NoOfShares,
                    PortfolioId = portfolio.Id,
                    Price = rate,
                    Symbol = model.Symbol
                };

                var result = _tradeRepository.InsertAsync(e);
            }
            else if (model.Action == "SELL")
            {
                var buyedShare = portfolio.Trade.Where(x => x.Symbol == model.Symbol && x.Action == "BUY")
                    .Sum(x => x.NoOfShares);

                var sellShare = portfolio.Trade.Where(x => x.Symbol == model.Symbol && x.Action == "SELL")
                    .Sum(x => x.NoOfShares);

                var remainShare = buyedShare - sellShare;

                if (remainShare >= model.NoOfShares)
                {
                    var e = new Trade
                    {
                        Action = "SELL",
                        NoOfShares = model.NoOfShares,
                        PortfolioId = portfolio.Id,
                        Price = rate,
                        Symbol = model.Symbol
                    };

                    var result = _tradeRepository.InsertAsync(e);
                }
            }

            return Created("Trade", model);
        }
    }
}