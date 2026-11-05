# Register with dependency injection

This guide shows the ways to register PureMapper with `Microsoft.Extensions.DependencyInjection`. `AddPureMapper` is part of the `PureMapper` package, so you do not need a second package.

## Register profiles from assemblies

```csharp
builder.Services.AddPureMapper(typeof(Program).Assembly);
```

You can pass several assemblies. Only [profiles PureMapper can create on its own](organize-maps-with-profiles.md#add-all-profiles-of-an-assembly) are found.

## Configure maps inline

```csharp
builder.Services.AddPureMapper(cfg =>
{
    cfg.CreateMap<Customer, CustomerDto>();
});
```

## Do both

```csharp
builder.Services.AddPureMapper(cfg =>
{
    cfg.CreateMap<Customer, CustomerDto>();
    cfg.AddProfile(new PricingProfile(culture));
}, typeof(Program).Assembly);
```

## Inject the mapper

`AddPureMapper` registers `IMapper` and `MapperConfiguration` as singletons, so you can inject `IMapper` into services of any lifetime:

```csharp
public class OrderService(IMapper mapper)
{
    public OrderDto GetDto(Order order) => mapper.Map<OrderDto>(order);
}
```

## Validate at startup

`AddPureMapper` builds the configuration immediately, so a duplicate map or a type that cannot be created fails right there. Unmapped members and missing maps are only reported when you ask:

```csharp
var app = builder.Build();

app.Services.GetRequiredService<MapperConfiguration>().AssertConfigurationIsValid();
```

This needs `using PureMapper;`. See [Validate your configuration](validate-your-configuration.md) for other places to run it.

## Use it outside ASP.NET Core

The package only depends on the abstractions. To build a container yourself, also add the implementation:

```sh
dotnet add package Microsoft.Extensions.DependencyInjection
```

```csharp
using var provider = new ServiceCollection()
    .AddPureMapper(typeof(Program).Assembly)
    .BuildServiceProvider();

var mapper = provider.GetRequiredService<IMapper>();
```

## Call it once

A second call to `AddPureMapper` throws an `InvalidOperationException`:

```text
PureMapper is already registered. Call AddPureMapper once and pass all assemblies and maps to it.
```

If several modules contribute maps, collect their assemblies and pass them all in one call.

## Use services in a map

PureMapper does not resolve profiles or callbacks from the container, so a `MapFrom` or `AfterMap` cannot have services injected. Either pass the values to the profile's constructor when they are fixed at startup, or do that part of the work after calling `Map`, in a service that has the dependency.

## Related

- [`AddPureMapper`](../reference/api.md#servicecollectionextensions)
- [Tutorial: Use PureMapper in a web API](../tutorials/use-in-a-web-api.md)
