using HermesZoneTorba.Application.HermesManagement.Commands.InstallHermes;
using HermesZoneTorba.Application.HermesManagement.Queries.GetHermesStatus;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HermesZoneTorba.Api.Controllers;

/// <summary>
/// Hermes Manager surface — install/status today; update/repair/restart/stop land alongside their
/// Application handlers in Phase 1 per docs/18-roadmap.md. See docs/04-api-spec.md for full contract
/// conventions (pagination, idempotency, Problem Details).
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/hermes")]
public sealed class HermesController(ISender mediator) : ControllerBase
{
    /// <summary>Installs a new Hermes instance. See docs/06-hermes-integration.md#install-flow.</summary>
    [HttpPost("install")]
    [ProducesResponseType<InstallHermesResult>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Install([FromBody] InstallHermesCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetStatus), new { instanceId = result.InstanceId }, result);
    }

    /// <summary>Point-in-time status for a Hermes instance. Live updates stream via AgentMonitorHub.</summary>
    [HttpGet("{instanceId:guid}/status")]
    [ProducesResponseType<HermesStatusDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatus(Guid instanceId, CancellationToken cancellationToken)
    {
        var status = await mediator.Send(new GetHermesStatusQuery(instanceId), cancellationToken);
        return status is null ? NotFound() : Ok(status);
    }
}
