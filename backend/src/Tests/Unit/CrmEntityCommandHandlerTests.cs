using Crm.Application.Common.Interfaces;
using Crm.Application.Features.Activities.Commands;
using Crm.Application.Features.Activities.Queries;
using Crm.Application.Features.Customers.Commands;
using Crm.Application.Features.Customers.Queries;
using Crm.Application.Features.Leads.Commands;
using Crm.Application.Features.Leads.Queries;
using Crm.Application.Features.Opportunities.Commands;
using Crm.Application.Features.Opportunities.Queries;
using Crm.Domain.Consts;
using Crm.Domain.Entities;
using Crm.Infrastructure.Persistence;
using Crm.Infrastructure.Persistence.Interceptors;
using Crm.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace Crm.Tests.Unit;

public class CrmEntityCommandHandlerTests
{
    private (ApplicationDbContext dbContext, TenantContext tenantContext) GetDbContext(Guid tenantId)
    {
        var tenantContext = new TenantContext();
        tenantContext.SetTenant(tenantId, "test-tenant");

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var auditableInterceptor = new AuditableEntityInterceptor(
            Substitute.For<ICurrentUserService>(),
            Substitute.For<IDateTime>());

        var dbContext = new ApplicationDbContext(options, auditableInterceptor, tenantContext);
        return (dbContext, tenantContext);
    }

    [Fact]
    public async Task CustomerCRUD_ShouldPerformSuccessfully()
    {
        var tenantId = Guid.NewGuid();
        var (context, _) = GetDbContext(tenantId);

        var createHandler = new CreateCustomerCommandHandler(context);
        var createCommand = new CreateCustomerCommand
        {
            Name = "Acme Client",
            Email = "acme@example.com",
            Phone = "555-0199",
            Company = "Acme Inc",
            Industry = "Technology"
        };

        var created = await createHandler.Handle(createCommand, CancellationToken.None);
        Assert.NotNull(created);
        Assert.True(created.Id > 0);
        Assert.Equal("Acme Client", created.Name);

        var getQueryHandler = new GetCustomersQueryHandler(context);
        var list = await getQueryHandler.Handle(new GetCustomersQuery(), CancellationToken.None);
        Assert.Single(list);

        var updateHandler = new UpdateCustomerCommandHandler(context);
        var updateCommand = new UpdateCustomerCommand
        {
            Id = created.Id,
            Name = "Acme Client Updated",
            Email = "acme@example.com",
            Phone = "555-0199",
            Company = "Acme Corp"
        };
        var updated = await updateHandler.Handle(updateCommand, CancellationToken.None);
        Assert.Equal("Acme Client Updated", updated.Name);

        var deleteHandler = new DeleteCustomerCommandHandler(context);
        var deleted = await deleteHandler.Handle(new DeleteCustomerCommand(created.Id), CancellationToken.None);
        Assert.True(deleted);

        var emptyList = await getQueryHandler.Handle(new GetCustomersQuery(), CancellationToken.None);
        Assert.Empty(emptyList);
    }

    [Fact]
    public async Task LeadCRUD_ShouldPerformSuccessfully()
    {
        var tenantId = Guid.NewGuid();
        var (context, _) = GetDbContext(tenantId);

        var createHandler = new CreateLeadCommandHandler(context);
        var createCommand = new CreateLeadCommand
        {
            Title = "Potential Software Deal",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@company.com",
            EstimatedValue = 50000m,
            Status = EnumLeadStatus.New
        };

        var created = await createHandler.Handle(createCommand, CancellationToken.None);
        Assert.NotNull(created);
        Assert.Equal("Potential Software Deal", created.Title);

        var getByIdHandler = new GetLeadByIdQueryHandler(context);
        var fetched = await getByIdHandler.Handle(new GetLeadByIdQuery(created.Id), CancellationToken.None);
        Assert.NotNull(fetched);
        Assert.Equal(EnumLeadStatus.New, fetched.Status);
    }

    [Fact]
    public async Task OpportunityCRUD_ShouldPerformSuccessfully()
    {
        var tenantId = Guid.NewGuid();
        var (context, _) = GetDbContext(tenantId);

        var createHandler = new CreateOpportunityCommandHandler(context);
        var createCommand = new CreateOpportunityCommand
        {
            Title = "Enterprise License Deal",
            Amount = 120000m,
            Stage = EnumOpportunityStage.Proposal,
            Probability = 75
        };

        var created = await createHandler.Handle(createCommand, CancellationToken.None);
        Assert.NotNull(created);
        Assert.Equal(120000m, created.Amount);
        Assert.Equal(EnumOpportunityStage.Proposal, created.Stage);
    }

    [Fact]
    public async Task ActivityCRUD_ShouldPerformSuccessfully()
    {
        var tenantId = Guid.NewGuid();
        var (context, _) = GetDbContext(tenantId);

        var createHandler = new CreateActivityCommandHandler(context);
        var createCommand = new CreateActivityCommand
        {
            Subject = "Demo Call with CTO",
            Type = EnumActivityType.Call,
            Description = "Discuss architecture and multi-tenancy requirements."
        };

        var created = await createHandler.Handle(createCommand, CancellationToken.None);
        Assert.NotNull(created);
        Assert.Equal("Demo Call with CTO", created.Subject);
        Assert.False(created.IsCompleted);

        var updateHandler = new UpdateActivityCommandHandler(context);
        var updated = await updateHandler.Handle(new UpdateActivityCommand
        {
            Id = created.Id,
            Subject = "Demo Call with CTO",
            Type = EnumActivityType.Call,
            IsCompleted = true
        }, CancellationToken.None);

        Assert.True(updated.IsCompleted);
    }

    [Fact]
    public async Task MultiTenantQueryFilter_ShouldIsolateEntitiesBetweenTenants()
    {
        var dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var auditableInterceptor = new AuditableEntityInterceptor(
            Substitute.For<ICurrentUserService>(),
            Substitute.For<IDateTime>());

        var tenantIdA = Guid.NewGuid();
        var tenantIdB = Guid.NewGuid();

        var tenantContextA = new TenantContext();
        tenantContextA.SetTenant(tenantIdA, "tenant-a");

        var tenantContextB = new TenantContext();
        tenantContextB.SetTenant(tenantIdB, "tenant-b");

        // Seed Tenant A customer
        using (var dbA = new ApplicationDbContext(options, auditableInterceptor, tenantContextA))
        {
            dbA.Customers.Add(new Customer { Name = "Tenant A Customer", TenantId = tenantIdA });
            await dbA.SaveChangesAsync();
        }

        // Seed Tenant B customer
        using (var dbB = new ApplicationDbContext(options, auditableInterceptor, tenantContextB))
        {
            dbB.Customers.Add(new Customer { Name = "Tenant B Customer", TenantId = tenantIdB });
            await dbB.SaveChangesAsync();
        }

        // Verify query from Tenant A context
        using (var dbAQuery = new ApplicationDbContext(options, auditableInterceptor, tenantContextA))
        {
            var customers = await dbAQuery.Customers.ToListAsync();
            Assert.Single(customers);
            Assert.Equal("Tenant A Customer", customers[0].Name);
        }

        // Verify query from Tenant B context
        using (var dbBQuery = new ApplicationDbContext(options, auditableInterceptor, tenantContextB))
        {
            var customers = await dbBQuery.Customers.ToListAsync();
            Assert.Single(customers);
            Assert.Equal("Tenant B Customer", customers[0].Name);
        }
    }
}
