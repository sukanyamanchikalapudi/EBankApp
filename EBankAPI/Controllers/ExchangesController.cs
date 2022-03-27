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

        // GET: api/Exchanges
        public IQueryable<Exchanges> GetExchanges()
        {
            return db.Exchanges;
        }

        // GET: api/Exchanges/5
        [ResponseType(typeof(Exchanges))]
        public async Task<IHttpActionResult> GetExchanges(int id)
        {
            Exchanges exchanges = await db.Exchanges.FindAsync(id);
            if (exchanges == null)
            {
                return NotFound();
            }

            return Ok(exchanges);
        }

        // PUT: api/Exchanges/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutExchanges(int id, Exchanges exchanges)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != exchanges.Id)
            {
                return BadRequest();
            }

            db.Entry(exchanges).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ExchangesExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Exchanges
        [ResponseType(typeof(Exchanges))]
        public async Task<IHttpActionResult> PostExchanges(Exchanges exchanges)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Exchanges.Add(exchanges);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = exchanges.Id }, exchanges);
        }

        // DELETE: api/Exchanges/5
        [ResponseType(typeof(Exchanges))]
        public async Task<IHttpActionResult> DeleteExchanges(int id)
        {
            Exchanges exchanges = await db.Exchanges.FindAsync(id);
            if (exchanges == null)
            {
                return NotFound();
            }

            db.Exchanges.Remove(exchanges);
            await db.SaveChangesAsync();

            return Ok(exchanges);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ExchangesExists(int id)
        {
            return db.Exchanges.Count(e => e.Id == id) > 0;
        }
    }
}