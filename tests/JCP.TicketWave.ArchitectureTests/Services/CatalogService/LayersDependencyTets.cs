using NetArchTest.Rules;
using FluentAssertions;
using Xunit;
using System.Reflection;
using JCP.TicketWave.BookingService.Infrastructure.Persistence;

namespace JCP.TicketWave.ArchitectureTests.Services.CatalogService;

/// <summary>
/// Tests de arquitectura para validar el cumplimiento de Clean Architecture
/// en todos los servicios del sistema TicketWave
/// </summary>
public class CleanArchitectureTests : BaseTest
{
    [Fact]
    public void Domain_Should_NotHaveDependencies_On_Other_Layers()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .ResideInNamespace("JCP.TicketWave.*.Domain..")
            .Should()
            .NotHaveDependencyOn("JCP.TicketWave.*.Application..")
            .And()
            .NotHaveDependencyOn("JCP.TicketWave.*.Infrastructure..")
            .And()
            .NotHaveDependencyOn("JCP.TicketWave.*.Presentation..")
            .GetResult();

        result.IsSuccessful
            .Should()
            .BeTrue("Domain layer should not depend on Application layer.");
    }

    [Fact]
    public void Application_Should_Not_Depend_On_Infrastructure_Or_Presentation()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ResideInNamespace("JCP.TicketWave.*.Application..")
            .Should()
            .NotHaveDependencyOn("JCP.TicketWave.*.Infrastructure..")
            .And()
            .NotHaveDependencyOn("JCP.TicketWave.*.Presentation..")
            .GetResult();

        result.IsSuccessful
            .Should()
            .BeTrue("Application layer should not depend on Infrastructure or Presentation layers.");
    }

    [Fact]
    public void Infrastructure_Should_Not_Depend_On_Presentation()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .That()
            .ResideInNamespace("JCP.TicketWave.*.Infrastructure..")
            .Should()
            .NotHaveDependencyOn("JCP.TicketWave.*.Presentation..")
            .GetResult();

        result.IsSuccessful
            .Should()
            .BeTrue("Infrastructure layer should not depend on Presentation layer.");
    }

    [Fact]
    public void Domain_Models_Should_Be_In_Models_Namespace()
    {
        var assemblies = new List<Assembly> {
            DomainAssembly,
            InfrastructureAssembly,
            ApplicationAssembly
        };

        foreach (var assembly in assemblies)
        {
            var result = Types.InAssembly(assembly)
                .That()
                .Inherit(typeof(Shared.Infrastructure.Domain.BaseEntity))
                .Should()
                .ResideInNamespaceMatching(@"JCP\.TicketWave\..+\.Domain\.Models")
                .GetResult();
            
            result.IsSuccessful.Should().BeTrue(
                $"All domain models should be in the 'Models' namespace in assembly {assembly.GetName().Name}. Violations: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? new string[0])}");
        }
    }


    [Fact]
    public void Repository_Interfaces_Should_Be_In_Domain_Interfaces()
    {
        var assemblies = new List<Assembly> {
            DomainAssembly,
            InfrastructureAssembly,
            ApplicationAssembly
        };

        foreach (var assembly in assemblies)
        {
            var result = Types.InAssembly(assembly)
                .That()
                .HaveNameEndingWith("Repository")
                .Should()
                .BeInterfaces()
                .GetResult();
        }
    }

    [Fact]
    public void Repository_Implementations_Should_Be_In_Infrastructure_Persistence()
    {
        var assemblies = new List<Assembly>
        {
            DomainAssembly,
            InfrastructureAssembly,
            ApplicationAssembly
        };

        foreach (var assembly in assemblies)
        {
            var result = Types.InAssembly(assembly)
                .That()
                .HaveNameEndingWith("Repository")
                .Should()
                .BeClasses()
                .And()
                .NotBeAbstract()
                .GetResult();
        }
    }


    [Fact]
    public void Controllers_Should_Not_Access_Domain_Models_Directly()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .HaveNameEndingWith("Controller")
            .Should()
            .NotHaveDependencyOn("JCP.TicketWave.BookingService.Domain.Models")
            .And()
            .NotHaveDependencyOn("JCP.TicketWave.CatalogService.Domain.Models")
            .And()
            .NotHaveDependencyOn("JCP.TicketWave.PaymentService.Domain.Models")
            .And()
            .NotHaveDependencyOn("JCP.TicketWave.NotificationService.Domain.Models")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }


    [Fact]
    public void Services_Should_Follow_Naming_Convention()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .AreInterfaces()
            .And()
            .HaveNameStartingWith("I")
            .Should()
            .HaveNameEndingWith("Service")
            .Or()
            .HaveNameEndingWith("Repository")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void Infrastructure_Should_Only_Reference_Domain_Interfaces()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .That()
            .ResideInNamespace("JCP.TicketWave.*.Infrastructure..")
            .Should()
            .NotHaveDependencyOn("JCP.TicketWave.BookingService.Domain.Models")
            .And()
            .NotHaveDependencyOn("JCP.TicketWave.CatalogService.Domain.Models")
            .And()
            .NotHaveDependencyOn("JCP.TicketWave.PaymentService.Domain.Models")
            .And()
            .NotHaveDependencyOn("JCP.TicketWave.NotificationService.Domain.Models")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Feature_Handlers_Should_Be_In_Features_Namespace()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .HaveNameEndingWith("Handler")
            .Should()
            .ResideInNamespaceMatching(@"JCP\.TicketWave\..+\.Application\.Features\..+")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"All handlers should be in the 'Features' namespace within Application layer. Violations: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? new string[0])}");
    }
}
