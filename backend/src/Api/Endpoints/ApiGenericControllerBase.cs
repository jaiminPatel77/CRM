using Crm.Application.Common.Models;
using Crm.Application.Common.Interfaces;
using Crm.Domain.Consts;
using Gridify;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public abstract class ApiGenericControllerBase<TCreateCmd, TUpdateCmd, TDeleteCmd, TGetQuery, TListQuery, TDetailDto, TListDto, TKey> : ApiControllerBase
    where TCreateCmd : IRequest<Result<TKey>>
    where TUpdateCmd : IRequest<Result<TKey>>, IHasId<TKey>
    where TDeleteCmd : IRequest<Result<TKey>>, IHasId<TKey>, new()
    where TGetQuery : IRequest<Result<TDetailDto>>, IHasId<TKey>, new()
    where TListQuery : IRequest<Result<Paging<TListDto>>>
{
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public virtual async Task<ActionResult<Result<TDetailDto>>> Get(TKey id)
    {
        var query = new TGetQuery { Id = id };
        return await Mediator.Send(query);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<ActionResult<Result<Paging<TListDto>>>> GetList([FromQuery] TListQuery query)
    {
        return await Mediator.Send(query);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public virtual async Task<ActionResult<Result<TKey>>> Create([FromBody] TCreateCmd command)
    {
        return await Mediator.Send(command);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<ActionResult<Result<TKey>>> Update(TKey id, [FromBody] TUpdateCmd command)
    {
        if (!EqualityComparer<TKey>.Default.Equals(id, command.Id))
        {
             command.Id = id;
        }
        return await Mediator.Send(command);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<ActionResult<Result<TKey>>> Delete(TKey id)
    {
        var command = new TDeleteCmd { Id = id };
        return await Mediator.Send(command);
    }
}

// Simplified version for long ID and Same DTO for details/list
public abstract class ApiGenericControllerBase<TCreateCmd, TUpdateCmd, TDeleteCmd, TGetQuery, TListQuery, TDto> 
    : ApiGenericControllerBase<TCreateCmd, TUpdateCmd, TDeleteCmd, TGetQuery, TListQuery, TDto, TDto, long>
    where TCreateCmd : IRequest<Result<long>>
    where TUpdateCmd : IRequest<Result<long>>, IHasId<long>
    where TDeleteCmd : IRequest<Result<long>>, IHasId<long>, new()
    where TGetQuery : IRequest<Result<TDto>>, IHasId<long>, new()
    where TListQuery : IRequest<Result<Paging<TDto>>>
{
}
