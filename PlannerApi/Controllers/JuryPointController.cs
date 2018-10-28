using System;
using System.Threading.Tasks;
using events_planner.Constants;
using events_planner.Deserializers;
using events_planner.Models;
using events_planner.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace events_planner.Controllers {
    
    [Route("api/[controller]")]
    public class JuryPointController : Controller {
        
        public IJuryPointServices Services { get; }

        public JuryPointController(IJuryPointServices services) {
            Services = services;
        }

        [HttpPost("create"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] JuryPointCreateDeserializer juryPointDsl) {
            if (!ModelState.IsValid) { return BadRequest(ModelState); }

            var jp = await Services.CreateJuryPointAsync(juryPointDsl.Points, juryPointDsl.Description,
                juryPointDsl.UserId, null);
           
            try {
                await Services.SaveAsync();
            } catch (DbUpdateException ex) {
                LogManager.GetCurrentClassLogger().Error(ex);
                return BadRequest(ApiErrors.JuryPointDbError);
            }
            
            return CreatedAtAction(nameof(Read), new { juryPointId = jp.Id }, jp);
        }

        [HttpGet("{juryPointId}"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> Read(int juryPointId) {
            var jp = await Services.GetJuryPointAsync(juryPointId);
            if (jp == null) { return BadRequest(ApiErrors.JuryPointNotFound); }

            return Ok(jp);
        }

        [HttpGet("user/{userId}"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReadMultiple(int userId) {
            JuryPoint[] jps = await Services.GetJuryPointsAsync(userId);
            return Ok(jps);
        }

        [HttpPatch, Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update([FromBody] JuryPointUpdateDeserializer juryPointUpdateDsl) {
            var jp = await Services.UpdateJuryPoint(juryPointUpdateDsl);
            if (jp == null) return BadRequest(ApiErrors.JuryPointNotFound);

            try {
                await Services.SaveAsync();
            } catch (DbUpdateException ex) {
                LogManager.GetCurrentClassLogger().Error(ex);
                return BadRequest(ApiErrors.JuryPointDbError);
            }
            
            return Ok(jp);
        }

        [HttpDelete("{juryPointId}"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int juryPointId) {
            var jp = await Services.GetJuryPointAsync(juryPointId);
            if (jp == null) return NotFound();

            try {
                Services.RemoveJuryPoints(jp, true);
                await Services.SaveAsync();
            } catch (DbUpdateException ex) {
                LogManager.GetCurrentClassLogger().Error(ex);
                return BadRequest(ApiErrors.JuryPointDbError);
            } catch (ArgumentException ex) {
                LogManager.GetCurrentClassLogger().Error(ex);
                return BadRequest(ApiErrors.JuryPointInvalidDelete); //TODO use constants
            }

            return Ok();
        }
    }
    
}