using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace MadeByMe.Web.Middlewares
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var method = context.Request.Method;
            var url = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}";
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "Невідомо";

            var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Не авторизований";

            // var headers = string.Join(" | ", context.Request.Headers.Select(h => $"{h.Key}: {h.Value}"));
            var headers = string.Join(" | ", context.Request.Headers
                .Where(h => h.Key != "Cookie")
                .Select(h => $"{h.Key}: {h.Value}"));

            string bodyAsText = string.Empty;

            context.Request.EnableBuffering();

            using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true))
            {
                bodyAsText = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;
            }

            Log.Information(
                "\n========== ІНФОРМАЦІЯ ПРО ЗАПИТ ==========\n" +
                "Метод: {Method}\n" +
                "URL: {Url}\n" +
                "IP Адреса: {Ip}\n" +
                "User ID: {UserId}\n" +
                "Хедери: {Headers}\n" +
                "Тіло запиту: {Body}\n" +
                "=============================================",
                method, url, ip, userId, headers, string.IsNullOrEmpty(bodyAsText) ? "[Порожнє]" : bodyAsText);

            await _next(context);
        }
    }
}