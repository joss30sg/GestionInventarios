using System.Net;
using System.Text.Json;
using InventoryManagementAPI.Api.Exceptions;

namespace InventoryManagementAPI.Api.Middleware;

/// <summary>
/// Middleware global para manejar todas las excepciones no capturadas
/// Proporciona respuestas JSON consistentes sin exponer detalles internos
/// </summary>
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
            _logger.LogError(ex, "[ERROR] Excepción no manejada en {Path}: {Message}", context.Request.Path, ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// Maneja la excepción y construye una respuesta JSON apropiada
    /// Mapea tipos de excepciones a status codes HTTP estándar
    /// </summary>
    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        // Manejar excepciones personalizadas de la API
        if (exception is ApiException apiException)
        {
            context.Response.StatusCode = apiException.StatusCode;

            object errorResponse = apiException switch
            {
                ValidationException validationEx => new
                {
                    success = false,
                    message = apiException.Message,
                    code = apiException.ErrorCode,
                    statusCode = apiException.StatusCode,
                    errors = validationEx.Errors,
                    timestamp = DateTime.UtcNow,
                    traceId = context.TraceIdentifier
                },
                InsufficientStockException stockEx => new
                {
                    success = false,
                    message = apiException.Message,
                    code = apiException.ErrorCode,
                    statusCode = apiException.StatusCode,
                    details = new
                    {
                        productId = stockEx.ProductId,
                        requested = stockEx.Requested,
                        available = stockEx.Available
                    },
                    timestamp = DateTime.UtcNow,
                    traceId = context.TraceIdentifier
                },
                _ => new
                {
                    success = false,
                    message = apiException.Message,
                    code = apiException.ErrorCode,
                    statusCode = apiException.StatusCode,
                    timestamp = DateTime.UtcNow,
                    traceId = context.TraceIdentifier
                }
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
        }

        // Manejar excepciones genéricas
        var (statusCode, message, errorCode) = exception switch
        {
            // Validación
            ArgumentNullException => (StatusCodes.Status400BadRequest, "Faltan datos requeridos", "ARGUMENT_NULL"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Argumento inválido", "ARGUMENT_INVALID"),
            
            // Acceso denegado
            UnauthorizedAccessException => (StatusCodes.Status403Forbidden, "Acceso denegado", "UNAUTHORIZED_ACCESS"),
            
            // Recurso no encontrado (genérico)
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Recurso no encontrado", "NOT_FOUND"),
            
            // Conflicto de datos
            InvalidOperationException ex when ex.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase)
                => (StatusCodes.Status409Conflict, "Conflicto con datos existentes", "DUPLICATE_ENTRY"),
            
            InvalidOperationException ex when ex.Message.Contains("stock", StringComparison.OrdinalIgnoreCase)
                => (StatusCodes.Status409Conflict, "Stock insuficiente", "INSUFFICIENT_STOCK"),
            
            InvalidOperationException => (StatusCodes.Status400BadRequest, "Operación inválida", "INVALID_OPERATION"),
            
            // Por defecto
            _ => (StatusCodes.Status500InternalServerError, "Error interno del servidor", "INTERNAL_SERVER_ERROR")
        };

        context.Response.StatusCode = statusCode;

        var genericErrorResponse = new
        {
            success = false,
            message = message,
            code = errorCode,
            statusCode = statusCode,
            timestamp = DateTime.UtcNow,
            traceId = context.TraceIdentifier
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(genericErrorResponse));
    }
}

public static class ExceptionHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }
}
