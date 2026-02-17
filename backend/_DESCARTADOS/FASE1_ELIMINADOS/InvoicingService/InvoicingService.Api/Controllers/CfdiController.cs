using InvoicingService.Application.DTOs;
using InvoicingService.Domain.Entities;
using InvoicingService.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InvoicingService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CfdiController : ControllerBase
{
    private readonly ICfdiConfigurationRepository _cfdiRepository;
    private readonly ILogger<CfdiController> _logger;

    public CfdiController(ICfdiConfigurationRepository cfdiRepository, ILogger<CfdiController> logger)
    {
        _cfdiRepository = cfdiRepository;
        _logger = logger;
    }

    private Guid GetDealerId() => Guid.Parse(User.FindFirstValue("dealerId") ?? throw new UnauthorizedAccessException());

    [HttpGet]
    public async Task<ActionResult<CfdiConfigurationDto>> GetConfiguration(CancellationToken cancellationToken)
    {
        var dealerId = GetDealerId();
        var config = await _cfdiRepository.GetByDealerAsync(dealerId, cancellationToken);

        if (config == null)
            return NotFound("CFDI configuration not found for this dealer");

        return Ok(CfdiConfigurationDto.FromEntity(config));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CfdiConfigurationDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var config = await _cfdiRepository.GetByIdAsync(id, cancellationToken);
        if (config == null)
            return NotFound();

        return Ok(CfdiConfigurationDto.FromEntity(config));
    }

    [HttpPost]
    public async Task<ActionResult<CfdiConfigurationDto>> Create([FromBody] CreateCfdiConfigurationRequest request, CancellationToken cancellationToken)
    {
        var dealerId = GetDealerId();

        // Check if configuration already exists
        if (await _cfdiRepository.ExistsForDealerAsync(dealerId, cancellationToken))
            return BadRequest("CFDI configuration already exists for this dealer");

        var config = new CfdiConfiguration(
            dealerId,
            request.Rfc,
            request.BusinessName,
            request.TaxRegime,
            request.FiscalAddress,
            request.PostalCode);

        if (!string.IsNullOrEmpty(request.DefaultSeries))
            config.SetSeries(request.DefaultSeries);

        await _cfdiRepository.AddAsync(config, cancellationToken);

        _logger.LogInformation("CFDI configuration created for dealer {DealerId}", dealerId);

        return CreatedAtAction(nameof(GetById), new { id = config.Id }, CfdiConfigurationDto.FromEntity(config));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CfdiConfigurationDto>> Update(Guid id, [FromBody] UpdateCfdiConfigurationRequest request, CancellationToken cancellationToken)
    {
        var config = await _cfdiRepository.GetByIdAsync(id, cancellationToken);
        if (config == null)
            return NotFound();

        config.UpdateIssuerInfo(
            request.Rfc,
            request.BusinessName,
            request.TaxRegime,
            request.FiscalAddress,
            request.PostalCode);

        await _cfdiRepository.UpdateAsync(config, cancellationToken);

        _logger.LogInformation("CFDI configuration {Id} updated", id);

        return Ok(CfdiConfigurationDto.FromEntity(config));
    }

    [HttpPost("{id:guid}/certificate")]
    public async Task<ActionResult<CfdiConfigurationDto>> UploadCertificate(Guid id, [FromBody] UploadCertificateRequest request, CancellationToken cancellationToken)
    {
        var config = await _cfdiRepository.GetByIdAsync(id, cancellationToken);
        if (config == null)
            return NotFound();

        var certificate = Convert.FromBase64String(request.CertificateBase64);
        var privateKey = Convert.FromBase64String(request.PrivateKeyBase64);

        config.SetCertificate(
            request.CertificateNumber,
            certificate,
            privateKey,
            request.Password,
            request.ValidFrom,
            request.ValidTo);

        await _cfdiRepository.UpdateAsync(config, cancellationToken);

        _logger.LogInformation("CFDI certificate uploaded for configuration {Id}", id);

        return Ok(CfdiConfigurationDto.FromEntity(config));
    }

    [HttpPost("{id:guid}/pac")]
    public async Task<ActionResult<CfdiConfigurationDto>> ConfigurePac(Guid id, [FromBody] ConfigurePacRequest request, CancellationToken cancellationToken)
    {
        var config = await _cfdiRepository.GetByIdAsync(id, cancellationToken);
        if (config == null)
            return NotFound();

        config.ConfigurePac(
            request.Provider,
            request.Username,
            request.Password,
            request.IsProduction);

        await _cfdiRepository.UpdateAsync(config, cancellationToken);

        _logger.LogInformation("CFDI PAC configured for {Id}: {Provider}", id, request.Provider);

        return Ok(CfdiConfigurationDto.FromEntity(config));
    }

    [HttpPost("{id:guid}/series")]
    public async Task<ActionResult<CfdiConfigurationDto>> SetSeries(Guid id, [FromBody] SetSeriesRequest request, CancellationToken cancellationToken)
    {
        var config = await _cfdiRepository.GetByIdAsync(id, cancellationToken);
        if (config == null)
            return NotFound();

        config.SetSeries(request.Series, request.StartFolio);

        await _cfdiRepository.UpdateAsync(config, cancellationToken);

        _logger.LogInformation("CFDI series set for {Id}: {Series} starting at {Folio}", id, request.Series, request.StartFolio);

        return Ok(CfdiConfigurationDto.FromEntity(config));
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult<CfdiConfigurationDto>> Activate(Guid id, CancellationToken cancellationToken)
    {
        var config = await _cfdiRepository.GetByIdAsync(id, cancellationToken);
        if (config == null)
            return NotFound();

        try
        {
            config.Activate();
            await _cfdiRepository.UpdateAsync(config, cancellationToken);

            _logger.LogInformation("CFDI configuration {Id} activated", id);

            return Ok(CfdiConfigurationDto.FromEntity(config));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<ActionResult<CfdiConfigurationDto>> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var config = await _cfdiRepository.GetByIdAsync(id, cancellationToken);
        if (config == null)
            return NotFound();

        config.Deactivate();
        await _cfdiRepository.UpdateAsync(config, cancellationToken);

        _logger.LogInformation("CFDI configuration {Id} deactivated", id);

        return Ok(CfdiConfigurationDto.FromEntity(config));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var config = await _cfdiRepository.GetByIdAsync(id, cancellationToken);
        if (config == null)
            return NotFound();

        if (config.IsActive)
            return BadRequest("Cannot delete active CFDI configuration. Deactivate it first.");

        await _cfdiRepository.DeleteAsync(id, cancellationToken);

        _logger.LogInformation("CFDI configuration {Id} deleted", id);

        return NoContent();
    }
}
