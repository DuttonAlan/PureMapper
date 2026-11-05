# Organize maps with profiles

This guide shows how to move maps out of your startup code into classes that group them, for example per feature.

## Create a profile

Derive from `Profile` and create the maps in the constructor:

```csharp
public class SalesProfile : Profile
{
    public SalesProfile()
    {
        CreateMap<Order, OrderDto>();
        CreateMap<OrderLine, OrderLineDto>();
    }
}
```

`CreateMap` here works in the same way as on `cfg`, including `ForMember` and `ReverseMap`.

## Add one profile

```csharp
var config = new MapperConfiguration(cfg =>
{
    cfg.AddProfile<SalesProfile>();
});
```

## Add all profiles of an assembly

```csharp
var config = new MapperConfiguration(cfg =>
{
    cfg.AddMaps(typeof(SalesProfile).Assembly);
});
```

`AddMaps` finds every non-abstract class derived from `Profile` with a public parameterless constructor, including `internal` classes. You can pass several assemblies, and you can use it together with `CreateMap` and `AddProfile` in the same configuration.

With dependency injection, pass the assemblies to [`AddPureMapper`](register-with-dependency-injection.md).

## Pass values to a profile

A profile with constructor parameters is **skipped by `AddMaps` without an error**. Create it yourself and pass the instance:

```csharp
var config = new MapperConfiguration(cfg =>
{
    cfg.AddProfile(new PricingProfile(CultureInfo.GetCultureInfo("de-DE")));
});
```

## Avoid duplicates

A pair of types may only be mapped once across the whole configuration, so `MapperConfiguration` throws if:

- two profiles map the same pair of types,
- a `ReverseMap` creates a map that also exists elsewhere, or
- a profile is added twice, e.g. with `AddProfile<SalesProfile>()` and `AddMaps` for its assembly.

```text
A map from Order to OrderDto is defined more than once.
```

Maps in different profiles can depend on each other: a nested object is mapped through any map in the configuration, regardless of the profile it is in.

## Related

- [`Profile`](../reference/api.md#profile)
- [Validate your configuration](validate-your-configuration.md)
