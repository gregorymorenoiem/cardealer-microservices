using MediaService.Application.Features.Media.Commands.InitUpload;
using MediaService.Application.Features.Media.Commands.FinalizeUpload;
using MediaService.Application.Features.Media.Queries.GetMedia;
using MediaService.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

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
        var result = await _mediator.Send(command);
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