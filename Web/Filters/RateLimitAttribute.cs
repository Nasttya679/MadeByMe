using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Serilog;

namespace MadeByMe.Web.Filters
{
    public class RateLimitAttribute : ActionFilterAttribute
    {
        private readonly int _requestLimit;

        public RateLimitAttribute(int requestLimit)
        {
            _requestLimit = requestLimit;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var ipAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown_ip";

            var userId = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var userAgent = context.HttpContext.Request.Headers["User-Agent"].ToString();

            string clientIdentifier = !string.IsNullOrEmpty(userId) ? userId : $"{ipAddress}_{userAgent}";

            string cacheKey = $"RateLimit_{clientIdentifier}_{context.ActionDescriptor.DisplayName}";

            var cache = context.HttpContext.RequestServices.GetRequiredService<IMemoryCache>();

            if (cache.TryGetValue(cacheKey, out int requestCount))
            {
                if (requestCount >= _requestLimit)
                {
                    Log.Warning(
                        "Rate limit exceeded for Client: {Client} on action: {Action}", clientIdentifier, context.ActionDescriptor.DisplayName);

                    context.Result = new RedirectToActionResult("TooManyRequests", "Home", null);
                    return;
                }

                cache.Set(cacheKey, requestCount + 1, TimeSpan.FromMinutes(1));
            }
            else
            {
                cache.Set(cacheKey, 1, TimeSpan.FromMinutes(1));
            }

            base.OnActionExecuting(context);
        }
    }
}