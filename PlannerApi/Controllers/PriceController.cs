using Microsoft.AspNetCore.Mvc;
using events_planner.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System;
using System.IO;

namespace events_planner.Controllers {

    [Route("api/[controller]")]
    public class PriceController : BaseController {

        public PlannerContext Context { get; }

        public PriceController(PlannerContext context) {
            Context = context;
        }

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