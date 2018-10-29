using System;
using events_planner.Models;
using System.Linq;
using System.Threading.Tasks;
using events_planner.Deserializers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace events_planner.Services {
    public class JuryPointServices : IJuryPointServices {
        
        private PlannerContext Context { get; }

        public JuryPointServices(PlannerContext context) {
            Context = context;
        }

        public async Task<JuryPoint> CreateJuryPointAsync(float points, string desc, int userId, int? eventId) {
            JuryPoint juryPoint = new JuryPoint {
                Points = points,
                Description = desc,
                UserId = userId,
                EventId = eventId
            };
            await Context.JuryPoints.AddAsync(juryPoint);
            return juryPoint;
        }

        public async Task<JuryPoint> GetJuryPointAsync(int juryPoint) {
            return await Context.JuryPoints.FirstOrDefaultAsync(j => j.Id == juryPoint);
        }

        public async Task<JuryPoint[]> GetJuryPointsAsync(int userId) {
            return await Context.JuryPoints.Where(j => j.UserId == userId).ToArrayAsync();
        }
        
        public async Task<JuryPoint> GetJuryPointAsync(int userId, int eventId) {
            return await Context.JuryPoints.FirstOrDefaultAsync(j => j.EventId == eventId && j.UserId == userId);
        }

        public JuryPoint GetJuryPointEpsilon(int userId, float points) {
            double epsilon = 0.01;
            return Context.JuryPoints.FirstOrDefault(jp => jp.UserId == userId &&
                                                           (jp.Points - points) < epsilon);
        }

        public async Task<JuryPoint> UpdateJuryPoint(JuryPointUpdateDeserializer updateDsl) {
            var jp = await GetJuryPointAsync(updateDsl.Id);
            if (jp == null) return null;

            if (!string.IsNullOrEmpty(updateDsl.Description))
                jp.Description = updateDsl.Description;
            if (updateDsl.Points.HasValue)
                jp.Points = (float)updateDsl.Points;

            Context.JuryPoints.Update(jp);
            return jp;
        }

        public void RemoveJuryPoints(JuryPoint juryPoint, bool throwIfEventLinked = false) {
            if (throwIfEventLinked && juryPoint.EventId.HasValue) {
                throw new ArgumentException("Jury point with event id linked cannot be deleted");
            }
            if (juryPoint != null)
                Context.Remove(juryPoint);
        }

        public async Task<int> SaveAsync() {
            return await Context.SaveChangesAsync();
        }
    }
}