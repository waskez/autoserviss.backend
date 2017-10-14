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
            catch (Exception exc)
            {
                try
                {
                    var statusCode = 500;
                    var errorMessage = "Kaut kas ir nogājis greizi :(";

                    var source = exc.TargetSite.DeclaringType.FullName;

                    var exceptionType = exc.GetType();
                    if (exceptionType == typeof(CustomException))
                    {
                        _logger.LogWarning($"[{source}] {exc.Message}");
                        statusCode = 400;
                        errorMessage = exc.Message;
                    }
                    else
                    {
                        _logger.LogError($"[{source}] {exc.Message}");
                        if(exc.InnerException != null)
                        {
                            _logger.LogError($"[{source}] {exc.InnerException.Message}");
                        }
                    }

                    context.Response.StatusCode = statusCode;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(new
                    {
                        message = errorMessage
                    }));                
                    // if you don't want to rethrow the original exception
                    // then call return:
                    return;
                }
                catch (Exception exc2)
                {
                    _logger.LogError(exc2.Message);
                }

                // Otherwise this handler will
                // re -throw the original exception
                throw;
            }
        }
    }
}
