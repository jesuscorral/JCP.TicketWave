using NetArchTest.Rules;
using FluentAssertions;
using Xunit;

namespace JCP.TicketWave.ArchitectureTests;

/// <summary>
/// Tests para validar las convenciones de nomenclatura
/// en todo el proyecto TicketWave
/// </summary>
public class NamingConventionTests
{
    // [Fact]
    // public void Interfaces_Should_Start_With_I()
    // {
    //     // Todas las interfaces deben comenzar con 'I'
    //     var result = Types
    //         .InCurrentDomain()
    //         .That()
    //         .ResideInNamespaceStartingWith("JCP.TicketWave")
    //         .And()
    //         .AreInterfaces()
    //         .Should()
    //         .HaveNameStartingWith("I");

    //     result.IsSuccessful.Should().BeTrue(
    //         $"All interfaces should start with 'I'. Violations: {string.Join(", ", result.FailingTypeNames)}");
    // }

    // [Fact]
    // public void Repository_Interfaces_Should_End_With_Repository()
    // {
    //     // Las interfaces de repositorio deben terminar con 'Repository'
    //     var interfaceNamespaces = new[]
    //     {
    //         "JCP.TicketWave.BookingService.Domain.Interfaces",
    //         "JCP.TicketWave.CatalogService.Domain.Interfaces",
    //         "JCP.TicketWave.PaymentService.Domain.Interfaces"
    //     };

    //     foreach (var ns in interfaceNamespaces)
    //     {
    //         var result = Types
    //             .InNamespace(ns)
    //             .That()
    //             .AreInterfaces()
    //             .And()
    //             .HaveNameStartingWith("I")
    //             .Should()
    //             .HaveNameEndingWith("Repository");

    //         result.IsSuccessful.Should().BeTrue(
    //             $"Repository interfaces in {ns} should end with 'Repository'. Violations: {string.Join(", ", result.FailingTypeNames)}");
    //     }
    // }

    // [Fact]
    // public void Repository_Implementations_Should_Match_Interface_Names()
    // {
    //     // Las implementaciones de repositorio deben coincidir con los nombres de interfaz
    //     var repositoryNamespaces = new[]
    //     {
    //         "JCP.TicketWave.BookingService.Infrastructure.Persistence.Repositories",
    //         "JCP.TicketWave.CatalogService.Infrastructure.Persistence.Repositories",
    //         "JCP.TicketWave.PaymentService.Infrastructure.Persistence.Repositories"
    //     };

    //     foreach (var ns in repositoryNamespaces)
    //     {
    //         var implementations = Types
    //             .InNamespace(ns)
    //             .That()
    //             .AreClasses()
    //             .And()
    //             .HaveNameEndingWith("Repository")
    //             .GetTypes();

    //         var serviceName = ns.Split('.')[2]; // Extract service name
    //         var interfaceNamespace = $"JCP.TicketWave.{serviceName}.Domain.Interfaces";
            
    //         var interfaces = Types
    //             .InNamespace(interfaceNamespace)
    //             .That()
    //             .AreInterfaces()
    //             .And()
    //             .HaveNameEndingWith("Repository")
    //             .GetTypes();

    //         foreach (var implementation in implementations)
    //         {
    //             var expectedInterfaceName = "I" + implementation.Name;
    //             var hasMatchingInterface = interfaces.Any(i => i.Name == expectedInterfaceName);

    //             hasMatchingInterface.Should().BeTrue(
    //                 $"Repository implementation {implementation.Name} should have matching interface {expectedInterfaceName}");
    //         }
    //     }
    // }

    // [Fact]
    // public void Controllers_Should_End_With_Controller()
    // {
    //     // Los controladores deben terminar con 'Controller'
    //     var controllerNamespaces = new[]
    //     {
    //         "JCP.TicketWave.BookingService.Controllers",
    //         "JCP.TicketWave.CatalogService.Controllers",
    //         "JCP.TicketWave.PaymentService.Controllers",
    //         "JCP.TicketWave.NotificationService.Controllers"
    //     };

    //     foreach (var ns in controllerNamespaces)
    //     {
    //         var result = Types
    //             .InNamespace(ns)
    //             .That()
    //             .AreClasses()
    //             .Should()
    //             .HaveNameEndingWith("Controller");

    //         result.IsSuccessful.Should().BeTrue(
    //             $"All controller classes in {ns} should end with 'Controller'. Violations: {string.Join(", ", result.FailingTypeNames)}");
    //     }
    // }

    // [Fact]
    // public void Feature_Handlers_Should_End_With_Handler()
    // {
    //     // Los handlers de características deben terminar con 'Handler'
    //     var featureNamespaces = new[]
    //     {
    //         "JCP.TicketWave.BookingService.Features",
    //         "JCP.TicketWave.CatalogService.Features",
    //         "JCP.TicketWave.PaymentService.Features",
    //         "JCP.TicketWave.NotificationService.Features"
    //     };

    //     foreach (var ns in featureNamespaces)
    //     {
    //         var handlerTypes = Types
    //             .InNamespace(ns)
    //             .That()
    //             .AreClasses()
    //             .And()
    //             .HaveNameEndingWith("Handler")
    //             .GetTypes();

    //         foreach (var handler in handlerTypes)
    //         {
    //             handler.Name.Should().EndWith("Handler",
    //                 $"Feature handler {handler.Name} should end with 'Handler'");
    //         }
    //     }
    // }

    // [Fact]
    // public void DbContexts_Should_End_With_DbContext()
    // {
    //     // Los DbContexts deben terminar con 'DbContext'
    //     var persistenceNamespaces = new[]
    //     {
    //         "JCP.TicketWave.BookingService.Infrastructure.Persistence",
    //         "JCP.TicketWave.CatalogService.Infrastructure.Persistence",
    //         "JCP.TicketWave.PaymentService.Infrastructure.Persistence",
    //         "JCP.TicketWave.NotificationService.Infrastructure.Persistence"
    //     };

    //     foreach (var ns in persistenceNamespaces)
    //     {
    //         var dbContexts = Types
    //             .InNamespace(ns)
    //             .That()
    //             .Inherit("Microsoft.EntityFrameworkCore.DbContext")
    //             .GetTypes();

    //         foreach (var dbContext in dbContexts)
    //         {
    //             dbContext.Name.Should().EndWith("DbContext",
    //                 $"DbContext class {dbContext.Name} should end with 'DbContext'");
    //         }
    //     }
    // }

    // [Fact]
    // public void Configuration_Classes_Should_End_With_Configuration()
    // {
    //     // Las clases de configuración deben terminar con 'Configuration'
    //     var configurationNamespaces = new[]
    //     {
    //         "JCP.TicketWave.BookingService.Infrastructure.Persistence.Configurations",
    //         "JCP.TicketWave.CatalogService.Infrastructure.Persistence.Configurations",
    //         "JCP.TicketWave.PaymentService.Infrastructure.Persistence.Configurations"
    //     };

    //     foreach (var ns in configurationNamespaces)
    //     {
    //         var configurations = Types
    //             .InNamespace(ns)
    //             .That()
    //             .AreClasses()
    //             .GetTypes();

    //         foreach (var config in configurations)
    //         {
    //             config.Name.Should().EndWith("Configuration",
    //                 $"Configuration class {config.Name} should end with 'Configuration'");
    //         }
    //     }
    // }

    // [Fact]
    // public void Enum_Types_Should_Not_Have_Enum_Suffix()
    // {
    //     // Los tipos enum no deben tener el sufijo 'Enum'
    //     var result = Types
    //         .InCurrentDomain()
    //         .That()
    //         .ResideInNamespaceStartingWith("JCP.TicketWave")
    //         .And()
    //         .AreEnums()
    //         .Should()
    //         .NotHaveNameEndingWith("Enum");

    //     result.IsSuccessful.Should().BeTrue(
    //         $"Enum types should not have 'Enum' suffix. Use descriptive names instead. Violations: {string.Join(", ", result.FailingTypeNames)}");
    // }

    // [Fact]
    // public void Status_Enums_Should_End_With_Status()
    // {
    //     // Los enums de estado deben terminar con 'Status'
    //     var statusEnums = Types
    //         .InCurrentDomain()
    //         .That()
    //         .ResideInNamespaceStartingWith("JCP.TicketWave")
    //         .And()
    //         .AreEnums()
    //         .And()
    //         .HaveNameContaining("Status")
    //         .GetTypes();

    //     foreach (var statusEnum in statusEnums)
    //     {
    //         statusEnum.Name.Should().EndWith("Status",
    //             $"Status enum {statusEnum.Name} should end with 'Status'");
    //     }
    // }

    // [Fact]
    // public void Domain_Models_Should_Use_Singular_Names()
    // {
    //     // Los modelos de dominio deben usar nombres en singular
    //     var domainModelNamespaces = new[]
    //     {
    //         "JCP.TicketWave.BookingService.Domain.Models",
    //         "JCP.TicketWave.CatalogService.Domain.Models",
    //         "JCP.TicketWave.PaymentService.Domain.Models",
    //         "JCP.TicketWave.NotificationService.Domain.Models"
    //     };

    //     var pluralWords = new[] { "Bookings", "Events", "Payments", "Categories", "Venues", "Tickets", "Refunds" };

    //     foreach (var ns in domainModelNamespaces)
    //     {
    //         var models = Types
    //             .InNamespace(ns)
    //             .That()
    //             .AreClasses()
    //             .GetTypes();

    //         foreach (var model in models)
    //         {
    //             var isPlural = pluralWords.Any(plural => model.Name.Equals(plural, StringComparison.OrdinalIgnoreCase));
                
    //             isPlural.Should().BeFalse(
    //                 $"Domain model {model.Name} should use singular name instead of plural");
    //         }
    //     }
    // }

    // [Fact]
    // public void Request_Response_Classes_Should_Have_Proper_Suffixes()
    // {
    //     // Las clases de request/response deben tener sufijos apropiados
    //     var featureNamespaces = new[]
    //     {
    //         "JCP.TicketWave.BookingService.Features",
    //         "JCP.TicketWave.CatalogService.Features",
    //         "JCP.TicketWave.PaymentService.Features",
    //         "JCP.TicketWave.NotificationService.Features"
    //     };

    //     foreach (var ns in featureNamespaces)
    //     {
    //         var requestResponseTypes = Types
    //             .InNamespace(ns)
    //             .That()
    //             .AreClasses()
    //             .And()
    //             .HaveNameContaining("Request")
    //             .Or()
    //             .HaveNameContaining("Response")
    //             .GetTypes();

    //         foreach (var type in requestResponseTypes)
    //         {
    //             var hasProperSuffix = type.Name.EndsWith("Request") || 
    //                                 type.Name.EndsWith("Response") || 
    //                                 type.Name.EndsWith("Query") || 
    //                                 type.Name.EndsWith("Command");

    //             hasProperSuffix.Should().BeTrue(
    //                 $"Request/Response class {type.Name} should end with 'Request', 'Response', 'Query', or 'Command'");
    //         }
    //     }
    // }

    // [Fact]
    // public void Private_Fields_Should_Start_With_Underscore()
    // {
    //     // Los campos privados deben comenzar con guión bajo
    //     var allTypes = Types
    //         .InCurrentDomain()
    //         .That()
    //         .ResideInNamespaceStartingWith("JCP.TicketWave")
    //         .GetTypes();

    //     var violatingTypes = new List<string>();

    //     foreach (var type in allTypes.Where(t => t.IsClass))
    //     {
    //         var privateFields = type.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
    //             .Where(f => f.IsPrivate && !f.IsStatic)
    //             .Where(f => !f.Name.StartsWith("_"))
    //             .Where(f => !f.Name.StartsWith("<")) // Exclude compiler-generated fields
    //             .ToArray();

    //         if (privateFields.Any())
    //         {
    //             violatingTypes.Add($"{type.Name}: {string.Join(", ", privateFields.Select(f => f.Name))}");
    //         }
    //     }

    //     violatingTypes.Should().BeEmpty(
    //         $"Private fields should start with underscore. Violations: {string.Join("; ", violatingTypes)}");
    // }

    // [Fact]
    // public void Constants_Should_Be_PascalCase()
    // {
    //     // Las constantes deben estar en PascalCase
    //     var allTypes = Types
    //         .InCurrentDomain()
    //         .That()
    //         .ResideInNamespaceStartingWith("JCP.TicketWave")
    //         .GetTypes();

    //     var violatingConstants = new List<string>();

    //     foreach (var type in allTypes.Where(t => t.IsClass))
    //     {
    //         var constants = type.GetFields()
    //             .Where(f => f.IsStatic && f.IsLiteral)
    //             .Where(f => !f.Name.Equals(f.Name.Substring(0, 1).ToUpper() + f.Name.Substring(1), StringComparison.Ordinal))
    //             .ToArray();

    //         if (constants.Any())
    //         {
    //             violatingConstants.AddRange(constants.Select(c => $"{type.Name}.{c.Name}"));
    //         }
    //     }

    //     violatingConstants.Should().BeEmpty(
    //         $"Constants should be in PascalCase. Violations: {string.Join(", ", violatingConstants)}");
    // }
}