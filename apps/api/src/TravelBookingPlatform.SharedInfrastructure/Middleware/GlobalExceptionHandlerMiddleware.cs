using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using TravelBookingPlatform.Core.Domain.Exceptions;

namespace TravelBookingPlatform.SharedInfrastructure.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception has occurred while executing the request");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Check if response has already started
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.ContentType = "application/json";

        int statusCode;
        object response;

        switch (exception)
        {
            case ValidationException validationException:
                statusCode = (int)HttpStatusCode.BadRequest;
                response = new
                {
                    message = "Validation failed",
                    errors = validationException.Errors.Select(e => new
                    {
                        property = e.PropertyName,
                        error = e.ErrorMessage
                    }).ToArray()
                };
                break;

            case BusinessValidationException businessException:
                statusCode = (int)HttpStatusCode.BadRequest;
                response = new
                {
                    message = "Validation failed",
                    errors = new[]
                    {
                        new
                        {
                            property = businessException.PropertyName ?? "General",
                            error = businessException.Message
                        }
                    }
                };
                break;

            case InvalidOperationException invalidOperationException:
                statusCode = (int)HttpStatusCode.BadRequest;
                response = new
                {
                    message = "Validation failed",
                    errors = new[]
                    {
                        new
                        {
                            property = "General",
                            error = invalidOperationException.Message
                        }
                    }
                };
                break;

            case ArgumentException argumentException:
                statusCode = (int)HttpStatusCode.BadRequest;
                response = new
                {
                    message = "Validation failed",
                    errors = new[]
                    {
                        new
                        {
                            property = argumentException.ParamName ?? "General",
                            error = argumentException.Message
                        }
                    }
                };
                break;

            case NotFoundException notFoundException:
                statusCode = (int)HttpStatusCode.NotFound;
                response = new
                {
                    message = "Resource not found",
                    errors = new[]
                    {
                        new
                        {
                            property = "General",
                            error = notFoundException.Message
                        }
                    }
                };
                break;

            case ForeignKeyViolationException foreignKeyException:
                statusCode = (int)HttpStatusCode.BadRequest;
                response = new
                {
                    message = "Validation failed",
                    errors = new[]
                    {
                        new
                        {
                            property = foreignKeyException.ForeignKeyName ?? "General",
                            error = foreignKeyException.Message
                        }
                    }
                };
                break;

            case UnauthorizedAccessException unauthorizedException:
                statusCode = (int)HttpStatusCode.Unauthorized;
                response = new
                {
                    message = "Authentication failed",
                    errors = new[]
                    {
                        new
                        {
                            property = "General",
                            error = unauthorizedException.Message
                        }
                    }
                };
                break;

            case DbUpdateException dbException when dbException.InnerException?.Message.Contains("FOREIGN KEY constraint") == true:
                statusCode = (int)HttpStatusCode.BadRequest;
                response = new
                {
                    message = "Validation failed",
                    errors = new[]
                    {
                        new
                        {
                            property = "General",
                            error = "The operation failed because one or more referenced entities do not exist."
                        }
                    }
                };
                break;

            case DbUpdateException dbException when dbException.InnerException?.Message.Contains("UNIQUE constraint") == true:
                statusCode = (int)HttpStatusCode.Conflict;
                response = new
                {
                    message = "Validation failed",
                    errors = new[]
                    {
                        new
                        {
                            property = "General",
                            error = "The operation failed because it would create a duplicate entry."
                        }
                    }
                };
                break;

            default:
                statusCode = (int)HttpStatusCode.InternalServerError;
                response = new
                {
                    message = "An error occurred",
                    errors = new[]
                    {
                        new
                        {
                            property = "General",
                            error = "An unexpected error occurred."
                        }
                    }
                };
                break;
        }

        context.Response.StatusCode = statusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        var jsonResponse = JsonSerializer.Serialize(response, options);
        await context.Response.WriteAsync(jsonResponse);
    }
}