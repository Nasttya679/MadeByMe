using System;
using System.Diagnostics;
using System.Threading.Tasks;
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

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Сталася помилка під час обробки запиту: [{Method}] {Url}",
                    context.Request.Method,
                    context.Request.Path);

                throw;
            }
            finally
            {
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
}