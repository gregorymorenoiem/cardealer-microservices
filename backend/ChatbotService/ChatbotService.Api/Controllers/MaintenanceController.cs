using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using ChatbotService.Application.DTOs;
using ChatbotService.Application.Features.Maintenance.Commands;
using ChatbotService.Domain.Interfaces;
using ChatbotService.Domain.Models;

namespace ChatbotService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class MaintenanceController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<MaintenanceController> _logger;
    private readonly IMaintenanceTaskRepository _maintenanceRepository;
    private readonly IHealthMonitoringService _healthService;
    private readonly IReportingService _reportingService;
    private readonly IUnansweredQuestionRepository _unansweredRepository;

    public MaintenanceController(
        IMediator mediator,
        ILogger<MaintenanceController> logger,
        IMaintenanceTaskRepository maintenanceRepository,
        IHealthMonitoringService healthService,
        IReportingService reportingService,
        IUnansweredQuestionRepository unansweredRepository)
    {
        _mediator = mediator;
        _logger = logger;
        _maintenanceRepository = maintenanceRepository;
        _healthService = healthService;
        _reportingService = reportingService;
        _unansweredRepository = unansweredRepository;
    }

    /// <summary>
    /// Get maintenance tasks for a configuration
    /// </summary>
    [HttpGet("{configurationId:guid}/tasks")]
    [ProducesResponseType(typeof(IEnumerable<MaintenanceTaskDto>), 200)]
    public async Task<IActionResult> GetTasks(Guid configurationId, CancellationToken cancellationToken)
    {
        try
        {
            var tasks = await _maintenanceRepository.GetByConfigurationIdAsync(configurationId, cancellationToken);

            var dtos = tasks.Select(t => new MaintenanceTaskDto
            {
                Id = t.Id,
                TaskType = t.TaskType,
                Name = t.Name,
                Description = t.Description,
                CronExpression = t.CronExpression,
                IsEnabled = t.IsEnabled,
                Status = t.Status,
                LastRunAt = t.LastRunAt,
                NextRunAt = t.NextRunAt,
                LastRunSuccess = t.LastRunSuccess,
                LastRunResult = t.LastRunResult,
                TotalExecutions = t.TotalExecutions,
                SuccessfulExecutions = t.SuccessfulExecutions,
                AvgExecutionTimeMs = t.AvgExecutionTimeMs
            });

            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting maintenance tasks");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Run a maintenance task manually
    /// </summary>
    [HttpPost("tasks/{taskId:guid}/run")]
    [ProducesResponseType(typeof(MaintenanceTaskResultDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> RunTask(Guid taskId, [FromQuery] bool force = false, CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new RunMaintenanceTaskCommand(taskId, force, "Manual execution from API");
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Task not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running maintenance task {TaskId}", taskId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Toggle maintenance task enabled/disabled
    /// </summary>
    [HttpPut("tasks/{taskId:guid}/toggle")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ToggleTask(Guid taskId, [FromQuery] bool enabled, CancellationToken cancellationToken)
    {
        try
        {
            var task = await _maintenanceRepository.GetByIdAsync(taskId, cancellationToken);
            if (task == null)
            {
                return NotFound(new { error = "Task not found" });
            }

            task.IsEnabled = enabled;
            task.UpdatedAt = DateTime.UtcNow;

            await _maintenanceRepository.UpdateAsync(task, cancellationToken);

            return Ok(new { message = $"Task {(enabled ? "enabled" : "disabled")}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling maintenance task {TaskId}", taskId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get health report
    /// </summary>
    [HttpGet("health/{configurationId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ChatbotHealthReport), 200)]
    public async Task<IActionResult> GetHealthReport(Guid configurationId, CancellationToken cancellationToken)
    {
        try
        {
            var report = await _healthService.GenerateHealthReportAsync(configurationId, cancellationToken);
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting health report");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get active alerts
    /// </summary>
    [HttpGet("alerts/{configurationId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<HealthAlert>), 200)]
    public async Task<IActionResult> GetAlerts(Guid configurationId, CancellationToken cancellationToken)
    {
        try
        {
            var alerts = await _healthService.GetActiveAlertsAsync(configurationId, cancellationToken);
            return Ok(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting alerts");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Sync inventory
    /// </summary>
    [HttpPost("sync/{configurationId:guid}")]
    [ProducesResponseType(typeof(InventorySyncResultDto), 200)]
    public async Task<IActionResult> SyncInventory(Guid configurationId, CancellationToken cancellationToken)
    {
        try
        {
            var command = new SyncInventoryCommand(configurationId);
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing inventory");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Run auto-learning analysis
    /// </summary>
    [HttpPost("learning/{configurationId:guid}")]
    [ProducesResponseType(typeof(AutoLearningResultDto), 200)]
    public async Task<IActionResult> RunAutoLearning(Guid configurationId, CancellationToken cancellationToken)
    {
        try
        {
            var command = new RunAutoLearningCommand(configurationId);
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running auto-learning");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Generate report
    /// </summary>
    [HttpPost("reports/{configurationId:guid}")]
    [ProducesResponseType(typeof(GenerateReportResultDto), 200)]
    public async Task<IActionResult> GenerateReport(
        Guid configurationId,
        [FromQuery] ReportType reportType = ReportType.Daily,
        [FromQuery] bool sendEmail = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new GenerateReportCommand(configurationId, reportType, DateTime.UtcNow, sendEmail, null);
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating report");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get cost analysis
    /// </summary>
    [HttpGet("costs/{configurationId:guid}")]
    [ProducesResponseType(typeof(CostAnalysisReport), 200)]
    public async Task<IActionResult> GetCostAnalysis(
        Guid configurationId,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var from = fromDate ?? DateTime.UtcNow.AddMonths(-1);
            var to = toDate ?? DateTime.UtcNow;
            var report = await _reportingService.GenerateCostReportAsync(configurationId, from, to, cancellationToken);
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cost analysis");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get unanswered questions
    /// </summary>
    [HttpGet("unanswered/{configurationId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<UnansweredQuestionDto>), 200)]
    public async Task<IActionResult> GetUnansweredQuestions(
        Guid configurationId,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var questions = await _unansweredRepository.GetMostFrequentAsync(configurationId, limit, cancellationToken);

            var dtos = questions.Select(q => new UnansweredQuestionDto
            {
                Id = q.Id,
                Question = q.Question,
                OccurrenceCount = q.OccurrenceCount,
                FirstAskedAt = q.FirstAskedAt,
                LastAskedAt = q.LastAskedAt,
                AttemptedIntentName = q.AttemptedIntentName,
                AttemptedConfidence = q.AttemptedConfidence,
                SuggestedIntentName = q.SuggestedIntentName,
                SuggestedResponse = q.SuggestedResponse,
                IsProcessed = q.IsProcessed
            });

            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unanswered questions");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Mark unanswered question as processed
    /// </summary>
    [HttpPost("unanswered/{questionId:guid}/process")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ProcessQuestion(
        Guid questionId,
        [FromBody] ProcessQuestionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userName = User.Identity?.Name ?? "Admin";
            await _unansweredRepository.MarkAsProcessedAsync(
                questionId,
                null,
                userName,
                cancellationToken);

            return Ok(new { message = "Question processed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing question");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}

public record ProcessQuestionRequest(string? SuggestedIntentName, string? SuggestedResponse);
