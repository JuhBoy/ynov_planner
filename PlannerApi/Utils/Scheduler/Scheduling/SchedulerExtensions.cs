using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace events_planner.Scheduler.Extensions {
    
    public static class SchedulerExtensions {
        
        public static IServiceCollection AddScheduler(this IServiceCollection services) {
            return services.AddSingleton<IHostedService, SchedulerHostedService>();
        }

        public static IServiceCollection AddScheduler(this IServiceCollection services, EventHandler<UnobservedTaskExceptionEventArgs> unobservedTaskExceptionHandler) {
            return services.AddSingleton<IHostedService, SchedulerHostedService>(serviceProvider => {
                var instance = new SchedulerHostedService(serviceProvider.GetServices<IScheduledTask>());
                instance.UnobservedTaskException += unobservedTaskExceptionHandler;
                return instance;
            });
        }

        public static void PrettyPrint(this IScheduledTask scheduledTask, string message, ConsoleColor color = ConsoleColor.White) {
            Console.ForegroundColor = color;
            Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] {message}");
            Console.ResetColor();
        }
    }
}