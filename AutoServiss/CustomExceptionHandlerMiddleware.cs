using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace AutoServiss
{
    public sealed class CustomExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public CustomExceptionHandlerMiddleware(
            RequestDelegate next,
            ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<CustomExceptionHandlerMiddleware>();
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                try
                {
                    // Do custom stuff
                    // Could be just as simple as calling _logger.LogError
                    _logger.LogError(ex.Message);
                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(new
                    {
                        message = "Kaut kas ir nogājis greizi :("
                    }));                
                    // if you don't want to rethrow the original exception
                    // then call return:
                    return;
                }
                catch (Exception ex2)
                {
                    _logger.LogError(ex2.Message);
                }

                // Otherwise this handler will
                // re -throw the original exception
                throw;
            }
        }
    }
}
