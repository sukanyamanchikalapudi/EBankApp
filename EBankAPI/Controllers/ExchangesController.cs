using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using EBankAPI;

namespace EBankAPI.Controllers
{
    public class ExchangesController : ApiController
    {
        private EBankAppEntities db = new EBankAppEntities();

        [HttpGet]
        public async Task<IHttpActionResult> GetById(string from, string to)
        {
            var exchangeValue = await ReturnExchangeValue(from, to);
            return Ok(new { Value = exchangeValue });
        }

        [HttpGet]
        public async Task<IHttpActionResult> GetCurrencies()
        {
            var currencies = await db.Exchanges.Select(x => x.CurrencyCode).ToListAsync();
            return Ok(currencies);
        }

        private async Task<double> ReturnExchangeValue(string from, string to)
        {
            if (from == CurrencyCode.GBP.ToString() && to == CurrencyCode.USD.ToString())
            {
                return await db.Exchanges.Where(x => x.CurrencyCode == CurrencyCode.GBP.ToString()).Select(x => x.ExchangeValue_USD).FirstOrDefaultAsync();
            }
            return 0.0;
        }
    }

    public enum CurrencyCode
    {
        USD,
        GBP
    }
}