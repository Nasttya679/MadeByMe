using Serilog;

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
            Log.Error(exception, "Сталася необроблена помилка під час виконання запиту {Path}. Повідомлення: {Message}", context.Request.Path, exception.Message);

            context.Response.Redirect("/Home/Error");

            return Task.CompletedTask;
        }
    }
}