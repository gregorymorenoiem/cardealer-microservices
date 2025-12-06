using MediaService.Application.Features.Media.Commands.InitUpload;
using MediaService.Application.Features.Media.Commands.FinalizeUpload;
using MediaService.Application.Features.Media.Queries.GetMedia;
using MediaService.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MediaService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MediaController : ControllerBase
{
    private readonly IMediator _mediator;

    public MediaController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("upload/init")]
    public async Task<ActionResult<ApiResponse<InitUploadResponse>>> InitUpload(InitUploadCommand command)
    {
        // Extract DealerId from JWT claims
        var dealerIdClaim = User.FindFirst("dealerId")?.Value;
        if (string.IsNullOrEmpty(dealerIdClaim) || !Guid.TryParse(dealerIdClaim, out var dealerId))
        {
            return BadRequest(ApiResponse<InitUploadResponse>.Fail("Invalid or missing dealerId claim"));
        }

        // Override DealerId from token (security measure)
        var commandWithDealerId = new InitUploadCommand(
            dealerId,
            command.OwnerId,
            command.Context,
            command.FileName,
            command.ContentType,
            command.FileSize
        );

        var result = await _mediator.Send(commandWithDealerId);
        return Ok(result);
    }

    [HttpPost("upload/finalize/{mediaId}")]
    public async Task<ActionResult<ApiResponse<FinalizeUploadResponse>>> FinalizeUpload(string mediaId)
    {
        var command = new FinalizeUploadCommand(mediaId);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpGet("{mediaId}")]
    public async Task<ActionResult<ApiResponse<GetMediaResponse>>> GetMedia(string mediaId)
    {
        var query = new GetMediaQuery(mediaId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}