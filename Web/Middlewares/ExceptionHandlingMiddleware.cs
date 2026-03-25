using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MadeByMe.Web.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            Console.WriteLine($"[Глобальна помилка]: {exception.Message}");

            context.Response.Redirect("/Home/Error");

            return Task.CompletedTask;
        }
    }
}