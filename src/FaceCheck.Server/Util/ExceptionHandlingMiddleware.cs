using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;

namespace FaceCheck.Server.Util
{
    /// <summary>
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <param name="next"> </param>
    /// <param name="loggerFactory"> </param>
    public class ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILoggerFactory loggerFactory)
    {
        private readonly RequestDelegate _next = next;
        private ILogger Logger { get; } = loggerFactory.CreateLogger<ExceptionHandlingMiddleware>();

        /// <summary>
        /// </summary>
        /// <param name="context"> </param>
        /// <returns> </returns>
        public async Task Invoke(HttpContext context)
        {
            string connStr = $"Remote:{context.Connection.RemoteIpAddress}:{context.Connection.RemotePort} Local:{context.Connection.LocalIpAddress}:{context.Connection.LocalPort}";
            if ("GET".Equals(context.Request.Method, StringComparison.OrdinalIgnoreCase))
            {
                if (!context.Request.QueryString.HasValue
                    || string.IsNullOrWhiteSpace(context.Request.QueryString.Value))
                {
                    Logger.LogInformation($"{connStr} Method:GET {context.Request.Path.Value}");
                }
                else
                {
                    Logger.LogInformation($"{connStr} Method:GET {context.Request.Path.Value}?{context.Request.QueryString}");
                }
            }
            else
            {
                Logger.LogInformation($"{connStr} Method:{context.Request.Method}  {context.Request.Path.Value}");
            }
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleException(context, ex);
            }
        }

        private Task HandleException(HttpContext context, Exception exception)
        {
            Logger.LogError(exception, exception.Message);

            if (!context.Response.HasStarted)
            {
                context.Response.ContentType = MediaTypeNames.Application.Json;
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
            return context.Response.WriteAsync($"{{\"success\":false,\"message\":\"{exception.Message}\"}}");
        }
    }

    /// <summary>
    /// </summary>
    public static class ExceptionHandlingMiddlewareExtensions
    {
        /// <summary>
        /// </summary>
        /// <param name="builder"> </param>
        /// <returns> </returns>
        public static IApplicationBuilder UseExceptionHandlingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}