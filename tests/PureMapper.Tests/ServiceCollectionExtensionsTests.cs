using Microsoft.Extensions.DependencyInjection;

namespace PureMapper.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void Registers_mapper_with_profiles_from_assemblies()
    {
        using var provider = new ServiceCollection()
            .AddPureMapper(typeof(CustomerProfile).Assembly)
            .BuildServiceProvider();

        var mapper = provider.GetRequiredService<IMapper>();

        Assert.Equal("Ada", mapper.Map<CustomerDto>(new Customer { FirstName = "Ada" }).FirstName);
        Assert.Same(mapper, provider.GetRequiredService<IMapper>());
        provider.GetRequiredService<MapperConfiguration>().AssertConfigurationIsValid();
    }

    [Fact]
    public void Registers_mapper_with_inline_configuration()
    {
        using var provider = new ServiceCollection()
            .AddPureMapper(cfg => cfg.CreateMap<Customer, CustomerRecord>())
            .BuildServiceProvider();

        var record = provider.GetRequiredService<IMapper>()
            .Map<CustomerRecord>(new Customer { Id = 1, FirstName = "Ada", LastName = "Lovelace" });

        Assert.Equal(new CustomerRecord(1, "Ada", "Lovelace"), record);
    }

    [Fact]
    public void Calling_twice_throws()
    {
        var services = new ServiceCollection().AddPureMapper(cfg => cfg.CreateMap<Customer, CustomerDto>());

        Assert.Throws<InvalidOperationException>(() => services.AddPureMapper(typeof(CustomerProfile).Assembly));
    }
}
