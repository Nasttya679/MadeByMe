using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace MadeByMe.Web.Middlewares
{
    public class ExecutionTimeMiddleware
    {
        private readonly RequestDelegate _next;

        public ExecutionTimeMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            await _next(context);

            stopwatch.Stop();
            var executionTime = stopwatch.ElapsedMilliseconds;

            Log.Information(
                " -- Запит до [{Method}] {Url} виконано за {ExecutionTime} мс --",
                context.Request.Method,
                context.Request.Path,
                executionTime);
        }
    }
}