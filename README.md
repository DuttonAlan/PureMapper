# PureMapper

[![CI](https://github.com/DuttonAlan/PureMapper/actions/workflows/ci.yml/badge.svg)](https://github.com/DuttonAlan/PureMapper/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/PureMapper.svg)](https://www.nuget.org/packages/PureMapper)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/DuttonAlan/PureMapper/blob/main/LICENSE)

A small, convention-based object-to-object mapper for .NET, in the spirit of AutoMapper.

- Zero configuration for matching names, flattening, nested objects, collections, enums and records
- Fluent `ForMember` customization, profiles and `ReverseMap`
- Startup validation that catches unmapped members and missing nested maps
- Built-in `AddPureMapper()` for dependency injection
- Compiled property accessors and a thread-safe, immutable configuration

## Installation

```bash
dotnet add package PureMapper
```

Requires .NET 10 or later.

## Quick start

```csharp
var config = new MapperConfiguration(cfg =>
{
    cfg.CreateMap<Order, OrderDto>();
    cfg.CreateMap<OrderLine, OrderLineDto>();
    cfg.CreateMap<Customer, CustomerDto>()
        .ForMember(d => d.FullName, o => o.MapFrom(s => s.FirstName + " " + s.LastName))
        .ForMember(d => d.InternalNotes, o => o.Ignore())
        .ReverseMap();
});

config.AssertConfigurationIsValid(); // fail fast at startup

IMapper mapper = config.CreateMapper();

OrderDto dto = mapper.Map<OrderDto>(order);
List<OrderDto> dtos = mapper.Map<List<OrderDto>>(orders);
mapper.Map(order, existingDto); // map onto an existing object
```

Create the `MapperConfiguration` once and reuse it. It and the mapper are thread-safe.

## Dependency injection

`AddPureMapper` is included in the `PureMapper` package:

```csharp
// Scan assemblies for Profile classes
builder.Services.AddPureMapper(typeof(Program).Assembly);

// Or configure inline (optionally plus assemblies)
builder.Services.AddPureMapper(cfg =>
{
    cfg.CreateMap<Customer, CustomerDto>();
}, typeof(Program).Assembly);
```

This registers `MapperConfiguration` and `IMapper` as singletons; inject `IMapper` wherever you need it.
Call `AddPureMapper` only once — a second call throws, so maps can't be silently dropped.

To validate at startup:

```csharp
app.Services.GetRequiredService<MapperConfiguration>().AssertConfigurationIsValid();
```

## Conventions

| Feature | Example |
| --- | --- |
| Same name (case-insensitive) | `Name` → `Name` |
| Flattening | `Customer.Address.City` → `CustomerAddressCity` (null-safe) |
| Nested objects | uses the matching `CreateMap` |
| Collections | arrays, `List<T>`, `IEnumerable<T>`, `IReadOnlyList<T>`, `HashSet<T>`, ... |
| Enums | by name, from strings, or from numbers |
| Simple conversions | via `Convert.ChangeType` (invariant culture), anything → `string` |
| Records / constructors | when there's no parameterless constructor, the largest public constructor is used and its parameters are matched by name |
| Inheritance | a map for `Customer` is also used for subclasses of `Customer` |

## Configuration options

```csharp
cfg.CreateMap<Source, Dest>()
    .ForMember(d => d.X, o => o.MapFrom(s => s.Y))     // custom source
    .ForMember(d => d.X, o => o.Ignore())              // skip
    .ForMember(d => d.X, o => o.Condition(s => s.Ok))  // map only if true
    .ForMember(d => d.X, o => o.NullSubstitute("n/a")) // value when source is null
    .ConstructUsing(s => new Dest(s.Id))               // custom creation
    .BeforeMap((s, d) => { })
    .AfterMap((s, d) => { })
    .ReverseMap();                                     // Dest -> Source by convention
```

## Profiles

```csharp
public class SalesProfile : Profile
{
    public SalesProfile()
    {
        CreateMap<Order, OrderDto>();
    }
}

var config = new MapperConfiguration(cfg =>
{
    cfg.AddProfile<SalesProfile>();
    // or: cfg.AddMaps(typeof(SalesProfile).Assembly);
});
```

## Not supported (by design, to keep it simple)

- `IQueryable` projection (`ProjectTo`)
- Circular references: an object graph that refers back to itself will overflow the stack
- Unflattening in `ReverseMap` (`CustomerName` → `Customer.Name`)
- Mapping into existing nested objects or collections (they are replaced)
- Native AOT and trimming (mapping relies on reflection and compiled expressions)

## Contributing

Bug reports, ideas and pull requests are welcome. Please read the
[contributing guide](https://github.com/DuttonAlan/PureMapper/blob/main/CONTRIBUTING.md) and the
[code of conduct](https://github.com/DuttonAlan/PureMapper/blob/main/CODE_OF_CONDUCT.md) first.
Report security issues privately as described in the
[security policy](https://github.com/DuttonAlan/PureMapper/blob/main/SECURITY.md).

See the [changelog](https://github.com/DuttonAlan/PureMapper/blob/main/CHANGELOG.md) for the release history.

## License

PureMapper is licensed under the MIT License. Copyright (c) 2026 Alan Dutton.
