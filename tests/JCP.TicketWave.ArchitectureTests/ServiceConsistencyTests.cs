using NetArchTest.Rules;
using FluentAssertions;
using Xunit;

namespace JCP.TicketWave.ArchitectureTests;

/// <summary>
/// Tests específicos para validar la consistencia estructural
/// de todos los servicios en el proyecto TicketWave
/// </summary>
public class ServiceConsistencyTests
{
    // [Fact]
    // public void All_Services_Should_Have_Domain_Models_Folder()
    // {
    //     // Todos los servicios deben tener una carpeta Domain.Models
    //     var serviceNamespaces = new[]
    //     {
    //         "JCP.TicketWave.BookingService.Domain.Models",
    //         "JCP.TicketWave.CatalogService.Domain.Models",
    //         "JCP.TicketWave.PaymentService.Domain.Models",
    //         "JCP.TicketWave.NotificationService.Domain.Models"
    //     };

    //     foreach (var ns in serviceNamespaces)
    //     {
    //         var types = Types.InNamespace(ns).GetTypes();
            
    //         types.Should().NotBeEmpty($"Service namespace {ns} should contain domain models");
    //     }
    // }

    // [Fact]
    // public void All_Services_Should_Have_Infrastructure_Persistence_Folder()
    // {
    //     // Todos los servicios deben tener una carpeta Infrastructure.Persistence
    //     var persistenceNamespaces = new[]
    //     {
    //         "JCP.TicketWave.BookingService.Infrastructure.Persistence",
    //         "JCP.TicketWave.CatalogService.Infrastructure.Persistence",
    //         "JCP.TicketWave.PaymentService.Infrastructure.Persistence"
    //     };

    //     foreach (var ns in persistenceNamespaces)
    //     {
    //         var types = Types.InNamespace(ns).GetTypes();
            
    //         types.Should().NotBeEmpty($"Service namespace {ns} should contain persistence classes");
    //     }
    // }

    // [Fact]
    // public void All_Repository_Interfaces_Should_Be_Async()
    // {
    //     // Todas las interfaces de repositorio deben ser asíncronas
    //     var repositoryInterfaceNamespaces = new[]
    //     {
    //         "JCP.TicketWave.BookingService.Domain.Interfaces",
    //         "JCP.TicketWave.CatalogService.Domain.Interfaces",
    //         "JCP.TicketWave.PaymentService.Domain.Interfaces"
    //     };

    //     foreach (var ns in repositoryInterfaceNamespaces)
    //     {
    //         var repositoryInterfaces = Types
    //             .InNamespace(ns)
    //             .That()
    //             .AreInterfaces()
    //             .And()
    //             .HaveNameEndingWith("Repository")
    //             .GetTypes();

    //         foreach (var repoInterface in repositoryInterfaces)
    //         {
    //             var methods = repoInterface.GetMethods();
    //             var asyncMethods = methods.Where(m => 
    //                 m.ReturnType.Name.Contains("Task") || 
    //                 m.ReturnType.Name.Contains("ValueTask"));

    //             methods.Length.Should().BeGreaterThan(0, 
    //                 $"Repository interface {repoInterface.Name} should have methods");
                
    //             // Al menos el 80% de los métodos deberían ser async
    //             var asyncPercentage = (double)asyncMethods.Count() / methods.Length;
    //             asyncPercentage.Should().BeGreaterThan(0.8, 
    //                 $"Repository interface {repoInterface.Name} should have mostly async methods");
    //         }
    //     }
    // }

    // [Fact]
    // public void Domain_Models_Should_Have_Private_Setters()
    // {
    //     // Los modelos de dominio deben tener setters privados para encapsulación
    //     var domainModelNamespaces = new[]
    //     {
    //         "JCP.TicketWave.BookingService.Domain.Models",
    //         "JCP.TicketWave.CatalogService.Domain.Models",
    //         "JCP.TicketWave.PaymentService.Domain.Models"
    //     };

    //     foreach (var ns in domainModelNamespaces)
    //     {
    //         var domainModels = Types
    //             .InNamespace(ns)
    //             .That()
    //             .AreClasses()
    //             .And()
    //             .AreNotAbstract()
    //             .And()
    //             .DoNotHaveNameEndingWith("Enum")
    //             .GetTypes();

    //         foreach (var model in domainModels.Where(t => !t.IsEnum))
    //         {
    //             var properties = model.GetProperties();
    //             var publicSetters = properties.Where(p => 
    //                 p.GetSetMethod() != null && 
    //                 p.GetSetMethod()!.IsPublic);

    //             publicSetters.Should().BeEmpty(
    //                 $"Domain model {model.Name} should not have public setters. Properties with public setters: {string.Join(", ", publicSetters.Select(p => p.Name))}");
    //         }
    //     }
    // }

    // [Fact]
    // public void All_Services_Should_Use_Same_Repository_Pattern()
    // {
    //     // Todos los servicios deben seguir el mismo patrón de repositorio
    //     var serviceNames = new[] { "BookingService", "CatalogService", "PaymentService" };

    //     foreach (var serviceName in serviceNames)
    //     {
    //         var interfaceNamespace = $"JCP.TicketWave.{serviceName}.Domain.Interfaces";
    //         var implementationNamespace = $"JCP.TicketWave.{serviceName}.Infrastructure.Persistence.Repositories";

    //         var interfaces = Types
    //             .InNamespace(interfaceNamespace)
    //             .That()
    //             .AreInterfaces()
    //             .And()
    //             .HaveNameEndingWith("Repository")
    //             .GetTypes();

    //         var implementations = Types
    //             .InNamespace(implementationNamespace)
    //             .That()
    //             .AreClasses()
    //             .And()
    //             .HaveNameEndingWith("Repository")
    //             .GetTypes();

    //         foreach (var interfaceType in interfaces)
    //         {
    //             var expectedImplementationName = interfaceType.Name.Substring(1); // Remove 'I' prefix
    //             var hasImplementation = implementations.Any(impl => impl.Name == expectedImplementationName);

    //             hasImplementation.Should().BeTrue(
    //                 $"Interface {interfaceType.Name} in {serviceName} should have a corresponding implementation {expectedImplementationName}");
    //         }
    //     }
    // }

    // [Fact]
    // public void Controllers_Should_Follow_Naming_Convention()
    // {
    //     // Los controladores deben seguir convenciones de nomenclatura
    //     var controllerNamespaces = new[]
    //     {
    //         "JCP.TicketWave.BookingService.Controllers",
    //         "JCP.TicketWave.CatalogService.Controllers",
    //         "JCP.TicketWave.PaymentService.Controllers",
    //         "JCP.TicketWave.NotificationService.Controllers"
    //     };

    //     foreach (var ns in controllerNamespaces)
    //     {
    //         var controllers = Types
    //             .InNamespace(ns)
    //             .That()
    //             .AreClasses()
    //             .GetTypes();

    //         foreach (var controller in controllers)
    //         {
    //             controller.Name.Should().EndWith("Controller", 
    //                 $"Controller class {controller.Name} should end with 'Controller'");
    //         }
    //     }
    // }

    // [Fact]
    // public void Features_Should_Be_Organized_By_Entity()
    // {
    //     // Las características deben estar organizadas por entidad
    //     var featureNamespaces = new[]
    //     {
    //         "JCP.TicketWave.BookingService.Features",
    //         "JCP.TicketWave.CatalogService.Features",
    //         "JCP.TicketWave.PaymentService.Features"
    //     };

    //     foreach (var ns in featureNamespaces)
    //     {
    //         var featureTypes = Types.InNamespace(ns).GetTypes();
            
    //         if (featureTypes.Any())
    //         {
    //             // Verificar que las features están organizadas por entidad (subfolder structure)
    //             var featureNamespacesByEntity = featureTypes
    //                 .GroupBy(t => t.Namespace?.Split('.').Skip(4).FirstOrDefault())
    //                 .Where(g => !string.IsNullOrEmpty(g.Key));

    //             featureNamespacesByEntity.Should().NotBeEmpty(
    //                 $"Features in {ns} should be organized by entity/aggregate in subfolders");
    //         }
    //     }
    // }

    // [Fact]
    // public void All_Services_Should_Have_Consistent_Folder_Structure()
    // {
    //     // Todos los servicios deben tener la misma estructura de carpetas
    //     var serviceNames = new[] { "BookingService", "CatalogService", "PaymentService" };

    //     foreach (var serviceName in serviceNames)
    //     {
    //         // Verificar Domain.Models
    //         var domainModels = Types
    //             .InNamespace($"JCP.TicketWave.{serviceName}.Domain.Models")
    //             .GetTypes();
    //         domainModels.Should().NotBeEmpty($"{serviceName} should have Domain.Models");

    //         // Verificar Domain.Interfaces
    //         var domainInterfaces = Types
    //             .InNamespace($"JCP.TicketWave.{serviceName}.Domain.Interfaces")
    //             .GetTypes();
    //         domainInterfaces.Should().NotBeEmpty($"{serviceName} should have Domain.Interfaces");

    //         // Verificar Infrastructure.Persistence
    //         var persistence = Types
    //             .InNamespace($"JCP.TicketWave.{serviceName}.Infrastructure.Persistence")
    //             .GetTypes();
    //         persistence.Should().NotBeEmpty($"{serviceName} should have Infrastructure.Persistence");

    //         // Verificar Infrastructure.Persistence.Repositories
    //         var repositories = Types
    //             .InNamespace($"JCP.TicketWave.{serviceName}.Infrastructure.Persistence.Repositories")
    //             .GetTypes();
    //         repositories.Should().NotBeEmpty($"{serviceName} should have Infrastructure.Persistence.Repositories");
    //     }
    // }

    // [Fact]
    // public void Entities_Should_Have_Factory_Methods()
    // {
    //     // Las entidades deben tener métodos factory para creación
    //     var domainModelNamespaces = new[]
    //     {
    //         "JCP.TicketWave.BookingService.Domain.Models",
    //         "JCP.TicketWave.CatalogService.Domain.Models",
    //         "JCP.TicketWave.PaymentService.Domain.Models"
    //     };

    //     foreach (var ns in domainModelNamespaces)
    //     {
    //         var entities = Types
    //             .InNamespace(ns)
    //             .That()
    //             .AreClasses()
    //             .And()
    //             .AreNotAbstract()
    //             .And()
    //             .DoNotHaveNameEndingWith("Status")
    //             .And()
    //             .DoNotHaveNameEndingWith("Type")
    //             .GetTypes();

    //         foreach (var entity in entities.Where(t => !t.IsEnum))
    //         {
    //             var factoryMethods = entity.GetMethods()
    //                 .Where(m => m.IsStatic && 
    //                            m.IsPublic && 
    //                            (m.Name.StartsWith("Create") || m.Name.StartsWith("New")))
    //                 .ToArray();

    //             factoryMethods.Should().NotBeEmpty(
    //                 $"Entity {entity.Name} should have at least one static factory method (Create* or New*)");
    //         }
    //     }
    // }
}