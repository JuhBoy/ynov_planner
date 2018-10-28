using System.Threading.Tasks;
using events_planner.Deserializers;
using events_planner.Models;
using Microsoft.AspNetCore.Mvc;

namespace events_planner.Services {
    public interface IJuryPointServices {
        Task<JuryPoint> CreateJuryPointAsync(float points, string desc, int userId, int? eventId);
        Task<JuryPoint> GetJuryPointAsync(int juryPoint);
        Task<JuryPoint> GetJuryPointAsync(int userId, int eventId);
        JuryPoint GetJuryPointEpsilon(int userId, float points);
        Task<JuryPoint[]> GetJuryPointsAsync(int userId);
        Task<JuryPoint> UpdateJuryPoint(JuryPointUpdateDeserializer updateDsl);
        void RemoveJuryPoints(JuryPoint juryPoint, bool throwIfEventLinked = false);
        Task<int> SaveAsync();
    }
}