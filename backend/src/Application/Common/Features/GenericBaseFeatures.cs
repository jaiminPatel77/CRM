using Crm.Application.Common.Exceptions;
using Crm.Application.Common.Interfaces;
using Crm.Application.Common.Models;
using Crm.Domain.Common;
using Gridify;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crm.Application.Common.Features;

// 1. GET (Single)
public abstract class BaseGetQueryHandler<TEntity, TDto, TQuery> : IRequestHandler<TQuery, Result<TDto>>
    where TEntity : BaseEntity
    where TQuery : IRequest<Result<TDto>>, IHasId<long>
{
    protected readonly IApplicationDbContext _context;
    public BaseGetQueryHandler(IApplicationDbContext context) => _context = context;

    public abstract TDto MapToDto(TEntity entity);
    
    // Hook for Includes
    protected virtual IQueryable<TEntity> GetQueryable() => _context.Set<TEntity>();

    public virtual async Task<Result<TDto>> Handle(TQuery request, CancellationToken cancellationToken)
    {
        var entity = await GetQueryable().FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (entity == null) throw new NotFoundException(typeof(TEntity).Name, request.Id);
        return Result<TDto>.Success(MapToDto(entity));
    }
}

// Pagination Options
public class PaginationOptions
{
    public int DefaultPageSize { get; set; } = 20;
    public int MaxPageSize { get; set; } = 100;
}

public class PagedResult<T>
{
    public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();
    public int TotalCount { get; set; }
    public int PageSize { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
    public bool HasNextPage => CurrentPage < TotalPages;
    public bool HasPreviousPage => CurrentPage > 1;

    public PagedResult() { }

    public PagedResult(int totalCount, IEnumerable<T> data)
    {
        TotalCount = totalCount;
        Data = data;
    }
}

// 2. LIST (Pagination)
public abstract class BaseListQueryHandler<TEntity, TDto, TQuery> : IRequestHandler<TQuery, Result<Paging<TDto>>>
    where TEntity : BaseEntity
    where TQuery : GridifyQuery, IRequest<Result<Paging<TDto>>>
{
    protected readonly IApplicationDbContext _context;
    protected PaginationOptions PaginationOptions { get; } = new();
    public BaseListQueryHandler(IApplicationDbContext context) => _context = context;

    public abstract TDto MapToDto(TEntity entity);

    public virtual async Task<Result<Paging<TDto>>> Handle(TQuery request, CancellationToken cancellationToken)
    {
        // Enforce pagination defaults and limits
        if (request.PageSize > PaginationOptions.MaxPageSize)
        {
            request.PageSize = PaginationOptions.MaxPageSize;
        }
        if (request.PageSize <= 0)
        {
            request.PageSize = PaginationOptions.DefaultPageSize;
        }
        if (request.Page < 1)
        {
            request.Page = 1;
        }

        // Fix Gridify double-wildcard bug: *value* → Gridify treats trailing * as literal
        // We convert to proper contains syntax by removing trailing * when leading * is present
        if (!string.IsNullOrEmpty(request.Filter))
        {
            request.Filter = SanitizeGridifyFilter(request.Filter);
        }
        
        if (string.IsNullOrEmpty(request.OrderBy))
        {
            request.OrderBy = "Id desc";
        }
        
        var query = _context.Set<TEntity>().AsQueryable();
        var count = await query.ApplyFiltering(request).CountAsync(cancellationToken);
        
        var list = await query.ApplyFiltering(request).ApplyOrdering(request).ApplyPaging(request).AsNoTracking().ToListAsync(cancellationToken);
        var dtos = list.Select(MapToDto).ToList();
        
        var paging = new Paging<TDto>(count, dtos);
        
        return Result<Paging<TDto>>.Success(paging);
    }
    
    /// <summary>
    /// Fixes Gridify 2.x double-wildcard issue where *value* treats trailing * as literal.
    /// Pattern: field=*value* becomes field=*value (Gridify auto-adds trailing % for contains).
    /// </summary>
    private static string SanitizeGridifyFilter(string filter)
    {
        // Regex to find patterns like: fieldName=*value* and convert to fieldName=*value
        // This allows Gridify to properly interpret leading * as "contains"
        return System.Text.RegularExpressions.Regex.Replace(
            filter,
            @"(\w+)=\*([^*,|]+)\*",  // Match: field=*value*
            "$1=*$2"                   // Replace with: field=*value
        );
    }
}

// 3. CREATE
public abstract class BaseCreateCommandHandler<TEntity, TCommand> : IRequestHandler<TCommand, Result<long>>
    where TEntity : BaseEntity
    where TCommand : IRequest<Result<long>>
{
    protected readonly IApplicationDbContext _context;
    public BaseCreateCommandHandler(IApplicationDbContext context) => _context = context;

    public abstract TEntity MapToEntity(TCommand command);

    public virtual async Task<Result<long>> Handle(TCommand request, CancellationToken cancellationToken)
    {
        var entity = MapToEntity(request);
        _context.Set<TEntity>().Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<long>.Success(entity.Id);
    }
}

// 4. UPDATE
public abstract class BaseUpdateCommandHandler<TEntity, TCommand> : IRequestHandler<TCommand, Result<long>>
    where TEntity : BaseEntity
    where TCommand : IRequest<Result<long>>, IHasId<long>
{
    protected readonly IApplicationDbContext _context;
    public BaseUpdateCommandHandler(IApplicationDbContext context) => _context = context;

    public abstract void MapToEntity(TCommand command, TEntity entity);

    public virtual async Task<Result<long>> Handle(TCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Set<TEntity>().FindAsync(new object[] { request.Id }, cancellationToken);
        if (entity == null) throw new NotFoundException(typeof(TEntity).Name, request.Id);

        MapToEntity(request, entity);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<long>.Success(entity.Id);
    }
}

// 5. DELETE
public abstract class BaseDeleteCommandHandler<TEntity, TCommand> : IRequestHandler<TCommand, Result<long>>
    where TEntity : BaseEntity
    where TCommand : IRequest<Result<long>>, IHasId<long>
{
    protected readonly IApplicationDbContext _context;
    public BaseDeleteCommandHandler(IApplicationDbContext context) => _context = context;

    public virtual async Task<Result<long>> Handle(TCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Set<TEntity>().FindAsync(new object[] { request.Id }, cancellationToken);
        if (entity == null) throw new NotFoundException(typeof(TEntity).Name, request.Id);

        _context.Set<TEntity>().Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<long>.Success(entity.Id);
    }
}
