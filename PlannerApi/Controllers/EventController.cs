using Microsoft.AspNetCore.Mvc;
using events_planner.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using events_planner.Services;
using Microsoft.AspNetCore.Authorization;
using events_planner.Deserializers;
using System.Linq;
using System;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Collections.Generic;
using System.Threading;

namespace events_planner.Controllers {

    [Route("api/[controller]")]
    public class EventController : BaseController {

        private IEventServices Services { get; set; }

        private ICategoryServices CategoryServices { get; set; }

        private IQueryable<Event> Query;

        public EventController(PlannerContext context,
                               IEventServices services,
                               ICategoryServices categoryServices) {
            Context = context;
            Services = services;
            CategoryServices = categoryServices;
        }

        private void InitializeQuery() {
            Query = Context.Event;

            bool loadImages = HttpContext.Request.Query["images"] == bool.TrueString;
            bool includeModerators = HttpContext.Request.Query["moderators"] == bool.TrueString;
            bool includeCategories = HttpContext.Request.Query["categories"] == bool.TrueString;
            bool includePrices = HttpContext.Request.Query["prices"] == bool.TrueString;
            bool includeRestrictedRoles = HttpContext.Request.Query["restricted"] == bool.TrueString;

            if (loadImages)
                Services.IncludeImages(ref Query);
            if (includeModerators)
                Services.IncludeModerators(ref Query);
            if (includeCategories)
                Services.IncludeCategories(ref Query);
            if (includePrices)
                Services.IncludePrices(ref Query);
            if (includeRestrictedRoles)
                Services.IncludeRestrictedRoles(ref Query);
        }

        #region User CRUD

        /// <summary>
        /// Create a new Event
        /// </summary>
        /// <response code="401">User token is not permitted</response>
        /// <response code="500">if the credential given is not valid or DB update failed</response>
        [HttpPost, Authorize(Roles = "Admin")]
        public IActionResult Create([FromBody] EventDeserializer eventDsl) {
            if (!ModelState.IsValid) { return BadRequest(ModelState); }

            Services.Create(eventDsl, out var mEvent);

            if (!Services.IsTimeWindowValid(ref mEvent)) {
                var error = new String[] { "Time window is not valid, ensure close start before open" };
                return BadRequest(new { TimeWindow = error });
            }

            try {
                Context.SaveChanges();
            } catch (DbUpdateException e) {
                return BadRequest(e.InnerException.Message);
            }

            return CreatedAtAction(nameof(Read), new { id = mEvent.Id }, mEvent);
        }

        /// <summary>
        /// Get Event By ID
        /// </summary>
        /// <response code="401">User/Admin token is not permitted</response>
        /// <response code="500">if the credential given is not valid or DB update failed</response>
        [HttpGet("{id}"), Authorize(Roles = "Admin, Student, Foreigner, Staff")]
        public async Task<IActionResult> Read(int id) {
            InitializeQuery();
            Event eventModel = await Query.AsNoTracking()
                                          .FirstOrDefaultAsync(e => e.Id == id);

            if (eventModel == null) { return NotFound("Event Not Found"); }

            Price price = await Services.GetPriceForRoleAsync(CurrentUser.Role.Id, id);
            bool booked = await Services.IsEventBooked(CurrentUser.Id, id);

            return Ok(new {
                Event = eventModel,
                Price = price,
                Booked = booked
            });
        }

        /// <summary>
        /// Update an event by ID
        /// </summary>
        /// <response code="401">User/Admin token is not permitted</response>
        /// <response code="500">if the credential given is not valid or DB update failed</response>
        [HttpPatch("{id}"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update([FromBody] EventUpdatableDeserializer eventDsl,
                                                int id) {
            if (!ModelState.IsValid) { return BadRequest(ModelState); }

            Event @event = await Context.Event.FirstOrDefaultAsync((obj) => obj.Id == id);
            if (@event == null) return NotFound("Event not found");

            Services.Update(eventDsl, @event);

            if (!Services.IsTimeWindowValid(ref @event)) {
                var error = new String[] { "Time window is not valid, ensure close start before open" };
                return BadRequest(new { TimeWindow = error });
            }

            if (@event.RestrictedEvent) {
              Services.AddAndRemoveEventRoles(eventDsl.RestrictedRolesList, @event);
            } else {
              Services.RemoveAllEventRoles(@event.Id);
            }

            try {
                Context.Event.Update(@event);
                await Context.SaveChangesAsync();
            } catch (DbUpdateException e) {
                return BadRequest(e.InnerException.Message);
            }

            return NoContent();
        }

        /// <summary>
        /// Delete an Event
        /// </summary>
        /// <response code="401">User/Admin token is not permitted</response>
        /// <response code="500">if the credential given is not valid or DB update failed</response>
        [HttpDelete("{id}"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id) {
            Event eventModel = await Context.Event.FirstOrDefaultAsync((obj) => obj.Id == id);
            if (eventModel == null) return NotFound("Event Not Found");

            try {
                Context.Event.Remove(eventModel);
                await Context.SaveChangesAsync();
            } catch (DbUpdateException e) {
                return BadRequest(e.InnerException.Message);
            }

            return NoContent();
        }

        #endregion

        #region Get Sub Operations

        /// <summary>
        /// Return a list of Events, parameters from and to can be used to select an interval of events
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /list/{order}
        ///     GET params:
        ///         - From (Date UTC)
        ///         - To (Date UTC)
        ///         - limit (Integer)
        ///         - images (boolean as "true string" = True / False, respect the case)
        ///         - obsolete (boolean as "true string")
        ///         - filters (list of categories as a string "," as a separator)
        ///         - moderators (boolean as true string) include moderators for all events
        ///         - categories (boolean as true string) include events categories
        ///
        /// </remarks>
        /// <param name="order"> ASC, DESC (Default: DESC)</param>
        /// <response code="200">Event + is Booked data</response>
        /// <response code="500">if the credential given is not valid or DB update failed</response>
        [HttpGet("list/{order}"), AllowAnonymous]
        public async Task<IActionResult> GetList(string order) {
            Event[] events;
            InitializeQuery();

            string from = HttpContext.Request.Query["from"];
            string to = HttpContext.Request.Query["to"];
            string limit = HttpContext.Request.Query["limit"];
            string filters = HttpContext.Request.Query["filters"];
            bool obsolete = HttpContext.Request.Query["obsolete"] == bool.TrueString;

            if (order == "ASC")
                Query = Query.OrderBy((Event arg) => arg.OpenAt);
            else
                Query = Query.OrderByDescending((Event arg) => arg.OpenAt);

            if (from != null) Services.FromDate(ref Query, from);

            if (to != null) Services.ToDate(ref Query, to);

            if (!obsolete) Services.NonObsolete(ref Query);

            if (limit != null) Services.LimitElements(ref Query, limit);

            if (filters != null) Services.FilterByCategories(ref Query, filters);

            try {
                events = await Query.AsNoTracking().ToArrayAsync();
            } catch (Exception e) {
                return BadRequest(e.InnerException.Message);
            }

            return Ok(events);
        }

        #endregion

        #region Set Sub Operation

        /// <summary>
        /// Add a Category the An Event
        /// </summary>
        /// <param name="categoryId">The category ID</param>
        /// <param name="eventId">The Event ID</param>
        /// <response code="401">User/Admin token is not permitted</response>
        /// <response code="404">if event or category has not been found</response>
        /// <response code="500">if the credential given is not valid or DB update failed</response>
        [HttpGet("{eventId}/category/{categoryId}"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddCategory(int eventId, int categoryId) {

            if (Context.EventCategory.Any((EventCategory ec) => ec.CategoryId == categoryId && ec.EventId == eventId)) {
                return BadRequest("Bind already exist");
            }

            Event mEvent = await Services.GetEventByIdAsync(eventId);
            Category category = await CategoryServices.GetByIdAsync(categoryId);

            if (mEvent == null || category == null) { return NotFound(); }

            if (category.ParentCategory.HasValue && !await Services.HasCategory(category.ParentCategory.Value, mEvent)) {
                var parentCategory = await CategoryServices.GetByIdAsync((int)category.ParentCategory);
                await Services.AddCategory(parentCategory, mEvent);
            }

            try {
                await Services.AddCategory(category, mEvent);
                await Context.SaveChangesAsync();
            } catch (DbUpdateException e) {
                return BadRequest(e.InnerException.Message);
            }

            return NoContent();
        }

        /// <summary>
        /// Delete a category from the Event
        /// </summary>
        /// <param name="categoryId">The category ID</param>
        /// <param name="eventId">Then Event ID</param>
        /// <response code="401">User/Admin token is not permitted</response>
        /// <response code="500">if the credential given is not valid or DB update failed</response>
        /// <response code="200">If category has been removed</response>
        /// TODO: Remove Parent category or children depending of the request
        [HttpDelete("{eventId}/category/{categoryId}"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(int eventId, int categoryId) {
            Event eventModel = await Context.Event
                                            .Include(inc => inc.EventCategory)
                                            .FirstOrDefaultAsync((Event ev) => ev.Id == eventId);

            if (eventModel == null ||
                eventModel.EventCategory == null ||
                !eventModel.EventCategory.Any(cc => cc.CategoryId == categoryId)) { return NotFound(); }

            EventCategory category = eventModel.EventCategory
                                               .FirstOrDefault(cc => cc.CategoryId == categoryId);

            try {
                Context.EventCategory.Remove(category);
                await Context.SaveChangesAsync();
            } catch (DbUpdateException e) {
                return BadRequest(e.InnerException.Message);
            }

            return Ok("Category removed");
        }

        /// <summary>
        /// Add images to the server.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST
        ///     imagesData = [{
        ///         "fileName": "chaton-diarrhee.jpg",
        ///         "title": "mytitle",
        ///         "alt": "myalt"
        ///         },
        ///         {
        ///             "fileName": "myfilename",
        ///             "title": "mytitle",
        ///             "alt": "myalt"
        ///         }]
        ///
        /// </remarks>
        /// <param name="eventId"></param>
        /// <returns>200</returns>
        [HttpPost("{eventId}/upload/images"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadImages(int eventId,
                                                      [FromServices] IImageServices imageServices,
                                                      [FromForm] ImageUploadDeserializer imageCore,
                                                      CancellationToken cancellationToken) {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            var imagesData = imageCore.GetImagesData();

            Event @event = await Services.GetEventByIdAsync(eventId);
            if (@event == null) return NotFound("Event Not Found");

            IFormFileCollection files = HttpContext.Request.Form.Files;

            string baseFileName = @event.Id.ToString() + "_" +
                                        @event.CreatedAt.ToString("yyyyMMdd");

            try {
                Dictionary<string, string> urls = await imageServices.UploadImageAsync(files, baseFileName, cancellationToken);
                List<Image> rangeUpdate = new List<Image>();

                foreach (var pair in urls) {
                    Image image = new Image() { Url = pair.Key, EventId = @event.Id };
                    var current = imagesData.FirstOrDefault((arg) => arg.FileName == pair.Value);

                    if (current != null) {
                        image.Alt = current.Alt;
                        image.Title = current.Title;
                    }

                    rangeUpdate.Add(image);
                }

                Context.Images.UpdateRange(rangeUpdate);
                await Context.SaveChangesAsync();
            } catch (Exception e) when (e is IOException ||
                                        e is DbUpdateException) {
                string message = (e.InnerException != null) ? e.InnerException.Message :
                                                               e.Message;
                return BadRequest(message);
            }

            return Ok();
        }

        [HttpDelete("{eventId}/delete/image/{imageId}"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteImage(int eventId,
                                                     int imageId,
                                                     [FromServices] IImageServices imageServices) {
            if (!Context.Event.Any((arg) => arg.Id == eventId) ||
                !Context.Images.Any((arg) => arg.ImageId == imageId)) {
                return BadRequest("Item Not Found");
            }

            try {
                Image image = Context.Images
                                     .FirstOrDefault((arg) => arg.ImageId == imageId);
                if (image == null) return BadRequest("Image Not Found");

                imageServices.RemoveImages(image.Url);
                Context.Images.Remove(image);
                Context.SaveChanges();
            } catch (FileNotFoundException e) {
                return BadRequest(e.Message);
            }

            return Ok();
        }

        #endregion
    }
}
