namespace PureMapper.Tests;

public class MapperTests
{
    private static Customer CreateCustomer() => new()
    {
        Id = 1,
        FirstName = "Ada",
        LastName = "Lovelace",
        Email = "ada@example.com",
        Address = new Address { City = "London", Street = "St James's Square" },
    };

    private static IMapper CreateMapper(Action<IMapperConfigurationExpression> configure) =>
        new MapperConfiguration(configure).CreateMapper();

    [Fact]
    public void Maps_properties_with_matching_names()
    {
        var mapper = CreateMapper(cfg => cfg.CreateMap<Customer, CustomerDto>());

        var dto = mapper.Map<CustomerDto>(CreateCustomer());

        Assert.Equal(1, dto.Id);
        Assert.Equal("Ada", dto.FirstName);
        Assert.Equal("Lovelace", dto.LastName);
        Assert.Equal("ada@example.com", dto.Email);
    }

    [Fact]
    public void Flattens_nested_collections_enums_and_converts_types()
    {
        var mapper = CreateMapper(cfg =>
        {
            cfg.CreateMap<Order, OrderDto>();
            cfg.CreateMap<OrderLine, OrderLineDto>();
        });
        var order = new Order
        {
            Id = 7,
            Customer = CreateCustomer(),
            Lines = [new OrderLine { Product = "Pen", Quantity = 2 }, new OrderLine { Product = "Ink", Quantity = 1 }],
            Status = OrderStatus.Shipped,
            Total = 12.5m,
        };

        var dto = mapper.Map<Order, OrderDto>(order);

        Assert.Equal("Ada", dto.CustomerFirstName);
        Assert.Equal("London", dto.CustomerAddressCity);
        Assert.Equal(["Pen", "Ink"], dto.Lines.Select(l => l.Product));
        Assert.Equal(OrderStatusDto.Shipped, dto.Status);
        Assert.Equal("12.5", dto.Total);
    }

    [Fact]
    public void Flattening_returns_null_when_an_intermediate_object_is_null()
    {
        var mapper = CreateMapper(cfg =>
        {
            cfg.CreateMap<Order, OrderDto>();
            cfg.CreateMap<OrderLine, OrderLineDto>();
        });

        var dto = mapper.Map<OrderDto>(new Order { Customer = null });

        Assert.Null(dto.CustomerFirstName);
        Assert.Null(dto.CustomerAddressCity);
    }

    [Fact]
    public void Maps_top_level_collections()
    {
        var mapper = CreateMapper(cfg => cfg.CreateMap<Customer, CustomerDto>());

        var dtos = mapper.Map<List<CustomerDto>>(new[] { CreateCustomer(), CreateCustomer() });

        Assert.Equal(2, dtos.Count);
        Assert.All(dtos, d => Assert.Equal("Ada", d.FirstName));
    }

    [Fact]
    public void ForMember_supports_MapFrom_Ignore_Condition_and_NullSubstitute()
    {
        var mapper = CreateMapper(cfg => cfg.CreateMap<Customer, CustomerDto>()
            .ForMember(d => d.FirstName, o => o.MapFrom(s => s.FirstName + " " + s.LastName))
            .ForMember(d => d.LastName, o => o.Ignore())
            .ForMember(d => d.Id, o => o.Condition(s => s.Id > 100))
            .ForMember(d => d.Email, o => o.NullSubstitute("n/a")));

        var dto = mapper.Map<CustomerDto>(new Customer { Id = 5, FirstName = "Ada", LastName = "Lovelace" });

        Assert.Equal("Ada Lovelace", dto.FirstName);
        Assert.Equal("", dto.LastName);
        Assert.Equal(0, dto.Id);
        Assert.Equal("n/a", dto.Email);
    }

    [Fact]
    public void Maps_onto_an_existing_destination()
    {
        var mapper = CreateMapper(cfg => cfg.CreateMap<Customer, CustomerDto>());
        var existing = new CustomerDto();

        var result = mapper.Map(CreateCustomer(), existing);

        Assert.Same(existing, result);
        Assert.Equal("Ada", existing.FirstName);
    }

    [Fact]
    public void Maps_to_records_through_the_constructor()
    {
        var mapper = CreateMapper(cfg => cfg.CreateMap<Customer, CustomerRecord>());

        var record = mapper.Map<CustomerRecord>(CreateCustomer());

        Assert.Equal(new CustomerRecord(1, "Ada", "Lovelace"), record);
    }

    [Fact]
    public void ReverseMap_and_profiles_work()
    {
        var mapper = CreateMapper(cfg => cfg.AddProfile<CustomerProfile>());

        var customer = mapper.Map<Customer>(new CustomerDto { Id = 3, FirstName = "Grace" });

        Assert.Equal(3, customer.Id);
        Assert.Equal("Grace", customer.FirstName);
    }

    [Fact]
    public void AddMaps_finds_profiles_in_an_assembly()
    {
        var mapper = CreateMapper(cfg => cfg.AddMaps(typeof(CustomerProfile).Assembly));

        Assert.Equal("Ada", mapper.Map<CustomerDto>(CreateCustomer()).FirstName);
    }

    [Fact]
    public void ConstructUsing_BeforeMap_and_AfterMap_are_called()
    {
        var calls = new List<string>();
        var mapper = CreateMapper(cfg => cfg.CreateMap<Customer, CustomerDto>()
            .ConstructUsing(_ => { calls.Add("construct"); return new CustomerDto(); })
            .BeforeMap((_, _) => calls.Add("before"))
            .AfterMap((_, d) => { calls.Add("after"); d.LastName = d.LastName.ToUpperInvariant(); }));

        var dto = mapper.Map<CustomerDto>(CreateCustomer());

        Assert.Equal(["construct", "before", "after"], calls);
        Assert.Equal("LOVELACE", dto.LastName);
    }

    [Fact]
    public void Map_for_a_base_type_is_used_for_derived_sources()
    {
        var mapper = CreateMapper(cfg => cfg.CreateMap<Customer, CustomerDto>());

        var dto = mapper.Map<CustomerDto>(new VipCustomer { FirstName = "Ada" });

        Assert.Equal("Ada", dto.FirstName);
    }

    [Fact]
    public void Null_source_returns_null()
    {
        var mapper = CreateMapper(cfg => cfg.CreateMap<Customer, CustomerDto>());

        Assert.Null(mapper.Map<CustomerDto>(null));
    }

    [Fact]
    public void Missing_map_throws_a_helpful_exception()
    {
        var mapper = CreateMapper(_ => { });

        var ex = Assert.Throws<MappingException>(() => mapper.Map<CustomerDto>(CreateCustomer()));

        Assert.Contains("CreateMap<Customer, CustomerDto>", ex.Message);
    }

    [Fact]
    public void AssertConfigurationIsValid_reports_unmapped_members_and_missing_nested_maps()
    {
        var config = new MapperConfiguration(cfg => cfg.CreateMap<CustomerDto, Order>());

        var ex = Assert.Throws<MappingConfigurationException>(config.AssertConfigurationIsValid);

        Assert.Contains("unmapped property 'Customer'", ex.Message);
        Assert.Contains("unmapped property 'Lines'", ex.Message);
    }

    [Fact]
    public void AssertConfigurationIsValid_reports_missing_nested_map()
    {
        var config = new MapperConfiguration(cfg => cfg.CreateMap<Order, OrderDto>());

        var ex = Assert.Throws<MappingConfigurationException>(config.AssertConfigurationIsValid);

        Assert.Contains("cannot map 'Lines'", ex.Message);
    }

    [Fact]
    public void AssertConfigurationIsValid_passes_for_a_complete_configuration()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Order, OrderDto>();
            cfg.CreateMap<OrderLine, OrderLineDto>();
            cfg.CreateMap<Customer, CustomerRecord>();
            cfg.AddProfile<CustomerProfile>();
        });

        config.AssertConfigurationIsValid();
    }

    [Fact]
    public void Duplicate_maps_are_rejected()
    {
        Assert.Throws<MappingConfigurationException>(() => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Customer, CustomerDto>();
            cfg.CreateMap<Customer, CustomerDto>();
        }));
    }

    [Fact]
    public void Maps_init_only_properties()
    {
        var mapper = CreateMapper(cfg => cfg.CreateMap<Customer, InitOnlyDto>());

        Assert.Equal("Ada", mapper.Map<InitOnlyDto>(CreateCustomer()).FirstName);
    }

    private class VipCustomer : Customer;

    private class InitOnlyDto
    {
        public string FirstName { get; init; } = "";
    }
}
