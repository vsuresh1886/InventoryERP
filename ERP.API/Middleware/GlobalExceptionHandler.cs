using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Diagnostics;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Twilio.Exceptions;


namespace ERP.Infrastructure.Repositories
{
    public  class GlobalExceptionHandler: IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
        {
            // Log the full error for debugging in Render logs
            _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

            // Define default error details
            var problemDetails = new ProblemDetails
            {
                Detail = "An unexpected error occurred on the server.",
                Title = "Server Error",
                Status = (int)HttpStatusCode.InternalServerError,
                Instance = httpContext.Request.Path
            };

            // Handle specific exceptions cleanly
            switch (exception)
            {
                case TwilioException twilioEx:
                    // Catch Twilio-specific API errors (e.g., invalid phone format, service down)
                    problemDetails.Title = "SMS/WhatsApp Provider Error";
                    problemDetails.Detail = twilioEx.Message;
                    problemDetails.Status = (int)HttpStatusCode.BadRequest;
                    break;

                case ArgumentException or InvalidOperationException:
                    problemDetails.Title = "Bad Request";
                    problemDetails.Detail = exception.Message;
                    problemDetails.Status = (int)HttpStatusCode.BadRequest;
                    break;

                    // Add more custom exceptions here as your ERP application grows
            }

            // Set response status code and content type
            httpContext.Response.StatusCode = problemDetails.Status;
            httpContext.Response.ContentType = "application/problem+json";

            // Write the clean error payload to the response stream
            var jsonString = System.Text.Json.JsonSerializer.Serialize(problemDetails);
            await httpContext.Response.WriteAsync(jsonString, cancellationToken);

            // Return true to signal that this exception has been handled completely
            return true;
        }

    }
}
