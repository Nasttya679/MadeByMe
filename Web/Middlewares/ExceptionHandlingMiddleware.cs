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

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            Log.Error(exception, "Сталася необроблена помилка під час виконання запиту {Path}. Повідомлення: {Message}", context.Request.Path, exception.Message);

            if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"success\": false, \"message\": \"Внутрішня помилка сервера.\"}");
            }
            else
            {
                context.Response.Redirect("/Home/Error");
            }
        }
    }
}