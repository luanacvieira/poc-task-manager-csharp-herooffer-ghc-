namespace TaskManager.Web.Common;

/// <summary>
/// Parâmetros de consulta para paginação, filtros e ordenação
/// </summary>
public class QueryParameters
{
    private const int MaxPageSize = 100;
    private int _pageSize = 10;

    /// <summary>
    /// Número da página (começa em 1)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Tamanho da página (máximo 100)
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }

    /// <summary>
    /// Campo para ordenação
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Direção da ordenação (asc ou desc)
    /// </summary>
    public string SortDirection { get; set; } = "asc";

    /// <summary>
    /// Filtro por título (pesquisa parcial)
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Filtro por prioridade
    /// </summary>
    public string? Priority { get; set; }

    /// <summary>
    /// Filtro por categoria
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Filtro por status de conclusão
    /// </summary>
    public bool? Completed { get; set; }

    /// <summary>
    /// Filtro por usuário
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Filtro por atribuição
    /// </summary>
    public string? AssignedTo { get; set; }

    /// <summary>
    /// Filtro por data de vencimento inicial
    /// </summary>
    public DateTime? DueDateFrom { get; set; }

    /// <summary>
    /// Filtro por data de vencimento final
    /// </summary>
    public DateTime? DueDateTo { get; set; }

    /// <summary>
    /// Filtro por tag
    /// </summary>
    public string? Tag { get; set; }
}
