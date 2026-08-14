using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceDeskSystem.Api.Models;
using ServiceDeskSystem.Application.Services.Tickets;
using ServiceDeskSystem.Application.Services.Tickets.Models;

namespace ServiceDeskSystem.Api.Controllers;

/// <summary>
/// Provides advanced analytics, KPIs, trends, and workload metrics for the Service Desk system.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Developer")]
public sealed class AnalyticsController(
    ITicketStatisticsService statisticsService,
    ILogger<AnalyticsController> logger) : ControllerBase
{
    /// <summary>
    /// Gets high-level ticket status and priority distribution counts.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> containing the status and priority distribution counts.</returns>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary()
    {
        var statusCounts = await statisticsService.GetTicketCountByStatusAsync().ConfigureAwait(false);
        var priorityCounts = await statisticsService.GetTicketCountByPriorityAsync().ConfigureAwait(false);
        var topDevs = await statisticsService.GetTopDevelopersAsync(5).ConfigureAwait(false);

        return Ok(new
        {
            StatusCounts = statusCounts,
            PriorityCounts = priorityCounts,
            TopDevelopers = topDevs.Select(d => new { d.Login, d.Count })
        });
    }

    /// <summary>
    /// Gets extended analytics including trends over time, developer workloads, product resolution metrics, and tag distributions.
    /// </summary>
    /// <param name="days">Number of past days for trend analysis (default 30).</param>
    /// <returns>An <see cref="IActionResult"/> containing the <see cref="ExtendedAnalyticsDto"/>.</returns>
    [HttpGet("extended")]
    [ProducesResponseType(typeof(ExtendedAnalyticsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExtended([FromQuery] int days = 30)
    {
        if (days < 1) days = 1;
        if (days > 365) days = 365;

        logger.LogInformation("Retrieving extended analytics for last {Days} days", days);
        var result = await statisticsService.GetExtendedAnalyticsAsync(days).ConfigureAwait(false);
        return Ok(result);
    }
}
