using System.Reflection;
using NetArchTest.Rules;
using Xunit;

namespace JCP.TicketWave.ArchitectureTests.Services.CatalogService.Domain;

public class DomainTests: BaseTest
{
    [Fact]
    public void DomainEvents_Should_BeSealed()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(Shared.Infrastructure.Domain.DomainEvent))
            .Should()
            .BeSealed()
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void DomainEvents_Should_HaveDomainEventPostFix()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(Shared.Infrastructure.Domain.DomainEvent))
            .Should()
            .HaveNameEndingWith("DomainEvent")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void Entities_Should_HavePrivateParameterlessConstructor()
    {
        var entityTypes = Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(Shared.Infrastructure.Domain.BaseEntity))
            .GetTypes();

        var failingTypes = new List<Type>();
        foreach (var type in entityTypes)
        {
            var constructor = type.GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                Type.EmptyTypes,
                null);

            if (constructor == null || !constructor.IsPrivate)
            {
                failingTypes.Add(type);
            }
        }

        Assert.True(failingTypes.Count == 0, 
            $"The following entities do not have a private parameterless constructor: {string.Join(", ", failingTypes.Select(t => t.Name))}");
    }
}