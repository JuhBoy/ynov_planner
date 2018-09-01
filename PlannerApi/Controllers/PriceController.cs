using Microsoft.AspNetCore.Mvc;
using events_planner.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;
using events_planner.Deserializers;

namespace events_planner.Controllers {

    [Route("api/[controller]")]
    public class PriceController : BaseController {

        public PlannerContext Context { get; }

        public PriceController(PlannerContext context) {
            Context = context;
        }

        /// <summary>
        /// Create a new price for a given event&role
        /// </summary>
        /// <param name="priceDsl">Price serialized</param>
        /// <returns>201 NoContent</returns>
        [HttpPost]
        public IActionResult Create([FromBody] PriceDeserializer priceDsl) {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            bool mPrice = Context.Price.Any(p => p.EventId == priceDsl.EventId && p.RoleId == priceDsl.RoleId);
            bool mEvent = Context.Event.Any(p => p.Id == priceDsl.EventId);
            bool mRole = Context.Role.Any(p => p.Id == priceDsl.RoleId);
            
            if (mPrice) return BadRequest("Price Already exist");
            if (!mEvent) return BadRequest("Event doesn't exist");
            if (!mRole) return BadRequest("Role doesn't exist");
            
            var price = new Price() {
                EventId = priceDsl.EventId,
                RoleId = priceDsl.RoleId,
                Amount = priceDsl.Amount
            };

            try {
                Context.Price.Add(price);
                Context.SaveChanges();
            } catch (DbUpdateException e) {
                return BadRequest(e.Message);
            }

            return NoContent();
        }

        /// <summary>
        /// Return Price object for an event
        /// </summary>
        /// <param name="eventId">The event id</param>
        /// <returns>Price model</returns>
        [HttpGet("{eventId}"), AllowAnonymous]
        public IActionResult GetPrice(int eventId) {
            var @event = Context.Event.Include(i => i.Prices)
                                      .ThenInclude(i => i.Role).FirstOrDefault(ev => ev.Id == eventId);
            if (@event == null) return BadRequest("Event Missing");
            return Ok(@event.Prices);
        }

        /// <summary>
        /// Update the Amount of the Price model
        /// </summary>
        /// <param name="priceId">Price uniq Id</param>
        /// <param name="amount">Amount as an integer</param>
        /// <returns>201</returns>
        [HttpPut("{priceId}"), Authorize(Roles = "Admin")]
        public IActionResult Update(int priceId, int amount) {
            var mPrice = Context.Price.FirstOrDefault(arg => arg.Id == priceId);

            if (mPrice == null) return NotFound();

            try {
                mPrice.Amount = amount;
                Context.Price.Update(mPrice);
                Context.SaveChanges();
            } catch (DbUpdateException e) {
                return BadRequest(e.InnerException.Message);
            }

            return NoContent();
        }

        /// <summary>
        /// Delete a Price Model
        /// </summary>
        /// <param name="priceId">Price uniq Id</param>
        /// <returns>201</returns>
        [HttpDelete("{priceId}"), Authorize(Roles = "Admin")]
        public IActionResult Delete(int priceId) {
            var mPrice = Context.Price.FirstOrDefault(arg => arg.Id == priceId);
            if (mPrice == null) return NotFound();

            try {
                Context.Price.Remove(mPrice);
                Context.SaveChanges();
            } catch (DbUpdateException e) {
                return BadRequest(e.InnerException.Message);
            }

            return NoContent();
        }
    }
}