using Microsoft.AspNetCore.Mvc;

namespace TaskManager.Api.Controllers;

/// <summary>
/// Controller responsável por fornecer informações sobre o status da aplicação
/// </summary>
/// <remarks>
/// Este controller implementa endpoints de health check que podem ser utilizados
/// por sistemas de monitoramento, load balancers e orquestradores (como Kubernetes)
/// para verificar se a aplicação está funcionando corretamente.
/// </remarks>
[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Endpoint de health check básico
    /// </summary>
    /// <remarks>
    /// Este endpoint retorna uma resposta simples indicando que a API está online e respondendo.
    /// É útil para:
    /// - Verificações de disponibilidade (liveness probe)
    /// - Monitoramento básico de uptime
    /// - Validação rápida de que o serviço está acessível
    /// 
    /// Não realiza verificações profundas de dependências (banco de dados, cache, etc.)
    /// </remarks>
    /// <returns>Status OK com indicação de que o serviço está funcionando</returns>
    /// <response code="200">Aplicação está funcionando corretamente</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetHealth()
    {
        // Cria um objeto anônimo com a propriedade 'status'
        // Este formato é simples e amplamente utilizado em APIs REST
        var healthResponse = new
        {
            status = "ok"
        };

        // Retorna HTTP 200 OK com o objeto JSON
        // O ASP.NET Core automaticamente serializa o objeto para JSON
        return Ok(healthResponse);
    }

    /// <summary>
    /// Endpoint de health check detalhado (opcional)
    /// </summary>
    /// <remarks>
    /// Este endpoint fornece informações adicionais sobre o estado da aplicação,
    /// incluindo timestamp, versão e outras métricas úteis para diagnóstico.
    /// </remarks>
    /// <returns>Status detalhado da aplicação</returns>
    /// <response code="200">Informações detalhadas sobre o status da aplicação</response>
    [HttpGet("detailed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetDetailedHealth()
    {
        // Cria um objeto com informações mais detalhadas
        // Útil para debugging e monitoramento avançado
        var detailedHealthResponse = new
        {
            status = "ok",
            timestamp = DateTime.UtcNow,
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
            machineName = Environment.MachineName,
            version = "1.0.0"
        };

        return Ok(detailedHealthResponse);
    }
}
