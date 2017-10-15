using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

                    var controller = exc.TargetSite.DeclaringType.Name; // klase
                    var method = exc.TargetSite.Name; // metode

                    var exceptionType = exc.GetType();
                    if (exceptionType == typeof(BadRequestException))
                    {
                        _logger.LogWarning($"[{controller}] [{method}] {exc.Message}");
                        statusCode = 400;
                        errorMessage = exc.Message;
                    }
                    else
                    {
                        _logger.LogError($"[{controller}] [{method}] {exc.Message}");
                        if(exc.InnerException != null)
                        {
                            _logger.LogError($"[{controller}] [{method}] {exc.InnerException.Message}");
                        }
                    }

                    context.Response.StatusCode = statusCode;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(new
                    {
                        messages = new List<string> { errorMessage }
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
