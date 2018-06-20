using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace events_planner.Utils {

    public class ExceptionHandler {

        private RequestDelegate Next { get; set; }

        public ExceptionHandler(RequestDelegate next) {
            Next = next;
        }

        public ExceptionHandler() {}

        public async Task Invoke(HttpContext context,
                                 IHostingEnvironment environment) {
            Contract.Ensures(Contract.Result<Task>() != null);

            try {
                await Next.Invoke(context);
            } catch (Exception error) {
                string userError = null;

                if (error.GetType() == typeof(NotFoundUserException)) {
                    context.Response.StatusCode = 401;
                    userError = error.Message;
                } else {
                    context.Response.StatusCode = 500;
                }

                using (var writter = new StreamWriter(context.Response.Body)) {
                    // Set the handler for previous Middleware
                    var handler = context.Features.Get<IExceptionHandlerFeature>();

                    if (handler == null) {
                        context.Features
                               .Set<IExceptionHandlerFeature>(
                                   new JsonExceptionHandlerFeature(error)
                                  );
                    }

                    // Serialize the Error in JSon format using stream.
                    if (environment.IsDevelopment()) {
                        new JsonSerializer().Serialize(writter, error);
                    } else {
                        new JsonSerializer().Serialize(writter, userError ?? "Internal Error");
                    }

                    await writter.FlushAsync().ConfigureAwait(false);
                }
            }
        }
    }

    public static class ExceptionHandlerExtension {

        public static IApplicationBuilder UseJsonExceptionHandler(this IApplicationBuilder builder) {
            return builder.UseMiddleware<ExceptionHandler>();
        }

    }

    public class JsonExceptionHandlerFeature : IExceptionHandlerFeature {

        Exception _exception;
        Exception IExceptionHandlerFeature.Error => _exception;

        public JsonExceptionHandlerFeature(Exception exception) {
            _exception = exception;
        }
    }
}