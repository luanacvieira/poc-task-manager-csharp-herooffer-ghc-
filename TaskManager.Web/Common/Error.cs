namespace TaskManager.Web.Common;

/// <summary>
/// Representa um erro com código e mensagem
/// </summary>
public class Error
{
    public string Code { get; }
    public string Message { get; }
    public Dictionary<string, string[]>? ValidationErrors { get; }

    public Error(string code, string message, Dictionary<string, string[]>? validationErrors = null)
    {
        Code = code;
        Message = message;
        ValidationErrors = validationErrors;
    }

    public static Error NotFound(string entity, object id) =>
        new("NotFound", $"{entity} com ID '{id}' não foi encontrado.");

    public static Error Validation(string message, Dictionary<string, string[]>? errors = null) =>
        new("ValidationError", message, errors);

    public static Error Conflict(string message) =>
        new("Conflict", message);

    public static Error Internal(string message = "Ocorreu um erro interno no servidor.") =>
        new("InternalError", message);

    public static Error Unauthorized(string message = "Não autorizado.") =>
        new("Unauthorized", message);

    public static Error Forbidden(string message = "Acesso negado.") =>
        new("Forbidden", message);
}
