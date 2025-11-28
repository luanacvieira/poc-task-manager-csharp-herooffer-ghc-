namespace TaskManager.Web.Common;

/// <summary>
/// Representa o resultado de uma operação que pode ser bem-sucedida ou falhar
/// </summary>
/// <typeparam name="T">O tipo do valor retornado em caso de sucesso</typeparam>
public class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public Error? Error { get; }

    private Result(bool isSuccess, T? value, Error? error)
    {
        if (isSuccess && error != null)
            throw new InvalidOperationException("Successful result cannot have an error");
        if (!isSuccess && error == null)
            throw new InvalidOperationException("Failed result must have an error");

        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    /// <summary>
    /// Cria um resultado de sucesso
    /// </summary>
    public static Result<T> Success(T value) => new(true, value, null);

    /// <summary>
    /// Cria um resultado de falha
    /// </summary>
    public static Result<T> Failure(Error error) => new(false, default, error);

    /// <summary>
    /// Cria um resultado de falha com mensagem simples
    /// </summary>
    public static Result<T> Failure(string code, string message) => 
        new(false, default, new Error(code, message));
}

/// <summary>
/// Representa um resultado de operação sem valor de retorno
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error? Error { get; }

    private Result(bool isSuccess, Error? error)
    {
        if (isSuccess && error != null)
            throw new InvalidOperationException("Successful result cannot have an error");
        if (!isSuccess && error == null)
            throw new InvalidOperationException("Failed result must have an error");

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Cria um resultado de sucesso
    /// </summary>
    public static Result Success() => new(true, null);

    /// <summary>
    /// Cria um resultado de falha
    /// </summary>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>
    /// Cria um resultado de falha com mensagem simples
    /// </summary>
    public static Result Failure(string code, string message) => 
        new(false, new Error(code, message));
}
