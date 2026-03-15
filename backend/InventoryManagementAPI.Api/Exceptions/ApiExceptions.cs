namespace InventoryManagementAPI.Api.Exceptions;

/// <summary>
/// Excepción base para excepciones de la API
/// </summary>
public abstract class ApiException : Exception
{
    public virtual int StatusCode { get; protected set; } = 500;
    public string? ErrorCode { get; protected set; }

    protected ApiException(string message, string? errorCode = null) : base(message)
    {
        ErrorCode = errorCode;
    }

    protected ApiException(string message, Exception innerException, string? errorCode = null) 
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}

/// <summary>
/// Excepción cuando no se encuentre un recurso
/// </summary>
public class ResourceNotFoundException : ApiException
{
    public override int StatusCode => 404;

    public ResourceNotFoundException(string resourceName, int id) 
        : base($"El recurso '{resourceName}' con ID {id} no fue encontrado", "RESOURCE_NOT_FOUND")
    {
    }

    public ResourceNotFoundException(string message) 
        : base(message, "RESOURCE_NOT_FOUND")
    {
    }
}

/// <summary>
/// Excepción cuando hay datos duplicados
/// </summary>
public class DuplicateEntryException : ApiException
{
    public override int StatusCode => 409;

    public DuplicateEntryException(string fieldName, string fieldValue) 
        : base($"El {fieldName} '{fieldValue}' ya existe en el sistema", "DUPLICATE_ENTRY")
    {
    }

    public DuplicateEntryException(string message) 
        : base(message, "DUPLICATE_ENTRY")
    {
    }
}

/// <summary>
/// Excepción cuando hay stock insuficiente
/// </summary>
public class InsufficientStockException : ApiException
{
    public override int StatusCode => 409;
    
    public int Requested { get; }
    public int Available { get; }
    public int ProductId { get; }

    public InsufficientStockException(int productId, int requested, int available) 
        : base($"Stock insuficiente para el producto {productId}. Solicitado: {requested}, Disponible: {available}", "INSUFFICIENT_STOCK")
    {
        ProductId = productId;
        Requested = requested;
        Available = available;
    }
}

/// <summary>
/// Excepción cuando la validación de negocio falla
/// </summary>
public class BusinessRuleException : ApiException
{
    public override int StatusCode => 422;

    public BusinessRuleException(string message) 
        : base(message, "BUSINESS_RULE_VIOLATION")
    {
    }

    public BusinessRuleException(string message, string errorCode) 
        : base(message, errorCode)
    {
    }
}

/// <summary>
/// Excepción cuando no hay autorización
/// </summary>
public class UnauthorizedException : ApiException
{
    public override int StatusCode => 401;

    public UnauthorizedException(string message = "No autorizado") 
        : base(message, "UNAUTHORIZED")
    {
    }
}

/// <summary>
/// Excepción cuando no hay permiso
/// </summary>
public class ForbiddenException : ApiException
{
    public override int StatusCode => 403;

    public ForbiddenException(string message = "Acceso denegado") 
        : base(message, "FORBIDDEN")
    {
    }
}

/// <summary>
/// Excepción cuando hay validación de datos  
/// </summary>
public class ValidationException : ApiException
{
    public override int StatusCode => 400;
    
    public Dictionary<string, List<string>> Errors { get; }

    public ValidationException(string message) 
        : base(message, "VALIDATION_ERROR")
    {
        Errors = new Dictionary<string, List<string>>();
    }

    public ValidationException(Dictionary<string, List<string>> errors) 
        : base("Validación fallida", "VALIDATION_ERROR")
    {
        Errors = errors;
    }

    public void AddError(string field, string errorMessage)
    {
        if (!Errors.ContainsKey(field))
        {
            Errors[field] = new List<string>();
        }
        Errors[field].Add(errorMessage);
    }
}
