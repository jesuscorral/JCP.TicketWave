using JCP.TicketWave.CatalogService.Application.Controllers;
using JCP.TicketWave.CatalogService.Domain.Models;
using JCP.TicketWave.CatalogService.Infrastructure.Persistence;
using System.Reflection;

namespace JCP.TicketWave.ArchitectureTests.Services.CatalogService;

public abstract class BaseTest
{
    protected static readonly Assembly DomainAssembly = typeof(Event).Assembly;
    protected static readonly Assembly ApplicationAssembly = typeof(EventsController).Assembly;
    protected static readonly Assembly InfrastructureAssembly = typeof(CatalogDbContext).Assembly;

}