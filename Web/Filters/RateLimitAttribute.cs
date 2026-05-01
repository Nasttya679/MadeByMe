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
            var ipAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrEmpty(ipAddress))
            {
                return;
            }

            var cache = context.HttpContext.RequestServices.GetRequiredService<IMemoryCache>();

            string cacheKey = $"RateLimit_{ipAddress}_{context.ActionDescriptor.DisplayName}";

            if (cache.TryGetValue(cacheKey, out int requestCount))
            {
                if (requestCount >= _requestLimit)
                {
                    Log.Warning("Rate limit exceeded for IP: {IP} on action: {Action}", ipAddress, context.ActionDescriptor.DisplayName);

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