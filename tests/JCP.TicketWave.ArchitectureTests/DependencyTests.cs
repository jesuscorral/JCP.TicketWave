using NetArchTest.Rules;
using FluentAssertions;
using Xunit;

namespace JCP.TicketWave.ArchitectureTests;

/// <summary>
/// Tests para validar las dependencias entre capas y servicios
/// según los principios de Clean Architecture
/// </summary>
public class DependencyTests
{
    // [Fact]
    // public void Services_Should_Not_Have_Circular_Dependencies()
    // {
    //     // Los servicios no deben tener dependencias circulares entre ellos
    //     var services = new[]
    //     {
    //         "JCP.TicketWave.BookingService",
    //         "JCP.TicketWave.CatalogService",
    //         "JCP.TicketWave.PaymentService",
    //         "JCP.TicketWave.NotificationService"
    //     };

    //     foreach (var service in services)
    //     {
    //         var otherServices = services.Where(s => s != service).ToArray();
            
    //         var result = Types
    //             .InNamespace(service)
    //             .Should()
    //             .NotHaveDependencyOnAny(otherServices);

    //         result.IsSuccessful.Should().BeTrue(
    //             $"Service {service} should not depend on other services directly. Use shared contracts instead. Violations: {string.Join(", ", result.FailingTypeNames)}");
    //     }
    // }

    // [Fact]
    // public void Only_Shared_Contracts_Should_Be_Referenced_Between_Services()
    // {
    //     // Solo los contratos compartidos deben ser referenciados entre servicios
    //     var services = new[]
    //     {
    //         "JCP.TicketWave.BookingService",
    //         "JCP.TicketWave.CatalogService",
    //         "JCP.TicketWave.PaymentService",
    //         "JCP.TicketWave.NotificationService"
    //     };

    //     foreach (var service in services)
    //     {
    //         var result = Types
    //             .InNamespace(service)
    //             .That()
    //             .ResideInNamespace(service)
    //             .Should()
    //             .NotHaveDependencyOnAny(services.Where(s => s != service).ToArray())
    //             .Or()
    //             .HaveDependencyOn("JCP.TicketWave.Shared.Contracts")
    //             .Or()
    //             .HaveDependencyOn("JCP.TicketWave.Shared.Infrastructure");

    //         // Esta validación es más compleja, así que usamos una verificación manual
    //         var typesInService = Types.InNamespace(service).GetTypes();
    //         var violatingTypes = new List<string>();

    //         foreach (var type in typesInService)
    //         {
    //             var dependencies = type.GetReferencedAssemblies()
    //                 .Where(a => services.Any(s => a.Name?.Contains(s.Split('.').Last()) == true && !a.Name.Contains(service.Split('.').Last())))
    //                 .ToArray();

    //             if (dependencies.Any())
    //             {
    //                 violatingTypes.Add($"{type.Name}: {string.Join(", ", dependencies.Select(d => d.Name))}");
    //             }
    //         }

    //         violatingTypes.Should().BeEmpty(
    //             $"Service {service} should only reference shared contracts, not other services directly. Violations: {string.Join("; ", violatingTypes)}");
    //     }
    // }

    // [Fact]
    // public void Infrastructure_Should_Not_Reference_Other_Service_Infrastructure()
    // {
    //     // La infraestructura de un servicio no debe referenciar la infraestructura de otro
    //     var infrastructureNamespaces = new[]
    //     {
    //         "JCP.TicketWave.BookingService.Infrastructure",
    //         "JCP.TicketWave.CatalogService.Infrastructure",
    //         "JCP.TicketWave.PaymentService.Infrastructure",
    //         "JCP.TicketWave.NotificationService.Infrastructure"
    //     };

    //     foreach (var infrastructure in infrastructureNamespaces)
    //     {
    //         var otherInfrastructures = infrastructureNamespaces.Where(i => i != infrastructure).ToArray();
            
    //         var result = Types
    //             .InNamespace(infrastructure)
    //             .Should()
    //             .NotHaveDependencyOnAny(otherInfrastructures);

    //         result.IsSuccessful.Should().BeTrue(
    //             $"Infrastructure {infrastructure} should not depend on other service infrastructures. Violations: {string.Join(", ", result.FailingTypeNames)}");
    //     }
    // }

    // [Fact]
    // public void Gateway_Should_Not_Directly_Reference_Domain_Models()
    // {
    //     // El Gateway no debe referenciar directamente modelos de dominio
    //     var domainModelNamespaces = new[]
    //     {
    //         "JCP.TicketWave.BookingService.Domain.Models",
    //         "JCP.TicketWave.CatalogService.Domain.Models",
    //         "JCP.TicketWave.PaymentService.Domain.Models",
    //         "JCP.TicketWave.NotificationService.Domain.Models"
    //     };

    //     var result = Types
    //         .InNamespace("JCP.TicketWave.Gateway")
    //         .Should()
    //         .NotHaveDependencyOnAny(domainModelNamespaces);

    //     result.IsSuccessful.Should().BeTrue(
    //         $"Gateway should not reference domain models directly. Use DTOs/Contracts instead. Violations: {string.Join(", ", result.FailingTypeNames)}");
    // }

    // [Fact]
    // public void Shared_Infrastructure_Should_Not_Depend_On_Services()
    // {
    //     // La infraestructura compartida no debe depender de servicios específicos
    //     var serviceNamespaces = new[]
    //     {
    //         "JCP.TicketWave.BookingService",
    //         "JCP.TicketWave.CatalogService",
    //         "JCP.TicketWave.PaymentService",
    //         "JCP.TicketWave.NotificationService",
    //         "JCP.TicketWave.Gateway"
    //     };

    //     var result = Types
    //         .InNamespace("JCP.TicketWave.Shared.Infrastructure")
    //         .Should()
    //         .NotHaveDependencyOnAny(serviceNamespaces);

    //     result.IsSuccessful.Should().BeTrue(
    //         $"Shared Infrastructure should not depend on specific services. Violations: {string.Join(", ", result.FailingTypeNames)}");
    // }

    // [Fact]
    // public void Shared_Contracts_Should_Not_Depend_On_Anything_Except_System()
    // {
    //     // Los contratos compartidos solo deben depender de tipos del sistema
    //     var forbiddenDependencies = new[]
    //     {
    //         "JCP.TicketWave.BookingService",
    //         "JCP.TicketWave.CatalogService",
    //         "JCP.TicketWave.PaymentService",
    //         "JCP.TicketWave.NotificationService",
    //         "JCP.TicketWave.Gateway",
    //         "JCP.TicketWave.Shared.Infrastructure",
    //         "Microsoft.EntityFrameworkCore",
    //         "Microsoft.AspNetCore",
    //         "Newtonsoft.Json"
    //     };

    //     var result = Types
    //         .InNamespace("JCP.TicketWave.Shared.Contracts")
    //         .Should()
    //         .NotHaveDependencyOnAny(forbiddenDependencies);

    //     result.IsSuccessful.Should().BeTrue(
    //         $"Shared Contracts should have minimal dependencies. Violations: {string.Join(", ", result.FailingTypeNames)}");
    // }

    // [Fact]
    // public void Entity_Framework_Dependencies_Should_Be_Only_In_Infrastructure()
    // {
    //     // Las dependencias de Entity Framework solo deben estar en la infraestructura
    //     var nonInfrastructureNamespaces = new[]
    //     {
    //         "JCP.TicketWave.BookingService.Domain",
    //         "JCP.TicketWave.CatalogService.Domain",
    //         "JCP.TicketWave.PaymentService.Domain",
    //         "JCP.TicketWave.NotificationService.Domain",
    //         "JCP.TicketWave.BookingService.Features",
    //         "JCP.TicketWave.CatalogService.Features",
    //         "JCP.TicketWave.PaymentService.Features",
    //         "JCP.TicketWave.NotificationService.Features",
    //         "JCP.TicketWave.Shared.Contracts"
    //     };

    //     foreach (var ns in nonInfrastructureNamespaces)
    //     {
    //         var result = Types
    //             .InNamespace(ns)
    //             .Should()
    //             .NotHaveDependencyOn("Microsoft.EntityFrameworkCore");

    //         result.IsSuccessful.Should().BeTrue(
    //             $"Namespace {ns} should not depend on Entity Framework. Violations: {string.Join(", ", result.FailingTypeNames)}");
    //     }
    // }

    // [Fact]
    // public void AspNetCore_Dependencies_Should_Be_Limited_To_Presentation()
    // {
    //     // Las dependencias de ASP.NET Core deben limitarse a la capa de presentación
    //     var nonPresentationNamespaces = new[]
    //     {
    //         "JCP.TicketWave.BookingService.Domain",
    //         "JCP.TicketWave.CatalogService.Domain",
    //         "JCP.TicketWave.PaymentService.Domain",
    //         "JCP.TicketWave.NotificationService.Domain",
    //         "JCP.TicketWave.Shared.Contracts"
    //     };

    //     foreach (var ns in nonPresentationNamespaces)
    //     {
    //         var result = Types
    //             .InNamespace(ns)
    //             .Should()
    //             .NotHaveDependencyOnAny(
    //                 "Microsoft.AspNetCore.Mvc",
    //                 "Microsoft.AspNetCore.Http",
    //                 "Microsoft.AspNetCore.Authorization");

    //         result.IsSuccessful.Should().BeTrue(
    //             $"Namespace {ns} should not depend on ASP.NET Core. Violations: {string.Join(", ", result.FailingTypeNames)}");
    //     }
    // }

    // [Fact]
    // public void Database_Specific_Dependencies_Should_Be_Only_In_Persistence()
    // {
    //     // Las dependencias específicas de base de datos solo deben estar en persistence
    //     var nonPersistenceNamespaces = new[]
    //     {
    //         "JCP.TicketWave.BookingService.Domain",
    //         "JCP.TicketWave.CatalogService.Domain",
    //         "JCP.TicketWave.PaymentService.Domain",
    //         "JCP.TicketWave.NotificationService.Domain",
    //         "JCP.TicketWave.BookingService.Features",
    //         "JCP.TicketWave.CatalogService.Features",
    //         "JCP.TicketWave.PaymentService.Features",
    //         "JCP.TicketWave.NotificationService.Features",
    //         "JCP.TicketWave.BookingService.Controllers",
    //         "JCP.TicketWave.CatalogService.Controllers",
    //         "JCP.TicketWave.PaymentService.Controllers",
    //         "JCP.TicketWave.NotificationService.Controllers"
    //     };

    //     foreach (var ns in nonPersistenceNamespaces)
    //     {
    //         var result = Types
    //             .InNamespace(ns)
    //             .Should()
    //             .NotHaveDependencyOnAny(
    //                 "System.Data.SqlClient",
    //                 "Microsoft.Data.SqlClient",
    //                 "MongoDB.Driver",
    //                 "StackExchange.Redis");

    //         result.IsSuccessful.Should().BeTrue(
    //             $"Namespace {ns} should not depend on database-specific libraries. Violations: {string.Join(", ", result.FailingTypeNames)}");
    //     }
    // }

    // [Fact]
    // public void External_Library_Dependencies_Should_Be_Abstracted()
    // {
    //     // Las dependencias de librerías externas deben estar abstraídas
    //     var businessLogicNamespaces = new[]
    //     {
    //         "JCP.TicketWave.BookingService.Domain",
    //         "JCP.TicketWave.CatalogService.Domain",
    //         "JCP.TicketWave.PaymentService.Domain",
    //         "JCP.TicketWave.NotificationService.Domain"
    //     };

    //     var externalLibraries = new[]
    //     {
    //         "Newtonsoft.Json",
    //         "AutoMapper",
    //         "FluentValidation",
    //         "MediatR",
    //         "Serilog"
    //     };

    //     foreach (var ns in businessLogicNamespaces)
    //     {
    //         var result = Types
    //             .InNamespace(ns)
    //             .Should()
    //             .NotHaveDependencyOnAny(externalLibraries);

    //         result.IsSuccessful.Should().BeTrue(
    //             $"Business logic in {ns} should not depend on external libraries directly. Create abstractions instead. Violations: {string.Join(", ", result.FailingTypeNames)}");
    //     }
    // }
}